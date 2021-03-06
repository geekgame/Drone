﻿#define DEBUG

using Drone.Properties;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Drone.Core.Networking
{
    public class Sock
    {
        #region Public Fields

        public static volatile Socket mySock;

        public static volatile string response;

        #endregion Public Fields

        #region Public Methods

        public static bool Connect(EndPoint remoteEP, Socket client)
        {
            Console.WriteLine(@"Connexion");
            try
            {
                client.BeginConnect(remoteEP, ConnectCallback, client);
            }
            catch (Exception)
            {
                Console.WriteLine(Resources.Sock_Connect_Unable_to_connect_to_server);
                return false;
            }
            return true;

            // connectDone.WaitOne();
        }

        /// <exception cref="SocketException">Une erreur s'est produite lors de la résolution de <paramref name="hostName" />. </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     La longueur de <paramref name="hostName" /> est supérieure à 255
        ///     caractères.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="hostName" /> a la valeur null. </exception>
        /// <exception cref="IOException">Une erreur d'E/S s'est produite. </exception>
        public static void init(string ServerIp, int ServerPort)
        {
            Console.WriteLine(Resources.Sock_init_Searching_IP_Address);
            Console.WriteLine(Resources.Sock_init_DNS_Resolving);
#pragma warning disable 618
            var iph = Dns.Resolve(Dns.GetHostName());
#pragma warning restore 618

            var ret = IPAddress.None;
            foreach (var ip in iph.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ret = ip;
                    Console.WriteLine(@"OK");
                    break;
                }
            }

            Console.WriteLine(@"IP = " + ret);

            var ipServer = IPAddress.Parse(ServerIp);
            var remoteEp = new IPEndPoint(ipServer, ServerPort);
            var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            mySock = sender;

            Connect(remoteEp, sender);
        }

        /// <summary>
        ///     La fonction attend et renvoie la valeur du socket lu
        ///     A utiliser uniquement dans un thread (car fonction bloquante)
        /// </summary>
        /// <returns>REnvoie le texte lu</returns>
        public static string Listen()
        {
            return string.Empty;
        }

        /// <summary>
        ///     The receive.
        /// </summary>
        /// <param name="client">
        ///     The client.
        /// </param>
        public static void Receive(Socket client)
        {
            try
            {
                var state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                Console.WriteLine(@"ready to receive");
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.HResult);
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     The send.
        /// </summary>
        /// <param name="client">
        ///     The client.
        /// </param>
        /// <param name="data">
        ///     The data.
        /// </param>
        public static void Send(Socket client, string data)
        {
            var byteData = Encoding.ASCII.GetBytes(data);

            try
            {
                client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, SendCallback, client);
            }
            catch (SocketException socketException)
            {
                Console.WriteLine(socketException.Message);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;

                client.EndConnect(ar);

                Console.WriteLine(@"Socket connecté " + client.RemoteEndPoint);

                // connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject) ar.AsyncState;
                var client = state.workSocket;

                var bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);

                    foreach (char a in state.buffer)
                    {
                        if (a != 0)
                        {
                            Console.Write(a);
                        }
                    }

                    Perform.PerformSock(Encoding.ASCII.GetString(state.buffer));

                    for (var i = 0; i <= 255; i++)
                    {
                        state.buffer[i] = new byte();
                    }

                    Console.WriteLine(string.Empty);
                }
                else
                {
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("server disconnected. Last msg : \"" + response + "\"");
                        Console.ForegroundColor = ConsoleColor.White;

                        // Send(mySock, "a");
                        // receiveDone.Set();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;

                var bytesSent = client.EndSend(ar);
                Console.WriteLine("Envoyé " + bytesSent + " bytes au serveur");

                // sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion Private Methods
    }

    public class StateObject
    {
        //public const int BufferSize = 256;

        #region Public Fields

        public const int BufferSize = 64;

        public byte[] buffer = new byte[BufferSize];

        public StringBuilder sb = new StringBuilder();

        public Socket workSocket;

        #endregion Public Fields
    }

    #region SOCK2

    // TODO : REFACTOR ALL SOCKETS IN THE CODE
    // TODO : ================================

    public class Sock2
    {
        #region Public Constructors

        public Sock2(string serverIp, int serverPort)
        {
            serverIP = serverIp;
            this.serverPort = serverPort;
            /*
            #pragma warning disable 618
                        //var iph = Dns.Resolve(Dns.GetHostName());
            #pragma warning restore 618
            */
            var iph = Dns.GetHostEntry(Dns.GetHostName());

            var ret = IPAddress.None;

            foreach (var ip in iph.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ret = ip;
                    Console.WriteLine(@"OK");
                    break;
                }
            }
            Console.WriteLine(@"IP = " + ret);

            var ipServer = IPAddress.Parse(serverIp);
            var remoteEp = new IPEndPoint(ipServer, serverPort);
            var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            mySock = sender;

            Connect(remoteEp, sender);
        }

        #endregion Public Constructors

        #region Public Fields

        public Socket mySock;
        public string response;

        #endregion Public Fields

        #region Private Fields

        private string serverIP;
        private int serverPort;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        ///     La fonction attend et renvoie la valeur du socket lu
        ///     A utiliser uniquement dans un thread (car fonction bloquante)
        /// </summary>
        /// <returns>REnvoie le texte lu</returns>
        public static string Listen()
        {
            return string.Empty;
        }

        public bool Connect(EndPoint remoteEP, Socket client)
        {
            Console.WriteLine(@"Connexion V2");
            try
            {
                client.BeginConnect(remoteEP, ConnectCallback, client);
            }
            catch (Exception)
            {
                Console.WriteLine(@"Impossible de se connecter au serveur V2");
                return false;
            }
            return true;
        }

        /// <summary>
        ///     The receive.
        /// </summary>
        /// <param name="client">
        ///     The client.
        /// </param>
        public void Receive(Socket client)
        {
            try
            {
                var state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                Console.WriteLine(@"ready to receive");
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.HResult);
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     The send.
        /// </summary>
        /// <param name="client">
        ///     The client.
        /// </param>
        /// <param name="data">
        ///     The data.
        /// </param>
        public void Send(Socket client, string data)
        {
            var byteData = Encoding.ASCII.GetBytes(data);

            try
            {
                client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, SendCallback, client);
            }
            catch (SocketException socketException)
            {
                Console.WriteLine(socketException.Message);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;

                client.EndConnect(ar);

                Console.WriteLine(@"Socket connecté " + client.RemoteEndPoint);

                // connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject) ar.AsyncState;
                var client = state.workSocket;

                var bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);

                    foreach (char a in state.buffer)
                    {
                        if (a != 0)
                        {
                            Console.Write(a);
                        }
                    }

                    Perform.PerformSock(Encoding.ASCII.GetString(state.buffer));

                    for (var i = 0; i <= 255; i++)
                    {
                        state.buffer[i] = new byte();
                    }

                    Console.WriteLine(string.Empty);
                }
                else
                {
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("server disconnected. Last msg : \"" + response + "\"");
                        Console.ForegroundColor = ConsoleColor.White;

                        // Send(mySock, "a");
                        // receiveDone.Set();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;

                var bytesSent = client.EndSend(ar);
                Console.WriteLine("Envoyé " + bytesSent + " bytes au serveur");

                // sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion Private Methods
    }
}

#endregion SOCK2