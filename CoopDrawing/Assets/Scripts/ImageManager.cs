using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageManager : MonoBehaviour, IService
{
    [Serializable]
    public struct Level
    {
        public Texture2D outline;
        public Texture2D final;
    }

    public Level[] levels;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }
}
