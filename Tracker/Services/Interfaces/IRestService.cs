using System.Dynamic;
using System.Threading.Tasks;
using BrowserHistoryGatherer.Data;
using TimeTracker.Models;

namespace TimeTracker.Services.Interfaces
{
    /// <summary>
    /// Interface for REST API operations, enabling dependency injection and unit testing.
    /// </summary>
    public interface IRestService
    {
        // Authentication
        Task<LoginResult> SignIn(Login login);

        // Time Logs
        Task<AddTimeLogAPIResult> AddTimeLog(TimeLog timeLog);
        Task<GetTimeLogAPIResult> GetTimeLogs(TimeLog timeLog);
        Task<GetTimeLogAPIResult> GetCurrentWeekTotalTime(CurrentWeekTotalTime timeLog);
        Task<GetTimeLogAPIResult> GetTimeLogsWithImages(TimeLog timeLog);

        // Error Logs
        void AddErrorLogs(ErrorLog errorLog);
        Task<GetErrorLogResult> GetErrorLogs(string userId);

        // Projects and Tasks
        Task<GetProjectListAPIResult> GetProjectListByUserId(ProjectRequest projectRequest);
        Task<GetTaskListAPIResult> GetTaskListByProject(TaskRequest taskRequest);
        Task<NewTaskResult> AddNewTask(CreateTaskRequest createTaskRequest);
        Task<string> AddNewProject(ProjectRequest projectRequest);
        Task<NewTaskResult> CompleteATask(string taskId, ExpandoObject status);

        // Application Tracking
        Task<ApplicationLogResult> AddUsedApplicationLog(ApplicationLog applicationLog);

        // Live Screen
        Task sendLiveScreenData(LiveImageRequest timeLog);
        Task sendLiveScreenDataV1(LiveImageRequest timeLog);
        Task<CheckLiveScreenResponse> checkLiveScreen(TaskUser user);

        // Productivity Applications
        Task<ProductivityAppResult> GetProductivityApps(string url);
        Task<ProductivityAppAddResult> AddProductivityApps(string url, ProductivityModel productivityModel);
        Task<ProductivityAppDeleteResult> DeleteProductivityApp(string id);
        Task<object> AddBrowserHistory(HistoryEntry historyEntry);

        // User Preferences
        Task<EnableBeepSoundResult> GetEnableBeepSoundSetting(string url);
        Task<UserPreferenceResult> GetUserPreferencesSetting(string url);
        Task<BaseResponse> SetUserPreferences(string url, CreateUserPreferenceRequest createUserPreferenceRequest);
        Task<Project> GetUserPreferencesSettingByKey(string url);

        // User Status
        Task<UpdateOnlineStatusResult> UpdateOnlineStatus(string userId, string machineId, bool isOnline, string project, string task);
    }
}
