

using Assets.Scripts.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ApiClient.ModelApiClient
{
    public class OuderVoogdApiClient : MonoBehaviour
    {
        public WebClient webClient;

        public async Awaitable<IWebRequestResponse> GetOuderVoogden()
        {
            string route = "/api/OuderVoogd";
            IWebRequestResponse webRequestResponse = await webClient.SendGetRequest(route);
            return ParseListResponse(webRequestResponse);
        }
        private IWebRequestResponse ParseResponse(IWebRequestResponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    OuderVoogd ouderVoogd = JsonConvert.DeserializeObject<OuderVoogd>(data.Data);
                    WebRequestData<OuderVoogd> parsedWebRequestData = new WebRequestData<OuderVoogd>(ouderVoogd);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }

        private IWebRequestResponse ParseListResponse(IWebRequestResponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    //Debug.Log("Response data raw: " + data.Data);
                    List<OuderVoogd> ouderVoogd = JsonConvert.DeserializeObject<List<OuderVoogd>>(data.Data);
                    WebRequestData<List<OuderVoogd>> parsedWebRequestData = new WebRequestData<List<OuderVoogd>>(ouderVoogd);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }
    }
}
