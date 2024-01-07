using System.Net.Quic;

namespace QuicShared;

public static class QuicShared
{
    private const string notSupportedMsg = "Quic is not supported for this OS, check https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/quic/quic-overview";

    public const long DefaultStreamErrorCode = 0x0A;
    public const long DefaultCloseErrorCode = 0x0B;
    public const string ProtocolName = "quic-protocol";

    private static bool IsSupported => QuicConnection.IsSupported;

    public static void ThrowIfNotSupported()
    {
        if (!IsSupported)
        {
            throw new NotSupportedException(notSupportedMsg);
        }
    }
}
