using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Text;
using System.Threading.Tasks;

namespace QuicServer;
public sealed class EchoQuicServer : QuicServer
{
    public override async Task ProcessAsync(QuicConnection quicConnection, QuicStream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        if (stream.CanWrite)
        {
            await stream.WriteAsync(memoryStream.ToArray());
        }
    }
}
