using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using UnityEngine;

public struct ServerUpdateWinCountAndAttemptsMessage : IBitSerializable
{
    public uint WinCount;
    public uint Attempts;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUInt(WinCount);
        data.AddUInt(Attempts);
    }

    public void Deserialize(ref BitBuffer data)
    {
        WinCount = data.ReadUInt();
        Attempts = data.ReadUInt();
    }
}
