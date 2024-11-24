using System;
using System.Collections;
using System.Collections.Generic;
using JamesFrowen.SimpleWeb;
using NetStack.Serialization;
using Networking;
using UnityEngine;

public class WebClient : MonoBehaviour, IClient
{
    public event Action Connected;
    public event Action Disconnected;
    public event Action<IBitSerializable> MessageReceived;
    public bool IsConnected => _connected;
    
    private UIManager _uiManager;
    private SimpleWebClient _ws;
    private Messenger _messenger = new Messenger();
    private bool _connected;
    private Coroutine _heartbeatCoroutine;
    
    // todo move this into a client message router, so that web client and local client can share logic in AddListener(), Remove, on data receive
    public Dictionary<Type, IClientMessageListener> _messageListeners = new Dictionary<Type, IClientMessageListener>();

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
        if (_connected)
        {
            _ws.ProcessMessageQueue();   
        }
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
        
        IBitSerializable message = _messenger.Receive(bitBuffer);
        Type type = message.GetType();
        if (_messageListeners.TryGetValue(type, out IClientMessageListener messageListener))
        {
            messageListener.SendMessage(message);
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

    public void Send(IBitSerializable serializable)
    {
        BitBuffer bitBuffer = _messenger.Serialize(serializable);
        byte[] byteBuffer = BufferPool.GetByteBuffer();
        bitBuffer.ToArray(byteBuffer);
        ArraySegment<byte> bytes = new ArraySegment<byte>(byteBuffer, 0, bitBuffer.Length);
        _ws.Send(bytes);
    }

    public void AddListener<T>(IClient.MessageReceivedDelegate<T> listener) where T : IBitSerializable
    {
        Type type = typeof(T);
        if (!_messageListeners.TryGetValue(type, out IClientMessageListener messageListener))
        {
            _messageListeners[typeof(T)] = messageListener = new ClientMessageListener<T>();
            _messageListeners.Add(type, messageListener);
        }
        ClientMessageListener<T> typedHandler = (ClientMessageListener<T>) messageListener;
        typedHandler.AddListener(listener);
    }

    public void RemoveListener<T>(IClient.MessageReceivedDelegate<T> listener) where T : IBitSerializable
    {
        Type type = typeof(T);
        if (_messageListeners.TryGetValue(type, out IClientMessageListener messageListener))
        {
            ClientMessageListener<T> typedListener = (ClientMessageListener<T>) messageListener;
            typedListener.RemoveListener(listener);
        }
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