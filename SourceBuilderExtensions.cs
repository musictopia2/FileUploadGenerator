namespace FileUploadGenerator;
internal static class SourceBuilderExtensions
{
    public static void WriteServer(this SourceCodeStringBuilder builder, Action<ICodeBlock> action, ClientServerResultsModel result)
    {
        builder.WriteLine("#nullable enable");
        builder.WriteLine($"namespace {result.Namespace}.ServerExtensions;")
            .WriteLine($"public static class Generator{result.ClassName}Extensions")
            .WriteCodeBlock(action.Invoke);
    }
    public static void WriteClientExtensions(this SourceCodeStringBuilder builder, Action<ICodeBlock> action, ClientServerResultsModel result)
    {
        builder.WriteLine("#nullable enable");
        builder.WriteLine($"namespace {result.Namespace}.ClientExtensions;")
            .WriteLine($"public static class Generator{result.ClassName}Extensions")
            .WriteCodeBlock(action.Invoke);
    }
    public static void WriteClientFileRegistry(this SourceCodeStringBuilder builder, Action<ICodeBlock> action, ClientServerResultsModel result)
    {
        builder.WriteLine("#nullable enable");
        builder.WriteLine($"namespace {result.Namespace}.FileRegistries;")
            .WriteLine($"public sealed class Generator{result.ClassName}FileRegistry")
            .WriteCodeBlock(action.Invoke);
    }
    public static void WriteUploadFileMetadataClass(this SourceCodeStringBuilder builder, Action<ICodeBlock> action, SharedResultsModel result)
    {
        builder.WriteLine("#nullable enable");
        builder.WriteLine($"namespace {result.Namespace}.GeneratedFileConfigs;")
            .WriteLine($"public static class {result.ClassName}FileMetadata")
            .WriteCodeBlock(action.Invoke);
    }

}