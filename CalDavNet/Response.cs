namespace CalDavNet;

public class Response
{
    public int StatusCode { get; }

    public bool IsSuccess => StatusCode >= 200 && StatusCode <= 299;

    public Response(int statusCode)
    {
        StatusCode = statusCode;
    }
}
