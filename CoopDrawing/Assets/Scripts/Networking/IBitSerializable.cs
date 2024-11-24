using System;
using NetStack.Serialization;

public interface IBitSerializable
{
    void Serialize(ref BitBuffer data);
    void Deserialize(ref BitBuffer data);
}