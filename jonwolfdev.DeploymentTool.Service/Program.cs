// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

Console.WriteLine("jonwolfdev Deployment Tool");

var psi = new ProcessStartInfo();
//psi.FileName = "/bin/bash";
//psi.Arguments = "mkdir temp";
psi.FileName = "/usr/bin/mkdir";
psi.Arguments = "temp";
psi.RedirectStandardOutput = true;
psi.UseShellExecute = false;
psi.CreateNoWindow = true;

using var process = Process.Start(psi);
if(process is null)
    throw new InvalidOperationException($"Couldn't create process: {psi.FileName} {psi.Arguments}");

await process.WaitForExitAsync();
var output = process.StandardOutput.ReadToEnd();

Console.WriteLine(output);
