using System.Collections;
using System.Collections.Generic;
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct HeartbeatMessage : BitSerializable
{
    public const ushort Id = 6;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();
    }
}
