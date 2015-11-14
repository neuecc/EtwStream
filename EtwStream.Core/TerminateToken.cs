using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtwStream
{
    public static class TerminateSystem
    {
        // needs to run sequential.
        static List<Action> UnloadingList = new List<Action>();

        static TerminateSystem()
        {
            AppDomain.CurrentDomain.DomainUnload += (_, __) => UnloadingList.ForEach(x => x());
            AppDomain.CurrentDomain.ProcessExit += (_, __) => UnloadingList.ForEach(x => x());
            AppDomain.CurrentDomain.UnhandledException += (_, __) => UnloadingList.ForEach(x => x());
        }

        public static CancellationToken GetToken()
        {
            var token = new CancellationTokenSource();
            UnloadingList.Add(() => token.Cancel());
            return token.Token;
        }

        public static void RegisterWaitForLogBufferComplete(Task[] asyncSubscriptions, int millisecondsTimeout)
        {
            UnloadingList.Add(() => Task.WaitAll(asyncSubscriptions, millisecondsTimeout));
        }
    }
}