using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class UserApiClient : MonoBehaviour
{
	public WebClient webClient;

    public async Awaitable<IWebRequestReponse> LoadCheckpointData(int checkpointId)
    {
        string route = "/api/ZorgMoment/" + checkpointId;
        return await webClient.SendGetRequest(route);
    }

    public IWebRequestReponse ProcessLoginResponse(IWebRequestReponse webRequestResponse)
	{
		switch (webRequestResponse)
		{
			case WebRequestData<string> data:
				Debug.Log("Response data raw: " + data.Data);
				string token = JsonHelper.ExtractToken(data.Data);
				webClient.SetToken(token);
                SecureUserSession.Instance.SetToken(token);
                return new WebRequestData<string>("Succes");
			default:
				return webRequestResponse;
		}
	}
}

