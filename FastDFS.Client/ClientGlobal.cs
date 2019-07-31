using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace FastDFS.Client
{
    public class ClientGlobal
    {
        public static readonly string CONF_KEY_CONNECT_TIMEOUT = "connect_timeout";
        public static readonly string CONF_KEY_NETWORK_TIMEOUT = "network_timeout";
        public static readonly string CONF_KEY_CHARSET = "charset";
        public static readonly string CONF_KEY_HTTP_ANTI_STEAL_TOKEN = "http.anti_steal_token";
        public static readonly string CONF_KEY_HTTP_SECRET_KEY = "http.secret_key";
        public static readonly string CONF_KEY_HTTP_TRACKER_HTTP_PORT = "http.tracker_http_port";
        public static readonly string CONF_KEY_TRACKER_SERVER = "tracker_server";

        public static readonly int DEFAULT_CONNECT_TIMEOUT = 5; //second
        public static readonly int DEFAULT_NETWORK_TIMEOUT = 30; //second
        public static readonly string DEFAULT_CHARSET = "UTF-8";
        public static readonly bool DEFAULT_HTTP_ANTI_STEAL_TOKEN = false;
        public static readonly string DEFAULT_HTTP_SECRET_KEY = "FastDFS1234567890";
        public static readonly int DEFAULT_HTTP_TRACKER_HTTP_PORT = 80;

        public static int g_connect_timeout = DEFAULT_CONNECT_TIMEOUT * 1000; //millisecond
        public static int g_network_timeout = DEFAULT_NETWORK_TIMEOUT * 1000; //millisecond
        public static string g_charset = DEFAULT_CHARSET;
        public static bool g_anti_steal_token = DEFAULT_HTTP_ANTI_STEAL_TOKEN; //if anti-steal token
        public static string g_secret_key = DEFAULT_HTTP_SECRET_KEY; //generage token secret key
        public static int g_tracker_http_port = DEFAULT_HTTP_TRACKER_HTTP_PORT;

        public static TrackerGroup g_tracker_group;

        public ClientGlobal()
        {
        }

        public static void init(String conf_filename)
        {
            IniFileReader iniReader;
            string[] szTrackerServers;
            string[] parts;

            iniReader = new IniFileReader(conf_filename);

            g_connect_timeout = iniReader.getIntValue("connect_timeout", DEFAULT_CONNECT_TIMEOUT);
            if (g_connect_timeout < 0)
            {
                g_connect_timeout = DEFAULT_CONNECT_TIMEOUT;
            }

            g_connect_timeout *= 1000; //millisecond

            g_network_timeout = iniReader.getIntValue("network_timeout", DEFAULT_NETWORK_TIMEOUT);
            if (g_network_timeout < 0)
            {
                g_network_timeout = DEFAULT_NETWORK_TIMEOUT;
            }

            g_network_timeout *= 1000; //millisecond

            g_charset = iniReader.getStrValue("charset");
            if (string.IsNullOrEmpty(g_charset))
            {
                g_charset = "ISO8859-1";
            }

            szTrackerServers = iniReader.getValues("tracker_server");
            if (szTrackerServers == null)
            {
                throw new FastDfsException($"item \"tracker_server\" in {conf_filename} not found");
            }

            var tracker_servers = new IPEndPoint[szTrackerServers.Length];
            for (int i = 0; i < szTrackerServers.Length; i++)
            {
                parts = szTrackerServers[i].Split(':');
                if (parts.Length != 2)
                {
                    throw new FastDfsException(
                            "the value of item \"tracker_server\" is invalid, the correct format is host:port");
                }

                tracker_servers[i] = new IPEndPoint(IPAddress.Parse(parts[0]), int.Parse(parts[1]));
            }

            g_tracker_group = new TrackerGroup(tracker_servers);

            g_tracker_http_port = iniReader.getIntValue("http.tracker_http_port", 80);
            g_anti_steal_token = iniReader.getBoolValue("http.anti_steal_token", false);
            if (g_anti_steal_token)
            {
                g_secret_key = iniReader.getStrValue("http.secret_key");
            }
        }

        public static void initByTrackers(string trackerServers)
        {
            var list = new List<IPEndPoint>();
            var spr1 = ',';
            var spr2 = ':';
            var arr1 = trackerServers.Trim().Split(spr1);
            foreach (var addrStr in arr1)
            {
                var arr2 = addrStr.Trim().Split(spr2);
                var host = arr2[0].Trim();
                var port = int.Parse(arr2[1].Trim());
                list.Add(new IPEndPoint(IPAddress.Parse(host), port));
            }

            g_tracker_group = new TrackerGroup(list);
        }

        public static Socket getSocket(string ip_addr, int port)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip_addr), port);
            var sock = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = g_network_timeout
            };
            sock.Connect(ipEndPoint);

            return sock;
        }

        public static Socket getSocket(IPEndPoint addr)
        {
            var sock = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = g_network_timeout
            };
            sock.Connect(addr);

            return sock;
        }

        public static int getG_connect_timeout()
        {
            return g_connect_timeout;
        }

        public static void setG_connect_timeout(int connect_timeout)
        {
            ClientGlobal.g_connect_timeout = connect_timeout;
        }

        public static int getG_network_timeout()
        {
            return g_network_timeout;
        }

        public static void setG_network_timeout(int network_timeout)
        {
            ClientGlobal.g_network_timeout = network_timeout;
        }

        public static String getG_charset()
        {
            return g_charset;
        }

        public static void setG_charset(String charset)
        {
            ClientGlobal.g_charset = charset;
        }

        public static int getG_tracker_http_port()
        {
            return g_tracker_http_port;
        }

        public static void setG_tracker_http_port(int tracker_http_port)
        {
            ClientGlobal.g_tracker_http_port = tracker_http_port;
        }

        public static bool getG_anti_steal_token()
        {
            return g_anti_steal_token;
        }

        public static bool isG_anti_steal_token()
        {
            return g_anti_steal_token;
        }

        public static void setG_anti_steal_token(bool anti_steal_token)
        {
            ClientGlobal.g_anti_steal_token = anti_steal_token;
        }

        public static String getG_secret_key()
        {
            return g_secret_key;
        }

        public static void setG_secret_key(String secret_key)
        {
            ClientGlobal.g_secret_key = secret_key;
        }

        public static TrackerGroup getG_tracker_group()
        {
            return g_tracker_group;
        }

        public static void setG_tracker_group(TrackerGroup tracker_group)
        {
            ClientGlobal.g_tracker_group = tracker_group;
        }

        public static string configInfo()
        {
            var trackerServers = "";
            if (g_tracker_group != null)
            {
                var trackerAddresses = g_tracker_group.tracker_servers;
                foreach (var inetSocketAddress in trackerAddresses)
                {
                    if (trackerServers.Length > 0)
                        trackerServers += ",";

                    trackerServers += inetSocketAddress.ToString();
                }
            }

            return "{"
                   + "\n  g_connect_timeout(ms) = " + g_connect_timeout
                   + "\n  g_network_timeout(ms) = " + g_network_timeout
                   + "\n  g_charset = " + g_charset
                   + "\n  g_anti_steal_token = " + g_anti_steal_token
                   + "\n  g_secret_key = " + g_secret_key
                   + "\n  g_tracker_http_port = " + g_tracker_http_port
                   + "\n  trackerServers = " + trackerServers
                   + "\n}";
        }
    }
}