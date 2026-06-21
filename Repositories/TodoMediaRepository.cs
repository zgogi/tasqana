namespace Tasqana.Repositories
{
    public class TodoMediaRepository : AbstractRepository<Models.TodoFile>
    {
        public TodoMediaRepository(TaskanaDb context)
            :base(context) { }
    }
}
