using System;
using NetStack.Serialization;

namespace Networking
{
    public interface IClient
    {
        public event Action Connected;
        public event Action Disconnected;
        public event Action<BitBuffer> DataReceived;
        public void Send(BitBuffer data);
    }
}