using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionSvcHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config) {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<Item>> GetItemsForSearchDb()
        {
            var lastUpdated = await DB.Find<Item, string>()
                .Sort(x => x.Descending(x => x.UpdatedAt))
                .Project(x => x.UpdatedAt.ToString())
                .ExecuteFirstAsync();

            string _url = _config["ServicesConfiguration:AuctionServiceUrl"];
            string _requestUrl = "";

            if (string.IsNullOrEmpty(lastUpdated)) {
                _requestUrl = string.Concat(_url, "api/auctions");
            }
            else
            {
                _requestUrl = string.Concat(_url, "api/auctions?date=", lastUpdated);
            }

            return await _httpClient.GetFromJsonAsync<List<Item>>(_requestUrl);
        }
    }
}
