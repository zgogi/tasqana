using WebApi.Controllers;

namespace WebApi.Models.http
{
    public class CategoryCreateDTO
    {
        public string title { get; set; } = null!;
        public long? parent_id { get; set; }
    }

    public class CategoryUpdateDTO
    {
        public long id { get; set; }
        public string? title { get; set; } = null!;
    }

    public class  CategoryDeleteDTO
    {
        public long id { get; set; }
    }

    public class  CategoryDTO 
    {
        public long id { get; set; }
        public long? parent_id;
        public string title { get; set; } = null!;
        public int todo_count { get; set; } = 0;
        public IEnumerable<CategoryDTO> sub_categories { get; set; } = Enumerable.Empty<CategoryDTO>();

        public CategoryDTO() { }

        public CategoryDTO(Category source, IEnumerable<CategoryDTO>? items=null)
        {
            id = source.Id;
            title = source.Title;
            sub_categories = items ?? Enumerable.Empty<CategoryDTO>();
            todo_count = source.Todos.Count;
        }

        public CategoryDTO(CategoryDTO source, IEnumerable<CategoryDTO>? items = null)
        {
            id = source.id;
            parent_id = source.parent_id;
            title = source.title;
            sub_categories = items ?? Enumerable.Empty<CategoryDTO>();
            todo_count = source.todo_count;
        }
    }
}
