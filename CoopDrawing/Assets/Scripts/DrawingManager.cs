using System;
using System.IO;
using NetStack.Serialization;
using UnityEngine;

public class DrawingManager : MonoBehaviour, IService
{
    public Vector2 DotPosition { get; private set; } // Page space
    public float TimeLeft { get; private set; }
    public event Action<Vector2> DotMoved;
    public event Action DotReset;
    public event Action DrawingFinished;
    public event Action<ServerRoleAssignmentMessage.Role> RoleChanged;
    public uint WinCount { get; private set; }
    public uint Attempts { get; private set; }

    [field: SerializeField] public Vector2 PageSize { get; private set; }
    [SerializeField] private int dotSpeed;
    [SerializeField] private float requiredDistanceToEnd;
    [SerializeField] private PlayerDrawing playerDrawing;

    private NetworkManager _networkManager;
    private StateManager _stateManager;
    private ImageManager _imageManager;
    private Vector2 _clientUnsentInputTotal;
    private float _clientTimeSinceSentInput;
    private bool _dotMovedSinceUpdate;
    private bool _resetNextUpdate;
    private ServerRoleAssignmentMessage.Role _currentRole; // Client only
    
    private void Awake()
    {
        GameManager.Instance.RegisterService(this);
        playerDrawing.Init((int)PageSize.x, (int)PageSize.y);
    }

    private void Start()
    {
        // Start dot in the middle
        DotPosition = PageSize / 2f;
        
        _networkManager = GameManager.Instance.GetService<NetworkManager>();
        _networkManager.ServerMessageRouter.AddListener<ClientInputMessage>(OnClientInputMessage);
        _networkManager.ClientMessageRouter.AddListener<ServerGameStateMessage>(OnServerGameStateMessage);
        _networkManager.ClientMessageRouter.AddListener<ServerRoleAssignmentMessage>(OnServerRoleAssignmentMessage);
        _networkManager.ClientMessageRouter.AddListener<ServerPlayingStateMessage>(OnServerPlayingStateMessage);
        _networkManager.ClientMessageRouter.AddListener<ServerUpdateWinCountAndAttemptsMessage>(OnServerUpdateWinCountAndAttemptsMessage);
        
        _stateManager = GameManager.Instance.GetService<StateManager>();

        _imageManager = GameManager.Instance.GetService<ImageManager>();
        _imageManager.ImageChanged += OnImageChanged;

        if (_networkManager.IsServer)
        {
            // check if file exists
            if (!File.Exists(Constants.WinCountFilename))
            {
                WinCount = 0;
            }
            else
            {
                // Load win count
                StreamReader reader = new StreamReader(Constants.WinCountFilename);
                WinCount = uint.Parse(reader.ReadLine());
                Attempts = uint.Parse(reader.ReadLine());
                reader.Close();
            }
        }
    }

    private void OnDestroy()
    {
        _networkManager.ServerMessageRouter.RemoveListener<ClientInputMessage>(OnClientInputMessage);
        _networkManager.ClientMessageRouter.RemoveListener<ServerGameStateMessage>(OnServerGameStateMessage);
        _networkManager.ClientMessageRouter.RemoveListener<ServerRoleAssignmentMessage>(OnServerRoleAssignmentMessage);
        _networkManager.ClientMessageRouter.RemoveListener<ServerPlayingStateMessage>(OnServerPlayingStateMessage);
        _networkManager.ClientMessageRouter.RemoveListener<ServerUpdateWinCountAndAttemptsMessage>(OnServerUpdateWinCountAndAttemptsMessage);
    }

    private void OnClientInputMessage(int peerId, ClientInputMessage cim)
    {
        if (_stateManager.CurrentState == StateManager.State.Playing)
        {
            MoveDot(cim.Direction);
        }
    }

    private void OnServerGameStateMessage(ServerGameStateMessage sgsm)
    {
        if (sgsm.DoReset)
        {
            DotPosition = _imageManager.CurrentLevel.start;
            playerDrawing.Clear();
            DotReset?.Invoke();
        }
        else
        {
            DotPosition = sgsm.DotPosition;
            playerDrawing.AdvancePixelData(DotPosition);
            DotMoved?.Invoke(DotPosition);
        }
    }
    
    private void OnServerRoleAssignmentMessage(ServerRoleAssignmentMessage sram)
    {
        _currentRole = sram.CurrentRole;
        RoleChanged?.Invoke(sram.CurrentRole);
    }

    private void OnServerPlayingStateMessage(ServerPlayingStateMessage spsm)
    {
        // We have drawing manager handle this message there is one sole handler
        // and we can garuntee the image is changed first
        _imageManager.ChangeImage(spsm.ImageIndex);
        TimeLeft = spsm.TimeLeft;
    }
    
    private void OnServerUpdateWinCountAndAttemptsMessage(ServerUpdateWinCountAndAttemptsMessage suwca)
    {
        WinCount = suwca.WinCount;
        Attempts = suwca.Attempts;
    }

    private void Update()
    {
        // Update timer
        if (_stateManager.CurrentState == StateManager.State.Playing)
        {
            TimeLeft -= Time.deltaTime;
        }
        
        if (_networkManager.IsServer)
        {
            if (_stateManager.CurrentState == StateManager.State.Playing && TimeLeft < -0.25f)
            {
                // Game over
                _stateManager.ChangeServerState(StateManager.State.Restarting);
            }
            else if (_dotMovedSinceUpdate)
            {
                // If the dot moved, tell clients the dot position
                _dotMovedSinceUpdate = false;
                _networkManager.SendToAllClient(new ServerGameStateMessage()
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
            // Restrict inputs to our role
            _clientUnsentInputTotal += new Vector2(
                                           Input.GetAxis("Horizontal") * (_currentRole == ServerRoleAssignmentMessage.Role.Horizontal ? 1 : 0), 
                                           Input.GetAxis("Vertical") * (_currentRole == ServerRoleAssignmentMessage.Role.Vertical ? 1 : 0))
                                       * (dotSpeed * Time.deltaTime);
            
            if (_clientTimeSinceSentInput > Constants.Step)
            {
                _clientTimeSinceSentInput -= Constants.Step;
                
                // Send input to server
                _networkManager.SendToServer(new ClientInputMessage()
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
        
        // Check for reaching the end
        if ((DotPosition - _imageManager.CurrentLevel.end).magnitude < requiredDistanceToEnd)
        {
            // Finished the level!
            DrawingFinished?.Invoke();
            
            _stateManager.ChangeServerState(StateManager.State.Ending);
        }
        // Check for out of bounds of the outline
        else if (!_imageManager.CurrentLevel.processedOutlinePixelData[(int)DotPosition.x, (int)DotPosition.y])
        {
            _resetNextUpdate = true;
            DotPosition = _imageManager.CurrentLevel.start;
            playerDrawing.Clear();
            DotReset?.Invoke();
        }
        else
        {
            playerDrawing.AdvancePixelData(DotPosition);
            DotMoved?.Invoke(DotPosition);
        }
    }
    
    private void OnImageChanged()
    {
        DotPosition = _imageManager.CurrentLevel.start;
        playerDrawing.Clear();
        
        // Set timer
        TimeLeft = _imageManager.CurrentLevel.timeLimit;
    }

    public void ServerIncrementWinCount()
    {
        WinCount++;
        UpdateFile();
        
        _networkManager.SendToAllClient(new ServerUpdateWinCountAndAttemptsMessage()
        {
            WinCount = WinCount,
            Attempts = Attempts,
        });
    }
    
    public void ServerIncrementAttempts()
    {
        Attempts++;
        UpdateFile();
        
        _networkManager.SendToAllClient(new ServerUpdateWinCountAndAttemptsMessage()
        {
            WinCount = WinCount,
            Attempts = Attempts,
        });
    }

    private void UpdateFile()
    {
        // Save win count to file, create the file if needed
        FileStream fs = File.Open(Constants.WinCountFilename, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter writer = new StreamWriter(fs);
        writer.WriteLine(WinCount);
        writer.WriteLine(Attempts);
        writer.Close();
    }
}
