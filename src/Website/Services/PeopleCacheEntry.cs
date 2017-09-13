using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Services
{
    public class PeopleCacheEntry<T>
    {
        public string ID { get; set; }

        public string Type { get; set; }

        public T Attributes { get; set; }
    }
}
