namespace ProductWorkflow.API.Middleware
{
    public class GlocalErrorHandler
    {
        private readonly ILogger<GlocalErrorHandler> _logger;
        private readonly RequestDelegate _next;

        public GlocalErrorHandler(RequestDelegate requestDelegate, ILogger<GlocalErrorHandler> logger)
        {
            _logger = logger;
            _next = requestDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught in Middleware");
            }
        }
    }
}
