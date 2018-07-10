namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class TestModel : AuthorizeModel
    {
        public TestModel(DataLibrary.DataContext db)
          : base(db)
        {
        }

        public bool IsSuccess { get; set; }

        public string Msg { get; set; }

        public void OnGet()
        {
            var query = from c in this.Db.BaseSpecifications select c;
            var query2 = from p in this.Db.BaseProductClass select p;
            var query3 = from m in this.Db.BaseProductMaterial select m;

            var queryResult = query.OrderBy(p => p.Id).ToList();
            var queryResult2 = query2.OrderBy(p => p.Id).ToList();
            var queryResult3 = query3.OrderBy(p => p.Id).ToList();

            this.Msg = string.Empty;
            this.ViewData["list"] = queryResult;
            this.ViewData["plist"] = queryResult2;
            this.ViewData["mlist"] = queryResult3;
            this.IsSuccess = false;
        }

        public void OnPostFirst(int materialid, string[] specname, string[] callname, double[] referpieceweight, int[] referpiececount, double[] refermeterweight, double[] referlength, double[] coldratio, double[] reverseratio)
        {
            var query = from c in this.Db.BaseSpecifications select c;
            var query2 = from p in this.Db.BaseProductClass select p;
            var query3 = from m in this.Db.BaseProductMaterial select m;

            var queryResult = query.OrderBy(p => p.Id).ToList();
            var queryResult2 = query2.OrderBy(p => p.Id).ToList();
            var queryResult3 = query3.OrderBy(p => p.Id).ToList();

            this.ViewData["list"] = queryResult;
            this.ViewData["plist"] = queryResult2;
            this.ViewData["mlist"] = queryResult3;
            List<DataLibrary.BaseSpecifications> list_baseSpec = new List<DataLibrary.BaseSpecifications>();
            var classid = this.Db.BaseProductMaterial.FirstOrDefault(f => f.Id == materialid).Classid;
            this.Msg = "操作成功";
            for (var i = 0; i < specname.Length; i++)
            {
                var tempClass = this.Db.BaseSpecifications.FirstOrDefault(c => c.Classid == classid
                                                                        && c.Materialid == materialid
                                                                        && c.Callname == callname[i]);
                if (tempClass != null)
                {
                    this.Msg += callname[i] + ",";
                }
                else
                {
                    var spec = new DataLibrary.BaseSpecifications()
                    {
                        Classid = classid,
                        Materialid = materialid,
                        Specname = specname[i],
                        Callname = callname[i],
                        Referpieceweight = referpieceweight[i],
                        Referpiececount = referpiececount[i],
                        Referlength = referlength[i],
                        Refermeterweight = refermeterweight[i],
                        Coldratio = coldratio[i],
                        Reverseratio = reverseratio[i]
                    };

                    this.IsSuccess = true;
                    this.Db.BaseSpecifications.Add(spec);
                    this.Db.SaveChanges();
                }
            }
        }
    }
}