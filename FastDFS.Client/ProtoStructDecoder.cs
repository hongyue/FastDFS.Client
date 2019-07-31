using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastDFS.Client
{
    public class ProtoStructDecoder<T> where T : StructBase
    {
        public ProtoStructDecoder()
        {
        }

        /**
         * decode byte buffer
         */
        public T[] decode<T>(byte[] bs, int fieldsTotalSize) where T: StructBase
        {
            if (bs.Length % fieldsTotalSize != 0)
            {
                throw new IOException("byte array length: " + bs.Length + " is invalid!");
            }

            int count = bs.Length / fieldsTotalSize;
            int offset;
            var results = new T[count];

            offset = 0;
            foreach (var c in results)
            {
                c.setFields(bs, offset);
                offset += fieldsTotalSize;
            }

            return results;
        }
    }
}