using Assets.Scripts.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ApiClient.ModelApiClient
{
    public class ArtsApiClient : MonoBehaviour
    {
        public WebClient webClient;

        public async Awaitable<IWebRequestResponse> GetArtsen()
        {
            string route = "/api/Arts";
            IWebRequestResponse webRequestResponse = await webClient.SendGetRequest(route);
            return ParseListResponse(webRequestResponse);
        }
        private IWebRequestResponse ParseResponse(IWebRequestResponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    Arts arts = JsonConvert.DeserializeObject<Arts>(data.Data);
                    WebRequestData<Arts> parsedWebRequestData = new WebRequestData<Arts>(arts);
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
                    List<Arts> arts = JsonConvert.DeserializeObject<List<Arts>>(data.Data);
                    WebRequestData<List<Arts>> parsedWebRequestData = new WebRequestData<List<Arts>>(arts);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }
    }
}

