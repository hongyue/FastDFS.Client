using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastDFS.Client
{
    public interface IUploadCallback
    {
        int send(Stream outputStream);
    }
}
