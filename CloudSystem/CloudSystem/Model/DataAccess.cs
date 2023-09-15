using Newtonsoft.Json;

namespace CloudSystem.Model;

public class DataAccess
{
    public static async Task<User?> GetUser(string id)
    {
        return await StorageAccess.GetUser(id);
    }
    
    public static async Task<IEnumerable<User>> GetUser()
    {
        return await StorageAccess.GetAllUsers();
    }
    
    public static async Task CreateUser(string id, string keyHashed)
    {
        if (await StorageAccess.GetUser(id) is not null) return;
        await StorageAccess.CreateUserDirectory(new User(id, keyHashed));
    }
    
    private static async Task<string?> GetPublicKey(string id)
    {
        var user = await StorageAccess.GetUser(id);
        return user?.PublicKey;
    }

    public static async Task<string> DeleteUser(string id, string key)
    {
        var user = await StorageAccess.GetUser(id);
        if(!CheckAuthKey(user, key)) return "Authentication not correct";
        return StorageAccess.DeleteUserDirectory(id);
    }

    private static bool CheckAuthKey(User? user, string key)
    {
        if (user?.AuthKey is null) return false;
        var userKey = user.AuthKey;
        if (userKey.Key != key) return false;
        return !(userKey.Stamp.Add(userKey.Span).Subtract(DateTime.Now).TotalMinutes < 0);
    }
    
    public static async Task<string> CreateSessionKey(string id)
    {
        string key = Guid.NewGuid().ToString();
        var publicKey = await GetPublicKey(id);
        if (publicKey is null) return string.Empty;
        string keyEncrypted = Cryptography.Encrypt(key, publicKey);
        var user = await StorageAccess.GetUser(id);
        if (user is null) return string.Empty;
        user.AuthKey = new User.AuthKeyClass(new TimeSpan(0, 15, 0), DateTime.Now, key);
        await StorageAccess.UpdateUser(user);
        return keyEncrypted;
    }
    
    public static async Task<string> GetFileList(string id)
    {
        var list = await StorageAccess.GetAllFiles(id);

        return JsonConvert.SerializeObject(list.Select(el => (el.Path, el.FileId)));
    }

    public static async Task<string> CreateFile(string idUser, string key, string fileId, string path, Stream file)
    {
        var user = await StorageAccess.GetUser(idUser);
        if (!CheckAuthKey(user, key))
        {
            Console.WriteLine("Auth not correct");
            return "Auth not correct";
        }
        
        if (StorageAccess.LoadFile(idUser, fileId))
        {
            Console.WriteLine("File already exists");
            return "File already exists";
        }
        await StorageAccess.StoreFile(idUser, fileId, path, file);
        return "";
    }
    
    public static async Task<string> DeleteFile(string idUser, string key, string idFile)
    {
        var user = await StorageAccess.GetUser(idUser);
        if (!CheckAuthKey(user, key)) return "Auth not correct";
        return StorageAccess.DeleteFile(idUser, idFile);
    }
}