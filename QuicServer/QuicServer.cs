using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using QuicShared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QuicServer;
public class QuicServer : IAsyncDisposable
{
    private const string notSupportedMsg = "Quic is not supported for this OS, check https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/quic/quic-overview";
    private volatile QuicListener? listener;

    public async Task StartAsync(IPEndPoint listenEndpoint, CancellationToken cancellationToken = default)
    {
        QuicShared.QuicShared.ThrowIfNotSupported();

        if (listener is not null)
        {
            throw new Exception("listener already started.");
        }

        var quicListenerOptions = new QuicListenerOptions
        {
            ListenEndPoint = listenEndpoint,
            ApplicationProtocols = [new SslApplicationProtocol(QuicShared.QuicShared.ProtocolName)],
            ConnectionOptionsCallback = ConnectionOptionsCallback
        };

        listener = await QuicListener.ListenAsync(quicListenerOptions, cancellationToken);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
          {
              while (!cancellationToken.IsCancellationRequested)
              {
                  await using var quicConnection = await listener.AcceptConnectionAsync(cancellationToken);

                  while (true)
                  {
                      await using var quicStream = await quicConnection.AcceptInboundStreamAsync(cancellationToken);
                      await ProcessAsync(quicConnection, quicStream);
                  }
              }
          }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

    }

    private async ValueTask<QuicServerConnectionOptions> ConnectionOptionsCallback(QuicConnection connection, SslClientHelloInfo info, CancellationToken token)
    {
        return new QuicServerConnectionOptions()
        {
            DefaultStreamErrorCode = QuicShared.QuicShared.DefaultStreamErrorCode,
            DefaultCloseErrorCode = QuicShared.QuicShared.DefaultCloseErrorCode,
            ServerAuthenticationOptions = new SslServerAuthenticationOptions
            {
                ApplicationProtocols = [new SslApplicationProtocol(QuicShared.QuicShared.ProtocolName)],
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.None,
            },
        };
    }


    public virtual Task ProcessAsync(QuicConnection quicConnection, QuicStream stream)
    {
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (listener is null)
        {
            return;
        }

        await listener.DisposeAsync();
        listener = null;
    }

    public static bool IsSupported => QuicListener.IsSupported;
}
