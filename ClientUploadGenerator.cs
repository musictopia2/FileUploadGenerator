namespace FileUploadGenerator;
[Generator]
internal class ClientUploadGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var list = ClientServerCompleteParser.PrepareModels(context, "Client.xml");
        var collected = list.Collect();
        context.RegisterSourceOutput(collected, Execute);
    }
    private void Execute(SourceProductionContext context, ImmutableArray<ClientServerResultsModel> list)
    {
        ClientServerEmitClass emit = new(list, context);
        emit.EmitClient();
    }
}