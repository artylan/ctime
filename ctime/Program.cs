using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

public class ProcessTime
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool GetProcessTimes(IntPtr handle,
                                              out FILETIME creation,
                                              out FILETIME exit,
                                              out FILETIME kernel,
                                              out FILETIME user);
}

class Programm
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: ctime program.exe [args]");
            return;
        }

        string program = args[0];
        string arguments = "";
        for (int i = 1; i < args.Length; i++)
        {
            arguments += args[i] + " ";
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = program,
            Arguments = arguments
        };

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();

                FILETIME ftCreation, ftExit, ftKernel, ftUser;

                ProcessTime.GetProcessTimes(process.Handle, out ftCreation, out ftExit, out ftKernel, out ftUser);

                Console.WriteLine("Real {0}", FiletimeToDateTime(ftExit) - FiletimeToDateTime(ftCreation));
                Console.WriteLine("User {0}", FiletimeToTimeSpan(ftUser));
                Console.WriteLine("Sys  {0}", FiletimeToTimeSpan(ftKernel));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public static DateTime FiletimeToDateTime(FILETIME fileTime)
    {
        ulong ft = unchecked((((ulong)(uint)fileTime.dwHighDateTime) << 32) | (uint)fileTime.dwLowDateTime);
        return DateTime.FromFileTimeUtc((long)ft);
    }

    public static TimeSpan FiletimeToTimeSpan(FILETIME fileTime)
    {
        ulong ft = unchecked((((ulong)(uint)fileTime.dwHighDateTime) << 32) | (uint)fileTime.dwLowDateTime);
        return TimeSpan.FromTicks((long)ft);
    }
}