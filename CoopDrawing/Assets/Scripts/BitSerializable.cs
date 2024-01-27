using System;
using NetStack.Serialization;

public interface BitSerializable
{
    void Serialize(ref BitBuffer data);
    void Deserialize(ref BitBuffer data);
}

public static class Writer
{
    public static ArraySegment<byte> SerializeToByteSegment(BitSerializable message)
    {
        BitBuffer bitBuffer = BufferPool.GetBitBuffer();
        byte[] byteBuffer = BufferPool.GetByteBuffer();
        
        message.Serialize(ref bitBuffer);
        
        bitBuffer.ToArray(byteBuffer);
        return new ArraySegment<byte>(byteBuffer, 0, bitBuffer.Length);
    }
}