using System;
using System.Collections.Generic;
using System.Text;

namespace FastDFS.Client
{
    public class FastDfsException : Exception
    {
        public FastDfsException() : base()
        {

        }

        public FastDfsException(string msg) : base(msg)
        {

        }
    }
}
