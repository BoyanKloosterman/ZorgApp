using UnityEngine;

public class WebRequestData<T> : IWebRequestResponse
{
    public readonly T Data;

    public WebRequestData(T data)
    {
        Data = data;
    }
}
