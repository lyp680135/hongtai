using DataLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfQualityCertPrinter.Common;
using XYNetCloud.Utils;

namespace WpfQualityCertPrinter.ModelAccess
{
    class SalePrintlogHttpAccess
    {
        public SalePrintlog Single(int? id = null)
        {
            List<Tuple<string, string>> paramlist = new List<Tuple<string, string>>();
            if (id != null)
            {
                paramlist.Add(new Tuple<string, string>("id", id.Value.ToString()));
            }

            try
            {
                string result = HttpUtils.GetInstance().Get(CommonHouse.GET_CERT_URL, paramlist);
                if (result != null)
                {
                    JObject obj = (JObject)JsonConvert.DeserializeObject(result);

                    if (obj != null)
                    {
                        var datastr = obj["data"];

                        if (datastr != null)
                        {
                            JObject data = (JObject)JsonConvert.DeserializeObject(datastr.ToString());

                            SalePrintlog log = new SalePrintlog();
                            log.Id = Convert.ToInt32(data["Id"].ToString());
                            log.Authid = Convert.ToInt32(data["Authid"].ToString());
                            log.Consignor = data["Consignor"].ToString();
                            log.Number = Convert.ToInt32(data["Number"].ToString());
                            log.Printno = data["Printno"].ToString();
                            log.Status = Convert.ToInt32(data["Status"].ToString());
                            log.Checkcode = data["Checkcode"].ToString();

                            return log;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return null;
        }

        public SalePrintlog Single(string printno)
        {
            List<Tuple<string, string>> paramlist = new List<Tuple<string, string>>();
            if (printno != null)
            {
                paramlist.Add(new Tuple<string, string>("printno", printno));
            }

            try
            {
                string result = HttpUtils.GetInstance().Get(CommonHouse.GET_CERT_URL, paramlist);
                if (result != null)
                {
                    JObject obj = (JObject)JsonConvert.DeserializeObject(result);

                    if (obj != null)
                    {
                        var datastr = obj["data"];

                        if (datastr != null)
                        {
                            JObject data = (JObject)JsonConvert.DeserializeObject(datastr.ToString());

                            SalePrintlog log = new SalePrintlog();
                            log.Id = Convert.ToInt32(data["Id"].ToString());
                            log.Authid = Convert.ToInt32(data["Authid"].ToString());
                            log.Consignor = data["Consignor"].ToString();
                            log.Number = Convert.ToInt32(data["Number"].ToString());
                            log.Printno = data["Printno"].ToString();
                            log.Status = Convert.ToInt32(data["Status"].ToString());
                            log.Checkcode = data["Checkcode"].ToString();

                            return log;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return null;
        }

    }
}
