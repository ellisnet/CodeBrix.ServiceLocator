================================================================================
MAINTAINER-README: CodeBrix.ServiceLocator
Notes for people and agents MAINTAINING this repository — not for package
consumers
================================================================================

Consumers of the NuGet package should read AGENT-README.txt instead; everything
below is about changing this repository.


PURPOSE AND SCOPE
=================

The repository produces exactly one NuGet package:

  CodeBrix.ServiceLocator.MsplLicenseForever
      Built from src/CodeBrix.ServiceLocator/CodeBrix.ServiceLocator.csproj.
      Consumer documentation: AGENT-README.txt (repository root), which is also
      packed into the nupkg root.

The library is a shared abstraction over IoC containers — five public types,
no dependencies, no container of its own. Its public surface is deliberately
frozen to match CommonServiceLocator 2.0.7 type-for-type; adding public types or
members breaks the drop-in-replacement promise and should not be done casually.


REPOSITORY LAYOUT
=================

  CodeBrix.ServiceLocator/
    CodeBrix.ServiceLocator.slnx        solution (Solution Items folder +
                                        Tests folder + the library project)
    src/CodeBrix.ServiceLocator/
      CodeBrix.ServiceLocator.csproj
      IServiceLocator.cs                the abstraction
      ServiceLocator.cs                 the static ambient accessor
      ServiceLocatorImplBase.cs         the abstract default base
      ServiceLocatorProvider.cs         the provider delegate
      ActivationException.cs            the resolution-failure exception
      InternalsVisibleTo.cs             CodeBrix-standard internals grant, plus
                                        the [assembly: CLSCompliant(true)]
                                        attribute preserved from upstream
    tests/CodeBrix.ServiceLocator.Tests/
      CodeBrix.ServiceLocator.Tests.csproj
      MockServiceLocator.cs             test infrastructure (ITestService,
                                        TestService, MockServiceLocator) —
                                        intentionally has no "Tests" suffix
      ServiceLocatorTests.cs
      ServiceLocatorImplBaseTests.cs
      ActivationExceptionTests.cs
    AGENT-README.txt                    consumer documentation (packed)
    MAINTAINER-README.txt               this file (not packed)
    EXTRAS-README.txt                   non-package content (not packed)
    README-INDEX.txt                    map of the README files (not packed)
    README.md                           human-facing overview (packed)
    LICENSE                             MS-PL
    THIRD-PARTY-NOTICES.txt             upstream attribution + MS-PL text
                                        (packed)
    icon-codebrix-128.png               package icon (packed)

The source folder is flat on purpose: one namespace, no sub-folders. That is a
deliberate exception to the usual CodeBrix "organize sources into sub-folders"
convention, because sub-folders would tempt sub-namespaces and the flat
namespace is what preserves drop-in compatibility.


BUILDING
========

From the repository root:

  dotnet restore CodeBrix.ServiceLocator.slnx
  dotnet build   CodeBrix.ServiceLocator.slnx

The build must finish with zero warnings and zero errors.
<GenerateDocumentationFile> is on, so every public/protected member needs an XML
doc comment; fix CS1591 at the source, never with <NoWarn>.


TESTING
=======

  dotnet test CodeBrix.ServiceLocator.slnx

Test stack: xUnit v3 + SilverAssertions, with coverlet.collector for coverage
and xunit.runner.visualstudio for IDE discovery. No environment variables, no
opt-in switches, no special preparation — the suite is pure in-memory unit
tests and runs anywhere.

Coverage in the suite:

  * ServiceLocatorTests — the ambient accessor: Current throwing when no
    provider is set, IsLocationProviderSet before and after SetLocatorProvider,
    and Current returning what the provider returns.
  * ServiceLocatorImplBaseTests — typed, named and by-Type resolution;
    GetService delegating to the unnamed GetInstance; both GetAllInstances
    forms including the empty case; ActivationException wrapping for single and
    multiple resolution; and the default message naming the requested type and
    key.
  * ActivationExceptionTests — the three constructors and inner-exception
    behaviour.
  * MockServiceLocator — a dictionary-backed concrete ServiceLocatorImplBase
    with a FailOnNextResolve() switch, used to drive the abstract base.

Because ServiceLocator's provider is process-wide static state, any test that
calls SetLocatorProvider must clear it (SetLocatorProvider(null)) so later tests
do not inherit it.

Test naming follows the family convention: one <Class>Tests.cs per production
class, snake_case method names, //Arrange //Act //Assert comment blocks.
InternalsVisibleTo grants CodeBrix.ServiceLocator.Tests access to internals.


PACKAGING AND PUBLISHING
========================

  * <GeneratePackageOnBuild>true</GeneratePackageOnBuild> — every build of the
    library project produces a .nupkg.
  * Versioning is the CodeBrix date-stamped scheme computed in the csproj from
    System.DateTime.UtcNow: 1.<years since _VersionBaseYear>.<day of year>.
    <minute of day>. It is strictly increasing over time and is NOT SemVer, so
    major/minor say nothing about API compatibility. Two builds within the same
    UTC minute produce the same version — never publish two packages from
    inside one minute. Re-baseline by changing _VersionBaseYear.
  * PackageId is CodeBrix.ServiceLocator.MsplLicenseForever and AssemblyName is
    CodeBrix.ServiceLocator, but RootNamespace is CodeBrix.ServiceLocatION. The
    license suffix exists only on the PackageId. The namespace/assembly mismatch
    is DELIBERATE and must not be "tidied up" — see the namespace-rename note
    under Port modifications below.
  * <PackageLicenseExpression>MS-PL</PackageLicenseExpression>, icon
    icon-codebrix-128.png, readme README.md,
    <PackageRequireLicenseAcceptance>true</PackageRequireLicenseAcceptance>.
  * Files packed to the nupkg root: icon-codebrix-128.png, README.md,
    AGENT-README.txt, THIRD-PARTY-NOTICES.txt. MAINTAINER-README.txt,
    EXTRAS-README.txt and README-INDEX.txt are NOT packed — they describe the
    repository, not the package.
  * Copyright line: "Copyright © .NET Foundation and Contributors. Copyright (c)
    2026 Jeremy Ellis and contributors" — the upstream attribution must stay,
    MS-PL section 3(C) requires it.
  * Git tags are expected to match the published NuGet version.


PROVENANCE AND VENDORED SOURCES
===============================

All five production types are ported from CommonServiceLocator 2.0.7
(https://github.com/unitycontainer/commonservicelocator, .NET Foundation and
Contributors, MS-PL). MS-PL is also this repository's own license, so the two
are compatible; the upstream notice and full license text are reproduced in
THIRD-PARTY-NOTICES.txt as MS-PL sections 3(C) and 3(D) require.

Port modifications (all recorded in THIRD-PARTY-NOTICES.txt):

  * Namespace rename CommonServiceLocator -> CodeBrix.ServiceLocation. Every
    ported file's namespace line carries a `//was previously:
    CommonServiceLocator;` provenance comment — keep those comments.
  * THE NAMESPACE IS `CodeBrix.ServiceLocation`, NOT `CodeBrix.ServiceLocator`,
    AND MUST STAY THAT WAY. Package versions through 1.0.242.982 used
    `CodeBrix.ServiceLocator`, which was a NAMESPACE and therefore a member of
    the enclosing `CodeBrix` namespace. From any other `CodeBrix.*` namespace
    the bare name `ServiceLocator` then bound to that namespace and hid the
    CLASS of the same name, so `ServiceLocator.Current` failed with CS0234.
    A using-alias at the top of a consumer's file does NOT fix that (an
    enclosing-namespace member outranks a compilation-unit using-alias), so
    every CodeBrix-family consumer would have needed an alias inside its
    namespace body. Renaming the namespace fixed it once, for all consumers.
    tests/CodeBrix.ServiceLocator.Tests/NamespaceCollisionGuardTests.cs lives in
    an unrelated `CodeBrix.*` namespace and STOPS COMPILING if the collision is
    reintroduced; that build break is intentional, do not paper over it with an
    alias. Note also that the test project sets <RootNamespace> explicitly: left
    at its default it would be CodeBrix.ServiceLocator.Tests, and xUnit v3's
    generated obj/**/SelfRegisteredExtensions.cs would re-create a
    `CodeBrix.ServiceLocator` namespace inside the test assembly.
    This rename is a BREAKING CHANGE for consumers and the date-stamped version
    scheme cannot signal it (major is pinned to 1); it is announced loudly at the
    top of AGENT-README.txt.
  * Block-scoped namespaces converted to file-scoped.
  * Targets netstandard2.0 and net10.0; upstream multi-targeted a dozen
    frameworks. netstandard2.0 was added so CodeBrix.Platform.Extensions —
    which multi-targets netstandard2.0 for Roslyn source generators — could
    take this package in place of CommonServiceLocator. LangVersion is pinned
    to `latest` because netstandard2.0 would otherwise default to C# 7.3 and
    the sources use file-scoped namespaces.
  * Strong-name signing dropped: upstream src/Properties/AssemblyInfo.cs and
    package.snk were not ported. The [assembly: CLSCompliant(true)] attribute
    they carried lives in InternalsVisibleTo.cs instead (it is required, because
    the public surface is annotated [CLSCompliant(true)] and CS3014 would
    otherwise fire).
  * src/net40/TypeInfo.cs not ported (a net40 reflection shim; intrinsic on
    net10).
  * src/Exceptions/ActivationException.Desktop.cs not ported — the net40-only
    [Serializable] SerializationInfo/StreamingContext constructor, which
    upstream itself excludes on modern targets and which would raise SYSLIB0051
    here. The ported ActivationException matches upstream's modern build.
  * The upstream files had no top-of-file license header, so none was
    fabricated.

If the surface ever needs to change, remember that the whole point of the port
is type-for-type substitutability for the upstream package.


CODING CONVENTIONS
==================

Standard CodeBrix family conventions apply, with no situational exceptions:

  * Nullable reference types OFF — no `?` on reference types, no `!`
    null-forgiveness operator. Value-type nullables are fine.
  * File-scoped namespaces only.
  * No `global using` directives; usings are explicit and per-file.
  * netstandard2.0 and net10.0 only; do not add further target frameworks.
    Nothing in the library may use an API outside the netstandard2.0 surface.
  * <GenerateDocumentationFile> on; CS1591 fixed at source.
  * No project-level warning suppression (<NoWarn>, <WarningLevel>0</...>,
    <TreatWarningsAsErrors>false</...> are all forbidden).
  * Tests: xUnit v3 + SilverAssertions;
    TestContext.Current.CancellationToken is threaded through any cancellable
    call in a test (xUnit1051) — none of the current tests are async.
  * `//was previously:` comments mark ported lines that were changed; comment
    out rather than delete when adjusting ported code.

For the full family convention list see CODEBRIX_LIBRARY_OBSERVATIONS.txt in the
CodeBrix.Library.Dev-private repository.


NOTES
=====

  * The exception message thrown by ServiceLocator.Current when no provider is
    set is " ServiceLocationProvider must be set." — with a leading space. That
    is upstream's text, preserved deliberately; do not "fix" it, and note that
    AGENT-README quotes it exactly.
  * The default activation-failure messages do not include the underlying
    exception's message even though the exception is passed to the formatter.
    That is upstream behaviour too; consumers override the formatter methods.
  * GetAllInstances<TService>() is a C# iterator upstream and stays one here —
    the deferred-execution behaviour is documented as a consumer pitfall, so
    changing it would be a behavioural break.
  * There is no samples project, no tools folder and no benchmark project in
    this repository (see EXTRAS-README.txt).
