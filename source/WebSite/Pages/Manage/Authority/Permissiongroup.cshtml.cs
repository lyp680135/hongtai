namespace WarrantyManage.Pages.Authority
{
    using System.Collections.Generic;
    using System.Linq;
    using DataLibrary;
    using Models;

    public class PermissiongroupModel : AuthorizeModel
    {
        public PermissiongroupModel(DataContext db)
             : base(db)
        {
        }

        public List<MngPermissiongroup> List_PermissionGroup { get; set; }

        public void OnGet()
        {
            this.List_PermissionGroup = this.Db.MngPermissiongroup.OrderBy(c => c.Sequence).ToList();
        }
    }
}