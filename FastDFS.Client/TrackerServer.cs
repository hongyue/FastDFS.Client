using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FastDFS.Client
{
    public class TrackerServer
    {
        protected Socket sock;
        protected InetSocketAddress inetSockAddr;

        /**
         * Constructor
         *
         * @param sock         Socket of server
         * @param inetSockAddr the server info
         */
        public TrackerServer(Socket sock, InetSocketAddress inetSockAddr)
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
        public InetSocketAddress getInetSocketAddress()
        {
            return this.inetSockAddr;
        }

        public OutputStream getOutputStream()
        {
            return this.sock.getOutputStream();
        }

        public InputStream getInputStream()
        {
            return this.sock.getInputStream();
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

        public boolean isConnected()
        {
            boolean isConnected = false;
            if (sock != null)
            {
                if (sock.isConnected())
                {
                    isConnected = true;
                }
            }

            return isConnected;
        }

        public boolean isAvaliable()
        {
            if (isConnected())
            {
                if (sock.getPort() == 0)
                {
                    return false;
                }

                if (sock.getInetAddress() == null)
                {
                    return false;
                }

                if (sock.getRemoteSocketAddress() == null)
                {
                    return false;
                }

                if (sock.isInputShutdown())
                {
                    return false;
                }

                if (sock.isOutputShutdown())
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}