using System.Linq;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using Serilog;
using Serilog.Events;

namespace UniAlltid.Language.API.Providers
{
    public class ExceptionLogProvider : ExceptionLogger
    {
        private readonly string _messageTemplate;
        private readonly ILogger _logger;
        private readonly LogEventLevel _logLevel;

        public ExceptionLogProvider(ILogger logger, LogEventLevel logLevel, string messageTemplate)
        {
            _logger = logger.ForContext<ExceptionLogProvider>();
            _messageTemplate = messageTemplate;
            _logLevel = logLevel;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            var ctx = context.Request.GetOwinContext();
            var path = string.Empty;
            try
            {
                path = ctx.Request.Uri.AbsolutePath;
            }
            catch
            {
                //Ignored
            }

            var logger = _logger;
            if (ctx.Request.Query.Any())
            {
                var query = ctx.Request.Query.Select(c => $"{c.Key}: {string.Join(",", c.Value)}");
                logger = logger.ForContext("QueryParams", query);
            }
            logger.Write(_logLevel, context.Exception, _messageTemplate, context.Request.Method, path);
        }
    }
}