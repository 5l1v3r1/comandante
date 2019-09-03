using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class Request : EmbeddedViewModel
    {
        public RequestModel Model { get; set; }

        public override async Task<EmbededViewResult> InitView()
        {
            var logsService = new MonitoringService();
            Model = new RequestModel();

            if (this.HttpContext.Request.Query.ContainsKey("TraceIdentifier"))
            {
                var traceIdentifier = this.HttpContext.Request.Query["TraceIdentifier"].ToString().Trim();
                Model.Request = new MonitoringService().AllRequests.FirstOrDefault(x => x.TraceIdentifier == traceIdentifier);
                Model.TraceIdentifier = traceIdentifier;
                
            }
            return await View();
        }

        public int ToLogTimelineValue(DateTime dateTime)
        {
            var value = (dateTime.ToUniversalTime() - Model.Request.Started.ToUniversalTime()).Ticks;
            var start = 0;
            var end = (Model.Request.Completed.ToUniversalTime() - Model.Request.Started.ToUniversalTime()).Ticks;
            return ConvertRange(start, end, 0, 200, value);
        }

        public int ToLogTimelineValue(TimeSpan? timeSpan, int minValue = 1)
        {
            if (timeSpan.HasValue == false)
                return minValue;

            var value = timeSpan.Value.Ticks;
            var start = 0;
            var end = (Model.Request.Completed.ToUniversalTime() - Model.Request.Started.ToUniversalTime()).Ticks;
            var convertedValue = ConvertRange(start, end, 0, 200, value);
            if (convertedValue < minValue)
                return minValue;
            return convertedValue;
        }

        public int ConvertRange(
            long originalStart, long originalEnd, // original range
            long newStart, long newEnd, // desired range
            long value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (int)(newStart + ((value - originalStart) * scale));
        }
    }

    public class RequestModel
    {
        public string TraceIdentifier { get; set; }
        public RequestResponseInfo Request { get; set; }
    }

}
