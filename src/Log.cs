using System;
using System.IO;

namespace Rinshan
{
    // A small text log in %LOCALAPPDATA%\Rinshan\log.txt, so a bug report can include what went wrong
    // (Tenhou not loading, a failed update, a crash). It never records the Tenhou ID or anything typed.
    static class Log
    {
        const long MaxSize = 256 * 1024;   // then it becomes log.old.txt and a new one starts
        static readonly object gate = new object();

        public static string FilePath { get; private set; }

        public static void Init(string directory)
        {
            FilePath = Path.Combine(directory, "log.txt");
            try
            {
                if (File.Exists(FilePath) && new FileInfo(FilePath).Length > MaxSize)
                {
                    var old = Path.Combine(directory, "log.old.txt");
                    if (File.Exists(old)) File.Delete(old);
                    File.Move(FilePath, old);
                }
            }
            catch { }
        }

        public static void Write(string message)
        {
            if (FilePath == null) return;
            lock (gate)
            {
                try { File.AppendAllText(FilePath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine); }
                catch { }
            }
        }

        public static void Error(string what, Exception ex)
        {
            Write(what + ": " + ex.GetType().Name + ": " + ex.Message);
        }

        // Crashes also get the stack trace
        public static void Crash(Exception ex)
        {
            Write("Unexpected error: " + ex);
        }
    }
}
