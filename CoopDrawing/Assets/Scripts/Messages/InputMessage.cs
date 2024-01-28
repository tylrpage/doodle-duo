using System.Collections;
using System.Collections.Generic;
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct InputMessage : BitSerializable
{
    public const ushort Id = 2;

    public Vector2 Direction;
    public bool Rewinding;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);

        QuantizedVector2 qDirection = BoundedRange.Quantize(Direction, Constants.InputDirectionBounds);
        data.AddUInt(qDirection.x);
        data.AddUInt(qDirection.y);
        
        data.AddBool(Rewinding);
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();

        QuantizedVector2 qPosition = new QuantizedVector2(data.ReadUInt(), data.ReadUInt());
        Direction = BoundedRange.Dequantize(qPosition, Constants.InputDirectionBounds);

        Rewinding = data.ReadBool();
    }
}
