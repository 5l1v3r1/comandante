using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace Comandante.Services.EventListeners
{
    sealed class DotNETRuntimeEventListener : EventListener
    {
        // from https://docs.microsoft.com/en-us/dotnet/framework/performance/garbage-collection-etw-events
        private const int GC_KEYWORD = 0x0000001;
        private const int TYPE_KEYWORD = 0x0080000;
        private const int GCHEAPANDTYPENAMES_KEYWORD = 0x1000000;

        private const int JITKeyword_KEYWORD = 0x00000010;
        private const int JITTracingKeyword_KEYWORD = 0x00001000;
        private const int StackKeyword_KEYWORD =	0x40000000;

        public static BlockingCollection<EventWrittenEventArgs> Events = new BlockingCollection<EventWrittenEventArgs>();

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);


            //Console.WriteLine($"{eventSource.Guid} | {eventSource.Name}");

            //// look for .NET Garbage Collection events
            //if (eventSource.Name.Equals("Microsoft-Windows-DotNETRuntime"))
            //{
            //    EnableEvents(
            //        eventSource,
            //        EventLevel.Verbose,
            //        //(EventKeywords)(GC_KEYWORD | GCHEAPANDTYPENAMES_KEYWORD | TYPE_KEYWORD | JITKeyword_KEYWORD | JITTracingKeyword_KEYWORD | StackKeyword_KEYWORD)
            //        (EventKeywords)(JITTracingKeyword_KEYWORD | StackKeyword_KEYWORD)
            //        );
            //}
        }

        // from https://blogs.msdn.microsoft.com/dotnet/2018/12/04/announcing-net-core-2-2/
        // Called whenever an event is written.
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            
            Events.Add(eventData);
        }
    }
}
