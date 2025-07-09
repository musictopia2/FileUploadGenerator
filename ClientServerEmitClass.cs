namespace FileUploadGenerator;
internal class ClientServerEmitClass(ImmutableArray<ClientServerResultsModel> results, SourceProductionContext context)
{
    public void EmitServer()
    {
        foreach (var item in results)
        {
            WriteServerItem(item);
        }
    }
    public void EmitClient()
    {
        foreach(var item in results)
        {
            WriteClientItem(item);
        }
    }
    private void WriteClientItem(ClientServerResultsModel item)
    {
        SourceCodeStringBuilder builder = new();
        //if you are doing partial classes, then this will do the work.  if something else, rethink.

        builder.WriteClientExtensions(w =>
        {
            PopulateClientExtensionDetails(w, item);
        }, item);
        context.AddSource($"{item.ClassName}.Extensions.g.cs", builder.ToString());

        builder = new();
        builder.WriteClientFileRegistry(w =>
        {
            PopulateClientRegistryDetails(w, item);
        }, item);
        context.AddSource($"{item.ClassName}.FileRegistries.g.cs", builder.ToString());
    }
    private void PopulateClientExtensionDetails(ICodeBlock w, ClientServerResultsModel result)
    {
        w.AppendClientAllowed()
          .AppendClientTryAddFile(result)
          .CreateClientExtension(result);
    }
    private void PopulateClientRegistryDetails(ICodeBlock w, ClientServerResultsModel result)
    {
        w.CreateClientPrivateDictionary()
            .PopulateClientClear()
            .PopulateClientAddMethods(result)
            .CreateTryGetFile();
    }
    private void WriteServerItem(ClientServerResultsModel item)
    {
        SourceCodeStringBuilder builder = new();
        //if you are doing partial classes, then this will do the work.  if something else, rethink.

        builder.WriteServer(w =>
        {
            PopulateServerDetails(w, item);
        }, item);
        context.AddSource($"{item.ClassName}.Server.g.cs", builder.ToString());
    }
    private void PopulateServerDetails(ICodeBlock w, ClientServerResultsModel result)
    {
        w.AppendServerAllowed()
            .AppendServerFileUploads(result)
            .CreateServerExtension(result);
    }
}