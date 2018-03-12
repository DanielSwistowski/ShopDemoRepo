namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSalesSummaryModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SaleSummary",
                c => new
                    {
                        SaleSummaryId = c.Int(nullable: false, identity: true),
                        MonthName = c.String(),
                        Year = c.Int(nullable: false),
                        Summary = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.SaleSummaryId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SaleSummary");
        }
    }
}
