using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace FunctionRemoveStudentsFromProjects
{
    public static class ClearStudents
    {
        [FunctionName("ClearStudents")]
        public static void Run([TimerTrigger("* 30 17 * * *")]TimerInfo myTimer, ILogger log)
        {
            //For debugging timer on the server
            log.LogInformation(DateTime.Now.ToString());

            RestClient client = new RestClient("https://opgavestyring.skpdata.dk/api");
            RestRequest request = new RestRequest("ClearStudentsFromProjects", Method.GET);
            IRestResponse response = client.Execute(request);
        }
    }
}
