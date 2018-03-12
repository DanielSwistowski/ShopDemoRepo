[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(ShopDemo.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(ShopDemo.App_Start.NinjectWebCommon), "Stop")]

namespace ShopDemo.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Service_Layer.Services;
    using DataAccessLayer.Models;
    using Microsoft.Owin.Security;
    using DataAccessLayer;
    using AutoMapper;
    using AutoMapperProfiles;
    using Ninject.Web.Common.WebHost;
    using Postal;
    using Hangfire;
    using NLog;
    using Areas.Admin;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                GlobalConfiguration.Configuration.UseNinjectActivator(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IApplicationDbContext>().To<ApplicationDbContext>().InRequestScope();
            kernel.Bind<ICustomUserStore<ApplicationUser, int>>().To<CustomUserStore>().WithConstructorArgument(new ApplicationDbContext());
            kernel.Bind<ApplicationUserManager>().ToSelf();
            kernel.Bind<ApplicationSignInManager>().ToSelf();
            kernel.Bind<IAuthenticationManager>().ToMethod(x => HttpContext.Current.GetOwinContext().Authentication);

            #region automapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductProfile());
                cfg.AddProfile(new ProductRateProfile());
                cfg.AddProfile(new CategoryProfile());
                cfg.AddProfile(new ProductAttributeProfile());
                cfg.AddProfile(new ProductAttributeValueProfile());
                cfg.AddProfile(new SearchFilterProfile());
                cfg.AddProfile(new PhotoProfile());
                cfg.AddProfile(new ProductDetailProfile());
                cfg.AddProfile(new ProductDiscountProfile());
                cfg.AddProfile(new ShoppingCartProfile());
                cfg.AddProfile(new UserProfile());
                cfg.AddProfile(new DeliveryProfile());
                cfg.AddProfile(new OrderProfile());
                cfg.AddProfile(new OrderDetailProfile());
                cfg.AddProfile(new PayuProfile());
                cfg.AddProfile(new StatisticsProfile());
            });
            var mapper = config.CreateMapper();
            kernel.Bind<MapperConfiguration>().ToConstant(config).InSingletonScope();
            kernel.Bind<IMapper>().ToConstant(mapper).InSingletonScope();
            #endregion

            kernel.Bind<IBackgroundJobClient>().To<BackgroundJobClient>();

            kernel.Bind<IPathProvider>().To<PathProvider>();
            kernel.Bind<IDateTimeProvider>().To<DateTimeProvider>();
            kernel.Bind<IProductService>().To<ProductService>();
            kernel.Bind<IPhotoFileManagement>().To<PhotoFileManagement>();
            kernel.Bind<IPhotoService>().To<PhotoService>();
            kernel.Bind<IProductRateService>().To<ProductRateService>();
            kernel.Bind<ICategoryService>().To<CategoryService>();
            kernel.Bind<IProductAttributeService>().To<ProductAttributeService>();
            kernel.Bind<IProductAttributeValueService>().To<ProductAttributeValueService>();
            kernel.Bind<ISearchFilterService>().To<SearchFilterService>();
            kernel.Bind<IProductDiscountService>().To<ProductDiscountService>();
            kernel.Bind<ICartService>().To<CartService>();
            kernel.Bind<IEmailService>().To<EmailService>();
            kernel.Bind<IDeliveryService>().To<DeliveryService>();
            kernel.Bind<IOrderService>().To<OrderService>();
            kernel.Bind<IStatisticsService>().To<StatisticsService>();
            kernel.Bind<ISalesSummaryService>().To<SalesSummaryService>();

            kernel.Bind<ILogger>().ToMethod(x => LogManager.GetCurrentClassLogger());
            kernel.Bind<ILogFileManagementService>().To<LogFileManagementService>();

            kernel.Bind<IPayuService>().To<PayuService>();

            kernel.Bind<IHangfireAutoCancelOrder>().To<HangfireAutoCancelOrder>();
            kernel.Bind<IHangfireRemovePhotoFiles>().To<HangfireRemovePhotoFiles>();
        }
    }
}