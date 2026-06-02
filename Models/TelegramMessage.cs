namespace Tasqana.Models
{
    public class TelegramMessage : AbstractModel<TelegramMessage>
    {
        public bool IsIncoming { get; set; }
        public long MessageId { get; set; }
        public long UserId { get; set; }
        public string? Text { get; set; }

        public User User { get; set; } = null!;

        public TelegramMessage() { }
        public TelegramMessage(long messageId, long userId, bool isIncoming, string text) { 
            this.MessageId = messageId;
            this.UserId = userId;
            this.IsIncoming = isIncoming;
            this.Text = text;
        }



    }
}
