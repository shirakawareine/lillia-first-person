using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace FirstPersonTest;

// File-only, bounded logging. No BepInEx ConsoleLogListener, Unity calls or joins.
// The game thread never waits for disk/console output or for the writer to stop.
internal sealed class DiagnosticLog
{
    private readonly BlockingCollection<string> _messages = new(64);
    internal DiagnosticLog(string path)
    {
        var worker = new Thread(() => Run(path)) { IsBackground = true, Name = "FirstPersonTest log" };
        worker.Start();
    }

    internal void Write(string message)
    {
        try { _messages.TryAdd($"{DateTime.UtcNow:O} {message}", 0); }
        catch (InvalidOperationException) { } // shutdown raced with a callback
    }

    internal void Close() => _messages.CompleteAdding(); // deliberately no Join()

    private void Run(string path)
    {
        try
        {
            using var file = new StreamWriter(new FileStream(path, FileMode.Append,
                FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
            foreach (string message in _messages.GetConsumingEnumerable())
                file.WriteLine(message);
        }
        catch (Exception)
        {
            // Do not fall back to the same synchronous global logger that hung the game.
            // If the file is unwritable, logging stops; the bounded queue cannot grow.
        }
    }
}
