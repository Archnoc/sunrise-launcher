using Qml.Net;
using Qml.Net.Runtimes;
using System;
using System.IO;

namespace sunrise_launcher
{
    public class Program
    {
        public static QGuiApplication App;

        static int Main(string[] args)
        {
            using (var fileout = new FileStream("./log.txt", FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(fileout))
            {
                Console.SetOut(writer);
                writer.AutoFlush = true;

                try
                {
                    RuntimeManager.DiscoverOrDownloadSuitableQtRuntime();
                    using (var app = new QGuiApplication(args))
                    {
                        App = app;
                        using (var engine = new QQmlApplicationEngine())
                        {
                            //register types
                            Qml.Net.Qml.RegisterType<ServerList>("sunrise", 1, 1);
                            Qml.Net.Qml.RegisterType<Server>("sunrise", 1, 1);

                            //load qml files
                            engine.Load("main.qml");

                            return app.Exec();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception in main: {0}", ex.Message);
                    return 1;
                }
            }
        }
    }
}
