
public class DeploymentDetailsModel
{
    public uint Version {get;set;}
    public Dictionary<string,string> Variables {get;set;} = new();

    public List<DeploymentDetailsCommandModel> Commands {get;set;} = new();

    public void Validate(){
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

    public void Transform(Dictionary<string,string> ExtraVars){
        foreach(var var in ExtraVars){
            if(!Variables.TryAdd(var.Key, var.Key)){
                throw new InvalidOperationException($"Can't insert {var.Key} {var.Value}");
            }
        }

        foreach(var cmd in Commands){
            cmd.Transform(Variables);
        }
    }
}

public class DeploymentDetailsCommandModel
{
    public string Filename {get;set;} = string.Empty;
    public string Arguments {get;set;} = string.Empty;
    public string WorkingDir {get;set;} = string.Empty;

    public void Validate(){
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

    public void Transform(Dictionary<string,string> vars){
        foreach(var var in vars){
            var replaceKey = $"$({var.Key})";
            Arguments = Arguments.Replace(replaceKey, var.Value);
            WorkingDir = WorkingDir.Replace(replaceKey, var.Value);
        }

        if(Arguments.Contains("$(")){
            throw new InvalidOperationException($"Arguments: {Arguments} still has `$(` Check if variable exists");
        }
        if(WorkingDir.Contains("$(")){
            throw new InvalidOperationException($"WorkingDir: {WorkingDir} still has `$(` Check if variable exists");
        }
    }
}