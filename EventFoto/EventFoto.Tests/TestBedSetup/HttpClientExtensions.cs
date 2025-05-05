using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace EventFoto.Tests.TestBedSetup;

public static class HttpClientExtensions
{
    public static async Task<T?> SendRequest<T>(this HttpClient client, HttpRequestMessage request)
    {
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception($"Request failed with status code {response.StatusCode}: {content}");
        }

        var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result;
    }

    public static async Task<T?> SendRequest<T>(this HttpClient client, HttpRequestMessage request, HttpStatusCode expectedStatusCode)
    {
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != expectedStatusCode)
        {
            throw new Exception($"Request failed with status code {response.StatusCode}: {content}");
        }
        var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result;
    }
}
