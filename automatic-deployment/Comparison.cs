using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.CheckedListBox;

namespace ServerMachineInfoShower_Client
{
    public partial class Comparison : Form
    {
        public Comparison()
        {
            InitializeComponent();
        }

        private void Comparison_Load(object sender, EventArgs e)
        {
            try
            {
                //初始化远程服务器项目列表到下拉列表
                string json = "{\"requestid\":100011111,\"message\":\"这是客户端传入的message\",\"headingcode\":\"b1bb0e5951a6a8f3ff7524cd0ad92ff1\"}";
                string response = SocketUtil.socket(ClientCache.GetCurrentServerIP(), Convert.ToInt32(ClientCache.GetInterflowPort()), json);
                JSONUtil jsonUtil = new JSONUtil(response);
                JObject jObject = jsonUtil.GetJObject();
                JArray array = (JArray)jObject["data"]["projects"];
                cb_remoteServer.Items.Add("-- 请选择服务器项目 --");
                for (int i = 0; i < array.Count; i++)
                {
                    cb_remoteServer.Items.Add(array[i]["name"].ToString());
                }
                cb_remoteServer.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                MessageBox.Show("获取服务器端在线列表出现错误"+ex.Message,"在线获取", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "请选择项目的根路径,避免规则不一致导致匹配失败!";
            DialogResult result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                MessageBox.Show("");
            }
        }

        private void listBox1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            listBox1.Tag = files[0];
            ClientCache.SetClientProjectDir(files[0]);
            foreach (string item in files)
            {
                FileInfo fileinfo = new FileInfo(item);
                string stuffix = fileinfo.Extension;
                if (stuffix == null || stuffix.Equals(string.Empty) || stuffix.Trim().Length == 0)
                {
                    DirectoryInfo dinfo = new DirectoryInfo(item);
                    bool exists = dinfo.Exists;
                    if (exists)
                    {
                        //Calls2CallsForSelf(item);
                        FileInfo[] fs = dinfo.GetFiles("*", SearchOption.AllDirectories);
                        foreach (FileInfo f in fs)
                        {
                            listBox1.Items.Add(f.FullName);
                        }
                        continue;
                    }
                }
                listBox1.Items.Add(item);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        /// <summary>
        /// MD5比对
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            ControlUtil.ClearComparisons();

            string remoteServer = cb_remoteServer.Text;
            if (remoteServer == null || remoteServer == "" || cb_remoteServer.SelectedIndex == 0)
            {
                MessageBox.Show("请先选择远程服务器!");
                cb_remoteServer.Focus();
                return;
            }


            if (listBox1.Items.Count <= 0)
            {
                MessageBox.Show("请先选择本地项目文件,可直接拖放!");
                return;
            }
            DialogResult result = MessageBox.Show("此操作耗时较长,可能会因为长时间阻塞导致页面无响应.  计算过程中请勿关闭窗口! 点击\"取消\"可撤回此操作. ", "MD5计算", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                //计算过程中先禁用此按钮,防止重复产生点击事件
                button2.Text = "MD5(计算中...)";
                button2.Enabled = false;

                List<Dictionary<string, string>> clientMD5s = new List<Dictionary<string, string>>();
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    listBox1.SelectedIndex = i;
                    string listTag = listBox1.Tag.ToString();
                    string path = listBox1.Items[i].ToString();
                    FileInfo fileinfo = new FileInfo(path);
                    long size = fileinfo.Length;
                    string md5Value = MD5Util.GetMD5HashFromFile(path);
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    path = path.Replace(listTag, "").Trim();
                    map.Add("md5", md5Value);
                    map.Add("url", path);
                    map.Add("size", Convert.ToString(size));
                    clientMD5s.Add(map);
                }

                //请求服务器计算服务端MD5值
                string serverResponse = SocketUtil.SocketMD5(ClientCache.GetCurrentServerIP(), Convert.ToInt32(ClientCache.GetInterflowPort()), JSONUtil.RequestServerMD5InfoJsonStr(remoteServer));
                List<Dictionary<string, string>> serverMD5s = JSONUtil.ParseServerMD5Info(serverResponse);

                List<string> usedList = new List<string>();
                int count = 1;
                //开始比对和生成记录到ListView控件中,以本地项目为参照物.  因为一般情况下本地项目的文件会多于服务器端
                foreach (Dictionary<string, string> client in clientMD5s)//Loop client
                {
                    string clientUrl = client["url"];
                    string clientMd5 = client["md5"];
                    string clientSize = client["size"];
                    foreach (Dictionary<string, string> server in serverMD5s)//Loop server
                    {
                        string serverUrl = server["url"];
                        string serverMd5 = server["md5"];
                        string serverSize = server["size"];
                        //这一个替换在操作线上代码时格外重要! 线上Linux是/而windows是\\
                        serverUrl = serverUrl.Replace("/","\\");//Linux. Windows下注释此代码
                        if (clientUrl.Equals(serverUrl))
                        {
                            Dictionary<string, string> map = new Dictionary<string, string>();
                            map.Add("num", Convert.ToString(count++));
                            map.Add("urlremote", serverUrl);
                            map.Add("urllocal", clientUrl);
                            map.Add("sizeremote", serverSize);
                            map.Add("sizelocal", clientSize);

                            //存在相同的url,即文件 . 比对MD5值是否一致
                            if (clientMd5.Equals(serverMd5))
                            {
                                //MD5也一致,视为没有任何更改
                                map.Add("result", "一致");
                                ControlUtil.AddListViewLineOfComparison(map, Color.White, "一致");
                                usedList.Add(clientUrl);//将客户端本数据标记为已匹配过
                            }
                            else
                            {
                                //客户端和服务端同一个文件MD5不一致,视为客户端有更改. 需要后续上传
                                map.Add("result", "不一致");
                                ControlUtil.AddListViewLineOfComparison(map, Color.Red, "不一致");
                                usedList.Add(clientUrl);//将客户端本数据标记为已匹配过
                            }

                        }
                    }
                }
                //检查客户端剩下的没有检查过的数据,一般视为本地新增的数据
                foreach (Dictionary<string, string> client in clientMD5s)
                {
                    string clientUrl = client["url"];
                    string clientMd5 = client["md5"];
                    string clientSize = client["size"];
                    if (usedList.Contains(clientUrl))
                    {
                        continue;
                    }
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    map.Add("num", Convert.ToString(count++));
                    map.Add("urlremote", "");
                    map.Add("urllocal", clientUrl);
                    map.Add("sizeremote", "0");
                    map.Add("sizelocal", clientSize);
                    map.Add("result", "新增");
                    ControlUtil.AddListViewLineOfComparison(map, Color.Green, "新增");
                }
                ControlUtil.RefueshToControlComparisons(listView1, "all");
                button2.Text = "MD5";
                button2.Enabled = true;
            }
        }

        //筛选并显示
        private void 只显示新增ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ControlUtil.RefueshToControlComparisons(listView1, "新增");
        }

        private void 只显示不一致ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ControlUtil.RefueshToControlComparisons(listView1, "不一致");
        }

        private void 只显示一致ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ControlUtil.RefueshToControlComparisons(listView1, "一致");
        }

        private void 清除所有ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }

        private void 显示所有ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ControlUtil.RefueshToControlComparisons(listView1, "all");
        }

        private void 显示已忽略ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ControlUtil.RefueshToControlComparisons(listView1, "已忽略");
        }

        private void 忽略此项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListView.SelectedListViewItemCollection collection = listView1.SelectedItems;
                string selectedNums = "";
                for (int i = 0; i < collection.Count; i++)
                {
                    selectedNums += collection[i].SubItems[0].Text + ",";
                }
                selectedNums = selectedNums.Substring(0, selectedNums.LastIndexOf(','));
                DialogResult result = MessageBox.Show(string.Format("确认忽略编号\"{0}\"的匹配记录吗?忽略之后将不会被提交并覆盖到服务器对应的文件.", selectedNums), "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                if (result == DialogResult.OK)
                {
                    List<string> selectedIgnoreNum = new List<string>();
                    for (int i = 0; i < collection.Count; i++)
                    {
                        collection[i].BackColor = Color.Gray;
                        collection[i].SubItems[5].Text = "已忽略";
                        selectedIgnoreNum.Add(collection[i].SubItems[0].Text);
                    }
                    //同步状态到内存
                    ControlUtil.SynchronizationComparison(selectedIgnoreNum);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //新增
            if (checkBox1.Checked)
            {
                ControlUtil.AddToDoUploadList("新增", listBox1.Tag.ToString());
            }
            else
            {
                ControlUtil.RemoveToDoUpload("新增", listView2);
            }
            ControlUtil.AddToDoUploadToControl(listView2);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            //不一致
            if (checkBox2.Checked)
            {
                ControlUtil.AddToDoUploadList("不一致", listBox1.Tag.ToString());
            }
            else
            {
                ControlUtil.RemoveToDoUpload("不一致", listView2);
            }
            ControlUtil.AddToDoUploadToControl(listView2);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            //一致
            if (checkBox3.Checked)
            {
                ControlUtil.AddToDoUploadList("一致", listBox1.Tag.ToString());
            }
            else
            {
                ControlUtil.RemoveToDoUpload("一致", listView2);
            }
            ControlUtil.AddToDoUploadToControl(listView2);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            //已忽略
            if (checkBox4.Checked)
            {
                ControlUtil.AddToDoUploadList("已忽略", listBox1.Tag.ToString());
            }
            else
            {
                ControlUtil.RemoveToDoUpload("已忽略", listView2);
            }
            ControlUtil.AddToDoUploadToControl(listView2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                ////获取选中的服务器列表key
                string serverKey = cb_remoteServer.Text;
                //string privateKey = GetPrivateKey();
                //if (privateKey == null || privateKey.Equals(string.Empty))
                //{
                //    MessageBox.Show("请求操作秘钥失败, 请稍后重试!", "提示");
                //    return;
                //}
                ListView.ListViewItemCollection collection = listView2.Items;
                for (int i = 0; i < collection.Count; i++)
                {
                    Thread.Sleep(120);
                    FileInfoEntity entity = (FileInfoEntity)collection[i].Tag;
                    //1.1发送文件名字,文件路径和文件字节大小 方便后端做好接受准备
                    string responseMessage = SendFileInfo(entity.LocalURL, entity.FileName, entity.FileSize, serverKey);
                    Console.WriteLine("上传文件信息成功:" + responseMessage);
                    //修改客户端界面数据效果
                    collection[i].Selected = true;
                    collection[i].BackColor = Color.Red;
                    collection[i].SubItems[4].ForeColor = Color.Blue;
                    collection[i].SubItems[4].Text = "上传中";
                    collection[i].SubItems[3].Text = "50%";
                    Thread.Sleep(120);
                    if ("success".Equals(responseMessage))
                    {
                        try
                        {
                            //1.2发送文件字节数组
                            using (FileStream fs = new FileStream(entity.AbsolutionPath, FileMode.Open))
                            {
                                byte[] data = new byte[fs.Length];
                                //int count = fs.Read(data, 0, data.Length);

                                //s
                                Console.WriteLine("流长度:" + fs.Length);
                                int idx = 0;
                                int totalLen = data.Length;
                                int readLen = 0;
                                while (idx < totalLen)
                                {
                                    readLen = fs.Read(data, idx, totalLen - idx);
                                    if (readLen > 0)
                                    {
                                        idx = idx + readLen;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                Console.WriteLine("实际长度:" + idx);

                                //e

                                Console.WriteLine("准备上传文件..");
                                string fileUploadResponse = SocketUtil.UploadFile(ClientCache.GetCurrentServerIP(), Convert.ToInt32(ClientCache.GetInterflowPort()), data);
                                Console.WriteLine("上传完成:" + fileUploadResponse);
                                JObject json = JObject.Parse(fileUploadResponse);
                                string message = json["message"].ToString();
                                if ("success".Equals(message))
                                {
                                    collection[i].Selected = false;
                                    collection[i].BackColor = Color.Green;
                                    collection[i].SubItems[4].ForeColor = Color.Blue;
                                    collection[i].SubItems[4].Text = "上传完成";
                                    collection[i].SubItems[3].Text = "100%";
                                }
                                else
                                {
                                    collection[i].Selected = false;
                                    collection[i].BackColor = Color.Red;
                                    collection[i].SubItems[4].ForeColor = Color.Blue;
                                    collection[i].SubItems[4].Text = "上传失败";
                                    collection[i].SubItems[3].Text = "100%";
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("当前文件出现错误,跳过"+ex.Message);
                            collection[i].Selected = false;
                            collection[i].BackColor = Color.Green;
                            collection[i].SubItems[4].ForeColor = Color.Blue;
                            collection[i].SubItems[4].Text = "上传完成";
                            collection[i].SubItems[3].Text = "100%";
                        }
                    }
                    else
                    {
                        //修改客户端界面数据效果
                        collection[i].Selected = false;
                        collection[i].BackColor = Color.Red;
                        collection[i].SubItems[4].ForeColor = Color.Blue;
                        collection[i].SubItems[4].Text = "跳过";
                        collection[i].SubItems[3].Text = "100%";
                    }
                }
                button1.Enabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;
            }
            
        }

        private string GetPrivateKey()
        {
            long requestid = CommonUtil.GetRandomRequiredRequestID();
            //请求服务器获得本次操作授权的秘钥
            string requestjson = "{\"requestid\":\"" + requestid + "\",\"message\":\"success\",\"headingcode\":\"4f79a3158d691c5a7c1cda522763a247\"}";
            string response = SocketUtil.socket(ClientCache.GetCurrentServerIP(), Convert.ToInt32(ClientCache.GetInterflowPort()), requestjson);
            JObject json = new JSONUtil(response).GetJObject();
            return json["message"].ToString();
        }


        /// <returns></returns>
        private string SendFileInfo(string localurl, string filename, string filesize, string serverDirKey)
        {
            long requestId = CommonUtil.GetRandomRequiredRequestID();
              string requestjson = "{\"localurl\":\"" + localurl + "\",\"filename\":\"" + filename + "\",\"filesize\":\"" + filesize + "\",\"requestid\":\"" + requestId + "\",\"serverDirKey\":\"" + serverDirKey + "\",\"message\":\"success\",\"headingcode\":\"3f74a7cf0b351cbaaf4875f7331bc25d\"}";
            string response = SocketUtil.socket(ClientCache.GetCurrentServerIP(), Convert.ToInt32(ClientCache.GetInterflowPort()), requestjson);
            JObject json = new JSONUtil(response).GetJObject();
            return json["message"].ToString();
        }

        /// <summary>
        /// 远程项目备份按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                //检查是否选中了服务项
                string servername = cb_remoteServer.Text;
                if (servername == null || string.Empty.Equals(servername) || cb_remoteServer.SelectedIndex == 0)
                {
                    MessageBox.Show("请先指定需要备份的远程项目!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    cb_remoteServer.Focus();
                    return;
                }

                //禁用此按钮,避免重复请求. 此时还应当禁用MD5校验,和覆盖上传按钮
                button7.Enabled = false;
                button2.Enabled = false;
                button1.Enabled = false;
                //因为线上文件一般较大,因此在用户进行此操作时应当做适当的提醒
                DialogResult result = MessageBox.Show("由于线上项目文件较大,因此备份(ZIP)过程会因为比较漫长而导致页面假死. 请耐心等待", "备份", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                if (result == DialogResult.OK)
                {
                    string requestjson = "{\"requestid\":\"" + CommonUtil.GetRandomRequiredRequestID() + "\",\"message\":\"" + servername + "\",\"headingcode\":\"ed9ea8907f1963430bc8be4689d446b4\"}";
                    string response = SocketUtil.socket(ClientCache.GetCurrentServerIP(), Convert.ToInt32(ClientCache.GetInterflowPort()), requestjson);
                    JSONUtil json = new JSONUtil(response);
                    string message = Convert.ToString(json.GetJObject()["message"]);
                    if ("failed".Equals(message))
                    {
                        MessageBox.Show(servername + "压缩失败,请通过查看前后端日志以了解或解决此问题!");
                    }
                    else
                    {
                        MessageBox.Show("压缩成功,文件已保存至服务器:" + message);
                    }
                }
                button7.Enabled = true;
                button2.Enabled = true;
                button1.Enabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 本地项目备份,打包成ZIP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (ClientCache.GetClientProjectDir() == null || ClientCache.GetClientProjectDir() == string.Empty)
                {
                    MessageBox.Show("请选择或拖入本地项目, 否则无法定位到需要备份的本地项目!", "提示", MessageBoxButtons.OK);
                    return;
                }
                button2.Enabled = false;
                button1.Enabled = false;

                FolderBrowserDialog folder = new FolderBrowserDialog();
                folder.ShowNewFolderButton = true;
                folder.Description = "选择本地备份文件存放位置";
                DialogResult result = folder.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string filepath = folder.SelectedPath;
                    string returnmsg = "success";
                    string savepath = filepath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "项目备份.zip";
                    ZIPUtil.Zip(ClientCache.GetClientProjectDir(), savepath, ref returnmsg);
                    if ("success".Equals(returnmsg))
                    {
                        MessageBox.Show("本地项目备份成功,已保存至" + savepath, "备份");
                    }
                    else
                    {
                        MessageBox.Show("备份失败," + returnmsg, "备份失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                button2.Enabled = true;
                button1.Enabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}