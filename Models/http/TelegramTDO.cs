using System.Security.Permissions;
using Tasqana.Extensions;
using static Tasqana.Models.http.Telegram;

namespace Tasqana.Models.http
{

    public class TelegramMessageDTO
    {
        public long id {  get; set; }
        public string? text { get; set; }
        public DateTime created_at { get; set; }
        public bool is_incoming { get; set; }
        public string user { get; set; } = null!;

        public TelegramMessageDTO(Models.TelegramMessage message) { 
            id = message.Id;
            text = message.Text;
            created_at = message.CreatedAt;
            is_incoming = message.IsIncoming;
            user = message.User.Name.ToTelegramUsername();
        }
    }
    public class Telegram
    {
        public class Response
        {
            public bool ok { get; set; }
            public Message result { get; set; } = null!;
            public Response() { }
        }

        public class Update
        {
            public long update_id { get; set; }
            public Message? message { get; set; }
        }


        public class Message
        {
            public long message_id { get; set; }
            public long date { get; set; }
            public string? text { get; set; }
            public string? caption { get; set; }
            public User? from { get; set; }

            public Models.TelegramMessage ToMessage(long userId)
            {
                var result = new Models.TelegramMessage();
                result.UserId = userId;
                result.MessageId = this.message_id;
                result.IsIncoming = true;
                result.Text = this?.text ?? this?.caption ?? "";
                return result;
            }
        }

        public class User
        {
            public long id { get; set; }
            public string? first_name { get; set; }
            public string? last_name { get; set; }
            public string? username { get; set; }

            public string full_name
            {
                get {
                    return (first_name ?? "") + " " + (last_name ?? string.Empty).Trim();
                }
            }
        }

        public class SendRequest
        {
            public long chat_id { get; set; }
            public string text { get; set; } = null!;
            public ReplyMarkup? reply_markup { get; set; } = null;

            public SendRequest(long chatId, string text, ReplyMarkup? replyMarkup=null) {
                this.chat_id = chatId;
                this.text = text;
                this.reply_markup = replyMarkup;
            }
            public static SendRequest CreateLogin(long chatId, string text, string buttonTitle, string url)
            {
                var buttons = new List<InlineKeyboardButton>();
                buttons.Add(new InlineKeyboardButton(buttonTitle, url));

                var reply = new ReplyMarkup();
                reply.inline_keyboard.Add(buttons);

                return new SendRequest(chatId, text, reply);
            }
        }

        public class ReplyMarkup
        {
            public List<List<InlineKeyboardButton>> inline_keyboard {  get; set; } = new List<List<InlineKeyboardButton>>();
        }

        public class InlineKeyboardButton
        {
            public string text { get; set; } = null!;
            public string url { get; set; } = null!;
            public InlineKeyboardButton(string text, string url)
            {
                this.text = text;
                this.url = url;
            }

        }

        public class LoginUrl
        {
            public string url { get; set; } = null!;
            public LoginUrl(string url) { this.url = url; }
        }
    }



}
