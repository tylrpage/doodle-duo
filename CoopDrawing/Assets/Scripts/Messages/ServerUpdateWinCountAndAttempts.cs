using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public struct ServerUpdateWinCountAndAttempts : BitSerializable
{
    public const ushort Id = 8;

    public uint WinCount;
    public uint Attempts;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);

        data.AddUInt(WinCount);
        data.AddUInt(Attempts);
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();

        WinCount = data.ReadUInt();
        Attempts = data.ReadUInt();
    }
}
