using UnityEngine;

public class WebRequestError: IWebRequestResponse
{
    public string ErrorMessage;

    public WebRequestError(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
