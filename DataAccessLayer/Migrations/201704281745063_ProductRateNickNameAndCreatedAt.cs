namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductRateNickNameAndCreatedAt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductRate", "NickName", c => c.String());
            AddColumn("dbo.ProductRate", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductRate", "CreatedAt");
            DropColumn("dbo.ProductRate", "NickName");
        }
    }
}
