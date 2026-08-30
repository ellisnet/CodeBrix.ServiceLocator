using System;
using CodeBrix.ServiceLocation;
using SilverAssertions;
using Xunit;

namespace CodeBrix.ServiceLocation.Tests;

public class ActivationExceptionTests
{
    [Fact]
    public void Default_ctor_has_null_inner_exception()
        => new ActivationException().InnerException.Should().BeNull();

    [Fact]
    public void Message_ctor_sets_the_message()
        => new ActivationException("boom").Message.Should().Be("boom");

    [Fact]
    public void Inner_exception_ctor_sets_message_and_inner()
    {
        //Arrange
        var inner = new InvalidOperationException("root cause");

        //Act
        var ex = new ActivationException("boom", inner);

        //Assert
        ex.Message.Should().Be("boom");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void Activation_exception_is_an_exception()
        => new ActivationException().Should().BeAssignableTo<Exception>();
}
