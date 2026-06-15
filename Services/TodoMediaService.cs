using System.Drawing;
using Tasqana.Repositories;

namespace Tasqana.Services
{
    public class TodoMediaService
    {
        private readonly TodoMediaRepository _repository;
        private readonly IFileStorageService _files;

        public TodoMediaService(TodoMediaRepository repository, IFileStorageService files)
        {
            _repository = repository;
            _files = files;
        }

        public async Task<Models.TodoMedia> InsertAsync(Models.Todo todo, string fileName, Stream content, Stream? preview=null, string? title=null)
        {
            content.Position = 0;
           
            if (preview != null) preview.Position = 0;

            var contentLength = content.Length;
            var previewLength = preview?.Length;

            var contentType = _files.GetContentType(fileName);
            var loadedLile = await _files.UploadFileAsync(fileName, content, contentType);
            string? loadedPreview = null;
            if (preview != null)
                loadedPreview = await _files.UploadFileAsync("preview_"+fileName, preview, contentType);
            
            var item = new Models.TodoMedia
            {
                TodoId = todo.Id,
                FileName = loadedLile,
                FileSize = contentLength,
                PreviewFileName = loadedPreview,
                PreviewFileSize = previewLength,
                Title = title,
                MimeType = contentType
            };
              
            return await _repository.InsertAsync(item);
        }
    }
}
