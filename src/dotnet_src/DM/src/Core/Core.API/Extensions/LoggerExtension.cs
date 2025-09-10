namespace Core.API.Extensions;

public static class LoggerExtension
{
    public static WebApplicationBuilder AddAppLogging(this WebApplicationBuilder builder)
    {
        // builder.Logging.AddConsole(opt =>
// {
//     opt.FormatterName = ConsoleFormatterNames.Simple;
// });

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "[HH:mm:ss] ";
            options.SingleLine = true;
        });
        //   builder.Logging.AddJsonConsole(o => o.IncludeScopes = true);
        return builder;
    }
}