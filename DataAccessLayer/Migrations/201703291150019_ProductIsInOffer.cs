namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductIsInOffer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "IsInOffer", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "IsInOffer");
        }
    }
}
