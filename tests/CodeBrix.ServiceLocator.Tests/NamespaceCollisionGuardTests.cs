using CodeBrix.ServiceLocation;
using SilverAssertions;
using Xunit;

// REGRESSION GUARD -- this file's namespace is deliberately NOT under
// CodeBrix.ServiceLocation.*, so it reproduces the situation of a real
// CodeBrix-family consumer such as CodeBrix.Platform.Extensions.
//
// The library's namespace used to be `CodeBrix.ServiceLocator`, which made a
// NAMESPACE of that name a member of the enclosing `CodeBrix` namespace. From
// any other CodeBrix.* namespace the bare name `ServiceLocator` then bound to
// that namespace instead of to the class, and the code below failed to compile
// with CS0234 ("the type or namespace name 'Current' does not exist in the
// namespace 'CodeBrix.ServiceLocator'"). Renaming the namespace to
// CodeBrix.ServiceLocation removed the collision.
//
// If anyone renames the library namespace back to CodeBrix.ServiceLocator --
// or otherwise reintroduces a `CodeBrix.<something>` namespace whose last
// segment matches a public type name -- THIS FILE STOPS COMPILING. That build
// break is the point; do not "fix" it with a using-alias.
namespace CodeBrix.CollisionGuard.Tests;

public class NamespaceCollisionGuardTests
{
    [Fact]
    public void Bare_ServiceLocator_name_binds_to_the_class_from_another_CodeBrix_namespace()
    {
        //Arrange
        ServiceLocator.SetLocatorProvider(null);

        //Act -- the bare name must resolve to the CLASS, not to a namespace.
        var isSet = ServiceLocator.IsLocationProviderSet;

        //Assert
        isSet.Should().BeFalse();
        typeof(ServiceLocator).Namespace.Should().Be("CodeBrix.ServiceLocation");
    }

    [Fact]
    public void Abstraction_types_are_usable_from_another_CodeBrix_namespace()
    {
        //Arrange
        IServiceLocator locator = new GuardLocator();
        ServiceLocator.SetLocatorProvider(() => locator);

        //Act
        var current = ServiceLocator.Current;

        //Assert
        current.Should().BeSameAs(locator);
        ServiceLocator.SetLocatorProvider(null);
    }

    // Deriving from ServiceLocatorImplBase here also proves the base type and
    // ActivationException resolve from outside the library's namespace tree.
    private sealed class GuardLocator : ServiceLocatorImplBase
    {
        protected override object DoGetInstance(System.Type serviceType, string key) => null;

        protected override System.Collections.Generic.IEnumerable<object> DoGetAllInstances(
            System.Type serviceType) => System.Array.Empty<object>();
    }
}
