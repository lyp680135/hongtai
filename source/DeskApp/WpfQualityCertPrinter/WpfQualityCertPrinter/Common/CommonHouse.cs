using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfQualityCertPrinter.Common
{
    class CommonHouse
    {
        private static string WEBAPI_PROTOCOL = "http://";
        private static string WEBAPI_SERVER = "dev.xiaoyutt.com";
        private static string WEBAPI_PORT = "7053";
        private static string WEBAPI_ROOT = "api";
        private static string WEBAPI_VERSION = "v1";


        private static string HTTP_DOMAIN_PREFIX = WEBAPI_PROTOCOL + WEBAPI_SERVER + ":" + WEBAPI_PORT + "/" + WEBAPI_ROOT + "/" + WEBAPI_VERSION + "/";
        public static string CERT_PATH_PREFIX = WEBAPI_PROTOCOL + WEBAPI_SERVER + ":" + WEBAPI_PORT + "/qualitypics/";

        public static string GET_CERT_URL = HTTP_DOMAIN_PREFIX + "cert";

        public static string GENERATE_CERT_URL = HTTP_DOMAIN_PREFIX + "cert";
    }
}
