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

namespace AspNetWithMicrosoftExtensionsDI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static UnityContainer Container { get; private set; }

        protected void Application_Start()
        {
            var unityContainer = new UnityContainer();
            Container = unityContainer;
            var services = new ServiceCollection();
            ConfigureServices(services);
            ConfigureContainer(unityContainer);
            // No puedo estar seguro pero creo que el tema de BuildServiceProvider lo cogí de aqui https://stackoverflow.com/questions/50705817/using-unity-instead-of-asp-net-core-di-iservicecollection
            // La "gracia" es la llamada a este BuildServiceProvider que es parte de Unity.Microsoft.DependencyInjection, que crea un serviceProvider "enlazando" el contenedor de unity y el de DI
            services.BuildServiceProvider(unityContainer);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyResolver.SetResolver(new UnityDependencyResolver(DependencyResolver.Current));
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }

        private void ConfigureContainer(UnityContainer unityContainer)
        {
            unityContainer.RegisterType<IMyInterface, MyClass>();
            unityContainer.RegisterType<IMyInterface, MyClassNamed>("paco");
        }
    }

    // <summary>
    /// MVC biene preparado para poder aplicar DI en los controladores, 
    /// en la construccion del los controladores llama el IDependencyResolver que podemos sobre escribir para que use nuestro IocFactory.
    /// Unity tiene un implementacion, pero no acaba de ajustarnos a como tenemos montado el IocFactory.
    /// Asi no es necesario sobrescivir tod0 el DefaulControllerFactory i solo modificamos la parte que nos interesa.
    /// </summary>
    public class UnityDependencyResolver : IDependencyResolver
    {

        private readonly IDependencyResolver resolver;

        public UnityDependencyResolver(IDependencyResolver resolver)
        {
            this.resolver = resolver;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                if (serviceType.IsInterface)
                {
                    return MvcApplication.Container.IsRegistered(serviceType)
                        ? MvcApplication.Container.Resolve(serviceType)
                        : resolver.GetService(serviceType);
                }
                return MvcApplication.Container.Resolve(serviceType);
            }
            catch
            {
                return resolver.GetService(serviceType);
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                if (serviceType.IsInterface)
                {
                    return MvcApplication.Container.IsRegistered(serviceType)
                        ? MvcApplication.Container.ResolveAll(serviceType)
                        : resolver.GetServices(serviceType);
                }
                return MvcApplication.Container.ResolveAll(serviceType);
            }
            catch
            {
                return resolver.GetServices(serviceType);
            }
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

    public class MyClassNamed : IMyInterface
    {
        public string Foo()
        {
            return "BarNamed";
        }
    }
}
