using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tasqana.Models.http
{

    public class CheckItemCreateDTO
    {
        public long todo_id { get; set; }
        public string title { get; set; } = null!;

        public Models.CheckItem ToDb()
        {
            return new CheckItem
            {
                TodoId = todo_id,
                Title = title,
            };
        }
    }
    public class CheckItemDTO
    {
        public long? id { get; set; }
        public string? title { get; set; }
        public bool? is_completed { get; set; }

        public CheckItemDTO() { }

        public CheckItemDTO(Models.CheckItem source)
        {
            id = source.Id;
            title = source.Title;
            is_completed = source.IsCompleted;
        }

        public Models.CheckItem ToDb()
        {
            return new CheckItem
            {
                Title = this.title ?? "",
                IsCompleted = this.is_completed ?? false,
            };
        }
    }
}
