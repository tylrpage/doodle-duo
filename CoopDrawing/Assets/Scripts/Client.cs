using System;
using Mirror.SimpleWeb;
using NetStack.Serialization;
using UnityEngine;

public class Client : MonoBehaviour
{
    public event Action Connected;
    public event Action Disconnected;
    
    private UIManager _uiManager;
    private SimpleWebClient _ws;
    private bool _connected;
    private StateMachine _stateMachine = new StateMachine();

    private void Awake()
    {
        _uiManager = GameManager.Instance.GetService<UIManager>();
        
        TcpConfig tcpConfig = new TcpConfig(true, 5000, 20000);
        _ws = SimpleWebClient.Create(16*1024, 1000, tcpConfig);
        
        _ws.onData += WsOnonData;
        _ws.onDisconnect += WsOnonDisconnect;
        _ws.onConnect += WsOnonConnect;
        _ws.onError += WsOnonError;
    }

    private void Update()
    {
        _ws.ProcessMessageQueue(this);
    }
    
    private void OnDestroy()
    {
        if (_connected)
        {
            _ws.Disconnect();
        }
        
        _ws.onData -= WsOnonData;
        _ws.onDisconnect -= WsOnonDisconnect;
        _ws.onConnect -= WsOnonConnect;
        _ws.onError -= WsOnonError;
    }

    private void WsOnonConnect()
    {
        _connected = true;
        Debug.Log("Connected!");
        
        _uiManager.SetStatusText("Connected!");
        
        Connected?.Invoke();
    }
    
    private void WsOnonDisconnect()
    {
        _connected = false;
        Debug.Log("Disconnected");
        Disconnected?.Invoke();
    }

    private void WsOnonData(ArraySegment<byte> data)
    {
        BitBuffer bitBuffer = BufferPool.GetBitBuffer();
        bitBuffer.FromArray(data.Array, data.Count);
        ushort messageId = bitBuffer.PeekUShort();

        switch (messageId)
        {
            case StateChange.Id:
                StateChange stateChange = new StateChange();
                stateChange.Deserialize(ref bitBuffer);
                _stateMachine.SetStateId(stateChange.StateId);
                break;
        }
    }
    
    private void WsOnonError(Exception exception)
    {
        Debug.LogError($"Web Client Error: {exception.Message}");
    }

    public void Connect(bool isRemote)
    {
        UriBuilder uriBuilder;

        if (isRemote)
        {
            uriBuilder = new UriBuilder()
            {
                Scheme = "wss",
                Host = Constants.RemoteHost,
                Port = Constants.GamePort,
            };
        }
        else
        {
            uriBuilder = new UriBuilder()
            {
                Scheme = "ws",
                Host = "localhost",
                Port = Constants.GamePort
            };
        }
        
        Debug.Log("Connecting to " + uriBuilder.Uri);
        _ws.Connect(uriBuilder.Uri);
        
        _uiManager.SetStatusText("Connecting...");
    }
}