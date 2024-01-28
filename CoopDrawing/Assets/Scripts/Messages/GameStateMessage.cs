using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct GameStateMessage : BitSerializable
{
    public const ushort Id = 3;

    public Vector2 DotPosition;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);

        data.AddInt((int)DotPosition.x);
        data.AddInt((int)DotPosition.y);
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();

        int dotPositionX = data.ReadInt();
        int dotPositionY = data.ReadInt();
        DotPosition = new Vector2(dotPositionX, dotPositionY);
    }
}
