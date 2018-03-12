namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategories_ProductDetails_SearchFilters : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductCategory",
                c => new
                    {
                        ProductCategoryId = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductCategoryId)
                .ForeignKey("dbo.Category", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        ParentCategoryId = c.Int(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.ProductAttribute",
                c => new
                    {
                        ProductAttributeId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductAttributeId)
                .ForeignKey("dbo.Category", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.ProductAttributeValue",
                c => new
                    {
                        ProductAttributeValueId = c.Int(nullable: false, identity: true),
                        AttributeValue = c.String(),
                        ProductAttributeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductAttributeValueId)
                .ForeignKey("dbo.ProductAttribute", t => t.ProductAttributeId, cascadeDelete: true)
                .Index(t => t.ProductAttributeId);
            
            CreateTable(
                "dbo.SearchFilter",
                c => new
                    {
                        ProductAttributeId = c.Int(nullable: false),
                        FilterType = c.Int(),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductAttributeId)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.ProductAttribute", t => t.ProductAttributeId, cascadeDelete: true)
                .Index(t => t.ProductAttributeId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.ProductDetail",
                c => new
                    {
                        ProductDetailId = c.Int(nullable: false, identity: true),
                        DetailValue = c.String(),
                        DetailName = c.String(),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductDetailId)
                .ForeignKey("dbo.Product", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductDetail", "ProductId", "dbo.Product");
            DropForeignKey("dbo.ProductCategory", "ProductId", "dbo.Product");
            DropForeignKey("dbo.ProductCategory", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.SearchFilter", "ProductAttributeId", "dbo.ProductAttribute");
            DropForeignKey("dbo.SearchFilter", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.ProductAttributeValue", "ProductAttributeId", "dbo.ProductAttribute");
            DropForeignKey("dbo.ProductAttribute", "CategoryId", "dbo.Category");
            DropIndex("dbo.ProductDetail", new[] { "ProductId" });
            DropIndex("dbo.SearchFilter", new[] { "CategoryId" });
            DropIndex("dbo.SearchFilter", new[] { "ProductAttributeId" });
            DropIndex("dbo.ProductAttributeValue", new[] { "ProductAttributeId" });
            DropIndex("dbo.ProductAttribute", new[] { "CategoryId" });
            DropIndex("dbo.ProductCategory", new[] { "CategoryId" });
            DropIndex("dbo.ProductCategory", new[] { "ProductId" });
            DropTable("dbo.ProductDetail");
            DropTable("dbo.SearchFilter");
            DropTable("dbo.ProductAttributeValue");
            DropTable("dbo.ProductAttribute");
            DropTable("dbo.Category");
            DropTable("dbo.ProductCategory");
        }
    }
}
