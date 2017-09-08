using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PcoApiClient
{
    public class PcoApiClient
    {
        public PcoApiClient(PcoApiOptions options)
        {
            _options = options;
        }

        public PcoApiClient(System.Net.Http.HttpClient http, PcoApiOptions options)
        {
            _client = http;
            _options = options;
        }

        private PcoApiOptions _options;
        private System.Net.Http.HttpClient _client;

        protected System.Net.Http.HttpClient EnsureClient()
        {
            if (_client == null)
            {
                _client = new System.Net.Http.HttpClient();
                _client.BaseAddress = new Uri(_options.Url);
            }

            return _client;
        }

        public HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var msg = new HttpRequestMessage();

            msg.Method = method;

            msg.RequestUri = new Uri(string.Concat(_options.Url, url));

            if (_options.AuthenticationMethod == "Bearer")
            {
                msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Password);
            }

            return msg;
        }

        public async Task<Models.PcoListResponse<T>> GetList<T>(string path, int pageSize = 100, int pagesToLoad = 1, IEnumerable<string> includes = null)
        {
            var url = new StringBuilder(path);

            url.Append($"?per_page={pageSize}");

            if (includes != null)
            {
                url.Append($"&include={string.Join(",", includes)}");
            }

            var response = await EnsureClient().SendAsync(this.CreateRequest(HttpMethod.Get, url.ToString()));
            
            if (response.IsSuccessStatusCode)
            {
                var firstPage = await response.Content.ReadJsonAsync<Models.PcoListResponse<T>>();
                int totalPages = (int)Math.Ceiling((decimal)firstPage.Meta.TotalCount / (decimal)pageSize);
                int loadedPages = 1;
                
                if (loadedPages < pagesToLoad)
                {
                    var list = firstPage.Data.ToList();
                    var included = firstPage.Included.ToList();

                    while (loadedPages < Math.Min(pagesToLoad, totalPages))
                    {
                        response = await EnsureClient().SendAsync(this.CreateRequest(HttpMethod.Get, string.Concat(url.ToString(), $"&offset={pageSize * loadedPages}")));
                        var responseItems = await response.Content.ReadJsonAsync<Models.PcoListResponse<T>>();

                        list.AddRange(responseItems.Data);
                        included.AddRange(responseItems.Included);
                        loadedPages++;
                    }

                    firstPage.Data = list;
                    firstPage.Included = included;
                }

                return firstPage;
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }       
        
        public async Task<Models.PcoSingleResponse<T>> Get<T>(string path)
        {
            var response = await EnsureClient().SendAsync(this.CreateRequest(HttpMethod.Get, path));

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadJsonAsync<Models.PcoSingleResponse<T>>();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        public Task<Models.PcoSingleResponse<Models.PcoOrganization>> GetOrganization()
        {
            return this.Get<Models.PcoOrganization>("people/v2");
        }

        public async Task RefreshList(int listID)
        {
            var response = await EnsureClient().SendAsync(this.CreateRequest(HttpMethod.Post, $"people/v2/lists/{listID}/run"));

            response.EnsureSuccessStatusCode();
        }
    }
}
