//!CAT.bundle.elevated
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;

public class Base
{
    private void CopyDirectory(string sourceDir, string destDir, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;

        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            if (token.IsCancellationRequested)
                break;
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            if (token.IsCancellationRequested)
                break;
            string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir, token);
        }
    }

    private bool IsAdministrator()
    {
        if (OperatingSystem.IsWindows())
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        else
        {
            return Environment.UserName == "root";
        }
    }

    public void Exit(string[] args, CancellationToken token)
    {
        Environment.Exit(0);
    }

    public void Echo(string[] args, CancellationToken token)
    {
        if (args.Length > 0)
        {
            Console.WriteLine(string.Join(" ", args));
        }
        else
        {
            string input = Console.In.ReadToEnd();
            Console.Write(input);
        }
    }

    public void Clear(string[] args, CancellationToken token)
    {
        Console.Clear();
    }

    public void Whoami(string[] args, CancellationToken token)
    {
        Console.WriteLine(Environment.UserName);
    }

    public void Version(string[] args, CancellationToken token)
    {
        Console.WriteLine($"  \u001b[35m.NET\u001b[0m  | {Environment.Version}");
        Console.WriteLine($"  \u001b[34mOS\u001b[0m    | {Environment.OSVersion}");
    }

    public void Cd(string[] args, CancellationToken token)
    {
        try
        {
            Directory.SetCurrentDirectory(args[0]);
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"Error, \"{args[0]}\" does not exist!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error, {ex.Message}");
        }
    }

    public void Ls(string[] args, CancellationToken token)
    {
        string directoryPath = string.Empty;

        try
        {
            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                directoryPath = Directory.GetCurrentDirectory();
            }
            else
            {
                directoryPath = args[0];
            }

            string[] entries = Directory.GetFileSystemEntries(directoryPath);

            foreach (string entry in entries)
            {
                Console.WriteLine(entry);
            }
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"Error, \"{directoryPath}\" does not exist!");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Error, access to {directoryPath} is denied!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error, {ex.Message}");
        }
    }

    public void Date(string[] args, CancellationToken token)
    {
        Console.WriteLine("dd/MM/yyyy");
        Console.WriteLine("HH:mm:ss");
        Console.WriteLine();
        Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy"));
        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
    }

    public void Env(string[] args, CancellationToken token)
    {
        if (args.Length > 0 && args[0].Contains('='))
        {
            if (IsAdministrator())
            {
                string[] parts = args[0].Split(new char[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    string variableName = parts[0];
                    string variableValue = parts[1];
                    try
                    {
                        Environment.SetEnvironmentVariable(variableName, variableValue);
                    }
                    catch (System.Security.SecurityException)
                    {
                        Console.WriteLine("Error, Insufficient permissions to set environment variables.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error setting environment variable: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Error, invalid syntax!");
                }
            }
            else
            {
                Console.WriteLine("Error, must run as root to set environment variables!");
            }
        }
        else if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            string variableName = args[0];
            string variableValue = Environment.GetEnvironmentVariable(variableName);

            if (variableValue != null)
            {
                Console.WriteLine($"{variableName}: {variableValue}");
            }
            else
            {
                Console.WriteLine($"Error, environment variable \"{variableName}\" was not found!");
            }
        }
        else
        {
            Console.WriteLine("--- Environment Variables ---");
            foreach (System.Collections.DictionaryEntry de in Environment.GetEnvironmentVariables())
            {
                Console.WriteLine($"  -  {de.Key}");
            }
            Console.WriteLine("---      End of list      ---");
        }
    }


    public void Cat(string[] args, CancellationToken token)
    {
        if (args == null || args.Length == 0)
        {
            string input = Console.In.ReadToEnd();
            Console.Write(input);
            return;
        }

        foreach (string filePath in args)
        {
            if (token.IsCancellationRequested)
                break;

            try
            {
                string content = File.ReadAllText(filePath);
                Console.Write(content);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error, \"{filePath}\" was not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error, unable to read file \"{filePath}\": {ex.Message}");
            }
        }
    }

    public void Grep(string[] args, CancellationToken token)
    {
        if (args.Length == 0)
        {
            return;
        }

        string pattern = args[0];
        string[] files = args.Length > 1 ? args[1..] : Array.Empty<string>();

        if (files.Length == 0)
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                if (token.IsCancellationRequested)
                    break;

                if (line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(line);
                }
            }
            return;
        }

        foreach (string filePath in files)
        {
            if (token.IsCancellationRequested)
                break;

            try
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    if (token.IsCancellationRequested)
                        break;

                    if (line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error, \"{filePath}\" was not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading \"{filePath}\": {ex.Message}");
            }
        }
    }

    public void Touch(string[] args, CancellationToken token)
    {
        if (args.Length == 0)
        {
            return;
        }

        foreach (string filePath in args)
        {
            if (token.IsCancellationRequested)
                break;

            try
            {
                if (File.Exists(filePath))
                {
                    File.SetLastWriteTime(filePath, DateTime.Now);
                }
                else
                {
                    using (File.Create(filePath)) { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error touching \"{filePath}\": {ex.Message}");
            }
        }
    }

    public void Mv(string[] args, CancellationToken token)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: mv <source> <destination>");
            return;
        }

        string source = args[0];
        string destination = args[1];

        if (token.IsCancellationRequested)
            return;

        try
        {
            if (File.Exists(source))
            {
                if (Directory.Exists(destination))
                {
                    string destPath = Path.Combine(destination, Path.GetFileName(source));
                    File.Move(source, destPath, overwrite: true);
                }
                else
                {
                    File.Move(source, destination, overwrite: true);
                }
            }
            else if (Directory.Exists(source))
            {
                if (Directory.Exists(destination))
                {
                    string destPath = Path.Combine(destination, Path.GetFileName(source));
                    Directory.Move(source, destPath);
                }
                else
                {
                    Directory.Move(source, destination);
                }
            }
            else
            {
                Console.WriteLine($"Error, source \"{source}\" does not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving \"{source}\" to \"{destination}\": {ex.Message}");
        }
    }

    public void Cp(string[] args, CancellationToken token)
    {
        if (args.Length < 2)
        {
            return;
        }

        string source = args[0];
        string destination = args[1];

        if (token.IsCancellationRequested)
            return;

        try
        {
            if (File.Exists(source))
            {
                if (Directory.Exists(destination))
                {
                    string destPath = Path.Combine(destination, Path.GetFileName(source));
                    File.Copy(source, destPath, overwrite: true);
                }
                else
                {
                    File.Copy(source, destination, overwrite: true);
                }
            }
            else if (Directory.Exists(source))
            {
                CopyDirectory(source, destination, token);
            }
            else
            {
                Console.WriteLine($"Error, source \"{source}\" does not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying \"{source}\" to \"{destination}\": {ex.Message}");
        }
    }

    public void Rm(string[] args, CancellationToken token)
    {
        if (args.Length == 0)
        {
            return;
        }

        foreach (string path in args)
        {
            if (token.IsCancellationRequested)
                break;

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
                else
                {
                    Console.WriteLine($"Error, \"{path}\" does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing \"{path}\": {ex.Message}");
            }
        }
    }

    public void Mkdir(string[] args, CancellationToken token)
    {
        if (args.Length == 0)
        {
            return;
        }

        foreach (string dirPath in args)
        {
            if (token.IsCancellationRequested)
                break;

            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                else
                {
                    Console.WriteLine($"Error, directory \"{dirPath}\" already exists.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory \"{dirPath}\": {ex.Message}");
            }
        }
    }

    public void Rmdir(string[] args, CancellationToken token)
    {
        if (args.Length == 0)
        {
            return;
        }

        bool recursive = false;
        var dirs = new List<string>();

        foreach (string arg in args)
        {
            if (arg == "-r" || arg == "--recursive")
            {
                recursive = true;
            }
            else
            {
                dirs.Add(arg);
            }
        }

        foreach (string dirPath in dirs)
        {
            if (token.IsCancellationRequested)
                break;

            try
            {
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, recursive);
                }
                else
                {
                    Console.WriteLine($"Error, directory \"{dirPath}\" does not exist.");
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"Error, directory \"{dirPath}\" is not empty.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing directory \"{dirPath}\": {ex.Message}");
            }
        }
    }

    public void Ps(string[] args, CancellationToken token)
    {
        bool showAll = false;
        bool userOnly = false;
        string currentUser = Environment.UserName;

        foreach (string arg in args)
        {
            if (arg == "-a") showAll = true;
            else if (arg == "-u") userOnly = true;
        }

        try
        {
            Process[] processes = Process.GetProcesses();

            Console.WriteLine($"{"PID",-10} {"Name",-30} {"Memory (MB)",15} {"User",-15}");
            Console.WriteLine(new string('-', 75));

            foreach (Process proc in processes)
            {
                if (token.IsCancellationRequested)
                    break;

                try
                {
                    long memoryMb = proc.WorkingSet64 / (1024 * 1024);

                    string owner = "?";

                    try
                    {
                        if (Environment.OSVersion.Platform == PlatformID.Unix ||
                            Environment.OSVersion.Platform == PlatformID.MacOSX)
                        {
                            owner = proc.StartInfo.UserName ?? "?";
                        }
                        else
                        {
                            owner = currentUser;
                        }
                    }
                    catch { owner = "?"; }

                    if (userOnly && owner != currentUser)
                        continue;

                    if (!showAll && !userOnly && owner != currentUser)
                        continue;

                    Console.WriteLine($"{proc.Id,-10} {proc.ProcessName,-30} {memoryMb,15} {owner,-15}");
                }
                catch
                {
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing processes: {ex.Message}");
        }
    }

    public void Kill(string[] args, CancellationToken token)
    {
        if (args.Length == 0)
        {
            return;
        }

        foreach (string target in args)
        {
            if (token.IsCancellationRequested)
                break;

            try
            {
                if (int.TryParse(target, out int pid))
                {
                    try
                    {
                        var proc = Process.GetProcessById(pid);
                        proc.Kill();
                        Console.WriteLine($"Killed process {pid} ({proc.ProcessName}).");
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine($"Error, No process with PID {pid} found!");
                    }
                }
                else
                {
                    var procs = Process.GetProcessesByName(target);
                    if (procs.Length == 0)
                    {
                        Console.WriteLine($"Error, No process named \"{target}\" found!");
                        continue;
                    }

                    foreach (var proc in procs)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        try
                        {
                            proc.Kill();
                            Console.WriteLine($"Killed process {proc.Id} ({proc.ProcessName}).");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error killing process {proc.Id}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing \"{target}\": {ex.Message}");
            }
        }
    }
}