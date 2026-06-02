namespace WebApi.Models
{
    public class Todo : AbstractModel<Todo>
    {
        public long UserId { get; set; }
        public long? CategoryId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public int Order { get; set; }
        public TodoState State { get; set; }

        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;

        public List<CheckItem> CheckItems { get; set; } = new List<CheckItem>();

    }

    public enum TodoState
    {
        Waiting = 0,
        Started = 1,
        Completed = 2,
    }

    public enum Priority
    {
        Highest = 0,
        High = 1,
        Middle = 2,
        Low = 3,
        Lowest = 4,
    }
}
