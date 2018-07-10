using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
