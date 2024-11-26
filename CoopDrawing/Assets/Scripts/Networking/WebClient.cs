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
    public event Action<BitBuffer> DataReceived;
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
        
        DataReceived?.Invoke(bitBuffer);
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

    public void Send(BitBuffer data)
    {
        byte[] byteBuffer = BufferPool.GetByteBuffer();
        data.ToArray(byteBuffer);
        ArraySegment<byte> bytes = new ArraySegment<byte>(byteBuffer, 0, data.Length);
        _ws.Send(bytes);
    }

    private IEnumerator HeartbeatCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds((Constants.ReceiveTimeoutMS / 1000f) / 2);
            
            //Send(new HeartbeatMessage());
        }
    }
}