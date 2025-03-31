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
            Debug.Log("GetPatients response: " + webRequestResponse);
            return ParseListResponse(webRequestResponse);
        }

        public async Awaitable<IWebRequestResponse> CreatePatient(Patient patient)
        {
            string route = "/api/Patient/register";
            string data = JsonConvert.SerializeObject(patient);
            IWebRequestResponse webRequestResponse = await webClient.SendPostRequest(route, data);

            return ParseResponse(webRequestResponse);
        }

        public async Awaitable<IWebRequestResponse> UpdatePatient(PatientDto patient)
        {
            string route = $"/api/Patient/{patient.id}";
            string data = JsonConvert.SerializeObject(patient);
            IWebRequestResponse webRequestResponse = await webClient.SendPutRequest(route, data);
            return ParseResponse(webRequestResponse);
        }

        public async Awaitable<IWebRequestResponse> UpdatePatientAvatar(PatientAvatarDto patient)
        {
            string route = $"/api/Patient/avatar";
            string data = JsonConvert.SerializeObject(patient);
            IWebRequestResponse webRequestResponse = await webClient.SendPutRequest(route, data);
            return ParseResponse(webRequestResponse);
        }

        private IWebRequestResponse ParseResponse(IWebRequestResponse webRequestResponse)
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

        private IWebRequestResponse ParseListResponse(IWebRequestResponse webRequestResponse)
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
