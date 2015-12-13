using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;

namespace EtwStream
{
    public static class RollingFileSink
    {
        // TraceEvent

        public static IDisposable LogToRollingFile(this IObservable<TraceEvent> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Func<TraceEvent, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new TraceEventSink(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToRollingFile(this IObservable<IList<TraceEvent>> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Func<TraceEvent, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new TraceEventSink(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        // EventArgs

        public static IDisposable LogToRollingFile(this IObservable<EventWrittenEventArgs> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Func<EventWrittenEventArgs, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new EventWrittenEventArgsSink(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToRollingFile(this IObservable<IList<EventWrittenEventArgs>> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Func<EventWrittenEventArgs, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new EventWrittenEventArgsSink(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        // string

        public static IDisposable LogToRollingFile(this IObservable<string> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Encoding encoding, bool autoFlush)
        {
            var sink = new StringSink(fileNameSelector, timestampPattern, rollSizeKB, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        public static IDisposable LogToRollingFile(this IObservable<IList<string>> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Encoding encoding, bool autoFlush)
        {
            var sink = new StringSink(fileNameSelector, timestampPattern, rollSizeKB, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        // Sinks

        abstract class RollingFileSinkBase<T> : SinkBase<T>
        {
            static readonly Regex NumberRegex = new Regex("(\\d)+$", RegexOptions.Compiled);

            readonly object newFileLock = new object();
            protected readonly Func<T, string> messageFormatter;
            readonly Func<DateTime, string> timestampPattern;
            readonly Func<DateTime, int, string> fileNameSelector;
            readonly Encoding encoding;
            readonly bool autoFlush;
            readonly long rollSizeInBytes;

            string currentTimestampPattern;

            protected AsyncFileWriter asyncFileWriter;

            public RollingFileSinkBase(
                 Func<DateTime, int, string> fileNameSelector,
                 Func<DateTime, string> timestampPattern,
                 int rollSizeKB,
                 Func<T, string> messageFormatter,
                 Encoding encoding,
                 bool autoFlush)
            {
                this.messageFormatter = messageFormatter;
                this.timestampPattern = timestampPattern;
                this.fileNameSelector = fileNameSelector;
                this.rollSizeInBytes = rollSizeKB * 1024;
                this.encoding = encoding;
                this.autoFlush = autoFlush;

                ValidateFileNameSelector();
            }

            void ValidateFileNameSelector()
            {
                var now = DateTime.Now;
                var fileName1 = Path.GetFileNameWithoutExtension(fileNameSelector(now, 0));
                var fileName2 = Path.GetFileNameWithoutExtension(fileNameSelector(now, 1));


                if (!NumberRegex.IsMatch(fileName1) || !NumberRegex.IsMatch(fileName2))
                {
                    throw new ArgumentException("fileNameSelector is invalid format, must be int(sequence no) is last.");
                }

                var seqStr1 = NumberRegex.Match(fileName1).Groups[0].Value;
                var seqStr2 = NumberRegex.Match(fileName2).Groups[0].Value;

                int seq1;
                int seq2;
                if (!int.TryParse(seqStr1, out seq1) || !int.TryParse(seqStr2, out seq2))
                {
                    throw new ArgumentException("fileNameSelector is invalid format, must be int(sequence no) is last.");
                }

                if (seq1 == seq2)
                {
                    throw new ArgumentException("fileNameSelector is invalid format, must be int(sequence no) is incremental.");
                }
            }

            protected void CheckFileRolling()
            {
                var now = DateTime.Now;
                string ts;
                try
                {
                    ts = timestampPattern(now);
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(nameof(RollingFileSink), "timestampPattern convert failed", ex.ToString());
                    return;
                }

                // needs to create next file
                var disposeTarget = asyncFileWriter;
                if (disposeTarget == null || ts != currentTimestampPattern || disposeTarget?.CurrentStreamLength >= rollSizeInBytes)
                {
                    lock (newFileLock)
                    {
                        if (this.asyncFileWriter == disposeTarget)
                        {
                            int sequenceNo = 0;
                            if (disposeTarget != null)
                            {
                                sequenceNo = ExtractCurrentSequence(asyncFileWriter.FileName) + 1;
                            }

                            string fn = null;
                            while (true)
                            {
                                try
                                {
                                    var newFn = fileNameSelector(now, sequenceNo);
                                    if (fn == newFn)
                                    {
                                        EtwStreamEventSource.Log.SinkError(nameof(RollingFileSink), "fileNameSelector indicate same filname", "");
                                        return;
                                    }
                                    fn = newFn;
                                }
                                catch (Exception ex)
                                {
                                    EtwStreamEventSource.Log.SinkError(nameof(RollingFileSink), "fileNamemessageFormatter convert failed", ex.ToString());
                                    return;
                                }

                                var fi = new FileInfo(fn);
                                if (fi.Exists)
                                {
                                    if (fi.Length >= rollSizeInBytes)
                                    {
                                        sequenceNo++;
                                        continue;
                                    }
                                }
                                break;
                            }

                            List<string> safe;
                            try
                            {
                                safe = disposeTarget?.Finalize(); // block!
                            }
                            catch (Exception ex)
                            {
                                EtwStreamEventSource.Log.SinkError(nameof(RollingFileSink), "Can't dispose fileStream", ex.ToString());
                                return;
                            }
                            try
                            {
                                asyncFileWriter = new AsyncFileWriter(nameof(RollingFileSink), fn, encoding, autoFlush);
                                currentTimestampPattern = ts;
                                if (safe != null)
                                {
                                    foreach (var item in safe)
                                    {
                                        asyncFileWriter.Enqueue(item);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                EtwStreamEventSource.Log.SinkError(nameof(RollingFileSink), "Can't create FileStream", ex.ToString());
                                return;
                            }
                        }
                    }
                }
            }

            static int ExtractCurrentSequence(string fileName)
            {
                int extensionDotIndex = fileName.LastIndexOf('.');

                fileName = Path.GetFileNameWithoutExtension(fileName);

                var sequenceString = NumberRegex.Match(fileName).Groups[0].Value;
                int seq;
                if (int.TryParse(sequenceString, out seq))
                {
                    return seq;
                }
                else
                {
                    return 0;
                }
            }

            public override void OnNext(T value)
            {
                CheckFileRolling();

                string v;
                try
                {
                    v = messageFormatter(value);
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(nameof(RollingFileSink), "messageFormatter convert failed", ex.ToString());
                    return;
                }
                asyncFileWriter.Enqueue(v);
            }

            public override void OnNext(IList<T> value)
            {
                CheckFileRolling();

                string v;
                try
                {
                    v = string.Join(Environment.NewLine, value.Select(x => messageFormatter(x)));
                }
                catch (Exception ex)
                {
                    EtwStreamEventSource.Log.SinkError(nameof(RollingFileSink), "messageFormatter convert failed", ex.ToString());
                    return;
                }
                asyncFileWriter.Enqueue(v);
            }

            public override void Dispose()
            {
                asyncFileWriter?.Finalize();
            }
        }


        class TraceEventSink : RollingFileSinkBase<TraceEvent>
        {
            public TraceEventSink(
                Func<DateTime, int, string> fileNameSelector,
                Func<DateTime, string> timestampPattern,
                int rollSizeKB,
                Func<TraceEvent, string> messageFormatter,
                Encoding encoding,
                bool autoFlush)
                : base(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush)
            {
            }
        }

        class EventWrittenEventArgsSink : RollingFileSinkBase<EventWrittenEventArgs>
        {
            public EventWrittenEventArgsSink(
                Func<DateTime, int, string> fileNameSelector,
                Func<DateTime, string> timestampPattern,
                int rollSizeKB,
                Func<EventWrittenEventArgs, string> messageFormatter,
                Encoding encoding,
                bool autoFlush)
                : base(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush)
            {
            }
        }

        class StringSink : RollingFileSinkBase<string>
        {
            public StringSink(
                Func<DateTime, int, string> fileNameSelector,
                Func<DateTime, string> timestampPattern,
                int rollSizeKB,
                Encoding encoding,
                bool autoFlush)
                : base(fileNameSelector, timestampPattern, rollSizeKB, x => x, encoding, autoFlush)
            {
            }
        }
    }
}