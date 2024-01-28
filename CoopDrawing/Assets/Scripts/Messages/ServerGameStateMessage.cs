using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct ServerGameStateMessage : BitSerializable
{
    public const ushort Id = 3;

    public Vector2 DotPosition;
    public bool DoReset;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);

        data.AddInt((int)DotPosition.x);
        data.AddInt((int)DotPosition.y);

        data.AddBool(DoReset);
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();

        int dotPositionX = data.ReadInt();
        int dotPositionY = data.ReadInt();
        DotPosition = new Vector2(dotPositionX, dotPositionY);

        DoReset = data.ReadBool();
    }
}
