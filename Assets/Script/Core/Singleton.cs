using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : Component
{
    bool isInitialized = false;

    private static bool isShutDown = false;

    private static T instance = null;

    public static T Instance
    {
        get
        {
            if (isShutDown)
            {
                Debug.LogWarning("싱글톤 삭제중");
                return null;
            }
            if(instance == null)
            {
                T singleton = FindAnyObjectByType<T>();
                if (singleton == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "Singleton";
                    singleton = obj.AddComponent<T>();
                }
                instance = singleton;
                DontDestroyOnLoad(singleton);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            if(instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(!isInitialized)
        {
            OnPreInitialize();
        }
        if(mode != LoadSceneMode.Additive)
        {
            OnInitialize();
        }
    }
    protected virtual void OnPreInitialize()
    {
        isInitialized = true;
    }

    protected virtual void OnInitialize()
    {
    }

}
