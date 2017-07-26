using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleProxyEncryption
{
    public static class DataExchenger
    {
        public static byte[] EncryptBytes(byte[] bytes, int count)
        {
            for (int i = 0; i < count; i++)
            {
                bytes[i]++;
            }
            return bytes;
        }

        public static byte[] DecryptBytes(byte[] bytes, int count)
        {
            for (int i = 0; i < count; i++)
            {
                bytes[i]--;
            }
            return bytes;
        }
    }
}
