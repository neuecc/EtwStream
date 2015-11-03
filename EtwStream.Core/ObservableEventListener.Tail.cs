using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.IO;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtwStream
{
    public partial class ObservableEventListener
    {
        /// <summary>
        /// Observe String-Line from file like tail -f.
        /// </summary>
        /// <param name="encoding">If null, use Encoding.UTF8</param>
        public static IObservable<string> FromFileTail(string filePath, bool readFromFirstLine = false, Encoding encoding = null)
        {
            return Observable.Defer(() =>
            {
                encoding = encoding ?? Encoding.UTF8;

                var subject = new Subject<string>();

                var fi = new System.IO.FileInfo(filePath);

                var stream = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var streamReader = new StreamReader(stream, encoding);

                try
                {
                    var firstSource = new List<string>();
                    if (readFromFirstLine)
                    {
                        while (!streamReader.EndOfStream)
                        {
                            firstSource.Add(streamReader.ReadLine());
                        }
                    }
                    else
                    {
                        stream.Seek(fi.Length, SeekOrigin.Current);
                    }

                    var readingLock = new object();
                    var fsw = new FileSystemWatcher(fi.DirectoryName, fi.Name);
                    fsw.NotifyFilter = NotifyFilters.Size;
                    fsw.Changed += (sender, e) =>
                    {
                        try
                        {
                            lock (readingLock)
                            {
                                string s;
                                while ((s = streamReader.ReadLine()) != null)
                                {
                                    if (s != "")
                                    {
                                        subject.OnNext(s);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            subject.OnError(ex);
                        }
                    };
                    fsw.EnableRaisingEvents = true;

                    return firstSource.ToObservable().Concat(subject).Finally(() =>
                    {
                        fsw.EnableRaisingEvents = false;
                        fsw.Dispose();
                        streamReader.Dispose();
                        stream.Dispose();
                    });
                }
                catch
                {
                    streamReader?.Dispose();
                    stream?.Dispose();
                    throw;
                }
            });
        }
    }
}