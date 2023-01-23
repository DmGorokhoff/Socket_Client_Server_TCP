using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client_TCP
{
    class HandlerCl_BL
    {
        public static string name;
        public static Socket tcpSocket;
        public static void StartClient()
        {
            const string serverHost = "localhost";
            //const string ip = "127.0.0.1";
            //const int port = 8080;
            const int port = 8080;

            IPHostEntry ipHost = Dns.GetHostEntry(serverHost);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint tcpEndPoint = new IPEndPoint(ipAddress, port);
            //var tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            tcpSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tcpSocket.Connect(tcpEndPoint);
            Console.WriteLine("Введите имя:");
            EnterName();
            
            while (tcpSocket.Connected)
            {
                try
                {
                    Console.WriteLine("Введите сообщение:");

                    string message = Console.ReadLine();
                    if (message == "")
                    {
                        ClearChat();
                        continue;
                    }
                    if (message.Contains("#getchat"))
                    {
                        byte[] data = Encoding.UTF8.GetBytes("#getchat&");
                        SendMess(data, tcpSocket);
                        message = "";
                    }
                    else
                    {
                        byte[] data = Encoding.UTF8.GetBytes("#clientmessage&" + message);
                        SendMess(data, tcpSocket);
                    }

                    var buffer = new byte[256];
                    int size = 0;
                    //var answer = new StringBuilder();
                    do
                    {
                        ClearChat();
                        size = tcpSocket.Receive(buffer);
                        //answer.Append(Encoding.UTF8.GetString(buffer, 0, size));
                        string answer = Encoding.UTF8.GetString(buffer, 0, size);
                        UpdateChat(answer);
                        
                    }
                    while (tcpSocket.Available > 0);

                    //Console.WriteLine(answer.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //tcpSocket.Shutdown(SocketShutdown.Both);
            //tcpSocket.Close();
        }
        static void UpdateChat(string data)
        {
            //#updatechat&userName~data|username~data

            string[] messages;
            messages = data.Split('&')[1].Split('|');
            int countMessages = messages.Length;
            if (countMessages <= 0) return;
            for (int i = 0; i < countMessages; i++)
            {
                try
                {
                    if (string.IsNullOrEmpty(messages[i])) continue;
                    Console.WriteLine($"[{messages[i].Split('~')[0]}]: {messages[i].Split('~')[1]}");
                }
                catch { continue; }
            }
        }
        static void ClearChat()
        {
            Console.Clear();
            Console.WriteLine($"Ваше учетное имя: [{name}]\n" +
                $"На экран выводится <=20 сообщений чата\n" +
                $"Все сообщения чата можно получить по команде: #getchat\n" +
                $"Выход из чата по закрытию окна (Х)\n" +
                $"Ваш чат:") ;
        }
        static void SendMess(byte[] dt, Socket tcpS)
        {
            try
            {
                tcpS.Send(dt);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
        static void EnterName()
        {
            string answer = "";
            while (name == null | name == "" | answer == "#invalidname")
            {
                try
                {   
                    name = Console.ReadLine();
                    if (name == "") 
                    {
                        Console.Clear();
                        Console.WriteLine("Введено пустое имя.");
                        continue; 
                    } 
                    byte[] data = Encoding.UTF8.GetBytes("#clientname&" + name);
                    SendMess(data, tcpSocket);

                    var buffer = new byte[256];
                    int size;
                    size = tcpSocket.Receive(buffer);
                    do
                    {

                        answer = Encoding.UTF8.GetString(buffer, 0, size);
                        if (answer == "#invalidname")
                        {
                            Console.WriteLine($"Имя [{name}] уже зарегистрировано в системе.\nВведите другое имя:");
                        }
                    }
                    while (tcpSocket.Available > 0);
                }
                catch (Exception) { }
            }
        }
    }
}
