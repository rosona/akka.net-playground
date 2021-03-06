﻿using System;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using AkkaTest.Shared;

namespace AkkaTest.Cluster.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                        deployment {
                            /greeting {
                                router = round-robin-pool # routing strategy
                                nr-of-instances = 1 # max number of total routees
                                cluster {
                                    enabled = on
                                    allow-local-routees = off
                                    use-role = server
                                    max-nr-of-instances-per-node = 10
                                }
                            }
                            /testing {
                                router = round-robin-pool # routing strategy
                                nr-of-instances = 1 # max number of total routees
                                cluster {
                                    enabled = on
                                    allow-local-routees = off
                                    use-role = server
                                    max-nr-of-instances-per-node = 2
                                }
                            }
                        }
                        debug {  
                          receive = on 
                          autoreceive = on
                          lifecycle = on
                          event-stream = on
                          unhandled = on
                        }
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
                    }
                }");

            using (var system = ActorSystem.Create("ClusterSystem", config))
            {
                var greeting = system.ActorOf(Props.Create<GreetingActor>().WithRouter(FromConfig.Instance), "greeting");
                while (true)
                {
                    // var message = Console.ReadLine();
                    greeting.Tell(new Ping("test"), system.ActorOf(Props.Create<GreetingActor>()));
                    Thread.Sleep(1000);
                }
            }
        }
    }
}