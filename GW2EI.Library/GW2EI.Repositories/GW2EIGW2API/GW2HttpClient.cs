using System.Net.Http.Json;
using System.Text.Json;
using GW2EIGW2API.GW2API;
using GW2EIGW2API.Interfaces;

namespace GW2EIGW2API;

public class GW2HttpClient : IGW2HttpClient
{
    private static readonly HttpClient _aPIClient = GetAPIClient();
    private static HttpClient GetAPIClient()
    {
        HttpClient APIClient = new()
        {
            BaseAddress = new Uri("https://api.guildwars2.com")
        };
        APIClient.DefaultRequestHeaders.Accept.Clear();
        APIClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return APIClient;
    }

    private readonly JsonSerializerOptions deserializerSettings = new()
    {
        WriteIndented = false,
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        //NOTE(Rennorb): does html escape by default
    };

    public async Task<IEnumerable<T>> GetGW2APIItems<T>(string apiPath, CancellationToken ct = default) where T : GW2APIBaseItem
    {
        var itemList = new List<T>();
        int page = 0;
        int pagesize = 200;
        while (true)
        {
            string path = apiPath + "?page=" + page + "&page_size=" + pagesize + "&lang=en";
            HttpResponseMessage response = await _aPIClient.GetAsync(path, ct);
            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            T[] responseArray = await response.Content.ReadFromJsonAsync<T[]>(deserializerSettings, ct) ?? [];
            itemList.AddRange(responseArray);
            page++;
        }

        return itemList;
    }

    public async Task<T?> GetGW2APIItem<T>(string apiPath, CancellationToken ct = default) where T : GW2APIBaseItem
    {
        HttpResponseMessage response = await _aPIClient.GetAsync(apiPath, ct);
        response.EnsureSuccessStatusCode();

        T? item = await response.Content.ReadFromJsonAsync<T>(deserializerSettings, ct);
        return item;
    }
}
