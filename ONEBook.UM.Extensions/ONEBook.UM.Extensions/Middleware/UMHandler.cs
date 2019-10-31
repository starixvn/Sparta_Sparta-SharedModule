using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ONEbook.UM.Models;
using ONEbook.UM.Services;

namespace ONEbook.UM.Extensions.Middleware
{
    /// <summary>
    /// UMHandler middleware skips all routes contain "/Error"
    /// </summary>
    public class UMHandler
    {
        private readonly RequestDelegate _next;
        private readonly IMaintenanceService _maintenanceService;
        internal static Func<HttpContext, UnderMaintenanceTime, Task> _customUmMessage;

        public UMHandler(RequestDelegate next, IMaintenanceService maintenanceService)
        {
            _next = next;
            _maintenanceService = maintenanceService;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            if (path.HasValue && !path.Value.Contains("/Error", StringComparison.OrdinalIgnoreCase) &&
                await _maintenanceService.IsUnderMaintenanceAsync())
            {
                var maintenanceTime = await _maintenanceService.GetUnderMaintenanceTimeAsync();
                var responseMessage = _customUmMessage;

                if (responseMessage == null)
                {
                    if (IsAjaxRequest(context.Request))
                    {
                        responseMessage = JsonMessage;
                    }
                    else
                    {
                        responseMessage = HtmlMessage;
                    }
                }

                await responseMessage(context, maintenanceTime);
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        internal static Task JsonMessage(HttpContext context, UnderMaintenanceTime maintenanceTime)
        {
            context.Response.ContentType = "application/json";
            var content = Serializer.Serialize(new
            {
                isSuccess = false,
                message = "System is currently unavailable due to scheduled maintenance.\r\n" +
                    $"This downtime will last from {maintenanceTime.From} to {maintenanceTime.To}."
            });

            return context.Response.WriteAsync(content);
        }

        internal static Task HtmlMessage(HttpContext context, UnderMaintenanceTime maintenanceTime)
        {
            context.Response.ContentType = "text/html";
            var content = "<div style='text-align:center;padding-top:10%'>" +
                "System is currently unavailable due to scheduled maintenance.<br /> " +
                $"This downtime will last from <b>{maintenanceTime.From}</b> &rarr; <b>{maintenanceTime.To}</b>. " +
                "Thanks for your patience!</div>";

            return context.Response.WriteAsync(content);
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers != null && request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}