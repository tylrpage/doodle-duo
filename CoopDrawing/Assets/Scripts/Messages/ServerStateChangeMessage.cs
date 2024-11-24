using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public struct ServerStateChangeMessage : IBitSerializable
{
    public short StateId;

    public void Serialize(ref BitBuffer data)
    {
        data.AddShort(StateId);
    }

    public void Deserialize(ref BitBuffer data)
    {
        StateId = data.ReadShort();
    }
}
