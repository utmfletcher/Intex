using System.Runtime.Serialization.Json;
using System.Text.Json;
namespace Intex.Infastructure
{
    public static class SessionExtensions
    {
        public static void SetJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));

        }
        //get json of type t 
        public static T? GetJson<T>(this ISession session, string key)
        {
            var sessionData = session.GetString(key);
            return sessionData == null? default(T) : JsonSerializer.Deserialize<T>(sessionData);
        }
    }
}
