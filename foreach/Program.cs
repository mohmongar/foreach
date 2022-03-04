using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;

namespace forEach
{
    class Program
    {
        const string defaultTermStr = "$$";
        const string defaultInsertStr = "$";
        static void usage()
        {
            Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            AssemblyName product = asm.GetName();
            Version version = product.Version;
            AssemblyCopyrightAttribute copyright = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;

            Console.Write(
              product.Name.ToString()
               + " Ver." + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Revision.ToString()
               + " " + copyright.Copyright + " mohmongar@mohmongar.net\n" +
               "usage : foreach.exe [option...] command [commandoption...] para1 para2 para3 ... \n" +
               "       It executes one command at a time using given parameters. \n"+
               "\n"+
               "option: -W[t]  wait t sec after all command. if t is zero or nothing, then wait till press key.\n"+
               "\n"+
               "        -T[x]  Set terminal sign. \n"+
               "               All the character strings to this [x] are taken as the option always attached to command instruction execution time.\n" +
               "               If x is nothing, then $$ is default sign.\n"+
               "               see example 2\n" +
               "\n" +
               "        -I[x]  Set insert sign.\n" +
               "               A parameter is inserted in the position of this sign of a command line at instruction execution time.\n"+
               "               If x is nothing, then $ is default sign.\n" +
               "               see example 3\n"+
               "\n" +
               "        -X     After command execution, if exitcode is except zero, processing will be interrupted. \n" +
               "\n" +
               "  (example 1)\n" +
               "       C:>foreach cmd file1 file2\n" +
               "       cmd file1\n" +
               "       cmd file2\n" +
               "       C:>\n" +
               "  (example 2)\n" +
               "       C:>foreach -T$$ cmd -S $$ file1 file2\n" +
               "       cmd -S file1\n" +
               "       cmd -S file2\n" +
               "       C:>\n"+ 
               "  (example 3)\n" +
               "       C:>foreach -I$ -T$$ cmd -S $ -E $$ file1 file2\n" +
               "       cmd -S file1 -E \n" +
               "       cmd -S file2 -E \n" +
               "       C:>\n" 
                );
            Environment.Exit(-1);
        }
        
        static void Main(string[] args)
        {
            bool waitKey = false;
            int waitSec = 0;
            bool exitFlag = false;
            queueType<string> argv = new queueType<string>();
            argv.queue(args);
            string insertStr = defaultInsertStr;
            string termStr ="";
            string cmd;

            for (;;) {
                cmd = argv.dequeue();
                if (cmd == null)
                {
                    usage();
                }
                else if (cmd[0] == '-' || cmd[0] == '/') {
                    char option = char.ToUpper(cmd[1]);
                    if(option == 'h' || option == '?') {
                        usage();
                    }
                    else if (option == 'W')
                    {
                        int.TryParse(cmd.Substring(2), out waitSec);
                        waitKey = true;
                    }
                    else if (option == 'T')
                    {
                        termStr = cmd.Substring(2);
                        if (termStr == "")
                        {
                            termStr = defaultTermStr;
                        }
                    }
                    else if (option == 'I')
                    {
                        insertStr = cmd.Substring(2);
                        if (termStr == "")
                        {
                            termStr = defaultInsertStr;
                        }
                    }
                    else if (option == 'X')
                    {
                        exitFlag = true;
                    }
                    else
                    {
                        usage();
                    }
                }
                else{
                    break;
                }
            }

            List<string> parameters = new List<string>();

            int pid = -1;
            if (termStr != "") {
                string parameter;
                while ((parameter = argv.dequeue()) != termStr)
                {
                    if (parameter == null)
                    {
                        Console.WriteLine("foreach: not found terminal string: "+termStr);
                        Environment.Exit(-1);
                    }
                    if (parameter == insertStr)
                    {
                        pid = parameters.Count;
                    }
                    parameters.Add(parameter);
                }
                if (pid < 0)
                {
                    pid = parameters.Count;
                    parameters.Add("$");
                }
            }
            else {
                pid = parameters.Count;
                parameters.Add("$");
            }
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = false;
            
            string sendto;
            int count = 0;
            int argnum = argv.Count - argv.pointer;
            while ((sendto = argv.dequeue()) != null) {
                if (sendto[0] != '"')
                {
                    sendto = "\"" + sendto + "\"";
                }

                parameters[pid] = sendto;
                proc.StartInfo.Arguments = string.Join(" ", parameters);
                
                Console.WriteLine(cmd + " " + proc.StartInfo.Arguments);
                try
                {
                    proc.Start();
                }
                catch (Exception err)
                {
                    Console.WriteLine("foreach: {0}\n", err.Message);
                    Environment.Exit(-1);
                }
                proc.WaitForExit();
                int exitCode = proc.ExitCode;
                proc.Close();
                if (exitCode == 0)
                {
                    count++;
                }
                if (exitFlag == true && exitCode != 0)
                {
                    Console.WriteLine("foreach: {0} exitcode {1}\n", cmd, exitCode);
                    Environment.Exit(exitCode);
                }
                Console.WriteLine("foreach: completed {0} / {1}\n", count, argnum);
            }
            Console.WriteLine("foreach: {0} commands executed.\n", count);
            if (waitKey)
            {
                if (waitSec > 0)
                {
                    Thread.Sleep(waitSec * 1000);
                }
                else
                {
                    Console.ReadKey();
                }
            }
        }
    }
}
