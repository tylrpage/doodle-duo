using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void RegisterService(IService service)
    {
        _services[service.GetType()] = service;
    }

    public T GetService<T>() where T : IService
    {
        if (_services.TryGetValue(typeof(T), out IService service))
        {
            return (T) service;
        }
        else
        {
            Debug.LogError($"Could not find service of type {typeof(T)}");
            return default;
        }
    }
}
