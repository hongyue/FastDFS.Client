using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace FastDFS.Client
{
    public class StorageClient
    {
        //public static readonly Base64 base64 = new Base64('-', '_', '.', 0);
        protected TrackerServer trackerServer;
        protected StorageServer storageServer;
        protected byte errno;

        /**
         * constructor using global settings in class ClientGlobal
         */
        public StorageClient()
        {
            this.trackerServer = null;
            this.storageServer = null;
        }

        /**
         * constructor with tracker server and storage server
         *
         * @param trackerServer the tracker server, can be null
         * @param storageServer the storage server, can be null
         */
        public StorageClient(TrackerServer trackerServer, StorageServer storageServer)
        {
            this.trackerServer = trackerServer;
            this.storageServer = storageServer;
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
         * upload file to storage server (by file name)
         *
         * @param local_filename local filename to upload
         * @param file_ext_name  file ext name, do not include dot(.), null to extract ext name from the local filename
         * @param meta_list      meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file </li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(String local_filename, String file_ext_name, NameValueCollection meta_list)
        {
            String group_name = null;
            return this.upload_file(group_name, local_filename, file_ext_name, meta_list);
        }

        /**
         * upload file to storage server (by file name)
         *
         * @param group_name     the group name to upload file to, can be empty
         * @param local_filename local filename to upload
         * @param file_ext_name  file ext name, do not include dot(.), null to extract ext name from the local filename
         * @param meta_list      meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file </li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        protected String[] upload_file(String group_name, String local_filename, String file_ext_name,
            NameValueCollection meta_list)
        {
            byte cmd = ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_FILE;
            return this.upload_file(cmd, group_name, local_filename, file_ext_name, meta_list);
        }

        /**
         * upload file to storage server (by file name)
         *
         * @param cmd            the command
         * @param group_name     the group name to upload file to, can be empty
         * @param local_filename local filename to upload
         * @param file_ext_name  file ext name, do not include dot(.), null to extract ext name from the local filename
         * @param meta_list      meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file </li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        protected String[] upload_file(byte cmd, String group_name, String local_filename, String file_ext_name,
            NameValueCollection meta_list)
        {
            var f = new System.IO.FileInfo(local_filename);
            var fis = new FileStream(local_filename, FileMode.Open);
            if (file_ext_name == null)
            {
                int nPos = local_filename.LastIndexOf('.');
                if (nPos > 0 && local_filename.Length - nPos <= ProtoCommon.FDFS_FILE_EXT_NAME_MAX_LEN + 1)
                {
                    file_ext_name = local_filename.Substring(nPos + 1);
                }
            }

            try
            {
                return do_upload_file(cmd, group_name, null, null, file_ext_name,
                    f.Length, new UploadStream(fis, f.Length), meta_list);
            }
            finally
            {
                fis.Close();
            }
        }

        /**
         * upload file to storage server (by file buff)
         *
         * @param file_buff     file content/buff
         * @param offset        start offset of the buff
         * @param Length        the Length of buff to upload
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(byte[] file_buff, int offset, int Length, String file_ext_name,
            NameValueCollection meta_list)
        {
            String group_name = null;
            return this.upload_file(group_name, file_buff, offset, Length, file_ext_name, meta_list);
        }

        /**
         * upload file to storage server (by file buff)
         *
         * @param group_name    the group name to upload file to, can be empty
         * @param file_buff     file content/buff
         * @param offset        start offset of the buff
         * @param Length        the Length of buff to upload
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(String group_name, byte[] file_buff, int offset, int Length,
            String file_ext_name, NameValueCollection meta_list)
        {
            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_FILE, group_name, null, null, file_ext_name,
                Length, new UploadBuff(file_buff, offset, Length), meta_list);
        }

        /**
         * upload file to storage server (by file buff)
         *
         * @param file_buff     file content/buff
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(byte[] file_buff, String file_ext_name, NameValueCollection meta_list)
        {
            String group_name = null;
            return this.upload_file(group_name, file_buff, 0, file_buff.Length, file_ext_name, meta_list);
        }

        /**
         * upload file to storage server (by file buff)
         *
         * @param group_name    the group name to upload file to, can be empty
         * @param file_buff     file content/buff
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(String group_name, byte[] file_buff,
            String file_ext_name, NameValueCollection meta_list)
        {
            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_FILE, group_name, null, null, file_ext_name,
                file_buff.Length, new UploadBuff(file_buff, 0, file_buff.Length), meta_list);
        }

        /**
         * upload file to storage server (by callback)
         *
         * @param group_name    the group name to upload file to, can be empty
         * @param file_size     the file size
         * @param callback      the write data callback object
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(String group_name, long file_size, IUploadCallback callback,
            String file_ext_name, NameValueCollection meta_list)
        {
            string master_filename = null;
            string prefix_name = null;

            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_FILE, group_name, master_filename,
                prefix_name,
                file_ext_name, file_size, callback, meta_list);
        }

        /**
         * upload file to storage server (by file name, slave file mode)
         *
         * @param group_name      the group name of master file
         * @param master_filename the master file name to generate the slave file
         * @param prefix_name     the prefix name to generate the slave file
         * @param local_filename  local filename to upload
         * @param file_ext_name   file ext name, do not include dot(.), null to extract ext name from the local filename
         * @param meta_list       meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file </li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(String group_name, String master_filename, String prefix_name,
            String local_filename, String file_ext_name, NameValueCollection meta_list)
        {
            if ((group_name == null || group_name.Length == 0) ||
                (master_filename == null || master_filename.Length == 0) ||
                (prefix_name == null))
            {
                throw new FastDfsException("invalid arguement");
            }

            var f = new System.IO.FileInfo(local_filename);
            var fis = new FileStream(local_filename, FileMode.Open);

            if (file_ext_name == null)
            {
                int nPos = local_filename.LastIndexOf('.');
                if (nPos > 0 && local_filename.Length - nPos <= ProtoCommon.FDFS_FILE_EXT_NAME_MAX_LEN + 1)
                {
                    file_ext_name = local_filename.Substring(nPos + 1);
                }
            }

            try
            {
                return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_SLAVE_FILE, group_name, master_filename,
                    prefix_name,
                    file_ext_name, f.Length, new UploadStream(fis, f.Length), meta_list);
            }
            finally
            {
                fis.Close();
            }
        }

        /**
         * upload file to storage server (by file buff, slave file mode)
         *
         * @param group_name      the group name of master file
         * @param master_filename the master file name to generate the slave file
         * @param prefix_name     the prefix name to generate the slave file
         * @param file_buff       file content/buff
         * @param file_ext_name   file ext name, do not include dot(.)
         * @param meta_list       meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(String group_name, String master_filename, String prefix_name,
            byte[] file_buff, String file_ext_name, NameValueCollection meta_list)
        {
            if ((group_name == null || group_name.Length == 0) ||
                (master_filename == null || master_filename.Length == 0) ||
                (prefix_name == null))
            {
                throw new FastDfsException("invalid arguement");
            }

            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_SLAVE_FILE, group_name, master_filename,
                prefix_name,
                file_ext_name, file_buff.Length, new UploadBuff(file_buff, 0, file_buff.Length), meta_list);
        }

        /**
         * upload file to storage server (by file buff, slave file mode)
         *
         * @param group_name      the group name of master file
         * @param master_filename the master file name to generate the slave file
         * @param prefix_name     the prefix name to generate the slave file
         * @param file_buff       file content/buff
         * @param offset          start offset of the buff
         * @param Length          the Length of buff to upload
         * @param file_ext_name   file ext name, do not include dot(.)
         * @param meta_list       meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(String group_name, String master_filename, String prefix_name,
            byte[] file_buff, int offset, int Length, String file_ext_name,
            NameValueCollection meta_list)
        {
            if ((group_name == null || group_name.Length == 0) ||
                (master_filename == null || master_filename.Length == 0) ||
                (prefix_name == null))
            {
                throw new FastDfsException("invalid arguement");
            }

            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_SLAVE_FILE, group_name, master_filename,
                prefix_name,
                file_ext_name, Length, new UploadBuff(file_buff, offset, Length), meta_list);
        }

        /**
         * upload file to storage server (by callback, slave file mode)
         *
         * @param group_name      the group name to upload file to, can be empty
         * @param master_filename the master file name to generate the slave file
         * @param prefix_name     the prefix name to generate the slave file
         * @param file_size       the file size
         * @param callback        the write data callback object
         * @param file_ext_name   file ext name, do not include dot(.)
         * @param meta_list       meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_file(String group_name, String master_filename,
            String prefix_name, long file_size, IUploadCallback callback,
            String file_ext_name, NameValueCollection meta_list)
        {
            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_SLAVE_FILE, group_name, master_filename,
                prefix_name,
                file_ext_name, file_size, callback, meta_list);
        }

        /**
         * upload appender file to storage server (by file name)
         *
         * @param local_filename local filename to upload
         * @param file_ext_name  file ext name, do not include dot(.), null to extract ext name from the local filename
         * @param meta_list      meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file </li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_appender_file(String local_filename, String file_ext_name,
            NameValueCollection meta_list)
        {
            String group_name = null;
            return this.upload_appender_file(group_name, local_filename, file_ext_name, meta_list);
        }

        /**
         * upload appender file to storage server (by file name)
         *
         * @param group_name     the group name to upload file to, can be empty
         * @param local_filename local filename to upload
         * @param file_ext_name  file ext name, do not include dot(.), null to extract ext name from the local filename
         * @param meta_list      meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file </li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        protected String[] upload_appender_file(String group_name, String local_filename, String file_ext_name,
            NameValueCollection meta_list)
        {
            byte cmd = ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_APPENDER_FILE;
            return this.upload_file(cmd, group_name, local_filename, file_ext_name, meta_list);
        }

        /**
         * upload appender file to storage server (by file buff)
         *
         * @param file_buff     file content/buff
         * @param offset        start offset of the buff
         * @param Length        the Length of buff to upload
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_appender_file(byte[] file_buff, int offset, int Length, String file_ext_name,
            NameValueCollection meta_list)
        {
            String group_name = null;
            return this.upload_appender_file(group_name, file_buff, offset, Length, file_ext_name, meta_list);
        }

        /**
         * upload appender file to storage server (by file buff)
         *
         * @param group_name    the group name to upload file to, can be empty
         * @param file_buff     file content/buff
         * @param offset        start offset of the buff
         * @param Length        the Length of buff to upload
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_appender_file(String group_name, byte[] file_buff, int offset, int Length,
            String file_ext_name, NameValueCollection meta_list)
        {
            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_APPENDER_FILE, group_name, null, null,
                file_ext_name,
                Length, new UploadBuff(file_buff, offset, Length), meta_list);
        }

        /**
         * upload appender file to storage server (by file buff)
         *
         * @param file_buff     file content/buff
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_appender_file(byte[] file_buff, String file_ext_name,
            NameValueCollection meta_list)
        {
            String group_name = null;
            return this.upload_appender_file(group_name, file_buff, 0, file_buff.Length, file_ext_name, meta_list);
        }

        /**
         * upload appender file to storage server (by file buff)
         *
         * @param group_name    the group name to upload file to, can be empty
         * @param file_buff     file content/buff
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_appender_file(String group_name, byte[] file_buff,
            String file_ext_name, NameValueCollection meta_list)
        {
            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_APPENDER_FILE, group_name, null, null,
                file_ext_name,
                file_buff.Length, new UploadBuff(file_buff, 0, file_buff.Length), meta_list);
        }

        /**
         * upload appender file to storage server (by callback)
         *
         * @param group_name    the group name to upload file to, can be empty
         * @param file_size     the file size
         * @param callback      the write data callback object
         * @param file_ext_name file ext name, do not include dot(.)
         * @param meta_list     meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li>results[0]: the group name to store the file</li></ul>
         * <ul><li>results[1]: the new created filename</li></ul>
         * return null if fail
         */
        public String[] upload_appender_file(String group_name, long file_size, IUploadCallback callback,
            String file_ext_name, NameValueCollection meta_list)
        {
            String master_filename = null;
            String prefix_name = null;

            return this.do_upload_file(ProtoCommon.STORAGE_PROTO_CMD_UPLOAD_APPENDER_FILE, group_name, master_filename,
                prefix_name,
                file_ext_name, file_size, callback, meta_list);
        }

        /**
         * append file to storage server (by file name)
         *
         * @param group_name        the group name of appender file
         * @param appender_filename the appender filename
         * @param local_filename    local filename to append
         * @return 0 for success, != 0 for error (error no)
         */
        public int append_file(String group_name, String appender_filename, String local_filename)
        {
            var f = new System.IO.FileInfo(local_filename);
            var fis = new FileStream(local_filename, FileMode.Append, FileAccess.ReadWrite);

            try
            {
                return this.do_append_file(group_name, appender_filename, f.Length,
                    new UploadStream(fis, f.Length));
            }
            finally
            {
                fis.Close();
            }
        }

        /**
         * append file to storage server (by file buff)
         *
         * @param group_name        the group name of appender file
         * @param appender_filename the appender filename
         * @param file_buff         file content/buff
         * @return 0 for success, != 0 for error (error no)
         */
        public int append_file(String group_name, String appender_filename, byte[] file_buff)
        {
            return this.do_append_file(group_name, appender_filename, file_buff.Length,
                new UploadBuff(file_buff, 0, file_buff.Length));
        }

        /**
         * append file to storage server (by file buff)
         *
         * @param group_name        the group name of appender file
         * @param appender_filename the appender filename
         * @param file_buff         file content/buff
         * @param offset            start offset of the buff
         * @param Length            the Length of buff to append
         * @return 0 for success, != 0 for error (error no)
         */
        public int append_file(String group_name, String appender_filename,
            byte[] file_buff, int offset, int Length)
        {
            return this.do_append_file(group_name, appender_filename, Length,
                new UploadBuff(file_buff, offset, Length));
        }

        /**
         * append file to storage server (by callback)
         *
         * @param group_name        the group name to append file to
         * @param appender_filename the appender filename
         * @param file_size         the file size
         * @param callback          the write data callback object
         * @return 0 for success, != 0 for error (error no)
         */
        public int append_file(String group_name, String appender_filename,
            long file_size, IUploadCallback callback)
        {
            return this.do_append_file(group_name, appender_filename, file_size, callback);
        }

        /**
         * modify appender file to storage server (by file name)
         *
         * @param group_name        the group name of appender file
         * @param appender_filename the appender filename
         * @param file_offset       the offset of appender file
         * @param local_filename    local filename to append
         * @return 0 for success, != 0 for error (error no)
         */
        public int modify_file(String group_name, String appender_filename,
            long file_offset, String local_filename)
        {
            var f = new System.IO.FileInfo(local_filename);
            var fis = new FileStream(local_filename, FileMode.Open, FileAccess.ReadWrite);

            try
            {
                return this.do_modify_file(group_name, appender_filename, file_offset,
                    f.Length, new UploadStream(fis, f.Length));
            }
            finally
            {
                fis.Close();
            }
        }

        /**
         * modify appender file to storage server (by file buff)
         *
         * @param group_name        the group name of appender file
         * @param appender_filename the appender filename
         * @param file_offset       the offset of appender file
         * @param file_buff         file content/buff
         * @return 0 for success, != 0 for error (error no)
         */
        public int modify_file(String group_name, String appender_filename,
            long file_offset, byte[] file_buff)
        {
            return this.do_modify_file(group_name, appender_filename, file_offset,
                file_buff.Length, new UploadBuff(file_buff, 0, file_buff.Length));
        }

        /**
         * modify appender file to storage server (by file buff)
         *
         * @param group_name        the group name of appender file
         * @param appender_filename the appender filename
         * @param file_offset       the offset of appender file
         * @param file_buff         file content/buff
         * @param buffer_offset     start offset of the buff
         * @param buffer_length     the Length of buff to modify
         * @return 0 for success, != 0 for error (error no)
         */
        public int modify_file(String group_name, String appender_filename,
            long file_offset, byte[] file_buff, int buffer_offset, int buffer_length)
        {
            return this.do_modify_file(group_name, appender_filename, file_offset,
                buffer_length, new UploadBuff(file_buff, buffer_offset, buffer_length));
        }

        /**
         * modify appender file to storage server (by callback)
         *
         * @param group_name        the group name to modify file to
         * @param appender_filename the appender filename
         * @param file_offset       the offset of appender file
         * @param modify_size       the modify size
         * @param callback          the write data callback object
         * @return 0 for success, != 0 for error (error no)
         */
        public int modify_file(String group_name, String appender_filename,
            long file_offset, long modify_size, IUploadCallback callback)
        {
            return this.do_modify_file(group_name, appender_filename, file_offset,
                modify_size, callback);
        }

        /**
         * upload file to storage server
         *
         * @param cmd             the command code
         * @param group_name      the group name to upload file to, can be empty
         * @param master_filename the master file name to generate the slave file
         * @param prefix_name     the prefix name to generate the slave file
         * @param file_ext_name   file ext name, do not include dot(.)
         * @param file_size       the file size
         * @param callback        the write data callback object
         * @param meta_list       meta info array
         * @return 2 elements string array if success:<br>
         * <ul><li> results[0]: the group name to store the file</li></ul>
         * <ul><li> results[1]: the new created filename</li></ul>
         * return null if fail
         */
        protected String[] do_upload_file(byte cmd, String group_name, String master_filename,
            String prefix_name, String file_ext_name, long file_size,
            IUploadCallback callback,
            NameValueCollection meta_list)
        {
            byte[] header;
            byte[] ext_name_bs;
            String new_group_name;
            String remote_filename;
            bool bNewConnection;
            Socket storageSocket;
            byte[] sizeBytes;
            byte[] hexLenBytes;
            byte[] masterFilenameBytes;
            bool bUploadSlave;
            int offset;
            long body_len;

            bUploadSlave = ((group_name != null && group_name.Length > 0) &&
                            (master_filename != null && master_filename.Length > 0) &&
                            (prefix_name != null));
            if (bUploadSlave)
            {
                bNewConnection = this.newUpdatableStorageConnection(group_name, master_filename);
            }
            else
            {
                bNewConnection = this.newWritableStorageConnection(group_name);
            }

            try
            {
                storageSocket = this.storageServer.getSocket();
                var encoder = Encoding.GetEncoding(ClientGlobal.g_charset);

                ext_name_bs = new byte[ProtoCommon.FDFS_FILE_EXT_NAME_MAX_LEN];
                ext_name_bs.Fill<byte>(0);
                if (file_ext_name != null && file_ext_name.Length > 0)
                {
                    byte[] bs = encoder.GetBytes(file_ext_name);
                    int ext_name_len = bs.Length;
                    if (ext_name_len > ProtoCommon.FDFS_FILE_EXT_NAME_MAX_LEN)
                    {
                        ext_name_len = ProtoCommon.FDFS_FILE_EXT_NAME_MAX_LEN;
                    }

                    Array.Copy(bs, 0, ext_name_bs,
                        0, ext_name_len);
                }

                if (bUploadSlave)
                {
                    masterFilenameBytes = encoder.GetBytes(master_filename);

                    sizeBytes = new byte[2 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE];
                    body_len = sizeBytes.Length + ProtoCommon.FDFS_FILE_PREFIX_MAX_LEN +
                               ProtoCommon.FDFS_FILE_EXT_NAME_MAX_LEN
                               + masterFilenameBytes.Length + file_size;

                    hexLenBytes = ProtoCommon.long2buff(master_filename.Length);
                    Array.Copy(hexLenBytes, 0, sizeBytes, 0, hexLenBytes.Length);
                    offset = hexLenBytes.Length;
                }
                else
                {
                    masterFilenameBytes = null;
                    sizeBytes = new byte[1 + 1 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE];
                    body_len = sizeBytes.Length + ProtoCommon.FDFS_FILE_EXT_NAME_MAX_LEN + file_size;

                    sizeBytes[0] = (byte) this.storageServer.getStorePathIndex();
                    offset = 1;
                }

                hexLenBytes = ProtoCommon.long2buff(file_size);
                Array.Copy(hexLenBytes, 0, sizeBytes, offset, hexLenBytes.Length);

                var outputStream = new NetworkStream(storageSocket);
                header = ProtoCommon.packHeader(cmd, body_len, (byte) 0);
                byte[] wholePkg = new byte[(int) (header.Length + body_len - file_size)];
                Array.Copy(header, 0, wholePkg, 0, header.Length);
                Array.Copy(sizeBytes, 0, wholePkg, header.Length, sizeBytes.Length);
                offset = header.Length + sizeBytes.Length;
                if (bUploadSlave)
                {
                    byte[] prefix_name_bs = new byte[ProtoCommon.FDFS_FILE_PREFIX_MAX_LEN];
                    byte[] bs = encoder.GetBytes(prefix_name);
                    int prefix_name_len = bs.Length;
                    prefix_name_bs.Fill<byte>(0);
                    if (prefix_name_len > ProtoCommon.FDFS_FILE_PREFIX_MAX_LEN)
                    {
                        prefix_name_len = ProtoCommon.FDFS_FILE_PREFIX_MAX_LEN;
                    }

                    if (prefix_name_len > 0)
                    {
                        Array.Copy(bs, 0, prefix_name_bs, 0, prefix_name_len);
                    }

                    Array.Copy(prefix_name_bs, 0, wholePkg, offset, prefix_name_bs.Length);
                    offset += prefix_name_bs.Length;
                }

                Array.Copy(ext_name_bs, 0, wholePkg, offset, ext_name_bs.Length);
                offset += ext_name_bs.Length;

                if (bUploadSlave)
                {
                    Array.Copy(masterFilenameBytes, 0, wholePkg, offset, masterFilenameBytes.Length);
                    offset += masterFilenameBytes.Length;
                }

                outputStream.Write(wholePkg, 0, wholePkg.Length);

                if ((this.errno = (byte) callback.send(outputStream)) != 0)
                {
                    return null;
                }

                ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                    ProtoCommon.STORAGE_PROTO_CMD_RESP, -1);
                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                if (pkgInfo.body.Length <= ProtoCommon.FDFS_GROUP_NAME_MAX_LEN)
                {
                    throw new FastDfsException("body Length: " + pkgInfo.body.Length + " <= " +
                                               ProtoCommon.FDFS_GROUP_NAME_MAX_LEN);
                }

                new_group_name = encoder.GetString(pkgInfo.body, 0, 
                    ProtoCommon.FDFS_GROUP_NAME_MAX_LEN).Trim();
                remote_filename = encoder.GetString(pkgInfo.body, ProtoCommon.FDFS_GROUP_NAME_MAX_LEN,
                    pkgInfo.body.Length - ProtoCommon.FDFS_GROUP_NAME_MAX_LEN);
                String[] results = new String[2];
                results[0] = new_group_name;
                results[1] = remote_filename;

                if (meta_list == null || meta_list.Count == 0)
                {
                    return results;
                }

                int result = 0;
                try
                {
                    result = this.set_metadata(new_group_name, remote_filename,
                        meta_list, ProtoCommon.STORAGE_SET_METADATA_FLAG_OVERWRITE);
                }
                catch (IOException ex)
                {
                    result = 5;
                    throw ex;
                }
                finally
                {
                    outputStream.Close();
                    if (result != 0)
                    {
                        this.errno = (byte) result;
                        this.delete_file(new_group_name, remote_filename);
                    }
                }

                return results;
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * append file to storage server
         *
         * @param group_name        the group name of appender file
         * @param appender_filename the appender filename
         * @param file_size         the file size
         * @param callback          the write data callback object
         * @return return true for success, false for fail
         */
        protected int do_append_file(String group_name, String appender_filename,
            long file_size, IUploadCallback callback)
        {
            byte[] header;
            bool bNewConnection;
            Socket storageSocket;
            byte[] hexLenBytes;
            byte[] appenderFilenameBytes;
            int offset;
            long body_len;

            if ((group_name == null || group_name.Length == 0) ||
                (appender_filename == null || appender_filename.Length == 0))
            {
                this.errno = ProtoCommon.ERR_NO_EINVAL;
                return this.errno;
            }

            bNewConnection = this.newUpdatableStorageConnection(group_name, appender_filename);

            try
            {
                storageSocket = this.storageServer.getSocket();
                var encoder = Encoding.GetEncoding(ClientGlobal.g_charset);

                appenderFilenameBytes = encoder.GetBytes(appender_filename);

                body_len = 2 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE + appenderFilenameBytes.Length + file_size;

                header = ProtoCommon.packHeader(ProtoCommon.STORAGE_PROTO_CMD_APPEND_FILE, body_len, (byte) 0);

                byte[] wholePkg = new byte[(int) (header.Length + body_len - file_size)];

                Array.Copy(header, 0, wholePkg, 0, header.Length);
                offset = header.Length;

                hexLenBytes = ProtoCommon.long2buff(appender_filename.Length);
                Array.Copy(hexLenBytes, 0, wholePkg, offset, hexLenBytes.Length);
                offset += hexLenBytes.Length;

                hexLenBytes = ProtoCommon.long2buff(file_size);
                Array.Copy(hexLenBytes, 0, wholePkg, offset, hexLenBytes.Length);
                offset += hexLenBytes.Length;

                using (var outputStream = new NetworkStream(storageSocket))
                {

                    Array.Copy(appenderFilenameBytes, 0, wholePkg,
                        offset, appenderFilenameBytes.Length);
                    offset += appenderFilenameBytes.Length;
                    outputStream.Write(wholePkg, 0, wholePkg.Length);
                    if ((this.errno = (byte) callback.send(outputStream)) != 0)
                    {
                        return this.errno;
                    }

                    ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                        ProtoCommon.STORAGE_PROTO_CMD_RESP, 0);
                    this.errno = pkgInfo.errno;
                    if (pkgInfo.errno != 0)
                    {
                        return this.errno;
                    }

                    return 0;
                }
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * modify appender file to storage server
         *
         * @param group_name        the group name of appender file
         * @param appender_filename the appender filename
         * @param file_offset       the offset of appender file
         * @param modify_size       the modify size
         * @param callback          the write data callback object
         * @return return true for success, false for fail
         */
        protected int do_modify_file(String group_name, String appender_filename,
            long file_offset, long modify_size, IUploadCallback callback)
        {
            byte[] header;
            bool bNewConnection;
            Socket storageSocket;
            byte[] hexLenBytes;
            byte[] appenderFilenameBytes;
            int offset;
            long body_len;

            if ((group_name == null || group_name.Length == 0) ||
                (appender_filename == null || appender_filename.Length == 0))
            {
                this.errno = ProtoCommon.ERR_NO_EINVAL;
                return this.errno;
            }

            bNewConnection = this.newUpdatableStorageConnection(group_name, appender_filename);

            try
            {
                storageSocket = this.storageServer.getSocket();
                var encoder = Encoding.GetEncoding(ClientGlobal.g_charset);

                appenderFilenameBytes = encoder.GetBytes(appender_filename);

                body_len = 3 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE + appenderFilenameBytes.Length + modify_size;

                header = ProtoCommon.packHeader(ProtoCommon.STORAGE_PROTO_CMD_MODIFY_FILE, body_len, (byte) 0);

                byte[] wholePkg = new byte[(int) (header.Length + body_len - modify_size)];

                Array.Copy(header, 0, wholePkg, 0, header.Length);
                offset = header.Length;

                hexLenBytes = ProtoCommon.long2buff(appender_filename.Length);
                Array.Copy(hexLenBytes, 0, wholePkg, offset, hexLenBytes.Length);
                offset += hexLenBytes.Length;

                hexLenBytes = ProtoCommon.long2buff(file_offset);
                Array.Copy(hexLenBytes, 0, wholePkg, offset, hexLenBytes.Length);
                offset += hexLenBytes.Length;

                hexLenBytes = ProtoCommon.long2buff(modify_size);
                Array.Copy(hexLenBytes, 0, wholePkg, offset, hexLenBytes.Length);
                offset += hexLenBytes.Length;

                using (var outputStream = new NetworkStream(storageSocket))
                {
                    Array.Copy(appenderFilenameBytes, 0, wholePkg, offset, appenderFilenameBytes.Length);
                    offset += appenderFilenameBytes.Length;
                    outputStream.Write(wholePkg, 0, wholePkg.Length);
                    if ((this.errno = (byte) callback.send(outputStream)) != 0)
                    {
                        return this.errno;
                    }

                    ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                        ProtoCommon.STORAGE_PROTO_CMD_RESP, 0);
                    this.errno = pkgInfo.errno;
                    if (pkgInfo.errno != 0)
                    {
                        return this.errno;
                    }

                    return 0;
                }
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * delete file from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @return 0 for success, none zero for fail (error code)
         */
        public int delete_file(String group_name, String remote_filename)
        {
            bool bNewConnection = this.newUpdatableStorageConnection(group_name, remote_filename);
            Socket storageSocket = this.storageServer.getSocket();

            try
            {
                this.send_package(ProtoCommon.STORAGE_PROTO_CMD_DELETE_FILE, group_name, remote_filename);
                using (var outputStream = new NetworkStream(storageSocket))
                {
                    ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                        ProtoCommon.STORAGE_PROTO_CMD_RESP, 0);

                    this.errno = pkgInfo.errno;
                    return pkgInfo.errno;
                }
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * truncate appender file to size 0 from storage server
         *
         * @param group_name        the group name of storage server
         * @param appender_filename the appender filename
         * @return 0 for success, none zero for fail (error code)
         */
        public int truncate_file(String group_name, String appender_filename)
        {
            long truncated_file_size = 0;
            return this.truncate_file(group_name, appender_filename, truncated_file_size);
        }

        /**
         * truncate appender file from storage server
         *
         * @param group_name          the group name of storage server
         * @param appender_filename   the appender filename
         * @param truncated_file_size truncated file size
         * @return 0 for success, none zero for fail (error code)
         */
        public int truncate_file(String group_name, String appender_filename,
            long truncated_file_size)
        {
            byte[] header;
            bool bNewConnection;
            Socket storageSocket;
            byte[] hexLenBytes;
            byte[] appenderFilenameBytes;
            int offset;
            int body_len;

            if ((group_name == null || group_name.Length == 0) ||
                (appender_filename == null || appender_filename.Length == 0))
            {
                this.errno = ProtoCommon.ERR_NO_EINVAL;
                return this.errno;
            }

            bNewConnection = this.newUpdatableStorageConnection(group_name, appender_filename);

            try
            {
                storageSocket = this.storageServer.getSocket();
                var encoder = Encoding.GetEncoding(ClientGlobal.g_charset);
                appenderFilenameBytes = encoder.GetBytes(appender_filename);

                body_len = 2 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE + appenderFilenameBytes.Length;

                header = ProtoCommon.packHeader(ProtoCommon.STORAGE_PROTO_CMD_TRUNCATE_FILE, body_len, (byte) 0);

                byte[] wholePkg = new byte[header.Length + body_len];

                Array.Copy(header, 0, wholePkg, 0, header.Length);
                offset = header.Length;

                hexLenBytes = ProtoCommon.long2buff(appender_filename.Length);
                Array.Copy(hexLenBytes, 0, wholePkg, offset, hexLenBytes.Length);
                offset += hexLenBytes.Length;

                hexLenBytes = ProtoCommon.long2buff(truncated_file_size);
                Array.Copy(hexLenBytes, 0, wholePkg, offset, hexLenBytes.Length);
                offset += hexLenBytes.Length;

                using (var outputStream = new NetworkStream(storageSocket))
                {

                    Array.Copy(appenderFilenameBytes, 0, wholePkg, offset, appenderFilenameBytes.Length);
                    offset += appenderFilenameBytes.Length;
                    outputStream.Write(wholePkg, 0, wholePkg.Length);
                    ProtoCommon.RecvPackageInfo pkgInfo = ProtoCommon.recvPackage(outputStream,
                        ProtoCommon.STORAGE_PROTO_CMD_RESP, 0);
                    this.errno = pkgInfo.errno;
                    return pkgInfo.errno;
                }
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * download file from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @return file content/buff, return null if fail
         */
        public byte[] download_file(String group_name, String remote_filename)
        {
            long file_offset = 0;
            long download_bytes = 0;

            return this.download_file(group_name, remote_filename, file_offset, download_bytes);
        }

        /**
         * download file from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @param file_offset     the start offset of the file
         * @param download_bytes  download bytes, 0 for remain bytes from offset
         * @return file content/buff, return null if fail
         */
        public byte[] download_file(String group_name, String remote_filename, long file_offset, long download_bytes)
        {
            bool bNewConnection = this.newReadableStorageConnection(group_name, remote_filename);
            Socket storageSocket = this.storageServer.getSocket();

            try
            {
                ProtoCommon.RecvPackageInfo pkgInfo;

                this.send_download_package(group_name, remote_filename, file_offset, download_bytes);
                pkgInfo = ProtoCommon.recvPackage(storageSocket.getInputStream(),
                    ProtoCommon.STORAGE_PROTO_CMD_RESP, -1);

                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                return pkgInfo.body;
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * download file from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @param local_filename  filename on local
         * @return 0 success, return none zero errno if fail
         */
        public int download_file(String group_name, String remote_filename,
            String local_filename)
        {
            long file_offset = 0;
            long download_bytes = 0;
            return this.download_file(group_name, remote_filename,
                file_offset, download_bytes, local_filename);
        }

        /**
         * download file from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @param file_offset     the start offset of the file
         * @param download_bytes  download bytes, 0 for remain bytes from offset
         * @param local_filename  filename on local
         * @return 0 success, return none zero errno if fail
         */
        public int download_file(String group_name, String remote_filename,
            long file_offset, long download_bytes,
            String local_filename)
        {
            bool bNewConnection = this.newReadableStorageConnection(group_name, remote_filename);
            Socket storageSocket = this.storageServer.getSocket();
            try
            {
                ProtoCommon.RecvHeaderInfo header;
                FileOutputStream out = new FileOutputStream(local_filename);
                try
                {
                    this.errno = 0;
                    this.send_download_package(group_name, remote_filename, file_offset, download_bytes);

                    InputStream in = storageSocket.getInputStream();
                    header = ProtoCommon.recvHeader(in, ProtoCommon.STORAGE_PROTO_CMD_RESP, -1);
                    this.errno = header.errno;
                    if (header.errno != 0)
                    {
                        return header.errno;
                    }

                    byte[]
                        buff = new byte[256 * 1024];
                    long remainBytes = header.body_len;
                    int bytes;

                    //System.out.println("expect_body_len=" + header.body_len);

                    while (remainBytes > 0)
                    {
                        if ((bytes = in.read(buff, 0,
                            remainBytes > buff.Length ? buff.Length : (int) remainBytes)) < 0) {
                            throw new IOException("recv package size " + (header.body_len - remainBytes) + " != " +
                                                  header.body_len);
                        }

                        out.write(buff, 0, bytes);
                        remainBytes -= bytes;

                        //System.out.println("totalBytes=" + (header.body_len - remainBytes));
                    }

                    return 0;
                }
                catch (IOException ex)
                {
                    if (this.errno == 0)
                    {
                        this.errno = ProtoCommon.ERR_NO_EIO;
                    }

                    throw ex;
                }
                finally
                {
                    out.close();
                    if (this.errno != 0)
                    {
                        (new File(local_filename)).delete();
                    }
                }
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * download file from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @param callback        call callback.recv() when data arrive
         * @return 0 success, return none zero errno if fail
         */
        public int download_file(String group_name, String remote_filename,
            DownloadCallback callback)
        {
            long file_offset = 0;
            long download_bytes = 0;
            return this.download_file(group_name, remote_filename,
                file_offset, download_bytes, callback);
        }

        /**
         * download file from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @param file_offset     the start offset of the file
         * @param download_bytes  download bytes, 0 for remain bytes from offset
         * @param callback        call callback.recv() when data arrive
         * @return 0 success, return none zero errno if fail
         */
        public int download_file(String group_name, String remote_filename,
            long file_offset, long download_bytes,
            DownloadCallback callback)
        {
            int result;
            bool bNewConnection = this.newReadableStorageConnection(group_name, remote_filename);
            Socket storageSocket = this.storageServer.getSocket();

            try
            {
                ProtoCommon.RecvHeaderInfo header;
                this.send_download_package(group_name, remote_filename, file_offset, download_bytes);

                InputStream in = storageSocket.getInputStream();
                header = ProtoCommon.recvHeader(in, ProtoCommon.STORAGE_PROTO_CMD_RESP, -1);
                this.errno = header.errno;
                if (header.errno != 0)
                {
                    return header.errno;
                }

                byte[]
                    buff = new byte[2 * 1024];
                long remainBytes = header.body_len;
                int bytes;

                //System.out.println("expect_body_len=" + header.body_len);

                while (remainBytes > 0)
                {
                    if ((bytes = in.read(buff, 0, remainBytes > buff.Length ? buff.Length : (int) remainBytes)) < 0) {
                        throw new IOException("recv package size " + (header.body_len - remainBytes) + " != " +
                                              header.body_len);
                    }

                    if ((result = callback.recv(header.body_len, buff, bytes)) != 0)
                    {
                        this.errno = (byte) result;
                        return result;
                    }

                    remainBytes -= bytes;

                    //System.out.println("totalBytes=" + (header.body_len - remainBytes));
                }

                return 0;
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * get all metadata items from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @return meta info array, return null if fail
         */
        public NameValuePair[] get_metadata(String group_name, String remote_filename)
        {
            bool bNewConnection = this.newUpdatableStorageConnection(group_name, remote_filename);
            Socket storageSocket = this.storageServer.getSocket();

            try
            {
                ProtoCommon.RecvPackageInfo pkgInfo;

                this.send_package(ProtoCommon.STORAGE_PROTO_CMD_GET_METADATA, group_name, remote_filename);
                pkgInfo = ProtoCommon.recvPackage(storageSocket.getInputStream(),
                    ProtoCommon.STORAGE_PROTO_CMD_RESP, -1);

                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                return ProtoCommon.split_metadata(new String(pkgInfo.body, ClientGlobal.g_charset));
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * set metadata items to storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @param meta_list       meta item array
         * @param op_flag         flag, can be one of following values: <br>
         *                        <ul><li> ProtoCommon.STORAGE_SET_METADATA_FLAG_OVERWRITE: overwrite all old
         *                        metadata items</li></ul>
         *                        <ul><li> ProtoCommon.STORAGE_SET_METADATA_FLAG_MERGE: merge, insert when
         *                        the metadata item not exist, otherwise update it</li></ul>
         * @return 0 for success, !=0 fail (error code)
         */
        public int set_metadata(String group_name, String remote_filename,
            NameValueCollection meta_list, byte op_flag)
        {
            bool bNewConnection = this.newUpdatableStorageConnection(group_name, remote_filename);
            Socket storageSocket = this.storageServer.getSocket();

            try
            {
                byte[] header;
                byte[] groupBytes;
                byte[] filenameBytes;
                byte[] meta_buff;
                byte[] bs;
                int groupLen;
                byte[] sizeBytes;
                ProtoCommon.RecvPackageInfo pkgInfo;

                if (meta_list == null)
                {
                    meta_buff = new byte[0];
                }
                else
                {
                    meta_buff = ProtoCommon.pack_metadata(meta_list).getBytes(ClientGlobal.g_charset);
                }

                filenameBytes = remote_filename.getBytes(ClientGlobal.g_charset);
                sizeBytes = new byte[2 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE];
                Arrays.fill(sizeBytes, (byte) 0);

                bs = ProtoCommon.long2buff(filenameBytes.Length);
                Array.Copy(bs, 0, sizeBytes, 0, bs.Length);
                bs = ProtoCommon.long2buff(meta_buff.Length);
                Array.Copy(bs, 0, sizeBytes, ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE, bs.Length);

                groupBytes = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];
                bs = group_name.getBytes(ClientGlobal.g_charset);

                Arrays.fill(groupBytes, (byte) 0);
                if (bs.Length <= groupBytes.Length)
                {
                    groupLen = bs.Length;
                }
                else
                {
                    groupLen = groupBytes.Length;
                }

                Array.Copy(bs, 0, groupBytes, 0, groupLen);

                header = ProtoCommon.packHeader(ProtoCommon.STORAGE_PROTO_CMD_SET_METADATA,
                    2 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE + 1 + groupBytes.Length
                    + filenameBytes.Length + meta_buff.Length, (byte) 0);
                OutputStream out = storageSocket.getOutputStream();
                byte[] wholePkg =
                    new byte[header.Length + sizeBytes.Length + 1 + groupBytes.Length + filenameBytes.Length];
                Array.Copy(header, 0, wholePkg, 0, header.Length);
                Array.Copy(sizeBytes, 0, wholePkg, header.Length, sizeBytes.Length);
                wholePkg[header.Length + sizeBytes.Length] = op_flag;
                Array.Copy(groupBytes, 0, wholePkg, header.Length + sizeBytes.Length + 1, groupBytes.Length);
                Array.Copy(filenameBytes, 0, wholePkg, header.Length + sizeBytes.Length + 1 + groupBytes.Length,
                        filenameBytes.Length);
                    out.write(wholePkg);
                if (meta_buff.Length > 0)
                {
                    out.write(meta_buff);
                }

                pkgInfo = ProtoCommon.recvPackage(storageSocket.getInputStream(),
                    ProtoCommon.STORAGE_PROTO_CMD_RESP, 0);

                this.errno = pkgInfo.errno;
                return pkgInfo.errno;
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * get file info decoded from the filename, fetch from the storage if necessary
         *
         * @param group_name      the group name
         * @param remote_filename the filename
         * @return FileInfo object for success, return null for fail
         */
        public FileInfo get_file_info(String group_name, String remote_filename)
        {
            if (remote_filename.Length() < ProtoCommon.FDFS_FILE_PATH_LEN + ProtoCommon.FDFS_FILENAME_BASE64_LENGTH
                                                                          + ProtoCommon.FDFS_FILE_EXT_NAME_MAX_LEN + 1)
            {
                this.errno = ProtoCommon.ERR_NO_EINVAL;
                return null;
            }

            byte[] buff = base64.decodeAuto(remote_filename.substring(ProtoCommon.FDFS_FILE_PATH_LEN,
                ProtoCommon.FDFS_FILE_PATH_LEN + ProtoCommon.FDFS_FILENAME_BASE64_LENGTH));

            long file_size = ProtoCommon.buff2long(buff, 4 * 2);
            if (((remote_filename.Length() > ProtoCommon.TRUNK_LOGIC_FILENAME_LENGTH) ||
                 ((remote_filename.Length() > ProtoCommon.NORMAL_LOGIC_FILENAME_LENGTH) &&
                  ((file_size & ProtoCommon.TRUNK_FILE_MARK_SIZE) == 0))) ||
                ((file_size & ProtoCommon.APPENDER_FILE_SIZE) != 0))
            {
                //slave file or appender file
                FileInfo fi = this.query_file_info(group_name, remote_filename);
                if (fi == null)
                {
                    return null;
                }

                return fi;
            }

            FileInfo fileInfo = new FileInfo(file_size, 0, 0, ProtoCommon.getIpAddress(buff, 0));
            fileInfo.setCreateTimestamp(ProtoCommon.buff2int(buff, 4));
            if ((file_size >> 63) != 0)
            {
                file_size &= 0xFFFFFFFFL; //low 32 bits is file size
                fileInfo.setFileSize(file_size);
            }

            fileInfo.setCrc32(ProtoCommon.buff2int(buff, 4 * 4));

            return fileInfo;
        }

        /**
         * get file info from storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @return FileInfo object for success, return null for fail
         */
        public FileInfo query_file_info(String group_name, String remote_filename)
        {
            bool bNewConnection = this.newUpdatableStorageConnection(group_name, remote_filename);
            Socket storageSocket = this.storageServer.getSocket();

            try
            {
                byte[] header;
                byte[] groupBytes;
                byte[] filenameBytes;
                byte[] bs;
                int groupLen;
                ProtoCommon.RecvPackageInfo pkgInfo;

                filenameBytes = remote_filename.getBytes(ClientGlobal.g_charset);
                groupBytes = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];
                bs = group_name.getBytes(ClientGlobal.g_charset);

                Arrays.fill(groupBytes, (byte) 0);
                if (bs.Length <= groupBytes.Length)
                {
                    groupLen = bs.Length;
                }
                else
                {
                    groupLen = groupBytes.Length;
                }

                Array.Copy(bs, 0, groupBytes, 0, groupLen);

                header = ProtoCommon.packHeader(ProtoCommon.STORAGE_PROTO_CMD_QUERY_FILE_INFO,
                    +groupBytes.Length + filenameBytes.Length, (byte) 0);
                OutputStream out = storageSocket.getOutputStream();
                byte[] wholePkg = new byte[header.Length + groupBytes.Length + filenameBytes.Length];
                Array.Copy(header, 0, wholePkg, 0, header.Length);
                Array.Copy(groupBytes, 0, wholePkg, header.Length, groupBytes.Length);
                Array.Copy(filenameBytes, 0, wholePkg, header.Length + groupBytes.Length, filenameBytes.Length);
                    out.write(wholePkg);

                pkgInfo = ProtoCommon.recvPackage(storageSocket.getInputStream(),
                    ProtoCommon.STORAGE_PROTO_CMD_RESP,
                    3 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE +
                    ProtoCommon.FDFS_IPADDR_SIZE);

                this.errno = pkgInfo.errno;
                if (pkgInfo.errno != 0)
                {
                    return null;
                }

                long file_size = ProtoCommon.buff2long(pkgInfo.body, 0);
                int create_timestamp = (int) ProtoCommon.buff2long(pkgInfo.body, ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE);
                int crc32 = (int) ProtoCommon.buff2long(pkgInfo.body, 2 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE);
                String source_ip_addr = (new String(pkgInfo.body, 3 * ProtoCommon.FDFS_PROTO_PKG_LEN_SIZE,
                    ProtoCommon.FDFS_IPADDR_SIZE)).trim();
                return new FileInfo(file_size, create_timestamp, crc32, source_ip_addr);
            }
            catch (IOException ex)
            {
                if (!bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }

                throw ex;
            }
            finally
            {
                if (bNewConnection)
                {
                    try
                    {
                        this.storageServer.close();
                    }
                    catch (IOException ex1)
                    {
                        ex1.printStackTrace();
                    }
                    finally
                    {
                        this.storageServer = null;
                    }
                }
            }
        }

        /**
         * check storage socket, if null create a new connection
         *
         * @param group_name the group name to upload file to, can be empty
         * @return true if create a new connection
         */
        protected bool newWritableStorageConnection(String group_name)
        {
            if (this.storageServer != null)
            {
                return false;
            }
            else
            {
                TrackerClient tracker = new TrackerClient();
                this.storageServer = tracker.getStoreStorage(this.trackerServer, group_name);
                if (this.storageServer == null)
                {
                    throw new MyException("getStoreStorage fail, errno code: " + tracker.getErrorCode());
                }

                return true;
            }
        }

        /**
         * check storage socket, if null create a new connection
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @return true if create a new connection
         */
        protected bool newReadableStorageConnection(String group_name, String remote_filename)
        {
            if (this.storageServer != null)
            {
                return false;
            }
            else
            {
                TrackerClient tracker = new TrackerClient();
                this.storageServer = tracker.getFetchStorage(this.trackerServer, group_name, remote_filename);
                if (this.storageServer == null)
                {
                    throw new MyException("getStoreStorage fail, errno code: " + tracker.getErrorCode());
                }

                return true;
            }
        }

        /**
         * check storage socket, if null create a new connection
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @return true if create a new connection
         */
        protected bool newUpdatableStorageConnection(String group_name, String remote_filename)
        {
            if (this.storageServer != null)
            {
                return false;
            }
            else
            {
                TrackerClient tracker = new TrackerClient();
                this.storageServer = tracker.getUpdateStorage(this.trackerServer, group_name, remote_filename);
                if (this.storageServer == null)
                {
                    throw new FastDfsException("getStoreStorage fail, errno code: " + tracker.getErrorCode());
                }

                return true;
            }
        }

        /**
         * send package to storage server
         *
         * @param cmd             which command to send
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         */
        protected void send_package(byte cmd, String group_name, String remote_filename)
        {
            byte[]
                header;
            byte[]
                groupBytes;
            byte[]
                filenameBytes;
            byte[]
                bs;
            int groupLen;

            groupBytes = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];
            bs = group_name.getBytes(ClientGlobal.g_charset);
            filenameBytes = remote_filename.getBytes(ClientGlobal.g_charset);

            Arrays.fill(groupBytes, (byte) 0);
            if (bs.Length <= groupBytes.Length)
            {
                groupLen = bs.Length;
            }
            else
            {
                groupLen = groupBytes.Length;
            }

            Array.Copy(bs, 0, groupBytes, 0, groupLen);

            header = ProtoCommon.packHeader(cmd, groupBytes.Length + filenameBytes.Length, (byte) 0);
            byte[] wholePkg = new byte[header.Length + groupBytes.Length + filenameBytes.Length];
            Array.Copy(header, 0, wholePkg, 0, header.Length);
            Array.Copy(groupBytes, 0, wholePkg, header.Length, groupBytes.Length);
            Array.Copy(filenameBytes, 0, wholePkg, header.Length + groupBytes.Length, filenameBytes.Length);
            this.storageServer.getSocket().getOutputStream().write(wholePkg);
        }

        /**
         * send package to storage server
         *
         * @param group_name      the group name of storage server
         * @param remote_filename filename on storage server
         * @param file_offset     the start offset of the file
         * @param download_bytes  download bytes
         */
        protected void send_download_package(String group_name, String remote_filename, long file_offset,
            long download_bytes)
        {
            byte[]
                header;
            byte[]
                bsOffset;
            byte[]
                bsDownBytes;
            byte[]
                groupBytes;
            byte[]
                filenameBytes;
            byte[]
                bs;
            int groupLen;

            bsOffset = ProtoCommon.long2buff(file_offset);
            bsDownBytes = ProtoCommon.long2buff(download_bytes);
            groupBytes = new byte[ProtoCommon.FDFS_GROUP_NAME_MAX_LEN];
            bs = group_name.getBytes(ClientGlobal.g_charset);
            filenameBytes = remote_filename.getBytes(ClientGlobal.g_charset);

            Arrays.fill(groupBytes, (byte) 0);
            if (bs.Length <= groupBytes.Length)
            {
                groupLen = bs.Length;
            }
            else
            {
                groupLen = groupBytes.Length;
            }

            Array.Copy(bs, 0, groupBytes, 0, groupLen);

            header = ProtoCommon.packHeader(ProtoCommon.STORAGE_PROTO_CMD_DOWNLOAD_FILE,
                bsOffset.Length + bsDownBytes.Length + groupBytes.Length + filenameBytes.Length, (byte) 0);
            byte[] wholePkg = new byte[header.Length + bsOffset.Length + bsDownBytes.Length + groupBytes.Length +
                                       filenameBytes.Length];
            Array.Copy(header, 0, wholePkg, 0, header.Length);
            Array.Copy(bsOffset, 0, wholePkg, header.Length, bsOffset.Length);
            Array.Copy(bsDownBytes, 0, wholePkg, header.Length + bsOffset.Length, bsDownBytes.Length);
            Array.Copy(groupBytes, 0, wholePkg, header.Length + bsOffset.Length + bsDownBytes.Length,
                groupBytes.Length);
            Array.Copy(filenameBytes, 0, wholePkg,
                header.Length + bsOffset.Length + bsDownBytes.Length + groupBytes.Length, filenameBytes.Length);
            this.storageServer.getSocket().getOutputStream().write(wholePkg);
        }

        public bool isConnected()
        {
            return trackerServer.isConnected();
        }

        public bool isAvaliable()
        {
            return trackerServer.isAvaliable();
        }

        public void close()
        {
            trackerServer.close();
        }

        public TrackerServer getTrackerServer()
        {
            return trackerServer;
        }

        public void setTrackerServer(TrackerServer trackerServer)
        {
            this.trackerServer = trackerServer;
        }

        public StorageServer getStorageServer()
        {
            return storageServer;
        }

        public void setStorageServer(StorageServer storageServer)
        {
            this.storageServer = storageServer;
        }

        /**
         * Upload file by file buff
         *
         * @author Happy Fish / YuQing
         * @version Version 1.12
         */
        public class UploadBuff : IUploadCallback
        {
            private byte[] fileBuff;
            private int offset;
            private int Length;

            /**
             * constructor
             *
             * @param fileBuff the file buff for uploading
             */
            public UploadBuff(byte[] fileBuff, int offset, int Length)
            {
                this.fileBuff = fileBuff;
                this.offset = offset;
                this.Length = Length;
            }

            /**
             * send file content callback function, be called only once when the file uploaded
             *
             * @param out output stream for writing file content
             * @return 0 success, return none zero(errno) if fail
             */
            public int send(OutputStream out)
            {
                out.write(this.fileBuff, this.offset, this.Length);

                return 0;
            }
        }
    }
}