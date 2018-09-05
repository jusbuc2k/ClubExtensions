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

                // invalidate the contact cache
                _cache.Remove(CreateCacheKey("Contacts", _tenant));

                cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(15);

                return await this.GetClubbers(new string[] { "field_data", "households" });
            });

            var primaryContacts = await _cache.GetOrCreateAsync(CreateCacheKey("Contacts", _tenant), async (cacheEntry) => {
                var primaryContactIDs = people.Included.Where(x => x.Type == "Household")
                    .Select(s => s.Attributes.ToObject<PcoPeopleHousehold>())
                    .Select(s => s.PrimaryContactID)
                    .Distinct()
                    .ToList();

                var result = new PcoListResponse<PcoPeoplePerson>();
                var list = new List<PcoPeoplePerson>();

                for (int i = 0; i < primaryContactIDs.Count; i += 25)
                {
                    //var idFilter = string.Join(",", primaryContactIDs.GetRange(i, Math.Min(25, primaryContactIDs.Count-i)));
                    //var phones = await _pcoClient.GetList<PcoApiClient.Models.PcoPhoneNumber>($"people/v2/people/{idFilter}/phone_numbers", pagesToLoad: int.MaxValue, includes: new string[] { "phone_numbers", "emails" });
                    //var primaryPhone = phones.Data.FirstOrDefault(x => x.Attributes.Primary);

                    //if (primaryPhone == null)
                    //{
                    //    primaryPhone = phones.Data.First();
                    //}

                    //if (primaryPhone != null)
                    //{
                    //    result.Data.First(x => x.Attributes.)
                    //}
                    //// result.Data
                }

                cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(15);

                return result;
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
                    Gender = person.Attributes.Gender,
                    PrimaryContactPhone = person.Relationships.GetData<IEnumerable<PcoPhoneNumber>>("phone_numbers").Select(s => s.Number).FirstOrDefault()
                };

                var household = people.GetRelated<PcoPeopleHousehold>(person, "households").FirstOrDefault();

                if (household != null)
                {
                    reportPerson.HouseholdID = household.ID;
                    reportPerson.HouseholdName = household.Attributes.Name;
                    reportPerson.PrimaryContactName = household.Attributes.PrimaryContactName;

                    //var primaryContact = primaryContacts.Data.SingleOrDefault(x => x.ID == household.Attributes.PrimaryContactID);

                    //if (primaryContact != null)
                    //{
                    //    var primaryPhone = primaryContacts.GetRelated<PcoPhoneNumber>(primaryContact, "phone_numbers").OrderBy(o => !o.Attributes.Primary).FirstOrDefault();
                    //    var primaryEmail = primaryContacts.GetRelated<PcoEmailAddress>(primaryContact, "emails").OrderBy(o => !o.Attributes.Primary).FirstOrDefault();

                    //    reportPerson.PrimaryContactEmail = primaryEmail?.Attributes.Address;
                    //    reportPerson.PrimaryContactPhone = primaryPhone?.Attributes.Number;
                    //}
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
