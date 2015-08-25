using System;
using System.IO;
using mega;

namespace ScheduledCameraUploadTaskAgent
{
    class MegaInputStream: MInputStream
    {
        private readonly Stream _inputStream;
        private long _offset;

        public MegaInputStream(Stream stream)
        {
            _inputStream = stream;
            _offset = 0;
        }

        public virtual ulong Length()
        {
            return (ulong)_inputStream.Length;
        }

        public virtual bool Read(byte[] buffer, ulong size)
        {
            if ((_offset + (long)size) > _inputStream.Length || (buffer != null && buffer.Length < (int)size))
            {
                return false;
            }

            if (buffer == null)
            {
                _offset += (long)size;
                return true;
            }

            _inputStream.Seek(_offset, SeekOrigin.Begin);

            int numBytesToRead = (int)size;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                int n = _inputStream.Read(buffer, numBytesRead, numBytesToRead);
                if (n == 0)
                {
                    return false;
                }

                numBytesRead += n;
                numBytesToRead -= n;
            }

            _offset += numBytesRead;
            return true;
        }
    }
}
