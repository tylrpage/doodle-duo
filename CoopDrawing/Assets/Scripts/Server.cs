﻿using System;
using System.Collections.Generic;
using System.Security.Authentication;
using JamesFrowen.SimpleWeb;
using UnityEngine;

public class Server : MonoBehaviour
{
    private SimpleWebServer _webServer;
    private bool _listening;
    private UIManager _uiManager;
    private List<int> _connectedPeers = new List<int>();
    private StateMachine _stateMachine = new StateMachine();

    private void Awake()
    {
        _uiManager = GameManager.Instance.GetService<UIManager>();
        
        Application.targetFrameRate = Constants.Tick;
        
        _webServer = Listen();
        
        _webServer.onConnect += WebServerOnonConnect;
        _webServer.onData += WebServerOnonData;
        _webServer.onError += WsOnonError;
        _webServer.onDisconnect += WebServerOnonDisconnect;
        
        _stateMachine.SetState<WaitingState>();
    }

    private void OnDestroy()
    {
        if (_listening)
        {
            _webServer.Stop();
        }
        
        _webServer.onConnect -= WebServerOnonConnect;
        _webServer.onData -= WebServerOnonData;
        _webServer.onError -= WsOnonError;
        _webServer.onDisconnect -= WebServerOnonDisconnect;
    }

    void Update()
    {
        _webServer.ProcessMessageQueue();
    }

    private SimpleWebServer Listen()
    {
        SimpleWebServer webServer;
        
        SslConfig sslConfig;
        TcpConfig tcpConfig = new TcpConfig(false, 5000, 20000);
        if (Application.isBatchMode)
        {
            Debug.Log($"Setting up secure server");
            sslConfig = new SslConfig(true, "cert-legacy.pfx", "", SslProtocols.Tls12);
        }
        else
        {
            Debug.Log($"Setting up non secure server");
            sslConfig = new SslConfig(false, "", "", SslProtocols.Tls12);
        }

        webServer = new SimpleWebServer(5000, tcpConfig, 16 * 1024, 3000, sslConfig);
        webServer.Start(Constants.GamePort);

        Debug.Log($"Server started, port: {Constants.GamePort}");
        _listening = true;
        
        _uiManager.SetStatusText("Listening...");

        return webServer;
    }
    
    private void WebServerOnonConnect(int peerId)
    {
        Debug.Log($"Client connected, id: {peerId}");
        
        _connectedPeers.Add(peerId);

        if (_stateMachine.CurrentState.GetType() == typeof(WaitingState) && _connectedPeers.Count >= 2)
        {
            // Begin game
            _stateMachine.SetState<PlayingState>();
        }
        
        // Send current state to client
        StateChange stateChange = new StateChange()
        {
            StateId = _stateMachine.GetStateId(_stateMachine.CurrentState),
        };
        ArraySegment<byte> bytes = Writer.SerializeToByteSegment(stateChange);
        _webServer.SendOne(peerId, bytes);
    }

    private void WebServerOnonDisconnect(int peerId)
    {
        Debug.Log($"Client disconnected, id: {peerId}");
        
        _connectedPeers.Remove(peerId);
    }

    private void WebServerOnonData(int peerId, ArraySegment<byte> data)
    {
    }
    
    private void WsOnonError(int connectionId, Exception exception)
    {
        Debug.LogError($"Web Server Error, Id: {connectionId}, {exception.Message}");
    }
}