namespace CloudSystem.Model;

public class DataAccess
{
    public static User GetUser(string id)
    {
        return User.UserDb.First(p => p.Id == id);
    }
    
    public static IEnumerable<User> GetUser()
    {
        return User.UserDb;
    }
    
    public static bool CreateUser(string id, string keyHashed)
    {
        if (User.UserDb.Any(p => p.Id == id)) return false;
        User.UserDb.Add(new User(id, keyHashed));
        return true;
    }
    
    public static string? GetPublicKey(string id)
    {
        var user =  User.UserDb.FirstOrDefault(p => p.Id == id);
        if (user is null) return null;
        return user.PublicKey;
    }

    public static void DeleteUser(string id, string key)
    {
        var user = User.UserDb.FirstOrDefault(p => p.Id == id);
        if (user is null || user.AuthKey is null) return;
        if (user.AuthKey != key) return;
        User.UserDb.RemoveWhere(p => p.Id == id);
        user.AuthKey = null;
    }
    
    public static string CreateSessionKey(string id)
    {
        string key = Guid.NewGuid().ToString();
        var publicKey = GetPublicKey(id);
        if (publicKey is null) return string.Empty;
        string keyEncrypted = Cryptography.Encrypt(key, publicKey);
        var user = User.UserDb.FirstOrDefault(p => p.Id == id);
        user.AuthKey = key;
        Task.Run(() => DestroySessionKeyAfterTime(user, new TimeSpan(0, 15, 0), id));
        return keyEncrypted;
    }

    private static Task DestroySessionKeyAfterTime(User? user, TimeSpan time, string id)
    {
        Thread.Sleep(time);
        if(user is null) return Task.CompletedTask;
        user.AuthKey = null;
        return Task.CompletedTask;
    }

    public static FileObject GetFile(string id)
    {
        return FileObject.FileDb.First(p => p.IdFile == id);
    }
    
    public static IEnumerable<string> GetFileList(string id)
    {
        return FileObject.FileDb.Where(p => p.IdUser == id).Select(p => p.Name);
    }

    public static void CreateFile(string idUser, string key, FileObject file)
    {
        var user = User.UserDb.FirstOrDefault(p => p.Id == idUser);
        if (user is null) return;
        if (user.AuthKey != key) return;
        FileObject.FileDb.Add(file);
    }
    
    public static void DeleteFile(string idUser, string key, string idFile)
    {
        var user = User.UserDb.FirstOrDefault(p => p.Id == idUser);
        if (user is null) return;
        var keyLocal = user.AuthKey;
        if (keyLocal is null) return;
        if (keyLocal != key) return;
        FileObject.FileDb.RemoveWhere(p => p.IdFile == idFile);
    }
}