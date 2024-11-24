using System;
using System.Collections.Generic;

namespace Networking
{
    public interface IClientMessageListener
    {
        public void SendMessage(object message);
    }
    
    public class ClientMessageListener<T> : IClientMessageListener where T : IBitSerializable
    {
        private readonly List<IClient.MessageReceivedDelegate<T>> _listeners = new List<IClient.MessageReceivedDelegate<T>>();

        public void AddListener(IClient.MessageReceivedDelegate<T> callback)
        {
            _listeners.Add(callback);
        }

        public void RemoveListener(IClient.MessageReceivedDelegate<T> callback)
        {
            _listeners.Remove(callback);
        }

        public void SendMessage(object message)
        {
            T typedMessage = (T) message;
            foreach (var listener in _listeners)
            {
                listener(typedMessage);
            }
        }
    }
}