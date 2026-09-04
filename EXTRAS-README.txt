================================================================================
EXTRAS-README: CodeBrix.ServiceLocator
Samples, tools and other content in this repository that is not part of a NuGet
package
================================================================================

This repository contains no samples, demo applications, tools, scripts or
optional test-data sets. It holds exactly two projects: the library that becomes
the CodeBrix.ServiceLocator.MsplLicenseForever package, and its test project.


TEST PROJECT (the only non-package content)
===========================================

  tests/CodeBrix.ServiceLocator.Tests/

The unit-test project for the library. It is not packed and is not published.
Run it from the repository root with:

  dotnet test CodeBrix.ServiceLocator.slnx

It needs no preparation, no environment variables and no external services.

Its MockServiceLocator.cs is worth knowing about even if you are only consuming
the package: it is a minimal, complete container adapter (a dictionary-backed
ServiceLocatorImplBase with a FailOnNextResolve() switch used to exercise the
ActivationException wrapping), and AGENT-README.txt points consumers at it as a
worked example. See MAINTAINER-README.txt for what the suite covers.

NamespaceCollisionGuardTests.cs is worth knowing about too, for a different
reason: it is a compile-time regression guard rather than a behaviour test. It
is deliberately placed in the unrelated namespace CodeBrix.CollisionGuard.Tests
so that it reproduces the situation of a real CodeBrix-family consumer, and it
STOPS COMPILING if the library's namespace is ever renamed back to
CodeBrix.ServiceLocator. That build break is the assertion — do not "fix" it
with a using-alias.
