using DALLib.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.IO
{
    // Based off HedgeLib's ExtendedBinary modified to better suit working with DAL
    public class ExtendedBinaryReader : BinaryReader
    {
        public uint Offset = 0;
        public bool IsBigEndian = false;

        protected Stream stream;
        protected byte[] buffer;
        protected byte[] bufferChar;
        protected char[] bufferSingleChar;
        protected Decoder decoder;

        protected const int MinBufferSize = 16;

        public override Stream BaseStream => stream;

        // Constructors
        public ExtendedBinaryReader(Stream input, bool isBigEndian = false) :
            this(input, Encoding.UTF8, isBigEndian)
        { }

        public ExtendedBinaryReader(Stream input, Encoding encoding,
            bool isBigEndian = false) : base(input, encoding, false)
        {
            stream = input;
            IsBigEndian = isBigEndian;

            int bufferSize = encoding.GetMaxByteCount(1);
            if (bufferSize < MinBufferSize)
                bufferSize = MinBufferSize;

            buffer = new byte[bufferSize];
            bufferChar = new byte[encoding.GetMaxByteCount(1)];
            bufferSingleChar = new char[1];
            decoder = encoding.GetDecoder();
        }

        // Methods
        public long GetLength()
        {
            return stream.Length;
        }

        public long GetPosition()
        {
            return stream.Position;
        }

        /// <summary>
        /// Sets the Extended Stream
        /// </summary>
        /// <param name="stream"></param>
        public void SetStream(Stream stream)
        {
            this.stream = stream;
        }

        public void SetEndian(bool isBigEndian)
        {
            IsBigEndian = isBigEndian;
        }

        public void JumpTo(long position, bool absolute = true)
        {
            stream.Position = (absolute) ? position : position + Offset;
        }

        public void JumpTo(StreamBlock block, bool absolute = true)
        {
            stream.Position = (absolute) ? block.Position : block.Position + Offset;
        }

        public void JumpAhead(long amount = 1)
        {
            stream.Position += amount;
        }

        public void JumpBehind(long amount = 1)
        {
            stream.Position -= amount;
        }

        public void FixPadding(uint amount = 4)
        {
            if (amount < 1) return;

            long jumpAmount = 0;
            while ((stream.Position + jumpAmount) % amount != 0) ++jumpAmount;
            JumpAhead(jumpAmount);
        }

        public string GetString(bool isAbsolute = false, bool isNullTerminated = true)
        {
            uint offset = (isAbsolute) ? ReadUInt32() : ReadUInt32() + Offset;
            return GetString(offset, isNullTerminated);
        }

        public string GetString(uint offset, bool isNullTerminated = true)
        {
            long curPos = stream.Position;
            stream.Position = offset;

            string str = (isNullTerminated) ?
                ReadNullTerminatedString() : ReadString();

            stream.Position = curPos;
            return str;
        }

        public string ReadSignature(int length = 4)
        {
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        public string PeekSignature(int length = 4)
        {
            string result = Encoding.ASCII.GetString(ReadBytes(length));
            JumpBehind(length);
            return result;
        }

        public string ReadTrimmedString(int length = 4)
        {
            return ReadSignature(length).Trim('\0');
        }

        public string ReadStringElsewhere(int position = 0, bool absolute = true)
        {
            long oldPos = stream.Position;
            try
            {
                if (position == 0)
                    JumpTo(ReadInt32(), absolute);
                else
                    JumpTo(position, absolute);
                if (stream.Position == (absolute ? 0 : Offset))
                {
                    if (position == 0)
                        JumpTo(oldPos + 4);
                    else
                        JumpTo(oldPos);
                    return null;
                }
                if (Offset > stream.Position)
                    Offset = (uint)stream.Position;
                string s = ReadNullTerminatedString();
                if (position == 0)
                    JumpTo(oldPos + 4);
                else
                    JumpTo(oldPos);
                return s;
            }
            catch
            {
                JumpTo(oldPos);
                return "";
            }
        }

        public string ReadNullTerminatedStringPointer()
        {
            long oldPos = stream.Position;
            try
            {
                JumpTo(ReadInt32());
                JumpTo(ReadInt32());
                if (Offset > stream.Position)
                    Offset = (uint)stream.Position;
                string s = ReadNullTerminatedString();
                JumpTo(oldPos + 4);
                return s;
            }
            catch
            {
                JumpTo(oldPos);
                return "";
            }
        }

        public object ReadByType(Type type)
        {
            if (type == typeof(bool))
                return ReadBoolean();
            else if (type == typeof(byte))
                return ReadByte();
            else if (type == typeof(sbyte))
                return ReadSByte();
            else if (type == typeof(char))
                return ReadChar();
            else if (type == typeof(short))
                return ReadInt16();
            else if (type == typeof(ushort))
                return ReadUInt16();
            else if (type == typeof(int))
                return ReadInt32();
            else if (type == typeof(uint))
                return ReadUInt32();
            else if (type == typeof(float))
                return ReadSingle();
            else if (type == typeof(long))
                return ReadInt64();
            else if (type == typeof(ulong))
                return ReadUInt64();
            else if (type == typeof(double))
                return ReadDouble();
            else if (type == typeof(string))
                return ReadStringElsewhere();

            return null;
        }

        public long ReadVariableSizeInt(int size)
        {
            switch (size)
            {
                case 1:
                    return ReadByte();
                case 2:
                    return ReadUInt16();
                case 3:
                    return ReadInt24();
                case 4:
                    return ReadInt32();
                default:
                    return ReadUInt32();
            }
        }

        public T ReadStruct<T>(Type LengthType = null)
        {
            return (T)ReadStruct(typeof(T), LengthType);
        }

        public object ReadStruct(Type type, Type LengthType = null)
        {
            if (LengthType == null)
                LengthType = typeof(byte);
            if (type == typeof(string))
                return ReadStringElsewhere();
            object structure = Activator.CreateInstance(type);
            var fields = from prop in type.GetProperties()
                         orderby prop.MetadataToken ascending
                         select prop;
            foreach (var field in fields)
            {
                if (field.PropertyType.IsArray)
                {
                    var array = Array.CreateInstance(field.PropertyType.GetElementType(),
                        (int)Convert.ChangeType(ReadByType(LengthType), typeof(int)));
                    for (int i = 0; i < array.Length; ++i)
                        array.SetValue(ReadStruct(field.PropertyType.GetElementType(), LengthType), i);
                    field.SetValue(structure, array);
                }
                else
                {
                    object value = ReadByType(field.PropertyType);
                    if (value != null)
                    {
                        field.SetMethod?.Invoke(structure, new [] { value });
                    }
                    else
                    {
                        if (field.PropertyType.IsEnum)
                        {
                            int size = 4;
                            var attb = field.GetCustomAttributes(true).FirstOrDefault(t => t.GetType() == typeof(DataSizeAttribute));
                            if (attb is DataSizeAttribute dataSizeAttb)
                                size = dataSizeAttb.Size;
                            field.SetMethod?.Invoke(structure, new[] { Enum.ToObject(field.PropertyType, ReadVariableSizeInt(size)) });
                        }
                        else
                            structure = ReadStruct(field.PropertyType, LengthType);
                    }
                }
            }
            return structure;
        }

        public byte[] ReadArrayRange(int start, int end)
        {
            long oldPos = stream.Position;
            try
            {
                JumpTo(start);
                if (Offset > stream.Position)
                    Offset = (uint)stream.Position;
                int length = end - start;
                byte[] bytes = ReadBytes(length);
                JumpTo(oldPos);
                return bytes;
            }
            catch
            {
                JumpTo(oldPos);
                return new byte[0] { };
            }
        }

        public string ReadNullTerminatedString()
        {
            var sb = new StringBuilder();
            long len = stream.Length;
            char curChar;

            do
            {
                curChar = ReadChar();
                if (curChar == 0) break;
                sb.Append(curChar);
            }
            while (stream.Position < len);
            return sb.ToString();
        }

        public int CheckDALSignature(string expected)
        {
            // Read and check the signature
            string sig = ReadSignature(expected.Length);
            if (sig != expected)
                return 0;
            // Calculate Size of Padding
            int padding = 0;
            while (true)
            {
                if (ReadByte() != 0x20)
                {
                    if (expected.Length + padding >= 0x14)
                        JumpBehind((expected.Length + padding) - 0x14 + 1);
                    else if (expected.Length + padding >= 0x0C)
                        JumpBehind((expected.Length + padding) - 0x0C + 1);
                    else if (expected.Length + padding >= 0x08)
                        JumpBehind((expected.Length + padding) - 0x08 + 1);
                    break;
                }
                ++padding;
            }
            // Total length of the signature
            return expected.Length + padding;
        }

        public string ReadDALSignature(string expected)
        {
            // Read and check the signature
            string sig = ReadSignature(expected.Length);
            if (sig != expected)
                return sig;
            // Calculate Size of Padding
            int padding = 0;
            while (true)
            {
                if (ReadByte() != 0x20)
                {
                    if (expected.Length + padding >= 0x14)
                        JumpBehind((expected.Length + padding) - 0x14 + 1);
                    else if (expected.Length + padding >= 0x0C)
                        JumpBehind((expected.Length + padding) - 0x0C + 1);
                    else if (expected.Length + padding >= 0x08)
                        JumpBehind((expected.Length + padding) - 0x08 + 1);
                    break;
                }
                ++padding;
            }
            return sig + new string(' ', padding);
        }

        // 1-Byte Types
        public override char ReadChar()
        {
            return (char)InternalReadOneChar();
        }

        public override bool ReadBoolean()
        {
            return (ReadByte() != 0);
        }

        // 2-Byte Types
        public override short ReadInt16()
        {
            FillBuffer(sizeof(short));
            return (IsBigEndian) ?
                (short)(buffer[0] << 8 | buffer[1]) :
                (short)(buffer[1] << 8 | buffer[0]);
        }

        public override ushort ReadUInt16()
        {
            FillBuffer(sizeof(ushort));
            return (IsBigEndian) ?
                (ushort)(buffer[0] << 8 | buffer[1]) :
                (ushort)(buffer[1] << 8 | buffer[0]);
        }

        // 3-Byte Types
        public int ReadInt24()
        {
            FillBuffer(3);
            return (IsBigEndian) ?
                buffer[0] << 16 | buffer[1] << 8 | buffer[2] :
                buffer[2] << 16 | buffer[1] << 8 | buffer[0];
        }

        // 4-Byte Types
        public override int ReadInt32()
        {
            FillBuffer(sizeof(int));
            return (IsBigEndian) ?
                buffer[0] << 24 | buffer[1] << 16 |
                    buffer[2] << 8 | buffer[3] :
                buffer[3] << 24 | buffer[2] << 16 |
                    buffer[1] << 8 | buffer[0];
        }

        public override uint ReadUInt32()
        {
            FillBuffer(sizeof(uint));
            return (IsBigEndian) ?
                ((uint)buffer[0] << 24 | (uint)buffer[1] << 16 |
                    (uint)buffer[2] << 8 | buffer[3]) :
                ((uint)buffer[3] << 24 | (uint)buffer[2] << 16 |
                    (uint)buffer[1] << 8 | buffer[0]);
        }

        public override unsafe float ReadSingle()
        {
            uint v = ReadUInt32();
            return *((float*)&v);
        }

        // 8-Byte Types
        public override long ReadInt64()
        {
            FillBuffer(sizeof(long));
            return (IsBigEndian) ?
                ((long)buffer[0] << 56 | (long)buffer[1] << 48 |
                    (long)buffer[2] << 40 | (long)buffer[3] << 32 |
                    (long)buffer[4] << 24 | (long)buffer[5] << 16 |
                    (long)buffer[6] << 8 | buffer[7]) :

                ((long)buffer[7] << 56 | (long)buffer[6] << 48 |
                    (long)buffer[5] << 40 | (long)buffer[4] << 32 |
                    (long)buffer[3] << 24 | (long)buffer[2] << 16 |
                    (long)buffer[1] << 8 | buffer[0]);
        }

        public override ulong ReadUInt64()
        {
            FillBuffer(sizeof(ulong));
            return (IsBigEndian) ?
                ((ulong)buffer[0] << 56 | (ulong)buffer[1] << 48 |
                    (ulong)buffer[2] << 40 | (ulong)buffer[3] << 32 |
                    (ulong)buffer[4] << 24 | (ulong)buffer[5] << 16 |
                    (ulong)buffer[6] << 8 | buffer[7]) :

                ((ulong)buffer[7] << 56 | (ulong)buffer[6] << 48 |
                    (ulong)buffer[5] << 40 | (ulong)buffer[4] << 32 |
                    (ulong)buffer[3] << 24 | (ulong)buffer[2] << 16 |
                    (ulong)buffer[1] << 8 | buffer[0]);
        }

        public override unsafe double ReadDouble()
        {
            ulong v = ReadUInt64();
            return *((double*)&v);
        }

        public override byte[] ReadBytes(int count)
        {
            byte[] bytes = new byte[count];
            stream.Read(bytes, 0, count);
            return bytes;
        }

        public override byte ReadByte()
        {
            return (byte)stream.ReadByte();
        }

        protected override void FillBuffer(int numBytes)
        {
            int n = 0, bytesRead = 0;
            if (stream == null)
            {
                throw new ObjectDisposedException(
                    "stream", "Cannot read; the Stream has been closed!");
            }

            if (numBytes == 1)
            {
                n = stream.ReadByte();
                if (n == -1)
                    throw new EndOfStreamException("Cannot read; the end of the Stream has been reached!");

                buffer[0] = (byte)n;
                return;
            }

            do
            {
                n = stream.Read(buffer, bytesRead, numBytes);
                if (n == 0)
                    throw new EndOfStreamException("Cannot read; the end of the Stream has been reached!");

                bytesRead += n;
                numBytes -= n;
            }
            while (numBytes > 0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && stream != null)
                stream.Close();

            stream = null;
            buffer = null;
        }

        private int InternalReadOneChar()
        {
            int charsRead = 0;
            decoder.Reset();
            while (charsRead == 0)
            {

                int r = stream.ReadByte();
                bufferChar[0] = (byte)r;
                try
                {
                    charsRead = decoder.GetChars(bufferChar, 0, 1, bufferSingleChar, 0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return (char)bufferChar[0];
                }
            }
            return bufferSingleChar[0];
        }

    }

    public class ExtendedBinaryWriter : BinaryWriter
    {
        // Variables/Constants
        public uint Offset = 0;
        public bool IsBigEndian = false;

        protected Dictionary<string, uint> offsets = new Dictionary<string, uint>();
        protected byte[] dataBuffer = new byte[BufferSize];
        protected Encoding encoding;
        protected const uint BufferSize = 16;

        // Constructors
        public ExtendedBinaryWriter(Stream output, bool isBigEndian = false) :
            this(output, Encoding.UTF8, false)
        {
            IsBigEndian = isBigEndian;
        }

        public ExtendedBinaryWriter(Stream output, Encoding encoding,
            bool isBigEndian = false) : base(output, encoding, false)
        {
            IsBigEndian = isBigEndian;
            this.encoding = encoding;
        }

        // Methods
        public void JumpTo(long position, bool absolute = true)
        {
            OutStream.Position = (absolute) ? position : position + Offset;
        }

        public void JumpAhead(long amount = 1)
        {
            OutStream.Position += amount;
        }

        public void JumpBehind(long amount = 1)
        {
            OutStream.Position -= amount;
        }

        public void SetEndian(bool isBigEndian)
        {
            IsBigEndian = isBigEndian;
        }

        public virtual void AddOffset(string name, uint offsetLength = 4)
        {
            if (offsets.ContainsKey(name))
                offsets[name] = (uint)BaseStream.Position;
            else
                offsets.Add(name, (uint)BaseStream.Position);

            WriteNulls(offsetLength);
        }

        public virtual bool HasOffset(string name)
        {
            return offsets.ContainsKey(name);
        }

        public virtual void FillInOffset(string name,
            bool absolute = true, bool removeOffset = true)
        {
            if (!HasOffset(name))
                return;
            long curPos = OutStream.Position;
            WriteOffsetValueAtPos(offsets[name], (uint)curPos, absolute);

            if (removeOffset)
                offsets.Remove(name);

            OutStream.Position = curPos;
        }

        public virtual void FillInOffsetLong(string name,
            bool absolute = true, bool removeOffset = true)
        {
            if (!HasOffset(name))
                return;
            long curPos = OutStream.Position;
            WriteOffsetValueAtPos(offsets[name], (ulong)curPos, absolute);

            if (removeOffset)
                offsets.Remove(name);

            OutStream.Position = curPos;
        }

        public virtual void FillInOffset(string name, uint value,
            bool absolute = true, bool removeOffset = true)
        {
            if (!HasOffset(name))
                return;
            long curPos = OutStream.Position;
            WriteOffsetValueAtPos(offsets[name], value, absolute);

            if (removeOffset)
                offsets.Remove(name);

            OutStream.Position = curPos;
        }

        public virtual void FillInOffset(string name, ulong value,
            bool absolute = true, bool removeOffset = true)
        {
            if (!HasOffset(name))
                return;
            long curPos = OutStream.Position;
            WriteOffsetValueAtPos(offsets[name], value, absolute);

            if (removeOffset)
                offsets.Remove(name);

            OutStream.Position = curPos;
        }

        protected virtual void WriteOffsetValueAtPos(
            uint pos, uint value, bool absolute = true)
        {
            OutStream.Position = pos;
            Write((absolute) ? value : value - Offset);
        }

        protected virtual void WriteOffsetValueAtPos(
            long pos, ulong value, bool absolute = true)
        {
            OutStream.Position = pos;
            Write((absolute) ? value : value - Offset);
        }

        /// <summary>
        /// Sets the Extended Stream
        /// </summary>
        /// <param name="stream"></param>
        public void SetStream(Stream stream)
        {
            OutStream = stream;
        }

        public void WriteDALSignature(string sig, bool smallSig)
        {
            WriteSignature(sig + new string(' ', (smallSig ? 0x08 : 0x14) - sig.Length));
        }

        public void WriteNull()
        {
            OutStream.WriteByte(0);
        }

        public void WriteNulls(uint count)
        {
            Write(new byte[count]);
        }

        public void WriteNullTerminatedString(string value)
        {
            Write(encoding.GetBytes(value));
            OutStream.WriteByte(0);
        }
        
        public void FixPadding(uint amount = 4)
        {
            if (amount < 1) return;

            uint padAmount = 0;
            while ((OutStream.Position + padAmount) % amount != 0) ++padAmount;
            WriteNulls(padAmount);
        }

        public void WriteSignature(string signature)
        {
            Write(encoding.GetBytes(signature));
        }
        
        // 2-Byte Types
        public override void Write(short value)
        {
            if (IsBigEndian)
            {
                dataBuffer[0] = (byte)(value >> 8);
                dataBuffer[1] = (byte)(value);
            }
            else
            {
                dataBuffer[0] = (byte)(value);
                dataBuffer[1] = (byte)(value >> 8);
            }

            OutStream.Write(dataBuffer, 0, sizeof(short));
        }

        public override void Write(ushort value)
        {
            if (IsBigEndian)
            {
                dataBuffer[0] = (byte)(value >> 8);
                dataBuffer[1] = (byte)(value);
            }
            else
            {
                dataBuffer[0] = (byte)(value);
                dataBuffer[1] = (byte)(value >> 8);
            }

            OutStream.Write(dataBuffer, 0, sizeof(ushort));
        }

        // 4-Byte Types
        public override void Write(int value)
        {
            if (IsBigEndian)
            {
                dataBuffer[0] = (byte)(value >> 24);
                dataBuffer[1] = (byte)(value >> 16);
                dataBuffer[2] = (byte)(value >> 8);
                dataBuffer[3] = (byte)(value);
            }
            else
            {
                dataBuffer[0] = (byte)(value);
                dataBuffer[1] = (byte)(value >> 8);
                dataBuffer[2] = (byte)(value >> 16);
                dataBuffer[3] = (byte)(value >> 24);
            }

            OutStream.Write(dataBuffer, 0, sizeof(int));
        }

        public override void Write(uint value)
        {
            if (IsBigEndian)
            {
                dataBuffer[0] = (byte)(value >> 24);
                dataBuffer[1] = (byte)(value >> 16);
                dataBuffer[2] = (byte)(value >> 8);
                dataBuffer[3] = (byte)(value);
            }
            else
            {
                dataBuffer[0] = (byte)(value);
                dataBuffer[1] = (byte)(value >> 8);
                dataBuffer[2] = (byte)(value >> 16);
                dataBuffer[3] = (byte)(value >> 24);
            }

            OutStream.Write(dataBuffer, 0, sizeof(uint));
        }

        public override unsafe void Write(float value)
        {
            Write(*((uint*)&value));
        }

        // 8-Byte Types
        public override void Write(long value)
        {
            if (IsBigEndian)
            {
                dataBuffer[0] = (byte)(value >> 56);
                dataBuffer[1] = (byte)(value >> 48);
                dataBuffer[2] = (byte)(value >> 40);
                dataBuffer[3] = (byte)(value >> 32);

                dataBuffer[4] = (byte)(value >> 24);
                dataBuffer[5] = (byte)(value >> 16);
                dataBuffer[6] = (byte)(value >> 8);
                dataBuffer[7] = (byte)(value);
            }
            else
            {
                dataBuffer[0] = (byte)(value);
                dataBuffer[1] = (byte)(value >> 8);
                dataBuffer[2] = (byte)(value >> 16);
                dataBuffer[3] = (byte)(value >> 24);

                dataBuffer[4] = (byte)(value >> 32);
                dataBuffer[5] = (byte)(value >> 40);
                dataBuffer[6] = (byte)(value >> 48);
                dataBuffer[7] = (byte)(value >> 56);
            }

            OutStream.Write(dataBuffer, 0, sizeof(long));
        }

        public override void Write(ulong value)
        {
            if (IsBigEndian)
            {
                dataBuffer[0] = (byte)(value >> 56);
                dataBuffer[1] = (byte)(value >> 48);
                dataBuffer[2] = (byte)(value >> 40);
                dataBuffer[3] = (byte)(value >> 32);

                dataBuffer[4] = (byte)(value >> 24);
                dataBuffer[5] = (byte)(value >> 16);
                dataBuffer[6] = (byte)(value >> 8);
                dataBuffer[7] = (byte)(value);
            }
            else
            {
                dataBuffer[0] = (byte)(value);
                dataBuffer[1] = (byte)(value >> 8);
                dataBuffer[2] = (byte)(value >> 16);
                dataBuffer[3] = (byte)(value >> 24);

                dataBuffer[4] = (byte)(value >> 32);
                dataBuffer[5] = (byte)(value >> 40);
                dataBuffer[6] = (byte)(value >> 48);
                dataBuffer[7] = (byte)(value >> 56);
            }

            OutStream.Write(dataBuffer, 0, sizeof(ulong));
        }

        public override unsafe void Write(double value)
        {
            Write(*((ulong*)&value));
        }

        public void WriteByType(object value)
        {
            var type = value.GetType();
            if (type == typeof(bool))
                Write((bool)value);
            else if (type == typeof(byte))
                Write((byte)value);
            else if (type == typeof(sbyte))
                Write((sbyte)value);
            else if (type == typeof(char))
                Write((char)value);
            else if (type == typeof(short))
                Write((short)value);
            else if (type == typeof(ushort))
                Write((ushort)value);
            else if (type == typeof(int))
                Write((int)value);
            else if (type == typeof(uint))
                Write((uint)value);
            else if (type == typeof(float))
                Write((float)value);
            else if (type == typeof(long))
                Write((long)value);
            else if (type == typeof(ulong))
                Write((ulong)value);
            else if (type == typeof(double))
                Write((double)value);
            else
                throw new NotImplementedException("Attempt to write unknown type: " + type);
        }

    }

    public class StreamBlock
    {
        public long Position;
        public long Length;

        public StreamBlock(long position, long length)
        {
            Position = position;
            Length = length;
        }
    }
}
