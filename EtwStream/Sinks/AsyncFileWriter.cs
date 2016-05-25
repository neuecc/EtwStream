using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtwStream
{
    internal class AsyncFileWriter
    {
        readonly ConcurrentQueue<string> q = new ConcurrentQueue<string>();
        readonly object gate = new object();
        readonly string sinkName;
        readonly FileStream fileStream;

        readonly Encoding encoding;
        readonly bool autoFlush;
        readonly byte[] newLine;

        Task lastQueueWorker;
        bool isConsuming = false;
        int isDisposed = 0;

        public string FileName { get; private set; }
        public long CurrentStreamLength { get; private set; }

        public AsyncFileWriter(string sinkName, string fileName, Encoding encoding, bool autoFlush)
        {
            {
                var fi = new FileInfo(fileName);
                if (!fi.Directory.Exists) fi.Directory.Create();
            }

            this.FileName = fileName;
            this.sinkName = sinkName;
            this.fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, true); // useAsync:true
            this.encoding = encoding;
            this.autoFlush = autoFlush;
            this.lastQueueWorker = Task.CompletedTask;
            this.newLine = encoding.GetBytes(Environment.NewLine);
            this.CurrentStreamLength = fileStream.Length;
        }

        bool SwitchStartConsume()
        {
            lock (gate)
            {
                if (isConsuming)
                {
                    return false;
                }
                else
                {
                    isConsuming = true;
                    return true;
                }
            }
        }

        async Task ConsumeQueue()
        {
            CONSUME_AGAIN:
            while (true)
            {
                string nextString;
                if (q.TryDequeue(out nextString))
                {
                    try
                    {
                        var bytes = encoding.GetBytes(nextString);
                        CurrentStreamLength += bytes.Length + newLine.Length;
                        if (!autoFlush)
                        {
                            await fileStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                            fileStream.Write(newLine, 0, newLine.Length);
                        }
                        else
                        {
                            await fileStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                            fileStream.Write(newLine, 0, newLine.Length);
                            await fileStream.FlushAsync().ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        EtwStreamEventSource.Log.SinkError(sinkName, "FileStream Write/Flush failed", ex.ToString());
                    }
                }
                else
                {
                    break;
                }
            }
            lock (gate)
            {
                // inlock, onNext enqued string and now checking isConsuming
                if (q.Count == 0)
                {
                    isConsuming = false;
                }
                else
                {
                    goto CONSUME_AGAIN;
                }
            }
        }

        public void Enqueue(string value)
        {
            q.Enqueue(value);
            if (SwitchStartConsume())
            {
                lastQueueWorker = ConsumeQueue();
            }
        }

        public List<string> Finalize()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                this.lastQueueWorker.Wait();
                try
                {
                    this.fileStream.Close();
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(sinkName, "FileStream Dispose failed", ex.ToString());
                }

                // rest line...
                var list = new List<string>();
                string r;
                while (q.TryDequeue(out r))
                {
                    list.Add(r);
                }

                return list;
            }
            return null;
        }
    }
}