using NetStack.Serialization;

public struct ServerPlayingStateMessage : IBitSerializable
{
    public short ImageIndex;
    public float TimeLeft;

    public void Serialize(ref BitBuffer data)
    {
        data.AddShort(ImageIndex);
        data.AddInt((int)(TimeLeft * 1000f));
    }

    public void Deserialize(ref BitBuffer data)
    {
        ImageIndex = data.ReadShort();
        TimeLeft = data.ReadInt() / 1000f;
    }
}