using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using PCLCrypto;
using System.Threading.Tasks;

namespace LinkBlync.Client
{
    public class ServiceBusClient : IDisposable
    {
        private const string nameSpace = "link-blync";

        private const string queueName = "colors";

        private const string sasKeyName = "sender";

        private const string sasKey = "sharedkey";

        private static string endPoint = $"https://{nameSpace}.servicebus.windows.net/";

        public static async Task<HttpResponseMessage> PostToQueueAsync(string value)
        {
            var client = GetClient();
            client.BaseAddress = new Uri(endPoint);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();

            var token = GetSasToken();
            client.DefaultRequestHeaders.Add("Authorization", token);

            var content = new StringContent(value, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var path = endPoint + queueName + "/messages";
            return await client.PostAsync(path, content).ConfigureAwait(false);
        }

        private static string GetSasToken()
        {
            var expiry = GetExpiry();
            var stringToSign = WebUtility.UrlEncode(endPoint + queueName) + "\n" + expiry;

            var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha256);
            var hasher = algorithm.CreateHash(Encoding.UTF8.GetBytes(sasKey));
            hasher.Append(Encoding.UTF8.GetBytes(stringToSign));
            var mac = hasher.GetValueAndReset();
            var signature = Convert.ToBase64String(mac);

            var sasToken = string.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", WebUtility.UrlEncode(endPoint + queueName), WebUtility.UrlEncode(signature), expiry, sasKeyName);
            return sasToken;
        }

        private static string GetExpiry()
        {
            var sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToString((int)sinceEpoch.TotalSeconds + 3600);
        }

        private static HttpClient httpClient;

        private static HttpClient GetClient()
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            return httpClient;
        }

        public void Dispose() => httpClient?.Dispose();
    }
}
