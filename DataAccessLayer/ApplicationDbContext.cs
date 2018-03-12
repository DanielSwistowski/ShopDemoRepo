using DataAccessLayer.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Threading.Tasks;
using System;

namespace DataAccessLayer
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; set; }
        DbSet<Photo> Photos { get; set; }
        DbSet<Address> Address { get; set; }
        DbSet<ProductRate> ProductRates { get; set; }
        DbSet<ProductDiscount> ProductDiscounts { get; set; }
        DbSet<Order> Orders { get; set; }
        DbSet<OrderDetails> OrderDetails { get; set; }
        DbSet<ProductDetail> ProductDetails { get; set; }
        DbSet<ProductAttribute> ProductAttributes { get; set; }
        DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<SearchFilter> SearchFilters { get; set; }
        DbSet<ProductCategory> ProductCategory { get; set; }
        DbSet<LockAccountReason> LockAccountReasons { get; set; }
        DbSet<Delivery> Deliveries { get; set; }
        DbSet<PayuOrderData> PayuData { get; set; }
        DbSet<SaleSummary> SalesSummary { get; set; }

        Task<int> SaveChangesAsync();
        void SaveChanges();

        void SetModified(object entity);

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbEntityEntry Entry<T>(T entity) where T : class;
        void Dispose();
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>, IApplicationDbContext
    {
        public ApplicationDbContext()
            : base("ShopDemoDb")
        {
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<ProductRate> ProductRates { get; set; }
        public virtual DbSet<ProductDiscount> ProductDiscounts { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetails> OrderDetails { get; set; }
        public virtual DbSet<ProductDetail> ProductDetails { get; set; }
        public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }
        public virtual DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<SearchFilter> SearchFilters { get; set; }
        public virtual DbSet<ProductCategory> ProductCategory { get; set; }
        public virtual DbSet<LockAccountReason> LockAccountReasons { get; set; }
        public virtual DbSet<Delivery> Deliveries { get; set; }
        public virtual DbSet<PayuOrderData> PayuData { get; set; }
        public virtual DbSet<SaleSummary> SalesSummary { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        Task<int> IApplicationDbContext.SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        DbEntityEntry IApplicationDbContext.Entry<T>(T entity)
        {
            return Entry(entity);
        }

        public void SetModified(object entity)
        {
            Entry(entity).State = EntityState.Modified;
        }

        void IApplicationDbContext.SaveChanges()
        {
            base.SaveChanges();
        }
    }
}