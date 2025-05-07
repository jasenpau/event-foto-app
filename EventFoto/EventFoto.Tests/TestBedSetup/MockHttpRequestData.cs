using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EventFoto.Tests.TestBedSetup;

public class MockHttpRequestData : HttpRequestData
{
    private FunctionContext _functionContext;

    public MockHttpRequestData(FunctionContext functionContext) : base(functionContext)
    {
        _functionContext = functionContext;
    }

    public override HttpResponseData CreateResponse()
    {
        return new MockHttpResponseData(_functionContext);
    }

    public override Stream Body { get; }
    public override HttpHeadersCollection Headers { get; }
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities { get; }
    public override string Method { get; }
}

public class MockHttpResponseData : HttpResponseData
{
    public MockHttpResponseData(FunctionContext functionContext) : base(functionContext)
    {
        Body = new MemoryStream();
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
    public override HttpCookies Cookies { get; }
}
