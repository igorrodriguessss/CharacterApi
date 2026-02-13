using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace $safeprojectname$.Resources
{
    class EventLogging
    {
        private static readonly Dictionary<string, EventLog> _logs = new Dictionary<string,EventLog>(StringComparer.Ordinal);

        public static void WriteEvent(string eventLog, string eventSource, int category, EventLogEntryType eventType, long eventId, object[] arguments, Exception error)
        {
            EventLog log;
            lock(_logs)
            {
                if(!_logs.TryGetValue(eventLog + '.' + eventSource, out log))
                    _logs.Add(eventLog + '.' + eventSource, log = new EventLog(eventLog, ".", eventSource));
            }

            const int fixedCount = 10;
            object[] fullargs = new object[arguments.Length + fixedCount];
            int ix = 0;
            fullargs[ix++] = eventId.ToString("x8");
            fullargs[ix++] = error == null 
                ? Resources.Exceptions.ExceptionStrings.HelpLinkFormat((int)eventId, typeof(Resources.Messages).FullName) 
                : error.HelpLink;
            fullargs[ix++] = error == null ? null : error.GetType().ToString();
            fullargs[ix++] = error == null ? null : error.GetBaseException().GetType().ToString();
            fullargs[ix++] = error == null || error.InnerException == null ? null : error.InnerException.StackTrace;
            fullargs[ix++] = new StackTrace(3).ToString();
            Array.Copy(arguments, 0, fullargs, fixedCount, arguments.Length);
            
            byte[] data = null;
            if (error != null)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter ser = new BinaryFormatter();
                        ser.Serialize(ms, error);
                        data = ms.ToArray();
                    }
                }
                catch { }
            }

            lock(log)
                log.WriteEvent(new EventInstance(eventId, category, eventType), data, fullargs);
        }
    }
}
