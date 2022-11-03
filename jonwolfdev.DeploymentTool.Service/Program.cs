// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

Console.WriteLine("jonwolfdev Deployment Tool");

// TODO: add host service
// TODO: add model validation
// TODO: add logging and send email for failed or success
// TODO: stop execution if one command fails
// sudo dotnet run /home/jon/Downloads/test-deployment.json
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

var config = new ConfigurationBuilder()
    .AddJsonFile("secrets.json")
    .Build();

//var secrets = config.GetSection(nameof(SecretsModel)).get;

Console.WriteLine($"Deployment: {deployment.Title}");
Console.Write("Press enter to confirm");
Console.ReadLine();

deployment.Validate();
deployment.Transform(new Dictionary<string, string>(){
    {"_date", DateTime.Now.ToString("yyyy-MM-dd")},
    {"_guid", Guid.NewGuid().ToString()},

    // TODO: these should be different
    {"homeFolderName", "jon"},
    {"appWwwFolder", "sk-api"},
    {"dbName", "testtool"},
    {"appServiceName", "ssh"},
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

    Console.WriteLine();
}

