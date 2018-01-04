using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleBlob
{
    public class BlobOperations
    {

        string storageAccountName = "wublobstorage001";
        string storageAccountKey = "MB3zjvuovkD/weCsD6f0qysruCVoN+9SosJvQZFiLXEuZpGnD4u6DOMdiOIYXpS7udkSx6q1VbIlyRuI8XeuWQ==";
        string containerName = "blob-ops";
        string localPicsToUploadPath = @"C:\OpsgilityTraining\Images";
        string localDownloadPath;
        string pseudoFolder = "images/";

        string image1Name = "sb_1.jpg";
        string image2Name = "sb_2.jpg";
        string image3Name = "sb_3.jpg";
        string image4Name = "sb_4.jpg";
        string textFileName = "testtextfile.txt";

        CloudStorageAccount cloudStorageAccount { get; set; }
        CloudBlobClient cloudBlobClient { get; set; }
        CloudBlobContainer cloudBlobContainer { get; set; }
        CloudBlockBlob cloudBlockBlob { get; set; }
        string ConnectionString { get; set; }

        string localFile = string.Empty;

        public void SetupObjects()
        {
            containerName = containerName + "-" + System.Guid.NewGuid().ToString().Substring(0, 12);

            ConnectionString = string.Format(@"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            cloudStorageAccount = CloudStorageAccount.Parse(ConnectionString);

            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            cloudBlobContainer.CreateIfNotExists();

            BlobContainerPermissions permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            cloudBlobContainer.SetPermissions(permissions);


        }

        public void BasicBlobOps()
        {
            SetupObjects();
            UploadBlobs();
            GetListOfBlobs();

            DownloadBlobs();

            CopyBlob();
            GetListOfBlobs();

            BlobProperties();

            DeleteOneBlob();
            GetListOfBlobs();

            CleanUp();

            Console.WriteLine("Press a key");
            Console.ReadKey();

        }

        public void UploadBlobs()
        {
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image1Name);
            cloudBlockBlob.UploadFromFile(Path.Combine(localPicsToUploadPath, image1Name));

            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image2Name);
            cloudBlockBlob.UploadFromFile(Path.Combine(localPicsToUploadPath, image2Name));

            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image3Name);
            cloudBlockBlob.UploadFromFile(Path.Combine(localPicsToUploadPath, image3Name));

            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(pseudoFolder + image4Name);
            cloudBlockBlob.UploadFromFile(Path.Combine(localPicsToUploadPath, image4Name));

            string textToUpload = "Opsgility makes training fun!";
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(pseudoFolder + textFileName);
            cloudBlockBlob.UploadText(textToUpload);
        }

        private string GetFileNameFromBlobURI(Uri theUri, string containerName)
        {
            string theFile = theUri.ToString();
            int dirIndex = theFile.IndexOf(containerName);
            string oneFile = theFile.Substring(dirIndex + containerName.Length + 1,
                theFile.Length - (dirIndex + containerName.Length + 1));
            return oneFile;
        }

        public void GetListOfBlobs()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("before list");

            foreach (IListBlobItem blobItem in
                cloudBlobContainer.ListBlobs(null, true, BlobListingDetails.None))
            {
                string oneFile = GetFileNameFromBlobURI(blobItem.Uri, containerName);
                Console.WriteLine("blob name = {0}", oneFile);
            }
            Console.WriteLine("after list");

        }

        public void DownloadBlobs()
        {
            localDownloadPath = Path.Combine(localPicsToUploadPath, "downloaded");
            if (!Directory.Exists(localDownloadPath))
            {
                Directory.CreateDirectory(localDownloadPath);
            }

            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image1Name);
            if (cloudBlockBlob.Exists())
            {
                localFile = Path.Combine(localDownloadPath, image1Name);
                cloudBlockBlob.DownloadToFile(localFile, FileMode.Create);
            }

            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image2Name);
            if (cloudBlockBlob.Exists())
            {
                localFile = Path.Combine(localDownloadPath, image2Name);
                cloudBlockBlob.DownloadToFile(localFile, FileMode.Create);
            }

        }

        public void CopyBlob()
        {
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image1Name);
            CloudBlockBlob destBlob = cloudBlobContainer.GetBlockBlobReference("copyof_" + image1Name);
            destBlob.StartCopy(cloudBlockBlob);
            GetListOfBlobs();
        }

        public void BlobProperties()
        {            
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(pseudoFolder + image4Name);

            Console.WriteLine(string.Empty);
         
            Console.WriteLine("blob type = " + cloudBlockBlob.BlobType);
            Console.WriteLine("blob name = " + cloudBlockBlob.Name);
            Console.WriteLine("blob URI = " + cloudBlockBlob.Uri);

            cloudBlockBlob.FetchAttributes();

            Console.WriteLine("content type = " + cloudBlockBlob.Properties.ContentType);
            Console.WriteLine("size = " + cloudBlockBlob.Properties.Length);

            cloudBlockBlob.Properties.ContentType = "image/jpg";
            cloudBlockBlob.SetProperties();

            cloudBlockBlob.FetchAttributes();
            Console.WriteLine("content type = " + cloudBlockBlob.Properties.ContentType);

            PrintMetadata();

            cloudBlockBlob.Metadata["First"] = "number one";
            cloudBlockBlob.Metadata["Second"] = "number two";
            cloudBlockBlob.Metadata["Three"] = "number three";
            cloudBlockBlob.SetMetadata();

            PrintMetadata();
            
            cloudBlockBlob.Metadata.Clear();
            cloudBlockBlob.SetMetadata();
            PrintMetadata();

        }

        public void PrintMetadata()
        {
            //fetch the attributes of the blob to make sure they are current
            cloudBlockBlob.FetchAttributes();
            //if there is metaata, loop throught he dictionary and print it out 
            int index = 0;
            if (cloudBlockBlob.Metadata.Count > 0)
            {
                IDictionary<string, string> metadata = cloudBlockBlob.Metadata;
                foreach (KeyValuePair<string, string> oneMetadata in metadata)
                {
                    index++;
                    Console.WriteLine("metadata {0} = {1}, {2}", index,
                        oneMetadata.Key.ToString(), oneMetadata.Value.ToString());
                }
            }
            else
            {
                Console.WriteLine("No metadata found.");
            }
        }

        public void DeleteOneBlob()
        {
            //delete one blob
            string blobName = image3Name;
            cloudBlockBlob =
                cloudBlobContainer.GetBlockBlobReference(pseudoFolder + textFileName);
            cloudBlockBlob.Delete();
        }

        public void DeleteAllBlobs()
        {
            List<string> listOBlobs = new List<string>();
            foreach (IListBlobItem blobItem in
                cloudBlobContainer.ListBlobs(null, true, BlobListingDetails.None))
            {
                string oneFile = GetFileNameFromBlobURI(blobItem.Uri, containerName);
                cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(oneFile);
                cloudBlockBlob.Delete();
            }
        }

        public void CleanUp()
        {
            DeleteAllBlobs();
            GetListOfBlobs();
            cloudBlobContainer.Delete();
            if (!string.IsNullOrEmpty(localDownloadPath))
            {
                Directory.Delete(localDownloadPath, true);
            }
        }
    }
}
