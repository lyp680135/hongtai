namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Storage;
    using MySql.Data.MySqlClient;

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions opt)
            : base(opt)
        {
        }

        public virtual DbSet<PdQualityProductPreset> PdQualityProductPreset { get; set; }

        public virtual DbSet<SiteLink> SiteLink { get; set; }

        public virtual DbSet<BaseGbProduction> BaseGbProduction { get; set; }

        public virtual DbSet<BaseMobileCode> BaseMobileCode { get; set; }

        public virtual DbSet<BaseProductClass> BaseProductClass { get; set; }

        public virtual DbSet<BaseProductGbClass> BaseProductGbClass { get; set; }

        public virtual DbSet<BaseProductMaterial> BaseProductMaterial { get; set; }

        public virtual DbSet<BaseQualityStandard> BaseQualityStandard { get; set; }

        public virtual DbSet<BaseSpecifications> BaseSpecifications { get; set; }

        public virtual DbSet<MngAdmin> MngAdmin { get; set; }

        public virtual DbSet<MngDepartmentclass> MngDepartmentclass { get; set; }

        public virtual DbSet<MngMenuclass> MngMenuclass { get; set; }

        public virtual DbSet<MngPermissiongroup> MngPermissiongroup { get; set; }

        public virtual DbSet<MngPermissiongroupset> MngPermissiongroupset { get; set; }

        public virtual DbSet<MngPermissionpersonset> MngPermissionpersonset { get; set; }

        public virtual DbSet<MngSetting> MngSetting { get; set; }

        public virtual DbSet<PdBatcode> PdBatcode { get; set; }

        public virtual DbSet<PdProduct> PdProduct { get; set; }

        public virtual DbSet<PdQRCodeAuth> PdQRCodeAuth { get; set; }

        public virtual DbSet<PdQRCodePrintedLog> PdQRCodePrintedLog { get; set; }

        public virtual DbSet<PdQuality> PdQuality { get; set; }

        public virtual DbSet<PdStockOut> PdStockOut { get; set; }

        public virtual DbSet<PdWorkshop> PdWorkshop { get; set; }

        public virtual DbSet<PdWorkshopTeam> PdWorkshopTeam { get; set; }

        public virtual DbSet<PdWorkshopTeamLog> PdWorkshopTeamLog { get; set; }

        public virtual DbSet<SalePrintlog> SalePrintlog { get; set; }

        public virtual DbSet<SalePrintLogDetail> SalePrintLogDetail { get; set; }

        public virtual DbSet<SaleSeller> SaleSeller { get; set; }

        public virtual DbSet<SaleSellerAuth> SaleSellerAuth { get; set; }

        public virtual DbSet<SaleSellerAuthDetail> SaleSellerAuthDetail { get; set; }

        public virtual DbSet<SiteSinglePage> SiteSinglePage { get; set; }

        public virtual DbSet<SiteBasic> SiteBasic { get; set; }

        public virtual DbSet<SiteModelColumn> SiteModelColumn { get; set; }

        public virtual DbSet<SiteModel> SiteModel { get; set; }

        public virtual DbSet<SiteCategory> SiteCategory { get; set; }

        public virtual DbSet<PdqualityPdSmeltCode> PdSmeltCode { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 建索引
            modelBuilder.Entity<MngAdmin>(e =>
            {
                e.HasIndex(x => x.Id);
            });
            modelBuilder.Entity<MngMenuclass>(e =>
            {
                e.HasIndex(x => x.Id);
            });
            modelBuilder.Entity<BaseMobileCode>(e =>
            {
                e.HasIndex(x => x.Mobile);
                e.HasIndex(x => x.Sendtime);
            });
            modelBuilder.Entity<PdQRCodePrintedLog>(e =>
            {
                e.HasIndex(x => x.SpecId);
                e.HasIndex(x => x.WorkshopId);
                e.HasIndex(x => x.Createtime);
            });
        }
    }
}
