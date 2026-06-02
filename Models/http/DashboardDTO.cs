namespace Tasqana.Models.http
{
    public class DashboardDTO
    {
        public List<CategoryDTO> categories { get; set; } = null!;
        public List<TodoDTO> unsorted { get; set; } = null!;


    }
}
