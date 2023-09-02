namespace CloudSystem.Model;

public class FileObject
{
    public string Content { get; set; }
    public string IdUser { get; set; }
    public string IdFile { get; set; }
    public string Name { get; set; }

    public FileObject(string content, string idUser, string idFile, string name)
    {
        Content = content;
        IdUser = idUser;
        IdFile = idFile;
        Name = name;
    }

    public static HashSet<FileObject> FileDb = new();
}