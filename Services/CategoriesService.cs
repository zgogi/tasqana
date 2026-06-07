using Microsoft.EntityFrameworkCore;
using Tasqana.Extensions;
using Tasqana.Models;
using Tasqana.Models.http;
using Tasqana.Repositories;

namespace Tasqana.Services
{
    public class CategoriesService
    {
        public readonly CategoriesRepository _categories;

        public CategoriesService(CategoriesRepository categoriesRepository)
        {
            _categories = categoriesRepository;
        }

        public async Task<Models.http.CategoryDTO> InsertAsync(Models.User user, string title, long? parentId = null)
        {
            var category = new Models.Category
            {
                Title = title,
                ParentId = parentId,
                UserId = user.Id,
            };
            var result = await _categories.InsertAsync(category);
            return new Models.http.CategoryDTO(result);
        }

        public async Task<Models.http.CategoryDTO> UpdateAsync(Models.User user, Models.http.CategoryUpdateDTO value)
        {
            var category = await _categories.GetByIdAsync(user, value.id, false);
            if (null == category) throw new Exception("Category not found");
            if (null != value.title)
                category.Title = value.title;
            await _categories.SaveChangesAsync();
            var items = await GetTreeAsync(user, category.Id);
            return new Models.http.CategoryDTO(category, items);
        }


        public async Task<List<Models.http.CategoryDTO>> GetTreeAsync(Models.User user, long? parentId = null)
        {
            var all = await _categories
                .Query(true)
                .Where(c => c.UserId == user.Id)
                .OrderBy(e => e.Order)
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
            await _categories.DeleteAsync(user, id);
        }

        public async Task MoveAsync(Models.User user, Models.http.ReorderDTO form)
        {
            if (form.before_id != null)
            {
                var item = await _categories.GetByIdAsync(user, form.id, false);
                if (item == null) throw new NotFoundException();

                var before = await _categories.GetByIdAsync(user, form.before_id ?? 0, true);
                if (before == null) throw new NotFoundException();

                var items = await _categories.GetByUserAndParentAsync(user, before.ParentId, false);

                if (!items.Contains(item))
                {
                    item.ParentId = before.ParentId;
                    items.Add(item);
                }

                items.MoveBefore(form.id, form.before_id);
                await _categories.SaveChangesAsync();

            } 
            else
            { // TODO Put at end of all

            }
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
