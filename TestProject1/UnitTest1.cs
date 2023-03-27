using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity;
using Unity.Microsoft.DependencyInjection;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void UnityExtensionsBuildServiceProviderTest()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IMyInterface, MyClass>();
            unityContainer.RegisterType<IMyInterface, MyClassNamed>("paco");

            var services = new ServiceCollection();
            services.AddHttpClient();

            var serviceProvider = services.BuildServiceProvider(unityContainer);    // Al llamar a esto se hace la "magia". Lo que hay registrado en ServiceProvider lo registra tambi�n en Unity y vice versa

            var httpClientThroughServiceProvider = serviceProvider.GetService<IHttpClientFactory>();
            Assert.IsNotNull(httpClientThroughServiceProvider);

            var httpClientThroughUnity = unityContainer.Resolve<IHttpClientFactory>();
            Assert.IsNotNull(httpClientThroughUnity);

            var myInterfaceThroughServiceProvider = serviceProvider.GetService<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughServiceProvider!.GetType());

            var myInterfaceThroughUnity = unityContainer.Resolve<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughUnity!.GetType());

            var myInterfaceNamedThroughUnity = unityContainer.Resolve<IMyInterface>("paco");
            Assert.AreEqual(typeof(MyClassNamed), myInterfaceNamedThroughUnity!.GetType());
        }

        /// <summary>
        /// Test que falla.
        /// si creamos contender / servicecollection y despu�s calzamos los registers, no va
        /// </summary>
        [TestMethod]
        public void UnityExtensionsBuildServiceProviderAsiNovaTest()
        {
            var unityContainer = new UnityContainer();
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider(unityContainer);    // Al llamar a esto se hace la "magia". Lo que hay registrado en ServiceProvider lo registra tambi�n en Unity y vice versa

            unityContainer.RegisterType<IMyInterface, MyClass>();
            unityContainer.RegisterType<IMyInterface, MyClassNamed>("paco");
            services.AddHttpClient();

            var httpClientThroughServiceProvider = serviceProvider.GetService<IHttpClientFactory>();
            Assert.IsNull(httpClientThroughServiceProvider);

            var myInterfaceThroughServiceProvider = serviceProvider.GetService<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughServiceProvider!.GetType()); // Esto si que va

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
            var httpClientFactory = host.Services.GetService<IHttpClientFactory>();
            Assert.IsNotNull(httpClientFactory);
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
                .UseUnityServiceProvider(unityContainer);   // Esto aparentemente aplica DI solo en una durecci�n Unity > ServiceProvider. A diferencia de services.BuildServiceProvider(unityContainer) que lo hace en ambas direcciones

            var host = hostBuilder.Build();
            var serviceProvider = host.Services;
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            Assert.IsNotNull(httpClientFactory);

            var myInterfaceThroughServiceProvider = serviceProvider.GetService<IMyInterface>();
            Assert.AreEqual(typeof(MyClass), myInterfaceThroughServiceProvider!.GetType());

            // Esta es la parte que no va sin llamar a buildserviceprovider
            //var httpClientThroughUnity = unityContainer.Resolve<IHttpClientFactory>();
            //Assert.IsNotNull(httpClientThroughUnity);
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