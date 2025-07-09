namespace FileUploadGenerator;
internal static class ClientServerWriterExtensions
{
    public static ICodeBlock AppendServerAllowed(this ICodeBlock w)
    {
        w.WriteLine("""
            private static bool IsAllowedContentType(global::Microsoft.AspNetCore.Http.IFormFile file, global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.UploadHelpers.UploadFileInfo upload)
            """)
            .WriteCodeBlock(w =>
            {
                w.WriteLine("if (upload.AllowedContentTypes.Count == 0)")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine("return true;");
                });
                w.WriteLine("return upload.AllowedContentTypes.Contains(file.ContentType);");
            });
        return w;
    }
    private static ICodeBlock GetUpload(this ICodeBlock w, ClientServerResultsModel result, string variableName)
    {
        w.WriteLine($"""
                    var upload = global::{result.Namespace}.GeneratedFileConfigs.{result.ClassName}FileMetadata.GetFileInfo({variableName});
                    """);
        return w;
    }
    public static ICodeBlock AppendServerFileUploads(this ICodeBlock w, ClientServerResultsModel result)
    {
        w.WriteLine("private static async Task<string> HandleFileUploadAsync(")
            .WriteLine("global::Microsoft.AspNetCore.Http.IFormCollection form,")
            .WriteLine("string propertyName,")
            .WriteLine("global::BasicAspNetServerLibrary.UploadHelpers.IFileSaver fileSaver)")
            .WriteCodeBlock(w =>
            {
                w.WriteLine("var file = form.Files.GetFile(propertyName);");
                w.GetUpload(result, "propertyName")
                .WriteLine("if (file is null)")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine("if (upload.Required)")
                    .WriteCodeBlock(w =>
                    {
                        w.WriteLine("""
                            throw new global::CommonBasicLibraries.BasicDataSettingsAndProcesses.CustomBasicException($"'{propertyName}' is required but was not provided.");
                            """);
                    });
                    w.WriteLine("""
                        return "";
                        """);
                })
                .WriteLine("if (file.Length > upload.MaxSize)")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine("""
                        throw new global::CommonBasicLibraries.BasicDataSettingsAndProcesses.CustomBasicException($"'{propertyName}' exceeds the maximum allowed size.");
                        """);
                });
                w.WriteLine("if (!IsAllowedContentType(file, upload))")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine("""
                        throw new global::CommonBasicLibraries.BasicDataSettingsAndProcesses.CustomBasicException($"'{propertyName}' has an invalid content type '{file.ContentType}'.");
                        """);
                });
                w.WriteLine("var context = new global::BasicAspNetServerLibrary.UploadHelpers.FileSaveContext(file, propertyName, file.FileName);")
                .WriteLine("return await fileSaver.SaveAsync(context);");
            });
        return w;
    }
    public static ICodeBlock CreateServerExtension(this ICodeBlock w, ClientServerResultsModel result)
    {
        w.WriteLine($"public static async Task<global::{result.Namespace}.{result.ClassName}> Bind{result.ClassName}Async(this global::BasicAspNetServerLibrary.UploadHelpers.IFileSaver fileSaver, global::Microsoft.AspNetCore.Http.HttpRequest request)")
            .WriteCodeBlock(w =>
            {
                w.WriteLine("""
                    var form = await request.ReadFormAsync() ?? throw new global::CommonBasicLibraries.BasicDataSettingsAndProcesses.CustomBasicException("No forms");
                    """)
                .WriteLine($"var model = new global::{result.Namespace}.{result.ClassName}();");
                foreach (var item in result.Properties)
                {
                    if (item.IsFile == false)
                    {
                        w.WriteLine($"""
                            model.{item.PropertyName} = form["{item.PropertyName}"].ToString() ?? "";
                            """);
                    }
                    else
                    {
                        w.WriteLine($"""
                            model.{item.PropertyName} = await HandleFileUploadAsync(form, "{item.PropertyName}", fileSaver);
                            """);
                    }
                }
                w.WriteLine("return model;");
            });
        return w;
    }


    public static ICodeBlock AppendClientAllowed(this ICodeBlock w)
    {
        w.WriteLine("""
        private static bool IsAllowedContentType(global::Microsoft.AspNetCore.Components.Forms.IBrowserFile file, global::CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.UploadHelpers.UploadFileInfo upload)
        """)
        .WriteCodeBlock(w =>
        {
            w.WriteLine("if (upload.AllowedContentTypes.Count == 0)")
             .WriteCodeBlock(w => w.WriteLine("return true;"));
            w.WriteLine("return upload.AllowedContentTypes.Contains(file.ContentType);");
        });
        return w;
    }
    public static ICodeBlock AppendClientTryAddFile(this ICodeBlock w, ClientServerResultsModel result)
    {
        string registryClass = $"global::{result.Namespace}.FileRegistries.Generator{result.ClassName}FileRegistry";
        w.WriteLine($"""
        private static bool TryAddFileToContent(
            string key,
            {registryClass} registry,
            MultipartFormDataContent content,
            out string? errorMessage)
        """)
         .WriteCodeBlock(w =>
         {
             w.WriteLine("errorMessage = null;")
             .GetUpload(result, "key")
             .WriteLine("if (upload is null)")
              .WriteCodeBlock(w =>
              {
                  w.WriteLine("""errorMessage = $"Error: Failed to get upload information for '{key}'.";""");
                  w.WriteLine("return false;");
              });

             w.WriteLine("if (upload.MaxSize == 0)")
              .WriteCodeBlock(w =>
              {
                  w.WriteLine("""errorMessage = $"Error: maxAllowedSize must be greater than 0 for '{key}'.";""");
                  w.WriteLine("return false;");
              });

             w.WriteLine("if (!registry.TryGetFile(key, out var file))")
              .WriteCodeBlock(w =>
              {
                  w.WriteLine("if (upload.Required)")
                   .WriteCodeBlock(w =>
                   {
                       w.WriteLine("""errorMessage = $"Error: Did not add '{key}' via the registry but it was required.";""");
                       w.WriteLine("return false;");
                   });
                  w.WriteLine("return true;");
              });

             w.WriteLine("if (!IsAllowedContentType(file!, upload))")
              .WriteCodeBlock(w =>
              {
                  w.WriteLine("""errorMessage = $"Error: {file!.ContentType} was not allowed for '{key}'.";""");
                  w.WriteLine("return false;");
              });

             w.WriteLine("var streamContent = new StreamContent(file!.OpenReadStream(upload.MaxSize));")
              .WriteLine("streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);")
              .WriteLine("content.Add(streamContent, key, file.Name);")
              .WriteLine("return true;");
         });
        return w;
    }
    public static ICodeBlock CreateClientExtension(this ICodeBlock w, ClientServerResultsModel result)
    {
        string registryClass = $"global::{result.Namespace}.FileRegistries.Generator{result.ClassName}FileRegistry";
        string fullModelType = $"global::{result.Namespace}.{result.ClassName}";

        w.WriteLine($"""
        public static async Task<(bool Success, string Message)> Upload{result.ClassName}Async(
            this {fullModelType} model,
            {registryClass} registry,
            global::System.Net.Http.HttpClient http,
            string uploadUrl)
        """)
         .WriteCodeBlock(w =>
         {
             w.WriteLine("try")
              .WriteCodeBlock(w =>
              {
                  w.WriteLine("using var content = new MultipartFormDataContent();");

                  foreach (var prop in result.Properties)
                  {
                      if (prop.IsFile == false)
                      {
                          w.WriteLine($"""content.Add(new StringContent(model.{prop.PropertyName} ?? ""), "{prop.PropertyName}");""");
                      }
                  }

                  foreach (var prop in result.Properties)
                  {
                      if (prop.IsFile == true)
                      {
                          w.WriteLine($"""if (!TryAddFileToContent("{prop.PropertyName}", registry, content, out var error))""")
                           .WriteCodeBlock(w =>
                           {
                               w.WriteLine("return (false, error ?? \"Unknown error\");");
                           });
                      }
                  }

                  w.WriteLine("var response = await http.PostAsync(uploadUrl, content);");
                  w.WriteLine("if (response.IsSuccessStatusCode)")
                  .WriteCodeBlock(w =>
                  {
                      w.WriteLine("""
                          return (true, "Upload successful!");
                          """);
                  });
                  w.WriteLine("""
                      return (false, $"Upload failed: {response.ReasonPhrase}");
                      """);
              });
             w.WriteLine("catch (Exception ex)")
              .WriteCodeBlock(w =>
              {
                  w.WriteLine("""
                      return (false, $"Error: {ex.Message}");
                      """);
              });
         });
        return w;
    }
    public static ICodeBlock CreateClientPrivateDictionary(this ICodeBlock w)
    {
        w.WriteLine("private readonly Dictionary<string, global::Microsoft.AspNetCore.Components.Forms.IBrowserFile> _files = [];");
        return w;
    }
    public static ICodeBlock PopulateClientClear(this ICodeBlock w)
    {
        w.WriteLine("public void Clear()")
            .WriteCodeBlock(w => w.WriteLine("_files.Clear();"));
        return w;
    }
    public static ICodeBlock PopulateClientAddMethods(this ICodeBlock w, ClientServerResultsModel result)
    {
        result.Properties.ForConditionalItems(x => x.IsFile, p =>
        {
            w.WriteLine($"""
                public void Add{p.PropertyName}File(global::Microsoft.AspNetCore.Components.Forms.IBrowserFile file) => _files["{p.PropertyName}"] = file;
                """);
        });
        return w;
    }
    public static ICodeBlock CreateTryGetFile(this ICodeBlock w)
    {
        w.WriteLine("public bool TryGetFile(string key, out global::Microsoft.AspNetCore.Components.Forms.IBrowserFile? file) => _files.TryGetValue(key, out file);");
        return w;
    }


}