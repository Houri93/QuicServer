using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuicClient;
public class QuicClient : IAsyncDisposable
{
    private QuicConnection? quicConnection;

    public async Task StartAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
    {
        QuicShared.QuicShared.ThrowIfNotSupported();

        var quicClientConnectionOptions = new QuicClientConnectionOptions
        {
            RemoteEndPoint = endPoint,
            DefaultCloseErrorCode = QuicShared.QuicShared.DefaultCloseErrorCode,
            DefaultStreamErrorCode = QuicShared.QuicShared.DefaultStreamErrorCode,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = [new SslApplicationProtocol(QuicShared.QuicShared.ProtocolName)],               
            },
        };

        quicConnection = await QuicConnection.ConnectAsync(quicClientConnectionOptions, cancellationToken);
    }

    public async Task<byte[]?> SendAsync(ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
    {
        if (quicConnection is null)
        {
            throw new Exception("Quic connection is yet started");
        }
        await using var quicStream = await quicConnection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, cancellationToken);
        await quicStream.WriteAsync(content, cancellationToken);

        using var memoryStream = new MemoryStream();
        await quicStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public async ValueTask DisposeAsync()
    {
        if (quicConnection is null)
        {
            return;
        }

        await quicConnection.DisposeAsync();
    }
}
