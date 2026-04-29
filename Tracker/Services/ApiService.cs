using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Models;
using TimeTracker.Services.Interfaces;

namespace TimeTracker.Services
{
    public interface IApiService
    {
        Task<bool> GetEnableBeepSoundSetting();
        Task<UserPreferenceResult> GetUserPreferencesSetting();
        Task<BaseResponse> SetUserPreferences(CreateUserPreferenceRequest createUserPreferenceRequest);
        Task<Project> GetUserPreferencesByKey();
    }

    public class APIService : IApiService
    {
        private readonly IRestService _restService;

        public APIService(IRestService restService)
        {
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        public async Task<bool> GetEnableBeepSoundSetting()
        {
            var result = await _restService.GetEnableBeepSoundSetting(
                $"api/v1/userPreferences/getUserPreferenceByKey/TimeTracker.BlurScreenshots"
            );
            return result?.data ?? false;
        }

        public async Task<UserPreferenceResult> GetUserPreferencesSetting()
        {
            var result = await _restService.GetUserPreferencesSetting(
                $"api/v1/userPreferences/user/{GlobalSetting.Instance.LoginResult.data.user.id}"
            );
            return result;
        }

        public async Task<BaseResponse> SetUserPreferences(
            CreateUserPreferenceRequest createUserPreferenceRequest
        )
        {
            var result = await _restService.SetUserPreferences(
                $"api/v1/userPreferences/create",
                createUserPreferenceRequest
            );
            return result;
        }

        public async Task<Project> GetUserPreferencesByKey()
        {
            var result = await _restService.GetUserPreferencesSettingByKey(
                $"api/v1/userPreferences/preference-key/{GlobalSetting.Instance.userPreferenceKey.TrackerSelectedProject}?userId={GlobalSetting.Instance.LoginResult.data.user.id}"
            );
            return result;
        }
    }
}
