using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace ServerMachineInfoShower_Client
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 界面加载,初始化一些资源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_Load(object sender, EventArgs e)
        {
            ClientCache.SetCurrentServerIP("127.0.0.1");
            ClientCache.SetCurrentServerPort("60001");
            ClientCache.SetInterflowPort("60002");
            //读取配置文件中记录的端口信息等
            //ConfigUtil config = new ConfigUtil();
            //ClientCache.SetCurrentServerPort(config.GetString("MainConfigPort", string.Empty));
            //ClientCache.SetInterflowPort(config.GetString("InterflowPort", string.Empty));

            //窗体默认打开全屏
            this.WindowState = FormWindowState.Maximized;


            //加载左侧树状服务器列表信息
            initServerTree();


            //添加控制台版权信息
            appendDebug("Copyright 2018 ICE FROG SOFTWARE All rights reserved.", false);
        }

        /// <summary>
        /// 建立Socket连接并获取到服务器响应的json数据,解析之后添加到"控制台中作为展示"
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void SocketRunner(String ip, int port)
        {
            //建立Socket(TCP/IP)连接并且获得服务器端响应的json数据
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddr = IPAddress.Parse(ip);
            IPEndPoint ipe = new IPEndPoint(ipAddr, port);
            socket.Connect(ipe);
            byte[] data = new byte[1024 * 1024];
            socket.Receive(data);
            string response = Encoding.UTF8.GetString(data);
            List<string> jsonList = CommonUtil.ParseJsonToList(response);
            Dictionary<String, String> map = CommonUtil.ParseJsonToMap(response);

            //将每一个产品的检测详情添加到页面上
            AddProductInfo(map);


            //将响应的日志数据拼接到控制台
            for (int i = 0; i < jsonList.Count; i++)
            {
                //判断依据为可能出现频率最高的排在最前面判断,可以减少CPU少走几次if
                if (jsonList[i].StartsWith("[DEBUG]"))
                {
                    appendDebug(jsonList[i], true);
                    continue;
                }
                else if (jsonList[i].StartsWith("[INFO]"))
                {
                    appendInfo(jsonList[i], true);
                    continue;
                }
                else if (jsonList[i].StartsWith("[WARN]"))
                {
                    appendWarn(jsonList[i], true);
                    continue;
                }
                else if (jsonList[i].StartsWith("[ERROR]"))
                {
                    appendError(jsonList[i], true);
                    continue;
                }
                else if (jsonList[i].StartsWith("[FATAL]"))
                {
                    appendFatal(jsonList[i], true);
                    continue;
                }
                else
                {
                    //默认使用debug级别输出
                    appendDebug(jsonList[i], true);
                }
            }
        }

        /// <summary>
        /// 显示redis信息
        /// </summary>
        /// <param name="map"></param>
        private void AddRedisInfo(Dictionary<String, String> map)
        {
            //设置Redis信息
            int redisCount = Convert.ToInt32(map["RedisCount"] == null || map["RedisCount"] == "" ? "0" : map["RedisCount"].ToString());
            int redisSuccess = Convert.ToInt32(map["RedisSuccess"] == null || map["RedisSuccess"] == "" ? "0" : map["RedisSuccess"].ToString());
            int redisFailedCount = redisCount - redisSuccess;
            int score = CommonUtil.CalcScore(redisCount, redisFailedCount);
            lb_redisCheckCount.Text = redisCount + "";       //redis总检测次数
            lb_redisFailedCount.Text = redisFailedCount + "";      //redis检测错误次数
            if (redisFailedCount > 0)
            {
                lb_redisFailedCount.ForeColor = Color.Red;
            }
            else
            {
                lb_redisFailedCount.ForeColor = Color.Teal;
            }

            if (score <= 50)
            {
                lb_redisScore.ForeColor = Color.Red;
            }
            else
            {
                lb_redisScore.ForeColor = Color.Teal;
            }
            lb_redisScore.Text = score + "";            //redis评分
            lb_redisSuccessCount.Text = redisSuccess + "";     //redis检测成功次数
            lb_redisSumMillsTime.Text = map["RedisIdentityMill"] + "ms";//redis检测总时间(ms)
            lb_redisVersion.Text = "unknow";          //redis版本
        }

        /// <summary>
        /// 显示MySQL信息
        /// </summary>
        /// <param name="map"></param>
        private void AddMySQLInfo(Dictionary<String, String> map)
        {
            //设置MySQL信息
            int mysqlCount = Convert.ToInt32(map["MySQLCount"] == null || map["MySQLCount"] == "" ? "0" : map["MySQLCount"].ToString());
            int mysqlSuccess = Convert.ToInt32(map["MySQLSuccess"] == null || map["MySQLSuccess"] == "" ? "0" : map["MySQLSuccess"].ToString()); ;
            int mysqlFailed = mysqlCount - mysqlSuccess;
            int mysqlScore = CommonUtil.CalcScore(mysqlCount, mysqlFailed);
            lb_mysqlCount.Text = mysqlCount + "";
            lb_mysqlFailed.Text = mysqlFailed + "";
            if (mysqlFailed > 0)
            {
                lb_mysqlFailed.ForeColor = Color.Red;
            }
            else
            {
                lb_mysqlFailed.ForeColor = Color.Teal;
            }
            if (mysqlScore <= 50)
            {
                lb_mysqlScore.ForeColor = Color.Red;
            }
            else
            {
                lb_mysqlScore.ForeColor = Color.Teal;
            }
            lb_mysqlScore.Text = mysqlScore + "";
            lb_mysqlSuccess.Text = mysqlSuccess + "";
            lb_mysqlSumMills.Text = map["MySQLIdentityMill"] + "ms";
            lb_mysqlVersion.Text = "unknow";
        }

        /// <summary>
        /// 显示ZBus信息
        /// </summary>
        /// <param name="map"></param>
        private void AddZbusInfo(Dictionary<String, String> map)
        {
            //设置ZBus信息
            int zbusCount = Convert.ToInt32(map["ZBusCount"] == null || map["ZBusCount"] == "" ? "0" : map["ZBusCount"].ToString());
            //MessageBox.Show(map["ZBusSuccess"]);
            int zbusSuccess = Convert.ToInt32(map["ZBusSuccess"] == null || map["ZBusSuccess"] == "" ? "0" : map["ZBusSuccess"].ToString());
            int zbusFailed = zbusCount - zbusSuccess;
            int zbusScore = CommonUtil.CalcScore(zbusCount, zbusFailed);
            lb_zbusCount.Text = zbusCount + "";
            lb_zbusFailed.Text = zbusFailed + "";
            lb_zbusMills.Text = map["ZBusIdentityMill"] + "ms";
            if (zbusFailed > 0)
            {
                lb_zbusFailed.ForeColor = Color.Red;
            }
            else
            {
                lb_zbusFailed.ForeColor = Color.Teal;
            }
            if (zbusScore <= 50)
            {
                lb_zbusScore.ForeColor = Color.Red;
            }
            else
            {
                lb_zbusScore.ForeColor = Color.Teal;
            }
            lb_zbusScore.Text = zbusScore + "";
            lb_zbusSuccess.Text = zbusSuccess + "";
            lb_zbusVersion.Text = "unknow";
        }

        /// <summary>
        /// 显示FTP信息
        /// </summary>
        /// <param name="map"></param>
        private void AddFTPInfo(Dictionary<String, String> map)
        {
            //设置FTP信息
            int ftpCount = Convert.ToInt32(map["FTPCount"] == null || map["FTPCount"] == "" ? "0" : map["FTPCount"].ToString());
            int ftpSuccess = Convert.ToInt32(map["FTPSuccess"] == null || map["FTPSuccess"] == "" ? "0" : map["FTPSuccess"].ToString());
            int ftpFailed = ftpCount - ftpSuccess;
            int ftpScore = CommonUtil.CalcScore(ftpCount, ftpFailed);
            if (ftpFailed > 0)
            {
                lb_ftpFailed.ForeColor = Color.Red;
            }
            else
            {
                lb_ftpFailed.ForeColor = Color.Teal;
            }
            if (ftpScore <= 50)
            {
                lb_ftpScore.ForeColor = Color.Red;
            }
            else
            {
                lb_ftpScore.ForeColor = Color.Teal;
            }
            lb_ftpCount.Text = ftpCount + "";
            lb_ftpFailed.Text = ftpFailed + "";
            lb_ftpMills.Text = map["FTPIdentityMill"] + "ms";
            lb_ftpScore.Text = ftpScore + "";
            lb_ftpSuccess.Text = ftpSuccess + "";
            lb_ftpVersion.Text = "unknow";
        }

        /// <summary>
        /// 显示其他信息
        /// </summary>
        /// <param name="map"></param>
        private void AddOtherInfo(Dictionary<String, String> map)
        {
            //设置Other信息
            int otherCount = Convert.ToInt32(map["OtherCount"] == null || map["OtherCount"] == "" ? "0" : map["OtherCount"].ToString());
            int otherSuccess = Convert.ToInt32(map["OtherSuccess"] == null || map["OtherSuccess"] == "" ? "0" : map["OtherSuccess"].ToString()); ;
            int otherFailed = otherCount - otherSuccess;
            int otherScore = CommonUtil.CalcScore(otherCount, otherFailed);
            if (otherFailed > 0)
            {
                lb_otherFailed.ForeColor = Color.Red;
            }
            else
            {
                lb_otherFailed.ForeColor = Color.Teal;
            }
            if (otherScore <= 50)
            {
                lb_otherScore.ForeColor = Color.Red;
            }
            else
            {
                lb_otherScore.ForeColor = Color.Teal;
            }
            lb_otherCount.Text = otherCount + "";
            lb_otherFailed.Text = otherFailed + "";
            lb_otherMills.Text = map["OtherIdentityMill"] + "ms";
            lb_otherScore.Text = otherScore + "";
            lb_otherSuccess.Text = otherSuccess + "";
            lb_otherVersion.Text = "unknow";
        }


        /// <summary>
        /// 按照服务器规则取出检测过程中积累的所有详细信息,并体现在页面对应的各个模块下
        /// </summary>
        /// <param name="map"></param>
        private void AddProductInfo(Dictionary<String, String> map)
        {
            AddRedisInfo(map);
            AddMySQLInfo(map);
            AddZbusInfo(map);
            AddFTPInfo(map);
            AddOtherInfo(map);
        }

        private void 添加服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddServerMachine serverMachine = new AddServerMachine();
            serverMachine.ShowDialog();
        }

        /// <summary>
        /// 加载左侧服务器信息列表,每次调用此方法都会先清空一次
        /// </summary>
        public void initServerTree()
        {
            tree_serverinfo.Nodes.Clear();
            string path = CommonUtil.GetProgressPath();
            List<String> serverinfo = CommonUtil.ReadFileToListOfLine(path + "\\" + GobalConst.ServerConfigFileName);

            //添加根节点
            TreeNode node = new TreeNode
            {
                Text = "@ICE FROG SOFTWARE",
                ImageIndex = 0,
                ToolTipText = "ICE FROG 软件"
            };
            tree_serverinfo.Nodes.Add(node);
            int serverCount = 0;
            foreach (String item in serverinfo)
            {
                //解析本行数据
                String[] strItem = item.Split('$');
                if (strItem == null || strItem.Length < 3)
                {
                    continue;
                }
                serverCount++;
                TreeNode childNote = new TreeNode
                {
                    Text = strItem[0], //别名
                    ToolTipText = strItem[1] + ":" + strItem[2] + " (正在检查网络...)",  //设置悬浮提示文本,显示IP加端口
                    Tag = strItem[1] + ":" + strItem[2], //隐藏Tag设置为 "ip:端口号"
                    ImageIndex = 3 //网络正常
                };
                node.Nodes.Add(childNote);
            }
            //展开所有树节点
            ExpandAll();

            //将服务器数量添加到页面底部状态栏
            toolStripServerCount.Text = serverCount + "台";

            //将当前时间添加到底部状态栏
            toolStripCurrentDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
        }

        /// <summary>
        /// 刷新树菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            initServerTree();
            //在"控制台"中显示一行刷新日志
            appendDebug("刷新服务器列表成功!", true);
        }

        /// <summary>
        /// 展开树菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 全部展开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExpandAll();
        }

        /// <summary>
        /// 合并树菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 全部合并ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CollapseAll();
        }

        /// <summary>
        /// 折叠树菜单所有节点
        /// </summary>
        private void CollapseAll()
        {
            tree_serverinfo.CollapseAll();
        }

        /// <summary>
        /// 展开所有树节点
        /// </summary>
        private void ExpandAll()
        {
            tree_serverinfo.ExpandAll();
        }

        private void 清空控制台ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rt_console.Clear();
        }

        /// <summary>
        /// 检查网络是否能ping通
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private bool IdentityNetwork(String ip)
        {
            try
            {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                System.Net.NetworkInformation.PingReply reply = ping.Send(ip);
                if (reply.Address == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 用户双击节点事执行事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tree_serverinfo_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                TreeNode selectedNode = tree_serverinfo.SelectedNode;
                if (selectedNode != null)
                {
                    //如果不是顶级节点,则开始操作
                    if (selectedNode.Parent != null)
                    {
                        //获得ip地址和端口信息
                        String tag = selectedNode.Tag.ToString();
                        String[] tempArr = tag.Split(':');
                        String ip = tempArr[0];
                        String port = tempArr[1];
                        //发起Socket请求
                        SocketRunner(ip, Convert.ToInt32(port));
                        toolStripStatusLabel6.Text = ip+":"+ port;
                        //将选择的服务器信息加到缓存中. 后续的Socket请求都会基于此ip和端口
                        ClientCache.SetCurrentServerIP(ip);
                        ClientCache.SetCurrentServerPort(port);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>
        /// 清空所有服务器配置,此方法调用会重启进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 清空所有服务器配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //清空配置文件内容
                CommonUtil.ClearFileContent(CommonUtil.GetProgressPath() + "//" + GobalConst.ServerConfigFileName);

                appendDebug("清空服务器配置内容成功!", true);

                //重启进程
                Application.Restart();
            }
            catch (Exception ex)
            {
                appendError("清空服务器配置内容失败," + ex.Message, true);
            }
        }


        /// <summary>
        /// 追加Debug级别的信息到“控制台中”
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newLine"></param>
        private void appendDebug(String content, bool newLine)
        {
            append(content, newLine, Color.Black);
        }

        /// <summary>
        /// 追加Info级别的信息到“控制台中”
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newLine"></param>
        private void appendInfo(String content, bool newLine)
        {
            append(content, newLine, Color.Blue);
        }
        /// <summary>
        /// 追加Warn级别的信息到“控制台中”
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newLine"></param>
        private void appendWarn(String content, bool newLine)
        {
            append(content, newLine, Color.Yellow);
        }
        /// <summary>
        /// 追加Error级别的信息到“控制台中”
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newLine"></param>
        private void appendError(String content, bool newLine)
        {
            append(content, newLine, Color.Red);
        }
        /// <summary>
        /// 追加Fatal级别的信息到“控制台中”
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newLine"></param>
        private void appendFatal(String content, bool newLine)
        {
            append(content, newLine, Color.Red);
        }

        /// <summary>
        /// 追加一段内容到"控制台"中
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newLine"></param>
        /// <param name="color"></param>
        private void append(String content, bool newLine, Color color)
        {
            rt_console.SelectionColor = color;
            if (newLine)
            {
                rt_console.AppendText(System.Environment.NewLine + content);
            }
            else
            {
                rt_console.AppendText(content);
            }
            //rt_console.Clear();
        }

        private void 复制到剪切板ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(rt_console.Text);
        }

        /// <summary>
        /// "控制台"内容改变事件,目前用于让滚动条一直滚动在最下方
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rt_console_TextChanged(object sender, EventArgs e)
        {
            rt_console.SelectionStart = rt_console.Text.Length;
            rt_console.ScrollToCaret();
        }

        private void 项目校验ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //检测客户端缓存中是否有服务器IP和端口
            if(ClientCache.GetCurrentServerIP() == null)
            {
                MessageBox.Show("请先配置并运行左侧树菜单服务器信息再使用本功能");
                return;
            }
            Comparison comparison = new Comparison();
            comparison.Show();
        }

        private void 配置通信端口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClientCache.SetInterflowPort("60002");
        }
    }
}
