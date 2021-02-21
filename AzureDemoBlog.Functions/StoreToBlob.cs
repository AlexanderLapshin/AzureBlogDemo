using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace AzureDemoBlog.Functions
{
    public static class StoreToBlob
    {
        [FunctionName("StoreToBlob")]
        public static async Task RunAsync([QueueTrigger("messages", Connection = "QueueStorageConnection")]
            string message, string id,
            [Blob("messages", FileAccess.Write, Connection = "BlobStorageConnection")] CloudBlobContainer outputContainer,
            [EventHub("messagehub", Connection = "SenderEventHubConnection")] IAsyncCollector<string> outputEvents,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {message}");
            
            // Push to blob
            var blobName = id + ".txt";
            var cloudBlockBlob = outputContainer.GetBlockBlobReference(blobName);
            await cloudBlockBlob.UploadTextAsync(message);
            
            // Push to event hub
            await outputEvents.AddAsync(JsonConvert.SerializeObject(new {blobName}));
        }
    }
}