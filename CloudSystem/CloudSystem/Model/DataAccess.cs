using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CloudSystem.Model;

public static class DataAccess
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
        if(!await CheckAuthKey(user, key)) return "Authentication not correct";
        return StorageAccess.DeleteUserDirectory(id);
    }

    private static async Task<bool> CheckAuthKey(User? user, string key)
    {
        if (user is null) return false;
        user.Tries.RemoveWhere(p => p.Subtract(DateTime.Now).TotalMinutes > 1440); // 24hr
        if (user.Tries.Count > 4)
        {
            Console.WriteLine("Too many tries");
            return false;
        }
        if (user.AuthKey is null)
        {
            Console.WriteLine("Auth Key not existing");
            return false;
        }
        var userKey = user.AuthKey;

        if(userKey.Key != key) 
        {
            Console.WriteLine("Key not correct");
            await AddTry(user, true);
            return false;
        }
        
        if (userKey.Stamp.Add(userKey.Span).Subtract(DateTime.Now).TotalMilliseconds < 0)
        {
            Console.WriteLine("Auth Key too old");
            await AddTry(user, false);
            return false;
        }

        await AddTry(user, false);
        return true;
    }

    private static async Task AddTry(User user, bool addTry)
    {
        if(addTry) user.Tries.Add(DateTime.Now);
        else user.Tries.Clear();
        user.AuthKey = null;
        await StorageAccess.UpdateUser(user);
    }
    
    public static async Task<string> CreateSessionKey(string id)
    {
        string key;
        try
        {
            // Create an instance of RNGCryptoServiceProvider
            using var rng = RandomNumberGenerator.Create();
            // Define the size of the byte array (e.g., 16 bytes for a random 128-bit value)
            int byteCount = 128; // Adjust the size as needed

            // Generate a random byte array
            byte[] randomBytes = new byte[byteCount];
            rng.GetBytes(randomBytes);
            
            key = Convert.ToBase64String(randomBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return string.Empty;
        }
        
        var publicKey = await GetPublicKey(id);
        if (publicKey is null) return string.Empty;
        string keyEncrypted = Cryptography.Encrypt(key, publicKey);
        var user = await StorageAccess.GetUser(id);
        if (user is null) return string.Empty;
        user.AuthKey = new User.AuthKeyClass(new TimeSpan(0, 2, 0), DateTime.Now, key);
        await StorageAccess.UpdateUser(user);
        return keyEncrypted;
    }
    
    public static async Task<string> GetFileList(string id)
    {
        var list = await StorageAccess.GetAllFiles(id);

        return JsonConvert.SerializeObject(list.Select(el => new ConfigClient(el.Path, el.FileId, el.Compress)));
    }

    private class ConfigClient
    {
        public ConfigClient(string path, string fileId, bool compressed)
        {
            Path = path;
            FileId = fileId;
            Compressed = compressed;
        }

        public string Path { get; set; }
        public string FileId { get; set; }
        public bool Compressed { get; set; }
    }

    public static async Task<string> CreateFile(string idUser, string key, string fileId, string path, bool compress, Stream file)
    {
        var user = await StorageAccess.GetUser(idUser);
        if (!await CheckAuthKey(user, key))
        {
            Console.WriteLine("Auth not correct");
            return "Auth not correct";
        }
        
        if (StorageAccess.LoadFile(idUser, fileId))
        {
            Console.WriteLine("File already exists");
            return "File already exists";
        }
        await StorageAccess.StoreFile(idUser, fileId, path, compress, file);
        return "";
    }
    
    public static async Task<string> DeleteFile(string idUser, string key, string idFile)
    {
        var user = await StorageAccess.GetUser(idUser);
        if (!await CheckAuthKey(user, key)) return "Auth not correct";
        return StorageAccess.DeleteFile(idUser, idFile);
    }
}