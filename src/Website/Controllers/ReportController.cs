using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PcoApiClient.Models;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly PcoApiClient.PcoApiClient _pcoClient;
        private readonly PcoTenant _tenant;

        public ReportController(PcoApiClient.PcoApiClient pcoClient, PcoTenant tenant)
        {
            _pcoClient = pcoClient;
            _tenant = tenant;
        }

        public async Task<IActionResult> PointSheet()
        {
            string mimeType;

            await _pcoClient.RefreshList(_tenant.ClubberListID);

            var people = await _pcoClient.GetList<PcoPeoplePerson>($"people/v2/lists/{_tenant.ClubberListID}/people", pagesToLoad: int.MaxValue, includes: new string[] { "field_data" }); 
            var reportData = new List<ClubExtensions.Reports.Models.ReportPerson>();

            foreach (var person in people.Data)
            {
                var field_data = people.GetRelated<PcoFieldDatum>(person, "field_data");

                var club = field_data.Where(x => x.Attributes.FieldDefinitionID == _tenant.ClubFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();

                var role = field_data.Where(x => x.Attributes.FieldDefinitionID == _tenant.RoleFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();

                reportData.Add(new ClubExtensions.Reports.Models.ReportPerson()
                {
                    FirstName = person.Attributes.FirstName,
                    LastName = person.Attributes.LastName,
                    ClubName = club,
                    Grade = person.Attributes.Grade,
                    BirthDate = person.Attributes.BirthDate
                });
            }

            var buffer = ClubExtensions.Reports.ReportHelper.RenderReport("ClubPointSheet", "ReportData", reportData, "pdf", out mimeType);

            // fix for PDF embedding in IE which requires a file name
            // controller.Response.AddHeader("Content-Disposition", (download ? "attachment" : "inline") + "; filename=" + fileName);

            return new FileContentResult(buffer, mimeType);
        }
    }
}
