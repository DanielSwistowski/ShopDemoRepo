namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderRealizationDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "OrderRealizationDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Order", "OrderRealizationDate");
        }
    }
}
