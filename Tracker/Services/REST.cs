using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BrowserHistoryGatherer;
using BrowserHistoryGatherer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TimeTracker.Models;
using TimeTracker.Trace;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace TimeTracker.Services
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

        public async Task<UserPreferenceResult> GetUserPreferencesSetting(string url)
        {
            var result = new UserPreferenceResult
            {
                isBlurScreenshot = false,
                isBeepSoundEnabled = false,
                isScreenshotNotificationEnabled = false
            };

            try
            {
                var uri = CombineUri(GlobalSetting.apiBaseUrl, url);
                var root = await _httpProvider.GetWithTokenAsync<UserPreferencesResponse>(
                    uri,
                    GlobalSetting.Instance.LoginResult.token
                );

                if (root == null || root.data?.preferences == null)
                {
                    return result;
                }

                result.status = root.status;
                result.message = root.message;
                result.statusCode = root.statusCode;
                var preferences = root.data.preferences;

                string GetValue(string key) =>
                    preferences.FirstOrDefault(p => p.preferenceOptionId?.preferenceKey == key)
                       ?.preferenceOptionId?.preferenceValue;

                result.isBeepSoundEnabled = GetValue(GlobalSetting.Instance.userPreferenceKey.ScreenshotSoundDisabled)?.ToLower() == "true";
                result.isScreenshotNotificationEnabled = GetValue(GlobalSetting.Instance.userPreferenceKey.ScreenshotNotificationDisabled)?.ToLower() == "true";
                result.isBlurScreenshot = GetValue(GlobalSetting.Instance.userPreferenceKey.ScreenshotBlur)?.ToLower() == "true";
                result.weeklyHoursLimit = int.TryParse(GetValue(GlobalSetting.Instance.userPreferenceKey.WeeklyHoursLimit), out var weeklyLimit)
                    ? weeklyLimit : 0;
                result.monthlyHoursLimit = int.TryParse(GetValue(GlobalSetting.Instance.userPreferenceKey.MonthlyHoursLimit), out var monthlyLimit)
                    ? monthlyLimit : 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"\tERROR {ex.Message}");
                result.status = "error";
                result.message = ex.Message;
                result.statusCode = HttpStatusCode.InternalServerError;
            }

            return result;
        }

        public async Task<BaseResponse> SetUserPreferences(string url, CreateUserPreferenceRequest createUserPreferenceRequest)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, url);
            var res = await _httpProvider.PostWithTokenAsync<BaseResponse, CreateUserPreferenceRequest>(
                uri,
                createUserPreferenceRequest,
                GlobalSetting.Instance.LoginResult.token
            );
            return res;
        }

        public async Task<Project> GetUserPreferencesSettingByKey(string url)
        {
            try
            {
                var uri = CombineUri(GlobalSetting.apiBaseUrl, url);
                var root = await _httpProvider.GetWithTokenAsync<UserPreferencesResponse>(
                    uri,
                    GlobalSetting.Instance.LoginResult.token
                );

                var preferenceValue = root?.data?.preferences?.FirstOrDefault()?.preferenceOptionId?.preferenceValue;

                if (!string.IsNullOrWhiteSpace(preferenceValue))
                {
                    var parts = preferenceValue.Split('#');
                    if (parts.Length >= 2)
                    {
                        if (parts[0].Length > 0 && parts[1].Length > 0)
                            return new Project
                            {
                                _id = parts[0],
                                projectName = parts[1]
                            };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }

            return null;
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
