using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfQualityCertPrinter.Common
{
    class CommonHouse
    {
#if DEBUG
        private static string WEBAPI_PROTOCOL = "http://";
        private static string WEBAPI_SERVER = "localhost";
        private static string WEBAPI_PORT = "41178";
        private static string WEBAPI_ROOT = "api";
        private static string WEBAPI_VERSION = "v1";
#else
        private static string WEBAPI_PROTOCOL = "http://";
        private static string WEBAPI_SERVER = "api.jshtsteel.com";
        private static string WEBAPI_PORT = "80";
        private static string WEBAPI_ROOT = "api";
        private static string WEBAPI_VERSION = "v1";
#endif

        private static string HTTP_DOMAIN_PREFIX = WEBAPI_PROTOCOL + WEBAPI_SERVER + ":" + WEBAPI_PORT + "/" + WEBAPI_ROOT + "/" + WEBAPI_VERSION + "/";
        public static string CERT_PATH_PREFIX = WEBAPI_PROTOCOL + WEBAPI_SERVER + ":" + WEBAPI_PORT + "/qualitypics/";

        public static string GET_CERT_URL = HTTP_DOMAIN_PREFIX + "certNew";

        public static string GENERATE_CERT_URL = HTTP_DOMAIN_PREFIX + "certNew";
    }
}
