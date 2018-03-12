using AFBus.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    internal class SETTINGS
    {
        internal const string AZURE_STORAGE = "AzureWebJobsStorage";
        internal const string LOCKSAGAS = "LockSagas";
    }

    internal class SettingsUtil
    {
        internal static T GetSettings<T>(string settingName) where T : IConvertible
        { 
            if (ConfigurationManager.AppSettings[settingName] != null)
                return (T)Convert.ChangeType(ConfigurationManager.AppSettings[settingName], typeof(T)); 
            else
                return (T)Convert.ChangeType(Settings.Default[settingName], typeof(T));
        }
    }
}
