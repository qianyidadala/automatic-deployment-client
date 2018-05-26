using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Core;
namespace ServerMachineInfoShower_Client
{
    public class ZIPUtil
    {

        /// <summary>   
        /// 压缩文件或文件夹   ----带密码
        /// </summary>   
        /// <param name="fileToZip">要压缩的路径-文件夹或者文件</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码</param>
        /// <param name="errorOut">如果失败返回失败信息</param>
        /// <returns>压缩结果</returns>   
        public static bool Zip(string fileToZip, string zipedFile, string password, ref string errorOut)
        {
            bool result = false;
            try
            {
                if (Directory.Exists(fileToZip))
                    result = ZipDirectory(fileToZip, zipedFile, password);
                else if (File.Exists(fileToZip))
                    result = ZipFile(fileToZip, zipedFile, password);
            }
            catch (Exception ex)
            {
                errorOut = ex.Message;
            }
            return result;
        }

        /// <summary>   
        /// 压缩文件或文件夹 ----无密码 
        /// </summary>   
        /// <param name="fileToZip">要压缩的路径-文件夹或者文件</param>   
        /// <param name="zipedFile">压缩后的文件名</param>
        /// <param name="errorOut">如果失败返回失败信息</param>
        /// <returns>压缩结果</returns>   
        public static bool Zip(string fileToZip, string zipedFile, ref string errorOut)
        {
            bool result = false;
            try
            {
                if (Directory.Exists(fileToZip))
                    result = ZipDirectory(fileToZip, zipedFile, null);
                else if (File.Exists(fileToZip))
                    result = ZipFile(fileToZip, zipedFile, null);
            }
            catch (Exception ex)
            {
                errorOut = ex.Message;
            }
            return result;
        }

        /// <summary>   
        /// 压缩文件   
        /// </summary>   
        /// <param name="fileToZip">要压缩的文件全名</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码</param>   
        /// <returns>压缩结果</returns>   
        private static bool ZipFile(string fileToZip, string zipedFile, string password)
        {
            bool result = true;
            ZipOutputStream zipStream = null;
            FileStream fs = null;
            ZipEntry ent = null;

            if (!File.Exists(fileToZip))
                return false;

            try
            {
                fs = File.OpenRead(fileToZip);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                fs = File.Create(zipedFile);
                zipStream = new ZipOutputStream(fs);
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                ent = new ZipEntry(Path.GetFileName(fileToZip));
                zipStream.PutNextEntry(ent);
                zipStream.SetLevel(6);

                zipStream.Write(buffer, 0, buffer.Length);

            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Finish();
                    zipStream.Close();
                }
                if (ent != null)
                {
                    ent = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
            GC.Collect();
            GC.Collect(1);

            return result;
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="strFile">带压缩的文件夹目录</param>
        /// <param name="strZip">压缩后的文件名</param>
        /// <param name="password">压缩密码</param>
        /// <returns>是否压缩成功</returns>
        private static bool ZipDirectory(string strFile, string strZip, string password)
        {
            bool result = false;
            if (!Directory.Exists(strFile)) return false;
            if (strFile[strFile.Length - 1] != Path.DirectorySeparatorChar)
                strFile += Path.DirectorySeparatorChar;
            ZipOutputStream s = new ZipOutputStream(File.Create(strZip));
            s.SetLevel(6); // 0 - store only to 9 - means best compression
            if (!string.IsNullOrEmpty(password)) s.Password = password;
            try
            {
                result = zip(strFile, s, strFile);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                s.Finish();
                s.Close();
            }
            return result;
        }

        /// <summary>
        /// 压缩文件夹内部方法
        /// </summary>
        /// <param name="strFile"></param>
        /// <param name="s"></param>
        /// <param name="staticFile"></param>
        /// <returns></returns>
        private static bool zip(string strFile, ZipOutputStream s, string staticFile)
        {
            bool result = true;
            if (strFile[strFile.Length - 1] != Path.DirectorySeparatorChar) strFile += Path.DirectorySeparatorChar;
            Crc32 crc = new Crc32();
            try
            {
                string[] filenames = Directory.GetFileSystemEntries(strFile);
                foreach (string file in filenames)
                {

                    if (Directory.Exists(file))
                    {
                        zip(file, s, staticFile);
                    }

                    else // 否则直接压缩文件
                    {
                        //打开压缩文件
                        FileStream fs = File.OpenRead(file);

                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        string tempfile = file.Substring(staticFile.LastIndexOf("\\") + 1);
                        ZipEntry entry = new ZipEntry(tempfile);

                        entry.DateTime = DateTime.Now;
                        entry.Size = fs.Length;
                        fs.Close();
                        crc.Reset();
                        crc.Update(buffer);
                        entry.Crc = crc.Value;
                        s.PutNextEntry(entry);

                        s.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }
            return result;

        }

        /// <summary>   
        /// 解压功能(解压压缩文件到指定目录)---->不需要密码
        /// </summary>   
        /// <param name="fileToUnZip">待解压的文件</param>   
        /// <param name="zipedFolder">指定解压目标目录</param>  
        /// <param name="errorOut">如果失败返回失败信息</param> 
        /// <returns>解压结果</returns>   
        public static bool UPZipFile(string fileToUnZip, string zipedFolder, ref string errorOut)
        {
            bool result = false;
            try
            {
                result = UPZipFileByPassword(fileToUnZip, zipedFolder, null);
            }
            catch (Exception ex)
            {
                errorOut = ex.Message;
            }
            return result;
        }
        /// <summary>   
        /// 解压功能(解压压缩文件到指定目录)---->需要密码
        /// </summary>   
        /// <param name="fileToUnZip">待解压的文件</param>   
        /// <param name="zipedFolder">指定解压目标目录</param>
        /// <param name="password">密码</param> 
        /// <param name="errorOut">如果失败返回失败信息</param> 
        /// <returns>解压结果</returns>
        public static bool UPZipFile(string fileToUnZip, string zipedFolder, string password, ref string errorOut)
        {
            bool result = false;
            try
            {
                result = UPZipFileByPassword(fileToUnZip, zipedFolder, password);
            }
            catch (Exception ex)
            {
                errorOut = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 解压功能 内部处理方法
        /// </summary>
        /// <param name="TargetFile">待解压的文件</param>
        /// <param name="fileDir">指定解压目标目录</param>
        /// <param name="password">密码</param>
        /// <returns>成功返回true</returns>
        private static bool UPZipFileByPassword(string TargetFile, string fileDir, string password)
        {
            bool rootFile = true;
            try
            {
                //读取压缩文件(zip文件)，准备解压缩
                ZipInputStream zipStream = new ZipInputStream(File.OpenRead(TargetFile.Trim()));
                ZipEntry theEntry;
                string path = fileDir;

                string rootDir = " ";
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                while ((theEntry = zipStream.GetNextEntry()) != null)
                {
                    rootDir = Path.GetDirectoryName(theEntry.Name);
                    if (rootDir.IndexOf("\\") >= 0)
                    {
                        rootDir = rootDir.Substring(0, rootDir.IndexOf("\\") + 1);
                    }
                    string dir = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (dir != " ")
                    {
                        if (!Directory.Exists(fileDir + "\\" + dir))
                        {
                            path = fileDir + "\\" + dir;
                            Directory.CreateDirectory(path);
                        }
                    }
                    else if (dir == " " && fileName != "")
                    {
                        path = fileDir;
                    }
                    else if (dir != " " && fileName != "")
                    {
                        if (dir.IndexOf("\\") > 0)
                        {
                            path = fileDir + "\\" + dir;
                        }
                    }

                    if (dir == rootDir)
                    {
                        path = fileDir + "\\" + rootDir;
                    }

                    //以下为解压缩zip文件的基本步骤
                    //基本思路就是遍历压缩文件里的所有文件，创建一个相同的文件。
                    if (fileName != String.Empty)
                    {
                        FileStream streamWriter = File.Create(path + "\\" + fileName);

                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }

                        streamWriter.Close();
                    }
                }
                if (theEntry != null)
                {
                    theEntry = null;
                }
                if (zipStream != null)
                {
                    zipStream.Close();
                }
            }
            catch (Exception ex)
            {
                rootFile = false;
                throw ex;
            }
            finally
            {
                GC.Collect();
                GC.Collect(1);
            }
            return rootFile;
        }
    }
}