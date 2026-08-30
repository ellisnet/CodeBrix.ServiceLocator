using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.ServiceLocation;

namespace CodeBrix.ServiceLocation.Tests;

// Test infrastructure (helper file -- intentionally no "Tests" suffix).

public interface ITestService
{
    string Name { get; }
}

public sealed class TestService : ITestService
{
    public TestService(string name) => Name = name;

    public string Name { get; }
}

// A minimal concrete ServiceLocatorImplBase used to exercise the abstract base.
public sealed class MockServiceLocator : ServiceLocatorImplBase
{
    private readonly Dictionary<Type, List<KeyValuePair<string, object>>> _registrations = new();
    private bool _throwOnGet;

    public MockServiceLocator Register(Type serviceType, string key, object instance)
    {
        if (!_registrations.TryGetValue(serviceType, out var list))
        {
            list = new List<KeyValuePair<string, object>>();
            _registrations[serviceType] = list;
        }
        list.Add(new KeyValuePair<string, object>(key, instance));
        return this;
    }

    public MockServiceLocator FailOnNextResolve()
    {
        _throwOnGet = true;
        return this;
    }

    protected override object DoGetInstance(Type serviceType, string key)
    {
        if (_throwOnGet)
        {
            throw new InvalidOperationException("simulated container failure");
        }
        return _registrations[serviceType].First(kv => kv.Key == key).Value;
    }

    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
    {
        if (_throwOnGet)
        {
            throw new InvalidOperationException("simulated container failure");
        }
        return _registrations.TryGetValue(serviceType, out var list)
            ? list.Select(kv => kv.Value)
            : Enumerable.Empty<object>();
    }
}
