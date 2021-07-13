using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace API.Abstraction
{
    public interface IPhotoUpload
    {
        public Task<ImageUploadResult> Upload(IFormFile file);
        public Task<DeletionResult> Delete(string publicId);
    }
}