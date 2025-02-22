using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace EShopWeb.Common
{
    public class CustomFileLoggerOptions
    {
        public virtual string FilePath { get; set; }
        public virtual string FolderPath { get; set; }
    }

    [ProviderAlias("CustomWebLoggerProvider")]
    public class CustomFileLoggerProvider : ILoggerProvider, IDisposable
    {
        public readonly CustomFileLoggerOptions Options;

        public CustomFileLoggerProvider(IOptions<CustomFileLoggerOptions> _options)
        {
            Options = _options.Value;
            if (!Directory.Exists(Options.FolderPath))
            {
                Directory.CreateDirectory(Options.FolderPath);
            }
        }

        public ILogger CreateLogger(string CategoryName)
        {
            return new CustomFileLogger(this);
        }

        public void Dispose()
        {
        }
    }

    public class CustomFileLogger : ILogger
    {
        protected readonly CustomFileLoggerProvider customFileLoggerProvider;
        public CustomFileLogger([NotNull] CustomFileLoggerProvider provider)
        {
            customFileLoggerProvider = provider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        private object lockobject = new object();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception ex, Func<TState, Exception, string> formatter)
        {
            //if (!IsEnabled(logLevel))
            //{
            //    return;
            //}
            
            IReadOnlyList<KeyValuePair<string, object>> obj = (IReadOnlyList<KeyValuePair<string, object>>)state;
            string fullPath = customFileLoggerProvider.Options.FolderPath + "/" + string.Format(customFileLoggerProvider.Options.FilePath, (DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0')));
            lock (lockobject)
            {
                if (logLevel == LogLevel.Error)
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.None))
                    {
                        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                        sw.WriteLine("=============================================================Start Exception Details=============================================================");
                        sw.WriteLine("Time : " + DateTime.Now.ToString());
                        sw.WriteLine("Name : " + obj.FirstOrDefault(m => m.Key == "Name").Value);
                        sw.WriteLine("USERNAME : " + obj.FirstOrDefault(m => m.Key == "USERNAME").Value);
                        sw.WriteLine("EMAILID : " + obj.FirstOrDefault(m => m.Key == "EMAILID").Value);
                        sw.WriteLine("Exception handled ClassName : " + obj.FirstOrDefault(m => m.Key == "TypeName").Value);
                        sw.WriteLine();
                        sw.WriteLine((ex != null) ? ex.ToString() : "");
                        sw.WriteLine();
                        sw.WriteLine("==============================================================End Exeption Details================================================================" + Environment.NewLine);
                        sw.Flush();
                        sw.Close();
                    }
                }
                else
                {
                    //using (FileStream fs = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.None))
                    //{
                    //    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    //    sw.WriteLine("=============================================================Start " + logLevel.ToString() + " Details=============================================================");
                    //    sw.WriteLine("");
                    //    sw.WriteLine("Time : " + DateTime.Now.ToString());
                    //    sw.WriteLine("Name : " + obj.FirstOrDefault(m => m.Key == "Name").Value);
                    //    sw.WriteLine("USERNAME : " + obj.FirstOrDefault(m => m.Key == "USERNAME").Value);
                    //    sw.WriteLine("EMAILID : " + obj.FirstOrDefault(m => m.Key == "EMAILID").Value);
                    //    sw.WriteLine("");
                    //    sw.WriteLine(obj.ToString());
                    //    sw.WriteLine("");
                    //    sw.WriteLine("==============================================================End " + logLevel.ToString() + " Details================================================================" + Environment.NewLine);
                    //    sw.Flush();
                    //    sw.Close();
                    //}
                }
            }
        }
    }
}
