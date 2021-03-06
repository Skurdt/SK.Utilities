/* MIT License

 * Copyright (c) 2020 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SK.Utilities
{
    public static class OSUtils
    {
        public struct ProcessCommand
        {
            public string Name;
            public string Id;
            public string Path;
            public string WorkingDirectory;
            public string Extension;
            public string CommandLine;
            public string CommandId;
        }

        public sealed class ProcessStartedData
        {
            public DateTime StartTime;
        }

        public sealed class ProcessExitedData
        {
            public DateTime StartTime;
            public DateTime ExitTime;
            public int ExitCode;
        }

        public delegate void ProcessStartedDelegate(ProcessStartedData processStartedData);
        public delegate void ProcessExitedDelegate(ProcessExitedData processExitedData);

        public sealed class ProcessLauncher
        {
            private readonly List<int> _persistentProcesses = new List<int>();
            private readonly Action<string> _logger;
            private readonly Action<string> _warningLogger;
            private readonly Action<string> _errorLogger;

            private int _currentProcessId = -1;

            public ProcessLauncher(Action<string> logger = null, Action<string> warningLogger = null, Action<string> errorLogger = null)
            {
                _logger        = logger;
                _warningLogger = warningLogger;
                _errorLogger   = errorLogger;
            }

            public bool StartProcess(ProcessCommand command, bool persistent, ProcessStartedDelegate processStartedDelegate, ProcessExitedDelegate processExitedDelegate)
            {
                StopCurrentProcess();

                try
                {
                    string programPath             = Path.GetFullPath(command.Path);
                    string programWorkingDirectory = !string.IsNullOrEmpty(command.WorkingDirectory) ? command.WorkingDirectory : Path.GetDirectoryName(programPath);
                    string extension               = !string.IsNullOrEmpty(command.Extension) ? $".{command.Extension}" : string.Empty;
                    string arguments               = !string.IsNullOrEmpty(command.CommandId) ? $"{command.CommandLine} {command.CommandId}{extension}" : command.CommandLine;

                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName               = programPath,
                            WorkingDirectory       = programWorkingDirectory,
                            Arguments              = arguments,
                            CreateNoWindow         = true,
                            UseShellExecute        = false,
                            RedirectStandardOutput = true,
                            RedirectStandardInput  = false,
                            RedirectStandardError  = true,
                            WindowStyle            = ProcessWindowStyle.Normal
                        },
                        EnableRaisingEvents = true
                    };

                    process.OutputDataReceived += ProcessOutputDataReceivedEventHandler;
                    process.ErrorDataReceived  += ProcessErrorDataReceivedEventHandler;

                    process.Exited += delegate
                    {
                        if (process is null)
                            return;

                        processExitedDelegate?.Invoke(new ProcessExitedData
                        {
                            StartTime = process.StartTime,
                            ExitTime  = process.ExitTime,
                            ExitCode  = process.ExitCode
                        });

                        process.Dispose();
                    };

                    if (!process.Start())
                        return false;

                    if (persistent)
                        _persistentProcesses.Add(process.Id);
                    else
                        _currentProcessId = process.Id;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    processStartedDelegate?.Invoke(new ProcessStartedData
                    {
                        StartTime = process.StartTime
                    });

                    return true;
                }
                catch (Exception e)
                {
                    _errorLogger?.Invoke($"[OSUtils.ProcessLauncher.StartProcess] {e.Message}");
                    return false;
                }
            }

            public void StopCurrentProcess() => StopProcess(_currentProcessId);

            public void StopAllProcesses()
            {
                StopProcess(_currentProcessId);

                foreach (int processId in _persistentProcesses)
                    StopProcess(processId);

                _persistentProcesses.Clear();
            }

            private void StopProcess(int id)
            {
                if (id < 0)
                    return;

                try
                {
                    Process process = Process.GetProcessById(id);
                    if (process is null)
                    {
                        _warningLogger?.Invoke("[OSUtils.ProcessLauncher.StopProcess] Process is null.");
                        return;
                    }

                    if (!process.HasExited)
                    {
                        _ = process.CloseMainWindow();
                        process.Close();
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    _currentProcessId = -1;
                }
            }

            private void ProcessOutputDataReceivedEventHandler(object sender, DataReceivedEventArgs eventArgs)
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                    _logger?.Invoke($"[STDOUT] {eventArgs.Data}");
            }

            private void ProcessErrorDataReceivedEventHandler(object sender, DataReceivedEventArgs eventArgs)
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                    _logger?.Invoke($"[STDERR] {eventArgs.Data}");
            }
        }
    }
}
