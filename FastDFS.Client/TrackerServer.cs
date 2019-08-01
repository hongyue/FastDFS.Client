using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FastDFS.Client
{
    public class TrackerServer
    {
        protected Socket sock;
        protected IPEndPoint inetSockAddr;

        /**
         * Constructor
         *
         * @param sock         Socket of server
         * @param inetSockAddr the server info
         */
        public TrackerServer(Socket sock, IPEndPoint inetSockAddr)
        {
            this.sock = sock;
            this.inetSockAddr = inetSockAddr;
        }

        /**
         * get the connected socket
         *
         * @return the socket
         */
        public Socket getSocket()
        {
            if (this.sock == null)
            {
                this.sock = ClientGlobal.getSocket(this.inetSockAddr);
            }

            return this.sock;
        }

        /**
         * get the server info
         *
         * @return the server info
         */
        public IPEndPoint getInetSocketAddress()
        {
            return this.inetSockAddr;
        }

        public Socket getOutputStream()
        {
            return this.sock;
        }

        public Socket getInputStream()
        {
            return this.sock;
        }

        public void close()
        {
            if (this.sock != null)
            {
                try
                {
                    ProtoCommon.closeSocket(this.sock);
                }
                finally
                {
                    this.sock = null;
                }
            }
        }

        protected void finalize()
        {
            this.close();
        }

        public bool isConnected()
        {
            bool isConnected = false;
            if (sock != null)
            {
                if (sock.Connected)
                {
                    isConnected = true;
                }
            }

            return isConnected;
        }

        public bool isAvaliable()
        {
            if (isConnected())
            {
                //if (sock.RemoteEndPoin() == 0)
                //{
                //    return false;
                //}

                //if (sock.getInetAddress() == null)
                //{
                //    return false;
                //}

                //if (sock.getRemoteSocketAddress() == null)
                //{
                //    return false;
                //}

                //if (sock.isInputShutdown())
                //{
                //    return false;
                //}

                //if (sock.isOutputShutdown())
                //{
                //    return false;
                //}


                return true;
            }

            return false;
        }
    }
}