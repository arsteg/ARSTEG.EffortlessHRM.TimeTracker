using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using BrowserHistoryGatherer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TimeTrackerX.Models;
using TimeTrackerX.Models;
using TimeTrackerX.Trace;
using TimeTrackerX.Utilities;

namespace TimeTrackerX.Services
{
    public class REST
    {
        private readonly JsonSerializerSettings _serializerSettings;
        HttpProviders _httpProvider;

        public REST(HttpProviders httpProvider)
        {
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore
            };
            _serializerSettings.Converters.Add(new StringEnumConverter());
            _httpProvider = httpProvider;
        }

        public static string CombineUri(params string[] uriParts)
        {
            string uri = string.Empty;
            if (uriParts != null && uriParts.Count() > 0)
            {
                char[] trims = new char[] { '\\', '/' };
                uri = (uriParts[0] ?? string.Empty).TrimEnd(trims);
                for (int i = 1; i < uriParts.Count(); i++)
                {
                    uri = string.Format(
                        "{0}/{1}",
                        uri.TrimEnd(trims),
                        (uriParts[i] ?? string.Empty).TrimStart(trims)
                    );
                }
            }
            return uri;
        }

        public async Task<LoginResult> SignIn(Login login)
        {
            LogManager.Logger.Info(
                $"calling Task<LoginResult> SignIn(Login login),  login={login.ToString()}"
            );
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/users/login");
            var res = await _httpProvider.PostAsync<LoginResult, Login>(uri, login);
            return (res);
        }

        public async Task<AddTimeLogAPIResult> AddTimeLog(TimeLog timeLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/timeLogs");
            var res = await _httpProvider.PostWithTokenAsync<AddTimeLogAPIResult, TimeLog>(
                uri,
                timeLog,
                GlobalSetting.Instance.LoginResult.token
            );
            return (res);
        }

        public async Task<GetTimeLogAPIResult> GetTimeLogs(TimeLog timeLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/timeLogs/getTimeLogs");
            var res = await _httpProvider.PostWithTokenAsync<GetTimeLogAPIResult, TimeLog>(
                uri,
                timeLog,
                GlobalSetting.Instance.LoginResult.token
            );
            return (res);
        }

        public async Task<GetTimeLogAPIResult> GetCurrentWeekTotalTime(CurrentWeekTotalTime timeLog)
        {
            var uri = CombineUri(
                GlobalSetting.apiBaseUrl,
                $"/api/v1/timeLogs/getCurrentWeekTotalTime"
            );
            var res = await _httpProvider.PostWithTokenAsync<
                GetTimeLogAPIResult,
                CurrentWeekTotalTime
            >(uri, timeLog, GlobalSetting.Instance.LoginResult.token);
            return (res);
        }

        public async Task<GetTimeLogAPIResult> GetTimeLogsWithImages(TimeLog timeLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/timeLogs/getLogsWithImages");
            var res = await _httpProvider.PostWithTokenAsync<GetTimeLogAPIResult, TimeLog>(
                uri,
                timeLog,
                GlobalSetting.Instance.LoginResult.token
            );
            return (res);
        }

        #region "ErrorLog"
        public void AddErrorLogs(ErrorLog errorLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/errorlogs/new");
            var res = _httpProvider
                .PostWithTokenAsync<AddErrorLogAPIResult, ErrorLog>(
                    uri,
                    errorLog,
                    GlobalSetting.Instance.LoginResult.token
                )
                .Result;
        }

        public async Task<GetErrorLogResult> GetErrorLogs(string userId)
        {
            var uri = CombineUri(
                GlobalSetting.apiBaseUrl,
                $"/api/v1/errorlogs/errorloglist/{userId}"
            );
            var res = await _httpProvider.GetWithTokenAsync<GetErrorLogResult>(
                uri,
                GlobalSetting.Instance.LoginResult.token
            );
            return res;
        }
        #endregion

        #region "Project And Task"

        public async Task<GetProjectListAPIResult> GetProjectListByUserId(
            ProjectRequest projectRequest
        )
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/project/projectlistbyuser");
            var res = await _httpProvider.PostWithTokenAsync<
                GetProjectListAPIResult,
                ProjectRequest
            >(uri, projectRequest, GlobalSetting.Instance.LoginResult.token);
            return res;
        }

        public async Task<GetTaskListAPIResult> GetTaskListByProject(TaskRequest taskRequest)
        {
            var uri = CombineUri(
                GlobalSetting.apiBaseUrl,
                $"/api/v1/task/getUserTaskListByProject"
            );
            var res = await _httpProvider.PostWithTokenAsync<GetTaskListAPIResult, TaskRequest>(
                uri,
                taskRequest,
                GlobalSetting.Instance.LoginResult.token
            );
            return res;
        }

        public async Task<NewTaskResult> AddNewTask(CreateTaskRequest createTaskRequest)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/task/newtask");
            var res = await _httpProvider.PostWithTokenAsync<NewTaskResult, CreateTaskRequest>(
                uri,
                createTaskRequest,
                GlobalSetting.Instance.LoginResult.token
            );
            return res;
        }

        public async Task<string> AddNewProject(ProjectRequest projectRequest)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/project/newproject");
            var res = await _httpProvider.PostWithTokenAsync<object, ProjectRequest>(
                uri,
                projectRequest,
                GlobalSetting.Instance.LoginResult.token
            );
            return ("");
        }

        public async Task<NewTaskResult> CompleteATask(string taskId, ExpandoObject status)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/task/update/{taskId}");
            var res = await _httpProvider.PutWithTokenAsync<NewTaskResult, ExpandoObject>(
                uri,
                status,
                GlobalSetting.Instance.LoginResult.token
            );
            return res;
        }
        #endregion

        #region "Track Application Website"
        public async Task<ApplicationLogResult> AddUsedApplicationLog(ApplicationLog applicationLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/appWebsite/create");
            var res = await _httpProvider.PostWithTokenAsync<ApplicationLogResult, ApplicationLog>(
                uri,
                applicationLog,
                GlobalSetting.Instance.LoginResult.token
            );
            return res;
        }
        #endregion

        #region "Live screen"
        public async Task sendLiveScreenData(LiveImageRequest timeLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/liveTracking/save");
            await _httpProvider.PostAsyncWithVoid<LiveImageRequest>(
                uri,
                timeLog,
                GlobalSetting.Instance.LoginResult.token
            );
        }

        public async Task sendLiveScreenDataV1(LiveImageRequest timeLog)
        {
            var uri = CombineUri(
                GlobalSetting.apiBaseUrl,
                $"/api/v1/liveTracking/updateUserScreen"
            );
            await _httpProvider.PostAsyncWithVoid<LiveImageRequest>(
                uri,
                timeLog,
                GlobalSetting.Instance.LoginResult.token
            );
        }

        public async Task<CheckLiveScreenResponse> checkLiveScreen(TaskUser user)
        {
            var uri = CombineUri(
                GlobalSetting.apiBaseUrl,
                $"/api/v1/liveTracking/getLiveTrackingByUserId"
            );
            var res = await _httpProvider.PostWithTokenAsync<CheckLiveScreenResponse, TaskUser>(
                uri,
                user,
                GlobalSetting.Instance.LoginResult.token
            );
            return res;
        }
        #endregion

        #region Productivity Applications

        public async Task<ProductivityAppResult> GetProductivityApps(string url)
        {
            ProductivityAppResult res = new ProductivityAppResult();
            try
            {
                HttpClient client = new HttpClient();
                var uri = CombineUri(GlobalSetting.apiBaseUrl, url);
                res = await _httpProvider.GetWithTokenAsync<ProductivityAppResult>(
                    uri,
                    GlobalSetting.Instance.LoginResult.token
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
            return res;
        }

        public async Task<ProductivityAppAddResult> AddProductivityApps(
            string url,
            ProductivityModel productivityModel
        )
        {
            ProductivityAppAddResult res = new ProductivityAppAddResult();
            try
            {
                HttpClient client = new HttpClient();
                var uri = CombineUri(GlobalSetting.apiBaseUrl, url);
                res = await _httpProvider.PostWithTokenAsync<
                    ProductivityAppAddResult,
                    ProductivityModel
                >(uri, productivityModel, GlobalSetting.Instance.LoginResult.token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
            return res;
        }

        public async Task<ProductivityAppDeleteResult> DeleteProductivityApp(string id)
        {
            ProductivityAppDeleteResult res = new ProductivityAppDeleteResult();
            try
            {
                HttpClient client = new HttpClient();
                var uri = CombineUri(GlobalSetting.apiBaseUrl, id);
                res = await _httpProvider.DeleteWithTokenAsync<ProductivityAppDeleteResult>(
                    uri,
                    GlobalSetting.Instance.LoginResult.token
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
            return res;
        }

        public async Task<Object> AddBrowserHistory(HistoryEntry historyEntry)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/appWebsite/browser-history");
            var res = await _httpProvider.PostWithTokenAsync<Object, HistoryEntry>(
                uri,
                historyEntry,
                GlobalSetting.Instance.LoginResult.token
            );
            return (res);
        }
        #endregion

        #region User Preferences

        public async Task<EnableBeepSoundResult> GetEnableBeepSoundSetting(string url)
        {
            var res = new EnableBeepSoundResult();
            try
            {
                HttpClient client = new HttpClient();
                var uri = CombineUri(GlobalSetting.apiBaseUrl, url);
                res = await _httpProvider.GetWithTokenAsync<EnableBeepSoundResult>(
                    uri,
                    GlobalSetting.Instance.LoginResult.token
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
            return res;
        }
        #endregion

        #region set User status
        // New method to update online status
        public async Task<UpdateOnlineStatusResult> UpdateOnlineStatus(
            string userId,
            string machineId,
            bool isOnline,
            string project,
            string task
        )
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, "/api/v1/common/onlineStatus");
            var requestData = new
            {
                userId,
                machineId,
                isOnline,
                project,
                task
            };
            LogManager.Logger.Info(
                $"Calling UpdateOnlineStatus with userId={userId}, machineId={machineId}, isOnline={isOnline}, project ={project} , task ={task}"
            );
            var res = await _httpProvider.PostWithTokenAsync<UpdateOnlineStatusResult, object>(
                uri,
                requestData,
                GlobalSetting.Instance.LoginResult.token
            );
            return res;
        }
        #endregion
    }
}
