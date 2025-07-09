namespace FileUploadGenerator;
internal static class ClientServerCompleteParser
{
    public static IncrementalValuesProvider<ClientServerResultsModel> PrepareModels(
       IncrementalGeneratorInitializationContext context,
       string fileName)
    {
        var uploads = context.AdditionalTextsProvider
            .Where(file => Path.GetFileName(file.Path).Equals(fileName, StringComparison.OrdinalIgnoreCase))
            .Select((file, ct) =>
            {
                var text = file.GetText(ct);
                return UploadsFromXML(text!.ToString());
            });

        var uploadInfos = uploads.SelectMany((list, _) => list);

        var compilation = context.CompilationProvider;

        var resolved = uploadInfos.Combine(compilation).Select((tuple, _) =>
        {
            var (upload, comp) = tuple;
            ClientServerModelParser parses = new(upload, comp);
            return parses.GetResult();
        });

        return resolved.Where(t => t is not null);
    }

    private static BasicList<UploadInformation> UploadsFromXML(string content)
    {
        XElement doc = XElement.Parse(content);
        BasicList<UploadInformation> output = [];
        foreach (var model in doc.Descendants("Model"))
        {
            var name = model.Element("ModelName")?.Value ?? "";
            var space = model.Element("Namespace")?.Value ?? "";
            var props = model.Element("Properties")?
                            .Elements("Property")
                            .Select(p => p.Value)
                            .ToBasicList() ?? [];

            output.Add(new UploadInformation
            {
                ModelName = name,
                Namespace = space,
                Properties = props
            });
        }
        return output;
    }
}