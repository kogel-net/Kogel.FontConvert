using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.RegularExpressions;

namespace FontConvert
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Multiselect = true;
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Clear();
                this.textBox1.Text = string.Join(",", openfile.FileNames);
            }
        }
        private void ReadFile(string filePath, Func<string, string> callback)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                FileInfo file = new FileInfo(filePath);
                StreamReader reader = file.OpenText();
                string line;
                //读取文本内容
                while (null != (line = reader.ReadLine()))
                {
                    string newLine = callback(line);
                    sb.Append(newLine + Environment.NewLine);
                }
                reader.Close();
                //开始写入繁体文本
                System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sb.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.textBox1.Text))
            {
                foreach (string itemPath in this.textBox1.Text.Split(','))
                {
                    ReadFile(itemPath, (line) =>
                    {
                        //进行简体转繁体
                        string newValue = ChineseStringUtility.ToTraditional(line);
                        return newValue;
                    });
                }
                MessageBox.Show("转换成功");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.textBox1.Text))
            {
                foreach (string itemPath in this.textBox1.Text.Split(','))
                {
                    ReadFile(itemPath, (line) =>
                    {
                        string newValue = line.Replace("新宋体", "微软雅黑").Replace("宋体", "微软雅黑").Replace("Microsoft Sans Serif", "微软雅黑").Replace("Arial", "微软雅黑");
                        return newValue;
                    });
                }
                MessageBox.Show("转换成功");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.textBox1.Text))
            {
                foreach (string itemPath in this.textBox1.Text.Split(','))
                {
                    ReadFile(itemPath, (line) =>
                    {
                        if (line.Contains("<TextObject") && !line.Contains("Font="))
                        {
                            line = line.Replace("/>", " Font=\"微软雅黑, 9pt\"/>");
                        }
                        return line;
                    });
                }
                MessageBox.Show("转换成功");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.textBox1.Text))
            {
                Regex regex = new Regex("Font=\"微软雅黑, ([1-9]\\d*\\.?\\d*)|(0\\.\\d*[1-9])\"");
                foreach (string itemPath in this.textBox1.Text.Split(','))
                {
                    ReadFile(itemPath, (line) =>
                    {
                        var groups = regex.Match(line).Groups;
                        if (groups.Count != 0)
                        {
                            for (var i = 0; i < groups.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(groups[i].Value) && Regex.IsMatch(groups[i].Value, @"^[+-]?\d*[.]?\d*$"))
                                {
                                    var num = Convert.ToDouble(groups[i].Value);
                                    if (num >= 9)
                                    {
                                        line = line.Replace("微软雅黑, " + num, "微软雅黑, " + (num - 0.5));
                                    }
                                    break;
                                }
                            }
                        }
                        return line;
                    });
                }
                MessageBox.Show("转换成功");
            }
        }

		private void button6_Click(object sender, EventArgs e)
		{
			dicList = new List<string>();
			DirectoryInfo root = new DirectoryInfo(this.textBox1.Text);
			string filePath = this.textBox1.Text;
			 GetName(filePath);
			this.textBox1.Text = string.Join(",", dicList);
		}
		static List<string> dicList = new List<string>();
		static string[] fileTypes = new string[] { ".cs", ".html" };
		public static void GetName(string path)
		{

			DirectoryInfo dir = new DirectoryInfo(path);
			//返回目录下的全部文件
			FileSystemInfo[] fileInfo = dir.GetFileSystemInfos();
			foreach (var item in fileInfo)
			{

				if (item is DirectoryInfo)
				{

					GetName(item.FullName);
				}
				else
				{
					if (fileTypes.FirstOrDefault(x => item.FullName.IndexOf(x) != -1) != null)
						dicList.Add(item.FullName);
				}
			}
		}
	}
	/// <summary>
	/// 中文字符工具类
	/// </summary>
	public static class ChineseStringUtility
    {
        private const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        /// <summary>
        /// 将字符转换成简体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        public static string ToSimplified(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, source, source.Length, target, source.Length);
            return target;
        }

        /// <summary>
        /// 将字符转换为繁体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        public static string ToTraditional(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, source, source.Length, target, source.Length);
            return target;
        }
    }
}
