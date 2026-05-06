using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IsaacEntityScannerRE.Services;

internal class ProcessInjector
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(int access, bool inherit, int pid);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint size, uint type, uint protect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out IntPtr written);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string name);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string name);

    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateRemoteThread(
        IntPtr hProcess,
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        uint dwCreationFlags,
        IntPtr lpThreadId);

    private const int PROCESS_ALL = 0x1F0FFF;

    public bool Inject(string processName, string dllPath)
    {
        var proc = Process.GetProcessesByName(processName.Replace(".exe", ""));
        if (proc.Length == 0)
            return false;

        var p = proc[0];

        IntPtr hProcess = OpenProcess(PROCESS_ALL, false, p.Id);
        if (hProcess == IntPtr.Zero)
            return false;

        byte[] dllBytes = Encoding.ASCII.GetBytes(dllPath + "\0");

        IntPtr alloc = VirtualAllocEx(
            hProcess,
            IntPtr.Zero,
            (uint)dllBytes.Length,
            0x3000, // MEM_COMMIT | MEM_RESERVE
            0x40    // PAGE_EXECUTE_READWRITE
        );

        if (alloc == IntPtr.Zero)
            return false;

        bool ok =  WriteProcessMemory(hProcess, alloc, dllBytes, (uint)dllBytes.Length, out _);

        if (!ok)
            return false;

        IntPtr kernel32 = GetModuleHandle("kernel32.dll");
        IntPtr loadLibrary = GetProcAddress(kernel32, "LoadLibraryA");

        IntPtr thread =  CreateRemoteThread(
            hProcess,
            IntPtr.Zero,
            0,
            loadLibrary,
            alloc,
            0,
            IntPtr.Zero
        );

        if (thread == IntPtr.Zero)
        {
            int err = Marshal.GetLastWin32Error();
            Console.WriteLine(Marshal.GetLastWin32Error());
            return false;
        }

        return true;
    }
}
