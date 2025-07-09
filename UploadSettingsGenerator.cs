namespace FileUploadGenerator;
[Generator]
internal class UploadSettingsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> declares1 = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => IsSyntaxTarget(s),
            (t, _) => GetTarget(t))
            .Where(m => m != null)!;
        var declares2 = context.CompilationProvider.Combine(declares1.Collect());
        var declares3 = declares2.SelectMany(static (x, _) =>
        {
            ImmutableHashSet<ClassDeclarationSyntax> start = [.. x.Right];
            return GetResults(start, x.Left);
        });
        var declares4 = declares3.Collect(); //if you need compilation, then look at past samples.  try to do without compilation at the end if possible
        context.RegisterSourceOutput(declares4, Execute);
    }
    private bool IsSyntaxTarget(SyntaxNode syntax)
    {

        bool rets = syntax is ClassDeclarationSyntax ctx &&
           ctx.BaseList is not null &&
           ctx.ToString().Contains("BaseUploadSettingsContext");

        if (rets)
        {

            return true;
        }
        return false;
    }
    private ClassDeclarationSyntax? GetTarget(GeneratorSyntaxContext context)
    {
        var ourClass = context.GetClassNode(); //can use the sematic model at this stage

        return ourClass; //for this one, return the class always in this case.

    }
    private static ImmutableHashSet<SharedResultsModel> GetResults(
        ImmutableHashSet<ClassDeclarationSyntax> classes,
        Compilation compilation
        )
    {

        SharedParserClass parses = new(classes, compilation);
        BasicList<SharedResultsModel> output = parses.GetResults();
        return [.. output];
    }
    private void Execute(SourceProductionContext context, ImmutableArray<SharedResultsModel> list)
    {
        SharedEmitClass emit = new(list, context);
        emit.Emit(); //start out with console.  later do reals once ready.
    }
}