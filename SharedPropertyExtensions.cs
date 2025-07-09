namespace FileUploadGenerator;
internal static class SharedPropertyExtensions
{
    public static void WritePropertiesDictionary(this ICodeBlock w, SharedResultsModel result)
    {
        StrCat cats = new();
        foreach (var item in result.UploadedProperties)
        {
            string data =
                $"""
                ["{item.Name}"] = new global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.UploadHelpers.UploadFileInfo("{item.Name}", {item.MaxSizeExpression}, {item.ContentTypesExpression}, {item.RequiredExpression})
                """;
            cats.AddToString(data, ",");
        }
        w.WriteLine(cats.GetInfo());
    }
    public static void WritePropertiesFileInfo(this ICodeBlock w, SharedResultsModel result)
    {
        foreach (var item in result.UploadedProperties)
        {
            w.WriteLine($"""
                public static global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.UploadHelpers.UploadFileInfo Get{item.Name}FileInfo() => _fileFields["{item.Name}"];
                """);
        }
    }
}
