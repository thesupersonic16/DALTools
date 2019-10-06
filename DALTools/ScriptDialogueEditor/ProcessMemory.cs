using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDialogueEditor
{
    public static class ProcessMemory
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hProcess);


        public static int WriteBytes(this IntPtr handle, long address, byte[] block)
        {
            WriteProcessMemory(handle, new UIntPtr((uint)address), block, (uint)block.LongLength, out int bytesWritten);
            return bytesWritten;
        }

        public static byte[] ReadBytes(this IntPtr handle, long address, uint size)
        {
            var @out = new byte[size];
            bool a = ReadProcessMemory(handle, new UIntPtr((uint)address), @out, size, out int wtf);
            return @out;
        }

        // Read Methods
        public static int ReadInt32(this IntPtr handle, long address)
        {
            return BitConverter.ToInt32(ReadBytes(handle, address, 4), 0);
        }

        public static uint ReadUInt32(this IntPtr handle, long address)
        {
            return BitConverter.ToUInt32(ReadBytes(handle, address, 4), 0);
        }

        public static byte ReadByte(this IntPtr handle, long address)
        {
            return ReadBytes(handle, address, 1)[0];
        }

        // Write Methods
        public static int WriteInt32(this IntPtr handle, long address, int value)
        {
            return WriteBytes(handle, address, BitConverter.GetBytes(value));
        }

        public static int WriteUInt32(this IntPtr handle, long address, uint value)
        {
            return WriteBytes(handle, address, BitConverter.GetBytes(value));
        }

        public static int WriteUInt16(this IntPtr handle, long address, ushort value)
        {
            return WriteBytes(handle, address, BitConverter.GetBytes(value));
        }

        public static int WriteByte(this IntPtr handle, long address, byte value)
        {
            return WriteBytes(handle, address, new byte[] { value });
        }
    }
}
