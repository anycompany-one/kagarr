using System;
using NLog;

namespace Kagarr.Common.Instrumentation
{
    public static class KagarrLogger
    {
        public static Logger GetLogger(object obj)
        {
            return LogManager.GetLogger(obj.GetType().FullName);
        }

        public static Logger GetLogger(Type type)
        {
            return LogManager.GetLogger(type.FullName);
        }
    }
}
