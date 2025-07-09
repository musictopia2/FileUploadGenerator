namespace FileUploadGenerator;
public class UploadInformation
{
    public string ModelName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public BasicList<string> Properties { get; set; } = [];
}