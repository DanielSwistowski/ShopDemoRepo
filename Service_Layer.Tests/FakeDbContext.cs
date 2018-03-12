using DataAccessLayer;
using DataAccessLayer.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Layer.Tests
{
    public class FakeDbContext : IApplicationDbContext
    {
        public FakeDbContext()
        {
            ProductAttributes = new FakeProductAttributeSet();
            ProductAttributeValues = new FakeProductAttributeValuesSet();
            OrderDetails = new FakeOrderDetailsSet();
            Products = new FakeProductSet();
            Orders = new FakeOrderSet();
            Categories = new FakeCategoriesSet();
            ProductCategory = new FakeProductCategorySet();
            SearchFilters = new FakeSearchFiltersSet();
            Photos = new FakePhotosSet();
            ProductDetails = new FakeProductDetailsSet();
            ProductDiscounts = new FakeProductDiscountSet();
            ProductRates = new FakeProductRatesSet();
            Deliveries = new FakeDeliverySet();
            PayuData = new FakePayuOrderDataSet();
            SalesSummary = new FakeSaleSummaryDataSet();
        }

        public DbSet<Address> Address { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Delivery> Deliveries { get; set; }

        public DbSet<LockAccountReason> LockAccountReasons { get; set; }

        public DbSet<OrderDetails> OrderDetails { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<PayuOrderData> PayuData { get; set; }

        public DbSet<Photo> Photos { get; set; }

        public DbSet<ProductAttribute> ProductAttributes { get; set; }

        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }

        public DbSet<ProductCategory> ProductCategory { get; set; }

        public DbSet<ProductDetail> ProductDetails { get; set; }

        public DbSet<ProductDiscount> ProductDiscounts { get; set; }

        public DbSet<ProductRate> ProductRates { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<SearchFilter> SearchFilters { get; set; }

        public DbSet<SaleSummary> SalesSummary { get; set; }

        public DbEntityEntry Entry<T>(T entity) where T : class
        {
            return Entry(entity);
        }

        public void SaveChanges()
        { }

        public Task<int> SaveChangesAsync()
        {
            return Task.FromResult(0);
        }

        public void SetModified(object entity)
        { }

        DbSet<TEntity> IApplicationDbContext.Set<TEntity>()
        {
            if (typeof(TEntity) == typeof(ProductAttribute))
                return ProductAttributes as DbSet<TEntity>;

            if (typeof(TEntity) == typeof(Product))
                return Products as DbSet<TEntity>;

            if (typeof(TEntity) == typeof(OrderDetails))
                return OrderDetails as DbSet<TEntity>;

            if (typeof(TEntity) == typeof(Order))
                return Orders as DbSet<TEntity>;

            if (typeof(TEntity) == typeof(Category))
                return Categories as DbSet<TEntity>;

            if (typeof(TEntity) == typeof(ProductCategory))
                return ProductCategory as DbSet<TEntity>;

            throw new NullReferenceException("Please provide DbSet for selected entity if you try to use method from generic BaseService");
        }

        public void Dispose()
        { }
    }


    public class FakeCategoriesSet : FakeDbSet<Category>
    {
        public override Category Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.CategoryId == (int)keyValues.Single());
        }

        public override Task<Category> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeProductCategorySet : FakeDbSet<ProductCategory>
    {
        public override ProductCategory Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.ProductCategoryId == (int)keyValues.Single());
        }

        public override Task<ProductCategory> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeProductAttributeSet : FakeDbSet<ProductAttribute>
    {
        public override ProductAttribute Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.ProductAttributeId == (int)keyValues.Single());
        }

        public override Task<ProductAttribute> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeProductAttributeValuesSet : FakeDbSet<ProductAttributeValue>
    {
        public override ProductAttributeValue Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.ProductAttributeValueId == (int)keyValues.Single());
        }

        public override Task<ProductAttributeValue> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeOrderDetailsSet : FakeDbSet<OrderDetails>
    {
        public override OrderDetails Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.OrderDetailsId == (int)keyValues.Single());
        }

        public override Task<OrderDetails> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeOrderSet : FakeDbSet<Order>
    {
        public override Order Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.OrderId == (int)keyValues.Single());
        }

        public override Task<Order> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeProductSet : FakeDbSet<Product>
    {
        public override Product Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.ProductId == (int)keyValues.Single());
        }

        public override Task<Product> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeSearchFiltersSet : FakeDbSet<SearchFilter>
    {
        public override SearchFilter Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.ProductAttributeId == (int)keyValues.Single());
        }

        public override Task<SearchFilter> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakePhotosSet : FakeDbSet<Photo>
    {
        public override Photo Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.PhotoId == (int)keyValues.Single());
        }

        public override Task<Photo> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeProductDetailsSet : FakeDbSet<ProductDetail>
    {
        public override ProductDetail Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.ProductDetailId == (int)keyValues.Single());
        }

        public override Task<ProductDetail> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeProductRatesSet : FakeDbSet<ProductRate>
    {
        public override ProductRate Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.ProductRateId == (int)keyValues.Single());
        }

        public override Task<ProductRate> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeProductDiscountSet : FakeDbSet<ProductDiscount>
    {
        public override ProductDiscount Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.ProductId == (int)keyValues.Single());
        }

        public override Task<ProductDiscount> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeDeliverySet : FakeDbSet<Delivery>
    {
        public override Delivery Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.DeliveryId == (int)keyValues.Single());
        }

        public override Task<Delivery> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakePayuOrderDataSet : FakeDbSet<PayuOrderData>
    {
        public override PayuOrderData Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.OrderId == (int)keyValues.Single());
        }

        public override Task<PayuOrderData> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    public class FakeSaleSummaryDataSet : FakeDbSet<SaleSummary>
    {
        public override SaleSummary Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.SaleSummaryId == (int)keyValues.Single());
        }

        public override Task<SaleSummary> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }
}
