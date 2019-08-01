using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastDFS.Client
{
    public class UploadStream : IUploadCallback
    {
        private Stream inputStream; //input stream for reading
        private long fileSize = 0; //size of the uploaded file

        /**
         * constructor
         *
         * @param inputStream input stream for uploading
         * @param fileSize    size of uploaded file
         */
        public UploadStream(Stream inputStream, long fileSize) : base()
        {
            this.inputStream = inputStream;
            this.fileSize = fileSize;
        }

        /**
         * send file content callback function, be called only once when the file uploaded
         *
         * @param out output stream for writing file content
         * @return 0 success, return none zero(errno) if fail
         */
        public int send(Stream outpuStream)
        {
            long remainBytes = fileSize;
            byte[] buff = new byte[256 * 1024];
            int bytes;
            while (remainBytes > 0)
            {
                try
                {
                    if ((bytes = inputStream.Read(buff, 0, remainBytes > buff.Length ? buff.Length : (int)remainBytes)) < 0)
                    {
                        return -1;
                    }
                }
                catch (IOException ex)
                {
                    return -1;
                }

                outpuStream.Write(buff, 0, bytes);
                remainBytes -= bytes;
            }

            return 0;
        }
    }
}