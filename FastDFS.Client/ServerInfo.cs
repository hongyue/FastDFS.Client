using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FastDFS.Client
{
    public class ServerInfo
    {
        protected String ip_addr;
        protected int port;

        /**
         * Constructor
         *
         * @param ip_addr address of the server
         * @param port    the port of the server
         */
        public ServerInfo(String ip_addr, int port)
        {
            this.ip_addr = ip_addr;
            this.port = port;
        }

        /**
         * return the ip address
         *
         * @return the ip address
         */
        public String getIpAddr()
        {
            return this.ip_addr;
        }

        /**
         * return the port of the server
         *
         * @return the port of the server
         */
        public int getPort()
        {
            return this.port;
        }

        /**
         * connect to server
         *
         * @return connected Socket object
         */
        public Socket connect()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip_addr), port);
            var sock = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = ClientGlobal.g_network_timeout
            };
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sock.Connect(ipEndPoint);

            return sock;
        }
    }
}