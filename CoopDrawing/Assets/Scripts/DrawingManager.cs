using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DrawingManager : MonoBehaviour, IService
{
    public Vector2 DotPosition { get; private set; } // Page space

    [field: SerializeField] public Vector2 PageSize { get; private set; }
    [SerializeField] private int dotSpeed;

    private NetworkManager _networkManager;
    private StateManager _stateManager;
    private Vector2 _clientUnsentInputTotal;
    private float _clientTimeSinceSentInput;
    
    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    private void Start()
    {
        // Start dot in the middle
        DotPosition = PageSize / 2f;
        
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        _stateManager = GameManager.Instance.GetService<StateManager>();

        if (_networkManager.IsServer)
        {
            _networkManager.Server.MessageReceived += OnMessageReceived;
        }
        else
        {
            _networkManager.Client.MessageReceived += OnMessageReceived;
        }
    }

    private void OnMessageReceived(BitSerializable message)
    {
        switch (message)
        {
            case InputMessage inputMessage:
                MoveDot(inputMessage.Direction);
                break;
            case GameStateMessage gameStateMessage:
                // todo: smooth movement
                DotPosition = gameStateMessage.DotPosition;
                break;
        }
    }

    private void Update()
    {
        if (_networkManager.IsServer)
        {
            // Tell clients the dot position
            _networkManager.Server.SendAll(new GameStateMessage()
            {
                DotPosition = DotPosition,
            });
        }
        else
        {
            // Must be playing to move the dot
            if (_stateManager.CurrentState != StateManager.State.Playing)
                return;
            
            // Collect input
            _clientTimeSinceSentInput += Time.deltaTime;
            _clientUnsentInputTotal += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) 
                                       * (dotSpeed * Time.deltaTime);
            
            if (_clientTimeSinceSentInput > Constants.Step)
            {
                _clientTimeSinceSentInput -= Constants.Step;
                
                // Send input to server
                _networkManager.Client.Send(new InputMessage()
                {
                    Direction = _clientUnsentInputTotal,
                    // todo: hook up rewinding
                    Rewinding = false,
                });
        
                // Clear out the total 
                _clientUnsentInputTotal = Vector2.zero;
            }
        }
    }

    public void MoveDot(Vector2 input)
    {
        DotPosition = new Vector2(
            Mathf.Clamp(DotPosition.x + input.x, 0, PageSize.x),
            Mathf.Clamp(DotPosition.y + input.y, 0, PageSize.y)
        );
    }
}
