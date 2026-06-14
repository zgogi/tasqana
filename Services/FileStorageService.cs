using Amazon.S3;
using Amazon.S3.Model;

namespace Tasqana.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType);
        Task DeleteFileAsync(string fileName);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public FileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["S3Settings:BucketName"]
                ?? throw new ArgumentNullException("BucketName is not configured");
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
        {
            // Формируем уникальное имя файла в хранилище, чтобы избежать коллизий
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

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
    }
}
