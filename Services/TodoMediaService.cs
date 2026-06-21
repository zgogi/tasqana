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

        public async Task<Models.TodoFile> InsertAsync(Models.Todo todo, string fileName, Stream content, Stream? preview=null, string? title=null)
        {
            content.Position = 0;
           
            if (preview != null) preview.Position = 0;

            var contentLength = content.Length;
            var previewLength = preview?.Length;

            var contentType = _files.GetContentType(fileName);
            var loadedLile = await _files.UploadFileAsync(fileName, content, contentType);
            string? loadedPreview = null;
            string? previewContentType = null;
            if (preview != null)
            {
                loadedPreview = await _files.UploadFileAsync("preview_"+fileName, preview, contentType);
                previewContentType = contentType;
            }
                
            
            var item = new Models.TodoFile
            {
                TodoId = todo.Id,
                FileName = loadedLile,
                FileSize = contentLength,
                ContentType = contentType,
                PreviewFileName = loadedPreview,
                PreviewFileSize = previewLength,
                PreviewContentType = previewContentType,
                Title = title,
                
            };
              
            return await _repository.InsertAsync(item);
        }
    }
}
