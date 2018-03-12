namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDatesToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "CreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.Product", "DeletedFromOfferDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "DeletedFromOfferDate");
            DropColumn("dbo.Product", "CreatedAt");
        }
    }
}
