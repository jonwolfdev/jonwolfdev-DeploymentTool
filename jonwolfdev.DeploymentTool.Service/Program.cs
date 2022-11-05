// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

Console.WriteLine("jonwolfdev Deployment Tool");

if(args.Length == 0){
    throw new InvalidOperationException("You have to specify the json file location. As the first argument");
}

var jsonFileLocation = args[0];

if(string.IsNullOrWhiteSpace(jsonFileLocation))
    throw new InvalidOperationException("You have to specify the json file location. As the first argument");

var contents = await File.ReadAllTextAsync(jsonFileLocation);

var deployment = JsonConvert.DeserializeObject<DeploymentModel>(contents);

if(deployment is null){
    throw new InvalidOperationException("details is null");
}

// TODO: use azure key vault or something
var config = new ConfigurationBuilder()
    .AddJsonFile("secrets.json")
    .Build();

var secrets = config.GetSection(nameof(Secrets)).Get<Secrets>();

Console.WriteLine($"Deployment: {deployment.Title}");
Console.WriteLine($"Secrets: {secrets.Values.Count}");
Console.WriteLine($"Commands: {deployment.Commands.Count}");
Console.WriteLine($"Dry run: {deployment.DryRun}");
Console.Write("Press enter to confirm: ");
Console.ReadLine();

deployment.Validate();
deployment.Transform(new Dictionary<string, string>(secrets.Values){
    {"_date", DateTime.Now.ToString("yyyy-MM-dd")},
    {"_guid", Guid.NewGuid().ToString()}
});

foreach(var cmd in deployment.Commands){

    if(cmd.Filename.StartsWith("jwd-task")){
        continue;
    }

    var psi = new ProcessStartInfo();
    
    psi.FileName = cmd.Filename;
    psi.Arguments = cmd.Arguments;
    psi.WorkingDirectory = cmd.WorkingDir;

    psi.RedirectStandardOutput = false;
    psi.RedirectStandardError = false;
    psi.UseShellExecute = false;
    psi.CreateNoWindow = true;
    
    Console.WriteLine($"=== Starting process {cmd.Title}");
    Console.WriteLine($"`{cmd.WorkingDir}` `{cmd.Filename}` `{cmd.Arguments}`");

    if(deployment.PauseBeforeProcessExec)
        Console.ReadLine();
    
    if(deployment.DryRun)
        continue;

    using var process = Process.Start(psi);
    if(process is null)
        throw new InvalidOperationException($"Couldn't create process: {psi.FileName} {psi.Arguments}");

    await process.WaitForExitAsync();

    Console.WriteLine();
}
Console.WriteLine("Finished. Press enter to exit...");
Console.ReadLine();
