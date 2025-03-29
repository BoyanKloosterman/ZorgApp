using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using Assets.Scripts.ApiClient;
using Assets.Scripts.Model;


public class TrajectApiClient : MonoBehaviour
{
    public WebClient webClient;

    public async Awaitable<IWebRequestResponse> GetBehandelplannen()
    {
        string route = "/api/traject";
        IWebRequestResponse webRequestResponse = await webClient.SendGetRequest(route);
        return ParseListResponse(webRequestResponse);
    }

    private IWebRequestResponse ParseListResponse(IWebRequestResponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    //Debug.Log("Response data raw: " + data.Data);
                    List<Traject> traject = JsonConvert.DeserializeObject<List<Traject>>(data.Data);
                    WebRequestData<List<Traject>> parsedWebRequestData = new WebRequestData<List<Traject>>(traject);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }
}
