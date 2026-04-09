using System;
using System.Collections.Generic;

public static class ServiceRegistry
{
    private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

    public static void Register<T>(T service) where T : class
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        Services[typeof(T)] = service;
    }

    public static T Get<T>() where T : class
    {
        if (Services.TryGetValue(typeof(T), out object service))
        {
            return service as T;
        }

        return null;
    }

    public static void Clear()
    {
        Services.Clear();
    }
}
