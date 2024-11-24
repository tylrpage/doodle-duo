using System;

namespace Networking
{
    public interface IClient
    {
        public event Action Connected;
        public event Action Disconnected;
        public void Send(IBitSerializable message);
        
        public delegate void MessageReceivedDelegate<T>(T message) where T : IBitSerializable;
        public void AddListener<T>(MessageReceivedDelegate<T> listener) where T : IBitSerializable;
        
        public void RemoveListener<T>(MessageReceivedDelegate<T> listener) where T : IBitSerializable;
    }
}