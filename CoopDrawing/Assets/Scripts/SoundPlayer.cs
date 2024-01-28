using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;

    private NetworkManager _networkManager;
    private StateManager _stateManager;
    private ImageManager _imageManager;

    private void Start()
    {
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        if (_networkManager.IsServer)
        {
            // No music on server, destroy ourselves
            Destroy(this);
            return;
        }
        
        _stateManager = GameManager.Instance.GetService<StateManager>();
        _stateManager.StateChanged += OnStateChanged;
        
        _imageManager = GameManager.Instance.GetService<ImageManager>();
        _imageManager.ImageChanged += OnImageChanged;
    }

    private void OnImageChanged()
    {
        // Play music at the adjusted pitch/speed for this image
        float musicSpeed = _stateManager.BaseTimeLimit / _imageManager.CurrentLevel.timeLimit;
        musicSource.pitch = musicSpeed;
        musicSource.Play();
    }

    private void OnStateChanged(StateManager.State newState)
    {
        switch (newState)
        {
            case StateManager.State.Restarting:
                // todo: play fail sfx
                break;
            case StateManager.State.Ending:
                // todo: play win sfx
                musicSource.Stop();
                break;
        }
    }
}
