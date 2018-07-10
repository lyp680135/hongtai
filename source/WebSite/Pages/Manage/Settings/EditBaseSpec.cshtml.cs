namespace WarrantyManage.Pages.Manage.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Models;

    public class EditBaseSpecModel : AuthorizeModel
    {
        public EditBaseSpecModel(DataLibrary.DataContext db)
         : base(db)
        {
        }

        public DataLibrary.BaseSpecifications Bs { get; set; }

        public DataLibrary.BaseProductClass Pm { get; set; }

        public bool IsSuccess { get; set; }

        public string Msg { get; set; }

        public int Status { get; set; }

        public DataLibrary.BaseProductMaterial Cz { get; set; }

        public void OnGet(int id)
        {
            var queryResult2 = this.Db.BaseProductClass.ToList();
            var queryResult3 = this.Db.BaseProductMaterial.ToList();
            this.ViewData["plist"] = queryResult2;
            this.ViewData["mlist"] = queryResult3;
            this.Bs = this.Db.BaseSpecifications.FirstOrDefault(c => c.Id == id);
            if (this.Bs != null)
            {
                this.Pm = this.Db.BaseProductClass.FirstOrDefault(p => p.Id == this.Bs.Classid);

                this.Cz = this.Db.BaseProductMaterial.FirstOrDefault(p => p.Id == this.Bs.Materialid);
            }
            else
            {
                this.RedirectToError();
            }

            this.Msg = string.Empty;
            this.IsSuccess = false;
        }

        public void OnPostFirst(int[] hiddenId, int classid, int materialid, string[] specname, string[] callname, double[] referpieceweight, int[] referpiececount, double[] referlength, double[] refermeterweight, double[] coldratio, double[] reverseratio)
        {
            List<DataLibrary.BaseSpecifications> list_BaseSpecifications = new List<DataLibrary.BaseSpecifications>();
            for (var i = 0; i < hiddenId.Length; i++)
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
                    this.Msg = "操作成功";
                    if (hiddenId[i] > 0)
                    {
                        var rs = this.Db.BaseSpecifications.FirstOrDefault(p => p.Id == hiddenId[i]);
                        if (rs != null)
                        {
                            rs.Classid = classid;
                            rs.Materialid = materialid;
                            rs.Callname = callname[i];
                            rs.Specname = specname[i];
                            rs.Referpieceweight = referpieceweight[i];
                            rs.Referpiececount = referpiececount[i];
                            rs.Referlength = referlength[i];
                            rs.Refermeterweight = refermeterweight[i];
                            rs.Coldratio = coldratio[i];
                            rs.Reverseratio = reverseratio[i];
                            this.Db.BaseSpecifications.Update(rs);
                            this.Db.SaveChanges();
                        }
                    }
                    else
                    {
                      var spec = new DataLibrary.BaseSpecifications()
                        {
                            Classid = classid,
                            Materialid = materialid,
                            Specname = specname[i],
                            Referpieceweight = referpieceweight[i],
                            Referpiececount = referpiececount[i],
                            Referlength = referlength[i],
                            Refermeterweight = refermeterweight[i],
                            Coldratio = coldratio[i],
                            Reverseratio = reverseratio[i],
                        };
                        this.Db.BaseSpecifications.Add(spec);
                        this.Db.SaveChanges();
                    }
                }
            }

            this.IsSuccess = true;
            var queryResult2 = this.Db.BaseProductClass.ToList();
            var queryResult3 = this.Db.BaseProductMaterial.ToList();
            this.ViewData["plist"] = queryResult2;
            this.ViewData["mlist"] = queryResult3;
            this.Bs = this.Db.BaseSpecifications.FirstOrDefault(c => c.Id == hiddenId[0]);
            this.Pm = this.Db.BaseProductClass.FirstOrDefault(p => p.Id == classid);
            this.Cz = this.Db.BaseProductMaterial.FirstOrDefault(p => p.Id == materialid);
        }
    }
}