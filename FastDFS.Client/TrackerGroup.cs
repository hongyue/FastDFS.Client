using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FastDFS.Client
{
    public class TrackerGroup
    {
        public int tracker_server_index;
        public IList<IPEndPoint> tracker_servers;
        protected Integer lock;

        /**
         * Constructor
         *
         * @param tracker_servers tracker servers
         */
        public TrackerGroup(IList<IPEndPoint> tracker_servers)
        {
            this.tracker_servers = tracker_servers;
            this.lock = new Integer(0);
            this.tracker_server_index = 0;
        }

        /**
         * return connected tracker server
         *
         * @return connected tracker server, null for fail
         */
        public TrackerServer getConnection(int serverIndex)
        {
            Socket sock = new Socket();
            sock.setReuseAddress(true);
            sock.setSoTimeout(ClientGlobal.g_network_timeout);
            sock.connect(this.tracker_servers[serverIndex], ClientGlobal.g_connect_timeout);
            return new TrackerServer(sock, this.tracker_servers[serverIndex]);
        }

        /**
         * return connected tracker server
         *
         * @return connected tracker server, null for fail
         */
        public TrackerServer getConnection()
        {
            int current_index;

            synchronized(this.lock) {
                this.tracker_server_index++;
                if (this.tracker_server_index >= this.tracker_servers.length)
                {
                    this.tracker_server_index = 0;
                }

                current_index = this.tracker_server_index;
            }

            try
            {
                return this.getConnection(current_index);
            }
            catch (IOException ex)
            {
                System.err.println("connect to server " +
                                   this.tracker_servers[current_index].getAddress().getHostAddress() + ":" +
                                   this.tracker_servers[current_index].getPort() + " fail");
                ex.printStackTrace(System.err);
            }

            for (int i = 0; i < this.tracker_servers.length; i++)
            {
                if (i == current_index)
                {
                    continue;
                }

                try
                {
                    TrackerServer trackerServer = this.getConnection(i);

                    synchronized(this.lock)
                    {
                        if (this.tracker_server_index == current_index)
                        {
                            this.tracker_server_index = i;
                        }
                    }

                    return trackerServer;
                }
                catch (IOException ex)
                {
                    System.err.println("connect to server " + this.tracker_servers[i].getAddress().getHostAddress() +
                                       ":" + this.tracker_servers[i].getPort() + " fail");
                    ex.printStackTrace(System.err);
                }
            }

            return null;
        }

        public Object clone()
        {
            InetSocketAddress[] trackerServers = new InetSocketAddress[this.tracker_servers.length];
            for (int i = 0; i < trackerServers.length; i++)
            {
                trackerServers[i] = new InetSocketAddress(this.tracker_servers[i].getAddress().getHostAddress(),
                        this.tracker_servers[i].getPort());
            }

            return new TrackerGroup(trackerServers);
        }
    }
}