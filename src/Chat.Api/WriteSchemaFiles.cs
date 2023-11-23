using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Samples.Chat.Api;

public class WriteSchemaFiles : IHostedService
{
    private static readonly IReadOnlyList<string> IgnoredTypeNames = new List<string>
    {
        "external",
        "requires",
        "provides",
        "key",
        "extends",
        "_Service",
        "_Entity",
        "_Any",
        "_FieldSet",
        "skip",
        "deprecated",
        "include"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<WriteSchemaFiles> _logger;
    private readonly SchemaCollection _schemaCollection;

    public WriteSchemaFiles(SchemaCollection schemaCollection, IWebHostEnvironment environment, ILogger<WriteSchemaFiles> logger)
    {
        _schemaCollection = schemaCollection;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IsDevelopment: {IsDevelopment}", _environment.IsDevelopment());
        foreach ((string name, ISchema schema) in _schemaCollection.Schemas)
        {
            string outputFileName = Path.GetFullPath($"../UI/graphql/{name}.graphql", _environment.WebRootPath);
            _logger.LogInformation("Writing schema '{SchemaName}' to file '{OutputFileName}'", name, outputFileName);
            await File.WriteAllTextAsync(
                outputFileName, 
                Printer.Print(schema.ToTypeSystem(), PrintNode, false),
                cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private bool PrintNode(INode node)
    {
        if (node is DirectiveDefinition directiveType)
            if (IgnoredTypeNames.Contains(directiveType.Name.Value) ||
                directiveType.Name.Value.StartsWith("__"))
                return false;

        if (node is ObjectDefinition namedType)
            if (IgnoredTypeNames.Contains(namedType.Name.Value) || namedType.Name.Value.StartsWith("__") ||
                namedType.Fields?.Any() == false)
                return false;

        if (node is ObjectDefinition queryType)
            if (queryType.Name.Value == "Query" &&
                queryType.Fields?.Where(f => !f.Name.Value.StartsWith("_")).Any() == false)
                return false;

        if (node is InterfaceDefinition interfaceDefinition)
            if (IgnoredTypeNames.Contains(interfaceDefinition.Name.Value) ||
                interfaceDefinition.Name.Value.StartsWith("__"))
                return false;

        if (node is UnionDefinition unionDefinition)
            if (IgnoredTypeNames.Contains(unionDefinition.Name.Value) ||
                unionDefinition.Name.Value.StartsWith("__"))
                return false;

        if (node is FieldDefinition fieldDefinition)
            if (new[] { "_service", "_entities", "__type", "__schema" }.Contains(fieldDefinition.Name.Value))
                return false;

        if (node is ScalarDefinition scalarDefinition)
            if (Scalars.Standard.Any(standard => standard.Type.Name.Value == scalarDefinition.Name.Value) ||
                IgnoredTypeNames.Contains(scalarDefinition.Name.Value))
                return false;

        if (node is EnumDefinition enumDefinition)
            if (IgnoredTypeNames.Contains(enumDefinition.Name.Value) ||
                enumDefinition.Name.Value.StartsWith("__"))
                return false;

        if (node is SchemaDefinition)
            return false;


        return true;
    }
}