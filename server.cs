using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleMUD
{
    class Server
    {
        const int PortNumber = 4000;
        const int BacklogSize = 20;

        static void Main(string[] args)
        {
            Socket server = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
            server.Listen(BacklogSize);
            while (true)
            {
                Socket conn = server.Accept();
                new Connection(conn);
            }
        }
    }

    class Connection
    {
        static object BigLock = new object();
        Socket socket;
        public StreamReader Reader;
        public StreamWriter Writer;
        static ArrayList connections = new ArrayList();

        public Connection(Socket socket)
        {
            this.socket = socket;
            Reader = new StreamReader(new NetworkStream(socket, false));
            Writer = new StreamWriter(new NetworkStream(socket, true));
            new Thread(ClientLoop).Start();
        }

        void ClientLoop()
        {
            try
            {
                lock (BigLock)
                {
                    OnConnect();
                }
                while (true)
                {
                    lock (BigLock)
                    {
                        foreach (Connection conn in connections)
                        {
                            conn.Writer.Flush();
                        }
                    }
                    string line = Reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    lock (BigLock)
                    {
                        ProcessLine(line);
                    }
                }
            }
            finally
            {
                lock (BigLock)
                {
                    socket.Close();
                    OnDisconnect();
                }
            }
        }

        void OnConnect()
        {
            Writer.WriteLine("Welcome!");
            connections.Add(this);
        }

        void OnDisconnect()
        {
            connections.Remove(this);
        }

        void ProcessLine(string line)
        {
            foreach (Connection conn in connections)
            {
                conn.Writer.WriteLine("Someone says, '" + line.Trim() + "'");
                conn.Writer.WriteLine("It is \033[31mnot\033[39m intelligent to use \033[32mhardcoded ANSI\033[39m codes!");
            }
        }
    }
}