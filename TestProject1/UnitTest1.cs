using Microsoft.Extensions.DependencyInjection;
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

            var serviceProvider = services.BuildServiceProvider(unityContainer);    // Al llamar a esto se hace la "magia". Lo que hay registrado en ServiceProvider lo registra también en Unity y vice versa

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