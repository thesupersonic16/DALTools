using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.IO
{
    public class VirtualStream : Stream
    {

        private Stream _internalStream;
        private bool _keepOpen;
        
        public long NewLength = 0;
        public long NewPosition = 0;

        public override bool CanRead => _internalStream.CanRead;

        public override bool CanSeek => _internalStream.CanSeek;

        public override bool CanWrite => _internalStream.CanWrite;

        public override long Length => NewLength;

        public override long Position
        {
            get
            {
                return _internalStream.Position - NewPosition;
            }
            set
            {
                _internalStream.Position = value + NewPosition;
            }
        }

        public VirtualStream(Stream underlayingStream, bool keepOpen)
        {
            _internalStream = underlayingStream;
            _keepOpen = keepOpen;
            NewPosition = underlayingStream.Position;
        }

        public VirtualStream(Stream underlayingStream, long position, long length, bool keepOpen)
        {
            _internalStream = underlayingStream;
            _keepOpen = keepOpen;
            NewPosition = position;
            NewLength = length;
        }

        public override void Flush()
        {
            _internalStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _internalStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return _internalStream.Seek(offset + NewPosition, SeekOrigin.Begin);
                case SeekOrigin.Current:
                    return _internalStream.Seek(offset, SeekOrigin.Current);
                case SeekOrigin.End:
                    return _internalStream.Seek(NewPosition + NewLength - offset, SeekOrigin.Begin);
            }
            // Incase for whatever reason we got a origin thats not part of the enum
            return _internalStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _internalStream.SetLength(value);
            NewLength = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _internalStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            base.Close();
            if (!_keepOpen)
                _internalStream.Close();
        }
    }
}
