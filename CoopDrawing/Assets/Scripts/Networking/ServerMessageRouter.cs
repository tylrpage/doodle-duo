using System;
using System.Collections.Generic;
using NetStack.Serialization;

namespace Networking
{
    public class ServerMessageRouter
    {
        #region Classes
        public delegate void MessageReceivedDelegate<T>(int peerID, T message) where T : IBitSerializable;
        private interface IServerMessageListener
        {
            public void SendMessage(int peerId, object message);
        }
    
        private class ServerMessageListener<T> : IServerMessageListener where T : IBitSerializable
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

            public void SendMessage(int peerId, object message)
            {
                T typedMessage = (T) message;
                foreach (var listener in _listeners)
                {
                    listener(peerId, typedMessage);
                }
            }
        }
        #endregion
        
        private Dictionary<Type, IServerMessageListener> _messageListeners = new Dictionary<Type, IServerMessageListener>();

        public void ProcessMessage(int peerId, IBitSerializable message)
        {
            Type type = message.GetType();
            if (_messageListeners.TryGetValue(type, out IServerMessageListener messageListener))
            {
                messageListener.SendMessage(peerId, message);
            }
        }
        
        public void AddListener<T>(MessageReceivedDelegate<T> listener) where T : IBitSerializable
        {
            Type type = typeof(T);
            if (!_messageListeners.TryGetValue(type, out IServerMessageListener messageListener))
            {
                _messageListeners[type] = messageListener = new ServerMessageListener<T>();
            }
            ServerMessageListener<T> typedHandler = (ServerMessageListener<T>) messageListener;
            typedHandler.AddListener(listener);
        }

        public void RemoveListener<T>(MessageReceivedDelegate<T> listener) where T : IBitSerializable
        {
            Type type = typeof(T);
            if (_messageListeners.TryGetValue(type, out IServerMessageListener messageListener))
            {
                ServerMessageListener<T> typedListener = (ServerMessageListener<T>) messageListener;
                typedListener.RemoveListener(listener);
            }
        }
    }
}