using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server_TCP
{    
     public static class AddClient
    {
        public static List<Client> clients = new List<Client>();
        public static Dictionary<int, string> newUser = new Dictionary<int, string>();
        public static int counter = 1;

        public static void NewClient(Socket sckt)
        {
            try
            {                      
                clients.Add(new Client(sckt));                
                Console.WriteLine($"New client connected: {sckt.RemoteEndPoint}\t{DateTime.Now}");
            }
            catch (Exception exp) { Console.WriteLine($"Error with addNewClient: {exp.Message}."); }
        }
        public static void UpdateAllChats()
        {
            try
            {
                int countUsers = clients.Count;
                for (int i = 0; i < countUsers; i++)
                {
                    clients[i].UpdateChat();
                }
            }
            catch (Exception exp) { Console.WriteLine($"Error with updateAlLChats: {exp.Message}."); }
        }
        public static void EndClient(Client client)
        {
            try
            {
                client.End();
                clients.Remove(client);                
                Console.WriteLine($"User [{client.UserName}] has been disconnected\t {DateTime.Now}.");
            }
            catch (Exception exp) { Console.WriteLine("Error with endClient: {0}.", exp.Message); }
        }

    }
    public class Client
    {
        private string _userName;
        private Socket _handler;
        private Thread _userThread;
        private static List<message> chat = new List<message>();

        public string UserName
        {
            get { return _userName; }
        }

        public Client(Socket socket)
        {
            _handler = socket;
            _userThread = new Thread(listner);
            _userThread.IsBackground = true;
            _userThread.Start();
        }
        public struct message
        {
            public string userName;
            public string data;
            public message(string name, string msg)
            {
                userName = name;
                data = msg;
            }
        }

        public void listner()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRec = _handler.Receive(buffer);
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);
                    HandleCommand(data);
                }
                catch
                {
                    AddClient.EndClient(this);
                    return;
                }
            }
        }
        public void HandleCommand(string data)
        {
            if (data.Contains("#clientname"))
            {                
                bool checkUser;                

                _userName = data.Split('&')[1];

                checkUser = AddClient.newUser.ContainsValue(_userName);
                if (checkUser) {_userName = "#invalidname";}
                else { AddClient.newUser.Add(AddClient.counter++, _userName); }
                Send(_userName);

                return;
            }
            if (data.Contains("#clientmessage"))
            {
                string message = data.Split('&')[1];
                AddMessage(_userName, message);
                return;
            }
            if (data.Contains("#getchat"))
            {
                UserGetChat(0);
                return;
            }
        }
        public void UpdateChat()
        {
            Send(GetChat());            
        }
        public void Send(string command)
        {
            try
            {
                if (command != "")
                {
                    int bytesSent = _handler.Send(Encoding.UTF8.GetBytes(command));
                    if (bytesSent > 0) Console.WriteLine("Success");
                }                
            }
            catch (Exception exp)

            { Console.WriteLine("Error with send command: {0}.", exp.Message); AddClient.EndClient(this); }
        }
        public void AddMessage(string userName, string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(msg)) return;
                int countMessages = chat.Count;                
                if (userName != "#invalidname")
                {
                    chat.Add(new message(userName, msg));
                    Console.WriteLine($"New message from [{userName}]\t {DateTime.Now}.");
                    UpdateChat();
                }                                
            }
            catch (Exception exp) { Console.WriteLine("Error with addMessage: {0}.", exp.Message); }
        }
        public string GetChat()
        {
            try
            {
                string data = "#updatechat&";
                int k=0;
                int countMessages = chat.Count;
                if (countMessages <= 0) return string.Empty;
                if (countMessages > 20) k = countMessages - 20;
                for (int i = k; i < countMessages; i++)
                {
                    data += string.Format($"{chat[i].userName}~{chat[i].data}|");                    
                }
                return data;
            }
            catch (Exception exp) { Console.WriteLine("Error with getChat: {0}", exp.Message); return string.Empty; }
        }
        public void UserGetChat(int k)
        {
            try
            {
                string data = "#updatechat&";
                
                int countMessages = chat.Count;
                if (countMessages > 0)                 
                for (int i = k; i < countMessages; i++)
                {
                    data += string.Format($"{chat[i].userName}~{chat[i].data}|");
                }
                Send(data);                
            }
            catch (Exception exp) { Console.WriteLine("Error with getChat: {0}", exp.Message); }
        }
        public void End()
        {
            try
            {
                _handler.Close();
                try
                {
                    _userThread.Abort();
                }
                catch { }
            }
            catch (Exception exp) { Console.WriteLine("Error with end: {0}.", exp.Message); }
        }
    }
}
