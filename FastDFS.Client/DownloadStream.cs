using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastDFS.Client
{
    public class DownloadStream : IDownloadCallback
    {
        private Stream _outputStream;
        private long currentBytes = 0;

        public DownloadStream(Stream stream)
        {
            _outputStream = stream;
        }

        public int recv(long fileSize, byte[] data, int bytes)
        {
            try
            {
                _outputStream.Write(data, 0, bytes);
            }
            catch (IOException ex)
            {
                return -1;
            }

            currentBytes += bytes;
            if (currentBytes == fileSize)
            {
                currentBytes = 0;
            }

            return 0;
        }
    }
}
