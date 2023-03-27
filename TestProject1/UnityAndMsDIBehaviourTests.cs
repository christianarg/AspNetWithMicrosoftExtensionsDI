using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity;
using Unity.Microsoft.DependencyInjection;

namespace TestProject1
{
    [TestClass]
    public class UnityAndMsDIBehaviourTests
    {
        [TestMethod]
        public void UnityExtensionsBuildServiceProviderTest()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IMyInterface, MyClass>();
            unityContainer.RegisterType<IMyInterface, MyClassNamed>("paco");

            var services = new ServiceCollection();
            services.AddHttpClient();

            var serviceProvider = services.BuildServiceProvider(unityContainer);    // Al llamar a esto se hace la "magia". Lo que hay registrado en ServiceProvider lo registra también en Unity y vice versa

            AssertServiceProviderHasHttpClientFactory(serviceProvider);

            AssertUnityHasHttpClientFactory(unityContainer);

            var myInterfaceThroughServiceProvider = serviceProvider.GetService<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughServiceProvider!.GetType());

            var myInterfaceThroughUnity = unityContainer.Resolve<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughUnity!.GetType());

            var myInterfaceNamedThroughUnity = unityContainer.Resolve<IMyInterface>("paco");
            Assert.AreEqual(typeof(MyClassNamed), myInterfaceNamedThroughUnity!.GetType());
        }

        private static void AssertUnityHasHttpClientFactory(UnityContainer unityContainer)
        {
            var httpClientThroughUnity = unityContainer.Resolve<IHttpClientFactory>();
            Assert.IsNotNull(httpClientThroughUnity);
        }

        private static void AssertServiceProviderHasHttpClientFactory(IServiceProvider serviceProvider)
        {
            var httpClientThroughServiceProvider = serviceProvider.GetService<IHttpClientFactory>();
            Assert.IsNotNull(httpClientThroughServiceProvider);
        }

        /// <summary>
        /// Test que falla.
        /// si creamos contender / servicecollection y después calzamos los registers, no va
        /// </summary>
        [TestMethod]
        public void UnityExtensionsBuildServiceProviderAsiNovaTest()
        {
            var unityContainer = new UnityContainer();
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider(unityContainer);    // Al llamar a esto se hace la "magia". Lo que hay registrado en ServiceProvider lo registra también en Unity y vice versa

            unityContainer.RegisterType<IMyInterface, MyClass>();
            unityContainer.RegisterType<IMyInterface, MyClassNamed>("paco");
            services.AddHttpClient();

            var httpClientThroughServiceProvider = serviceProvider.GetService<IHttpClientFactory>();
            Assert.IsNull(httpClientThroughServiceProvider);    // Al haber llamado al buildserviceprovider ANTES de registrar, esto no se encuentra

            var myInterfaceThroughServiceProvider = serviceProvider.GetService<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughServiceProvider!.GetType()); // Esto sorprendentemente si que va, aparentemente si lo llamas después si que conecta Unity > MS.DI

            bool httpClientThroughUnityPeta = false;
            try
            {
                var httpClientThroughUnity = unityContainer.Resolve<IHttpClientFactory>();
            }
            catch (Exception)
            {
                httpClientThroughUnityPeta = true;
            }
            Assert.IsTrue(httpClientThroughUnityPeta);

            // Test en principio redundantes, lo que registramos por unity pues, lo obtenemos por unity
            var myInterfaceThroughUnity = unityContainer.Resolve<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughUnity!.GetType());

            var myInterfaceNamedThroughUnity = unityContainer.Resolve<IMyInterface>("paco");
            Assert.AreEqual(typeof(MyClassNamed), myInterfaceNamedThroughUnity!.GetType());
        }

        [TestMethod]
        public void ConHostBuilderBase()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                });
            var host = hostBuilder.Build();
            var serviceProvider = host.Services;
            AssertServiceProviderHasHttpClientFactory(serviceProvider);
        }

        [TestMethod]
        public void ConHostBuilder()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IMyInterface, MyClass>();
            unityContainer.RegisterType<IMyInterface, MyClassNamed>("paco");

            var hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                })
                .UseUnityServiceProvider(unityContainer);   // Esto aparentemente aplica DI solo en una durección Unity > ServiceProvider. A diferencia de services.BuildServiceProvider(unityContainer) que lo hace en ambas direcciones

            var host = hostBuilder.Build();
            var serviceProvider = host.Services;
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            Assert.IsNotNull(httpClientFactory);

            var myInterfaceThroughServiceProvider = serviceProvider.GetService<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughServiceProvider!.GetType());

            // Esta es la parte que no va sin llamar a buildserviceprovider
            bool httpClientThroughUnityPeta = false;
            try
            {
                var httpClientThroughUnity = unityContainer.Resolve<IHttpClientFactory>();
            }
            catch (Exception)
            {
                httpClientThroughUnityPeta = true;
            }
            Assert.IsTrue(httpClientThroughUnityPeta);
        }

        [TestMethod]
        public void ConHostBuilderBidireccional()
        {
            var unityContainer = new UnityContainer();

            unityContainer.RegisterType<IMyInterface, MyClass>();
            unityContainer.RegisterType<IMyInterface, MyClassNamed>("paco");

            var hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHttpClient();
                    services.BuildServiceProvider(unityContainer);  // Con esto + UseUnityServiceProvider tenemos la "bidireccionalidad"
                })
                .UseUnityServiceProvider(unityContainer);   // Esto aparentemente aplica DI solo en una durección Unity > ServiceProvider. A diferencia de services.BuildServiceProvider(unityContainer) que lo hace en ambas direcciones
            var host = hostBuilder.Build();
            var serviceProvider = host.Services;
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            Assert.IsNotNull(httpClientFactory);

            var myInterfaceThroughServiceProvider = serviceProvider.GetService<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughServiceProvider!.GetType());

            AssertUnityHasHttpClientFactory(unityContainer);
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