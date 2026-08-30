using System;
using CodeBrix.ServiceLocation;
using SilverAssertions;
using Xunit;

namespace CodeBrix.ServiceLocation.Tests;

// All tests for the static ambient `ServiceLocator` accessor live in this single
// class. xUnit runs the methods of one test class sequentially, so each test sets
// the (process-global) provider explicitly and does not depend on ordering.
public class ServiceLocatorTests
{
    [Fact]
    public void Current_throws_when_provider_not_set()
    {
        //Arrange
        ServiceLocator.SetLocatorProvider(null);

        //Act
        var act = () => ServiceLocator.Current;

        //Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsLocationProviderSet_is_false_when_provider_not_set()
    {
        //Arrange
        ServiceLocator.SetLocatorProvider(null);

        //Assert
        ServiceLocator.IsLocationProviderSet.Should().BeFalse();
    }

    [Fact]
    public void IsLocationProviderSet_is_true_after_provider_is_set()
    {
        //Arrange
        var locator = new MockServiceLocator();

        //Act
        ServiceLocator.SetLocatorProvider(() => locator);

        //Assert
        ServiceLocator.IsLocationProviderSet.Should().BeTrue();
    }

    [Fact]
    public void Current_returns_the_instance_from_the_provider()
    {
        //Arrange
        var locator = new MockServiceLocator();
        ServiceLocator.SetLocatorProvider(() => locator);

        //Act
        var current = ServiceLocator.Current;

        //Assert
        current.Should().BeSameAs(locator);
    }
}
