using System;
using System.Collections;
using JamesFrowen.SimpleWeb;
using NetStack.Serialization;
using UnityEngine;

public class Client : MonoBehaviour
{
    public event Action Connected;
    public event Action Disconnected;
    public event Action<BitSerializable> MessageReceived;
    public bool IsConnected => _connected;
    
    private UIManager _uiManager;
    private SimpleWebClient _ws;
    private bool _connected;
    private Coroutine _heartbeatCoroutine;

    private void Awake()
    {
        _uiManager = GameManager.Instance.GetService<UIManager>();
        
        TcpConfig tcpConfig = new TcpConfig(false, 5000, Constants.ReceiveTimeoutMS);
        _ws = SimpleWebClient.Create(16*1024, 5000, tcpConfig);
        
        _ws.onData += WsOnonData;
        _ws.onDisconnect += WsOnonDisconnect;
        _ws.onConnect += WsOnonConnect;
        _ws.onError += WsOnonError;
    }

    private void Update()
    {
        _ws.ProcessMessageQueue();
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

        _heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());
        
        Connected?.Invoke();
    }
    
    private void WsOnonDisconnect()
    {
        _connected = false;
        Debug.Log("Disconnected");
        
        if (_heartbeatCoroutine != null)
        {
            StopCoroutine(_heartbeatCoroutine);
        }

        Disconnected?.Invoke();
    }

    private void WsOnonData(ArraySegment<byte> data)
    {
        BitBuffer bitBuffer = BufferPool.GetBitBuffer();
        bitBuffer.FromArray(data.Array, data.Count);
        ushort messageId = bitBuffer.PeekUShort();
        
        BitSerializable message = null;
        // todo: get rid of this boilerplate somehow
        switch (messageId)
        {
            case ServerStateChangeMessage.Id:
                message = new ServerStateChangeMessage();
                message.Deserialize(ref bitBuffer);
                break;
            case ServerGameStateMessage.Id:
                message = new ServerGameStateMessage();
                message.Deserialize(ref bitBuffer);
                break;
            case ServerChangeImageMessage.Id:
                message = new ServerChangeImageMessage();
                message.Deserialize(ref bitBuffer);
                break;
            case ServerRoleAssignmentMessage.Id:
                message = new ServerRoleAssignmentMessage();
                message.Deserialize(ref bitBuffer);
                break;
            case HeartbeatMessage.Id:
                break;
            case ServerPlayingStateMessage.Id:
                message = new ServerPlayingStateMessage();
                message.Deserialize(ref bitBuffer);
                break;
            default:
                Debug.LogError($"Received a message with an unknown id: {messageId}");
                break;
        }
        
        if (message != null)
        {
            MessageReceived?.Invoke(message);
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

    public void Send(BitSerializable serializable)
    {
        ArraySegment<byte> bytes = Writer.SerializeToByteSegment(serializable);
        _ws.Send(bytes);
    }
    
    private IEnumerator HeartbeatCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds((Constants.ReceiveTimeoutMS / 1000f) / 2);
            
            Send(new HeartbeatMessage());
        }
    }
}