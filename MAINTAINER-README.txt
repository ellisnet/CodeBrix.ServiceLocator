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
  * PackageId is CodeBrix.ServiceLocator.MsplLicenseForever while the
    AssemblyName and RootNamespace are plain CodeBrix.ServiceLocator; the
    license suffix exists only on the PackageId.
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

  * Namespace rename CommonServiceLocator -> CodeBrix.ServiceLocator. Every
    ported file's namespace line carries a `//was previously:
    CommonServiceLocator;` provenance comment — keep those comments.
  * Block-scoped namespaces converted to file-scoped.
  * net10.0 only; upstream multi-targeted a dozen frameworks.
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
  * net10.0 only; no multi-targeting.
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
