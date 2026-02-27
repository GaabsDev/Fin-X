using System.Diagnostics;

var psi = new ProcessStartInfo();
psi.FileName = "dotnet";
psi.ArgumentList.Add("run");
psi.ArgumentList.Add("--project");
psi.ArgumentList.Add("FinX.Api");
psi.RedirectStandardOutput = true;
psi.RedirectStandardError = true;
psi.RedirectStandardInput = false;
psi.UseShellExecute = false;

using var proc = new Process { StartInfo = psi };
proc.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
proc.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
proc.Start();
proc.BeginOutputReadLine();
proc.BeginErrorReadLine();
proc.WaitForExit();
Environment.Exit(proc.ExitCode);
