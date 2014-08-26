using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Umbraco.PasteAndTranslate
{
    public class BingTranslator : ITranslator
    {
        private const int Timeout = 10000;
        private static string clientId;
        private static string secret;
        private AdmAccessToken token;

        static BingTranslator()
        {
            FindKeys();
        }

        public string Translate(string text, string from, string to)
        {
            EnsureToken();

            string body = null;

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"text", text},
                {"from", from},
                {"to", to},
            });
            content.ReadAsStringAsync()
                .ContinueWith(r => (object)(body = r.Result))
                .Wait(Timeout);
            var request = new HttpRequestMessage(HttpMethod.Get,
                "http://api.microsofttranslator.com/v2/Http.svc/Translate?" + body);
            request.Headers.Add("Authorization", "Bearer " + token.access_token);
            var translateTask = client.SendAsync(request)
                .ContinueWith(r => r.Result.Content.ReadAsStringAsync())
                //.Wait(Timeout);
            //result.Content.ReadAsStringAsync()
                //.ContinueWith(r => r.Result.ReadAsStringAsync())
                .ContinueWith(t =>
                {
                    var xml = t.Unwrap().Result;
                    var serializer = new XmlSerializer(typeof(string), "http://schemas.microsoft.com/2003/10/Serialization/");
                    return serializer.Deserialize(new StringReader(xml)) as string;

                });
            translateTask.Wait(Timeout);
            return translateTask.Result;
        }

        private void EnsureToken()
        {
            if (token != null)
                return;
            HttpResponseMessage msg = null;
            string result = null;
 
            if (String.IsNullOrWhiteSpace(clientId) || String.IsNullOrWhiteSpace(secret))
                throw new Exception("Configure 'PastAndTranslate/ClientId' and 'PastAndTranslate/Secret' appSettings or environment variables.");
           
            const string datamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", clientId},
                    {"client_secret", secret},
                    {"scope", "http://api.microsofttranslator.com"}
                }
            );
            client.PostAsync(datamarketAccessUri, content)
                .ContinueWith(r => msg = r.Result)
                .Wait(Timeout);
            msg.Content.ReadAsStringAsync()
                .ContinueWith(r => result = r.Result)
                .Wait(Timeout);
            token = JsonConvert.DeserializeObject<AdmAccessToken>(result);
        }

        private static void FindKeys()
        {
            // TODO: Config file

            clientId = GetConfiguredValue("PasteAndTranslate/ClientId");
            secret = GetConfiguredValue("PasteAndTranslate/Secret");
        }

        private static string GetConfiguredValue(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        }

        public class AdmAccessToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string scope { get; set; }
        }
    }


}
