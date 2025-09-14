using Microsoft.AspNetCore.Http;

namespace online_event_booking_system.Services
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string folder = "events");
        Task<bool> DeleteFileAsync(string key);
        Task<string> GetFileUrlAsync(string key);
    }
}
