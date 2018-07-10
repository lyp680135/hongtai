using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XYNetCloud.Utils
{
    class LogHelper
    {
        private LogHelper()
        {
        }

        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("AccessLogger");

        public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("ErrorLogger");

        public static void SetConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void SetConfig(FileInfo configFile)
        {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        public static void WriteLog(string info)
        {
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(info);
            }
        }

        public static void WriteLog(string info, Exception se)
        {
            if (logerror.IsErrorEnabled)
            {
                logerror.Error(info, se);
            }
        }
    }
}
