namespace FileUploadGenerator;
internal class SharedResultsModel : ICustomResult
{
    public string ClassName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public BasicList<SharedPropertyInformation> UploadedProperties { get; set; } = [];
}