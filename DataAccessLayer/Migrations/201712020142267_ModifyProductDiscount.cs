namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyProductDiscount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductDiscount", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.ProductDiscount", "StartDiscountJobId", c => c.String());
            AddColumn("dbo.ProductDiscount", "StopDiscountJobId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductDiscount", "StopDiscountJobId");
            DropColumn("dbo.ProductDiscount", "StartDiscountJobId");
            DropColumn("dbo.ProductDiscount", "Status");
        }
    }
}
