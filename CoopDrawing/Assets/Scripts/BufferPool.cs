// Create a one-time allocation buffer pool

using System;
using NetStack.Serialization;

static class BufferPool {
    [ThreadStatic]
    private static BitBuffer bitBuffer;
    [ThreadStatic]
    private static byte[] byteBuffer;

    public static BitBuffer GetBitBuffer() {
        if (bitBuffer == null)
            bitBuffer = new BitBuffer(1024);

        bitBuffer.Clear();
        return bitBuffer;
    }
    
    public static byte[] GetByteBuffer() {
        if (byteBuffer == null)
            byteBuffer = new byte[1024];

        Array.Clear(byteBuffer, 0, 1024);
        return byteBuffer;
    }
}