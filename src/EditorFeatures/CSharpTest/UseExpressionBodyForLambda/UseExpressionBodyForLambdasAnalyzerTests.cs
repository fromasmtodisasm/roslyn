﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.UseExpressionBodyForLambda;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.UseExpressionBody
{
    public class UseExpressionBodyForLambdasAnalyzerTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        internal override (DiagnosticAnalyzer, CodeFixProvider) CreateDiagnosticProviderAndFixer(Workspace workspace)
            => (new UseExpressionBodyForLambdaDiagnosticAnalyzer(), new UseExpressionBodyForLambdaCodeFixProvider());

        private IDictionary<OptionKey, object> UseExpressionBody =>
            this.Option(CSharpCodeStyleOptions.PreferExpressionBodiedLambdas, CSharpCodeStyleOptions.WhenPossibleWithSilentEnforcement);

        private IDictionary<OptionKey, object> UseBlockBody =>
            this.Option(CSharpCodeStyleOptions.PreferExpressionBodiedLambdas, CSharpCodeStyleOptions.NeverWithSilentEnforcement);

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyInFieldInitializer()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x [|=>|]
        {
            return x.ToString();
        };
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x => x.ToString();
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyInFieldInitializer()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x [|=>|] x.ToString();
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x =>
        {
            return x.ToString();
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyInArgument()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        TargetMethod(x [|=>|]
        {
            return x.ToString();
        });
    }

    void TargetMethod(Func<int, string> targetParam) { }
}",
@"using System;

class C
{
    void Goo()
    {
        TargetMethod(x => x.ToString());
    }

    void TargetMethod(Func<int, string> targetParam) { }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyInArgument()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        TargetMethod(x [|=>|] x.ToString());
    }

    void TargetMethod(Func<int, string> targetParam) { }
}",
@"using System;

class C
{
    void Goo()
    {
        TargetMethod(x =>
        {
            return x.ToString();
        });
    }

    void TargetMethod(Func<int, string> targetParam) { }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyFromReturnKeyword()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x =>
        {
            [|return|] x.ToString();
        };
    }
}", new TestParameters(options: UseExpressionBody));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyFromLambdaOpeningBrace()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x =>
        [|{|]
            return x.ToString();
        };
    }
}", new TestParameters(options: UseExpressionBody));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyFromLambdaClosingBrace()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x =>
        {
            return x.ToString();
        [|}|];
    }
}", new TestParameters(options: UseExpressionBody));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x [|=>|]
        {
            throw null;
        };
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x => throw null;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x [|=>|] throw null;
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Func<int, string> f = x =>
        {
            throw null;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithVoidReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = x [|=>|]
        {
            x.ToString();
        };
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = x => x.ToString();
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithVoidReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = x [|=>|]
        {
            throw null;
        };
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = x => throw null;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithVoidReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = x [|=>|] x.ToString();
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = x =>
        {
            x.ToString();
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithVoidReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = x [|=>|] throw null;
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = x =>
        {
            throw null;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithAsyncVoidReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = async x [|=>|]
        {
            x.ToString();
        };
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = async x => x.ToString();
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithAsyncVoidReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = async x [|=>|]
        {
            throw null;
        };
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = async x => throw null;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithAsyncVoidReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = async x [|=>|] x.ToString();
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = async x =>
        {
            x.ToString();
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithAsyncVoidReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = async x [|=>|] throw null;
    }
}",
@"using System;

class C
{
    void Goo()
    {
        Action<int> f = async x =>
        {
            throw null;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithTaskReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = () [|=>|]
        {
            return Task.CompletedTask;
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = () => Task.CompletedTask;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithTaskReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = () [|=>|]
        {
            throw null;
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = () => throw null;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithTaskReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = () [|=>|] Task.CompletedTask;
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = () =>
        {
            return Task.CompletedTask;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithTaskReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = () [|=>|] throw null;
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = () =>
        {
            throw null;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithAsyncTaskReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = async () [|=>|]
        {
            await Task.CompletedTask;
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = async () => await Task.CompletedTask;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithAsyncTaskReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = async () [|=>|]
        {
            throw null;
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = async () => throw null;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithAsyncTaskReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = async () [|=>|] await Task.CompletedTask;
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = async () =>
        {
            await Task.CompletedTask;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithAsyncTaskReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = async () [|=>|] throw null;
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<Task> f = async () =>
        {
            throw null;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithTaskTReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = x [|=>|]
        {
            return Task.FromResult(x.ToString());
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = x => Task.FromResult(x.ToString());
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithTaskTReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = x [|=>|]
        {
            throw null;
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = x => throw null;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithTaskTReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = x [|=>|] Task.FromResult(x.ToString());
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = x =>
        {
            return Task.FromResult(x.ToString());
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithTaskTReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = x [|=>|] throw null;
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = x =>
        {
            throw null;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithAsyncTaskTReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = async x [|=>|]
        {
            return await Task.FromResult(x.ToString());
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = async x => await Task.FromResult(x.ToString());
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithAsyncTaskTReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = async x [|=>|]
        {
            throw null;
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = async x => throw null;
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithAsyncTaskTReturn()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = async x [|=>|] await Task.FromResult(x.ToString());
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = async x =>
        {
            return await Task.FromResult(x.ToString());
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithAsyncTaskTReturnThrowing()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = async x [|=>|] throw null;
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, Task<string>> f = async x =>
        {
            throw null;
        };
    }
}", options: UseBlockBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithPrecedingComment()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, string> f = x [|=>|]
        {
            // Comment
            return x.ToString();
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, string> f = x =>
            // Comment
            x.ToString();
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseExpressionBodyWithEndingComment()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, string> f = x [|=>|]
        {
            return x.ToString(); // Comment
        };
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, string> f = x => x.ToString();
    }
}", options: UseExpressionBody);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsUseExpressionBody)]
        public async Task UseBlockBodyWithEndingComment()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, string> f = x [|=>|] x.ToString(); // Comment
    }
}",
@"using System;
using System.Threading.Tasks;

class C
{
    void Goo()
    {
        Func<int, string> f = x =>
        {
            return x.ToString();
        }; // Comment
    }
}", options: UseBlockBody);
        }
    }
}
