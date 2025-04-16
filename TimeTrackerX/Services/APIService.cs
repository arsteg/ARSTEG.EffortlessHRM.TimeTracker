using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrackerX.Models;

namespace TimeTrackerX.Services
{
    public static class APIService
    {
        public static async Task<bool> GetEnableBeepSoundSetting()
        {
            var rest = new REST(new HttpProviders());
            var result = await rest.GetEnableBeepSoundSetting(
                $"api/v1/userPreferences/getUserPreferenceByKey/TimeTracker.BlurScreenshots"
            );
            return result.data;
        }
    }
}
