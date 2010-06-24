using System;
using System.Collections;
using Microsoft.Samples.Debugging.MdbgEngine;

namespace StackDump
{
    public class Dumper
    {
        private static void Main(string[] args)
        {
            MDbgProcess proc = null;
            try
            {
                int processId = Int32.Parse(args[0]);
                MDbgEngine debugger = new MDbgEngine();
                proc = debugger.Attach(processId);
                InitializeMdbg(debugger, proc);
                DumpThreads(proc);
            }
            catch
            {
                PrintUsage();
            }
            finally
            {
                if (proc != null)
                {
                    // We always want to detach from target.
                    proc.Detach().WaitOne();
                }
            }
        }

        private static void InitializeMdbg(MDbgEngine debugger, MDbgProcess proc)
        {
            bool stopOnNewThread = debugger.Options.StopOnNewThread;
            debugger.Options.StopOnNewThread = false;
            proc.Go().WaitOne();
            debugger.Options.StopOnNewThread = true;
            while (proc.CorProcess.HasQueuedCallbacks(null))
            {
                proc.Go().WaitOne();
            }
            debugger.Options.StopOnNewThread = stopOnNewThread;
        }

        private static void DumpThreads(MDbgProcess proc)
        {
            foreach (MDbgThread thread in (IEnumerable)proc.Threads)
            {
                DumpThread(thread);
            }
        }

        private static void DumpThread(MDbgThread thread)
        {
            Console.Out.WriteLine(String.Format("OS Thread Id:{0}", thread.Id));
            foreach (MDbgFrame frame in thread.Frames)
            {
                if (!frame.IsInfoOnly)
                {
                    Console.Out.Write(String.Format("  {0}(",frame.Function.FullName));
                    
                    foreach (MDbgValue value2 in frame.Function.GetArguments(frame))
                    {
                        Console.Out.Write(value2.TypeName);
                    }
                    Console.Out.WriteLine(")");
                }
            }
        }

        private static void PrintUsage()
        {
            Console.Out.WriteLine("Usage:");
            Console.Out.WriteLine("StackDump <process id>");
        }
    }
}
