using System;

namespace Networking
{
    public interface IClient
    {
        public event Action Connected;
        public event Action Disconnected;
        public void Send(IBitSerializable message);
        public void AddListener(Action<IBitSerializable> listener);
    }
}