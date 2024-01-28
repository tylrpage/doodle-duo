using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public struct ServerStateChangeMessage : BitSerializable
{
    public const ushort Id = 1;

    public short StateId;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);

        data.AddShort(StateId);
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();

        StateId = data.ReadShort();
    }
}
