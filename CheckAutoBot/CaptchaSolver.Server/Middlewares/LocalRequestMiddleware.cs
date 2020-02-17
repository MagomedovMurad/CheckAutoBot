using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptchaSolver.Server.Middlewares
{
    public class LocalRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public LocalRequestMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var hostString = context.Request.Host;

            if (hostString.Host == "127.0.0.1")
            { 
                
            }

            var id = context.Request.Query["id"];
            if (Guid.TryParse(id, out Guid result))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Token is invalid");
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }

}
