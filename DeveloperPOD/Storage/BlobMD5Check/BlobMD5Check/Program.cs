using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;
using System.Security.Cryptography;

namespace BlobMD5Check
{
    class Program
    {
        static void Main()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=***;AccountKey=***;EndpointSuffix=core.windows.net";

            Console.WriteLine("=============== Upload Hash OK ===============");
            UploadHashOK(connectionString);

            Console.WriteLine("=============== Upload Hash NG ===============");
            UploadHashNG(connectionString);

            Console.WriteLine("=============== Download Hash OK ===============");
            DownloadHashOK(connectionString);
        }


        static void UploadHashOK(string connectionString)
        {
            string localPath = "./data/";
            string fileName = Guid.NewGuid().ToString() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);
            string content = "Hello, World!";

            Directory.CreateDirectory(localPath);
            File.WriteAllText(localFilePath, content);
            Console.WriteLine($"Created a local file: {localFilePath}");

            byte[] hashBytes;
            string hash;
            using (var stream = File.OpenRead(localFilePath))
            using (var md5 = MD5.Create())
            {
                hashBytes = md5.ComputeHash(stream);
                hash = Convert.ToBase64String(hashBytes);
            }
            Console.WriteLine($"Calculated a local file hash: {hash}");

            string containerName = Guid.NewGuid().ToString();
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            container.Create();
            BlobClient blobClient = container.GetBlobClient(fileName);
            Console.WriteLine($"Initialized a blob client: {containerName}");

            try
            {
                var blobHeaders = new BlobHttpHeaders { ContentHash = hashBytes };
                var blobUploadOptions = new BlobUploadOptions { HttpHeaders = blobHeaders };

                var response = blobClient.Upload(localFilePath, blobUploadOptions);
                Console.WriteLine($"Uploaded a blob. Hash of the blob uploaded: {Convert.ToBase64String(response.Value.ContentHash)}");
            }
            finally
            {
                container.Delete();
            }

        }
        static void UploadHashNG(string connectionString)
        {
            string localPath = "./data/";
            string fileName = Guid.NewGuid().ToString() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);
            string content = "Hello, World!";


            Directory.CreateDirectory(localPath);
            File.WriteAllText(localFilePath, content);
            Console.WriteLine($"Created a local file: {localFilePath}");

            string hash;
            using (var stream = File.OpenRead(localFilePath))
            using (var md5 = MD5.Create())
            {
                hash = Convert.ToBase64String(md5.ComputeHash(stream));
            }
            Console.WriteLine($"Calculated a local file hash: {hash}");

            string containerName = Guid.NewGuid().ToString();
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            container.Create();
            BlobClient blobClient = container.GetBlobClient(fileName);
            Console.WriteLine($"Initialized a blob client: {containerName}");

            try
            {
                var blobHeaders = new BlobHttpHeaders { ContentHash = new byte[16] };
                var blobUploadOptions = new BlobUploadOptions { HttpHeaders = blobHeaders };

                Console.WriteLine($"Uploading a blob with a wrong hash.");
                blobClient.Upload(localFilePath, blobUploadOptions);
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine("==============================================");
                Console.WriteLine(e.Message);
                Console.WriteLine("==============================================");
            }
            finally
            {
                container.Delete();
            }

        }
        static void DownloadHashOK(string connectionString)
        {
            string localPath = "./data/";
            string fileName = Guid.NewGuid().ToString() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);
            string content = "Hello, World!";

            Directory.CreateDirectory(localPath);
            File.WriteAllText(localFilePath, content);
            Console.WriteLine($"Created a local file: {localFilePath}");

            string containerName = Guid.NewGuid().ToString();
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            container.Create();
            BlobClient blobClient = container.GetBlobClient(fileName);
            Console.WriteLine($"Initialized a blob client: {containerName}");

            try
            {
                blobClient.Upload(localFilePath);
                var response = blobClient.DownloadContent();

                string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");
                File.WriteAllBytes(downloadFilePath, response.Value.Content.ToArray());

                string responseHash = Convert.ToBase64String(response.Value.Details.ContentHash);
                Console.WriteLine($"Downloaded the blob. Hash from blob download result: {responseHash}");

                string downloadedFileHash;
                using (var stream = File.OpenRead(downloadFilePath))
                using (var md5 = MD5.Create())
                {
                    downloadedFileHash = Convert.ToBase64String(md5.ComputeHash(stream));
                }

                if (downloadedFileHash.Equals(responseHash))
                {
                    Console.WriteLine($"Hash from downloaded file({downloadedFileHash}) and hash from response ({responseHash}) matched!");
                }

            }
            finally
            {
                container.Delete();
            }
        }
    }
}