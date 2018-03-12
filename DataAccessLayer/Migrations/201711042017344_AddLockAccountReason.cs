namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLockAccountReason : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LockAccountReason",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        LockReason = c.String(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            AddColumn("dbo.AspNetUsers", "AccountIsEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LockAccountReason", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.LockAccountReason", new[] { "UserId" });
            DropColumn("dbo.AspNetUsers", "AccountIsEnabled");
            DropTable("dbo.LockAccountReason");
        }
    }
}
