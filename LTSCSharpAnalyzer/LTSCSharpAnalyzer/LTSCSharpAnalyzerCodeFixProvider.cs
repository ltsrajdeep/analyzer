using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace LTSCSharpAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LTSCSharpAnalyzerCodeFixProvider)), Shared]
    public class LTSCSharpAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Await keyword";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(LTSCSharpAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            try
            {
                var diagnostic = context.Diagnostics.First();
                context.RegisterCodeFix(CodeAction.Create("'await' before async method ", async token =>
                {

                    var document = context.Document;
                    var root = await document.GetSyntaxRootAsync(token);

                    var fullInvocationExpression = root.FindNode(diagnostic.Location.SourceSpan, false).Parent as InvocationExpressionSyntax;

                    var awaitExpression = SyntaxFactory.AwaitExpression(SyntaxFactory.Token(SyntaxKind.AwaitKeyword),
                        fullInvocationExpression);

                    var node = root.FindNode(diagnostic.Location.SourceSpan, false);

                    var updatedParameterNode = node.ReplaceNode(node, awaitExpression);

                    var newDoc = document.WithSyntaxRoot(root.ReplaceNode(fullInvocationExpression, updatedParameterNode));
                    return newDoc;
                }, "KEY"), diagnostic);
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }
    }
}
