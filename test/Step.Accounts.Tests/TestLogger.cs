using System;
using Microsoft.Extensions.Logging;

namespace Tests
{
    public class TestLogger : ILogger
    {
        public TestLogger() { }
        public TestLogger(string context)
        {
            _context = context;
        }

        private string _context = "";

        // public List<TestLoggerMessage> Messages { get; set; } = new List<TestLoggerMessage>();
        // public class TestLoggerMessage
        // {
        //     public LogLevel Level { get; set; }
        //     public EventId EventId { get; set; }
        //     public string Message { get; set; }
        //     public Exception Exception { get; set; }
        // }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(formatter(state, exception));
            // Messages.Add(new Tests.TestLogger.TestLoggerMessage()
            // {
            //     EventId = eventId,
            //     Level = logLevel,
            //     Message = formatter(state, exception),
            //     Exception = exception
            // });
        }
    }

    public class TestLogger<T> :TestLogger,ILogger<T>
    {

    }

    public class TestLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string context)
        {
            return new TestLogger();
        }

        public ILogger CreateLogger(Type context)
        {
            return new TestLogger(context.Name);
        }

        public void AddProvider(ILoggerProvider provider)
        {

        }

        public void Dispose()
        {

        }
    }

}