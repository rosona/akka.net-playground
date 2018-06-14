using System;
using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
using AkkaTest.Shared;

namespace AkkaTest.Cluster.Receptionist.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
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
                        roles = [client]
                        client {
                            initial-contacts = [""akka.tcp://ClusterSystem@127.0.0.1:2551/system/receptionist""]
                        }
                    }
                }");

            using (var system = ActorSystem.Create("ClusterSystemClient", config))
            {
                system.Settings.InjectTopLevelFallback(ClusterClientReceptionist.DefaultConfig());
                var clusterClient =
                    system.ActorOf(ClusterClient.Props(ClusterClientSettings.Create(system)), "greeting");
                while (true)
                {
                    var message = Console.ReadLine();
                    clusterClient.Tell(new ClusterClient.Send("/user/greeting", new Ping(message)),
                        system.ActorOf(Props.Create<GreetingActor>()));
                }
            }
        }
    }
}