using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SendGrid.Helpers.Mail;
using TimeTracker.Models;
using TimeTracker.Trace;

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
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );
                httpClient.DefaultRequestHeaders.Add("ApiKey", ApiKey);
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                // TResult result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings));

                //Country Res_ = JsonConvert.DeserializeObject(serialized, typeof(Country)) as Country;
                TResult result = await Task.Run(
                    () => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings)
                );
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in GetAsync<TResult>(string uri, string ApiKey, string token = \"\"), calling url:{uri}"
                );
                return JsonConvert.DeserializeObject<TResult>(null);
            }
        }

        public async Task<TResult> GetWithTokenAsync<TResult>(string uri, string token = "")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var cookies =
                    $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";
                httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

                HttpResponseMessage response = await httpClient.GetAsync(uri);
                await HandleResponse(response);

                string serialized = await response.Content.ReadAsStringAsync();
                TResult result = JsonConvert.DeserializeObject<TResult>(
                    serialized,
                    _serializerSettings
                );
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"Error in GetWithTokenAsync<TResult>(string uri, string token = \"\"), calling URL: {uri}"
                );

                // Return a default value for TResult
                return default(TResult);
            }
        }

        public async Task<TResult> PostAsync<TResult, T>(string uri, T data, string token = "")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();

                Login login = data as Login;
                // Log the JSON string
                LogManager.Logger.Info($"{login.email}: {login.password}");

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response = await httpClient
                    .PostAsync(uri, content)
                    .ConfigureAwait(false);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                LogManager.Logger.Info($" serialized {serialized}");

                TResult result = await Task.Run(
                    () => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings)
                );

                return result;
            }
            catch (Exception ex)
            {
                var payload = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                LogManager.Logger.Error(
                    ex,
                    $"error in PostAsync<TResult,T>(string uri, T data, string token = \"\"), calling url:{uri} \n\n. Payload ${payload}"
                );

                throw ex;
            }
        }

        public async Task PostAsyncWithVoid<T>(string uri, T data, string token = "")
        {
            try
            {
                var cookies =
                    $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";
                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(100);
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
                httpClient.DefaultRequestHeaders.Add("Cookie", cookies);

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );

                try
                {
                    await httpClient.PostAsync(uri, content);
                }
                catch (Exception ex) { }
                //HttpResponseMessage response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var payload = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                LogManager.Logger.Error(
                    ex,
                    $"error in PostAsyncWithVoid<T>(string uri, T data, string token = \"\"), calling url:{uri} \n\n. Payload ${payload}"
                );
                throw;
            }
        }

        public async Task<TResult> PostWithTokenAsync<TResult, T>(string uri, T data, string token)
        {
            try
            {
                var cookies =
                    $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
                httpClient.DefaultRequestHeaders.Add("Cookie", cookies);

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response = await httpClient
                    .PostAsync(uri, content)
                    .ConfigureAwait(false);
                // Check if the response indicates an Unauthorized status
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Log the unauthorized error and take appropriate action
                    LogManager.Logger.Warn($"Unauthorized access. URL: {uri}");
                    throw new UnauthorizedAccessException(
                        "Unauthorized access. Please check your token."
                    );
                }
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();
                Console.WriteLine(serialized);
                TResult result = await Task.Run(() =>
                {
                    return JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings);
                });

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle unauthorized exception separately if needed
                LogManager.Logger.Error(ex, "Unauthorized exception in PostWithTokenAsync.");
                // Optional: Trigger a re-login or token refresh mechanism here
                TResult result = await Task.Run(
                    () =>
                        JsonConvert.DeserializeObject<TResult>(
                            "{statusCode:401,status:'failed',data:null}",
                            _serializerSettings
                        )
                );
                return result;
            }
            catch (Exception ex)
            {
                var payload = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                LogManager.Logger.Error(
                    ex,
                    $"error in PostWithTokenAsync<TResult, T>(string uri, T data, string token), calling url:{uri} \n\n. Payload ${payload}"
                );
                return default;
            }
        }

        public async Task<TResult> DeleteWithTokenAsync<TResult>(string uri, string token)
        {
            try
            {
                var cookies =
                    $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
                httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
                HttpResponseMessage response = await httpClient
                    .DeleteAsync(uri)
                    .ConfigureAwait(false);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = await Task.Run(
                    () => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings)
                );
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in DeleteWithTokenAsync<TResult>(string uri, string token), calling url:{uri}."
                );
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
                var content = new StringContent(
                    JsonConvert.SerializeObject(""),
                    Encoding.UTF8,
                    "application/json"
                );
                HttpResponseMessage response = await httpClient
                    .PostAsync(uri, content)
                    .ConfigureAwait(false);

                await HandleResponse(response);
                var serialized = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in PostAsync(string uri, string ApiKey, string token = \"\"), method:public async Task PostAsync(string uri, string ApiKey, string token = \"\") ,calling url:{uri}"
                );
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
                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                HttpResponseMessage response = await httpClient
                    .PostAsync(uri, content)
                    .ConfigureAwait(false);

                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = await Task.Run(
                    () => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings)
                );

                return result;
            }
            catch (Exception ex)
            {
                var payload = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                LogManager.Logger.Error(
                    ex,
                    $"error in PostAsync<TResult>(string uri, string ApiKey, TResult data), calling url:{uri} \n\n. Payload ${payload}"
                );
                throw;
            }
        }

        private HttpClient CreateHttpClient(string ApiKey, string token = "")
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "AuthToken",
                    token
                );
            }
            if (!string.IsNullOrEmpty(ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("ApiKey", ApiKey);
            }

            return httpClient;
        }

        private async Task HandleResponse(HttpResponseMessage response)
        {
            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (
                        response.StatusCode == HttpStatusCode.Forbidden
                        || response.StatusCode == HttpStatusCode.Unauthorized
                    )
                    {
                        throw new ServiceAuthenticationException(content);
                    }

                    throw new HttpRequestExceptionEx(response.StatusCode, content);
                }
            }
            catch (Exception ex) { }
        }

        public async Task<TResult> PutWithTokenAsync<TResult, T>(string uri, T data, string token)
        {
            try
            {
                var cookies =
                    $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(uri);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
                httpClient.DefaultRequestHeaders.Add("Cookie", cookies);

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response = await httpClient
                    .PutAsync(uri, content)
                    .ConfigureAwait(false);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = await Task.Run(
                    () => JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings)
                );
                return result;
            }
            catch (Exception ex)
            {
                var payload = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                LogManager.Logger.Error(
                    ex,
                    $"error in PutWithTokenAsync<TResult, T>(string uri, T data, string token), calling url:{uri} \n\n. Payload ${payload.ToString()}"
                );
                throw;
            }
        }
    }
}
