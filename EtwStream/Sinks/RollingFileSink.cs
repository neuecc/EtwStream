using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;
#if TRACE_EVENT
using Microsoft.Diagnostics.Tracing;
#endif

namespace EtwStream
{
    public static class RollingFileSink
    {
        // TraceEvent

#if TRACE_EVENT

        /// <summary>
        /// Write to text file, file is rolled.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="fileNameSelector">Selector of output file name. DateTime is date of file open time, int is number sequence.</param>
        /// <param name="timestampPattern">Pattern of rolling identifier. DateTime is write time of message. If pattern is different roll new file.</param>
        /// <param name="rollSizeKB">Size of start next file.</param>
        /// <param name="messageFormatter">Converter of message per line.</param>
        /// <param name="encoding">String encoding.</param>
        /// <param name="autoFlush">If true, call Flush on every write.</param>
        public static IDisposable LogToRollingFile(this IObservable<TraceEvent> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Func<TraceEvent, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new TraceEventSink(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        /// <summary>
        /// Write to text file, file is rolled.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="fileNameSelector">Selector of output file name. DateTime is date of file open time, int is number sequence.</param>
        /// <param name="timestampPattern">Pattern of rolling identifier. DateTime is write time of message. If pattern is different roll new file.</param>
        /// <param name="rollSizeKB">Size of start next file.</param>
        /// <param name="messageFormatter">Converter of message per line.</param>
        /// <param name="encoding">String encoding.</param>
        /// <param name="autoFlush">If true, call Flush on every write.</param>
        public static IDisposable LogToRollingFile(this IObservable<IList<TraceEvent>> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Func<TraceEvent, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new TraceEventSink(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

#endif

        // EventArgs

        /// <summary>
        /// Write to text file, file is rolled.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="fileNameSelector">Selector of output file name. DateTime is date of file open time, int is number sequence.</param>
        /// <param name="timestampPattern">Pattern of rolling identifier. DateTime is write time of message. If pattern is different roll new file.</param>
        /// <param name="rollSizeKB">Size of start next file.</param>
        /// <param name="messageFormatter">Converter of message per line.</param>
        /// <param name="encoding">String encoding.</param>
        /// <param name="autoFlush">If true, call Flush on every write.</param>
        public static IDisposable LogToRollingFile(this IObservable<EventWrittenEventArgs> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Func<EventWrittenEventArgs, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new EventWrittenEventArgsSink(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        /// <summary>
        /// Write to text file, file is rolled.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="fileNameSelector">Selector of output file name. DateTime is date of file open time, int is number sequence.</param>
        /// <param name="timestampPattern">Pattern of rolling identifier. DateTime is write time of message. If pattern is different roll new file.</param>
        /// <param name="rollSizeKB">Size of start next file.</param>
        /// <param name="messageFormatter">Converter of message per line.</param>
        /// <param name="encoding">String encoding.</param>
        /// <param name="autoFlush">If true, call Flush on every write.</param>
        public static IDisposable LogToRollingFile(this IObservable<IList<EventWrittenEventArgs>> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Func<EventWrittenEventArgs, string> messageFormatter, Encoding encoding, bool autoFlush)
        {
            var sink = new EventWrittenEventArgsSink(fileNameSelector, timestampPattern, rollSizeKB, messageFormatter, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        // string

        /// <summary>
        /// Write to text file, file is rolled.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="fileNameSelector">Selector of output file name. DateTime is date of file open time, int is number sequence.</param>
        /// <param name="timestampPattern">Pattern of rolling identifier. DateTime is write time of message. If pattern is different roll new file.</param>
        /// <param name="rollSizeKB">Size of start next file.</param>
        /// <param name="encoding">String encoding.</param>
        /// <param name="autoFlush">If true, call Flush on every write.</param>
        public static IDisposable LogToRollingFile(this IObservable<string> source, Func<DateTime, int, string> fileNameSelector, Func<DateTime, string> timestampPattern, int rollSizeKB, Encoding encoding, bool autoFlush)
        {
            var sink = new StringSink(fileNameSelector, timestampPattern, rollSizeKB, encoding, autoFlush);
            var subscription = source.Subscribe(sink);
            return sink.CreateLinkedDisposable(subscription);
        }

        /// <summary>
        /// Write to text file, file is rolled.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="fileNameSelector">Selector of output file name. DateTime is date of file open time, int is number sequence.</param>
        /// <param name="timestampPattern">Pattern of rolling identifier. DateTime is write time of message. If pattern is different roll new file.</param>
        /// <param name="rollSizeKB">Size of start next file.</param>
        /// <param name="encoding">String encoding.</param>
        /// <param name="autoFlush">If true, call Flush on every write.</param>
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
            readonly Action<T> onNextCore;

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
                this.onNextCore = OnNextCore;

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

                            string[] safe;
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
                OnNextCore(value);
            }

            public override void OnNext(IList<T> value)
            {
                CheckFileRolling();
                value.FastForEach(onNextCore);
            }

            void OnNextCore(T value)
            {
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

            public override void Dispose()
            {
                asyncFileWriter?.Finalize();
            }
        }

#if TRACE_EVENT

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

#endif

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