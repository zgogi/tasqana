namespace Tasqana.Repositories
{
    public class TodoMediaRepository : AbstractRepository<Models.TodoMedia>
    {
        public TodoMediaRepository(TaskanaDb context)
            :base(context) { }
    }
}
