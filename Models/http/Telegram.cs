using static WebApi.Models.http.Telegram;

namespace WebApi.Models.http
{
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
            public User? from { get; set; }
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




    /*

    public class TelegramUpdate
    {
        public long update_id { get; set; }
        public TelegramMessage? message { get; set; }

    }

    public class TelegramMessage
    {
        public long message_id { get; set; }
        public long date { get; set; }
        public string? text { get; set; }
        public TelegramUser? from { get; set; }

    }

    public class TelegramUser
    {
        public long id { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? username { get; set; }
        
    }

    public class TelegramSendResponse
    {
        public bool ok { get; set; }
        public TelegramMessage result { get; set; } = null!;

        public TelegramSendResponse() { }
    }*/
}
