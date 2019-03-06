using Destructurama;
using Serilog;
using Serilog.Configuration;

// ReSharper disable IdentifierTypo
namespace Aop.Profiler.SerilogBatchProcessor
{
    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration ProfilerTypes(this LoggerDestructuringConfiguration configuration)
        {
            return configuration.JsonNetTypes();
        }
    }
}
