using System;
using System.Collections.Generic;
using System.Text;

namespace FastDFS.Client
{
    public interface IDownloadCallback
    {
        int recv(long file_size, byte[] data, int bytes);
    }
}
