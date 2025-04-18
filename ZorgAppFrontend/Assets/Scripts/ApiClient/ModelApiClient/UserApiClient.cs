using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class UserApiClient : MonoBehaviour
{
	public WebClient webClient;

    public async Awaitable<IWebRequestResponse> LoadBehaaldeZorgMomenten()
    {
        string route = "/api/UserZorgMoment";
        return await webClient.SendGetRequest(route);
    }

    public async Awaitable<IWebRequestResponse> LoadZorgMomenten()
    {
        string route = "/api/ZorgMoment";
        return await webClient.SendGetRequest(route);
    }

    public async Awaitable<IWebRequestResponse> Logout()
    {
        string route = "/account/logout";
        string data = "";
        return await webClient.SendPostRequest(route, data);
    }

    public async Awaitable<IWebRequestResponse> LoadZorgMomentData(int zorgMomentId)
    {
        string route = "/api/ZorgMoment/" + zorgMomentId;
        Debug.Log("ZorgMomentId: " + zorgMomentId); 
        return await webClient.SendGetRequest(route);
    }

    public async Awaitable<IWebRequestResponse> FinishZorgMoment(int zorgMomentId)
    {
        string route = "/api/UserZorgMoment";

        // Gebruik een concrete klasse i.p.v. anoniem object
        var dataObject = new ZorgmomentRequest { ZorgMomentId = zorgMomentId };
        string data = JsonUtility.ToJson(dataObject);

        //Debug.Log("Verzonden JSON: " + data); // Controleer nu de output

        return await webClient.SendPostRequest(route, data);
    }

    //tijdelijk
    [System.Serializable]
    public class ZorgmomentRequest
    {
        public int ZorgMomentId; 
    }

    public async Awaitable<IWebRequestResponse> Register(User user)
	{
		string route = "/api/Auth/register";
		string data = JsonUtility.ToJson(user);

		return await webClient.SendPostRequest(route, data);
	}

    public async Awaitable<IWebRequestResponse> Login(User user)
	{
		string route = "/account/login";
		string data = JsonUtility.ToJson(user);

		IWebRequestResponse response = await webClient.SendPostRequest(route, data);
		return ProcessLoginResponse(response);
	}

    public async Awaitable<IWebRequestResponse> GetCurretnUserRole()
    {
        string route = "/api/Auth/role";
        return await webClient.SendGetRequest(route);
    }

    private IWebRequestResponse ProcessLoginResponse(IWebRequestResponse webRequestResponse)
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

