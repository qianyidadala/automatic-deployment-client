using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerMachineInfoShower_Client
{
    public class SocketUtil
    {
        public static string SocketMD5(string ip,int port,string sendMessage)
        {
            //建立Socket(TCP/IP)连接并且获得服务器端响应的json数据
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddr = IPAddress.Parse(ip);
            IPEndPoint ipe = new IPEndPoint(ipAddr, port);
            socket.Connect(ipe);
            //发送数据
            byte[] requestData = Encoding.UTF8.GetBytes(sendMessage);
            socket.Send(requestData);

            byte[] data = new byte[1024 * 1024 * 5];
            int idx = 0;
            int totalLen = data.Length;
            int readLen = 0;
            while (idx < totalLen)
            {
                readLen = socket.Receive(data, idx, totalLen - idx, SocketFlags.None);
                if (readLen > 0)
                {
                    idx = idx + readLen;
                }
                else
                {
                    break;
                }
            }
            string response = Encoding.UTF8.GetString(data);
            return response;
        }


        public static string socket(string ip, int port, string sendMessage)
        {
            //建立Socket(TCP/IP)连接并且获得服务器端响应的json数据
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddr = IPAddress.Parse(ip);
            IPEndPoint ipe = new IPEndPoint(ipAddr, port);
            socket.Connect(ipe);
            //发送数据
            byte[] requestData = Encoding.UTF8.GetBytes(sendMessage);
            socket.Send(requestData);
            byte[] responseData = new byte[1024 * 1024 * 5];
            int receiveCount = socket.Receive(responseData);
            string response = Encoding.UTF8.GetString(responseData, 0, receiveCount);
            return response;
        }


        public static string UploadFile(string ip,int port,byte[] data)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddr = IPAddress.Parse(ip);
            IPEndPoint ipe = new IPEndPoint(ipAddr, port);
            socket.Connect(ipe);
            socket.Send(data);


            int idx = 0;
            int totalLen = data.Length;
            int readLen = 0;
            while (idx < totalLen)
            {
                readLen = socket.Send(data, idx, totalLen - idx, SocketFlags.None);
                if (readLen > 0)
                {
                    idx = idx + readLen;
                }
                else
                {
                    break;
                }
            }


            byte[] responseData = new byte[1024];
            int receiveCount = socket.Receive(responseData);
            string response = Encoding.UTF8.GetString(responseData, 0, receiveCount);
            return response;
        }
    }
}