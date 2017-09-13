using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PcoApiClient.Models;
using Website.Models;
using Website.Extensions;

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

        private async Task<IEnumerable<ClubExtensions.Reports.Models.ReportPerson>> GetClubberList()
        {
            await _pcoClient.RefreshList(_tenant.ClubberListID);

            var people = await _pcoClient.GetList<PcoPeoplePerson>($"people/v2/lists/{_tenant.ClubberListID}/people", pagesToLoad: int.MaxValue, includes: new string[] { "field_data" });
            var reportData = new List<ClubExtensions.Reports.Models.ReportPerson>();

            foreach (var person in people.Data)
            {
                var field_data = people.GetRelated<PcoFieldDatum>(person, "field_data");

                var club = field_data.Where(x => x.Relationships["field_definition"].Data.ToObject<PcoApiClient.Models.PcoRecord>().ID == _tenant.ClubFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();

                var role = field_data.Where(x => x.Relationships["field_definition"].Data.ToObject<PcoApiClient.Models.PcoRecord>().ID == _tenant.RoleFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();

                reportData.Add(new ClubExtensions.Reports.Models.ReportPerson()
                {
                    FirstName = person.Attributes.FirstName,
                    LastName = person.Attributes.LastName,
                    ClubName = club,
                    Grade = person.Attributes.Grade,
                    BirthDate = person.Attributes.BirthDate,
                    Gender = person.Attributes.Gender
                });
            }

            return reportData;
        }

        public async Task<IActionResult> PointSheet()
        {
            string mimeType;

            var reportData = await this.GetClubberList();

            var buffer = ClubExtensions.Reports.ReportHelper.RenderReport("ClubPointSheet", "ReportData", reportData, "pdf", out mimeType);

            return new FileContentResult(buffer, mimeType);
        }

        public async Task<IActionResult> KidsByClub()
        {
            string mimeType;

            var reportData = await this.GetClubberList();

            var buffer = ClubExtensions.Reports.ReportHelper.RenderReport("KidsByClub", "ReportData", reportData, "pdf", out mimeType);

            return new FileContentResult(buffer, mimeType);
        }

        public async Task<IActionResult> Roster()
        {
            string mimeType;

            var reportData = await this.GetClubberList();

            var buffer = ClubExtensions.Reports.ReportHelper.RenderReport("Roster", "ReportData", reportData, "pdf", out mimeType);

            return new FileContentResult(buffer, mimeType);
        }

        public async Task<IActionResult> ClubData()
        {
           // await _pcoClient.RefreshList(_tenant.ClubberListID);
            //await _pcoClient.RefreshList(_tenant.ParentListID);

            var clubbers = await _pcoClient.GetList<PcoPeoplePerson>($"people/v2/lists/{_tenant.ClubberListID}/people", pagesToLoad: int.MaxValue, includes: new string[] { "field_data", "households" });
            var parents = await _pcoClient.GetList<PcoPeoplePerson>($"people/v2/lists/{_tenant.ParentListID}/people", pagesToLoad: int.MaxValue, includes: new string[] { "phone_numbers", "emails" });
            var reportData = new List<ClubExtensions.Reports.Models.ReportPerson>();

            foreach (var person in clubbers.Data)
            {
                var field_data = clubbers.GetRelated<PcoFieldDatum>(person, "field_data");

                var club = field_data.Where(x => x.Attributes.FieldDefinitionID == _tenant.ClubFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();

                var role = field_data.Where(x => x.Attributes.FieldDefinitionID == _tenant.RoleFieldDefinitionID)
                    .Select(s => s.Attributes.Value)
                    .SingleOrDefault();

                var household = clubbers.GetRelated<PcoPeopleHousehold>(person, "households").FirstOrDefault();
                var primaryContact = parents.Data.Single(x => x.ID == household.Attributes.PrimaryContactID);
                var primaryContactEmail = parents.GetRelated<PcoEmailAddress>(primaryContact, "emails").FirstOrDefault();
                var primaryContactPhone = parents.GetRelated<PcoPhoneNumber>(primaryContact, "phone_numbers").FirstOrDefault();

                reportData.Add(new ClubExtensions.Reports.Models.ReportPerson()
                {
                    PersonID = person.ID,
                    FirstName = person.Attributes.FirstName,
                    LastName = person.Attributes.LastName,
                    ClubName = club,
                    Grade = person.Attributes.Grade,
                    BirthDate = person.Attributes.BirthDate,
                    HouseholdID = household.ID,
                    HouseholdName = household.Attributes.Name,
                    PrimaryContactName = household.Attributes.PrimaryContactName,
                    PrimaryContactEmail = primaryContactEmail == null ? string.Empty : primaryContactEmail.Attributes.Address,
                    PrimaryContactPhone = primaryContactPhone == null ? string.Empty : primaryContactPhone.Attributes.Number,
                });
            }

            this.Response.Headers.Add("Content-Disposition", "attachment; filename=ClubberData.csv");
            this.Response.ContentType = "text/csv";

            var ms = new System.IO.MemoryStream();
            var writer = new Csg.IO.TextData.CsvWriter(ms);

            writer.EnableHeader = true;
            writer.Fields = new string[] { "PersonID", "FirstName", "LastName", "ClubName", "Grade", "BirthDate", "Age", "HouseholdID", "HouseholdName", "PrimaryContact", "PrimaryContactEmail", "PrimaryContactPhone" };
            writer.WriteHeader();

            foreach (var item in reportData)
            {
                writer.WriteLine(new string[]
                {
                    item.PersonID, 
                    item.FirstName,
                    item.LastName,
                    item.ClubName,
                    item.GradeName,
                    item.BirthDate.HasValue ? item.BirthDate.Value.ToString("m/d/yyyy") : string.Empty,
                    item.Age.ToString(),
                    item.HouseholdID,
                    item.HouseholdName,
                    item.PrimaryContactName,
                    item.PrimaryContactEmail,
                    item.PrimaryContactPhone
                });
            }

            writer.Flush();
            //writer.Close();

            //writer.Flush();
            //var buffer = ClubExtensions.Reports.ReportHelper.RenderReport("Households", "ReportData", reportData, "pdf", out mimeType);
            // fix for PDF embedding in IE which requires a file name
            // controller.Response.AddHeader("Content-Disposition", (download ? "attachment" : "inline") + "; filename=" + fileName);

            ms.Position = 0;

            return this.File(ms, "text/csv");
        }
    }
}
