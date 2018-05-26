using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerMachineInfoShower_Client
{
    public partial class AddServerMachine : Form
    {
        public AddServerMachine()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String ipAddr = txt_ip.Text;
            String port = txt_port.Text;
            String nickname = txt_nickname.Text;

            if (ipAddr == null || ipAddr == String.Empty)
            {
                MessageBox.Show("请输入IP地址!");
                txt_ip.Focus();
                txt_ip.Select();
                return;
            }
            if (port == null || port == String.Empty)
            {
                MessageBox.Show("请输入端口号!");
                txt_port.Focus();
                txt_port.Select();
                return;
            }
            if (nickname == null || nickname == String.Empty)
            {
                MessageBox.Show("请输入服务器别名!");
                txt_nickname.Focus();
                txt_nickname.Select();
                return;
            }

            try
            {
                String line = nickname + "$" + ipAddr + "$" + port;
                string path = Application.ExecutablePath;
                path = path.Substring(0, path.LastIndexOf('\\'));
                //将一行配置写入服务器信息配置文件中
                if (CommonUtil.ExistsFile(GobalConst.ServerConfigFileName))
                {
                    CommonUtil.AppendToFile(path + "\\" + GobalConst.ServerConfigFileName, line);
                }
                else
                {
                    //文件不存在,先创建
                    CommonUtil.CreateNewFile(GobalConst.ServerConfigFileName);
                    CommonUtil.AppendToFile(path + "\\" + GobalConst.ServerConfigFileName, line);
                }
                DialogResult result = MessageBox.Show("添加成功! 点击'确定'重启此应用程序以刷新数据,或手动进入主界面刷新树菜单!","添加成功", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if(result == DialogResult.OK)
                {
                    //重启此应用程序
                    Application.Restart();
                }
                else
                {
                    this.Close();
                }
            }catch(Exception ex)
            {
                MessageBox.Show("添加服务器信息到配置时出现异常,请联系开发者:icefrogsu@gmail.com"+System.Environment.NewLine+"错误信息:"+ex.Message,"不可预知的错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            
           
        }

        private void AddServerMachine_Load(object sender, EventArgs e)
        {
            //加载事件,让第一个文本框获得焦点
            txt_ip.Focus();
            txt_ip.Select();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            String msg = "拍车集汽车电子商务有限公司"+System.Environment.NewLine
                        +"本功能用于自定义新的服务器信息,包含服务器IP,端口,别名. 其中关于每一项的定义解释如下:"+System.Environment.NewLine
                        +"服务器IP：用于定义连接到服务器的IPV4信息,请确保此IP可以使用ping.exe工具确认可连通,并且请确保此IP指向的服务器已经运行于此客户端对应的服务器工具"+System.Environment.NewLine
                        +"如果对此客户端对应的服务器工具并不知情,可联系开发者:icefrogsu@gmail.com"+System.Environment.NewLine
                        +"端口：端口的配置必须和服务器工具端口一致,若IP和此端口配置有错,则可能导致本工具大部分功能无法使用!"+System.Environment.NewLine
                        +"别名：别名为本客户端表示本次服务器信息配置的一种标记,方便你放弃使用无逻辑的IP地址记忆方式!"+System.Environment.NewLine
                        +"注意:IP地址和端口号并未进行有效性校验,请谨慎输入!" + System.Environment.NewLine+ System.Environment.NewLine
                        +"版权信息:"+ System.Environment.NewLine
                        + "Copyright 2017 www.paicheji.com Inc. All rights reserved.";

            MessageBox.Show(msg,"提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
    }
}
