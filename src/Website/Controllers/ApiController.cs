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
        public async Task<IEnumerable<ClubExtensions.Reports.Models.ReportPerson>> AssignClubs()
        {
            var locations = await _client.GetList<PcoApiClient.Models.PcoCheckInsLocation>($"check_ins/v2/events/{_tenant.EventID}/locations", pagesToLoad: int.MaxValue);
            var clubbers = await _client.GetList<PcoApiClient.Models.PcoPeoplePerson>($"people/v2/lists/{_tenant.ClubberListID}/people", pagesToLoad: int.MaxValue);
            var clubsToAssign = new Dictionary<string, string>();
            var assignments = new List<ClubExtensions.Reports.Models.ReportPerson>();

            foreach (var clubber in clubbers.Data)
            {
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
                        var minBirthDAte = DateTime.MinValue;// ageOn.AddMonths(-location.Attributes.MaxAgeInMonths.Value);

                        if (clubber.Attributes.BirthDate < minBirthDAte)
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

                clubsToAssign.Add(clubber.Attributes.Name, match.Attributes.Name);

                assignments.Add(new ClubExtensions.Reports.Models.ReportPerson()
                {
                    FirstName = clubber.Attributes.FirstName,
                    LastName = clubber.Attributes.LastName,
                    ClubName = match.Attributes.Name
                });
            }

            return assignments.OrderBy(o => o.ClubName);
        }
    }
}
