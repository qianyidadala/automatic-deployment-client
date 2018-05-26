using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMachineInfoShower_Client
{
    public static class ClientCache
    {
        
        private static string CurrentServerIP;
        private static string CurrentServerPort;
        private static string InterflowPort;

        private static string ClientProjectDir;

        /// <summary>
        /// 设置客户拖入的对比项目的根路径
        /// </summary>
        /// <param name="clientProjectDir"></param>
        public static void SetClientProjectDir(string clientProjectDir)
        {
            ClientProjectDir = clientProjectDir;
        }

        /// <summary>
        /// 获取客户拖入的对比项目的根路径
        /// </summary>
        /// <returns></returns>
        public static string GetClientProjectDir()
        {
            return ClientProjectDir;
        }

        /// <summary>
        /// 设置通信专用端口号,此端口号需要用户在客户端额外配置
        /// </summary>
        /// <param name="interflowPort"></param>
        public static void SetInterflowPort(string interflowPort)
        {
            InterflowPort = interflowPort;
        }

        /// <summary>
        /// 获取通信专用端口号,此端口需要用户在客户端额外配置
        /// </summary>
        /// <returns></returns>
        public static string GetInterflowPort()
        {
            return InterflowPort;
        }

        /// <summary>
        /// 当前选择的服务器端口
        /// </summary>
        public static string GetCurrentServerPort()
        {
            return CurrentServerPort;
        }

        /// <summary>
        /// 当前选择的服务器IP
        /// </summary>
        public static string GetCurrentServerIP()
        {
            return CurrentServerIP;
        }

        /// <summary>
        /// 将当前操作的服务器IP加入到缓存,后续的Socket请求都是基于此IP
        /// </summary>
        /// <param name="currentServerIP"></param>
        public static void SetCurrentServerIP(string currentServerIP)
        {
            CurrentServerIP = currentServerIP;
        }

        /// <summary>
        /// 将当前操作的服务器端口加入到缓存,后续的Socket请求都是基于此端口
        /// </summary>
        /// <param name="currentServerPort"></param>
        public static void SetCurrentServerPort(string currentServerPort)
        {
            CurrentServerPort = currentServerPort;
        }
    }
}
