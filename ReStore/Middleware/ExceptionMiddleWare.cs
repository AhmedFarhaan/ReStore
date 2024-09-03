using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ReStore.Middleware
{
    public class ExceptionMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleWare> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleWare(RequestDelegate next, ILogger<ExceptionMiddleWare>logger,IHostEnvironment env)
        {
            _next = next; //allow us to excute our next method
            _logger = logger; //allow us to log any exception we get
            _env = env; // to see if we are running in production mode or in developer mode
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // next piece of middleware, pass it to InvokeAsync()
            }
            catch(Exception ex) //catchs the exception and display it in json format
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;

                var response = new ProblemDetails
                {
                    Status = 500,
                    Detail = _env.IsDevelopment() ? ex.StackTrace?.ToString() : null,
                    Title=ex.Message
                };
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);
                //what will be returned to the client if this problem poped up
                await context.Response.WriteAsync(json);
            }
        }
    }
}
