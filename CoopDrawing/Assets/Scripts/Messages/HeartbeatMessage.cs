using System.Collections;
using System.Collections.Generic;
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct HeartbeatMessage : IBitSerializable
{
    public void Serialize(ref BitBuffer data)
    {
    }

    public void Deserialize(ref BitBuffer data)
    {
    }
}
