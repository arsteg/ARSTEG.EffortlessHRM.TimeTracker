using System.Threading.Tasks;

namespace TimeTracker.Services.Interfaces
{
    /// <summary>
    /// Interface for HTTP operations, enabling dependency injection and unit testing.
    /// </summary>
    public interface IHttpProvider
    {
        Task<TResult> GetAsync<TResult>(string uri, string apiKey, string token = "");
        Task<TResult> GetWithTokenAsync<TResult>(string uri, string token = "");
        Task<TResult> PostAsync<TResult, T>(string uri, T data, string token = "");
        Task<TResult> PostAsync<TResult>(string uri, string apiKey, TResult data);
        Task PostAsync(string uri, string apiKey, string token = "");
        Task PostAsyncWithVoid<T>(string uri, T data, string token = "");
        Task<TResult> PostWithTokenAsync<TResult, T>(string uri, T data, string token);
        Task<TResult> DeleteWithTokenAsync<TResult>(string uri, string token);
        Task<TResult> PutWithTokenAsync<TResult, T>(string uri, T data, string token);
    }
}
