using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client_TCP
{
    class Program
    {  
        static void Main(string[] args)
        {
            HandlerCl_BL.StartClient();            
            Console.ReadLine();
        }        
    }
}
