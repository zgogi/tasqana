using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.StaticFiles;

namespace Tasqana.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType);
        Task DeleteFileAsync(string fileName);
        string GetContentType(string fileName);
        string GetUrl(string fileName, double minutes=15);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly FileExtensionContentTypeProvider _mimeProvider;

        public FileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["S3Settings:BucketName"]
                ?? throw new ArgumentNullException("BucketName is not configured");
            _mimeProvider = new FileExtensionContentTypeProvider();
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
        {
            // Формируем уникальное имя файла в хранилище, чтобы избежать коллизий
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            fileStream.Position = 0;
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = uniqueFileName,
                InputStream = fileStream,
                ContentType = contentType
            };

            // Отправляем файл в MinIO
            await _s3Client.PutObjectAsync(putRequest);

            // Возвращаем уникальный ключ файла, который мы сохраним в базу данных (в таблицу задач)
            return uniqueFileName;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
        }

        public string GetContentType(string fileName)
        {
            if (_mimeProvider.TryGetContentType(fileName, out string? contentType))
            {
                return contentType;
            }
            else
            {
                return "application/octet-stream";
            }
        }

        public string GetUrl(string fileName, double minutes)
        {
            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                Expires = DateTime.UtcNow.AddMinutes(minutes)
            };
            return _s3Client.GetPreSignedURL(urlRequest);
        }
    }
}
