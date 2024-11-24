using System;
using System.Collections.Generic;

namespace Networking
{
    public interface IServerMessageListener
    {
        public void SendMessage(int peerId, object message);
    }
    
    public class ServerMessageListener<T> : IServerMessageListener where T : IBitSerializable
    {
        private readonly List<IServer.MessageReceivedDelegate<T>> _listeners = new List<IServer.MessageReceivedDelegate<T>>();

        public void AddListener(IServer.MessageReceivedDelegate<T> callback)
        {
            _listeners.Add(callback);
        }

        public void RemoveListener(IServer.MessageReceivedDelegate<T> callback)
        {
            _listeners.Remove(callback);
        }

        public void SendMessage(int peerId, object message)
        {
            T typedMessage = (T) message;
            foreach (var listener in _listeners)
            {
                listener(peerId, typedMessage);
            }
        }
    }
}