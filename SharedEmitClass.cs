namespace FileUploadGenerator;
internal class SharedEmitClass(ImmutableArray<SharedResultsModel> results, SourceProductionContext context)
{
    public void Emit()
    {
        foreach (var item in results)
        {
            WriteItem(item);
        }
    }
    private void WriteItem(SharedResultsModel item)
    {
        SourceCodeStringBuilder builder = new();
        //if you are doing partial classes, then this will do the work.  if something else, rethink.
        builder.WriteUploadFileMetadataClass(w =>
        {
            PopulateDetails(w, item);
        }, item);

        context.AddSource($"{item.ClassName}.FileMeta.g.cs", builder.ToString()); //change sample to what you want.
    }
    private void PopulateDetails(ICodeBlock w, SharedResultsModel result)
    {
        w.WriteLine($"public static global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.UploadHelpers.UploadFileInfo? GetFileInfo(string propertyName) => _fileFields.TryGetValue(propertyName, out var info) ? info : null;");
        w.WriteLine("private static readonly Dictionary<string, global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.UploadHelpers.UploadFileInfo> _fileFields = new()")
            .WriteCodeBlock(w =>
            {
                w.WritePropertiesDictionary(result);

            }, true)
            .WritePropertiesFileInfo(result);
    }
}