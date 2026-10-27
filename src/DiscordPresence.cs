using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Rinshan
{
    // Minimal Discord Rich Presence client over Discord's local IPC pipe (\\.\pipe\discord-ipc-N).
    // Runs on its own thread: callers just set the latest activity and it is sent when Discord is available.
    class DiscordPresence : IDisposable
    {
        const int OpHandshake = 0, OpFrame = 1, OpClose = 2;

        readonly JavaScriptSerializer json = new JavaScriptSerializer();
        readonly AutoResetEvent wake = new AutoResetEvent(false);
        readonly object gate = new object();
        readonly Thread worker;
        string appId;
        Dictionary<string, object> activity;
        bool dirty, disposed;
        NamedPipeClientStream pipe;

        public DiscordPresence()
        {
            worker = new Thread(Run) { IsBackground = true, Name = "Discord presence" };
            worker.Start();
        }

        // Changing the application id reconnects with the new one
        public void Configure(string applicationId)
        {
            lock (gate)
            {
                if (applicationId == appId) return;
                appId = applicationId;
                dirty = true;
                Disconnect();
            }
            wake.Set();
        }

        // null clears the presence
        public void Set(Dictionary<string, object> newActivity)
        {
            lock (gate)
            {
                activity = newActivity;
                dirty = true;
            }
            wake.Set();
        }

        void Run()
        {
            while (!disposed)
            {
                // Retry periodically so presence appears even if Discord is started after Tenhou
                wake.WaitOne(15000);
                if (disposed) break;

                string id;
                Dictionary<string, object> current;
                lock (gate)
                {
                    if (!dirty) continue;
                    id = appId;
                    current = activity;
                }
                if (string.IsNullOrEmpty(id)) continue;

                try
                {
                    if (pipe == null || !pipe.IsConnected) Connect(id);
                    if (pipe == null) continue;
                    Send(OpFrame, new Dictionary<string, object> {
                        { "cmd", "SET_ACTIVITY" },
                        { "args", new Dictionary<string, object> { { "pid", Process.GetCurrentProcess().Id }, { "activity", current } } },
                        { "nonce", Guid.NewGuid().ToString() }
                    });
                    Read();
                    lock (gate) { if (current == activity) dirty = false; }
                }
                catch (Exception)
                {
                    lock (gate) Disconnect();
                }
            }
        }

        void Connect(string id)
        {
            for (int i = 0; i < 10; i++)
            {
                var candidate = new NamedPipeClientStream(".", "discord-ipc-" + i, PipeDirection.InOut);
                try { candidate.Connect(200); }
                catch { candidate.Dispose(); continue; }
                pipe = candidate;
                Send(OpHandshake, new Dictionary<string, object> { { "v", 1 }, { "client_id", id } });
                if (Read() == OpClose) { Disconnect(); return; }   // e.g. an invalid application id
                return;
            }
        }

        void Send(int op, object payload)
        {
            var body = Encoding.UTF8.GetBytes(json.Serialize(payload));
            var frame = new byte[8 + body.Length];
            BitConverter.GetBytes(op).CopyTo(frame, 0);
            BitConverter.GetBytes(body.Length).CopyTo(frame, 4);
            body.CopyTo(frame, 8);
            pipe.Write(frame, 0, frame.Length);
            pipe.Flush();
        }

        int Read()
        {
            var header = ReadExactly(8);
            int op = BitConverter.ToInt32(header, 0);
            ReadExactly(BitConverter.ToInt32(header, 4));
            return op;
        }

        byte[] ReadExactly(int count)
        {
            var buffer = new byte[count];
            int read = 0;
            while (read < count)
            {
                int n = pipe.Read(buffer, read, count - read);
                if (n <= 0) throw new IOException("Discord closed the connection");
                read += n;
            }
            return buffer;
        }

        void Disconnect()
        {
            if (pipe != null) { try { pipe.Dispose(); } catch { } pipe = null; }
        }

        public void Dispose()
        {
            disposed = true;
            wake.Set();
            lock (gate) Disconnect();
        }
    }
}
