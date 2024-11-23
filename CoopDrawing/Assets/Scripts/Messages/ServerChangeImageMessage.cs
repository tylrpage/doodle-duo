using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public struct ServerChangeImageMessage : IBitSerializable
{
    public const ushort Id = 7;

    public short ImageIndex;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);

        data.AddShort(ImageIndex);
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();

        ImageIndex = data.ReadShort();
    }
}
