using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace CloudSystem.Model;

public static class StorageAccess
{
    public static readonly Semaphore Sem = new (1, 1);
    private static readonly UnixFileMode FileAccessPermissions = UnixFileMode.GroupRead | UnixFileMode.OtherRead 
                                                                                        | UnixFileMode.UserRead
                                                                                        | UnixFileMode.UserWrite;
    private static readonly bool OnLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static async Task CreateUserDirectory(User user)
    {
        try
        {
            Sem.WaitOne();
            Directory.CreateDirectory($"/data/content/{user.Id}");
            Directory.CreateDirectory($"/data/configs/{user.Id}");
            Directory.CreateDirectory("/data/users");

            await File.WriteAllTextAsync($"/data/users/{user.Id}.json", JsonConvert.SerializeObject(user));
            if(OnLinux) File.SetUnixFileMode($"/data/users/{user.Id}.json", FileAccessPermissions);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Handle UnauthorizedAccessException here
            Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
        }
        catch (IOException ex)
        {
            // Handle IOException here
            Console.WriteLine($"IOException: {ex.Message}");
        }
        finally
        {
            Sem.Release();
        }
    }

    public static async Task<User?> GetUser(string userId)
    {
        try
        {
            Sem.WaitOne();
            if (!File.Exists($"/data/users/{userId}.json")) return null;

            var data = JsonConvert.DeserializeObject<User>(await File.ReadAllTextAsync($"/data/users/{userId}.json"));
            return data;
        }
        finally
        {
            Sem.Release();
        }
    }
    
    public static async Task UpdateUser(User user)
    {
        try
        {
            Sem.WaitOne();
            if (!File.Exists($"/data/users/{user.Id}.json")) return;

            await File.WriteAllTextAsync($"/data/users/{user.Id}.json", JsonConvert.SerializeObject(user));
        }
        finally
        {
            Sem.Release();
        }
    }
    
    public static async Task<IEnumerable<User>> GetAllUsers()
    {
        try
        {
            Sem.WaitOne();
            var users = new List<User>();
            if (!Directory.Exists("/data/users")) return users;

            foreach (var file in Directory.EnumerateFiles("/data/users"))
            {
                var user = JsonConvert.DeserializeObject<User>(await File.ReadAllTextAsync(file));
                if (user is null) continue;
                users.Add(user);
            }

            return users;
        }
        finally
        {
            Sem.Release();
        }
    }
    
    public static async Task<IEnumerable<FileConfig>> GetAllFiles(string userId)
    {
        try
        {
            Sem.WaitOne();
            var files = new List<FileConfig>();
            if (!Directory.Exists($"/data/configs/{userId}")) return files;
            foreach (var file in Directory.EnumerateFiles($"/data/configs/{userId}"))
            {
                var fileObject = JsonConvert.DeserializeObject<FileConfig>(await File.ReadAllTextAsync(file));
                if (fileObject is null) continue;
                files.Add(fileObject);
            }
            return files;
        }
        finally
        {
            Sem.Release();
        }
    }

        public static void DeleteAll()
        {
            try
            {
                Sem.WaitOne();
                if (!Directory.Exists("/data")) return;

                foreach (var directory in Directory.GetDirectories("/data"))
                {
                    Directory.Delete(directory, true);
                }

                foreach (var file in Directory.GetFiles("/data"))
                {
                    File.Delete(file);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle UnauthorizedAccessException here
                Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
            }
            catch (IOException ex)
            {
                // Handle IOException here
                Console.WriteLine($"IOException: {ex.Message}");
            }
            finally
            {
                Sem.Release();
            }
        }

        public static string DeleteUserDirectory(string userId)
        {
            try
            {
                Sem.WaitOne();
                string status = string.Empty;
                if (!Directory.Exists($"/data/content/{userId}")) status = "User Not Found";
                else Directory.Delete($"/data/content/{userId}", true);
                
                if (!Directory.Exists($"/data/configs/{userId}")) status = "User Not Found";
                else Directory.Delete($"/data/configs/{userId}", true);
                
                if (!File.Exists($"/data/users/{userId}.json")) status = "User Not Found";
                else File.Delete($"/data/users/{userId}.json");
                return status;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle UnauthorizedAccessException here
                Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
                return "Unauthorized access while deleting user directory.";
            }
            catch (IOException ex)
            {
                // Handle IOException here
                Console.WriteLine($"IOException: {ex.Message}");
                return "Error occurred while deleting user directory.";
            }
            finally
            {
                Sem.Release();
            }
        }

        // ... other methods ...

        public static async Task StoreFile(string userId, string fileId, string path, bool compress, Stream file)
        {
            try
            {
                Sem.WaitOne();
                await using (var st = new FileStream($"/data/content/{userId}/{fileId}.json", FileMode.Create))
                {
                    await file.CopyToAsync(st);
                }

                await File.WriteAllTextAsync($"/data/configs/{userId}/{fileId}.json",
                    JsonConvert.SerializeObject(new FileConfig(userId, path, fileId, compress)));
                if(OnLinux)
                {
                    File.SetUnixFileMode($"/data/configs/{userId}/{fileId}.json", FileAccessPermissions);
                    File.SetUnixFileMode($"/data/content/{userId}/{fileId}.json", FileAccessPermissions);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle UnauthorizedAccessException here
                Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
            }
            catch (IOException ex)
            {
                // Handle IOException here
                Console.WriteLine($"IOException: {ex.Message}");
            }
            finally
            {
                Sem.Release();
            }
        }

        public static bool LoadFile(string userId, string fileId)
        {
            try
            {
                Sem.WaitOne();
                return File.Exists($"/data/content/{userId}/{fileId}.json");
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle UnauthorizedAccessException here
                Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                // Handle IOException here
                Console.WriteLine($"IOException: {ex.Message}");
                return false;
            }
            finally
            {
                Sem.Release();
            }
        }

        public static string DeleteFile(string userId, string fileId)
        {
            try
            {
                Sem.WaitOne();
                string status = string.Empty;
                if (!File.Exists($"/data/content/{userId}/{fileId}.json")) status = "File Not Found";
                else File.Delete($"/data/content/{userId}/{fileId}.json");
                
                if (!File.Exists($"/data/configs/{userId}/{fileId}.json")) status = "File Not Found";
                else File.Delete($"/data/configs/{userId}/{fileId}.json");
                return status;
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle UnauthorizedAccessException here
                Console.WriteLine($"UnauthorizedAccessException: {ex.Message}");
                return "Unauthorized access while deleting the file.";
            }
            catch (IOException ex)
            {
                // Handle IOException here
                Console.WriteLine($"IOException: {ex.Message}");
                return "Error occurred while deleting the file.";
            }
            finally
            {
                Sem.Release();
            }
        }

        public class FileConfig
        {
            public string Path { get; set; }
            public string UserId { get; set; }
            public string FileId { get; set; }
            public bool Compress { get; set; }

            public FileConfig(string userId, string path, string fileId, bool compress)
            {
                Path = path;
                UserId = userId;
                FileId = fileId;
                Compress = compress;
            }
        }
}