using System;
using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
using AkkaTest.Shared;

namespace AkkaTest.Cluster.Receptionist
{
    class Program
    {
        private static void Main(string[] args)
        {
            StartUp(args.Length == 0 ? new[] {"2551", "2552"} : args);
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        static void StartUp(string[] ports)
        {
            var akkaConfig = ConfigurationFactory.ParseString(@"
                akka {
                    extensions = [""Akka.Cluster.Tools.Client.ClusterClientReceptionistExtensionProvider, Akka.Cluster.Tools""]
                    actor {
                        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                    }
                    remote {
                        log-remote-lifecycle-events = DEBUG
                        dot-netty.tcp {
                            hostname = ""127.0.0.1""
                            port = 0
                        }
                    }
                    cluster {
                        seed-nodes = [
                            ""akka.tcp://ClusterSystem@127.0.0.1:2551"",
                        ]
                        auto-down-unreachable-after = 30s
                        roles = [server]
                    }
                }");
            foreach (var port in ports)
            {
                var config = ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(akkaConfig);

                var system = ActorSystem.Create("ClusterSystem", config);
                var greeting = system.ActorOf(Props.Create<GreetingActor>(), "greeting");
                ClusterClientReceptionist.Get(system).RegisterService(greeting);
            }
        }
    }
}