using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Models;

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
                    uri = string.Format("{0}/{1}", uri.TrimEnd(trims), (uriParts[i] ?? string.Empty).TrimStart(trims));
                }
            }
            return uri;
        }

        public async Task<LoginResult> SignIn(Login login)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/users/login");
            var res = await _httpProvider.PostAsync<LoginResult, Login>(uri, login);
            return (res);
        }

        public async Task<TimeLog> AddTimeLog(TimeLog timeLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/timeLogs");
            var res = await _httpProvider.PostWithTokenAsync<TimeLog, TimeLog>(uri, timeLog, GlobalSetting.Instance.LoginResult.token);
            return (res);
        }

        public async Task<GetTimeLogAPIResult> GetTimeLogs(TimeLog timeLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/timeLogs/getTimeLogs");
            var res = await _httpProvider.PostWithTokenAsync<GetTimeLogAPIResult, TimeLog>(uri, timeLog, GlobalSetting.Instance.LoginResult.token);
            return (res);
        }
        public async Task<GetTimeLogAPIResult> GetCurrentWeekTotalTime(CurrentWeekTotalTime timeLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/timeLogs/getCurrentWeekTotalTime");
            var res = await _httpProvider.PostWithTokenAsync<GetTimeLogAPIResult, CurrentWeekTotalTime>(uri, timeLog, GlobalSetting.Instance.LoginResult.token);
            return (res);
        }

        public async Task<GetTimeLogAPIResult> GetTimeLogsWithImages(TimeLog timeLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/timeLogs/getLogsWithImages");
            var res = await _httpProvider.PostWithTokenAsync<GetTimeLogAPIResult, TimeLog>(uri, timeLog, GlobalSetting.Instance.LoginResult.token);
            return (res);
        }

        #region "ErrorLog"
        public void AddErrorLogs(ErrorLog errorLog)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/errorlogs/new");
            var res = _httpProvider.PostWithTokenAsync<AddErrorLogAPIResult, ErrorLog>(uri, errorLog, GlobalSetting.Instance.LoginResult.token).Result;
        }

        public async Task<GetErrorLogResult> GetErrorLogs(string userId)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/errorlogs/errorloglist/{userId}");
            var res = await _httpProvider.GetWithTokenAsync<GetErrorLogResult>(uri, GlobalSetting.Instance.LoginResult.token);
            return res;
        }
        #endregion

        #region "Project And Task"

        public async Task<GetProjectListAPIResult> GetProjectListByUserId(ProjectRequest projectRequest)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/project/projectlistbyuser");
            var res = await _httpProvider.PostWithTokenAsync<GetProjectListAPIResult, ProjectRequest>(uri, projectRequest, GlobalSetting.Instance.LoginResult.token);
            return res;
        }
        public async Task<GetTaskListAPIResult> GetTaskListByProject(string projectId)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/task/tasklistbyproject/{projectId}");
            var res = await _httpProvider.GetWithTokenAsync<GetTaskListAPIResult>(uri, GlobalSetting.Instance.LoginResult.token);
            return res;
        }

        public async Task<NewTaskResult> AddNewTask(CreateTaskRequest createTaskRequest)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/task/newtask");
            var res = await _httpProvider.PostWithTokenAsync<NewTaskResult, CreateTaskRequest>(uri, createTaskRequest, GlobalSetting.Instance.LoginResult.token);
            return res;
        }

        public async Task<string> AddNewProject(ProjectRequest projectRequest)
        {
            var uri = CombineUri(GlobalSetting.apiBaseUrl, $"/api/v1/project/newproject");
            var res = await _httpProvider.PostWithTokenAsync<object, ProjectRequest>(uri, projectRequest, GlobalSetting.Instance.LoginResult.token);
            return ("");
        }
        #endregion

        //public async Task<DeviceInstallation> UpdateDevice(DeviceInstallation di)
        //{
        //    var uri = CombineUri(GlobalSetting.DefaultEndpoint, $"/api/Mobile/UpdateDeviceGPI");
        //    DeviceInstallation res = await _httpProvider.PostAsync<DeviceInstallation>(uri, GlobalSetting.ApiKey, di);
        //    return (res);
        //}

        //public async Task<List<ES_VideoMediaCategory>> GetMedias(int nb, int W, int H)
        //{
        //    List<ES_VideoMediaCategory> data = new List<ES_VideoMediaCategory>();
        //    var uri = CombineUri(GlobalSetting.DefaultEndpoint, $"/api/Mobile/GetMedias?nb=" + nb);

        //    try
        //    {
        //        data = await _httpProvider.GetAsync<List<ES_VideoMediaCategory>>(uri, GlobalSetting.ApiKey);
        //        if (data != null)
        //        {
        //            foreach (var e in data)
        //            {
        //                foreach (var v in e.esVideos)
        //                {
        //                    //v.img = new Xamarin.Forms.UriImageSource() { CachingEnabled=false, Uri = new Uri(string.Format("https://content.grandprix.info/gptv/medias/{1}/{2}/{0}.jpg", v.VideoId.ToString().ToLower(), W, H)) };
        //                    //v.VideoPictureMiniatureURL = "http://loremflickr.com/600/600/nature?filename=simple.jpg";
        //                    v.VideoPictureMiniatureURL = string.Format("https://content.grandprix.info/gptv/medias/{1}/{2}/{0}.jpg", v.VideoId.ToString().ToLower(), W, H);
        //                }
        //            }
        //        }
        //        else
        //            return (new List<ES_VideoMediaCategory>());
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(@"\tERROR {0}", ex.Message);
        //    }

        //    return (data);
        //}

        //public async Task<ES_VideoMediaCategory> GetMediasCategory(string catid, int nb, int W, int H)
        //{
        //    ES_VideoMediaCategory data = null;
        //    var uri = CombineUri(GlobalSetting.DefaultEndpoint, $"/api/Mobile/GetMediasCategory?category=" + catid + "&nb=" + nb);

        //    try
        //    {
        //        data = await _httpProvider.GetAsync<ES_VideoMediaCategory>(uri, GlobalSetting.ApiKey);
        //        if (data != null)
        //        {
        //            foreach (var v in data.esVideos)
        //            {
        //                v.VideoPictureMiniatureURL = string.Format("https://content.grandprix.info/gptv/medias/{1}/{2}/{0}.jpg", v.VideoId.ToString().ToLower(), W, H);
        //                if (string.IsNullOrEmpty(v.VideoDescription) == false && v.VideoDescription.Length > 100)
        //                    v.VideoDescription = v.VideoDescription.Substring(0, 100) + " ...";
        //                else
        //                    v.VideoDescription = v.VideoDescription;
        //            }

        //        }
        //        else
        //            return (null);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(@"\tERROR {0}", ex.Message);
        //    }

        //    return (data);
        //}

        //public async Task<EsVideo> GetVideo(long VideoId, string token, int W = 160, int H = 90)
        //{
        //    EsVideo data = null;

        //    var uri = CombineUri(GlobalSetting.DefaultEndpoint, $"/api/Mobile/GetVideo?VideoId=" + VideoId);
        //    try
        //    {
        //        data = await _httpProvider.GetAsync<EsVideo>(uri, GlobalSetting.ApiKey, token);
        //        if (data != null)
        //            data.VideoPictureMiniatureURL = string.Format("https://content.grandprix.info/gptv/medias/{1}/{2}/{0}.jpg", data.VideoId.ToString().ToLower(), W, H);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(@"\tERROR {0}", ex.Message);
        //    }
        //    return data;
        //}

        //public async Task<classConfig> GetMenu()
        //{
        //    classConfig res = null;
        //    try
        //    {
        //        HttpClient client = new HttpClient();

        //        var uri = CombineUri(GlobalSetting.DefaultEndpoint, $"/gpi/ws/fr/ios-phone/config.json");
        //        res = await _httpProvider.GetAsync<classConfig>(uri, GlobalSetting.ApiKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(@"\tERROR {0}", ex.Message);
        //    }

        //    return res;
        //}

        //public async Task<classRubrique> GetModuleCollection(string url)
        //{
        //    classRubrique res = null;
        //    try
        //    {
        //        HttpClient client = new HttpClient();
        //        var uri = CombineUri(GlobalSetting.DefaultEndpoint, url);
        //        res = await _httpProvider.GetAsync<classRubrique>(uri, GlobalSetting.ApiKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(@"\tERROR {0}", ex.Message);
        //    }

        //    return res;
        //}

    }
}
