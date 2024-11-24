using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public struct ServerRoleAssignmentMessage : IBitSerializable
{
    public enum Role : byte
    {
        None,
        Horizontal,
        Vertical,
    }
    public Role CurrentRole;

    public void Serialize(ref BitBuffer data)
    {
        data.AddByte((byte)CurrentRole);
    }

    public void Deserialize(ref BitBuffer data)
    {
        CurrentRole = (Role)data.ReadByte();
    }
}
