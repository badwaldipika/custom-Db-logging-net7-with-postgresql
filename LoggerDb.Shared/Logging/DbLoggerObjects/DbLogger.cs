namespace LoggerDb.Shared.Logging.DbLoggerObjects
{
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using NpgsqlTypes;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    namespace LoggerDb.Shared.Logging.DbLoggerObjects
    {
        /// <summary>
        /// Writes a log entry to the database.
        /// </summary>
        public class DbLogger : ILogger
        {
            /// <summary>
            /// Instance of <see cref="DbLoggerProvider" />.
            /// </summary>
            private readonly DbLoggerProvider _dbLoggerProvider;

            /// <summary>
            /// Creates a new instance of <see cref="FileLogger" />.
            /// </summary>
            /// <param name="fileLoggerProvider">Instance of <see cref="FileLoggerProvider" />.</param>
            public DbLogger([NotNull] DbLoggerProvider dbLoggerProvider)
            {
                _dbLoggerProvider = dbLoggerProvider;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            /// <summary>
            /// Whether to log the entry.
            /// </summary>
            /// <param name="logLevel"></param>
            /// <returns></returns>
            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel != LogLevel.None;
            }


            /// <summary>
            /// Used to log the entry.
            /// </summary>
            /// <typeparam name="TState"></typeparam>
            /// <param name="logLevel">An instance of <see cref="LogLevel"/>.</param>
            /// <param name="eventId">The event's ID. An instance of <see cref="EventId"/>.</param>
            /// <param name="state">The event's state.</param>
            /// <param name="exception">The event's exception. An instance of <see cref="Exception" /></param>
            /// <param name="formatter">A delegate that formats </param>
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    // Don't log the entry if it's not enabled.
                    return;
                }

                var threadId = Thread.CurrentThread.ManagedThreadId; // Get the current thread ID to use in the log file. 

                // Store record.
                using (var connection = new NpgsqlConnection(_dbLoggerProvider.Options.ConnectionString))
                {
                    connection.Open();

                    // Add to database.

                    // LogLevel
                    // ThreadId
                    // EventId
                    // Exception Message (use formatter)
                    // Exception Stack Trace
                    // Exception Source

                    string loglevel, threadid, eventid, eventname, message, exceptionmessage, exceptionstacktrace, exceptionsource;

                    loglevel = threadid = eventid = eventname = message = exceptionmessage = exceptionstacktrace = exceptionsource = string.Empty;

                    if (_dbLoggerProvider?.Options?.LogFields?.Any() ?? false)
                    {
                        foreach (var logField in _dbLoggerProvider.Options.LogFields)
                        {
                            switch (logField)
                            {
                                case "LogLevel":
                                    if (!string.IsNullOrWhiteSpace(logLevel.ToString()))
                                    {
                                        loglevel = logLevel.ToString();
                                    }
                                    break;
                                case "ThreadId":
                                    threadid = threadId.ToString();
                                    break;
                                case "EventId":
                                    eventid = eventId.Id.ToString();
                                    break;
                                case "EventName":
                                    if (!string.IsNullOrWhiteSpace(eventId.Name))
                                    {
                                        eventname = eventId.Name;
                                    }
                                    break;
                                case "Message":
                                    if (!string.IsNullOrWhiteSpace(formatter(state, exception)))
                                    {
                                        message = formatter(state, exception);
                                    }
                                    break;
                                case "ExceptionMessage":
                                    if (exception != null &&
                                        !string.IsNullOrWhiteSpace(exception.Message))
                                    {
                                        exceptionmessage = exception?.Message;
                                    }
                                    break;
                                case "ExceptionStackTrace":
                                    if (exception != null
                                        && !string.IsNullOrWhiteSpace(exception.StackTrace))
                                    {
                                        exceptionstacktrace = exception?.StackTrace;
                                    }
                                    break;
                                case "ExceptionSource":
                                    if (exception != null
                                        && !string.IsNullOrWhiteSpace(exception.Source))
                                    {
                                        exceptionsource = exception?.Source;
                                    }
                                    break;
                            }
                        }
                    }

                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = string.Format("INSERT INTO {0} (date, thread_id, log_level, event_id, event_name, exception_message, exception_stack_trace, exception_source) " +
                            "VALUES (@date, @thread_id, @log_level, @event_id, @event_name, @exception_message, @exception_stack_trace, @exception_source)",
                            _dbLoggerProvider.Options.LogTable);

                        var curruntDateTime = new NpgsqlParameter("@date", NpgsqlDbType.TimestampTz);
                        curruntDateTime.Value = DateTime.Now;
                        command.Parameters.Add(curruntDateTime);
                        command.Parameters.Add(new NpgsqlParameter("@thread_id", threadid));
                        command.Parameters.Add(new NpgsqlParameter("@log_level", loglevel));
                        command.Parameters.Add(new NpgsqlParameter("@event_id", eventid));
                        command.Parameters.Add(new NpgsqlParameter("@event_name", eventname));
                        command.Parameters.Add(new NpgsqlParameter("@exception_message", exceptionmessage));
                        command.Parameters.Add(new NpgsqlParameter("@exception_stack_trace", exceptionstacktrace));
                        command.Parameters.Add(new NpgsqlParameter("@exception_source", exceptionsource));

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
        }
    }

}
