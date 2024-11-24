using System;
using System.Collections.Generic;
using System.Linq;
using NetStack.Serialization;

namespace Networking
{
    public class Messenger
    {
        private Dictionary<Type, ushort> _messageTypeToId = new Dictionary<Type, ushort>();
        private Dictionary<ushort, Type> _messageIdToType = new Dictionary<ushort, Type>();

        public Messenger()
        {
            // Generate message IDs
            Type[] messageTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IBitSerializable)))
                .ToArray();
            for (ushort i = 0; i < messageTypes.Length; i++)
            {
                Type messageType = messageTypes[i];
                ushort id = i;
                _messageTypeToId[messageType] = id;
                _messageIdToType[id] = messageType;
            }
        }

        public IBitSerializable Receive(BitBuffer bitBuffer)
        {
            ushort messageId = bitBuffer.ReadUShort();
            if (!_messageIdToType.TryGetValue(messageId, out Type messageType))
            {
                throw new ArgumentException($"Unknown message ID: {messageId}");
            }
            
            IBitSerializable message = (IBitSerializable)InstanceFactory.CreateInstance(messageType);
            message.Deserialize(ref bitBuffer);
            return message;
        }
        
        public BitBuffer Serialize(IBitSerializable message)
        {
            BitBuffer bitBuffer = BufferPool.GetBitBuffer();
            
            if (!_messageTypeToId.TryGetValue(message.GetType(), out ushort messageId))
            {
                throw new ArgumentException($"Unknown message type: {message.GetType()}");
            }
            
            bitBuffer.AddUShort(messageId);
            message.Serialize(ref bitBuffer);
            
            return bitBuffer;
        }
    }
}