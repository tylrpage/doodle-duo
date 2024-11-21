using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource oneShotSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private AudioSource pencilSource;
    [SerializeField] private AudioClip connectionSound;

    private NetworkManager _networkManager;
    private StateManager _stateManager;
    private ImageManager _imageManager;
    private DrawingManager _drawingManager;
    private Vector2? _previousDotPosition;
    private float _timeToCheckForNextMute;

    private void Start()
    {
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        if (_networkManager.IsServer)
        {
            // No music on server, destroy ourselves
            Destroy(this);
            return;
        }
        else
        {
            _networkManager.ClientConnected += OnClientConnected;
        }
        
        _stateManager = GameManager.Instance.GetService<StateManager>();
        _stateManager.StateChanged += OnStateChanged;
        
        _imageManager = GameManager.Instance.GetService<ImageManager>();
        _imageManager.ImageChanged += OnImageChanged;
        
        _drawingManager = GameManager.Instance.GetService<DrawingManager>();
    }

    private void LateUpdate()
    {
        if (_stateManager.CurrentState == StateManager.State.Playing)
        {
            if (_previousDotPosition != null)
            {
                Vector2 delta = _drawingManager.DotPosition - _previousDotPosition.Value;
                bool shouldMute = delta.sqrMagnitude <= Mathf.Epsilon;
                if (!shouldMute)
                {
                    pencilSource.mute = false;
                    _timeToCheckForNextMute = Time.time + Constants.Step * 2f;
                }
                if (shouldMute && Time.time >= _timeToCheckForNextMute)
                {
                    pencilSource.mute = true;
                }
            }
        }
        else
        {
            pencilSource.Stop();
        }
        
        _previousDotPosition = _drawingManager.DotPosition;
    }
    
    private void OnClientConnected()
    {
        oneShotSource.PlayOneShot(connectionSound);
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
            case StateManager.State.Waiting:
                musicSource.Stop();
                break;
            case StateManager.State.Playing:
                pencilSource.Play();
                pencilSource.mute = true;
                break;
            case StateManager.State.Restarting:
                oneShotSource.PlayOneShot(failSound);
                break;
            case StateManager.State.Ending:
                oneShotSource.PlayOneShot(successSound);
                musicSource.Stop();
                break;
        }
    }
}
