using Microsoft.Extensions.Caching.Memory;
using PcoApiClient;
using PcoApiClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Services
{
    public class PcoHelper
    {
        private readonly PcoApiClient.PcoApiClient _pcoClient;
        private readonly PcoTenant _tenant;
        private readonly IMemoryCache _cache;

        public PcoHelper(PcoApiClient.PcoApiClient pcoClient, PcoTenant tenant, IMemoryCache cache)
        {
            _pcoClient = pcoClient;
            _tenant = tenant;
            _cache = cache;
        }

        private string CreateCacheKey(string prefix, PcoTenant tenant)
        {
            return $"{prefix}:{tenant.OrganizationID}";
        }

        public PcoTenant Tenant
        {
            get
            {
                return _tenant;
            }
        }

        public PcoApiClient.PcoApiClient ApiClient
        {
            get
            {
                return _pcoClient;
            }
        }

        public Task RefreshClubberList()
        {
            return _pcoClient.RefreshList(_tenant.ClubberListID);
        }

        public Task<PcoListResponse<PcoCheckInsLocation>> GetCheckInLocations()
        {
           return _pcoClient.GetList<PcoApiClient.Models.PcoCheckInsLocation>($"check_ins/v2/events/{_tenant.EventID}/locations", pagesToLoad: int.MaxValue);
        }

        public Task<PcoListResponse<PcoPeoplePerson>> GetClubbers(IEnumerable<string> include = null)
        {
            if (include == null)
            {
                include = new string[] { "field_data" };
            }

            return _pcoClient.GetList<PcoApiClient.Models.PcoPeoplePerson>($"people/v2/lists/{_tenant.ClubberListID}/people", pagesToLoad: int.MaxValue, includes: include);
        }

        public async Task<IEnumerable<ClubExtensions.Reports.Models.ReportPerson>> GetClubberReportCache()
        {
            var reportData = new List<ClubExtensions.Reports.Models.ReportPerson>();
            var contactsToFetch = new List<string>();

            var people = await _cache.GetOrCreateAsync(CreateCacheKey("Clubbers", _tenant), async (cacheEntry) =>
            {
                await this.RefreshClubberList();

                cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(15);

                return await this.GetClubbers(new string[] { "field_data", "households" });
            });

            var parents = await _cache.GetOrCreateAsync(CreateCacheKey("Parents", _tenant), async (cacheEntry) =>
            {
                var data = await _pcoClient.GetList<PcoPeoplePerson>($"people/v2/lists/{_tenant.ParentListID}/people", pagesToLoad: int.MaxValue, includes: new string[] { "phone_numbers","emails" });

                cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(15);

                return data;
            });

            foreach (var person in people.Data)
            {
                var field_data = people.GetRelated<PcoFieldDatum>(person, "field_data");
                
                var club = field_data.Where(x => x.Relationships.GetData<PcoApiClient.Models.PcoRecord>("field_definition").ID == _tenant.ClubFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();

                var role = field_data.Where(x => x.Relationships.GetData<PcoApiClient.Models.PcoRecord>("field_definition").ID == _tenant.RoleFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();

                var subGroup = field_data.Where(x => x.Relationships.GetData<PcoApiClient.Models.PcoRecord>("field_definition").ID == _tenant.SubGroupFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();
                
                var reportPerson = new ClubExtensions.Reports.Models.ReportPerson()
                {
                    FirstName = person.Attributes.FirstName,
                    LastName = person.Attributes.LastName,
                    ClubName = club,
                    SubGroup = subGroup,
                    Grade = person.Attributes.Grade,
                    BirthDate = person.Attributes.BirthDate,
                    Gender = person.Attributes.Gender
//                  PrimaryContactPhone = person.Relationships.GetData<IEnumerable<PcoPhoneNumber>>("phone_numbers").Select(s => s.Number).FirstOrDefault()
                };

                var household = people.GetRelated<PcoPeopleHousehold>(person, "households").FirstOrDefault();

                if (household != null)
                {
                    reportPerson.HouseholdID = household.ID;
                    reportPerson.HouseholdName = household.Attributes.Name;
                    reportPerson.PrimaryContactName = household.Attributes.PrimaryContactName;

                    var primaryContactPhone = parents.Data.Where(x => x.ID == household.Attributes.PrimaryContactID)
                        .SelectMany(pr => parents.GetRelated<PcoPhoneNumber>(pr, "phone_numbers"))
                        .OrderByDescending(x => x.Attributes.Primary)
                        .Select(s => s.Attributes.Number)
                        .FirstOrDefault();

                    var primaryContactEmail = parents.Data.Where(x => x.ID == household.Attributes.PrimaryContactID)
                        .SelectMany(pr => parents.GetRelated<PcoEmailAddress>(pr, "emails"))
                        .Where(x => x.Attributes.Primary)
                        .Select(s => s.Attributes.Address)
                        .FirstOrDefault();

                    reportPerson.PrimaryContactPhone = primaryContactPhone;
                    reportPerson.PrimaryContactEmail = primaryContactEmail;
                }

                reportData.Add(reportPerson);
            }

            return reportData;
        }

        public async Task UpdatePersonClub(string personID, string clubDatumID, string clubName)
        {
            var datum = new PcoApiClient.Models.PcoFieldDatum()
            {
                FieldDefinitionID = _tenant.ClubFieldDefinitionID,
                Value = clubName
            };

            if (clubDatumID == null)
            {
                await _pcoClient.CreatePersonFieldData(personID, datum);
            }
            else
            {
                await _pcoClient.UpdatePersonFieldData(personID, clubDatumID, datum);
            }
        }
    }
}
