using System;

namespace Networking
{
    public interface IServer
    {
        public delegate void PeerConnectedDelegate(int peerID);
        public event PeerConnectedDelegate PeerConnected;
        
        public delegate void PeerDisconnectedDelegate(int peerID);
        public event PeerDisconnectedDelegate PeerDisconnected;
        
        public void Send(int peerID, IBitSerializable message);
        
        public delegate void MessageReceivedDelegate<T>(int peerID, T message) where T : IBitSerializable;
        public void AddListener<T>(MessageReceivedDelegate<T> listener) where T : IBitSerializable;
        
        public void RemoveListener<T>(MessageReceivedDelegate<T> listener) where T : IBitSerializable;
    }
}