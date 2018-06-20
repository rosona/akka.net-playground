using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using AkkaTest.Shared;

namespace AkkaTest.Deployer
{
    public class ReplyActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            Console.WriteLine("Message from {0} - {1}", Sender, message);
        }
    }

    class Program
    {
        private static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    log-config-on-start = on
                    stdout-loglevel = DEBUG
                    loglevel = DEBUG
                    actor {
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                        
                        debug {  
                          receive = on 
                          autoreceive = on
                          lifecycle = on
                          event-stream = on
                          unhandled = on
                        }
                
                        deployment {
                            /localactor {
                                router = round-robin-pool
                                nr-of-instances = 5
                            }
                            /remoteactor {
                                router = round-robin-pool
                                nr-of-instances = 5
                                remote = ""akka.tcp://system2@localhost:8888""
                            }
                        }
                    }
                    remote {
                        dot-netty.tcp {
                            port = 8889
                            hostname = localhost
                        }
                    }
                }
            ");
            using (var system = ActorSystem.Create("system1", config))
            {
                var reply = system.ActorOf<ReplyActor>("reply");
                //create a local group router (see config)
                var local = system.ActorOf(Props.Create(() => new SomeActor("hello", 123)).WithRouter(FromConfig.Instance), "localactor");

                //create a remote deployed actor
                var remote = system.ActorOf(Props.Create(() => new SomeActor(null, 123)).WithRouter(FromConfig.Instance), "remoteactor");

                //these messages should reach the workers via the routed local ref
                local.Tell("Local message 1", reply);
                local.Tell("Local message 2", reply);
                local.Tell("Local message 3", reply);
                local.Tell("Local message 4", reply);
                local.Tell("Local message 5", reply);
                local.Tell("Local message 6", reply);
                local.Tell("Local message 7", reply);
                local.Tell("Local message 8", reply);
                local.Tell("Local message 9", reply);
                local.Tell("Local message 0", reply);

                //this should reach the remote deployed ref
                remote.Tell("Remote message 1", reply);
                remote.Tell("Remote message 2", reply);
                remote.Tell("Remote message 3", reply);
                remote.Tell("Remote message 4", reply);
                remote.Tell("Remote message 5", reply);
                remote.Tell("Remote message 6", reply);
                remote.Tell("Remote message 7", reply);
                remote.Tell("Remote message 8", reply);
                remote.Tell("Remote message 9", reply);
                remote.Tell("Remote message 0", reply);

                Console.ReadLine();
            }
        }

    }}