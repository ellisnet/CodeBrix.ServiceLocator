# CodeBrix.ServiceLocator

A fully managed service-location abstraction library for .NET — a faithful port of [CommonServiceLocator](https://github.com/unitycontainer/commonservicelocator) 2.0.7 into the `CodeBrix.ServiceLocator` namespace, intended as a drop-in replacement for the `CommonServiceLocator` NuGet package. It provides a shared interface over IoC containers and service locators, so an application can resolve services indirectly without taking a hard reference on any specific container.
CodeBrix.ServiceLocator has no dependencies other than .NET, and is provided as a .NET 10 library and associated `CodeBrix.ServiceLocator.MsplLicenseForever` NuGet package.

CodeBrix.ServiceLocator supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## CodeBrix.ServiceLocator supports:

* **`IServiceLocator`** — the shared abstraction for resolving services by type or by type + name, including `GetInstance`, `GetInstance<TService>`, `GetAllInstances`, and `GetAllInstances<TService>`.
* **`ServiceLocator`** — a static ambient-container accessor (`ServiceLocator.Current`, `ServiceLocator.SetLocatorProvider`, `ServiceLocator.IsLocationProviderSet`).
* **`ServiceLocatorImplBase`** — an abstract base that implements the full `IServiceLocator` surface in terms of two `DoGetInstance` / `DoGetAllInstances` template methods, so a container adapter only needs to implement two methods.
* **`ActivationException`** — the standard exception raised when service resolution fails (resolution errors thrown by the underlying container are wrapped in it).
* **`ServiceLocatorProvider`** — the delegate used to supply the ambient container to `ServiceLocator`.

The public type-for-type surface matches CommonServiceLocator 2.0.7, so migrating is largely a namespace change from `CommonServiceLocator` to `CodeBrix.ServiceLocator`.

## Sample Code

### Implement a container adapter and use the ambient accessor

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.ServiceLocator;

// 1. Adapt your IoC container by deriving from ServiceLocatorImplBase.
public sealed class MyContainerAdapter : ServiceLocatorImplBase
{
    protected override object DoGetInstance(Type serviceType, string key)
        => /* resolve a single service from your container */;

    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        => /* resolve all registered services of serviceType */;
}

// 2. Register it as the ambient container.
ServiceLocator.SetLocatorProvider(() => new MyContainerAdapter());

// 3. Resolve services anywhere via the ambient accessor.
var service = ServiceLocator.Current.GetInstance<IMyService>();
```

## License

The project is licensed under the MS-PL License. see: https://en.wikipedia.org/wiki/Microsoft_Public_License

CodeBrix.ServiceLocator is a derivative work of CommonServiceLocator (MS-PL). Its copyright/attribution notice and the full MS-PL license text are reproduced in [THIRD-PARTY-NOTICES.txt](./THIRD-PARTY-NOTICES.txt).
