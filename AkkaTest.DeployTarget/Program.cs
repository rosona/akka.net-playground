﻿using System;
using Akka.Actor;
using Akka.Configuration;

namespace AkkaTest.DeployTarget
{
    internal class Program
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
                    }
                    remote {
                        dot-netty.tcp {
                            port = 8888
                            hostname = localhost
                        }
                    }
                }
            ");
            //testing connectivity
            using (ActorSystem.Create("system2", config))
            {
                Console.ReadLine();
            }
        }
    }
}