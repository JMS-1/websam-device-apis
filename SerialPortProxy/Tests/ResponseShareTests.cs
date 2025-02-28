using SerialPortProxy;

namespace SerialPortProxyTests;

[TestFixture]
public class ResponseShareTests
{
    [Test, CancelAfter(2500)]
    public async Task Will_Share_Response()
    {
        var count = 0;

        var cut = new ResponseShare<int, bool>((ctx) => Task.Delay(100).ContinueWith(_ => ++count));

        var first = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => cut.Execute(true)));

        Array.ForEach(first, v => Assert.That(v, Is.EqualTo(1)));

        for (; cut.IsBusy; Thread.Sleep(100))
            TestContext.Out.WriteLine("still busy - retry in 100ms");

        var second = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => cut.Execute(true)));

        Array.ForEach(second, v => Assert.That(v, Is.EqualTo(2)));
    }
}
