﻿//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.VisualStudio.LanguageServices.LiveShare.Client
{
    /// <summary>
    /// A diagnostic analyzer that fetches diagnostics from the remote side.
    /// </summary>
    /// <remarks>
    /// Since DiagnosticAnalyzers don't participate in MEF composition, we get the diagnostics through a
    /// language service called <see cref="IRemoteDiagnosticsService"/>
    /// </remarks>
#if VS_16_0
    [DiagnosticAnalyzer(StringConstants.CSharpLspLanguageName, StringConstants.VBLspLanguageName)]
#else
    [DiagnosticAnalyzer(StringConstants.CSharpLspLanguageName, StringConstants.VBLspLanguageName, StringConstants.TypeScriptLanguageName)]
#endif
    internal class RoslynDocumentDiagnosticAnalyzer : DocumentDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray<DiagnosticDescriptor>.Empty;

        public override async Task<ImmutableArray<Diagnostic>> AnalyzeSemanticsAsync(Document document, CancellationToken cancellationToken)
        {
            var diagnosticsService = document.Project.LanguageServices.GetService<IRemoteDiagnosticsService>();
            if (diagnosticsService == null)
            {
                return ImmutableArray<Diagnostic>.Empty;
            }

            return await diagnosticsService.GetDiagnosticsAsync(document, cancellationToken).ConfigureAwait(false);
        }

        public override Task<ImmutableArray<Diagnostic>> AnalyzeSyntaxAsync(Document document, CancellationToken cancellationToken)
        {
            return Task.FromResult(ImmutableArray<Diagnostic>.Empty);
        }
    }
}