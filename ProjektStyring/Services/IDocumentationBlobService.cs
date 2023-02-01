using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using ProjektStyring.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjektStyring.Services
{
    public interface IDocumentationBlobService
    {
        Task CreateContainerAsync(ProjectModel project);
        Task DeleteDocumentationAsync(ProjectModel project, string fileName);
        Task<List<BlobItem>> GetAllBlobsForProjectAsync(ProjectModel project);
        Task UploadDocumentationAsync(ProjectModel project, string localPath, string fileName);
        Task<Uri> UploadDocumentationAsFileAsync(ProjectModel project, FormFile file);
    }
}