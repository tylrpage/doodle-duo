using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public struct ServerChangeImageMessage : IBitSerializable
{
    public short ImageIndex;

    public void Serialize(ref BitBuffer data)
    {
        data.AddShort(ImageIndex);
    }

    public void Deserialize(ref BitBuffer data)
    {
        ImageIndex = data.ReadShort();
    }
}
