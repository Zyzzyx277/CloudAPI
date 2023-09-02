namespace CloudSystem.Model;

public class FileObject
{
    public string Content { get; set; }
    public string IdUser { get; set; }
    public string Path { get; set; }

    public FileObject(string content, string idUser, string path)
    {
        Content = content;
        IdUser = idUser;
        Path = path;
    }

    public static HashSet<FileObject> FileDb = new();
}