using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace UtilMonitor.Non_WPF_Code
{
   public static class configReadWriter
    {
        //Config reader
        public static string readConfig(string targetKey)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                return appSettings.Get(targetKey);
            }
            catch {
                return "Error reading file";
                    }
        }
        public static string addUpdateKey(string targetKey, string newVal)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings[targetKey] == null)
            {
                settings.Add(targetKey, newVal);
            }
                else
            {
                settings[targetKey].Value = newVal;
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            return "Setting updated";
        }
    }
}
