using System.Net;

namespace QuickServer.Tests;

public class UnitTest1
{
    [Fact]
    public async Task EchoTest()
    {
        var endpoint = new IPEndPoint(IPAddress.Loopback, 5554);

        await using var server = new QuicServer.EchoQuicServer();
        await server.StartAsync(endpoint);

        await using var client = new QuicClient.QuicClient();
        await client.StartAsync(endpoint);

        var request = new byte[] { 1 };
        var response = await client.SendAsync(request);

        Assert.Equal(request, response);    
    }
}