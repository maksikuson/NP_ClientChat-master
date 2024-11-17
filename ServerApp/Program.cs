using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerApp
{
    public class ChatServer
    {
        const short port = 4040;
        const string JOIN_CMD = "$<join>";
        const string LEAVE_CMD = "$<leave>";
        UdpClient server;
        IPEndPoint clientEndPoint = null;
        List<IPEndPoint> members;

        public ChatServer()
        {
            server = new UdpClient(port);
            members = new List<IPEndPoint>();
        }

        public void Start()
        {
            while (true)
            {
                byte[] data = server.Receive(ref clientEndPoint);
                string message = Encoding.UTF8.GetString(data);
                Console.WriteLine($"Message: {message} from {clientEndPoint} at {DateTime.Now}");

                if (message.StartsWith(JOIN_CMD))
                {
                    string userName = message.Substring(JOIN_CMD.Length);
                    AddMember(clientEndPoint, userName);
                }
                else if (message == LEAVE_CMD)
                {
                    RemoveMember(clientEndPoint);
                }
                else
                {
                    SendToAll(data);
                }
            }
        }

        private void AddMember(IPEndPoint member, string userName)
        {
            members.Add(member);
            string welcomeMessage = $"{userName} has joined the chat!";
            SendToAll(Encoding.UTF8.GetBytes(welcomeMessage));
            Console.WriteLine("Member was added!");
        }

        private void RemoveMember(IPEndPoint member)
        {
            members.Remove(member);
            string leaveMessage = $"{member.Address}:{member.Port} has left the chat.";
            SendToAll(Encoding.UTF8.GetBytes(leaveMessage));
            Console.WriteLine("Member has left.");
        }

        private void SendToAll(byte[] data)
        {
            foreach (var member in members)
            {
                server.SendAsync(data, data.Length, member);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            ChatServer server = new ChatServer();
            server.Start();
        }
    }
}