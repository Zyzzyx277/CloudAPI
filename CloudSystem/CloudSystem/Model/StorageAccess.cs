using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace CloudSystem.Model;

public class StorageAccess
{
    public static Semaphore sem = new (1, 1);
    private static UnixFileMode fileAccessPermissions = UnixFileMode.GroupRead | UnixFileMode.OtherRead 
                                                                               | UnixFileMode.UserRead
                                                                               | UnixFileMode.UserWrite;
    private static bool onLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    
    public static async Task CreateUserDirectory(User user)
    {
        try
        {
            sem.WaitOne();
            Directory.CreateDirectory($"/data/content/{user.Id}");
            Directory.CreateDirectory($"/data/configs/{user.Id}");
            Directory.CreateDirectory("/data/users");

            await File.WriteAllTextAsync($"/data/users/{user.Id}.json", JsonConvert.SerializeObject(user));
            if(onLinux) File.SetUnixFileMode($"/data/users/{user.Id}.json", fileAccessPermissions);
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
            sem.Release();
        }
    }

    public static async Task<User?> GetUser(string userId)
    {
        try
        {
            sem.WaitOne();
            if (!File.Exists($"/data/users/{userId}.json")) return null;

            var data = JsonConvert.DeserializeObject<User>(await File.ReadAllTextAsync($"/data/users/{userId}.json"));
            return data;
        }
        finally
        {
            sem.Release();
        }
    }
    
    public static async Task UpdateUser(User user)
    {
        try
        {
            sem.WaitOne();
            if (!File.Exists($"/data/users/{user.Id}.json")) return;

            await File.WriteAllTextAsync($"/data/users/{user.Id}.json", JsonConvert.SerializeObject(user));
        }
        finally
        {
            sem.Release();
        }
    }
    
    public static async Task<IEnumerable<User>> GetAllUsers()
    {
        try
        {
            sem.WaitOne();
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
            sem.Release();
        }
    }
    
    public static async Task<IEnumerable<FileConfig>> GetAllFiles(string userId)
    {
        try
        {
            sem.WaitOne();
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
            sem.Release();
        }
    }

        public static void DeleteAll()
        {
            try
            {
                sem.WaitOne();
                if (!Directory.Exists("/data")) return;

                foreach (var directory in Directory.GetDirectories("/data"))
                {
                    Directory.Delete(directory, true);
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
                sem.Release();
            }
        }

        public static string DeleteUserDirectory(string userId)
        {
            try
            {
                sem.WaitOne();
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
                sem.Release();
            }
        }

        // ... other methods ...

        public static async Task StoreFile(string userId, string fileId, string path, bool compress, Stream file)
        {
            try
            {
                sem.WaitOne();
                await using (var st = new FileStream($"/data/content/{userId}/{fileId}.json", FileMode.Create))
                {
                    await file.CopyToAsync(st);
                }

                await File.WriteAllTextAsync($"/data/configs/{userId}/{fileId}.json",
                    JsonConvert.SerializeObject(new FileConfig(userId, path, fileId, compress)));
                if(onLinux)
                {
                    File.SetUnixFileMode($"/data/configs/{userId}/{fileId}.json", fileAccessPermissions);
                    File.SetUnixFileMode($"/data/content/{userId}/{fileId}.json", fileAccessPermissions);
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
                sem.Release();
            }
        }

        public static bool LoadFile(string userId, string fileId)
        {
            try
            {
                sem.WaitOne();
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
                sem.Release();
            }
        }

        public static string DeleteFile(string userId, string fileId)
        {
            try
            {
                sem.WaitOne();
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
                sem.Release();
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