using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace online_event_booking_system.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3? _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<S3Service> _logger;

        public S3Service(IAmazonS3? s3Client, IConfiguration configuration, ILogger<S3Service> logger)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3BucketName"] ?? "event-booking-images";
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder = "events")
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty or null");

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.");

                // Validate file size (10MB max)
                if (file.Length > 10 * 1024 * 1024)
                    throw new ArgumentException("File size cannot exceed 10MB");

                // Check if AWS credentials are available
                if (_s3Client == null)
                {
                    return await SaveFileLocallyAsync(file, folder);
                }

                var key = $"{folder}/{Guid.NewGuid()}_{file.FileName}";
                
                using var stream = file.OpenReadStream();
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                await _s3Client.PutObjectAsync(request);
                return key;
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "InvalidAccessKeyId" || ex.ErrorCode == "SignatureDoesNotMatch")
            {
                _logger.LogWarning("AWS credentials not configured properly. Using local file storage fallback.");
                return await SaveFileLocallyAsync(file, folder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to S3, falling back to local storage");
                return await SaveFileLocallyAsync(file, folder);
            }
        }

        private async Task<string> SaveFileLocallyAsync(IFormFile file, string folder)
        {
            try
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads", folder);
                Directory.CreateDirectory(uploadsFolder);
                
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                
                var relativePath = $"/uploads/{folder}/{fileName}";
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file locally");
                throw;
            }
        }

        public async Task<string> UploadByteArrayAsync(byte[] data, string fileName, string contentType, string folder = "events")
        {
            try
            {
                if (data == null || data.Length == 0)
                    throw new ArgumentException("Data is empty or null");

                // Check if AWS credentials are available
                if (_s3Client == null)
                {
                    return await SaveByteArrayLocallyAsync(data, fileName, folder);
                }

                var key = $"{folder}/{Guid.NewGuid()}_{fileName}";
                
                using var stream = new MemoryStream(data);
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                await _s3Client.PutObjectAsync(request);
                return key;
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "InvalidAccessKeyId" || ex.ErrorCode == "SignatureDoesNotMatch")
            {
                _logger.LogWarning("AWS credentials not configured properly. Using local file storage fallback.");
                return await SaveByteArrayLocallyAsync(data, fileName, folder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading byte array to S3, falling back to local storage");
                return await SaveByteArrayLocallyAsync(data, fileName, folder);
            }
        }

        private async Task<string> SaveByteArrayLocallyAsync(byte[] data, string fileName, string folder)
        {
            try
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads", folder);
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                await File.WriteAllBytesAsync(filePath, data);
                
                var relativePath = $"/uploads/{folder}/{uniqueFileName}";
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving byte array locally");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string key)
        {
            try
            {
                if (_s3Client == null)
                {
                    return false;
                }

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "InvalidAccessKeyId" || ex.ErrorCode == "SignatureDoesNotMatch")
            {
                _logger.LogWarning("AWS credentials not configured properly. Cannot delete file from S3.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from S3");
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string key)
        {
            try
            {
                if (_s3Client == null)
                {
                    return key; // If it's a local file path, return as is
                }

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddHours(24)
                };

                return await _s3Client.GetPreSignedURLAsync(request);
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "InvalidAccessKeyId" || ex.ErrorCode == "SignatureDoesNotMatch")
            {
                _logger.LogWarning("AWS credentials not configured properly. Returning local file URL.");
                return key; // If it's a local file path, return as is
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating file URL from S3");
                return key; // Fallback to returning the key as is
            }
        }

        public async Task<string> GetImageUrlAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return string.Empty;

                // If it's already a URL (starts with http/https), return as-is
                if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                    return imagePath;

                // If it's a local path (starts with /), return as-is
                if (imagePath.StartsWith("/"))
                    return imagePath;

                // Otherwise, it's an S3 key, so generate the URL
                return await GetFileUrlAsync(imagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image URL for path: {Path}", imagePath);
                return string.Empty;
            }
        }
    }
}
