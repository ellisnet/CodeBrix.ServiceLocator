using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.ServiceLocator;
using SilverAssertions;
using Xunit;

namespace CodeBrix.ServiceLocator.Tests;

public class ServiceLocatorImplBaseTests
{
    private static MockServiceLocator WithService(string name, string key = null)
        => new MockServiceLocator().Register(typeof(ITestService), key, new TestService(name));

    [Fact]
    public void GetInstance_generic_returns_registered_instance()
    {
        //Arrange
        var locator = WithService("primary");

        //Act
        var service = locator.GetInstance<ITestService>();

        //Assert
        service.Name.Should().Be("primary");
    }

    [Fact]
    public void GetInstance_by_type_returns_registered_instance()
    {
        //Arrange
        var locator = WithService("primary");

        //Act
        var service = locator.GetInstance(typeof(ITestService));

        //Assert
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void GetInstance_named_returns_matching_instance()
    {
        //Arrange
        var locator = WithService("named", key: "alpha");

        //Act
        var service = locator.GetInstance<ITestService>("alpha");

        //Assert
        service.Name.Should().Be("named");
    }

    [Fact]
    public void GetInstance_named_by_type_returns_matching_instance()
    {
        //Arrange
        var locator = WithService("named", key: "alpha");

        //Act
        var service = locator.GetInstance(typeof(ITestService), "alpha");

        //Assert
        ((ITestService)service).Name.Should().Be("named");
    }

    [Fact]
    public void GetService_delegates_to_unnamed_get_instance()
    {
        //Arrange
        var locator = WithService("via-provider");

        //Act
        var service = ((IServiceProvider)locator).GetService(typeof(ITestService));

        //Assert
        ((ITestService)service).Name.Should().Be("via-provider");
    }

    [Fact]
    public void GetAllInstances_by_type_returns_all_registrations()
    {
        //Arrange
        var locator = new MockServiceLocator()
            .Register(typeof(ITestService), null, new TestService("a"))
            .Register(typeof(ITestService), null, new TestService("b"));

        //Act
        var all = locator.GetAllInstances(typeof(ITestService)).ToList();

        //Assert
        all.Should().HaveCount(2);
    }

    [Fact]
    public void GetAllInstances_generic_returns_all_registrations_typed()
    {
        //Arrange
        var locator = new MockServiceLocator()
            .Register(typeof(ITestService), null, new TestService("a"))
            .Register(typeof(ITestService), null, new TestService("b"));

        //Act
        var all = locator.GetAllInstances<ITestService>().Select(s => s.Name).ToList();

        //Assert
        all.Should().ContainInOrder("a", "b");
    }

    [Fact]
    public void GetAllInstances_generic_returns_empty_when_none_registered()
        => new MockServiceLocator().GetAllInstances<ITestService>().Should().BeEmpty();

    [Fact]
    public void GetInstance_wraps_resolution_failure_in_activation_exception()
    {
        //Arrange
        var locator = WithService("primary").FailOnNextResolve();

        //Act
        var ex = Assert.Throws<ActivationException>(() => locator.GetInstance<ITestService>());

        //Assert
        ex.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void GetAllInstances_wraps_resolution_failure_in_activation_exception()
    {
        //Arrange
        var locator = new MockServiceLocator().FailOnNextResolve();

        //Act
        var ex = Assert.Throws<ActivationException>(() => locator.GetAllInstances(typeof(ITestService)).ToList());

        //Assert
        ex.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void Activation_exception_message_names_the_requested_type_and_key()
    {
        //Arrange
        var locator = WithService("primary", key: "alpha").FailOnNextResolve();

        //Act
        var ex = Assert.Throws<ActivationException>(() => locator.GetInstance<ITestService>("alpha"));

        //Assert
        ex.Message.Should().Contain("ITestService");
        ex.Message.Should().Contain("alpha");
    }
}
