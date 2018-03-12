namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDeliveryModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Delivery",
                c => new
                    {
                        DeliveryId = c.Int(nullable: false, identity: true),
                        Option = c.String(),
                        PaymentOption = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RealizationTimeInDays = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DeliveryId);
            
            AddColumn("dbo.Order", "DeliveryId", c => c.Int(nullable: false));
            CreateIndex("dbo.Order", "DeliveryId");
            AddForeignKey("dbo.Order", "DeliveryId", "dbo.Delivery", "DeliveryId", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Order", "DeliveryId", "dbo.Delivery");
            DropIndex("dbo.Order", new[] { "DeliveryId" });
            DropColumn("dbo.Order", "DeliveryId");
            DropTable("dbo.Delivery");
        }
    }
}
