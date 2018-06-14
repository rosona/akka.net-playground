using System;
using Akka.Actor;
using Akka.Configuration;
using AkkaTest.Shared;

namespace AkkaTest.Deployer
{
    class Program
    {
        class HelloActor : ReceiveActor
        {
            private IActorRef _remoteActor;
            private int _helloCounter;
            private ICancelable _helloTask;

            public HelloActor(IActorRef remoteActor)
            {
                _remoteActor = remoteActor;
                Context.Watch(_remoteActor);

                Receive<Ping>(hello => { Console.WriteLine("Received {1} from {0}", Sender, hello); });

                Receive<Pong>(sayHello => { _remoteActor.Tell("hello" + _helloCounter++); });

                Receive<Terminated>(terminated =>
                {
                    Console.WriteLine(terminated.ActorRef);
                    Console.WriteLine("Was address terminated? {0}", terminated.AddressTerminated);
                    _helloTask.Cancel();
                });
            }

            protected override void PreStart()
            {
                _helloTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1), Context.Self, "hi, hello!", ActorRefs.NoSender);
            }

            protected override void PostStop()
            {
                _helloTask.Cancel();
            }
        }

        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("Deployer", ConfigurationFactory.ParseString(@"
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
                            /remote-echo {
                                router = round-robin-pool
                                nr-of-instances = 5
                                remote = ""akka.tcp://DeployTarget@localhost:8080""
                            }
                        }
                    }
                    remote {
                        dot-netty.tcp {
                            port = 8090
                            hostname = localhost
                        }
                    }
                }")))
            {
                var remoteAddress = Address.Parse("akka.tcp://DeployTarget@localhost:8080");
                var remoteEcho1 = system.ActorOf(Props.Create(() => new GreetingActor()), "remote-echo");
                var remoteEcho2 = system.ActorOf(Props.Create(() => new GreetingActor()).WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))), "code-remote-echo");

                system.ActorOf(Props.Create(() => new HelloActor(remoteEcho1)));
                system.ActorOf(Props.Create(() => new HelloActor(remoteEcho2)));

                // system.ActorSelection("/user/remote-echo").Tell("hi from selection!");

                Console.ReadKey();
            }
        }
    }
}