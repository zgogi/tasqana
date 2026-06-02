namespace Tasqana.Extensions
{
    public static class StrimgExtension
    {
        public static string ToTelegramUsername(this string str)
        {
            return "@" + str;
        }

        public static string NullIfEmpty(this string str)
        {
            if (str == "") return "";
            return str;
        }
    }
}
