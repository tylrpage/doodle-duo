using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageManager : MonoBehaviour, IService
{
    public event Action ImageChanged;
    
    [Serializable]
    public struct Level
    {
        public Sprite outline;
        public Sprite final;
    }

    public Level[] levels;
    public short CurrentImageIndex { get; private set; } = -1; // -1 means no image

    private NetworkManager _networkManager;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    private void Start()
    {
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        _networkManager.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(BitSerializable message)
    {
        switch (message)
        {
            case ServerChangeImageMessage changeImageMessage:
                CurrentImageIndex = changeImageMessage.ImageIndex;
                ImageChanged?.Invoke();
                break;
        }
    }

    public void GetNextImage()
    {
        CurrentImageIndex++;
        // Wrap around
        if (CurrentImageIndex >= levels.Length)
        {
            CurrentImageIndex = 0;
        }
        
        ImageChanged?.Invoke();
    }
}
