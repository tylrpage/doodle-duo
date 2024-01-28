using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ImageManager : MonoBehaviour, IService
{
    public event Action ImageChanged;
    
    public short CurrentImageIndex { get; private set; } = -1; // -1 means no image
    
    [Serializable]
    public class Level
    {
        public Sprite outline;
        public Sprite final;
        [NonSerialized] public Sprite processedOutline;
    }
    public Level[] levels;
    
    [SerializeField] private ImageData imageData;
    [SerializeField] private ImageViewer imageViewer;
    
    private NetworkManager _networkManager;

    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
        
        // Process images
        foreach (var level in levels)
        {
            imageData.SetNewImage(level.outline.texture, level.final.texture);
            level.processedOutline = imageViewer.GetRenderedSprite(imageData.pixelData);
        }
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
