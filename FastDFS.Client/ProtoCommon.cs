using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

using FastDFS.Client;

namespace FastDFS.Client
{
    public static class ProtoCommon
    {
        public const byte FDFS_PROTO_CMD_QUIT = 82;
        public const byte TRACKER_PROTO_CMD_SERVER_LIST_GROUP = 91;
        public const byte TRACKER_PROTO_CMD_SERVER_LIST_STORAGE = 92;
        public const byte TRACKER_PROTO_CMD_SERVER_DELETE_STORAGE = 93;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITHOUT_GROUP_ONE = 101;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_FETCH_ONE = 102;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_UPDATE = 103;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITH_GROUP_ONE = 104;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_FETCH_ALL = 105;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITHOUT_GROUP_ALL = 106;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITH_GROUP_ALL = 107;
        public const byte TRACKER_PROTO_CMD_RESP = 100;
        public const byte FDFS_PROTO_CMD_ACTIVE_TEST = 111;
        public const byte STORAGE_PROTO_CMD_UPLOAD_FILE = 11;
        public const byte STORAGE_PROTO_CMD_DELETE_FILE = 12;
        public const byte STORAGE_PROTO_CMD_SET_METADATA = 13;
        public const byte STORAGE_PROTO_CMD_DOWNLOAD_FILE = 14;
        public const byte STORAGE_PROTO_CMD_GET_METADATA = 15;
        public const byte STORAGE_PROTO_CMD_UPLOAD_SLAVE_FILE = 21;
        public const byte STORAGE_PROTO_CMD_QUERY_FILE_INFO = 22;
        public const byte STORAGE_PROTO_CMD_UPLOAD_APPENDER_FILE = 23; //create appender file
        public const byte STORAGE_PROTO_CMD_APPEND_FILE = 24; //append file
        public const byte STORAGE_PROTO_CMD_MODIFY_FILE = 34; //modify appender file
        public const byte STORAGE_PROTO_CMD_TRUNCATE_FILE = 36; //truncate appender file
        public const byte STORAGE_PROTO_CMD_REGENERATE_APPENDER_FILENAME = 38;  //rename appender file to normal file
        public const byte STORAGE_PROTO_CMD_RESP = TRACKER_PROTO_CMD_RESP;
        public const byte FDFS_STORAGE_STATUS_INIT = 0;
        public const byte FDFS_STORAGE_STATUS_WAIT_SYNC = 1;
        public const byte FDFS_STORAGE_STATUS_SYNCING = 2;
        public const byte FDFS_STORAGE_STATUS_IP_CHANGED = 3;
        public const byte FDFS_STORAGE_STATUS_DELETED = 4;
        public const byte FDFS_STORAGE_STATUS_OFFLINE = 5;
        public const byte FDFS_STORAGE_STATUS_ONLINE = 6;
        public const byte FDFS_STORAGE_STATUS_ACTIVE = 7;
        public const byte FDFS_STORAGE_STATUS_NONE = 99;
        /**
         * for overwrite all old metadata
         */
        public static readonly byte STORAGE_SET_METADATA_FLAG_OVERWRITE = (byte)'O';
        /**
         * for replace, insert when the meta item not exist, otherwise update it
         */
        public const byte STORAGE_SET_METADATA_FLAG_MERGE = (byte)'M';
        public const int FDFS_PROTO_PKG_LEN_SIZE = 8;
        public const int FDFS_PROTO_CMD_SIZE = 1;
        public const int FDFS_GROUP_NAME_MAX_LEN = 16;
        public const int FDFS_IPADDR_SIZE = 16;
        public const int FDFS_DOMAIN_NAME_MAX_SIZE = 128;
        public const int FDFS_VERSION_SIZE = 6;
        public const int FDFS_STORAGE_ID_MAX_SIZE = 16;
        public const char FDFS_RECORD_SEPERATOR = '\u0001';
        public const char FDFS_FIELD_SEPERATOR = '\u0002';
        public const int TRACKER_QUERY_STORAGE_FETCH_BODY_LEN =
                FDFS_GROUP_NAME_MAX_LEN + FDFS_IPADDR_SIZE - 1 + FDFS_PROTO_PKG_LEN_SIZE;
        public const int TRACKER_QUERY_STORAGE_STORE_BODY_LEN =
                FDFS_GROUP_NAME_MAX_LEN + FDFS_IPADDR_SIZE + FDFS_PROTO_PKG_LEN_SIZE;
        public const byte FDFS_FILE_EXT_NAME_MAX_LEN = 6;
        public const byte FDFS_FILE_PREFIX_MAX_LEN = 16;
        public const byte FDFS_FILE_PATH_LEN = 10;
        public const byte FDFS_FILENAME_BASE64_LENGTH = 27;
        public const byte FDFS_TRUNK_FILE_INFO_LEN = 16;
        public const byte ERR_NO_ENOENT = 2;
        public const byte ERR_NO_EIO = 5;
        public const byte ERR_NO_EBUSY = 16;
        public const byte ERR_NO_EINVAL = 22;
        public const byte ERR_NO_ENOSPC = 28;
        public const byte ECONNREFUSED = 61;
        public const byte ERR_NO_EALREADY = 114;
        public const long INFINITE_FILE_SIZE = 256 * 1024L * 1024 * 1024 * 1024 * 1024L;
        public const long APPENDER_FILE_SIZE = INFINITE_FILE_SIZE;
        public const long TRUNK_FILE_MARK_SIZE = 512 * 1024L * 1024 * 1024 * 1024 * 1024L;
        public const long NORMAL_LOGIC_FILENAME_LENGTH =
                FDFS_FILE_PATH_LEN + FDFS_FILENAME_BASE64_LENGTH + FDFS_FILE_EXT_NAME_MAX_LEN + 1;
        public const long TRUNK_LOGIC_FILENAME_LENGTH =
                NORMAL_LOGIC_FILENAME_LENGTH + FDFS_TRUNK_FILE_INFO_LEN;

        private const int PROTO_HEADER_CMD_INDEX = FDFS_PROTO_PKG_LEN_SIZE;
        private const int PROTO_HEADER_STATUS_INDEX = FDFS_PROTO_PKG_LEN_SIZE + 1;

        public static string getStorageStatusCaption(byte status)
        {
            switch (status)
            {
                case FDFS_STORAGE_STATUS_INIT:
                    return "INIT";
                case FDFS_STORAGE_STATUS_WAIT_SYNC:
                    return "WAIT_SYNC";
                case FDFS_STORAGE_STATUS_SYNCING:
                    return "SYNCING";
                case FDFS_STORAGE_STATUS_IP_CHANGED:
                    return "IP_CHANGED";
                case FDFS_STORAGE_STATUS_DELETED:
                    return "DELETED";
                case FDFS_STORAGE_STATUS_OFFLINE:
                    return "OFFLINE";
                case FDFS_STORAGE_STATUS_ONLINE:
                    return "ONLINE";
                case FDFS_STORAGE_STATUS_ACTIVE:
                    return "ACTIVE";
                case FDFS_STORAGE_STATUS_NONE:
                    return "NONE";
                default:
                    return "UNKOWN";
            }
        }

        /**
         * pack header by FastDFS transfer protocol
         *
         * @param cmd     which command to send
         * @param pkg_len package body length
         * @param errno   status code, should be (byte)0
         * @return packed byte buffer
         */
        public static byte[] packHeader(byte cmd, long pkg_len, byte errno)
        {
            var header = new byte[FDFS_PROTO_PKG_LEN_SIZE + 2];
            for (var i = 0; i < header.Length; i++)
                header[i] = 0;

            var hex_len = long2buff(pkg_len);
            Array.Copy(hex_len, 0, header, 0, hex_len.Length);
            header[PROTO_HEADER_CMD_INDEX] = cmd;
            header[PROTO_HEADER_STATUS_INDEX] = errno;

            return header;
        }

        /**
         * receive pack header
         *
         * @param in              input stream
         * @param expect_cmd      expect response command
         * @param expect_body_len expect response package body length
         * @return RecvHeaderInfo: errno and pkg body length
         */
        public static RecvHeaderInfo recvHeader(Stream inputStream, byte expect_cmd, long expect_body_len)
        {
            byte[] header;
            int bytes;
            long pkg_len;

            header = new byte[FDFS_PROTO_PKG_LEN_SIZE + 2];

            if ((bytes = inputStream.Read(header, 0, header.Length)) != header.Length)
            {
                throw new IOException("recv package size " + bytes + " != " + header.Length);
            }

            if (header[PROTO_HEADER_CMD_INDEX] != expect_cmd)
            {
                throw new IOException("recv cmd: " + header[PROTO_HEADER_CMD_INDEX] + " is not correct, expect cmd: " +
                                      expect_cmd);
            }

            if (header[PROTO_HEADER_STATUS_INDEX] != 0)
            {
                return new RecvHeaderInfo(header[PROTO_HEADER_STATUS_INDEX], 0);
            }

            pkg_len = ProtoCommon.buff2long(header, 0);
            if (pkg_len < 0)
            {
                throw new IOException("recv body length: " + pkg_len + " < 0!");
            }

            if (expect_body_len >= 0 && pkg_len != expect_body_len)
            {
                throw new IOException("recv body length: " + pkg_len + " is not correct, expect length: " +
                                      expect_body_len);
            }

            return new RecvHeaderInfo(0, pkg_len);
        }

        /**
         * receive whole pack
         *
         * @param in              input stream
         * @param expect_cmd      expect response command
         * @param expect_body_len expect response package body length
         * @return RecvPackageInfo: errno and reponse body(byte buff)
         */
        public static RecvPackageInfo recvPackage(Stream inputStream, byte expect_cmd, long expect_body_len)
        {
            var header = recvHeader(inputStream, expect_cmd, expect_body_len);
            if (header.errno != 0)
            {
                return new RecvPackageInfo(header.errno, null);
            }

            byte[] body = new byte[(int)header.body_len];
            int totalBytes = 0;
            int remainBytes = (int)header.body_len;
            int bytes;

            while (totalBytes < header.body_len)
            {
                if ((bytes = inputStream.Read(body, totalBytes, remainBytes)) < 0)
                {
                    break;
                }

                totalBytes += bytes;
                remainBytes -= bytes;
            }

            if (totalBytes != header.body_len)
            {
                throw new IOException("recv package size " + totalBytes + " != " + header.body_len);
            }

            return new RecvPackageInfo((byte)0, body);
        }

        /**
         * split metadata to name value pair array
         *
         * @param meta_buff metadata
         * @return name value pair array
         */
        public static NameValueCollection split_metadata(string meta_buff)
        {
            return split_metadata(meta_buff, FDFS_RECORD_SEPERATOR, FDFS_FIELD_SEPERATOR);
        }

        /**
         * split metadata to name value pair array
         *
         * @param meta_buff       metadata
         * @param recordSeperator record/row seperator
         * @param filedSeperator  field/column seperator
         * @return name value pair array
         */
        public static NameValueCollection split_metadata(string meta_buff, char recordSeperator, char filedSeperator)
        {
            string[] rows;
            string[] cols;
            var meta_list = new NameValueCollection();
            rows = meta_buff.Split(recordSeperator);
            foreach (var metaData in rows)
            {
                cols = metaData.Split(filedSeperator);
                meta_list.Add(cols[0], cols.Length == 2 ? cols[1] : "");
            }

            return meta_list;
        }

        /**
         * pack metadata array to string
         *
         * @param meta_list metadata array
         * @return packed metadata
         */
        public static string pack_metadata(NameValueCollection meta_list)
        {
            if (meta_list.Count == 0)
            {
                return "";
            }

            var sb = new StringBuilder(32 * meta_list.Count);
            sb.Append(meta_list.Keys[0]).Append(FDFS_FIELD_SEPERATOR).Append(meta_list[0]);
            for (int i = 1; i < meta_list.Count; i++)
            {
                sb.Append(FDFS_RECORD_SEPERATOR);
                sb.Append(meta_list.Keys[i]).Append(FDFS_FIELD_SEPERATOR).Append(meta_list[i]);
            }

            return sb.ToString();
        }

        /**
         * send quit command to server and close socket
         *
         * @param sock the Socket object
         */
        public static void closeSocket(Socket sock)
        {
            var header = packHeader(FDFS_PROTO_CMD_QUIT, 0, (byte)0);
            sock.Send(header);
            sock.Close();
        }

        /**
         * send ACTIVE_TEST command to server, test if network is ok and the server is alive
         *
         * @param sock the Socket object
         */
        public static bool activeTest(Socket sock)
        {
            var header = packHeader(FDFS_PROTO_CMD_ACTIVE_TEST, 0, (byte)0);
            sock.Send(header);

            using (var networkStream = new NetworkStream(sock))
            {
                var headerInfo = recvHeader(networkStream, TRACKER_PROTO_CMD_RESP, 0);

                return headerInfo.errno == 0 ? true : false;
            }
        }

        /**
         * long convert to buff (big-endian)
         *
         * @param n long number
         * @return 8 bytes buff
         */
        public static byte[] long2buff(long n)
        {
            byte[] bs = new byte[8];
            bs[0] = (byte)((n >> 56) & 0xFF);
            bs[1] = (byte)((n >> 48) & 0xFF);
            bs[2] = (byte)((n >> 40) & 0xFF);
            bs[3] = (byte)((n >> 32) & 0xFF);
            bs[4] = (byte)((n >> 24) & 0xFF);
            bs[5] = (byte)((n >> 16) & 0xFF);
            bs[6] = (byte)((n >> 8) & 0xFF);
            bs[7] = (byte)(n & 0xFF);

            return bs;
        }

        /**
         * buff convert to long
         *
         * @param bs     the buffer (big-endian)
         * @param offset the start position based 0
         * @return long number
         */
        public static long buff2long(byte[] bs, int offset)
        {
            return (((long)(bs[offset] >= 0 ? bs[offset] : 256 + bs[offset])) << 56) |
                   (((long)(bs[offset + 1] >= 0 ? bs[offset + 1] : 256 + bs[offset + 1])) << 48) |
                   (((long)(bs[offset + 2] >= 0 ? bs[offset + 2] : 256 + bs[offset + 2])) << 40) |
                   (((long)(bs[offset + 3] >= 0 ? bs[offset + 3] : 256 + bs[offset + 3])) << 32) |
                   (((long)(bs[offset + 4] >= 0 ? bs[offset + 4] : 256 + bs[offset + 4])) << 24) |
                   (((long)(bs[offset + 5] >= 0 ? bs[offset + 5] : 256 + bs[offset + 5])) << 16) |
                   (((long)(bs[offset + 6] >= 0 ? bs[offset + 6] : 256 + bs[offset + 6])) << 8) |
                   ((long)(bs[offset + 7] >= 0 ? bs[offset + 7] : 256 + bs[offset + 7]));
        }

        /**
         * buff convert to int
         *
         * @param bs     the buffer (big-endian)
         * @param offset the start position based 0
         * @return int number
         */
        public static int buff2int(byte[] bs, int offset)
        {
            return (((int)(bs[offset] >= 0 ? bs[offset] : 256 + bs[offset])) << 24) |
                   (((int)(bs[offset + 1] >= 0 ? bs[offset + 1] : 256 + bs[offset + 1])) << 16) |
                   (((int)(bs[offset + 2] >= 0 ? bs[offset + 2] : 256 + bs[offset + 2])) << 8) |
                   ((int)(bs[offset + 3] >= 0 ? bs[offset + 3] : 256 + bs[offset + 3]));
        }

        /**
         * buff convert to ip address
         *
         * @param bs     the buffer (big-endian)
         * @param offset the start position based 0
         * @return ip address
         */
        public static string getIpAddress(byte[] bs, int offset)
        {
            if (bs[0] == 0 || bs[3] == 0) //storage server ID
            {
                return "";
            }

            int n;
            StringBuilder sbResult = new StringBuilder(16);
            for (int i = offset; i < offset + 4; i++)
            {
                n = (bs[i] >= 0) ? bs[i] : 256 + bs[i];
                if (sbResult.Length > 0)
                {
                    sbResult.Append(".");
                }

                sbResult.Append(n);
            }

            return sbResult.ToString();
        }

        /**
         * md5 function
         *
         * @param source the input buffer
         * @return md5 string
         */
        public static string md5(byte[] source)
        {
            char[] hexDigits =  {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
            };
            using (var md = MD5.Create())
            {
                byte[] tmp = md.ComputeHash(source);
                char[] str = new char[32];
                int k = 0;
                for (int i = 0; i < 16; i++)
                {
                    str[k++] = hexDigits[tmp[i] >> 4 & 0xf];
                    str[k++] = hexDigits[tmp[i] & 0xf];
                }

                return new string(str);
            }
        }

        /**
         * get token for file URL
         *
         * @param remote_filename the filename return by FastDFS server
         * @param ts              unix timestamp, unit: second
         * @param secret_key      the secret key
         * @return token string
         */
        public static String getToken(String remote_filename, int ts, String secret_key)
        {
            var encoding = Encoding.GetEncoding(ClientGlobal.g_charset);
            byte[] bsFilename = encoding.GetBytes(remote_filename);
            byte[] bsKey = encoding.GetBytes(secret_key);
            byte[] bsTimestamp = encoding.GetBytes(ts.ToString());

            byte[] buff = new byte[bsFilename.Length + bsKey.Length + bsTimestamp.Length];
            Array.Copy(bsFilename, 0, buff, 0, bsFilename.Length);
            Array.Copy(bsKey, 0, buff, bsFilename.Length, bsKey.Length);
            Array.Copy(bsTimestamp, 0, buff, bsFilename.Length + bsKey.Length, bsTimestamp.Length);

            return md5(buff);
        }

        /**
         * generate slave filename
         *
         * @param master_filename the master filename to generate the slave filename
         * @param prefix_name     the prefix name to generate the slave filename
         * @param ext_name        the extension name of slave filename, null for same as the master extension name
         * @return slave filename string
         */
        public static String genSlaveFilename(String master_filename,
                                              String prefix_name, String ext_name)
        {
            String true_ext_name;
            int dotIndex;

            if (master_filename.Length < 28 + FDFS_FILE_EXT_NAME_MAX_LEN)
            {
                throw new FastDfsException("master filename \"" + master_filename + "\" is invalid");
            }

            dotIndex = master_filename.IndexOf('.', master_filename.Length - (FDFS_FILE_EXT_NAME_MAX_LEN + 1));
            if (ext_name != null)
            {
                if (ext_name.Length == 0)
                {
                    true_ext_name = "";
                }
                else if (ext_name[0] == '.')
                {
                    true_ext_name = ext_name;
                }
                else
                {
                    true_ext_name = "." + ext_name;
                }
            }
            else
            {
                if (dotIndex < 0)
                {
                    true_ext_name = "";
                }
                else
                {
                    true_ext_name = master_filename.Substring(dotIndex);
                }
            }

            if (true_ext_name.Length == 0 && prefix_name.Equals("-m"))
            {
                throw new FastDfsException("prefix_name \"" + prefix_name + "\" is invalid");
            }

            if (dotIndex < 0)
            {
                return master_filename + prefix_name + true_ext_name;
            }
            else
            {
                return master_filename.Substring(0, dotIndex) + prefix_name + true_ext_name;
            }
        }

        /**
         * receive package info
         */
        public class RecvPackageInfo
        {
            public byte errno;
            public byte[] body;

            public RecvPackageInfo(byte errno, byte[] body)
            {
                this.errno = errno;
                this.body = body;
            }
        }

        /**
         * receive header info
         */
        public class RecvHeaderInfo
        {
            public byte errno;
            public long body_len;

            public RecvHeaderInfo(byte errno, long body_len)
            {
                this.errno = errno;
                this.body_len = body_len;
            }
        }
    }
}