using Assets.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace Assets.Scripts.ApiClient.ModelApiClient
{
    public class AfspraakApiClient : MonoBehaviour
    {
        public WebClient webClient;

        public async Task<IWebRequestResponse> GetAfspraken()
        {
            string route = "/api/Afspraak";
            IWebRequestResponse webRequestResponse = await webClient.SendGetRequest(route);
            return ParseListResponse(webRequestResponse);
        }

        public async Task<IWebRequestResponse> CreateAfspraak(Afspraak afspraak)
        {
            string route = "/api/Afspraak";
            string data = JsonConvert.SerializeObject(afspraak);
            IWebRequestResponse webRequestResponse = await webClient.SendPostRequest(route, data);
            return ParseResponse(webRequestResponse);
        }

        public async Task<IWebRequestResponse> UpdateAfspraak(Afspraak afspraak)
        {
            string route = $"/api/Afspraak/{afspraak.id}";
            string data = JsonConvert.SerializeObject(afspraak);
            IWebRequestResponse webRequestResponse = await webClient.SendPutRequest(route, data);
            return ParseResponse(webRequestResponse);
        }

        private IWebRequestResponse ParseResponse(IWebRequestResponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    Afspraak afspraak = JsonConvert.DeserializeObject<Afspraak>(data.Data);
                    WebRequestData<Afspraak> parsedWebRequestData = new WebRequestData<Afspraak>(afspraak);
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
                    List<Afspraak> afspraken = JsonConvert.DeserializeObject<List<Afspraak>>(data.Data);
                    WebRequestData<List<Afspraak>> parsedWebRequestData = new WebRequestData<List<Afspraak>>(afspraken);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }
    }
}