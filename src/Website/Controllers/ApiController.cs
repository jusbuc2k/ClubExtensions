using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Website.Controllers
{
    public class ApiController : Controller
    {
        private readonly PcoApiClient.PcoApiClient _client;
        private readonly Models.PcoTenant _tenant;

        public ApiController(PcoApiClient.PcoApiClient client, Models.PcoTenant tenant)
        {
            _client = client;
            _tenant = tenant;
        }

        [HttpPost]
        public async Task<dynamic> PreviewClubAssignments()
        {
            var locations = await _client.GetList<PcoApiClient.Models.PcoCheckInsLocation>($"check_ins/v2/events/{_tenant.EventID}/locations", pagesToLoad: int.MaxValue);
            var clubbers = await _client.GetList<PcoApiClient.Models.PcoPeoplePerson>($"people/v2/lists/{_tenant.ClubberListID}/people", pagesToLoad: int.MaxValue, includes: new string[] { "field_data" });
            var assignments = new List<Models.ClubAssignment>();

            foreach (var clubber in clubbers.Data)
            {
                var fieldData = clubbers.GetRelated<PcoApiClient.Models.PcoFieldDatum>(clubber, "field_data");

                var match = locations.Data.OrderBy(o => o.Attributes.Position).LastOrDefault(location =>
                {
                    if (location.Attributes.Name.Contains("Girls"))
                    {
                        location.Attributes.Gender = "F";
                    }
                    else if (location.Attributes.Name.Contains("Boys"))
                    {
                        location.Attributes.Gender = "M";
                    }

                    if (location.Attributes.Gender != null)
                    {
                        if (clubber.Attributes.Gender != location.Attributes.Gender)
                        {
                            return false;
                        }
                    }

                    if (location.Attributes.GradeMin.HasValue)
                    {
                        if (!clubber.Attributes.Grade.HasValue)
                        {
                            return false;
                        }
                        else if (clubber.Attributes.Grade.Value < location.Attributes.GradeMin.Value)
                        {
                            return false;
                        }
                    }

                    if (location.Attributes.GradeMax.HasValue)
                    {
                        if (!clubber.Attributes.Grade.HasValue)
                        {
                            return false;
                        }
                        else if (clubber.Attributes.Grade.Value > location.Attributes.GradeMax.Value)
                        {
                            return false;
                        }
                    }

                    if (location.Attributes.MinAgeInMonths.HasValue)
                    {
                        if (!clubber.Attributes.BirthDate.HasValue)
                        {
                            return false;
                        }

                        var ageOn = location.Attributes.AgeOn.HasValue ? location.Attributes.AgeOn.Value : DateTime.Now;
                        var ageInMonths = ageOn.Subtract(clubber.Attributes.BirthDate.Value);
                        var maxBirthDate = ageOn.AddMonths(-location.Attributes.MinAgeInMonths.Value);
                        var minBirthDate = DateTime.MinValue;// ageOn.AddMonths(-location.Attributes.MaxAgeInMonths.Value);

                        if (clubber.Attributes.BirthDate < minBirthDate)
                        {
                            return false;
                        }

                        if (clubber.Attributes.BirthDate > maxBirthDate)
                        {
                            return false;
                        }
                    }

                    return true;
                });

                var clubDatum = fieldData.FirstOrDefault(x => x.Attributes.FieldDefinitionID == _tenant.ClubFieldDefinitionID);

                assignments.Add(new Models.ClubAssignment()
                {
                    PersonID = clubber.ID,
                    FirstName = clubber.Attributes.FirstName,
                    LastName = clubber.Attributes.LastName,
                    BirthDate = clubber.Attributes.BirthDate,
                    Gender = clubber.Attributes.Gender,
                    Grade = clubber.Attributes.Grade,
                    ClubDatumID = clubDatum == null ? null : clubDatum.ID,
                    OldClubName = clubDatum == null ? null : clubDatum.Attributes.Value,
                    NewClubName = match.Attributes.Name
                });
            }

            return new
            {
                List = assignments.OrderByDescending(o => o.BirthDate).ThenBy(o => o.FirstName),
                ChangeCount = assignments.Count(x => x.IsChange)
            };
        }

        [HttpPost]
        public async Task<IActionResult> AssignClubs([FromBody]IEnumerable<Models.ClubAssignment> model)
        {
            if (model == null || model.Count() <= 0)
            {
                return this.BadRequest();
            }

            foreach (var clubber in model)
            {
                var datum = new PcoApiClient.Models.PcoFieldDatum()
                {
                    FieldDefinitionID = _tenant.ClubFieldDefinitionID,
                    Value = clubber.NewClubName
                };

                if (clubber.ClubDatumID == null)
                {
                    await _client.CreatePersonFieldData(clubber.PersonID, datum);
                }
                else
                {
                    await _client.UpdatePersonFieldData(clubber.PersonID, clubber.ClubDatumID, datum);
                }
            }

            return this.NoContent();
        }
    }
}
