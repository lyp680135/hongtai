using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYNetCloud.Utils
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
    }
}
