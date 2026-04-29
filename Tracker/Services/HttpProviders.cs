using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SendGrid.Helpers.Mail;
using TimeTracker.Models;
using TimeTracker.Services.Interfaces;
using TimeTracker.Trace;

namespace TimeTracker.Services
{
    public class HttpProviders : IHttpProvider
    {
        private readonly JsonSerializerSettings _serializerSettings;

        // Shared HttpClient instance to prevent socket exhaustion
        private static readonly HttpClient _sharedHttpClient;

        static HttpProviders()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            _sharedHttpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(100)
            };
        }

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
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("ApiKey", ApiKey);

                using HttpResponseMessage response = await _sharedHttpClient.SendAsync(request);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings);
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in GetAsync<TResult>(string uri, string ApiKey, string token = \"\"), calling url:{uri}"
                );
                return default;
            }
        }

        public async Task<TResult> GetWithTokenAsync<TResult>(string uri, string token = "")
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                var cookies =
                    $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";
                request.Headers.Add("Cookie", cookies);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using HttpResponseMessage response = await _sharedHttpClient.SendAsync(request);
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

                return default;
            }
        }

        public async Task<TResult> PostAsync<TResult, T>(string uri, T data, string token = "")
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Accept.Clear();

                // Log only email for login attempts (never log passwords)
                if (data is Login login)
                {
                    LogManager.Logger.Info($"Login attempt for: {login.email}");
                }

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                request.Content = content;

                using HttpResponseMessage response = await _sharedHttpClient
                    .SendAsync(request)
                    .ConfigureAwait(false);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings);

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in PostAsync<TResult,T>(string uri, T data, string token = \"\"), calling url:{uri}"
                );

                throw;
            }
        }

        public async Task PostAsyncWithVoid<T>(string uri, T data, string token = "")
        {
            try
            {
                var cookies =
                    $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";

                using var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Accept.Clear();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("Cookie", cookies);

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                request.Content = content;

                try
                {
                    using var response = await _sharedHttpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Warn($"PostAsyncWithVoid failed for {uri}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in PostAsyncWithVoid<T>(string uri, T data, string token = \"\"), calling url:{uri}"
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

                using var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Accept.Clear();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("Cookie", cookies);

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                request.Content = content;

                using HttpResponseMessage response = await _sharedHttpClient
                    .SendAsync(request)
                    .ConfigureAwait(false);

                // Check if the response indicates an Unauthorized status
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    LogManager.Logger.Warn($"Unauthorized access. URL: {uri}");
                    // Return a properly formed error response
                    return JsonConvert.DeserializeObject<TResult>(
                        "{\"statusCode\":401,\"status\":\"failed\",\"data\":null}",
                        _serializerSettings
                    );
                }

                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();
                TResult result = JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings);

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in PostWithTokenAsync<TResult, T>(string uri, T data, string token), calling url:{uri}"
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

                using var request = new HttpRequestMessage(HttpMethod.Delete, uri);
                request.Headers.Accept.Clear();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("Cookie", cookies);

                using HttpResponseMessage response = await _sharedHttpClient
                    .SendAsync(request)
                    .ConfigureAwait(false);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings);
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
                using var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Accept.Clear();
                request.Headers.Add("ApiKey", ApiKey);
                var content = new StringContent(
                    JsonConvert.SerializeObject(""),
                    Encoding.UTF8,
                    "application/json"
                );
                request.Content = content;

                using HttpResponseMessage response = await _sharedHttpClient
                    .SendAsync(request)
                    .ConfigureAwait(false);

                await HandleResponse(response);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in PostAsync(string uri, string ApiKey, string token = \"\"), calling url:{uri}"
                );
                throw;
            }
        }

        public async Task<TResult> PostAsync<TResult>(string uri, string ApiKey, TResult data)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Accept.Clear();
                request.Headers.Add("ApiKey", ApiKey);
                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                request.Content = content;

                using HttpResponseMessage response = await _sharedHttpClient
                    .SendAsync(request)
                    .ConfigureAwait(false);

                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings);

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in PostAsync<TResult>(string uri, string ApiKey, TResult data), calling url:{uri}"
                );
                throw;
            }
        }

        private async Task HandleResponse(HttpResponseMessage response)
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

        public async Task<TResult> PutWithTokenAsync<TResult, T>(string uri, T data, string token)
        {
            try
            {
                var cookies =
                    $"companyId={GlobalSetting.Instance.LoginResult.data.user.company.id}; jwt={token}; userId={GlobalSetting.Instance.LoginResult.data.user.id}";

                using var request = new HttpRequestMessage(HttpMethod.Put, uri);
                request.Headers.Accept.Clear();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("Cookie", cookies);

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );
                request.Content = content;

                using HttpResponseMessage response = await _sharedHttpClient
                    .SendAsync(request)
                    .ConfigureAwait(false);
                await HandleResponse(response);
                string serialized = await response.Content.ReadAsStringAsync();

                TResult result = JsonConvert.DeserializeObject<TResult>(serialized, _serializerSettings);
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(
                    ex,
                    $"error in PutWithTokenAsync<TResult, T>(string uri, T data, string token), calling url:{uri}"
                );
                throw;
            }
        }
    }
}
