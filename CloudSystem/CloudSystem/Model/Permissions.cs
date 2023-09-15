using System.Security.Cryptography;
using System.Text;

namespace CloudSystem.Model;

public class Permissions
{
    private static readonly Dictionary<string, int> permissions = 
        new (){{"5994471ABB01112AFCC18159F6CC74B4F511B99806DA59B3CAF5A9C173CACFC5", 0}};

    public static string DeleteAll(string key)
    {
        using SHA256 sha = SHA256.Create();
        string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
        if (!permissions.ContainsKey(hash) || permissions[hash] != 0) return "Wrong Key";
        StorageAccess.DeleteAll();
        return "";
    }
}