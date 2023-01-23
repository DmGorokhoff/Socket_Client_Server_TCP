using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Server_TCP
{
    class Program
    {
        private static Thread _serverThread;
        static void Main(string[] args)
        {
            _serverThread = new Thread(StartServer);
            _serverThread.IsBackground = true;
            _serverThread.Start();
                        
            Console.ReadLine();
        }
        private static void StartServer()
        {
            const string serverHost = "localhost";
            //const string ip = "127.0.0.1";
            //const int port = 8080;
            const int port = 8080;

            IPHostEntry ipHost = Dns.GetHostEntry(serverHost);
            IPAddress ipAddress = ipHost.AddressList[0];
            Console.WriteLine($"ipHost: {ipHost};\t ipAddress: {ipAddress}");

            IPEndPoint tcpEndPoint = new IPEndPoint(ipAddress, port);
            //var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Socket tcpSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(tcpEndPoint);
            tcpSocket.Listen(2);
            //Socket listener = tcpSocket.Accept();
            
            try
            {
                while (true) 
                {
                    Socket listener = tcpSocket.Accept();
                    AddClient.NewClient(listener);
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                
            }
            finally
            {
                tcpSocket.Shutdown(SocketShutdown.Both);
                tcpSocket.Close();
            }
        }
    }
}
