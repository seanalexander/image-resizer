using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

using System.Linq;
using SerilogTimings;
using SerilogTimings.Extensions;

namespace ImageResizer
{
    public class EventGridEvent
    {
        public string Id { get; set; }
        public string Topic { get; set; }
        public string Subject { get; set; }
        public string EventType { get; set; }
        public DateTime EventTime { get; set; }
        public string DataVersion { get; set; }
        public string MetaDataVersion { get; set; }
        public object Data { get; set; }
    }
    public class BlobCreated
    {
        public string Api { get; set; }
        public Guid ClientRequestId { get; set; }
        public Guid RequestId { get; set; }
        public string eTag { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public string BlobType { get; set; }
        public string Url { get; set; }
        public string Sequencer { get; set; }
        public object StorageDiagnostics { get; set; }
    }
    public class StorageDiagnostics
    {
        public string BatchId { get; set; }
    }
    public class AzureBlobQueueReader
    {
        private readonly ILogger _logger;
        public char file_separator = System.IO.Path.DirectorySeparatorChar;
        private const string connectionString = "DefaultEndpointsProtocol=https;AccountName=tenimageresizer01;AccountKey=l4MfDtIZdZaEHkFgcsBFaMu3Ylm74bloMHN+AYcr7Xkilp91RZ50Qv5E8xtNDrCyATL9Nw5xiPVKBCIZgMODNA==;EndpointSuffix=core.windows.net";
        private const string queueName = "allimages";
        public AzureBlobQueueReader(ILogger<AzureBlobQueueReader> logger)
        {
            _logger = logger;
        }
        internal async Task RunAsync()
        {
            using (Operation.Time("Retreving Item from Azure Storage Queue: {queueName} has", queueName))
            {
                CloudStorageAccount _CloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                CloudQueueClient _CloudQueueClient = _CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue _CloudQueue = _CloudQueueClient.GetQueueReference(queueName);


                //Verify Cloud Queue Exists
                using (Operation.Time("Verify Cloud Queue Exists has"))
                {
                    bool _CloudQueueExists = await CloudQueueExists(_CloudQueue);

                    if (_CloudQueueExists)
                    {
                        //Get Cloud Queue Message
                        using (Operation.Time("Retrieving the Cloud Queue Message has"))
                        {
                            CloudQueueMessage _CloudQueueMessage = await CloudQueueGetMessageAsync(_CloudQueue);

                            if (_CloudQueueMessage != null)
                            {
                                using (Operation.Time("Deserializing the Cloud Queue Message into an Event Grid Event has"))
                                {
                                    //Deserialize Cloud Queue Message into an Event Grid Event
                                    EventGridEvent _EventGridEvent = await DeSerializeCloudQueueMessageToEventGridEvent(_CloudQueueMessage);

                                    if (_EventGridEvent != null)
                                    {
                                        //Make sure this is a BlobCreated event
                                        if (_EventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
                                        {
                                            using (Operation.Time("Retrieving the Event Data from the Event Grid Event has"))
                                            {
                                                //Retrieve Event Data from Event Grid Event
                                                JObject _jObject = await DeserializeEventGridEventData(_EventGridEvent);

                                                if (_jObject != null)
                                                {
                                                    using (Operation.Time("Deserializing the BlobCreated Item has"))
                                                    {
                                                        //Deserialize the BlobCreated Item
                                                        BlobCreated _BlobCreated = await DeserializeBlobCreated(_jObject);

                                                        if (_BlobCreated != null)
                                                        {
                                                            using (Operation.Time("Logging the BlobCreatedItem has"))
                                                            {
                                                                //Log the BlobCreatedItem
                                                                LogBlobCreatedItem(_BlobCreated);
                                                            }
                                                        }
                                                    }
                                                }
                                                await _CloudQueue.DeleteMessageAsync(_CloudQueueMessage);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LogBlobCreatedItem(BlobCreated _BlobCreated)
        {
            _logger.LogInformation($"Api: {_BlobCreated.Api}");
            _logger.LogInformation($"Content Type: {_BlobCreated.ContentType}");
            _logger.LogInformation($"Content Length: {_BlobCreated.ContentLength}");
            _logger.LogInformation($"Url: {_BlobCreated.Url}");
        }

        private async Task<bool> CloudQueueExists(CloudQueue _CloudQueue)
        {
            if (_CloudQueue != null)
            {
                return await _CloudQueue.ExistsAsync();
            }

            _logger.LogError($"The following Cloud Queue does not exist: {queueName}");
            return await Task.FromResult<bool>(false);
        }

        private async Task<BlobCreated> DeserializeBlobCreated(JObject _jObject)
        {
            if (_jObject != null)
            {
                return await Task.FromResult(_jObject.ToObject<BlobCreated>());
            }

            _logger.LogError($"BlobCreated is null");
            return await Task.FromResult<BlobCreated>(null);
        }

        private async Task<JObject> DeserializeEventGridEventData(EventGridEvent _EventGridEvent)
        {
            if (_EventGridEvent != null && _EventGridEvent.Data != null)
            {
                return await Task.FromResult(_EventGridEvent.Data as JObject);
            }

            _logger.LogError($"The queue is empty.");
            return await Task.FromResult<JObject>(null);
        }
        private async Task<CloudQueueMessage> CloudQueueGetMessageAsync(CloudQueue _CloudQueue)
        {
            bool exists = await _CloudQueue.ExistsAsync();
            if (exists)
            {
                CloudQueueMessage _CloudQueueMessage = await _CloudQueue.GetMessageAsync();

                if (_CloudQueueMessage != null)
                {
                    return await Task.FromResult(_CloudQueueMessage);
                }

                _logger.LogError($"CloudQueueMessage is null");
            }

            _logger.LogError($"CloudQueue does not Exist");
            return await Task.FromResult<CloudQueueMessage>(null);
        }
        private async Task<EventGridEvent> DeSerializeCloudQueueMessageToEventGridEvent(CloudQueueMessage _CloudQueueMessage)
        {
            if (_CloudQueueMessage != null)
            {
                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<EventGridEvent>(_CloudQueueMessage.AsString));
            }

            _logger.LogError($"CloudQueueMessage is null");
            return await Task.FromResult<EventGridEvent>(null);
        }
        private async Task<string> GetUrlFromEventData(EventGridEvent _EventGridEvent)
        {
            var data = _EventGridEvent.Data as JObject;
            var eventData = data.ToObject<BlobCreated>();

            _logger.LogError($"eventData.Url: {eventData.Url}");

            if (eventData != null && eventData.Url != null)
            {
                return await Task.FromResult(eventData.Url);
            }
            
            _logger.LogError($"The queue is empty.");
            return await Task.FromResult("");
        }
    }
}
