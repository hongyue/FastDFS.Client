using System;
using System.Collections.Generic;
using System.Text;

namespace FastDFS.Client
{
    public abstract class StructBase
    {
        /**
   * set fields
   *
   * @param bs     byte array
   * @param offset start offset
   */
        public abstract void setFields(byte[] bs, int offset);

        protected String stringValue(byte[] bs, int offset, FieldInfo filedInfo)
        {
            var encoder = Encoding.GetEncoding(ClientGlobal.g_charset);
            return encoder.GetString(bs, offset + filedInfo.offset, filedInfo.size).Trim('\0');
        }

        protected long longValue(byte[] bs, int offset, FieldInfo filedInfo)
        {
            return ProtoCommon.buff2long(bs, offset + filedInfo.offset);
        }

        protected int intValue(byte[] bs, int offset, FieldInfo filedInfo)
        {
            return (int)ProtoCommon.buff2long(bs, offset + filedInfo.offset);
        }

        protected int int32Value(byte[] bs, int offset, FieldInfo filedInfo)
        {
            return ProtoCommon.buff2int(bs, offset + filedInfo.offset);
        }

        protected byte byteValue(byte[] bs, int offset, FieldInfo filedInfo)
        {
            return bs[offset + filedInfo.offset];
        }

        protected bool booleanValue(byte[] bs, int offset, FieldInfo filedInfo)
        {
            return bs[offset + filedInfo.offset] != 0;
        }

        protected DateTime dateValue(byte[] bs, int offset, FieldInfo filedInfo)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(
                    ProtoCommon.buff2long(bs, offset + filedInfo.offset) * 1000).DateTime;
        }

        protected class FieldInfo
        {
            public String name;
            public int offset;
            public int size;

            public FieldInfo(String name, int offset, int size)
            {
                this.name = name;
                this.offset = offset;
                this.size = size;
            }
        }
    }
}