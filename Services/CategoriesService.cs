using Microsoft.EntityFrameworkCore;
using Tasqana.Models;
using Tasqana.Models.http;
using Tasqana.Repositories;

namespace Tasqana.Services
{
    public class CategoriesService
    {
        public readonly CategoriesRepository _categoriesRepository;

        public CategoriesService(CategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public async Task<Models.http.CategoryDTO> InsertAsync(Models.User user, string title, long? parentId = null)
        {
            var category = new Models.Category
            {
                Title = title,
                ParentId = parentId,
                UserId = user.Id,
            };
            var result = await _categoriesRepository.InsertAsync(category);
            return new Models.http.CategoryDTO(result);
        }

        public async Task<Models.http.CategoryDTO> UpdateAsync(Models.User user, Models.http.CategoryUpdateDTO value)
        {
            var category = await _categoriesRepository.GetByIdAsync(user, value.id, false);
            if (null == category) throw new Exception("Category not found");
            if (null != value.title)
                category.Title = value.title;
            await _categoriesRepository.SaveChangesAsync();
            var items = await GetTreeAsync(user, category.Id);
            return new Models.http.CategoryDTO(category, items);
        }


        public async Task<List<Models.http.CategoryDTO>> GetTreeAsync(Models.User user, long? parentId = null)
        {
            var all = await _categoriesRepository
                .Query(true)
                .Where(c => c.UserId == user.Id)
                .Select(c => new Models.http.CategoryDTO
                {
                    id = c.Id,
                    parent_id = c.ParentId,
                    title = c.Title,
                    todo_count = c.Todos.Count(t => t.State != TodoState.Completed)
                })
                .ToListAsync();

            //var all = await _categoriesRepository.GetByUser(user);
            return GetListRecursively(all, parentId);
        }

        public async Task DeleteAsync(Models.User user, long id)
        {
            await _categoriesRepository.DeleteAsync(user, id);
        }

        private List<Models.http.CategoryDTO> GetListRecursively(List<Models.http.CategoryDTO> all, long? parentId)
        {
            var result = new List<Models.http.CategoryDTO>();
            var items = all.FindAll(c => c.parent_id == parentId);
            foreach (var item in items)
            {
                var newItem = new Models.http.CategoryDTO(item, GetListRecursively(all, item.id));
                result.Add(newItem);

            }
            return result;
        }

    }
}
