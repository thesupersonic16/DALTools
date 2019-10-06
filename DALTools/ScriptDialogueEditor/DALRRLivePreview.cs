using DALLib.File;
using DALLib.IO;
using DALLib.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDialogueEditor
{
    public class DALRRLivePreview
    {

        //public static uint ScriptInterpreter_Ptr = 0x005D5580;
        public static uint ScriptInterpreter_Ptr = 0x005D5644;

        public IntPtr ProcessHandle = IntPtr.Zero;

        private Process _process;

        public DALRRLivePreview()
        {

        }

        /// <summary>
        /// Creates a handle to the game's process so we can start reading and writing memory
        /// </summary>
        /// <returns>if the attachment is successful</returns>
        public bool Attach()
        {
            // Close old process
            if (ProcessHandle != IntPtr.Zero)
            {
                ProcessMemory.CloseHandle(ProcessHandle);
                ProcessHandle = IntPtr.Zero;
                _process = null;
            }

            // Find DAL: RR Process
            _process = Process.GetProcesses().FirstOrDefault(t => t.ProcessName.Contains("DATE A LIVE RIO-REINCARNATION"));
            if (_process == null)
                return false;
            // Get process handle
            ProcessHandle = ProcessMemory.OpenProcess(ProcessMemory.ProcessAccessFlags.All, false, _process.Id);
            if (ProcessHandle == IntPtr.Zero)
                return false;
            // Patch out the IP out of bounds check
            ProcessHandle.WriteUInt16(0x0048A5C2, 0x9090);
            return true;
        }

        /// <summary>
        /// Closes the process handle we opened at the start
        /// </summary>
        public void Detach()
        {
            if (ProcessHandle != IntPtr.Zero)
            {
                // Close the handle
                ProcessMemory.CloseHandle(ProcessHandle);
                // Set process handle to null
                ProcessHandle = IntPtr.Zero;
                _process = null;
            }
        }

        /// <summary>
        /// Builds and injects the modfied script into the game's memory
        /// </summary>
        /// <param name="file"></param>
        public void BuildAndInject(STSCFile file)
        {
            var interpreter = GetScriptInterpreter();
            using (var stream = new MemoryStream())
            {
                file.Save(stream);
                stream.Position = 0x3C;
                ProcessHandle.WriteBytes(interpreter.EntryPoint_addr, stream.CacheStream().ToArray());
            }
        }

        /// <summary>
        /// Reads the pointer and returns some information about the script and the position of the script that is running
        /// </summary>
        /// <returns>Information of the interpreter</returns>
        public ScriptInterpreter GetScriptInterpreter()
        {
            ScriptInterpreter interpreter = new ScriptInterpreter();
            if (ProcessHandle != IntPtr.Zero)
            {
                long scriptInterpreter_Addr         = ProcessHandle.ReadInt32(ScriptInterpreter_Ptr);
                scriptInterpreter_Addr              = ProcessHandle.ReadInt32(scriptInterpreter_Addr + 0x14);
                scriptInterpreter_Addr              = ProcessHandle.ReadInt32(scriptInterpreter_Addr + 0x08);
                interpreter.Header_addr             = ProcessHandle.ReadInt32(scriptInterpreter_Addr + 0);
                interpreter.EntryPoint_addr         = ProcessHandle.ReadInt32(scriptInterpreter_Addr + 4);
                interpreter.InstructionPointer_addr = (int)scriptInterpreter_Addr + 8;
            }
            return interpreter;
        }

        /// <summary>
        /// Sets the IP to a new MesWait with a following Goto command, then skips the wait. Make sure this is only called during a MesWait command
        /// </summary>
        /// <param name="address"></param>
        public void SetInstructionPointerMessage(int address)
        {
            var interpreter = GetScriptInterpreter();
            
            // Workaround for reloading messages
            ProcessHandle.WriteByte(interpreter.Header_addr - 50, 0x51); // MesWait
            ProcessHandle.WriteByte(interpreter.Header_addr - 49, 0x06); // Goto
            ProcessHandle.WriteInt32(interpreter.Header_addr - 48, address + 0x3C); // Goto Address

            // Set instruction pointer
            ProcessHandle.WriteInt32(interpreter.InstructionPointer_addr, interpreter.Header_addr - 50);
            
            // Skip MesWait
            var ptr = ProcessHandle.ReadInt32(0x005D5594) + 0x0746;
            ProcessHandle.WriteByte(ptr, (byte)(ProcessHandle.ReadByte(ptr) | 8));
        }

        // NOTE: Maybe we should check if the interpreter is on a MesWait?
        /// <summary>
        /// Checks if the game is ready and waiting on a message
        /// </summary>
        /// <returns>Game ready and waiting on a message (MesWait)</returns>
        public bool CheckGameReadyState()
        {
            var ptr = ProcessHandle.ReadInt32(0x005D5594) + 0x0746;
            return (ProcessHandle.ReadByte(ptr) & 6) == 6;
        }

        /// <summary>
        /// Gets the name of the script, This is different from the filename of the script
        /// </summary>
        /// <returns>The name of the script</returns>
        public string GetScriptName()
        {
            var interpreter = GetScriptInterpreter();
            byte[] buffer = ProcessHandle.ReadBytes(interpreter.Header_addr + 12, 0x20);
            return Encoding.ASCII.GetString(buffer).Trim('\0');
        }

        /// <summary>
        /// Sets the interpreter's instruction pointer. This function may cause more issues over <see cref="SetInstructionPointerMessage"/>
        /// </summary>
        /// <param name="address"></param>
        public void SetInstructionPointer(int address)
        {
            var interpreter = GetScriptInterpreter();
            
            // Set instruction pointer
            ProcessHandle.WriteInt32(interpreter.InstructionPointer_addr, interpreter.EntryPoint_addr + address);
        }

        public struct ScriptInterpreter
        {
            public int Header_addr;
            public int EntryPoint_addr;
            public int InstructionPointer_addr;
        }
    }
}
