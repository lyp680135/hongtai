namespace ServiceXUnit.Test
{
    using System;
    using Common.IService;
    using Common.Service;
    using DataLibrary;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string dbconnectstring = "Server=192.168.0.120; Port=3306; Uid=warranty; Pwd=warranty123456; Database=warranty_hongtai";

            DbContextOptions<DataLibrary.DataContext> options;
            var builder = new DbContextOptionsBuilder<DataLibrary.DataContext>();
            builder.UseMySql(dbconnectstring);
            options = builder.Options;

            DataLibrary.DataContext dataContext = new DataLibrary.DataContext(options);

            BaseService<PdQuality> pdQuality = new BaseService<PdQuality>(dataContext);
            BaseService<MngSetting> mngSetting = new BaseService<MngSetting>(dataContext);
            BaseService<PdQualityProductPreset> pdQualityProductPreset = new BaseService<PdQualityProductPreset>(dataContext);

            QualityService qservice = new QualityService(pdQuality, mngSetting, pdQualityProductPreset);

            StockOutService stock = new StockOutService(dataContext);

            CertService service = new CertService(dataContext, qservice, stock);

            var str = "{\"list\":[{\"Batcode\":\"A18050012\",\"Classid\":1,\"Classname\":null,\"Materialid\":1,\"Materialname\":\"HRB400\"," +
                "\"Specid\":1,\"Length\":\"L\",\"Lengthtype\":0,\"Lengthnote\":\"定尺\",\"Specfullname\":\"Ф12x9\",\"Number\":2}],"
                + "\"lpn\":\"江XXX\",\"sellerid\":1,\"consignor\":\"\"}";
            str = "{\"printid\":0,\"list\":\"[{\\\"Batcode\\\":\\\"06B-181301\\\",\\\"Classid\\\":2,\\\"Classname\\\":null,\\\"Materialid\\\":2,\\\"Materialname\\\":\\\"HRB400E\\\",\\\"Specid\\\":5,\\\"Length\\\":\\\"L\\\",\\\"Lengthtype\\\":10,\\\"Lengthnote\\\":\\\"非尺\\\",\\\"Specfullname\\\":\\\"Φ12x9\\\",\\\"Startbundle\\\":0,\\\"Endbundle\\\":0,\\\"Number\\\":1}]\",\"lpn\":\"浙A000\",\"sellerid\":1,\"consignor\":\"江工\"}";
            JArray array = new JArray();
            var obj = (JObject)JsonConvert.DeserializeObject(str);
            var list = obj["list"].ToString();
            var lpn = obj["lpn"].ToString();
            var sellerid = int.Parse(obj["sellerid"].ToString());
            var consignor = obj["consignor"].ToString();

            array = (JArray)JsonConvert.DeserializeObject(list);

            var result = service.AddCert(array, sellerid, lpn, consignor, 0);

            Assert.True(result != null);
        }

        [Fact]
        public void Test2()
        {
            string dbconnectstring = "Server=192.168.0.120; Port=3306; Uid=warranty; Pwd=warranty123456; Database=warranty_hongtai";

            DbContextOptions<DataLibrary.DataContext> options;
            var builder = new DbContextOptionsBuilder<DataLibrary.DataContext>();
            builder.UseMySql(dbconnectstring);
            options = builder.Options;

            DataLibrary.DataContext dataContext = new DataLibrary.DataContext(options);

            BaseService<PdQuality> pdQuality = new BaseService<PdQuality>(dataContext);
            BaseService<MngSetting> mngSetting = new BaseService<MngSetting>(dataContext);
            BaseService<PdQualityProductPreset> pdQualityProductPreset = new BaseService<PdQualityProductPreset>(dataContext);

            QualityService qservice = new QualityService(pdQuality, mngSetting, pdQualityProductPreset);

            StockOutService stock = new StockOutService(dataContext);

            CertService service = new CertService(dataContext, qservice, stock);

            var result = service.GenerateCert("10000044", Environment.CurrentDirectory, true);

            Assert.True(result != null && result.Status == (int)CommonResultStatus.Success && result.Message == "生成成功");
        }

        [Fact]
        public void Test3()
        {
            string dbconnectstring = "Server=192.168.0.120; Port=3306; Uid=warranty; Pwd=warranty123456; Database=warranty_hongtai";

            DbContextOptions<DataLibrary.DataContext> options;
            var builder = new DbContextOptionsBuilder<DataLibrary.DataContext>();
            builder.UseMySql(dbconnectstring);
            options = builder.Options;

            DataLibrary.DataContext dataContext = new DataLibrary.DataContext(options);

            BatcodeService service = new BatcodeService(dataContext);

            var result = service.GenerateNextBatcode(1, 2, true);

            Assert.True(result.Data.ToString() == "06B-B181401");
        }

        [Fact]
        public void Test4()
        {
            string dbconnectstring = "Server=192.168.0.120; Port=3306; Uid=warranty; Pwd=warranty123456; Database=warranty_hongtai";

            DbContextOptions<DataLibrary.DataContext> options;
            var builder = new DbContextOptionsBuilder<DataLibrary.DataContext>();
            builder.UseMySql(dbconnectstring);
            options = builder.Options;

            DataLibrary.DataContext dataContext = new DataLibrary.DataContext(options);

            BatcodeService service = new BatcodeService(dataContext);

            var result = service.GetPrevBatcode("A18050012");

            Assert.True(result == "A18050011");
        }
    }
}
