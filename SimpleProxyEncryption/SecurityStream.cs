using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleProxyEncryption
{
    public class SecurityStream : IDisposable
    {
        public bool IsEnabled { get; set; } = true;
        public SecurityStream(Stream stream)
        {
            BaseStream = stream;
        }

        public Stream BaseStream { get; set; }

        public bool CanRead => BaseStream.CanRead;

        public bool CanSeek => BaseStream.CanSeek;

        public bool CanWrite => BaseStream.CanWrite;

        public long Length => BaseStream.Length;

        public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }
        public int ReadTimeout { get => BaseStream.ReadTimeout; set => BaseStream.ReadTimeout = value; }

        public void Flush()
        {
            BaseStream.Flush();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var readCount = BaseStream.Read(buffer, offset, count);
            if (IsEnabled)
                DataExchenger.DecryptBytes(buffer, count);
            return readCount;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (IsEnabled)
                DataExchenger.EncryptBytes(buffer, count);
            BaseStream.Write(buffer, offset, count);
        }

        public void Close()
        {
            BaseStream.Close();
        }

        public void Dispose()
        {
            BaseStream.Dispose();
        }
    }
}
