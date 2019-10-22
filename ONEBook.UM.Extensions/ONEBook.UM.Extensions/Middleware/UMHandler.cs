using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

                await context.Response.WriteAsync(
                    "<div style='text-align:center;padding-top:10%'>" +
                    "System is currently unavailable due to scheduled maintenance.<br /> " +
                    $"This downtime will last from <b>{maintenanceTime.From}</b> &rarr; <b>{maintenanceTime.To}</b>. " +
                    "Thanks for your patience!</div>");
                return;
            }

            await _next.Invoke(context);
        }
    }
}