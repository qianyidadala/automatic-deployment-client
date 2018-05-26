using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerMachineInfoShower_Client
{
    /// <summary>
    /// 控件元素帮助类,一般用于将操作页面上的控件代码从业务代码中脱离出来. 避免代码可读性差
    /// </summary>
    public class ControlUtil
    {
        private static List<ListViewItem> Comparisons = new List<ListViewItem>();

        public static void AddListViewLineOfComparison(Dictionary<string, string> map, Color backColor, string tag)
        {
            ListViewItem item = new ListViewItem(map["num"]);
            item.Tag = tag;
            item.BackColor = backColor;
            item.SubItems.Add(map["urlremote"]);
            item.SubItems.Add(map["urllocal"]);
            item.SubItems.Add(map["sizeremote"]);
            item.SubItems.Add(map["sizelocal"]);
            item.SubItems.Add(map["result"]);
            item.ToolTipText = map["result"];
            Comparisons.Add(item);
        }

        public static void ClearComparisons()
        {
            Comparisons.Clear();
        }

        public static void SynchronizationComparison(List<string> nums)
        {
            foreach (string reg in nums)
            {
                for (int i = 0; i < Comparisons.Count; i++)
                {
                    if (Comparisons[i].SubItems[0].Text.Equals(reg))
                    {
                        //此处并不是移除,而是修改状态
                        Comparisons[i].SubItems[5].Text = "已忽略";
                        Comparisons[i].Tag = "已忽略";
                    }
                }
            }
        }

        /// <summary>
        /// 将内存中的数据添加到Listview中
        /// type:all/一致/不一致/新增 
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="type"></param>
        public static void RefueshToControlComparisons(ListView listView, string type)
        {
            listView.Items.Clear();
            if ("all".Equals(type))
            {
                foreach (ListViewItem item in Comparisons)
                {
                    listView.Items.Add(item);
                }
            }
            else if ("一致".Equals(type))
            {
                foreach (ListViewItem item in Comparisons)
                {
                    string tag = Convert.ToString(item.Tag);
                    if ("一致".Equals(tag))
                    {
                        listView.Items.Add(item);
                    }
                }
            }
            else if ("不一致".Equals(type))
            {
                foreach (ListViewItem item in Comparisons)
                {
                    string tag = Convert.ToString(item.Tag);
                    if ("不一致".Equals(tag))
                    {
                        listView.Items.Add(item);
                    }
                }
            }
            else if ("新增".Equals(type))
            {
                foreach (ListViewItem item in Comparisons)
                {
                    string tag = Convert.ToString(item.Tag);
                    if ("新增".Equals(tag))
                    {
                        listView.Items.Add(item);
                    }
                }
            }
            else if ("已忽略".Equals(type))
            {
                foreach (ListViewItem item in Comparisons)
                {
                    string tag = Convert.ToString(item.Tag);
                    if ("已忽略".Equals(tag))
                    {
                        listView.Items.Add(item);
                    }
                }
            }
        }




        /// <summary>
        /// 待上传的listview数据list
        /// </summary>
        private static List<Dictionary<string, string>> ToDoUploadList = new List<Dictionary<string, string>>();

        public static void AddToDoUploadList(string type,string absolutionPath)
        {
            foreach (ListViewItem comparisons in Comparisons)
            {
                string tag = Convert.ToString(comparisons.Tag);
                if (type.Equals(tag))
                {
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    string localUrl = comparisons.SubItems[2].Text;
                    localUrl = absolutionPath + localUrl;
                    FileInfo fileinfo = new FileInfo(localUrl);
                    if (fileinfo.Exists)
                    {
                        string id = CommonUtil.GetRamdomStr();
                        map.Add("num", id);//进程编号,依次递增
                        map.Add("filename", fileinfo.Name);//文件名
                        map.Add("filesize", Convert.ToString(fileinfo.Length));//文件大小
                        map.Add("process", "0%");//进度
                        map.Add("status", "就绪");//状态
                        map.Add("type", type);//此状态用于添加到view的tag下
                        map.Add("localurl", comparisons.SubItems[2].Text);
                        map.Add("absolutionPath", localUrl);
                        ToDoUploadList.Add(map);
                    }
                }
            }
        }

        public static void RemoveToDoUpload(string type,ListView listview)
        {
            for (int i = ToDoUploadList.Count - 1; i >= 0; i--)
            {
                Dictionary<string, string> map = ToDoUploadList[i];
                if (type.Equals(map["type"]))
                {
                    ToDoUploadList.RemoveAt(i);
                }
            }
        }

        public static void AddToDoUploadToControl(ListView listView)
        {
            listView.Items.Clear();
            foreach (Dictionary<string, string> item in ToDoUploadList)
            {
                ListViewItem viewItem = new ListViewItem(item["num"]);
                viewItem.SubItems.Add(item["filename"]);
                viewItem.SubItems.Add(item["filesize"]);
                viewItem.SubItems.Add(item["process"]);
                viewItem.SubItems.Add(item["status"]);
                FileInfoEntity entity = new FileInfoEntity();
                entity.LocalURL = item["localurl"];
                entity.AbsolutionPath = item["absolutionPath"];
                entity.FileName = item["filename"];
                entity.FileSize = item["filesize"];
                //viewItem.Tag = item["type"];
                viewItem.Tag = entity;
                listView.Items.Add(viewItem);
            }
        }

        /// <summary>
        /// 获得待上传的文件列表
        /// </summary>
        /// <returns></returns>
        public static List<Dictionary<string, string>> GetTODOUploadList()
        {
            return ToDoUploadList;
        }
    }
}