using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aop.Profiler.EventProcessing;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;

namespace Aop.Profiler.SerilogBatchProcessor
{
    public class SerilogBatchProcessor : EventBatchProcessorBase
    {
        private readonly string _messageTemplate;
        private readonly LogEventLevel _logEventLevel;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _serializerSettings;

        public SerilogBatchProcessor(ILogger logger, int batchSizeLimit, TimeSpan period, uint queueLimit, LogEventLevel logEventLevel = LogEventLevel.Information, string messageTemplate = "Profiler Results")
        : base(batchSizeLimit, period, queueLimit)
        {
            _logger = logger;
            _logEventLevel = logEventLevel;
            _messageTemplate = messageTemplate;
            _serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        }

        public override Task ProcessEvents(Queue<IDictionary<string, object>> queue)
        {
            return
                Task
                    .Run
                    (
                        () =>
                        {
                            foreach (var @event in queue)
                            {
                                var properties = @event
                                    .Select
                                    (
                                        x =>
                                        {
                                            if (x.Key == nameof(CaptureOptions.SerializedResult) || x.Key == nameof(CaptureOptions.SerializedInputParameters))
                                            {
                                                return
                                                    new PropertyEnricher
                                                    (
                                                        x.Key,
                                                        JsonConvert.DeserializeObject<dynamic>((string)x.Value, _serializerSettings),
                                                        true
                                                    );
                                            }

                                            return new PropertyEnricher(x.Key, x.Value);
                                        }
                                    );

                                _logger
                                    .ForContext(properties)
                                    .Write(_logEventLevel, _messageTemplate);
                            }
                        }
                    );
        }
    }
}
