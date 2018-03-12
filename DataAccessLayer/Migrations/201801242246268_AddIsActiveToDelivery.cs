namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsActiveToDelivery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Delivery", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Delivery", "IsActive");
        }
    }
}
