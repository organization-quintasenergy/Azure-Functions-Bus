using AFBus.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public class SETTINGS
    {
        public const string AZURE_STORAGE = "AzureWebJobsStorage";
        public const string LOCKSAGAS = "LockSagas";
    }

    public class SettingsUtil
    {
        public static T GetSettings<T>(string settingName) where T : IConvertible
        { 
            if (System.Environment.GetEnvironmentVariable(settingName)!= null)
                return (T)Convert.ChangeType(System.Environment.GetEnvironmentVariable(settingName), typeof(T)); 
            else
                return (T)Convert.ChangeType(Settings.Default[settingName], typeof(T));
        }
    }
}
