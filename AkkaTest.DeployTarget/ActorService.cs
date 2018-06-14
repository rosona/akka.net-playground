using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Akka.Actor;

namespace AkkaTest.DeployTarget
{
    public class ActorService
    {
        protected ActorSystem ClusterSystem;
        public Task WhenTerminated => ClusterSystem.WhenTerminated;

        public bool Start()
        {
            var configuration = @"
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
                    }
                    remote {
                        dot-netty.tcp {
                            port = 8080
                            hostname = localhost
                        }
                    }
                }";
            ClusterSystem = ActorSystem.Create("DeployTarget", configuration);
            return true;
        }

        public Task Stop()
        {
            return CoordinatedShutdown.Get(ClusterSystem).Run();
        }
    }
}