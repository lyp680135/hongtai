using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace WpfCardPrinter.Utils
{
    class CommonUtils
    {
        /// <summary>
        /// 从配置文件中获取坐标值
        /// </summary>
        /// <param name="settingvalue"></param>
        /// <returns></returns>
        public static Point GetPointFromSetting(string settingvalue)
        {
            Point point = new Point(0, 0);
            if (!string.IsNullOrEmpty(settingvalue))
            {
                settingvalue.Replace("|", ",");
                settingvalue.Replace("，", ",");
                settingvalue.Replace("/", ",");
                settingvalue.Replace("\\", ",");
                settingvalue.Replace("-", ",");
                settingvalue.Replace("#", ",");

                var values = settingvalue.Split(',');
                if (values.Length > 0)
                {
                    int x = 0, y = 0;
                    int.TryParse(values.First(), out x);
                    int.TryParse(values.Last(), out y);

                    point.X = x;
                    point.Y = y;
                }
            }

            return point;
        }
        /// <summary>
        /// 更新appconfig里面的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void UpdateAppSetting(string key, string value)
        {
            bool isModified = false;
            foreach (string keys in ConfigurationManager.AppSettings)
            {
                if (keys == key)
                {
                    isModified = true;
                }
            }
            Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (isModified)
            {
                config.AppSettings.Settings.Remove(key);
            }

            config.AppSettings.Settings.Add(key, value);

            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
        /// <summary>
        /// 执行CMD命令
        /// </summary>
        /// <param name="str"></param>
        public static void start(string str)
        {
            using (System.Diagnostics.Process p = new System.Diagnostics.Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.StartInfo.Verb = "RunAs";
                //p.StartInfo.Arguments = str;
                p.Start();//启动程序
                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine(str);
                //p.StandardInput.WriteLine("exit");
                p.StandardInput.AutoFlush = true;

            }

        }

        public static List<string> GetKeys(string keywords)
        {
            /* Task_120，查带有空格关键词时会自动删除关键词中的空格 ， 2017-02-22，章圣钻  start*/
            var keys = keywords.Replace("\n", ",").Replace("，", ",").Replace("、", ",").Replace("。", ",").Split(',');
            List<string> res = new List<string>();
            foreach (var item in keys)
            {
                var key = item.Trim().ToLower();
                if (!string.IsNullOrEmpty(key) && !res.Contains(key))
                {
                    res.Add(key);
                }
            }
            return res;
            /*end*/
        }
    }
}
