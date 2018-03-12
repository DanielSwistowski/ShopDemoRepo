namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddPayuOrderData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PayuOrderData",
                c => new
                {
                    OrderId = c.Int(nullable: false),
                    PaymentStatus = c.String(),
                    PayuOrderId = c.String(),
                })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.Order", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.PayuOrderData", "OrderId", "dbo.Order");
            DropIndex("dbo.PayuOrderData", new[] { "OrderId" });
            DropTable("dbo.PayuOrderData");
        }
    }
}
