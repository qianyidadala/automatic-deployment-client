using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerMachineInfoShower_Client
{
    public class MD5Util
    {
        /// <summary>  
        /// 获取文件的MD5码  
        /// </summary>  
        /// <param name="fileName">传入的文件名（含路径及后缀名）</param>  
        /// <returns></returns>  
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder buffer = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    buffer.Append(retVal[i].ToString("x2"));
                }
                return buffer.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}
