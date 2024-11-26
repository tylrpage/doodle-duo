using System;
using System.Collections.Generic;
using NetStack.Serialization;

namespace Networking
{
    public class ClientMessageRouter
    {
        #region Classes
        public delegate void MessageReceivedDelegate<T>(T message) where T : IBitSerializable;
        private interface IClientMessageListener
        {
            public void SendMessage(object message);
        }
    
        private class ClientMessageListener<T> : IClientMessageListener where T : IBitSerializable
        {
            private readonly List<MessageReceivedDelegate<T>> _listeners = new List<MessageReceivedDelegate<T>>();

            public void AddListener(MessageReceivedDelegate<T> callback)
            {
                _listeners.Add(callback);
            }

            public void RemoveListener(MessageReceivedDelegate<T> callback)
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
        #endregion
        
        private Dictionary<Type, IClientMessageListener> _messageListeners = new Dictionary<Type, IClientMessageListener>();

        public void ProcessMessage(IBitSerializable message)
        {
            Type type = message.GetType();
            if (_messageListeners.TryGetValue(type, out IClientMessageListener messageListener))
            {
                messageListener.SendMessage(message);
            }
        }
        
        public void AddListener<T>(MessageReceivedDelegate<T> listener) where T : IBitSerializable
        {
            Type type = typeof(T);
            if (!_messageListeners.TryGetValue(type, out IClientMessageListener messageListener))
            {
                _messageListeners[type] = messageListener = new ClientMessageListener<T>();
            }
            ClientMessageListener<T> typedHandler = (ClientMessageListener<T>) messageListener;
            typedHandler.AddListener(listener);
        }

        public void RemoveListener<T>(MessageReceivedDelegate<T> listener) where T : IBitSerializable
        {
            Type type = typeof(T);
            if (_messageListeners.TryGetValue(type, out IClientMessageListener messageListener))
            {
                ClientMessageListener<T> typedListener = (ClientMessageListener<T>) messageListener;
                typedListener.RemoveListener(listener);
            }
        }
    }
}