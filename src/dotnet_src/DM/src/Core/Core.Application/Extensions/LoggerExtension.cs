using System.Diagnostics;
using Core.Application.Common.Results;
using Microsoft.Extensions.Logging;

namespace Core.Application.Extensions;

public static class LoggerExtension
{
    
    private static readonly ActivitySource AppActivitySource = new ActivitySource("App"); 
    
    
    [Obsolete("use BeginErrorScope")]
    public static void LogWarningApp(this ILogger logger, string message, Error error, params object[] args)
    {
        // "Auth error. Email: {Email}. Error Warining: {ErrorData}"
        var merged = new object?[args.Length + 3];
        Array.Copy(args, merged, args.Length);
        merged[^3]= error.Message;
        merged[^2]= error.Type;
        merged[^1]= error.Code;
        logger.LogWarning($"{message} Error Data Warning. | Message: {{AppErrorMessage}}| Type: {{AppErrorType}} | Code: {{AppErrorCode}}", merged);
    }



    public static IDisposable? BeginErrorScope(this ILogger logger, Error error)
    {
        var scope = logger.BeginScope(new 
        {
            AppErrorMessage = error.Message,
            AppErrorType = error.Type.ToString(),
            AppErrorCode = error.Code.ToString(),
            traceId = Activity.Current?.TraceId.ToString(),
            spanId = Activity.Current?.SpanId.ToString()
        });

        // var scopeState = new List<KeyValuePair<string, object>>(5)
        // {
        //     new("app.error.message", error.Message),
        //     new("app.error.type",    error.Type.ToString()),
        //     new("app.error.code",    error.Code),
        //     new("traceId",           Activity.Current?.TraceId.ToString() ?? ""),
        //     new("spanId",            Activity.Current?.SpanId.ToString() ?? "")
        // };
        // var scope = logger.BeginScope(scopeState);
        
        
        var created = false;
        var activity = Activity.Current;

        if (activity is null)
        {
            // TODO: аттрибут [CallerMemberName] влияет на варнинг
            activity = AppActivitySource.StartActivity(name: "app.error", ActivityKind.Internal);
            created = activity != null;
        }

        if (activity?.IsAllDataRequested == true)
        {
            activity.SetStatus(ActivityStatusCode.Error, error.Message);
            activity.SetTag("app.error.type", error.Type.ToString());
            activity.SetTag("app.error.code", error.Code.ToString());
            activity.AddEvent(new ActivityEvent("app.error", tags: new ActivityTagsCollection()
            {
                ["app.error.message"]=error.Message,
                ["app.error.type"]=error.Type.ToString(),
                ["app.error.code"]=error.Code,
            }));
        }


        if (activity is not null)
            return new ScopeWithActivity(scope, activity);

        return scope;
    }


    private sealed class ScopeWithActivity : IDisposable
    {
        private readonly IDisposable? _scope;
        private readonly Activity? _activity;


        public ScopeWithActivity(IDisposable? scope, Activity? activity)
        {
            _scope = scope;
            _activity = activity;
        }

        public void Dispose()
        {
            _activity?.Dispose();
            _scope?.Dispose();
        }
    }
    
}