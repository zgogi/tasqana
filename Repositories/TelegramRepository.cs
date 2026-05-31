namespace WebApi.Repositories
{
    public class TelegramRepository : AbstractRepository<Models.TelegramMessage>
    {
        public TelegramRepository(TaskanaDb context)
            : base(context) { }





    }
}
