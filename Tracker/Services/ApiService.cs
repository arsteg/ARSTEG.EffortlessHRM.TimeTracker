using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public static class APIService
    {
        public static async Task<bool> GetEnableBeepSoundSetting()
        {
            var rest = new REST(new HttpProviders());
            var result = await rest.GetEnableBeepSoundSetting($"api/v1/userPreferences/getUserPreferenceByKey/TimeTracker.BlurScreenshots");
            return result.data;             
        }

        public static async Task<UserPreferenceResult> GetUserPreferencesSetting()
        {
            var rest = new REST(new HttpProviders());
            var result = await rest.GetUserPreferencesSetting($"api/v1/userPreferences/user/{GlobalSetting.Instance.LoginResult.data.user.id}");
            return result;
        }

        public static async Task<BaseResponse> SetUserPreferences(CreateUserPreferenceRequest createUserPreferenceRequest)
        {
            var rest = new REST(new HttpProviders());
            var result = await rest.SetUserPreferences($"api/v1/userPreferences/create", createUserPreferenceRequest);
            return result;
        }

        public static async Task<Project> GetUserPreferencesByKey()
        {
            var rest = new REST(new HttpProviders());
            var result = await rest.GetUserPreferencesSettingByKey($"api/v1/userPreferences/preference-key/{GlobalSetting.Instance.userPreferenceKey.TrackerSelectedProject}?userId={GlobalSetting.Instance.LoginResult.data.user.id}");
            return result;
        }
    }
}
