namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRemovedToOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "Removed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Order", "Removed");
        }
    }
}
