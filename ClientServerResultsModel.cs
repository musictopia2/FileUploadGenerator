namespace FileUploadGenerator;
internal class ClientServerResultsModel : ICustomResult
{
    public string ClassName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public BasicList<ClientServerPropertyInformation> Properties { get; set; } = [];
}