using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct ServerGameStateMessage : IBitSerializable
{
    public Vector2 DotPosition;
    public bool DoReset;

    public enum Role
    {
        None,
        Horizontal,
        Vertical,
    }
    public Role CurrentRole;

    public void Serialize(ref BitBuffer data)
    {
        data.AddInt((int)DotPosition.x);
        data.AddInt((int)DotPosition.y);

        data.AddBool(DoReset);
    }

    public void Deserialize(ref BitBuffer data)
    {
        int dotPositionX = data.ReadInt();
        int dotPositionY = data.ReadInt();
        DotPosition = new Vector2(dotPositionX, dotPositionY);

        DoReset = data.ReadBool();
    }
}
