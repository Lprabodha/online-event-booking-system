using Microsoft.AspNetCore.Http;

namespace online_event_booking_system.Services
{
    /// <summary>
    /// Service interface for handling AWS S3 operations such as file upload, deletion, and URL retrieval.
    /// </summary>
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string folder = "events");
        Task<string> UploadByteArrayAsync(byte[] data, string fileName, string contentType, string folder = "events");
        Task<bool> DeleteFileAsync(string key);
        Task<string> GetFileUrlAsync(string key);
        Task<string> GetImageUrlAsync(string imagePath);
        string GetDirectUrl(string key);
    }
}
