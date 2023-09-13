﻿using Newtonsoft.Json;

namespace CloudSystem.Model;

public class StorageAccess
{
    public static string DeleteUserDirectory(string userId)
    {
        if (!Directory.Exists($"/data/content/{userId}")) return "User Not Found";
        Directory.Delete($"/data/content/{userId}", true);
        Directory.Delete($"/data/configs/{userId}", true);
        File.Delete($"/data/users/{userId}.json");
        return "";
    }

    public static async Task CreateUserDirectory(User user)
    {
        Directory.CreateDirectory($"/data/content/{user.Id}");
        Directory.CreateDirectory($"/data/configs/{user.Id}");
        Directory.CreateDirectory("/data/users");

        await File.WriteAllTextAsync($"/data/users/{user.Id}.json", JsonConvert.SerializeObject(user));
    }

    public static async Task<User?> GetUser(string userId)
    {
        if (!File.Exists($"/data/users/{userId}.json")) return null;
        return JsonConvert.DeserializeObject<User>(await File.ReadAllTextAsync($"/data/users/{userId}.json"));
    }
    
    public static async Task UpdateUser(User user)
    {
        if (!File.Exists($"/data/users/{user.Id}.json")) return;
        await File.WriteAllTextAsync($"/data/users/{user.Id}.json", JsonConvert.SerializeObject(user));
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
    
    public static async Task<IEnumerable<FileConfig>> GetAllFiles(string userId)
    {
        var files = new List<FileConfig>();
        if (!Directory.Exists($"/data/configs/{userId}")) return files;
        foreach (var file in Directory.EnumerateFiles($"/data/configs/{userId}"))
        {
            var fileObject = JsonConvert.DeserializeObject<FileConfig>(await File.ReadAllTextAsync(file));
            if(fileObject is null) continue;
            files.Add(fileObject);
        }

        return files;
    }

    public static async Task StoreFile(string userId, string fileId, string path, Stream file)
    {
        await using var st = new FileStream($"/data/content/{userId}/{fileId}.json", FileMode.Create);
        await file.CopyToAsync(st);

        await File.WriteAllTextAsync($"/data/configs/{userId}/{fileId}.json",
            JsonConvert.SerializeObject(new FileConfig(userId, path, fileId)));
    }
    
    public static async Task<FileObject?> LoadFile(string userId, string fileId)
    {
        if (!File.Exists($"/data/content/{userId}/{fileId}.json")) return null;
        string content = await File.ReadAllTextAsync($"/data/content/{userId}/{fileId}.json");
        var config =
            JsonConvert.DeserializeObject<FileConfig>(await File.ReadAllTextAsync($"/data/configs/{userId}/{fileId}.json"));
        return config is null ? null : new FileObject(content, userId, config.Path, fileId);
    }

    public static string DeleteFile(string userId, string fileId)
    {
        if (!File.Exists($"/data/content/{userId}/{fileId}.json"))
        {
            Console.WriteLine("File Not Found");
            return "File Not Found";
        }
        File.Delete($"/data/content/{userId}/{fileId}.json");
        File.Delete($"/data/configs/{userId}/{fileId}.json");
        return "";
    }

    public class FileConfig
    {
        public string Path { get; set; }
        public string UserId { get; set; }
        public string FileId { get; set; }

        public FileConfig(string userId, string path, string fileID)
        {
            Path = path;
            UserId = userId;
            FileId = fileID;
        }
    }
}