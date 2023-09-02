namespace CloudSystem.Model;

public class FileObject
{
    public string Content { get; set; }
    public string IdUser { get; set; }
    public string IdFile { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }

    public FileObject(string content, string idUser, string idFile, string name, string path)
    {
        Content = content;
        IdUser = idUser;
        IdFile = idFile;
        Name = name;
        Path = path;
    }

    public static HashSet<FileObject> FileDb = new();
}