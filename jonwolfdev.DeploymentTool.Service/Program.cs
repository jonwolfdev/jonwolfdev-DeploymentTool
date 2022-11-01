// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Newtonsoft.Json;

Console.WriteLine("jonwolfdev Deployment Tool");

// TODO: add host service
// TODO: add model validation
// TODO: add logging and send email for failed or success

// dotnet run /home/jon/Downloads/test-deployment.json
if(args.Length == 0){
    throw new InvalidOperationException("You have to specify the json file location. As the first argument");
}

var jsonFileLocation = args[0];

if(string.IsNullOrWhiteSpace(jsonFileLocation))
    throw new InvalidOperationException("You have to specify the json file location. As the first argument");

var contents = await File.ReadAllTextAsync(jsonFileLocation);

var details = JsonConvert.DeserializeObject<DeploymentDetailsModel>(contents);

if(details is null){
    throw new InvalidOperationException("details is null");
}
details.Validate();
details.Transform(new Dictionary<string, string>());

foreach(var cmd in details.Commands){
    var psi = new ProcessStartInfo();
    
    psi.FileName = cmd.Filename;
    psi.Arguments = cmd.Arguments;
    psi.WorkingDirectory = cmd.WorkingDir;

    psi.RedirectStandardOutput = false;
    psi.RedirectStandardError = false;
    psi.UseShellExecute = false;
    psi.CreateNoWindow = true;
    

    Console.WriteLine($"=== Starting process `{cmd.WorkingDir}` `{cmd.Filename}` `{cmd.Arguments}`");
    using var process = Process.Start(psi);
    if(process is null)
        throw new InvalidOperationException($"Couldn't create process: {psi.FileName} {psi.Arguments}");

    await process.WaitForExitAsync();
    //var output = await process.StandardOutput.ReadToEndAsync();
    //var err = process.StandardError.ReadToEnd();
    //Console.WriteLine("Output: " + output);
    //Console.WriteLine("Error: " + err);
    // if(!string.IsNullOrEmpty(err) && err.Length > 0){
    //     throw new InvalidOperationException("Process wrote to error output");
    // }

    Console.WriteLine("=== End process");
    Console.WriteLine();

}

