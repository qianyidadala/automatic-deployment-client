using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ServerMachineInfoShower_Client
{
    public class ConfigUtil
    {

        public string GetString(string key,string defaultValue)
        {
            foreach (string item in ConfigurationManager.AppSettings)
            {
                if (key.Equals(item))
                {
                    string value = ConfigurationManager.AppSettings[item];
                    if(value == null)
                    {
                        return defaultValue;
                    }
                    else
                    {
                        return value;
                    }
                }
            }
            return defaultValue;
        }
    }
}
