//namespace HRMS.Services
//{
//    public class FaceService
//    {
//    }
//}

using System.Net.Http.Headers;
using System.Text.Json;

namespace HRMS.Services
{
    public class FaceService
    {
        private readonly HttpClient _http;
        private readonly string _endpoint;
        private readonly string _key;

        public FaceService(IConfiguration config, HttpClient http)
        {
            _endpoint = config["AzureFace:Endpoint"] ?? throw new ArgumentNullException("AzureFace:Endpoint");
            _key = config["AzureFace:Key"] ?? throw new ArgumentNullException("AzureFace:Key");
            _http = http;
            _http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _key);
        }

        // Detect faces and return faceIds (for verify)
        public async Task<List<string>> DetectFacesAsync(Stream imageStream)
        {
            var url = $"{_endpoint}/face/v1.0/detect?returnFaceId=true";
            using var content = new StreamContent(imageStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var res = await _http.PostAsync(url, content);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var list = new List<string>();
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (el.TryGetProperty("faceId", out var fid))
                    list.Add(fid.GetString()!);
            }
            return list;
        }

        // Verify two faceIds (returns boolean matched + confidence)
        public async Task<(bool isIdentical, double confidence)> VerifyFacesAsync(string faceId1, string faceId2)
        {
            var url = $"{_endpoint}/face/v1.0/verify";
            var body = JsonSerializer.Serialize(new { faceId1, faceId2 });
            using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(url, content);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var isIdentical = doc.RootElement.GetProperty("isIdentical").GetBoolean();
            var confidence = doc.RootElement.GetProperty("confidence").GetDouble();
            return (isIdentical, confidence);
        }
    }
}

