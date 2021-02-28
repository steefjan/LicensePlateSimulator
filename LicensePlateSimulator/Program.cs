using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace LicensePlateSimulator
{
    internal class Program
    {
        //Service bus connection string
        private const string ServiceBusConnectionString = "<service bus connection string>";
        //Name of the queue 
        private const string QueueName = "<queue name>";
        //Folder where to find the license plates
        private const string FolderPath = @"<Your file location; path>";


        private static void Main()
        {
            Console.WriteLine("======================================================");
            Console.WriteLine("License Plate Simulator");
            Console.WriteLine("======================================================");

            //Process
            ProcessAsync().GetAwaiter().GetResult();

            //Wait
            Thread.Sleep(5000);

            //Read results
            GetMessage();
            GetMessage();
            GetMessage();
            GetMessage();
            GetMessage();

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit.");
            Console.WriteLine("======================================================");

            Console.ReadKey();
        }



        private static async Task ProcessAsync()
        {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(
                "<blob storage endpoint>");

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.                                    
            var container = blobClient.GetContainerReference("images");

            await container.CreateIfNotExistsAsync();

            // Set the permissions so the blobs are public. 
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await container.SetPermissionsAsync(permissions);

            //read license plates from folder
            foreach (var file in Directory.EnumerateFiles(FolderPath, "*.png"))
            {
                var fileName = file.Replace(FolderPath, "");
                var blockBlob = container.GetBlockBlobReference(fileName);
                await blockBlob.UploadFromFileAsync(file);

                Console.WriteLine($"Upload file : {fileName} to blob container images");

            }
        }

        private static void GetMessage()
        {
            var queueClient = QueueClient.CreateFromConnectionString(ServiceBusConnectionString, QueueName);

            var message = queueClient.Receive();
            var stream = message.GetBody<Stream>();
            var reader = new StreamReader(stream);
            var s = reader.ReadToEnd();

            Console.WriteLine($"License : {s}");

            message.Complete();
        }

    }
}
