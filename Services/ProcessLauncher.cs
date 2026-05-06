using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace IsaacEntityScannerRE.Services;

public class ProcessLauncher
{
    public Process? Process { get; private set; }

    public int ProcessId => Process?.Id ?? -1;

    public bool IsRunning => Process != null && !Process.HasExited;

    /// <summary>
    /// Launches process from full path to executable.
    /// </summary>
    public bool Launch(string exePath, string arguments = "")
    {
        if (string.IsNullOrWhiteSpace(exePath))
            return false;

        if (!File.Exists(exePath))
            return false;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            Process = Process.Start(startInfo);

            return Process != null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Process launch failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Attempts to find already running process by name.
    /// </summary>
    public bool Attach(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
            return false;

        try
        {
            var name = Path.GetFileNameWithoutExtension(processName);

            var proc = Process.GetProcessesByName(name);

            if (proc.Length == 0)
                return false;

            Process = proc[0];
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Attach failed: {ex.Message}");
            return false;
        }
    }

    public void Kill()
    {
        try
        {
            if (Process != null && !Process.HasExited)
                Process.Kill();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Kill failed: {ex.Message}");
        }
    }
}
