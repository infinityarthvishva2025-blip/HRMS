using HRMS.DTOs;
using Newtonsoft.Json;
using System.Text;

public class PanVerificationService
{
    private readonly HttpClient _httpClient;

    private readonly string apiKey = "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb";
    private readonly string apiSecret = "secret_live_bea7b5d94c1b487d803926a63195b4f3";

    public PanVerificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> Authenticate()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("x-api-secret", apiSecret);

        var response = await _httpClient.PostAsync(
            "https://api.sandbox.co.in/authenticate",
            null
        );

        var result = await response.Content.ReadAsStringAsync();

        var auth = JsonConvert.DeserializeObject<SandboxAuthResponseDto>(result);

        return auth?.data?.access_token;
    }

    public async Task<PanVerifyResponseDto> VerifyPan(
        string token,
        PanVerifyRequest request
    )
    {
        _httpClient.DefaultRequestHeaders.Clear();

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var json = JsonConvert.SerializeObject(request);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            "https://api.sandbox.co.in/kyc/pan/verify",
            content
        );

        var result = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<PanVerifyResponseDto>(result);
    }
}