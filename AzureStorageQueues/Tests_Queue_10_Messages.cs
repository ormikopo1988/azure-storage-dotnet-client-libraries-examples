using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using NFluent;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageQueues
{
    [TestClass]
    public class Tests_Queue_10_Messages
    {
        private static CloudQueueClient _devQueueClient = null;

        [ClassInitialize]
        public static void Class_Init(TestContext context)
        {
            var storageAccountDevCredentials = CloudStorageAccount.DevelopmentStorageAccount;

            _devQueueClient = storageAccountDevCredentials.CreateCloudQueueClient();
        }

        [TestMethod]
        public async Task Test_10_SendReceiveMessages()
        {
            var testQueueName = "test-q-send-receive";

            var theQueue = _devQueueClient.GetQueueReference(testQueueName);

            await theQueue.CreateIfNotExistsAsync();

            await theQueue.ClearAsync(); // clear the queue

            var data = "the quick brown fox jumps over the lazy dog";

            var msgToSend = new CloudQueueMessage(data);

            await theQueue.AddMessageAsync(msgToSend);

            var msgReceived = await theQueue.GetMessageAsync();

            Check.That(msgReceived.AsString).IsEqualTo(data).And.IsEqualTo(msgToSend.AsString);

            Check.That(msgReceived.DequeueCount).IsEqualTo(1); // we are the first ones to look at the message

            await theQueue.DeleteMessageAsync(msgReceived);
        }

        [TestMethod]
        public async Task Test_11_MsgOperations()
        {
            var testQName = "test-q-visibility";

            var theQueue = _devQueueClient.GetQueueReference(testQName);

            await theQueue.CreateIfNotExistsAsync();

            await theQueue.ClearAsync();

            var data = "my data";

            var msgToSend = new CloudQueueMessage(data);

            await theQueue.AddMessageAsync(msgToSend);

            var msgReceived = await theQueue.GetMessageAsync();

            Check.That(msgReceived.NextVisibleTime).IsNotNull();
            Check.That(msgReceived.NextVisibleTime > DateTimeOffset.Now).IsTrue();
            Check.That(msgReceived.NextVisibleTime < DateTimeOffset.Now.AddSeconds(30)).IsTrue();

            // extend the visibility of the message on the queue for 5 seconds
            await theQueue.UpdateMessageAsync(msgReceived, TimeSpan.FromSeconds(5), MessageUpdateFields.Visibility);

            await theQueue.DeleteMessageAsync(msgReceived);
        }

        [TestMethod]
        public async Task Test_12_ReceiveBatch()
        {
            var testQueueName = "test-q-batch";

            var theQueue = _devQueueClient.GetQueueReference(testQueueName);

            await theQueue.CreateIfNotExistsAsync();

            await theQueue.ClearAsync();

            var data = "my data";

            var msgToSend = new CloudQueueMessage(data);

            await theQueue.AddMessageAsync(msgToSend);
            await theQueue.AddMessageAsync(msgToSend);
            await theQueue.AddMessageAsync(msgToSend);

            var messages = await theQueue.GetMessagesAsync(3);

            foreach(var msg in messages)
            {
                // here process each msg but get them as a batch at first
                await theQueue.DeleteMessageAsync(msg);
            }
        }

        [TestMethod]
        public async Task Test_13_MaxMessageSize()
        {
            var maxMsgSize = (int)CloudQueueMessage.MaxMessageSize;

            Check.That(maxMsgSize).IsEqualTo(64 * 1024);

            var testQName = "test-q-max-message";

            var theQueue = _devQueueClient.GetQueueReference(testQName);

            await theQueue.CreateIfNotExistsAsync();

            await theQueue.ClearAsync();

            var rawData = new byte[48 * 1024]; // 48 KB

            var binaryMsg = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(rawData);

            await theQueue.AddMessageAsync(binaryMsg);

            var data = string.Empty.PadLeft(48 * 1024, '*');

            Check.That(Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(data)).Length).IsStrictlyLessThan(maxMsgSize);

            var sMsg = new CloudQueueMessage(data);

            await theQueue.AddMessageAsync(sMsg);
        }
    }
}
