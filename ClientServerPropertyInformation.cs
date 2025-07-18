﻿namespace FileUploadGenerator;
internal class ClientServerPropertyInformation : ICustomProperty
{
    public EnumSimpleTypeCategory VariableCustomCategory { get; set; }
    public bool Nullable { get; set; }
    public string PropertyName { get; set; } = "";
    public string ContainingNameSpace { get; set; } = "";
    public string UnderlyingSymbolName { get; set; } = "";
    public bool IsFile { get; set; }
}