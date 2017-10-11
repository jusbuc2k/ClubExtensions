using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PcoApiClient.Models;
using PcoApiClient;
using Website.Services;

namespace Website.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        private readonly PcoHelper _pco;

        public ApiController(PcoHelper pco)
        {
            _pco = pco;
        }

        [HttpPost]
        public async Task<dynamic> PreviewClubAssignments()
        {
            await _pco.RefreshClubberList();
            var locations = await _pco.GetCheckInLocations();
            var clubbers = await _pco.GetClubbers();
            var assignments = new List<Models.ClubAssignment>();

            foreach (var clubber in clubbers.Data)
            {
                var fieldData = clubbers.GetRelated<PcoApiClient.Models.PcoFieldDatum>(clubber, "field_data");

                var match = locations.Data.OrderBy(o => o.Attributes.Position).LastOrDefault(location =>
                {
                    // If the location has a gender filter, and the user's doesn't match, return false.
                    if (location.Attributes.Gender != null)
                    {
                        if (clubber.Attributes.Gender != location.Attributes.Gender)
                        {
                            return false;
                        }
                    }

                    // If the location has a min grade filter
                    if (location.Attributes.GradeMin.HasValue)
                    {
                        // If the person has no grade value, then we can't determine a match, so return false
                        if (!clubber.Attributes.Grade.HasValue)
                        {
                            return false;
                        }
                        else if (clubber.Attributes.Grade.Value < location.Attributes.GradeMin.Value)
                        {
                            return false;
                        }
                    }

                    // If the location has a max grade filter
                    if (location.Attributes.GradeMax.HasValue)
                    {
                        // if the person has no grade value, then we can't determine a match, so return false
                        if (!clubber.Attributes.Grade.HasValue)
                        {
                            return false;
                        }
                        else if (clubber.Attributes.Grade.Value > location.Attributes.GradeMax.Value)
                        {
                            return false;
                        }
                    }

                    // If the location has a min age filter (then it will also have a max)
                    if (location.Attributes.MinAgeInMonths.HasValue)
                    {
                        // If the person has no birth date, then we can't evaluate
                        if (!clubber.Attributes.BirthDate.HasValue)
                        {
                            return false;
                        }

                        // ageOn is the date relative to which we want to calculate the age
                        // for rules involving a specific cutoff (e.g. must be 3 years old by August 1st)
                        var ageOn = location.Attributes.AgeOn.HasValue ? location.Attributes.AgeOn.Value : DateTime.Now;
                        // compute the age in months of the person from the ageOn date
                        var ageInMonths = ageOn.Subtract(clubber.Attributes.BirthDate.Value);
                        // compute the maximum date on which the person could have been born to match
                        var maxBirthDate = ageOn.AddMonths(-location.Attributes.MinAgeInMonths.Value);
                        // compute the minimum date on which the person could have been born to match
                        var minBirthDate = ageOn.AddMonths(-location.Attributes.MaxAgeInMonths.Value);

                        // if they are less than the min window, return false
                        if (clubber.Attributes.BirthDate < minBirthDate)
                        {
                            return false;
                        }

                        // if they are greater than the max window, return false
                        if (clubber.Attributes.BirthDate > maxBirthDate)
                        {
                            return false;
                        }
                    }
                    
                    // If no rules failed, then we assume a match
                    return true;
                });

                var clubDatum = fieldData.FirstOrDefault(x => x.Relationships.GetData<PcoApiClient.Models.PcoRecord>("field_definition").ID == _pco.Tenant.ClubFieldDefinitionID);

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
                    NewClubName = match == null ? null : match.Attributes.Name
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
                await _pco.UpdatePersonClub(clubber.PersonID, clubber.ClubDatumID, clubber.NewClubName);
            }

            return this.NoContent();
        }
        
        [Route("[controller]/Cache")]
        public IActionResult CheckCache()
        {
            return this.NoContent();
        }

        //[Route("[controller]/Cache/Refresh")]
        //[HttpPost]
        //public async Task<IActionResult> RefreshCache()
        //{
        //    var clubbers = await _pcoClient.GetList<PcoPeoplePerson>($"people/v2/lists/{_tenant.ClubberListID}/people", pagesToLoad: int.MaxValue, includes: new string[] { "field_data", "households" });
        //    var parents = await _pcoClient.GetList<PcoPeoplePerson>($"people/v2/households", pagesToLoad: int.MaxValue, includes: new string[] { "phone_numbers", "emails" });
        //}
    }
}
