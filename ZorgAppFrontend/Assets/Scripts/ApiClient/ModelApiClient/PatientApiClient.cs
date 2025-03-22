using Assets.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace Assets.Scripts.ApiClient.ModelApiClient
{
    public class PatientApiClient : MonoBehaviour
    {
        public WebClient webClient;

        public async Awaitable<IWebRequestResponse> GetPatients()
        {
            string route = "/api/Patient";
            IWebRequestResponse webRequestResponse = await webClient.SendGetRequest(route);
            return ParseEnvironment2DListResponse(webRequestResponse);
        }
        private IWebRequestResponse ParseEnvironment2DResponse(IWebRequestResponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    Patient patient = JsonConvert.DeserializeObject<Patient>(data.Data);
                    WebRequestData<Patient> parsedWebRequestData = new WebRequestData<Patient>(patient);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }

        private IWebRequestResponse ParseEnvironment2DListResponse(IWebRequestResponse webRequestResponse)
        {
            switch (webRequestResponse)
            {
                case WebRequestData<string> data:
                    //Debug.Log("Response data raw: " + data.Data);
                    List<Patient> patient = JsonConvert.DeserializeObject<List<Patient>>(data.Data);
                    WebRequestData<List<Patient>> parsedWebRequestData = new WebRequestData<List<Patient>>(patient);
                    return parsedWebRequestData;
                default:
                    return webRequestResponse;
            }
        }
    }
}
