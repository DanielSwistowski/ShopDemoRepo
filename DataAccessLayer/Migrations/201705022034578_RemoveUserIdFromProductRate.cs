namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserIdFromProductRate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProductRate", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.ProductRate", new[] { "UserId" });
            DropColumn("dbo.ProductRate", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProductRate", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProductRate", "UserId");
            AddForeignKey("dbo.ProductRate", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
