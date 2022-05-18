using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace FastDFS.Client
{
    public class TrackerClient
    {
        protected TrackerGroup tracker_group;
        protected byte errno;
        private Encoding _encoding = Encoding.GetEncoding(ClientGlobal.g_charset);

        /**
         * constructor with global tracker group
         */
        public TrackerClient()
        {
            this.tracker_group = ClientGlobal.g_tracker_group;
        }

        /**
         * constructor with specified tracker group
         *
         * @param tracker_group the tracker group object
         */
        public TrackerClient(TrackerGroup tracker_group)
        {
            this.tracker_group = tracker_group;
        }

        /**
         * get the error code of last call
         *
         * @return the error code of last call
         */
        public byte getErrorCode()
        {
            return this.errno;
        }

        /**
         * get a connection to tracker server
         *
         * @return tracker server Socket object, return null if fail
         */
        public TrackerServer getConnection()
        {
            return this.tracker_group.getConnection();
        }

        /**
         * query storage server to upload file
         *
         * @param trackerServer the tracker server
         * @return storage server Socket object, return null if fail
         */
        public StorageServer getStoreStorage(TrackerServer trackerServer)
        {
            String groupName = null;
            return this.getStoreStorage(trackerServer, groupName);
        }

        /**
         * query storage server to upload file
         *
         * @param trackerServer the tracker server
         * @param groupName     the group name to upload file to, can be empty
         * @return storage server object, return null if fail
         */
        public StorageServer getStoreStorage(TrackerServer trackerServer, String groupName)
        {
            byte[] header;
            String ip_addr;
            int port;
            byte cmd;
            int out_len;
            bool bNewConnection;
            byte store_path;
            Socket trackerSocket;

            if (trackerServer == null)
            {
                trackerServer = getConnection();
                if (trackerServer == null)
                {
                    return null;
                }

                bNewConnection = true;
            }
            else
            {
                bNewConnection = false;
            }

            trackerSocket = trackerServer.getSocket();
            var outputStream = new NetworkStream(trackerSocket);

            try
            {
                if (groupName == null || groupName.Length == 0)
                {
                    cmd = ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITHOUT_GROUP_ONE;
                    out_len = 0;
                }
                else
                {
                    cmd = ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITH_GROUP_ONE;
                    out_len = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN;
                }

                header = ProtoCommon.packHeader(cmd, out_len, (byte)0);
                outputStream.Write(header, 0, header.Length);
                var encoder = Encoding.GetEncoding(ClientGlobal.g_charset);
                if (groupName != null && groupName.Length > 0)
                {
                    byte[] bGroupName;
                    byte[] bs;
                    int group_len;

                    bs = encoder.GetBytes(groupName);
                    bGroupName = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];

                    if (bs.Length <= ProtoCommon.FDFS_GROUP_NAME_MAX_LEN)
                    {
                        group_len = bs.Length;
                    }
                    else
                    {
                        group_len = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN;
                    }

                    bGroupName.Fill<byte>(0);
                    Array.Copy(bs, 0, bGroupName, 0, group_len);
                    outputStream.Write(bGroupName, 0, bGroupName.Length);
                }

                ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                    ProtoCommon.TRACKER_PROTO_CMD_RESP,
                    ProtoCommon.TRACKER_QUERY_STORAGE_STORE_BODY_LEN);
                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                ip_addr = Encoding.ASCII.GetString(pkgInfo.body, ProtoCommon.FDFS_GROUP_NAME_MAX_LEN,
                    ProtoCommon.FDFS_IPADDR_SIZE - 1).Trim('\0');

                port = (int)ProtoCommon.buff2long(pkgInfo.body, ProtoCommon.FDFS_GROUP_NAME_MAX_LEN
                    + ProtoCommon.FDFS_IPADDR_SIZE - 1);
                store_path = pkgInfo.body[ProtoCommon.TRACKER_QUERY_STORAGE_STORE_BODY_LEN - 1];

                return new StorageServer(ip_addr, port, store_path);
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }

                throw ex;
            }
            finally
            {
                outputStream.Close();
                if (bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }
            }
        }

        /**
         * query storage servers to upload file
         *
         * @param trackerServer the tracker server
         * @param groupName     the group name to upload file to, can be empty
         * @return storage servers, return null if fail
         */
        public StorageServer[] getStoreStorages(TrackerServer trackerServer, String groupName)
        {
            byte[] header;
            String ip_addr;
            int port;
            byte cmd;
            int out_len;
            bool bNewConnection;
            Socket trackerSocket;

            if (trackerServer == null)
            {
                trackerServer = getConnection();
                if (trackerServer == null)
                {
                    return null;
                }

                bNewConnection = true;
            }
            else
            {
                bNewConnection = false;
            }

            trackerSocket = trackerServer.getSocket();
            var outputStream = new NetworkStream(trackerSocket);

            try
            {
                if (groupName == null || groupName.Length == 0)
                {
                    cmd = ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITHOUT_GROUP_ALL;
                    out_len = 0;
                }
                else
                {
                    cmd = ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITH_GROUP_ALL;
                    out_len = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN;
                }

                header = ProtoCommon.packHeader(cmd, out_len, (byte)0);
                outputStream.Write(header, 0, header.Length);
                var encoder = Encoding.GetEncoding(ClientGlobal.g_charset);
                if (groupName != null && groupName.Length > 0)
                {
                    byte[] bGroupName;
                    byte[] bs;
                    int group_len;

                    bs = encoder.GetBytes(groupName);
                    bGroupName = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];

                    if (bs.Length <= ProtoCommon.FDFS_GROUP_NAME_MAX_LEN)
                    {
                        group_len = bs.Length;
                    }
                    else
                    {
                        group_len = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN;
                    }

                    bGroupName.Fill<byte>(0);
                    Array.Copy(bs, 0, bGroupName, 0, group_len);
                    outputStream.Write(bGroupName, 0, bGroupName.Length);
                }


                ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                    ProtoCommon.TRACKER_PROTO_CMD_RESP, -1);
                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                if (pkgInfo.body.Length < ProtoCommon.TRACKER_QUERY_STORAGE_STORE_BODY_LEN)
                {
                    this.errno = ProtoCommon.ERR_NO_EINVAL;
                    return null;
                }

                int ipPortLen = pkgInfo.body.Length - (ProtoCommon.FDFS_GROUP_NAME_MAX_LEN + 1);
                int recordLength = ProtoCommon.FDFS_IPADDR_SIZE - 1 + ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE;

                if (ipPortLen % recordLength != 0)
                {
                    this.errno = ProtoCommon.ERR_NO_EINVAL;
                    return null;
                }

                int serverCount = ipPortLen / recordLength;
                if (serverCount > 16)
                {
                    this.errno = ProtoCommon.ERR_NO_ENOSPC;
                    return null;
                }

                StorageServer[] results = new StorageServer[serverCount];
                byte store_path = pkgInfo.body[pkgInfo.body.Length - 1];
                int offset = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN;

                for (int i = 0; i < serverCount; i++)
                {
                    ip_addr = Encoding.ASCII.GetString(pkgInfo.body, offset, ProtoCommon.FDFS_IPADDR_SIZE - 1).Trim(
                        '\0');
                    offset += ProtoCommon.FDFS_IPADDR_SIZE - 1;

                    port = (int)ProtoCommon.buff2long(pkgInfo.body, offset);
                    offset += ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE;

                    results[i] = new StorageServer(ip_addr, port, store_path);
                }

                return results;
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }

                throw ex;
            }
            finally
            {
                outputStream.Close();
                if (bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }
            }
        }

        /**
         * query storage server to download file
         *
         * @param trackerServer the tracker server
         * @param groupName     the group name of storage server
         * @param filename      filename on storage server
         * @return storage server Socket object, return null if fail
         */
        public StorageServer getFetchStorage(TrackerServer trackerServer,
            String groupName, String filename)
        {
            ServerInfo[]
                servers = this.getStorages(trackerServer, ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_FETCH_ONE,
                    groupName, filename);
            if (servers == null)
            {
                return null;
            }
            else
            {
                return new StorageServer(servers[0].getIpAddr(), servers[0].getPort(), 0);
            }
        }

        /**
         * query storage server to update file (delete file or set meta data)
         *
         * @param trackerServer the tracker server
         * @param groupName     the group name of storage server
         * @param filename      filename on storage server
         * @return storage server Socket object, return null if fail
         */
        public StorageServer getUpdateStorage(TrackerServer trackerServer,
            String groupName, String filename)
        {
            var servers = getStorages(trackerServer, ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_UPDATE,
                    groupName, filename);
            if (servers == null)
            {
                return null;
            }

            return new StorageServer(servers[0].getIpAddr(), servers[0].getPort(), 0);
        }

        /**
         * get storage servers to download file
         *
         * @param trackerServer the tracker server
         * @param groupName     the group name of storage server
         * @param filename      filename on storage server
         * @return storage servers, return null if fail
         */
        public ServerInfo[] getFetchStorages(TrackerServer trackerServer,
            String groupName, String filename)
        {
            return this.getStorages(trackerServer, ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_FETCH_ALL,
                groupName, filename);
        }

        /**
         * query storage server to download file
         *
         * @param trackerServer the tracker server
         * @param cmd           command code, ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_FETCH_ONE or
         *                      ProtoCommon.TRACKER_PROTO_CMD_SERVICE_QUERY_UPDATE
         * @param groupName     the group name of storage server
         * @param filename      filename on storage server
         * @return storage server Socket object, return null if fail
         */
        protected ServerInfo[] getStorages(TrackerServer trackerServer,
            byte cmd, String groupName, String filename)
        {
            byte[] header;
            byte[] bFileName;
            byte[] bGroupName;
            byte[] bs;
            int len;
            String ip_addr;
            int port;
            bool bNewConnection;
            Socket trackerSocket;

            if (trackerServer == null)
            {
                trackerServer = getConnection();
                if (trackerServer == null)
                {
                    return null;
                }

                bNewConnection = true;
            }
            else
            {
                bNewConnection = false;
            }

            trackerSocket = trackerServer.getSocket();
            var outputStream = new NetworkStream(trackerSocket);

            try
            {
                bs = _encoding.GetBytes(groupName);
                bGroupName = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];
                bFileName = _encoding.GetBytes(filename);

                if (bs.Length <= ProtoCommon.FDFS_GROUP_NAME_MAX_LEN)
                {
                    len = bs.Length;
                }
                else
                {
                    len = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN;
                }

                bGroupName.Fill<byte>(0);
                Array.Copy(bs, 0, bGroupName, 0, len);

                header = ProtoCommon.packHeader(cmd, ProtoCommon.FDFS_GROUP_NAME_MAX_LEN + bFileName.Length, 0);
                byte[] wholePkg = new byte[header.Length + bGroupName.Length + bFileName.Length];
                Array.Copy(header, 0, wholePkg, 0, header.Length);
                Array.Copy(bGroupName, 0, wholePkg, header.Length, bGroupName.Length);
                Array.Copy(bFileName, 0, wholePkg, header.Length + bGroupName.Length, bFileName.Length);
                outputStream.Write(wholePkg, 0, wholePkg.Length);

                ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                    ProtoCommon.TRACKER_PROTO_CMD_RESP, -1);
                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                if (pkgInfo.body.Length < ProtoCommon.TRACKER_QUERY_STORAGE_FETCH_BODY_LEN)
                {
                    throw new IOException("Invalid body Length: " + pkgInfo.body.Length);
                }

                if ((pkgInfo.body.Length - ProtoCommon.TRACKER_QUERY_STORAGE_FETCH_BODY_LEN) %
                    (ProtoCommon.FDFS_IPADDR_SIZE - 1) != 0)
                {
                    throw new IOException("Invalid body Length: " + pkgInfo.body.Length);
                }

                int server_count = 1 + (pkgInfo.body.Length - ProtoCommon.TRACKER_QUERY_STORAGE_FETCH_BODY_LEN) /
                    (ProtoCommon.FDFS_IPADDR_SIZE - 1);

                ip_addr = Encoding.ASCII.GetString(pkgInfo.body, ProtoCommon.FDFS_GROUP_NAME_MAX_LEN,
                    ProtoCommon.FDFS_IPADDR_SIZE - 1).Trim('\0');
                int offset = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN + ProtoCommon.FDFS_IPADDR_SIZE - 1;

                port = (int)ProtoCommon.buff2long(pkgInfo.body, offset);
                offset += ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE;

                ServerInfo[] servers = new ServerInfo[server_count];
                servers[0] = new ServerInfo(ip_addr, port);
                for (int i = 1; i < server_count; i++)
                {
                    servers[i] = new ServerInfo(Encoding.ASCII.GetString(
                            pkgInfo.body, offset, ProtoCommon.FDFS_IPADDR_SIZE - 1).Trim('\0'),
                        port);
                    offset += ProtoCommon.FDFS_IPADDR_SIZE - 1;
                }

                return servers;
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }

                throw ex;
            }
            finally
            {
                outputStream.Close();
                if (bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }
            }
        }

        /**
         * query storage server to download file
         *
         * @param trackerServer the tracker server
         * @param file_id       the file id(including group name and filename)
         * @return storage server Socket object, return null if fail
         */
        public StorageServer getFetchStorage1(TrackerServer trackerServer, String file_id)
        {
            String[]
                parts = new String[2];
            this.errno = StorageClient1.split_file_id(file_id, parts);
            if (this.errno != 0)
            {
                return null;
            }

            return this.getFetchStorage(trackerServer, parts[0], parts[1]);
        }

        /**
         * get storage servers to download file
         *
         * @param trackerServer the tracker server
         * @param file_id       the file id(including group name and filename)
         * @return storage servers, return null if fail
         */
        public ServerInfo[] getFetchStorages1(TrackerServer trackerServer, String file_id)
        {
            String[]
                parts = new String[2];
            this.errno = StorageClient1.split_file_id(file_id, parts);
            if (this.errno != 0)
            {
                return null;
            }

            return this.getFetchStorages(trackerServer, parts[0], parts[1]);
        }

        /**
         * list groups
         *
         * @param trackerServer the tracker server
         * @return group stat array, return null if fail
         */
        public StructGroupStat[] listGroups(TrackerServer trackerServer)
        {
            byte[] header;
            String ip_addr;
            int port;
            byte cmd;
            int out_len;
            bool bNewConnection;
            byte store_path;
            Socket trackerSocket;

            if (trackerServer == null)
            {
                trackerServer = getConnection();
                if (trackerServer == null)
                {
                    return null;
                }

                bNewConnection = true;
            }
            else
            {
                bNewConnection = false;
            }

            trackerSocket = trackerServer.getSocket();
            var outputStream = new NetworkStream(trackerSocket);

            try
            {
                header = ProtoCommon.packHeader(ProtoCommon.TRACKER_PROTO_CMD_SERVER_LIST_GROUP, 0, (byte)0);
                outputStream.Write(header, 0, header.Length);

                ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                    ProtoCommon.TRACKER_PROTO_CMD_RESP, -1);
                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                ProtoStructDecoder<StructGroupStat> decoder = new ProtoStructDecoder<StructGroupStat>();
                return decoder.decode<StructGroupStat>(pkgInfo.body, StructGroupStat.getFieldsTotalSize());
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }

                throw ex;
            }
            catch (Exception ex)
            {
                this.errno = ProtoCommon.ERR_NO_EINVAL;
                return null;
            }
            finally
            {
                outputStream.Close();
                if (bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }
            }
        }

        /**
         * query storage server stat info of the group
         *
         * @param trackerServer the tracker server
         * @param groupName     the group name of storage server
         * @return storage server stat array, return null if fail
         */
        public StructStorageStat[] listStorages(TrackerServer trackerServer, String groupName)
        {
            String storageIpAddr = null;
            return this.listStorages(trackerServer, groupName, storageIpAddr);
        }

        /**
         * query storage server stat info of the group
         *
         * @param trackerServer the tracker server
         * @param groupName     the group name of storage server
         * @param storageIpAddr the storage server ip address, can be null or empty
         * @return storage server stat array, return null if fail
         */
        public StructStorageStat[] listStorages(TrackerServer trackerServer,
            String groupName, String storageIpAddr)
        {
            byte[]
                header;
            byte[]
                bGroupName;
            byte[]
                bs;
            int len;
            bool bNewConnection;
            Socket trackerSocket;

            if (trackerServer == null)
            {
                trackerServer = getConnection();
                if (trackerServer == null)
                {
                    return null;
                }

                bNewConnection = true;
            }
            else
            {
                bNewConnection = false;
            }

            trackerSocket = trackerServer.getSocket();
            var outputStream = new NetworkStream(trackerSocket);

            try
            {
                bs = _encoding.GetBytes(groupName);
                bGroupName = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];

                if (bs.Length <= ProtoCommon.FDFS_GROUP_NAME_MAX_LEN)
                {
                    len = bs.Length;
                }
                else
                {
                    len = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN;
                }

                bGroupName.Fill<byte>(0);
                Array.Copy(bs, 0, bGroupName, 0, len);

                int ipAddrLen;
                byte[] bIpAddr;
                if (storageIpAddr != null && storageIpAddr.Length > 0)
                {
                    bIpAddr = _encoding.GetBytes(storageIpAddr);
                    if (bIpAddr.Length < ProtoCommon.FDFS_IPADDR_SIZE)
                    {
                        ipAddrLen = bIpAddr.Length;
                    }
                    else
                    {
                        ipAddrLen = ProtoCommon.FDFS_IPADDR_SIZE - 1;
                    }
                }
                else
                {
                    bIpAddr = null;
                    ipAddrLen = 0;
                }

                header = ProtoCommon.packHeader(ProtoCommon.TRACKER_PROTO_CMD_SERVER_LIST_STORAGE,
                    ProtoCommon.FDFS_GROUP_NAME_MAX_LEN + ipAddrLen, (byte)0);
                byte[] wholePkg = new byte[header.Length + bGroupName.Length + ipAddrLen];
                Array.Copy(header, 0, wholePkg, 0, header.Length);
                Array.Copy(bGroupName, 0, wholePkg, header.Length, bGroupName.Length);
                if (ipAddrLen > 0)
                {
                    Array.Copy(bIpAddr, 0, wholePkg, header.Length + bGroupName.Length, ipAddrLen);
                }

                outputStream.Write(wholePkg, 0, wholePkg.Length);

                ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                    ProtoCommon.TRACKER_PROTO_CMD_RESP, -1);
                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                ProtoStructDecoder<StructStorageStat> decoder = new ProtoStructDecoder<StructStorageStat>();
                return decoder.decode<StructStorageStat>(pkgInfo.body, StructStorageStat.getFieldsTotalSize());
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }

                throw ex;
            }
            catch (Exception ex)
            {
                this.errno = ProtoCommon.ERR_NO_EINVAL;
                return null;
            }
            finally
            {
                outputStream.Close();
                if (bNewConnection)
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }
            }
        }

        /**
         * delete a storage server from the tracker server
         *
         * @param trackerServer the connected tracker server
         * @param groupName     the group name of storage server
         * @param storageIpAddr the storage server ip address
         * @return true for success, false for fail
         */
        private bool deleteStorage(TrackerServer trackerServer,
            String groupName, String storageIpAddr)
        {
            byte[]
                header;
            byte[]
                bGroupName;
            byte[]
                bs;
            int len;
            Socket trackerSocket;

            trackerSocket = trackerServer.getSocket();
            using (var outputStream = new NetworkStream(trackerSocket))
            {
                bs = _encoding.GetBytes(groupName);
                bGroupName = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];

                if (bs.Length <= ProtoCommon.FDFS_GROUP_NAME_MAX_LEN)
                {
                    len = bs.Length;
                }
                else
                {
                    len = ProtoCommon.FDFS_GROUP_NAME_MAX_LEN;
                }

                bGroupName.Fill<byte>(0);
                Array.Copy(bs, 0, bGroupName, 0, len);

                int ipAddrLen;
                byte[] bIpAddr = _encoding.GetBytes(storageIpAddr);
                if (bIpAddr.Length < ProtoCommon.FDFS_IPADDR_SIZE)
                {
                    ipAddrLen = bIpAddr.Length;
                }
                else
                {
                    ipAddrLen = ProtoCommon.FDFS_IPADDR_SIZE - 1;
                }

                header = ProtoCommon.packHeader(ProtoCommon.TRACKER_PROTO_CMD_SERVER_DELETE_STORAGE,
                    ProtoCommon.FDFS_GROUP_NAME_MAX_LEN + ipAddrLen, (byte)0);
                byte[] wholePkg = new byte[header.Length + bGroupName.Length + ipAddrLen];
                Array.Copy(header, 0, wholePkg, 0, header.Length);
                Array.Copy(bGroupName, 0, wholePkg, header.Length, bGroupName.Length);
                Array.Copy(bIpAddr, 0, wholePkg, header.Length + bGroupName.Length, ipAddrLen);
                outputStream.Write(wholePkg, 0, wholePkg.Length);

                ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                    ProtoCommon.TRACKER_PROTO_CMD_RESP, 0);
                this.errno = pkgInfo.errno;
                return pkgInfo.errno == 0;
            }
        }

        /**
         * delete a storage server from the global FastDFS cluster
         *
         * @param groupName     the group name of storage server
         * @param storageIpAddr the storage server ip address
         * @return true for success, false for fail
         */
        public bool deleteStorage(String groupName, String storageIpAddr)
        {
            return this.deleteStorage(ClientGlobal.g_tracker_group, groupName, storageIpAddr);
        }

        /**
         * delete a storage server from the FastDFS cluster
         *
         * @param trackerGroup  the tracker server group
         * @param groupName     the group name of storage server
         * @param storageIpAddr the storage server ip address
         * @return true for success, false for fail
         */
        public bool deleteStorage(TrackerGroup trackerGroup,
            String groupName, String storageIpAddr)
        {
            int serverIndex;
            int notFoundCount;
            TrackerServer trackerServer;

            notFoundCount = 0;
            for (serverIndex = 0; serverIndex < trackerGroup.tracker_servers.Count; serverIndex++)
            {
                try
                {
                    trackerServer = trackerGroup.getConnection(serverIndex);
                }
                catch (IOException ex)
                {
                    this.errno = ProtoCommon.ECONNREFUSED;
                    return false;
                }

                try
                {
                    StructStorageStat[] storageStats = listStorages(trackerServer, groupName, storageIpAddr);
                    if (storageStats == null)
                    {
                        if (this.errno == ProtoCommon.ERR_NO_ENOENT)
                        {
                            notFoundCount++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (storageStats.Length == 0)
                    {
                        notFoundCount++;
                    }
                    else if (storageStats[0].getStatus() == ProtoCommon.FDFS_STORAGE_STATUS_ONLINE ||
                             storageStats[0].getStatus() == ProtoCommon.FDFS_STORAGE_STATUS_ACTIVE)
                    {
                        this.errno = ProtoCommon.ERR_NO_EBUSY;
                        return false;
                    }
                }
                finally
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }
            }

            if (notFoundCount == trackerGroup.tracker_servers.Count)
            {
                this.errno = ProtoCommon.ERR_NO_ENOENT;
                return false;
            }

            notFoundCount = 0;
            for (serverIndex = 0; serverIndex < trackerGroup.tracker_servers.Count; serverIndex++)
            {
                try
                {
                    trackerServer = trackerGroup.getConnection(serverIndex);
                }
                catch (IOException ex)
                {
                    //System.err.println("connect to server " +
                    //                   trackerGroup.tracker_servers[serverIndex].getAddress().getHostAddress() + ":" +
                    //                   trackerGroup.tracker_servers[serverIndex].getPort() + " fail");
                    //ex.printStackTrace(System.err);
                    this.errno = ProtoCommon.ECONNREFUSED;
                    return false;
                }

                try
                {
                    if (!this.deleteStorage(trackerServer, groupName, storageIpAddr))
                    {
                        if (this.errno != 0)
                        {
                            if (this.errno == ProtoCommon.ERR_NO_ENOENT)
                            {
                                notFoundCount++;
                            }
                            else if (this.errno != ProtoCommon.ERR_NO_EALREADY)
                            {
                                return false;
                            }
                        }
                    }
                }
                finally
                {
                    try
                    {
                        trackerServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                }
            }

            if (notFoundCount == trackerGroup.tracker_servers.Count)
            {
                this.errno = ProtoCommon.ERR_NO_ENOENT;
                return false;
            }

            if (this.errno == ProtoCommon.ERR_NO_ENOENT)
            {
                this.errno = 0;
            }

            return this.errno == 0;
        }
    }
}