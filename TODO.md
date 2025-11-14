Corrigir "Não foi possível enviar a requisição. Verifique o backend." quando se coloca a o intervalo a e b no metodo bissessao. A função escolhida foi: x^3 - 2x + 2 e a=1 e b=2. O resto padrão.

log do servidor:
```
fail: Microsoft.AspNetCore.Server.Kestrel[13]
      Connection id "0HNH3QQHCNNKD", Request id "0HNH3QQHCNNKD:00000004": An unhandled exception was thrown by the application.
      NumericalMethods.Core.RootFinding.ExpressionParseException: Expressão inválida para avaliação.
         at NumericalMethods.Core.RootFinding.ExpressionEvaluator.Evaluate(Token[] rpn, Double x) in /src/src/NumericalMethods.Core/RootFinding/ExpressionEvaluator.cs:line 317
         at NumericalMethods.Core.RootFinding.ExpressionEvaluator.<>c__DisplayClass5_0.<Compile>b__0(Double x) in /src/src/NumericalMethods.Core/RootFinding/ExpressionEvaluator.cs:line 61
         at NumericalMethods.Core.Services.RootFindingService.SolveWithBisection(Func`2 f, RootFindingRequest request) in /src/src/NumericalMethods.Core/Services/RootFindingService.cs:line 78
         at NumericalMethods.Core.Services.RootFindingService.Solve(RootFindingRequest request, Boolean returnSteps) in /src/src/NumericalMethods.Core/Services/RootFindingService.cs:line 47
         at NumericalMethods.Api.Controllers.RootFindingController.Solve(RootFindingSolveRequestDto request) in /src/src/NumericalMethods.Api/Controllers/RootFindingController.cs:line 24
         at lambda_method2(Closure, Object, Object[])
         at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.SyncObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeNextActionFilterAsync()
      --- End of stack trace from previous location ---
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
      --- End of stack trace from previous location ---
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
         at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpProtocol.ProcessRequests[TContext](IHttpApplication`1 application)
```