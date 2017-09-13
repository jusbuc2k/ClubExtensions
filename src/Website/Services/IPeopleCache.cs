using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PcoApiClient.Models;

namespace Website.Services
{
    public interface IPeopleCache
    {
        Task UpdateCache(PcoListResponse<PcoPeoplePerson> people);

        Task UpdateCache(PcoListResponse<PcoPeopleHousehold> people);

        Task<IEnumerable<PeopleCacheEntry<PcoApiClient.Models.PcoPeoplePerson>>> GetPeople();

        Task<IEnumerable<PeopleCacheEntry<PcoApiClient.Models.PcoPeopleHousehold>>> GetHouseholds();

        Task<IEnumerable<PeopleCacheEntry<PcoApiClient.Models.PcoHouseholdMembership>>> GetMemberships();
    }
}
