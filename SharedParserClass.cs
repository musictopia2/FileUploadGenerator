namespace FileUploadGenerator;
internal class SharedParserClass(IEnumerable<ClassDeclarationSyntax> list, Compilation compilation)
{
    public BasicList<SharedResultsModel> GetResults()
    {
        BasicList<SharedResultsModel> output = [];
        foreach (var item in list)
        {
            var temp = GetFluentResults(item);
            output.AddRange(temp);
        }
        return output;
    }
    private BasicList<SharedResultsModel> GetFluentResults(ClassDeclarationSyntax node)
    {
        ParseContext context = new(compilation, node);
        var members = node.DescendantNodes().OfType<MethodDeclarationSyntax>();
        foreach (var m in members)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(m) as IMethodSymbol;
            if (symbol is not null && symbol.Name == "Configure") //has to be magic strings now.
            {
                BasicList<SharedResultsModel> output = [];
                ParseSettings(output, context, m);
                return output;
            }
        }
        return [];
    }
    private void ParseSettings(BasicList<SharedResultsModel> results, ParseContext context, MethodDeclarationSyntax syntax)
    {
        var makeCalls = ParseUtils.FindCallsOfMethodWithName(context, syntax, "Make");
        foreach (CallInfo make in makeCalls)
        {
            SharedResultsModel result = new();
            results.Add(result);
            // Get the model type passed into Make<T>
            INamedTypeSymbol makeType = (INamedTypeSymbol)make.MethodSymbol.TypeArguments[0]!;
            result.ClassName = makeType.Name;
            result.Namespace = makeType.ContainingNamespace.ToDisplayString();
            // Extract the lambda passed into Make<T>(...)
            if (make.ArgumentList.Arguments.FirstOrDefault()?.Expression is not SimpleLambdaExpressionSyntax lambda)
            {
                continue;
            }

            if (lambda.Body is not BlockSyntax lambdaBody)
            {
                continue;
            }

            foreach (var statement in lambdaBody.Statements.OfType<ExpressionStatementSyntax>())
            {
                if (statement.Expression is not InvocationExpressionSyntax invocation)
                {
                    continue;
                }

                foreach (var setUploadCall in ExtractChainedSetUploadCalls(invocation, context))
                {
                    var args = setUploadCall.ArgumentList.Arguments;
                    if (args.Count < 3)
                    {
                        continue;
                    }

                    var lambdaExpr = args[0].Expression as SimpleLambdaExpressionSyntax;
                    var memberAccess = lambdaExpr?.Body as MemberAccessExpressionSyntax;
                    var propertyName = memberAccess?.Name.Identifier.Text ?? "";
                    var propertyInfo = new SharedPropertyInformation
                    {
                        Name = propertyName,
                        MaxSizeExpression = args[1].ToString(),
                        ContentTypesExpression = args[2].ToString(),
                        RequiredExpression = args.Count >= 4 ? args[3].ToString() : "true"
                    };
                    result.UploadedProperties.Add(propertyInfo);
                }
            }
        }
    }


    private IEnumerable<InvocationExpressionSyntax> ExtractChainedSetUploadCalls(
    InvocationExpressionSyntax rootInvocation,
    ParseContext context)
    {
        var current = rootInvocation;
        while (current != null)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(current);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol && methodSymbol.Name == "SetUploadFile")
            {
                yield return current;
            }

            if (current.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is InvocationExpressionSyntax innerInvocation)
            {
                current = innerInvocation;
            }
            else
            {
                break;
            }
        }
    }
}