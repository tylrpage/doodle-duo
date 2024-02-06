using NetStack.Serialization;

public struct ServerPlayingStateMessage : BitSerializable
{
    public const ushort Id = 4;

    public short ImageIndex;
    public float TimeLeft;

    public void Serialize(ref BitBuffer data)
    {
        data.AddUShort(Id);

        data.AddShort(ImageIndex);
        data.AddInt((int)(TimeLeft * 1000f));
    }

    public void Deserialize(ref BitBuffer data)
    {
        data.ReadUShort();

        ImageIndex = data.ReadShort();
        TimeLeft = data.ReadInt() / 1000f;
    }
}