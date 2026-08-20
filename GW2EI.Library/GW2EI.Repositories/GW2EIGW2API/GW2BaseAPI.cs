using System.Net.Http.Json;
using System.Text.Json;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public class GW2BaseAPI<T>(string APIPath) : IGW2BaseAPI<T> where T : GW2APIBaseItem
{
    private static HttpClient APIClient;
    private static HttpClient GetAPIClient()
    {
        if (APIClient == null)
        {
            APIClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.guildwars2.com")
            };
            APIClient.DefaultRequestHeaders.Accept.Clear();
            APIClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        return APIClient;
    }

    public async Task<IEnumerable<T>> GetGW2APIItems()
    {
        var itemList = new List<T>();
        int page = 0;
        int pagesize = 200;
        HttpClient apiClient = GetAPIClient();
        while (true)
        {
            string path = APIPath + "?page=" + page + "&page_size=" + pagesize + "&lang=en";
            HttpResponseMessage response = await apiClient.GetAsync(new Uri(path, UriKind.Relative));
            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            JsonSerializerOptions deSerializerSettings = new()
            {
                WriteIndented = false,
                IncludeFields = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                //NOTE(Rennorb): does html escape by default
            };
            T[] responseArray = await response.Content.ReadFromJsonAsync<T[]>(deSerializerSettings);
            itemList.AddRange(responseArray);
            page++;
        }

        return itemList;
    }
}
