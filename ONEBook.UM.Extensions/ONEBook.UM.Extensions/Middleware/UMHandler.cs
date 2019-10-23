using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ONEbook.UM.Services;

namespace ONEBook.UM.Extensions.Middleware
{
    /// <summary>
    /// UMHandler middleware skips all routes contain "/Error"
    /// </summary>
    public class UMHandler
    {
        private readonly RequestDelegate _next;
        private readonly IMaintenanceService _maintenanceService;

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
                var response = context.Response;
                string content = null;

                if (IsAjaxRequest(context.Request))
                {
                    response.ContentType = "application/json";
                    content = JsonConvert.SerializeObject(new
                    {
                        IsSuccess = false,
                        Message = "System is currently unavailable due to scheduled maintenance.\r\n" +
                            $"This downtime will last from {maintenanceTime.From} to {maintenanceTime.To}."
                    });
                }
                else
                {
                    response.ContentType = "text/html";
                    content = "<div style='text-align:center;padding-top:10%'>" +
                        "System is currently unavailable due to scheduled maintenance.<br /> " +
                        $"This downtime will last from <b>{maintenanceTime.From}</b> &rarr; <b>{maintenanceTime.To}</b>. " +
                        "Thanks for your patience!</div>";
                }

                await response.WriteAsync(content);
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            if (request.Headers == null)
            {
                return false;
            }

            var headers = request.Headers;
            var isXhr = headers["X-Requested-With"] == "XMLHttpRequest";
            var isFetch = headers["Accept"] == "*/*" && headers["Sec-Fetch-Mode"] == "cors";

            return isXhr || isFetch;
        }
    }
}