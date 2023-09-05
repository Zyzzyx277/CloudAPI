
namespace CloudSystem.Model;

public class User
{
    public User(string id, string publicKey)
    {
        Id = id;
        PublicKey = publicKey;
    }

    public string Id { get; set; }
    public string PublicKey { get; set; }
    public AuthKeyClass? AuthKey { get; set; }

    public class AuthKeyClass
    {
        public AuthKeyClass(TimeSpan span, DateTime stamp, string key)
        {
            Span = span;
            Stamp = stamp;
            Key = key;
        }

        public TimeSpan Span { get; set; }
        public DateTime Stamp { get; set; }
        public string Key { get; set; }
    }
}