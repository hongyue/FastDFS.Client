using System;
using System.Collections.Generic;
using System.Text;

namespace FastDFS.Client
{
    public class FileInfo
    {
        protected string source_ip_addr;
        protected long file_size;
        protected DateTime create_timestamp;
        protected int crc32;

        public FileInfo(long file_size, int create_timestamp, int crc32, string source_ip_addr)
        {
            this.file_size = file_size;
            this.create_timestamp = DateTimeOffset.FromUnixTimeMilliseconds(create_timestamp * 1000L).DateTime;
            this.crc32 = crc32;
            this.source_ip_addr = source_ip_addr;
        }

        public string getSourceIpAddr()
        {
            return source_ip_addr;
        }

        /**
   * set the source ip address of the file uploaded to
   *
   * @param source_ip_addr the source ip address
   */
        public void setSourceIpAddr(string source_ip_addr)
        {
            this.source_ip_addr = source_ip_addr;
        }

        /**
         * get the file size
         *
         * @return the file size
         */
        public long getFileSize()
        {
            return file_size;
        }

        /**
         * set the file size
         *
         * @param file_size the file size
         */
        public void setFileSize(long fileSize)
        {
            file_size = fileSize;
        }

        /**
         * get the create timestamp of the file
         *
         * @return the create timestamp of the file
         */
        public DateTime getCreateTimestamp()
        {
            return create_timestamp;
        }

        /**
         * set the create timestamp of the file
         *
         * @param create_timestamp create timestamp in seconds
         */
        public void setCreateTimestamp(int millisecs)
        {
            create_timestamp = DateTimeOffset.FromUnixTimeMilliseconds(millisecs * 1000L).DateTime;
        }

        /**
         * get the file CRC32 signature
         *
         * @return the file CRC32 signature
         */
        public long getCrc32()
        {
            return this.crc32;
        }

        /**
         * set the create timestamp of the file
         *
         * @param crc32 the crc32 signature
         */
        public void setCrc32(int crc32)
        {
            this.crc32 = crc32;
        }

        public string toString()
        {
            return "source_ip_addr = " + source_ip_addr + ", " +
                   "file_size = " + file_size + ", " +
                   "create_timestamp = " + create_timestamp.ToString("yyyy-MM-dd HH:mm:ss") + ", " +
                   "crc32 = " + crc32;
        }
    }
}