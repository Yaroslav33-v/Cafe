using System.Text.Json;

namespace CafeWeb.Static
{
    public static class SessionExtensions
    {
        // Методы для вставки/получения сложных данных в сессию (из неё)
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
