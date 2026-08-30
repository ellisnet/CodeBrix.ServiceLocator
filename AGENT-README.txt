================================================================================
AGENT-README: CodeBrix.ServiceLocator
A Guide for AI Coding Agents — CONSUMING the
CodeBrix.ServiceLocator.MsplLicenseForever NuGet package
================================================================================


********************************************************************************
**                                                                            **
**   BREAKING CHANGE -- THE NAMESPACE IS NOW  CodeBrix.ServiceLocation        **
**                                                                            **
********************************************************************************

  READ THIS BEFORE WRITING ANY `using` FOR THIS PACKAGE.

  The namespace changed from `CodeBrix.ServiceLocator` to
  `CodeBrix.ServiceLocation`. Versions through 1.0.242.982 used the OLD name;
  every later version uses the NEW one.

      using CodeBrix.ServiceLocation;     // CORRECT -- current package
      using CodeBrix.ServiceLocation;      // WRONG   -- will not compile

  NOTHING ELSE CHANGED. Same five public types, same members, same behavior,
  same PackageId, same assembly name. Upgrading is a one-line find/replace of
  the `using` directive in each file that has one.

  WHY IT CHANGED. The old namespace collided with its own main class for a
  whole category of consumers. `CodeBrix.ServiceLocator` was a NAMESPACE and
  therefore a member of the enclosing `CodeBrix` namespace, so in any code
  whose own namespace began with `CodeBrix.` the bare name `ServiceLocator`
  bound to the namespace and hid the CLASS of the same name:

      namespace CodeBrix.MyApp
      {
          // error CS0234: the type or namespace name 'Current' does not
          // exist in the namespace 'CodeBrix.ServiceLocator'
          var c = ServiceLocator.Current;
      }

  That broke the package's core promise of being a drop-in replacement for
  CommonServiceLocator, and it could not be worked around cleanly: a
  using-alias at the top of the file does NOT help, because an
  enclosing-namespace member outranks a compilation-unit using-alias. Every
  CodeBrix-family consumer would have needed an alias declared inside its
  namespace body. Renaming the namespace fixes it once, for everyone.

  `CodeBrix.ServiceLocation` is not a member-name match for any public type,
  so `using CodeBrix.ServiceLocation;` followed by `ServiceLocator.Current`
  now resolves correctly from ANY namespace, CodeBrix.* included.

  VERSIONING CANNOT WARN YOU. This repository uses date-stamped versions
  (1.<years>.<day-of-year>.<minute-of-day>) where the major is pinned to 1, so
  major/minor do NOT signal API compatibility and this break does NOT appear
  as a major-version bump. Pin deliberately, and expect a compile error rather
  than a version warning if you upgrade across the boundary.

  DO NOT "FIX" THIS BY RENAMING THE NAMESPACE BACK. The test project contains
  tests/CodeBrix.ServiceLocator.Tests/NamespaceCollisionGuardTests.cs, which
  lives in an unrelated CodeBrix.* namespace specifically so that reintroducing
  the collision fails the build.

********************************************************************************


OVERVIEW
========

CodeBrix.ServiceLocator is a shared abstraction over IoC containers and service
locators. It lets an application resolve services indirectly — by type, or by
type plus a string key — without taking a hard reference on any specific
container. The package contains the abstraction only: it has no container of its
own, performs no registration, and creates no objects. You (or a library you
consume) supply a container adapter; this package defines the shape that adapter
presents to the rest of the application.

Five public types make up the whole surface:

  IServiceLocator          the abstraction (extends System.IServiceProvider)
  ServiceLocatorImplBase   abstract base that implements all of IServiceLocator
                           in terms of two template methods
  ServiceLocator           static ambient-container accessor
  ServiceLocatorProvider   delegate that supplies the ambient container
  ActivationException      the resolution-failure exception

Target frameworks: netstandard2.0 and net10.0. The library is fully managed,
has no NuGet dependencies, and the assembly is marked [CLSCompliant(true)].
The netstandard2.0 target exists so downlevel and Roslyn source-generator /
analyzer projects can consume the abstraction; it was added after 1.0.242.982,
which shipped net10.0-only.

Provenance: this is a faithful port of CommonServiceLocator 2.0.7 into the
`CodeBrix.ServiceLocation` namespace, intended as a drop-in replacement for the
CommonServiceLocator NuGet package. The public surface is type-for-type
identical and the namespace is deliberately flat, so migration is a namespace
change. Do NOT write `using CommonServiceLocator;` in code that consumes this
package, and do not reference the upstream package alongside it — the type names
are the same in both, so having both in scope produces ambiguous-reference
compiler errors.

The namespace is `CodeBrix.ServiceLocation` and NOT `CodeBrix.ServiceLocator`,
deliberately: a namespace of the latter name would hide the `ServiceLocator`
class from every `CodeBrix.*` consumer. See the breaking-change notice at the
top of this file.


INSTALLATION
============

NuGet PackageId:   CodeBrix.ServiceLocator.MsplLicenseForever

  dotnet add package CodeBrix.ServiceLocator.MsplLicenseForever

NuGet dependencies: none. The library uses only the .NET base class library
(System.IServiceProvider, System.Exception, IEnumerable<T>).

License: MS-PL. The package is published under the SPDX expression MS-PL; the
upstream CommonServiceLocator attribution is reproduced in the packaged
THIRD-PARTY-NOTICES.txt.

Requirements: .NET 10, or any framework compatible with netstandard2.0. No
native libraries, no platform-specific code, no OS restrictions — the package
runs anywhere those run, including trimmed and AOT-published applications
(nothing in it uses reflection; the types your adapter resolves may, but that
is your container's concern).

Assembly name: `CodeBrix.ServiceLocator`. Namespace: `CodeBrix.ServiceLocation`.
These deliberately DIFFER — the assembly, the repository and the PackageId keep
the "ServiceLocator" spelling, while the namespace uses "ServiceLocation" to
avoid hiding the `ServiceLocator` class from `CodeBrix.*` consumers. You write
the NAMESPACE in `using` directives, so it is `using CodeBrix.ServiceLocation;`.
The `.MsplLicenseForever` suffix exists only on the NuGet PackageId, for license
disambiguation across the CodeBrix family; it never appears in code.


KEY NAMESPACES / USINGS
=======================

    using CodeBrix.ServiceLocation;

That single namespace holds every public type. There are no sub-namespaces —
the flat layout is what preserves type-for-type drop-in compatibility. Note the
spelling: ServiceLocatION for the namespace, ServiceLocatOR for the class.

A container adapter normally also needs:

    using System;                      // Type
    using System.Collections.Generic;  // IEnumerable<object>

Common combination in an adapter file:

    using System;
    using System.Collections.Generic;
    using CodeBrix.ServiceLocation;


CORE API REFERENCE
==================

IServiceLocator : IServiceProvider
----------------------------------

    public interface IServiceLocator : IServiceProvider
    {
        object                GetInstance(Type serviceType);
        object                GetInstance(Type serviceType, string key);
        IEnumerable<object>   GetAllInstances(Type serviceType);
        TService              GetInstance<TService>();
        TService              GetInstance<TService>(string key);
        IEnumerable<TService> GetAllInstances<TService>();
    }

  * Resolve by type, or by type plus a string key (the name the object was
    registered with in the underlying container).
  * Because it extends System.IServiceProvider, an IServiceLocator can be
    handed to any API that accepts IServiceProvider; that inherited member is
    `object GetService(Type serviceType)`.
  * Every member is documented as throwing ActivationException when resolution
    fails.

ServiceLocator (static ambient accessor)
----------------------------------------

    public static class ServiceLocator
    {
        public static IServiceLocator Current { get; }
        public static void SetLocatorProvider(ServiceLocatorProvider newProvider);
        public static bool IsLocationProviderSet { get; }
    }

  * `Current` invokes the registered ServiceLocatorProvider delegate and returns
    what it returns. It is a property, and the delegate is invoked on EVERY get.
  * If no provider has been set, `Current` throws InvalidOperationException with
    the message " ServiceLocationProvider must be set." (the upstream message,
    leading space included). It never returns null on its own — a provider that
    returns null makes `Current` return null.
  * `SetLocatorProvider` stores the delegate; passing null clears it. There is
    no separate Reset/Clear method.
  * `IsLocationProviderSet` is simply "a provider delegate is currently stored";
    it does not call the delegate and says nothing about what the delegate will
    return.
  * The stored provider is a plain private static field — process-wide, not
    thread-local, not AsyncLocal, and not synchronized.

ServiceLocatorProvider (delegate)
---------------------------------

    public delegate IServiceLocator ServiceLocatorProvider();

  Supplies the ambient container to the static ServiceLocator accessor. Usually
  a lambda over a field holding one adapter instance.

ServiceLocatorImplBase : IServiceLocator (abstract)
---------------------------------------------------

The base class you derive from to adapt a container. It implements the entire
IServiceLocator surface — every member is `virtual`, so overriding is possible
but rarely needed — in terms of two abstract template methods:

    public abstract class ServiceLocatorImplBase : IServiceLocator
    {
        public virtual object                GetService(Type serviceType);
        public virtual object                GetInstance(Type serviceType);
        public virtual object                GetInstance(Type serviceType, string key);
        public virtual IEnumerable<object>   GetAllInstances(Type serviceType);
        public virtual TService              GetInstance<TService>();
        public virtual TService              GetInstance<TService>(string key);
        public virtual IEnumerable<TService> GetAllInstances<TService>();

        protected abstract object              DoGetInstance(Type serviceType, string key);
        protected abstract IEnumerable<object> DoGetAllInstances(Type serviceType);

        protected virtual string FormatActivationExceptionMessage(
            Exception actualException, Type serviceType, string key);
        protected virtual string FormatActivateAllExceptionMessage(
            Exception actualException, Type serviceType);
    }

Exact behaviour of the implemented members — this is the contract your two
overrides must satisfy:

  * GetService(serviceType)          -> GetInstance(serviceType, null)
  * GetInstance(serviceType)         -> GetInstance(serviceType, null)
  * GetInstance(serviceType, key)    -> DoGetInstance(serviceType, key) inside a
                                        try/catch; ANY exception is caught and
                                        rethrown as ActivationException whose
                                        InnerException is the original.
  * GetAllInstances(serviceType)     -> DoGetAllInstances(serviceType) inside the
                                        same try/catch, using
                                        FormatActivateAllExceptionMessage.
  * GetInstance<TService>()          -> (TService)GetInstance(typeof(TService), null)
  * GetInstance<TService>(key)       -> (TService)GetInstance(typeof(TService), key)
  * GetAllInstances<TService>()      -> a C# iterator that walks
                                        GetAllInstances(typeof(TService)) and
                                        casts each element to TService.

So a container adapter implements exactly two methods. `key` is null whenever
the caller did not ask for a named registration — treat null as "the default
registration for this type".

Default exception messages (both overridable):

    FormatActivationExceptionMessage:
      Activation error occurred while trying to get instance of type
      {serviceType.Name}, key "{key}"

    FormatActivateAllExceptionMessage:
      Activation error occurred while trying to get all instances of type
      {serviceType.Name}

Note that neither default message includes the underlying exception's own
message, even though the actual exception is passed in — override these if you
want the container's diagnostics in the message text rather than only in
InnerException.

ActivationException : Exception
-------------------------------

    public class ActivationException : Exception
    {
        public ActivationException();
        public ActivationException(string message);
        public ActivationException(string message, Exception innerException);
    }

  The standard resolution-failure exception. ServiceLocatorImplBase raises it
  with the message-plus-inner-exception constructor, so the container's own
  exception is always reachable through `InnerException`. It carries no extra
  members beyond Exception. The legacy [Serializable]
  (SerializationInfo, StreamingContext) constructor from the upstream net40
  build is intentionally absent.


ERROR MODEL
===========

  * All failures raised through a ServiceLocatorImplBase-derived adapter arrive
    as ActivationException. `catch (ActivationException ex)` is the one catch a
    consumer needs; `ex.InnerException` holds the container's exception.
  * "Not registered" is not distinguished from "registered but failed to
    construct" — both are whatever your adapter's container throws, wrapped.
    There is no TryGetInstance / TryResolve and no null-returning overload.
  * An unset ambient provider is a different failure: InvalidOperationException
    from ServiceLocator.Current, not ActivationException.
  * Casts performed by the generic overloads happen OUTSIDE the wrapping
    try/catch, so a container that returns an object of the wrong type produces
    a plain InvalidCastException.


COMPLETE EXAMPLES
=================

Example 1 — a container adapter, end to end
-------------------------------------------

A self-contained adapter over a trivial registry. Substitute the two method
bodies with calls into the real container (Autofac, Unity, DryIoc, ...); the
shape does not change.

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CodeBrix.ServiceLocation;

    namespace MyApp.Composition;

    public sealed class MyContainerLocator : ServiceLocatorImplBase
    {
        private readonly Dictionary<Type, List<KeyValuePair<string, object>>> _registry
            = new Dictionary<Type, List<KeyValuePair<string, object>>>();

        public MyContainerLocator Register(Type serviceType, string key, object instance)
        {
            if (!_registry.TryGetValue(serviceType, out List<KeyValuePair<string, object>> list))
            {
                list = new List<KeyValuePair<string, object>>();
                _registry[serviceType] = list;
            }
            list.Add(new KeyValuePair<string, object>(key, instance));
            return this;
        }

        //Called for GetInstance/GetService; key is null for unnamed requests.
        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (!_registry.TryGetValue(serviceType, out List<KeyValuePair<string, object>> list))
            {
                throw new InvalidOperationException(
                    "No registration for " + serviceType.FullName);
            }

            return list.First(kv => kv.Key == key).Value;
        }

        //Called for GetAllInstances; return an already-materialized sequence so
        //that failures happen inside the base class's try/catch.
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return _registry.TryGetValue(serviceType, out List<KeyValuePair<string, object>> list)
                ? list.Select(kv => kv.Value).ToList()
                : (IEnumerable<object>)Array.Empty<object>();
        }
    }

Example 2 — publishing the adapter as the ambient container
-----------------------------------------------------------

Build the adapter ONCE at startup and hand the same instance out, because the
provider delegate runs on every ServiceLocator.Current access.

    using CodeBrix.ServiceLocation;

    namespace MyApp.Composition;

    public static class AppComposition
    {
        private static MyContainerLocator _locator;

        public static void Initialize()
        {
            _locator = new MyContainerLocator()
                .Register(typeof(IClock), null, new SystemClock())
                .Register(typeof(IGreeter), "friendly", new FriendlyGreeter())
                .Register(typeof(IGreeter), "terse", new TerseGreeter());

            //The lambda closes over the single instance - do not "new" here.
            ServiceLocator.SetLocatorProvider(() => _locator);
        }

        public static void Shutdown()
        {
            //Clearing the ambient provider: pass null.
            ServiceLocator.SetLocatorProvider(null);
            _locator = null;
        }
    }

Example 3 — consumer-side resolution
------------------------------------

    using System;
    using System.Collections.Generic;
    using CodeBrix.ServiceLocation;

    public sealed class ReportService
    {
        public void Run()
        {
            //Typed resolution - the common case.
            IClock clock = ServiceLocator.Current.GetInstance<IClock>();

            //Named resolution.
            IGreeter greeter = ServiceLocator.Current.GetInstance<IGreeter>("friendly");

            //All registrations of a type. Materialize before use (see PITFALLS).
            List<IGreeter> all =
                new List<IGreeter>(ServiceLocator.Current.GetAllInstances<IGreeter>());

            //Non-generic form, when the type is only known at run time.
            object byType = ServiceLocator.Current.GetInstance(typeof(IClock));

            //IServiceProvider form - inherited member; throws, does not return null.
            object viaProvider = ((IServiceProvider)ServiceLocator.Current)
                .GetService(typeof(IClock));

            Console.WriteLine(clock.GetType().Name + " / " + greeter.GetType().Name
                              + " / " + all.Count + " / " + byType
                              + " / " + viaProvider);
        }
    }

Example 4 — defensive resolution and failure handling
-----------------------------------------------------

    using System;
    using CodeBrix.ServiceLocation;

    public static class Resolver
    {
        public static TService TryResolve<TService>() where TService : class
        {
            //Never let Current throw just because startup has not run yet.
            if (!ServiceLocator.IsLocationProviderSet)
            {
                return null;
            }

            try
            {
                return ServiceLocator.Current.GetInstance<TService>();
            }
            catch (ActivationException ex)
            {
                //The container's own exception is the inner one; the
                //ActivationException message only names the type and key.
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.InnerException);
                return null;
            }
        }
    }

Example 5 — richer failure messages
-----------------------------------

Override the two format methods when the default "Activation error occurred
while trying to get instance of type X, key "y"" is not enough.

    using System;
    using System.Collections.Generic;
    using CodeBrix.ServiceLocation;

    public sealed class DiagnosticLocator : ServiceLocatorImplBase
    {
        protected override object DoGetInstance(Type serviceType, string key) =>
            throw new NotImplementedException("call the real container here");

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType) =>
            throw new NotImplementedException("call the real container here");

        protected override string FormatActivationExceptionMessage(
            Exception actualException, Type serviceType, string key)
        {
            return "Could not resolve " + serviceType.FullName
                   + " (key: " + (key ?? "<none>") + "): "
                   + actualException.Message;
        }

        protected override string FormatActivateAllExceptionMessage(
            Exception actualException, Type serviceType)
        {
            return "Could not resolve all " + serviceType.FullName + ": "
                   + actualException.Message;
        }
    }


MINIMUM VIABLE PROJECT
======================

MyApp.csproj

    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <RootNamespace>MyApp</RootNamespace>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="CodeBrix.ServiceLocator.MsplLicenseForever" />
      </ItemGroup>
    </Project>

(Use the current published version in the Version attribute, or a central
package-management entry; this file states no versions on purpose.)

Program.cs

    using System;
    using System.Collections.Generic;
    using CodeBrix.ServiceLocation;

    namespace MyApp;

    public interface IGreeter
    {
        string Greet(string name);
    }

    public sealed class Greeter : IGreeter
    {
        public string Greet(string name) => "Hello, " + name + ".";
    }

    public sealed class TinyLocator : ServiceLocatorImplBase
    {
        private readonly Dictionary<Type, object> _map = new Dictionary<Type, object>();

        public TinyLocator Add<TService>(TService instance)
        {
            _map[typeof(TService)] = instance;
            return this;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (_map.TryGetValue(serviceType, out object instance))
            {
                return instance;
            }
            throw new KeyNotFoundException(serviceType.FullName);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return _map.TryGetValue(serviceType, out object instance)
                ? new List<object> { instance }
                : new List<object>();
        }
    }

    public static class Program
    {
        private static readonly TinyLocator Locator =
            new TinyLocator().Add<IGreeter>(new Greeter());

        public static void Main()
        {
            ServiceLocator.SetLocatorProvider(() => Locator);

            IGreeter greeter = ServiceLocator.Current.GetInstance<IGreeter>();
            Console.WriteLine(greeter.Greet("world"));

            try
            {
                ServiceLocator.Current.GetInstance<IDisposable>();
            }
            catch (ActivationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.GetType().Name);
            }
        }
    }


PERFORMANCE TIPS
================

  * ServiceLocator.Current calls the provider delegate on every single access.
    Return a cached instance from the delegate; never construct the adapter (or
    the container) inside it. A hot loop that reads Current repeatedly should
    hoist `IServiceLocator locator = ServiceLocator.Current;` out of the loop.
  * Resolution cost is entirely your container's. This package adds one delegate
    invocation, one virtual call, one try/catch (free when nothing throws) and,
    for the generic overloads, one cast per instance.
  * GetAllInstances<TService>() casts element by element as it enumerates and
    re-walks the container each time you enumerate it. Materialize once
    (ToList/ToArray) when the same set is used more than once.
  * Exceptions are the expensive path: ActivationException is constructed with
    a formatted message plus the inner exception, so do not use failed
    resolution as normal control flow in hot code — check with your container
    or keep a "was it registered" flag of your own.
  * Nothing here allocates per-call state otherwise; the abstraction is a thin
    indirection layer and adds no caching of its own.


COMMON PITFALLS TO AVOID
========================

  1. ServiceLocator.Current throws when no provider is set. The exception is
     InvalidOperationException (" ServiceLocationProvider must be set."), NOT
     ActivationException. Guard with `if (ServiceLocator.IsLocationProviderSet)`
     in library code that may run before an application's startup path.

  2. SetLocatorProvider(null) is how you clear the ambient container — there is
     no Reset() or ClearLocatorProvider(). After clearing,
     IsLocationProviderSet is false and Current throws again. Tests that set the
     provider must clear it afterwards, or later tests inherit it.

  3. The provider delegate runs on EVERY Current access.
     `SetLocatorProvider(() => new MyLocator())` therefore builds a NEW adapter
     (and possibly a new container) per resolution — a subtle and expensive bug.
     Capture one instance in a field and return it.

  4. ActivationException hides the real cause in InnerException. The default
     message names only the requested type and key, never the container's own
     message. Always log or inspect `ex.InnerException`; overriding
     FormatActivationExceptionMessage is the way to fold it into the message.

  5. GetService(Type) does not follow the usual IServiceProvider convention of
     returning null for an unknown service — ServiceLocatorImplBase routes it to
     GetInstance(serviceType, null), so an unresolvable service throws
     ActivationException. Do not pass an IServiceLocator to code that tests the
     result of GetService for null and expects no exception.

  6. Null-key semantics: GetInstance(Type), GetInstance<TService>() and
     GetService(Type) all reach DoGetInstance with `key == null`. An adapter
     that indexes registrations by a non-null name must map null onto its
     default registration, or every unnamed resolution fails.

  7. The generic casts are unwrapped. GetInstance<TService>() casts the result
     of GetInstance(typeof(TService), null) AFTER the try/catch, so a container
     returning an object of the wrong type raises InvalidCastException, not
     ActivationException.

  8. GetAllInstances<TService>() is a C# iterator: calling it executes nothing.
     The underlying resolution — and therefore any ActivationException — happens
     on the first MoveNext. Put the enumeration, not the call, inside your
     try/catch, and materialize before crossing an error boundary.

  9. Wrapping only covers producing the sequence. GetAllInstances(Type) calls
     DoGetAllInstances inside try/catch; if your override returns a lazily
     evaluated LINQ query, exceptions thrown while the caller enumerates escape
     UNWRAPPED. Materialize inside DoGetAllInstances (ToList/ToArray).

 10. The ambient provider is process-wide static state — not thread-local, not
     AsyncLocal, and not synchronized. Set it once during startup, before other
     threads run. Parallel test classes that each set the provider will
     interfere with one another; prefer injecting an IServiceLocator directly in
     tests, or serialize those tests.

 11. Every ServiceLocatorImplBase member is virtual. Overriding
     GetInstance(Type, string) or GetAllInstances(Type) without calling base
     silently removes the ActivationException wrapping that consumers rely on.
     Override the Do* methods instead unless you deliberately want that change.

 12. Do not reference the upstream CommonServiceLocator package alongside this
     one: identical type names in two namespaces produce ambiguous references.
     Migrating means replacing `using CommonServiceLocator;` with
     `using CodeBrix.ServiceLocation;` — nothing else changes.

 13. ServiceLocator.Current can legitimately return null if the registered
     provider delegate returns null; the accessor does not check. A provider
     that reads a field cleared at shutdown will hand out null rather than
     throw.


WHAT THIS PACKAGE DOES NOT DO
=============================

  * It is not a container. There is no registration, binding, factory, decorator
    or module API — nothing named Register/Bind/Configure exists in it.
  * It does not manage lifetimes or scopes: no singleton/transient/scoped
    concepts, no child containers, no disposal. Whatever your adapter returns is
    what callers get, and this package never disposes it.
  * It ships no adapter for any specific container. Deriving from
    ServiceLocatorImplBase for the container you use is the intended work.
  * No asynchronous resolution: there is no GetInstanceAsync, and nothing here
    returns Task.
  * No try-style API: no TryGetInstance, no TryResolve, no null-on-missing
    overload. Failure is an exception.
  * No constructor injection, no attributes, no source generator, no
    assembly scanning, no reflection-based auto-registration.
  * No thread-safety or ambient-scope guarantees for the static accessor (no
    lock, no AsyncLocal, no per-request scope).
  * ActivationException has no [Serializable] SerializationInfo/StreamingContext
    constructor; the upstream net40-only constructor is deliberately not ported
    (it would trip SYSLIB0051 on modern .NET).
  * The assembly is not strong-name signed. It targets netstandard2.0 and
    net10.0; no older-framework-specific build exists.
  * The namespace is `CodeBrix.ServiceLocation`, not `CodeBrix.ServiceLocator`.
    Getting this wrong is the single most likely thing to go wrong when you
    consume this package, especially if you are working from an older sample,
    from package version 1.0.242.982 or earlier, or from memory. If you write
    `using CodeBrix.ServiceLocation;` you get CS0246 (namespace not found); if
    you are inside a `CodeBrix.*` namespace and rely on an old alias you may
    instead see CS0234 or CS0118 naming a namespace where you expected a type.
    Both mean the same thing: use `using CodeBrix.ServiceLocation;`. See the
    breaking-change notice at the top of this file.


WORKING EXAMPLES ON GITHUB
==========================

The test project is the executable specification for everything above:

  https://github.com/ellisnet/CodeBrix.ServiceLocator/tree/main/tests/CodeBrix.ServiceLocator.Tests

  MockServiceLocator.cs
      A minimal concrete ServiceLocatorImplBase (dictionary-backed, with a
      "fail on next resolve" switch) — the smallest complete adapter, and the
      model Example 1 above follows.
      https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/MockServiceLocator.cs

  ServiceLocatorTests.cs
      Ambient-accessor behaviour: Current throwing when no provider is set,
      IsLocationProviderSet before/after SetLocatorProvider, and Current
      returning the instance the provider hands back.
      https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/ServiceLocatorTests.cs

  ServiceLocatorImplBaseTests.cs
      Typed, named and by-Type resolution; GetService delegation to the unnamed
      GetInstance; GetAllInstances in both forms (including the empty case);
      ActivationException wrapping for single and multi resolution; and the
      default message naming the requested type and key.
      https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/ServiceLocatorImplBaseTests.cs

  ActivationExceptionTests.cs
      The three constructors and the inner-exception behaviour.
      https://github.com/ellisnet/CodeBrix.ServiceLocator/blob/main/tests/CodeBrix.ServiceLocator.Tests/ActivationExceptionTests.cs


QUICK REFERENCE CARD
====================

PACKAGE
  Id          CodeBrix.ServiceLocator.MsplLicenseForever
  Namespace   CodeBrix.ServiceLocation         (flat; NOT "...ServiceLocator")
  Assembly    CodeBrix.ServiceLocator          (differs from the namespace)
  License     MS-PL   TFMs  netstandard2.0; net10.0   Dependencies  none

SET UP (once, at startup)
  ServiceLocator.SetLocatorProvider(() => _locator);   // cached instance
  ServiceLocator.SetLocatorProvider(null);             // clear it

RESOLVE
  ServiceLocator.Current.GetInstance<TService>()
  ServiceLocator.Current.GetInstance<TService>("key")
  ServiceLocator.Current.GetInstance(typeof(TService))
  ServiceLocator.Current.GetInstance(typeof(TService), "key")
  ServiceLocator.Current.GetAllInstances<TService>()      // iterator: deferred
  ServiceLocator.Current.GetAllInstances(typeof(TService))
  ((IServiceProvider)locator).GetService(typeof(TService))  // throws, not null

WRITE AN ADAPTER
  class MyLocator : ServiceLocatorImplBase
    protected override object              DoGetInstance(Type serviceType, string key)
    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
  optional:
    protected override string FormatActivationExceptionMessage(Exception, Type, string)
    protected override string FormatActivateAllExceptionMessage(Exception, Type)

GUARDS AND FAILURES
  ServiceLocator.IsLocationProviderSet     -> false before SetLocatorProvider
  ServiceLocator.Current (unset provider)  -> InvalidOperationException
  resolution failure                       -> ActivationException
                                              (real cause in InnerException)
  wrong-type return from container         -> InvalidCastException (unwrapped)

RULES OF THUMB
  * one adapter instance, returned by the provider delegate
  * key == null means "the default registration"
  * materialize DoGetAllInstances results before returning them
  * catch ActivationException, read InnerException
  * never `using CommonServiceLocator;`

================================================================================
END OF AGENT-README
================================================================================
