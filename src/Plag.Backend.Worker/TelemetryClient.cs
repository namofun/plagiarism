#nullable enable
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.Worker
{
    internal class FunctionsTelemetryClient : ITelemetryClient
    {
        private readonly TelemetryClient _appInsights;

        public FunctionsTelemetryClient(TelemetryClient telemetryClient)
        {
            _appInsights = telemetryClient;
        }

        public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string? message = null, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _appInsights.TrackAvailability(name, timeStamp, duration, runLocation, success, message, properties, metrics);
        }

        public void TrackDependency(string dependencyTypeName, string target, string operationName, string data, DateTimeOffset startTime, TimeSpan duration, string resultCode, bool success)
        {
            _appInsights.TrackDependency(dependencyTypeName, target, operationName, data, startTime, duration, resultCode, success);
        }

        public void TrackDependency(string dependencyTypeName, string operationName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            _appInsights.TrackDependency(dependencyTypeName, operationName, data, startTime, duration, success);
        }

        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _appInsights.TrackEvent(eventName, properties, metrics);
        }

        public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _appInsights.TrackException(exception, properties, metrics);
        }

        public void TrackPageView(string name)
        {
            _appInsights.TrackPageView(name);
        }

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            _appInsights.TrackRequest(name, startTime, duration, responseCode, success);
        }

        public void TrackTrace(string message, LogLevel severityLevel, IDictionary<string, string>? properties = null)
        {
            _appInsights.TrackTrace(message, GetSeverityLevel(severityLevel), properties);
        }

        private static SeverityLevel GetSeverityLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => SeverityLevel.Critical,
                LogLevel.Error => SeverityLevel.Error,
                LogLevel.Warning => SeverityLevel.Warning,
                LogLevel.Information => SeverityLevel.Information,
                _ => SeverityLevel.Verbose,
            };
        }

        public string GetHeadJavascript()
        {
            return string.Empty;
        }

        private IOperationHolder<DependencyTelemetry> StartInProcOperationScope(string scopeName)
        {
            return _appInsights.StartOperation(new DependencyTelemetry()
            {
                Name = scopeName,
                Type = "InProc",
                Context = { Operation = { Name = scopeName } }
            });
        }

        public void TrackScope(string scopeName, Action scopeFunc)
        {
            using var operationHolder = StartInProcOperationScope(scopeName);
            try
            {
                scopeFunc();
            }
            catch
            {
                operationHolder.Telemetry.Success = false;
                throw;
            }
            finally
            {
                _appInsights.StopOperation(operationHolder);
            }
        }

        public async Task TrackScope(string scopeName, Func<Task> scopeFunc)
        {
            using var operationHolder = StartInProcOperationScope(scopeName);
            try
            {
                await scopeFunc();
            }
            catch
            {
                operationHolder.Telemetry.Success = false;
                throw;
            }
            finally
            {
                _appInsights.StopOperation(operationHolder);
            }
        }

        public async Task<T> TrackScope<T>(string scopeName, Func<Task<T>> scopeFunc)
        {
            using var operationHolder = StartInProcOperationScope(scopeName);
            try
            {
                return await scopeFunc();
            }
            catch
            {
                operationHolder.Telemetry.Success = false;
                throw;
            }
            finally
            {
                _appInsights.StopOperation(operationHolder);
            }
        }

        public IDependencyTracker StartOperation(string dependencyTypeName, string? target, string operationName)
        {
            return new DependencyTracker(_appInsights, dependencyTypeName, target, operationName);
        }

        public void StopOperation(IDependencyTracker tracker)
        {
            if (tracker is DependencyTracker tracker1)
            {
                _appInsights.StopOperation(tracker1.OperationHolder);
            }
        }

        private sealed class DependencyTracker : IDependencyTracker
        {
            public IOperationHolder<DependencyTelemetry> OperationHolder { get; }

            public DependencyTracker(
                TelemetryClient telemetryClient,
                string dependencyTypeName,
                string? target,
                string operationName)
            {
                OperationHolder = telemetryClient.StartOperation(new DependencyTelemetry()
                {
                    Target = target,
                    Name = operationName,
                    Type = dependencyTypeName,
                });
            }

            public string DependencyType => OperationHolder.Telemetry.Type;

            public string? DependencyTarget => OperationHolder.Telemetry.Target;

            public string OperationName => OperationHolder.Telemetry.Name;

            public IDictionary<string, double> Metrics => OperationHolder.Telemetry.Metrics;

            public IDictionary<string, string> Properties => OperationHolder.Telemetry.Properties;

            public bool? Success
            {
                get => OperationHolder.Telemetry.Success;
                set => OperationHolder.Telemetry.Success = value;
            }

            public string? Data
            {
                get => OperationHolder.Telemetry.Data;
                set => OperationHolder.Telemetry.Data = value;
            }

            public string? ResultCode
            {
                get => OperationHolder.Telemetry.ResultCode;
                set => OperationHolder.Telemetry.ResultCode = value;
            }

            public void Dispose()
            {
                OperationHolder.Dispose();
            }
        }
    }
}
