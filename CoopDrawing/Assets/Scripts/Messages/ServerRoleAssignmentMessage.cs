using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct ServerRoleAssignmentMessage : BitSerializable
{
    public const ushort Id = 5;

    public enum Role : byte
    {
        None,
        Horizontal,
        Vertical,
    }
    public Role CurrentRole;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);

        data.AddByte((byte)CurrentRole);
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();

        CurrentRole = (Role)data.ReadByte();
    }
}
