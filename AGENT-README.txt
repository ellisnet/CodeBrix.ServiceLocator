================================================================================
AGENT-README: CodeBrix.ServiceLocator
A Comprehensive Guide for AI Coding Agents
================================================================================

OVERVIEW
--------------------------------------------------------------------------------
CodeBrix.ServiceLocator is a shared abstraction over IoC containers and service
locators. It is a faithful port of CommonServiceLocator 2.0.7 into the
CodeBrix.ServiceLocator namespace, intended as a drop-in replacement for the
CommonServiceLocator NuGet package. The public type-for-type surface matches the
original, so consumers migrate by changing the namespace from
`CommonServiceLocator` to `CodeBrix.ServiceLocator`.

The library has no third-party NuGet dependencies; it depends only on .NET 10.


INSTALLATION
--------------------------------------------------------------------------------
NuGet package id:   CodeBrix.ServiceLocator.MsplLicenseForever
Install:            dotnet add package CodeBrix.ServiceLocator.MsplLicenseForever
Namespace:          CodeBrix.ServiceLocator (NOTE: the namespace has NO license
                    suffix; only the NuGet package id carries ".MsplLicenseForever")
Target framework:   .NET 10.0 or higher
License:            MS-PL (this library), ported from CommonServiceLocator
                    (MS-PL) -- see THIRD-PARTY-NOTICES.txt


KEY NAMESPACE
--------------------------------------------------------------------------------
  using CodeBrix.ServiceLocator;

The namespace is intentionally FLAT (no sub-namespaces) to preserve type-for-type
drop-in compatibility with CommonServiceLocator 2.0.7.


CORE API REFERENCE
--------------------------------------------------------------------------------
IServiceLocator : IServiceProvider
  object               GetInstance(Type serviceType)
  object               GetInstance(Type serviceType, string key)
  IEnumerable<object>  GetAllInstances(Type serviceType)
  TService             GetInstance<TService>()
  TService             GetInstance<TService>(string key)
  IEnumerable<TService> GetAllInstances<TService>()
    The service-location abstraction. Resolve services by type, or by type + a
    string key (name). Resolution failures surface as ActivationException.

ServiceLocator   (static ambient accessor)
  static IServiceLocator Current                 -- the current ambient container;
                                                    throws InvalidOperationException
                                                    if no provider has been set.
  static void  SetLocatorProvider(ServiceLocatorProvider newProvider)
  static bool  IsLocationProviderSet             -- whether a provider has been set.
    Set the provider with SetLocatorProvider(() => myLocator); pass null to clear.

ServiceLocatorImplBase : IServiceLocator   (abstract)
    Implements the entire IServiceLocator surface in terms of two template
    methods. A container adapter only overrides:
      protected abstract object              DoGetInstance(Type serviceType, string key)
      protected abstract IEnumerable<object> DoGetAllInstances(Type serviceType)
    Exceptions thrown by these overrides are caught and re-thrown wrapped in an
    ActivationException, with a message produced by the virtual
    FormatActivationExceptionMessage / FormatActivateAllExceptionMessage methods
    (both overridable). GetService(Type) (the IServiceProvider member) delegates
    to GetInstance(serviceType, null).

ServiceLocatorProvider   (delegate)
    public delegate IServiceLocator ServiceLocatorProvider();
    Supplies the ambient container to the static ServiceLocator accessor.

ActivationException : Exception
    Thrown when service resolution fails. Constructors: (), (string message),
    (string message, Exception innerException). Note: unlike the upstream net40
    build, the legacy [Serializable] SerializationInfo/StreamingContext
    constructor is intentionally not included (see ARCHITECTURE).

Error model:
    All resolution failures from a ServiceLocatorImplBase-derived adapter are
    wrapped in ActivationException; the underlying container exception is
    available via InnerException.


CODING CONVENTIONS (CodeBrix family)
--------------------------------------------------------------------------------
This repository follows the standard CodeBrix family conventions (no situational
exceptions are in force):

  - Nullable reference types are OFF (the upstream code is not NRT-annotated); no
    `?` on reference types and no null-forgiveness `!` operator.
  - File-scoped namespaces only; no block-scoped namespaces.
  - No `global using` directives; usings are explicit and per-file.
  - Target framework is net10.0 only; no multi-targeting.
  - <GenerateDocumentationFile> is ON; every public member carries an XML doc
    comment (CS1591 is fixed at source, never suppressed).
  - No project-level warning suppression (<NoWarn>); the solution builds with
    zero warnings and zero errors.
  - Tests use xUnit v3 + SilverAssertions.
  - The library project root carries InternalsVisibleTo.cs granting internals
    access to CodeBrix.ServiceLocator.Tests.


ARCHITECTURE
--------------------------------------------------------------------------------
Source layout under src/CodeBrix.ServiceLocator/ (flat -- one namespace):

  IServiceLocator.cs          the abstraction
  ServiceLocator.cs           the static ambient accessor
  ServiceLocatorImplBase.cs   the abstract default base
  ServiceLocatorProvider.cs   the provider delegate
  ActivationException.cs      the resolution-failure exception
  InternalsVisibleTo.cs       CodeBrix-standard internals grant

Port provenance: each ported file's namespace line carries a
`//was previously: CommonServiceLocator;` comment. The upstream files had no
top-of-file license header, so none was fabricated.

Intentionally NOT ported from CommonServiceLocator 2.0.7:
  - src/Properties/AssemblyInfo.cs   (strong-name / SecurityTransparent / CLS
                                      assembly attributes; CodeBrix does not sign)
  - src/net40/TypeInfo.cs            (net40-only reflection shim)
  - src/Exceptions/ActivationException.Desktop.cs  (the net40-only [Serializable]
                                      constructor; excluded on modern frameworks
                                      upstream, and would trip SYSLIB0051 on net10)


TESTING
--------------------------------------------------------------------------------
Tests live in tests/CodeBrix.ServiceLocator.Tests/ and use xUnit v3 +
SilverAssertions. A MockServiceLocator (a concrete ServiceLocatorImplBase) drives
the abstract base. The suite covers the ambient accessor (set/unset/resolve),
typed and named resolution, GetAllInstances, IServiceProvider delegation,
ActivationException wrapping of resolution failures, and the exception's own
constructors.

Run everything from the repository root:

  dotnet restore CodeBrix.ServiceLocator.slnx
  dotnet build   CodeBrix.ServiceLocator.slnx
  dotnet test    CodeBrix.ServiceLocator.slnx

================================================================================
END OF AGENT-README
================================================================================
