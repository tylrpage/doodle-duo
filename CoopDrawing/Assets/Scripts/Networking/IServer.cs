using System;
using NetStack.Serialization;

namespace Networking
{
    public interface IServer
    {
        public delegate void PeerConnectedDelegate(int peerID);
        public event PeerConnectedDelegate PeerConnected;
        
        public delegate void PeerDisconnectedDelegate(int peerID);
        public event PeerDisconnectedDelegate PeerDisconnected;
        
        public event Action<int, BitBuffer> DataReceived;
        
        public void Send(int peerID, BitBuffer data);
    }
}