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
        readonly BlockingCollection<string> q = new BlockingCollection<string>();
        readonly object gate = new object();
        readonly string sinkName;
        readonly FileStream fileStream;

        readonly Encoding encoding;
        readonly bool autoFlush;
        readonly byte[] newLine;

        readonly Task processingTask;
        int isDisposed = 0;
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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
            this.fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, useAsync: false); // useAsync:false, use dedicated processor
            this.encoding = encoding;
            this.autoFlush = autoFlush;

            this.newLine = encoding.GetBytes(Environment.NewLine);
            this.CurrentStreamLength = fileStream.Length;

            this.processingTask = Task.Factory.StartNew(ConsumeQueue, TaskCreationOptions.LongRunning);
        }

        void ConsumeQueue()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                string nextString;
                try
                {
                    if (q.TryTake(out nextString, Timeout.Infinite, cancellationTokenSource.Token))
                    {
                        try
                        {
                            var bytes = encoding.GetBytes(nextString);
                            CurrentStreamLength += bytes.Length + newLine.Length;
                            if (!autoFlush)
                            {
                                fileStream.Write(bytes, 0, bytes.Length);
                                fileStream.Write(newLine, 0, newLine.Length);
                            }
                            else
                            {
                                fileStream.Write(bytes, 0, bytes.Length);
                                fileStream.Write(newLine, 0, newLine.Length);
                                fileStream.Flush();
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
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        public void Enqueue(string value)
        {
            q.Add(value);
        }

        public string[] Finalize()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                cancellationTokenSource.Cancel();
                processingTask.Wait();
                try
                {
                    this.fileStream.Close();
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(sinkName, "FileStream Dispose failed", ex.ToString());
                }

                // rest line...
                return q.ToArray();
            }
            return null;
        }
    }
}