using System;
using UnityEngine;

public class DrawingManager : MonoBehaviour, IService
{
    public Vector2 DotPosition { get; private set; } // Page space
    public event Action<Vector2> DotMoved;
    public event Action DotReset;

    [field: SerializeField] public Vector2 PageSize { get; private set; }
    [SerializeField] private int dotSpeed;

    private NetworkManager _networkManager;
    private StateManager _stateManager;
    private ImageManager _imageManager;
    private Vector2 _clientUnsentInputTotal;
    private float _clientTimeSinceSentInput;
    private bool _dotMovedSinceUpdate;
    private bool _resetNextUpdate;
    
    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
    }

    private void Start()
    {
        // Start dot in the middle
        DotPosition = PageSize / 2f;
        
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        _networkManager.MessageReceived += OnMessageReceived;
        
        _stateManager = GameManager.Instance.GetService<StateManager>();

        _imageManager = GameManager.Instance.GetService<ImageManager>();
        _imageManager.ImageChanged += OnImageChanged;
    }

    private void OnMessageReceived(BitSerializable message)
    {
        switch (message)
        {
            case ClientInputMessage inputMessage:
                MoveDot(inputMessage.Direction);
                break;
            case ServerGameStateMessage gameStateMessage:
                // todo: smooth movement?
                if (gameStateMessage.DoReset)
                {
                    DotPosition = _imageManager.CurrentLevel.start;
                    DotReset?.Invoke();
                }
                else
                {
                    DotPosition = gameStateMessage.DotPosition;
                    DotMoved?.Invoke(DotPosition);
                }
                break;
        }
    }

    private void Update()
    {
        if (_networkManager.IsServer)
        {
            if (_dotMovedSinceUpdate)
            {
                // If the dot moved, tell clients the dot position
                _dotMovedSinceUpdate = false;
                _networkManager.Server.SendAll(new ServerGameStateMessage()
                {
                    DotPosition = DotPosition,
                    DoReset = _resetNextUpdate
                });

                _resetNextUpdate = false;
            }
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
                _networkManager.Client.Send(new ClientInputMessage()
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

    // Server only
    private void MoveDot(Vector2 input)
    {
        // Ignore tiny values
        if (input.magnitude < Mathf.Epsilon)
        {
            return;
        }
        
        _dotMovedSinceUpdate = true;
        
        DotPosition = new Vector2(
            Mathf.Clamp(DotPosition.x + input.x, 0, PageSize.x),
            Mathf.Clamp(DotPosition.y + input.y, 0, PageSize.y)
        );
        
        // Check for out of bounds of the outline
        if (!_imageManager.CurrentLevel.processedOutlinePixelData[(int)DotPosition.x, (int)DotPosition.y])
        {
            _resetNextUpdate = true;
            DotPosition = _imageManager.CurrentLevel.start;
            DotReset?.Invoke();
        }
        else
        {
            DotMoved?.Invoke(DotPosition);
        }
    }
    
    private void OnImageChanged()
    {
        DotPosition = _imageManager.CurrentLevel.start;
    }
}
