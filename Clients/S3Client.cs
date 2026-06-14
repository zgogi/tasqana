using Amazon.S3;

namespace Tasqana.Clients
{
    public static class S3Client
    {
        public static IAmazonS3 Create(WebApplicationBuilder builder)
        {
            var s3Endpoint = builder.Configuration["S3Settings:Endpoint"];
            var accessKey = builder.Configuration["S3Settings:AccessKey"];
            var secretKey = builder.Configuration["S3Settings:SecretKey"];

            var s3Config = new AmazonS3Config
            {
                ServiceURL = s3Endpoint,
                ForcePathStyle = true // КРИТИЧНО ДЛЯ MinIO: заставляет SDK использовать путь вида server/bucket вместо bucket.server
            };

            return new AmazonS3Client(accessKey, secretKey, s3Config);
        }

    }
}
