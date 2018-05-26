using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ServerMachineInfoShower_Client
{
    public class CommonUtil
    {

        static List<string> RandomStrPools = new List<string>();

        public static string GetRamdomStr()
        {
            string id = System.Guid.NewGuid().ToString("N");
            id = id.Substring(0,5).ToUpper();
            while (RandomStrPools.Contains(id))
            {
                id = System.Guid.NewGuid().ToString("N");
                id = id.Substring(0, 5).ToUpper();
            }
            RandomStrPools.Add(id);
            return id;
        }

        /// <summary>
        /// 生成随机请求ID
        /// </summary>
        /// <returns></returns>
        public static long GetRandomRequiredRequestID()
        {
            return DateTime.Now.Ticks;
        }

        /// <summary>
        /// 将Json字符串转换成为List集合,请确保传入的json本身即是集合格式
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<string> ParseJsonToList(String json)
        {
            JObject jobj = JObject.Parse(json);
            JArray jarray = JArray.Parse(jobj["logs"].ToString());

            List<string> list = new List<string>();
            for (int i = 0; i < jarray.Count; i++)
            {
                list.Add(jarray[i]+"");
            }
            return list;
        }

        public static Dictionary<String,String> ParseJsonToMap(String json)
        {
            Newtonsoft.Json.Linq.JObject jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            Dictionary<String, String> map = new Dictionary<string, string>();
            map.Add("FTPCount", jsonObj["FTPCount"] == null ? "" : jsonObj["FTPCount"].ToString());//FTP一共检验的次数
            map.Add("FTPSuccess", jsonObj["FTPSuccess"] == null ? "" : jsonObj["FTPSuccess"].ToString());//FTP正确的次数
            map.Add("FTPIdentityMill", jsonObj["FTPIdentityMill"] == null ? "" : jsonObj["FTPIdentityMill"].ToString());//记录监测此项功能的毫秒数

            map.Add("MySQLCount", jsonObj["MySQLCount"] == null ? "" : jsonObj["MySQLCount"].ToString());//MySQL一共检验的次数
            map.Add("MySQLSuccess", jsonObj["MySQLSuccess"] == null ? "" : jsonObj["MySQLSuccess"].ToString());//MySQL正确的次数
            map.Add("MySQLIdentityMill", jsonObj["MySQLIdentityMill"] == null ? "" : jsonObj["MySQLIdentityMill"].ToString());//记录监测此项功能的毫秒数

            map.Add("OtherCount", jsonObj["OtherCount"] == null ? "" : jsonObj["OtherCount"].ToString());//Other一共检验的次数
            map.Add("OtherSuccess", jsonObj["OtherSuccess"] == null ? "" : jsonObj["OtherSuccess"].ToString());//Other正确的次数
            map.Add("OtherIdentityMill", jsonObj["OtherIdentityMill"] == null ? "" : jsonObj["OtherIdentityMill"].ToString());//记录监测此项功能的毫秒数

            map.Add("RedisCount", jsonObj["RedisCount"] == null ? "" : jsonObj["RedisCount"].ToString());//Redis一共检验的次数
            map.Add("RedisSuccess", jsonObj["RedisSuccess"] == null ? "" : jsonObj["RedisSuccess"].ToString());//Redis正确的次数
            map.Add("RedisIdentityMill", jsonObj["RedisIdentityMill"] == null ? "" : jsonObj["RedisIdentityMill"].ToString());//记录监测此项功能的毫秒数

            map.Add("ZBusCount", jsonObj["ZBusCount"] == null ? "" : jsonObj["ZBusCount"].ToString());//ZBus一共检验的次数
            map.Add("ZBusSuccess", jsonObj["ZBusSuccess"] == null ? "" : jsonObj["ZBusSuccess"].ToString());//ZBus正确的次数
            map.Add("ZBusIdentityMill", jsonObj["ZBusIdentityMill"] == null ? "" : jsonObj["ZBusIdentityMill"].ToString());//记录监测此项功能的毫秒数

            map.Add("CheckedProductCount", jsonObj["CheckedProductCount"] == null ? "" : jsonObj["CheckedProductCount"].ToString()); //系统中项目数量
            return map;
        }

        /// <summary>
        /// 传入一个文件名以检测此文件是否存在
        /// 路径为当前进程
        /// </summary>
        /// <returns></returns>
        public static bool ExistsFile(String filename)
        {

            string path = GetProgressPath();
            return File.Exists(path + "\\" + filename);

        }

        /// <summary>
        /// 传入文件名.在当前进程同级目录下创建一个文件,请确保传入的filename包含文件后缀(不包含也没事)
        /// </summary>
        public static void CreateNewFile(String filename)
        {
            string path = GetProgressPath();
            //此处必须调用Close方法,否则遇到调用此方法创建文件之后若立即进行写入则会存在文件被另一进程占用问题
            File.Create(path + "\\" + GobalConst.ServerConfigFileName).Close();
        }

        /// <summary>
        /// 写入一行内容到文件,每一次写之前都会先加入一个换行符.
        /// 传入的path请确保是完整的文件路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="line"></param>
        public static void AppendToFile(String path,String line)
        {
            using (FileStream fs = new FileStream(path, FileMode.Append))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(System.Environment.NewLine + line);
                fs.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// 将一个文件内的内容按行读取并且读入List集合中
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<String> ReadFileToListOfLine(String path)
        {
            List<String> resultList = new List<String>();
            using (FileStream fs = new FileStream(path,FileMode.OpenOrCreate))
            {
                StreamReader reader = new StreamReader(fs, Encoding.UTF8);
                String line;
                while((line = reader.ReadLine()) != null)
                {
                    resultList.Add(line);
                }
            }
            //剔除空行
            for (int i = 0; i < resultList.Count; i++)
            {
                if (resultList[i] == null || resultList[i] == "")
                {
                    resultList.RemoveAt(i);
                }
            }
            return resultList;
        }

        /// <summary>
        /// 清空一个文件里的内容
        /// </summary>
        /// <param name="path"></param>
        public static void ClearFileContent(String path)
        {
            if (File.Exists(path))
            {
                //创建或覆盖文件
                File.Create(path);
            }
        }

        /// <summary>
        /// 获得当前可执行程序的路径,不包含线程(程序名) 不包含最后的双斜杠
        /// </summary>
        /// <returns></returns>
        public static String GetProgressPath()
        {
            string path = Application.ExecutablePath;
            return path.Substring(0, path.LastIndexOf('\\'));
        }

        /// <summary>
        /// 计算评分
        /// </summary>
        /// <param name="successCount"></param>
        /// <param name="failedCount"></param>
        /// <returns></returns>
        public static int CalcScore(int sumCount, int failedCount)
        {
            if(sumCount == 0)
            {
                return 100;
            }
            if(failedCount == 0)
            {
                return 100;
            }
            if (failedCount >= sumCount)
            {
                return 0;
            }
            double score = failedCount / sumCount;

            return (int)(score * (double)100);
        }
    }
}