using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tasqana.Services;

namespace Tasqana.Repositories
{
    public class DeleteFileS3Interceptor : SaveChangesInterceptor
    {
        private readonly IFileStorageService _storageService;

        public DeleteFileS3Interceptor(IFileStorageService storageService)
        {
            _storageService = storageService;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var deletedFileEntries = context.ChangeTracker.Entries<Models.TodoFile>() 
                .Where(e => e.State == EntityState.Deleted)
                .Select(e => e.Entity)
                .ToList();

            foreach (var file in deletedFileEntries)
            {
                // Используем Fire-and-Forget
                // чтобы сбой S3 не заблокировал удаление из БД
                _ = _storageService.DeleteFileAsync(file.FileName);
                if (file.PreviewFileName != null)
                    _ = _storageService.DeleteFileAsync(file.PreviewFileName);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);

        }
    }
}
