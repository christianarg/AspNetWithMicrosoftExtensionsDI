using AspNetWithMicrosoftExtensionsDI.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity;
using Unity.Microsoft.DependencyInjection;
// https://gist.github.com/davidfowl/563a602936426a18f67cd77088574e61
[assembly: PreApplicationStartMethod(typeof(AspNetWithMicrosoftExtensionsDI.MvcApplication), "InitModule")]

namespace AspNetWithMicrosoftExtensionsDI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void InitModule()
        {
            RegisterModule(typeof(ServiceScopeModule));
        }

        protected void Application_Start()
        {
            var unityContainer = new UnityContainer();
            var services = new ServiceCollection();
            ConfigureServices(services);
            ConfigureContainer(unityContainer);
            // No puedo estar seguro pero creo que el tema de BuildServiceProvider lo cogí de aqui https://stackoverflow.com/questions/50705817/using-unity-instead-of-asp-net-core-di-iservicecollection
            ServiceScopeModule.SetServiceProvider(services.BuildServiceProvider(unityContainer));

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyResolver.SetResolver(new ServiceProviderDependencyResolver());
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
            // services.AddTransient<HomeController>(); // no se porque estaba esto :)
            //services.AddTransient<IMyInterface, MyClass>();
        }

        private void ConfigureContainer(UnityContainer unityContainer)
        {
            unityContainer.RegisterType<IMyInterface, MyClass>();
        }
    }


    internal class ServiceScopeModule : IHttpModule
    {
        private static IServiceProvider _serviceProvider;

        public void Dispose()
        {

        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;
            context.EndRequest += Context_EndRequest;
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            if (context.Items[typeof(IServiceScope)] is IServiceScope scope)
            {
                scope.Dispose();
            }
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            context.Items[typeof(IServiceScope)] = _serviceProvider.CreateScope();
        }

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }

    internal class ServiceProviderDependencyResolver : IDependencyResolver
    {
        public object GetService(Type serviceType)
        {
            if (HttpContext.Current?.Items[typeof(IServiceScope)] is IServiceScope scope)
            {
                return scope.ServiceProvider.GetService(serviceType);
            }

            throw new InvalidOperationException("IServiceScope not provided");
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (HttpContext.Current?.Items[typeof(IServiceScope)] is IServiceScope scope)
            {
                return scope.ServiceProvider.GetServices(serviceType);
            }

            throw new InvalidOperationException("IServiceScope not provided");
        }
    }

    public interface IMyInterface
    {
        string Foo();
    }

    public class MyClass : IMyInterface
    {
        public string Foo()
        {
            return "Bar";
        }
    }
}
