namespace FileUploadGenerator;
internal class ClientServerModelParser(UploadInformation upload, Compilation compilation)
{
    public ClientServerResultsModel GetResult()
    {
        ClientServerResultsModel output;
        var fullName = $"{upload.Namespace}.{upload.ModelName}";
        INamedTypeSymbol symbol = compilation.GetTypeByMetadataName(fullName)!;
        output = symbol.GetStartingResults<ClientServerResultsModel>();
        var list = symbol.GetAllPublicProperties();
        foreach (var start in list)
        {
            ClientServerPropertyInformation property = start.GetStartingPropertyInformation<ClientServerPropertyInformation>();
            if (upload.Properties.Any(x => x == property.PropertyName))
            {
                property.IsFile = true;
            }
            output.Properties.Add(property);
        }
        return output;
    }
}