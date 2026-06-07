using Tasqana.Models;

namespace Tasqana.Extensions
{
    public static class EnumExtensions
    {
        /*public static T? ParseNum<T>(this T en, int? value) where T : Enum 
        {
            if (value == null) return default(T?);
            var v = value ?? 0;
            if (Enum.IsDefined(typeof(T), v))
                return (T)(object)v;
            else
                return default(T?);
        }*/

        public static T? ToEnum<T>(this int? value) where T : struct, Enum
        {
            if (value == null) return null;

            if (Enum.IsDefined(typeof(T), value.Value))
                return (T)(object)value.Value;

            return null;
        }

    }
}
