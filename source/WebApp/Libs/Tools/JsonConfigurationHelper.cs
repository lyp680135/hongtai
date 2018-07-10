using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Util
{
   public class JsonConfigurationHelper
    {
        private IConfiguration Configuration { get; set; }
        public JsonConfigurationHelper() { }
        private string jsonName = "config.json";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">文件名</param>
        public JsonConfigurationHelper(string jsonName = "")
        {
            if (!string.IsNullOrEmpty(jsonName))
               this. jsonName = jsonName;
            var baseDir = Directory.GetCurrentDirectory() + "\\";
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .Add(new JsonConfigurationSource { Path =this. jsonName, Optional = false, ReloadOnChange = true })
                .Build();
            Configuration = config;
        }

        public string GetJsonValue(string jsonRootName)
        {
            return Configuration[jsonRootName].ToString();
        }
    }
}
