using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using Comandante.Pages;
using Comandante.PagesRenderer;
using Comandante.Services.EventListeners;
using Microsoft.AspNetCore.Mvc;

namespace Comandante.Pages
{
    public class EventListener : EmbeddedViewModel
    {

        public EventListenerModel Model { get; set; }

        public override async Task<EmbededViewResult> Execute()
        {
            Model = new EventListenerModel();
            Model.Events = DotNETRuntimeEventListener.Events;
            return await View();
        }
    }

    public class EventListenerModel
    {
        public BlockingCollection<EventWrittenEventArgs> Events;
    }

}
