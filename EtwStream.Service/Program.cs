using Topshelf;

namespace EtwStream.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                // Automate recovery
                x.EnableServiceRecovery(recover => { recover.RestartService(0); });

                // Reference to Logic Class
                x.Service<OutOfProcessService>(s =>
                {
                    s.ConstructUsing(name => new OutOfProcessService());
                    s.WhenStarted(sc => sc.Start());
                    s.WhenStopped(sc => sc.Stop());
                });

                // Service Start mode
                x.StartAutomaticallyDelayed();

                // Service RunAs
                x.RunAsLocalSystem();

                // Service information
                x.SetServiceName("EtwStream.Service");
                x.SetDisplayName("EtwStream Service");
                x.SetDescription("EtwStream Out-of-Process Service, trace event and output to anywhere.");
            });
        }
    }
}