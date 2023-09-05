using Newtonsoft.Json;

namespace CloudSystem.Model;

public class StorageAccess
{
    public static void DeleteUserDirectory(string userId)
    {
        if (!Directory.Exists($"/data/content/{userId}")) return;
        Directory.Delete($"/data/content/{userId}", true);
        Directory.Delete($"/data/configs/{userId}", true);
        File.Delete($"/data/users/{userId}");
    }

    public static async Task CreateUserDirectory(User user)
    {
        Directory.CreateDirectory($"/data/content/{user.Id}");
        Directory.CreateDirectory($"/data/configs/{user.Id}");
        
        await using var wr = new StreamWriter($"/data/users/{user.Id}");
        await wr.WriteLineAsync(JsonConvert.SerializeObject(user));
    }

    public static async Task<User?> GetUser(string userId)
    {
        if (!File.Exists($"/data/users/{userId}")) return null;
        return JsonConvert.DeserializeObject<User>(await File.ReadAllTextAsync($"/data/users/{userId}"));
    }
    
    public static async Task<IEnumerable<User>> GetAllUsers()
    {
        var users = new List<User>();
        if (!Directory.Exists("/data/users")) return users;
        foreach (var file in Directory.EnumerateFiles("/data/users"))
        {
            var user = JsonConvert.DeserializeObject<User>(await File.ReadAllTextAsync(file));
            if(user is null) continue;
            users.Add(user);
        }

        return users;
    }
    
    public static async Task<IEnumerable<FileObject>> GetAllFiles()
    {
        var files = new List<FileObject>();
        if (!Directory.Exists("/data/users")) return files;
        foreach (var file in Directory.EnumerateFiles("/data/users"))
        {
            var fileObject = JsonConvert.DeserializeObject<FileObject>(await File.ReadAllTextAsync(file));
            if(fileObject is null) continue;
            files.Add(fileObject);
        }

        return files;
    }

    public static async Task StoreFile(FileObject file)
    {
        await using var wr = new StreamWriter($"/data/content/{file.IdUser}/{file.Id}");
        await wr.WriteLineAsync(file.Content);

        await using var wr2 = new StreamWriter($"/data/configs/{file.IdUser}/{file.Id}");
        await wr.WriteLineAsync(JsonConvert.SerializeObject(new FileConfig(file.IdUser, file.Path)));
    }
    
    public static async Task<FileObject?> LoadFile(string userId, string fileId)
    {
        if (!File.Exists($"/data/content/{userId}/{fileId}")) return null;
        string content = await File.ReadAllTextAsync($"/data/content/{userId}/{fileId}");
        var config =
            JsonConvert.DeserializeObject<FileConfig>(await File.ReadAllTextAsync($"/data/configs/{userId}/{fileId}"));
        return config is null ? null : new FileObject(content, userId, config.Path, fileId);
    }

    public static void DeleteFile(string userId, string fileId)
    {
        if (!File.Exists($"/data/content/{userId}/{fileId}")) return;
        File.Delete($"/data/content/{userId}/{fileId}");
        File.Delete($"/data/configs/{userId}/{fileId}");
    }

    private class FileConfig
    {
        public string Path { get; set; }
        public string UserId { get; set; }

        public FileConfig(string userId, string path)
        {
            Path = path;
            UserId = userId;
        }
    }
}