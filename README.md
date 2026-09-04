# CodeBrix.ServiceLocator

A fully managed service-location abstraction library for .NET. It provides a shared interface over IoC containers and service locators, so an application can resolve services indirectly — by type, or by type plus a string key — without taking a hard reference on any specific container. The library is the abstraction only: it contains no container of its own, performs no registration and creates no objects; you supply a container adapter, and this library defines the shape that adapter presents to the rest of the application.

CodeBrix.ServiceLocator has no dependencies other than .NET, and is provided as a .NET Standard 2.0 / .NET 10 library and associated `CodeBrix.ServiceLocator.MsplLicenseForever` NuGet package.

CodeBrix.ServiceLocator supports applications and assemblies that target Microsoft .NET version 10.0 and later, and — via its .NET Standard 2.0 target — downlevel consumers such as Roslyn source generators and analyzers.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## Installation

```
dotnet add package CodeBrix.ServiceLocator.MsplLicenseForever
```

Note that the NuGet package ID and the namespace are different - there is no package named plain `CodeBrix.ServiceLocation`:

* NuGet package ID: `CodeBrix.ServiceLocator.MsplLicenseForever`
* Assembly: `CodeBrix.ServiceLocator`
* Primary namespace: `CodeBrix.ServiceLocation` - i.e. `using CodeBrix.ServiceLocation;`

**Read that difference carefully — it is the one thing that most often goes wrong here.** The package ID and the assembly end in **ServiceLocat*or***, while the namespace you write in a `using` directive ends in **ServiceLocat*ion***. `using CodeBrix.ServiceLocator;` does not compile — it fails with CS0246, because no namespace of that name exists.

The namespace is spelled that way deliberately. A namespace named `CodeBrix.ServiceLocator` would be a member of the enclosing `CodeBrix` namespace, so in any consumer whose own namespace begins with `CodeBrix.` the bare name `ServiceLocator` would bind to the *namespace* and hide the *class* of the same name. With `CodeBrix.ServiceLocation`, `ServiceLocator.Current` resolves correctly from any namespace.

The library has no NuGet dependencies — it uses only the .NET base class library — and there are no native assets and no platform restrictions. XML documentation (IntelliSense) ships alongside the assembly.

## CodeBrix.ServiceLocator supports:

* **`IServiceLocator`** — the shared abstraction for resolving services by type or by type + name, including `GetInstance`, `GetInstance<TService>`, `GetAllInstances`, and `GetAllInstances<TService>`. It extends `System.IServiceProvider`.
* **`ServiceLocator`** — a static ambient-container accessor (`ServiceLocator.Current`, `ServiceLocator.SetLocatorProvider`, `ServiceLocator.IsLocationProviderSet`).
* **`ServiceLocatorImplBase`** — an abstract base that implements the full `IServiceLocator` surface in terms of two `DoGetInstance` / `DoGetAllInstances` template methods, so a container adapter only needs to implement two methods.
* **`ActivationException`** — the standard exception raised when service resolution fails (resolution errors thrown by the underlying container are wrapped in it).
* **`ServiceLocatorProvider`** — the delegate used to supply the ambient container to `ServiceLocator`.

## Sample Code

### Implement a container adapter and use the ambient accessor

```csharp
using System;
using System.Collections.Generic;
using CodeBrix.ServiceLocation;

// 1. Adapt your IoC container by deriving from ServiceLocatorImplBase.
public sealed class MyContainerAdapter : ServiceLocatorImplBase
{
    private readonly IMyContainer _container;

    public MyContainerAdapter(IMyContainer container)
    {
        _container = container;
    }

    protected override object DoGetInstance(Type serviceType, string key)
    {
        // Resolve a single service from your container.
        return _container.Resolve(serviceType, key);
    }

    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
    {
        // Resolve all registered services of serviceType.
        return _container.ResolveAll(serviceType);
    }
}

// 2. Register the adapter as the ambient container, once, at startup.
//    Create the adapter ONCE and have the provider hand back that same
//    instance: the delegate is invoked on every read of ServiceLocator.Current,
//    so `() => new MyContainerAdapter(...)` would build a new adapter on every
//    single resolution.
var locator = new MyContainerAdapter(myContainer);
ServiceLocator.SetLocatorProvider(() => locator);

// 3. Resolve services anywhere via the ambient accessor.
var service = ServiceLocator.Current.GetInstance<IMyService>();
var named = ServiceLocator.Current.GetInstance<IMyService>("secondary");
var all = ServiceLocator.Current.GetAllInstances<IMyService>();
```

### Clear the ambient container

```csharp
using CodeBrix.ServiceLocation;

// The provider is process-wide static state. Tests, and applications that
// tear down and rebuild their container, should clear it explicitly.
ServiceLocator.SetLocatorProvider(null);

if (!ServiceLocator.IsLocationProviderSet)
{
    // ServiceLocator.Current would now throw InvalidOperationException.
}
```

## Documentation

The NuGet package includes `AGENT-README.txt`, a complete API reference and usage guide written for AI coding agents - point your agent at that file when it is writing code against this library.

Additional sample code and usage examples are available in the `CodeBrix.ServiceLocator.Tests` project:
https://github.com/ellisnet/CodeBrix.ServiceLocator/tree/main/tests/CodeBrix.ServiceLocator.Tests

## License

CodeBrix.ServiceLocator is licensed under the Microsoft Public License (Ms-PL) - see the
[LICENSE](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/LICENSE) file.

For licensing and provenance information about the open source code included in
this package, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/THIRD-PARTY-NOTICES.txt).
