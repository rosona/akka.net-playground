using System;
using System.Net;
using System.Net.Sockets;
using Akka.Actor;
using AkkaTest.Shared;

namespace AkkaTest.Remote.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GetLocalIPAddress());
            var system = ActorSystem.Create("Mylient");
            ActorSelection server = system.ActorSelection("akka.tcp://MyServer@localhost:8081/user/ChatServer");
            while (true)
            {
                var message = Console.ReadLine();
                server.Tell(new Ping(message), system.ActorOf(Props.Create<GreetingActor>()));
            }
        }

        static string GetLocalIPAddress()
        {
            string localIp = null;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                var endPoint = (IPEndPoint) socket.LocalEndPoint;
                if (endPoint != null) localIp = endPoint.Address.ToString();
            }

            return localIp;
        }
    }
}