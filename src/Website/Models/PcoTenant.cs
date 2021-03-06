﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class PcoTenant
    {
        public string OrganizationID { get; set; }

        public PcoApiClient.Models.PcoOrganization Organization { get; set; }

        public int ClubberListID { get; set; } = 272098;

        public int ParentListID { get; set; } = 272096;

        public int ClubFieldDefinitionID { get; set; } = 145522;

        public int RoleFieldDefinitionID { get; set; } = 145834;

        public int SubGroupFieldDefinitionID { get; set; } = 156621;

        public string ClubberRoleValue { get; set; } = "Clubber";

        public int EventID { get; set; } = 122646;
    }
}
