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
        protected object _lock = new object();

        /**
         * Constructor
         *
         * @param tracker_servers tracker servers
         */
        public TrackerGroup(IList<IPEndPoint> tracker_servers)
        {
            this.tracker_servers = tracker_servers;
            this.tracker_server_index = 0;
        }

        /**
         * return connected tracker server
         *
         * @return connected tracker server, null for fail
         */
        public TrackerServer getConnection(int serverIndex)
        {
            Socket sock = new Socket(tracker_servers[serverIndex].AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = ClientGlobal.g_network_timeout
            };
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sock.Connect(tracker_servers[serverIndex]);
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

            lock(_lock) {
                this.tracker_server_index++;
                if (this.tracker_server_index >= this.tracker_servers.Count)
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
                //System.err.println("connect to server " +
                //                   this.tracker_servers[current_index].getAddress().getHostAddress() + ":" +
                //                   this.tracker_servers[current_index].getPort() + " fail");
                //ex.printStackTrace(System.err);
            }

            for (int i = 0; i < this.tracker_servers.Count; i++)
            {
                if (i == current_index)
                {
                    continue;
                }

                try
                {
                    TrackerServer trackerServer = this.getConnection(i);

                    lock(_lock)
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
                    //System.err.println("connect to server " + this.tracker_servers[i].getAddress().getHostAddress() +
                    //                   ":" + this.tracker_servers[i].getPort() + " fail");
                    //ex.printStackTrace(System.err);
                }
            }

            return null;
        }

        public Object clone()
        {
            var trackerServers = new IPEndPoint[this.tracker_servers.Count];
            for (int i = 0; i < trackerServers.Length; i++)
            {
                trackerServers[i] = new IPEndPoint(this.tracker_servers[i].Address,
                        this.tracker_servers[i].Port);
            }

            return new TrackerGroup(trackerServers);
        }
    }
}