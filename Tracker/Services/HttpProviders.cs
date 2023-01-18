using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class HttpProviders
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public HttpProviders()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };
            _serializerSettings.Converters.Add(new StringEnumConverter());
        }

        public Task DeleteAsync(string uri, string ApiKey, string token = "")
        {
            throw new NotImplementedException();
        }

        /// Get request
        public async Task<TResult> GetAsync<TResult>(string uri, string ApiKey, string token = "")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("ApiKey", ApiKey);
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                // TResult result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));

                //Country Res_ = JsonConvert.DeserializeObject(serialized, typeof(Country)) as Country;
                TResult result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));
                return result;
            }
            catch (Exception ex)
            {
                return JsonConvert.DeserializeObject<TResult>(null);
            }
        }

        public async Task<TResult> GetWithTokenAsync<TResult>(string uri, string token = "")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                var cookies = $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";
                httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();
                TResult result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));
                return result;
            }
            catch (Exception ex)
            {
                return JsonConvert.DeserializeObject<TResult>(null);
            }
        }

        public async Task<TResult> PostAsync<TResult,T>(string uri, T data, string token = "")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = await Task.Run(() =>
                 JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));

                return result;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<TResult> PostWithTokenAsync<TResult, T>(string uri, T data, string token)
        {
            try
            {
                var cookies = $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpClient.DefaultRequestHeaders.Add("Cookie", cookies);

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = await Task.Run(() =>
                 JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));

                return result;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task PostAsync(string uri, string ApiKey, string token = "")
        {
            try
            {

                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("ApiKey", ApiKey);
                var content = new StringContent(JsonConvert.SerializeObject(""), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);

                await HandleResponse(response);
                var serialized = await response.Content.ReadAsStringAsync();


            }
            catch (Exception ex)
            {

                throw;
            }
        }               

        public async Task<TResult> PostAsync<TResult>(string uri, string ApiKey, TResult data)
        {
            try
            {

                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("ApiKey", ApiKey);
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);

                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = await Task.Run(() =>
                 JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));

                return result;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        
        private HttpClient CreateHttpClient(string ApiKey, string token = "")
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("AuthToken", token);
            }
            if (!string.IsNullOrEmpty(ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("ApiKey", ApiKey);
            }

            return httpClient;
        }
        private async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.Forbidden ||
                    response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ServiceAuthenticationException(content);
                }

                throw new HttpRequestExceptionEx(response.StatusCode, content);
            }
        }
    }
}
