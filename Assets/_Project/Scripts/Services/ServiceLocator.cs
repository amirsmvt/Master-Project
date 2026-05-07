using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeuroQuest.Services
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);

            if (Services.ContainsKey(type))
            {
                Services[type] = service;
            }
            else
            {
                Services.Add(type, service);
            }
        }

        public static T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (Services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            Debug.LogError($"ServiceLocator: Service of type {type.Name} is not registered.");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);

            if (Services.TryGetValue(type, out object foundService))
            {
                service = foundService as T;
                return service != null;
            }

            service = null;
            return false;
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}