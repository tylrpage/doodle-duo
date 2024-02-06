using NetStack.Quantization;

public static class Constants
{
    public static readonly string RemoteHost = "tylrpage.com";
    public static readonly ushort GamePort = 9006;
    public static readonly string WinCountFilename = "win_count.txt";

    public const int Tick = 30;
    public static readonly float Step = 1f / Tick;

    public const int ReceiveTimeoutMS = 20000;
    
    public static BoundedRange[] InputDirectionBounds = new BoundedRange[]
    {
        new BoundedRange(-10f, 10f, 0.05f),
        new BoundedRange(-10f, 10f, 0.05f),
        new BoundedRange(-10f, 10f, 0.05f),
    };
}