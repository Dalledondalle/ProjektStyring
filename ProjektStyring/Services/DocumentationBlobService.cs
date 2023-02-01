using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;
using ProjektStyring.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ProjektStyring.Services
{
    public class DocumentationBlobService : IDocumentationBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public DocumentationBlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        static string ContainerNameOfProject(ProjectModel project)
        {
            return project.Id.Trim().ToLower().Replace("æ", "ae").Replace("å", "aa").Replace("ø", "oe").Replace(" ", "");
        }

        public async Task CreateContainerAsync(ProjectModel project)
        {
            string containerName = ContainerNameOfProject(project);
            try
            {
            BlobContainerClient containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName, PublicAccessType.BlobContainer);
            }
            catch
            {
                Console.WriteLine("Container Alrdy Exists");
            }
        }

        public async Task UploadDocumentationAsync(ProjectModel project, string localPath, string fileName)
        {
            string localFilePath = Path.Combine(localPath, fileName);
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerNameOfProject(project));
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            using Stream uploadFileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
        }

        public async Task<Uri> UploadDocumentationAsFileAsync(ProjectModel project, FormFile file)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerNameOfProject(project));
            BlobClient blobClient = containerClient.GetBlobClient(file.FileName);
            using Stream uploadFileStream = file.OpenReadStream();
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
            return blobClient.Uri;
        }

        public async Task<List<BlobItem>> GetAllBlobsForProjectAsync(ProjectModel project)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerNameOfProject(project));
            List<BlobItem> listToReturn = new List<BlobItem>();
            await foreach (BlobItem item in containerClient.GetBlobsAsync())
            {
                listToReturn.Add(item);
            }
            return listToReturn;
        }

        public async Task DeleteDocumentationAsync(ProjectModel project, string fileName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerNameOfProject(project));
            await containerClient.DeleteBlobAsync(fileName);
        }
    }
}
