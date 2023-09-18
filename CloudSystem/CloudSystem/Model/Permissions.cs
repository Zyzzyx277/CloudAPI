using System.Security.Cryptography;
using System.Text;

namespace CloudSystem.Model;

public static class Permissions
{
    private static readonly Dictionary<string, int> PermissionsDirectory = 
        new (){{"5994471ABB01112AFCC18159F6CC74B4F511B99806DA59B3CAF5A9C173CACFC5", 0}};

    public static string DeleteAll(string key)
    {
        using SHA256 sha = SHA256.Create();
        string hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
        if (!PermissionsDirectory.ContainsKey(hash) || PermissionsDirectory[hash] != 0) return "Wrong Key";
        StorageAccess.DeleteAll();
        return "";
    }
}