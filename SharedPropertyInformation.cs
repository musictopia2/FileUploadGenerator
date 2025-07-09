namespace FileUploadGenerator;
internal class SharedPropertyInformation
{
    public string Name { get; set; } = "";
    public string MaxSizeExpression { get; set; } = "";
    public string ContentTypesExpression { get; set; } = "";
    public string RequiredExpression { get; set; } = "true"; // default
}