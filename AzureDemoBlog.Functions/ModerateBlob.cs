using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace AzureDemoBlog.Functions
{
    public static class ModerateBlob
    {
        [FunctionName("ModerateBlob")]
        public static async Task RunAsync([EventHubTrigger("messagehub", Connection = "ListenerEventHubConnection")]
            string myEventHubMessage,
            [Blob("messages", FileAccess.ReadWrite, Connection ="BlobStorageConnection")] CloudBlobContainer inputContainer,
            ILogger log)
        {
            log.LogInformation($"C# Event Hub trigger function processed a message: {myEventHubMessage}");

            var eventHubMessage = JsonConvert.DeserializeObject<EventHubMessage>(myEventHubMessage);
            
            var cloudBlockBlob = inputContainer.GetBlockBlobReference(eventHubMessage.BlobName);
            var message = await cloudBlockBlob.DownloadTextAsync();

            if (message.ToLower().Contains("nazi"))
            {
                await cloudBlockBlob.DeleteAsync();
                log.LogInformation("Message deleted for inappropriate content");
                return;
            }
            
            log.LogInformation("Moderation passed");
        }
    }

    public class EventHubMessage
    {
        public string BlobName { get; set; }
    }
}