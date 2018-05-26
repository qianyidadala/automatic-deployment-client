using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ServerMachineInfoShower_Client
{
    public class JSONUtil
    {

        private string json;
        private JObject jObject;

        public JSONUtil(string json)
        {
            this.json = json;
            this.jObject = JObject.Parse(json);
        }

        /// <summary>
        /// 报文:客户端请求服务端指定项目的MD5值列表
        /// </summary>
        private const string GetServerMD5 = "0ea4ee48dd0258028cf7374de691b3b5";


        public static List<Dictionary<string, string>> ParseServerMD5Info(string json)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Console.WriteLine(json);
            JObject jObject =  JObject.Parse(json);
            JArray array = JArray.Parse(jObject["message"].ToString());
            foreach (JToken item in array)
            {
                Dictionary<string, string> map = new Dictionary<string, string>();
                map.Add("url", item["url"].ToString());
                map.Add("md5", item["md5"].ToString());
                map.Add("size", item["size"].ToString());
                list.Add(map);
            }
            return list;
        }


        public static string RequestServerMD5InfoJsonStr(string serverName)
        {
            string json = "{";
            json += "\"requestid\":" + "\"" + Convert.ToString(CommonUtil.GetRandomRequiredRequestID()) + "\",";
            json += "\"message\":" + "\"nonmessage\",";
            json += "\"removeServerName\":" + "\"" + serverName + "\",";
            json += "\"headingcode\":" + "\"" + GetServerMD5 + "\"}";
            return json;
        }

        public static string GenerateMD5JsonStr(List<Dictionary<string, string>> list,string removeServerName)
        {
            string json = "{";
            json += "\"requestid\":"+ "\""+ Convert.ToString(CommonUtil.GetRandomRequiredRequestID()) + "\",";
            json += "\"message\":" + "\"nonmessage\",";
            json += "\"removeServerName\":" + "\""+ removeServerName + "\",";
            json += "\"headingcode\":" + "\"a62e33be4197be8440f048da761755f7\",";
            json += "\"clientmd5s\":" + "[";
            foreach (Dictionary<string, string> item in list)
            {
                json += "{";
                int innerLoop = 0;
                foreach (KeyValuePair<string, string> pair in item)
                {
                    innerLoop++;
                    json += "\""+pair.Key+"\":"+"\""+pair.Value+"\"";
                    if (innerLoop < 2)
                        json += ",";
                }
                json += "},";
            }
            json = json.Substring(0,json.LastIndexOf(","));
            json += "]";
            json += "}";
            return json;
        }

        public object GetObj(string key)
        {
            return this.jObject[key];
        }

        public JArray GetArray(string key)
        {
            return JArray.Parse(jObject[key].ToString());
        }

        public JObject GetJObject()
        {
            return this.jObject;
        }

        public void ReInit(string json)
        {
            this.json = json;
            this.jObject = JObject.Parse(json);
        }
    }
}
