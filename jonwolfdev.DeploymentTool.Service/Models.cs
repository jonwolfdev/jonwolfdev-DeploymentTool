
public class Secrets
{
    public Dictionary<string,string> Values {get;set;} = new();
}
public class DeploymentModel
{
    public string Title {get;set;} = string.Empty;
    public bool PauseBeforeProcessExec {get;set;} = true;
    public bool DryRun {get;set;} = false;
    public uint Version {get;set;}
    public Dictionary<string,string>[] Variables {get;set;} = new Dictionary<string, string>[]{};

    public List<string> Secrets {get;set;} = new();
    public List<DeploymentCommandModel> Commands {get;set;} = new();

    public void Validate(){
        if(string.IsNullOrWhiteSpace(Title)){
            throw new InvalidOperationException("Title has an invalid value");
        }
        if(Version <= 0){
            throw new InvalidOperationException("version has an invalid value");
        }

        if(Commands.Count == 0){
            throw new InvalidOperationException("There are no commands");
        }

        foreach(var cmd in Commands){
            cmd.Validate();
        }
    }

    public void Transform(Dictionary<string,string> extraVars){
        var transformedVariables = new Dictionary<string,string>();

        foreach(var phase in Variables){
            var localdict = new Dictionary<string,string>(phase);

            // Transform with previous phase
            foreach(var var in transformedVariables){
                foreach(var varToModify in localdict){
                    var replaceKey = $"$({var.Key})";
                    localdict[varToModify.Key] = varToModify.Value.Replace(replaceKey, var.Value);
                }
            }

            // Add or replace into our collection (old key values will be replaced with the new ones)
            foreach(var var in localdict){
                transformedVariables[var.Key] = var.Value;
            }
        }

        // Transform with extra vars
        foreach(var var in extraVars){
            foreach(var varToModify in transformedVariables){
                var replaceKey = $"#({var.Key})";
                transformedVariables[varToModify.Key] = varToModify.Value.Replace(replaceKey, var.Value);
            }
        }

        foreach(var var in transformedVariables){
            if(var.Value.Contains("$(") || var.Value.Contains("#(")){
                throw new InvalidOperationException($"Variable: {var.Key} {var.Value} still has `$(` or `#(` Check if variable exists");
            }
        }

        // foreach(var var in transformedVariables){
        //     Console.WriteLine($"{var.Key} = {var.Value}");
        // }
        // Console.ReadLine();

        foreach(var cmd in Commands){
            cmd.Transform(transformedVariables, extraVars);
        }
    }
}

public class DeploymentCommandModel
{
    public string Title {get;set;} = string.Empty;
    public string Filename {get;set;} = string.Empty;
    public string Arguments {get;set;} = string.Empty;
    public string WorkingDir {get;set;} = string.Empty;

    public void Validate(){
        if(string.IsNullOrWhiteSpace(Title)){
            throw new InvalidOperationException("Title is null or empty");
        }

        if(string.IsNullOrWhiteSpace(Filename)){
            throw new InvalidOperationException("Filename is null or empty");
        }

        if(string.IsNullOrWhiteSpace(Filename)){
            throw new InvalidOperationException("Arguments is null or empty");
        }

        if(string.IsNullOrWhiteSpace(WorkingDir)){
            throw new InvalidOperationException("WorkingDir is null or empty");
        }
    }

    public void Transform(Dictionary<string,string> vars, Dictionary<string, string> extraVars){
        foreach(var var in vars){
            var replaceKey = $"$({var.Key})";
            Arguments = Arguments.Replace(replaceKey, var.Value);
            WorkingDir = WorkingDir.Replace(replaceKey, var.Value);
        }

        foreach(var var in extraVars){
            var replaceKey = $"#({var.Key})";
            Arguments = Arguments.Replace(replaceKey, var.Value);
            WorkingDir = WorkingDir.Replace(replaceKey, var.Value);
        }

        // TODO: it would be better a regex `$(*)`
        if(Arguments.Contains("$(") || Arguments.Contains("#(")){
            throw new InvalidOperationException($"Arguments: {Arguments} still has `$(` or `#(` Check if variable exists");
        }
        if(WorkingDir.Contains("$(") || WorkingDir.Contains("#(")){
            throw new InvalidOperationException($"WorkingDir: {WorkingDir} still has `$(` or `#(` Check if variable exists");
        }
    }
}