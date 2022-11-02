
public class DeploymentDetailsModel
{
    public bool PauseBeforeProcessExec {get;set;} = true;
    public uint Version {get;set;}
    public Dictionary<string,string> Variables {get;set;} = new();

    public List<string> Secrets {get;set;} = new();
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
            foreach(var varToModify in Variables){
                var replaceKey = $"$({var.Key})";
                Variables[varToModify.Key] = varToModify.Value.Replace(replaceKey, var.Value);
            }
        }

        foreach(var var in Variables){
            if(var.Value.Contains("$(")){
                throw new InvalidOperationException($"Arguments: {var.Key} {var.Value} still has `$(` Check if variable exists");
            }
        }

        foreach(var var in ExtraVars){
            if(!Variables.TryAdd(var.Key, var.Value)){
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

    public void Transform(Dictionary<string,string> vars){
        foreach(var var in vars){
            var replaceKey = $"$({var.Key})";
            Arguments = Arguments.Replace(replaceKey, var.Value);
            WorkingDir = WorkingDir.Replace(replaceKey, var.Value);
        }

        // TODO: it would be better a regex `$(*)`
        if(Arguments.Contains("$(")){
            throw new InvalidOperationException($"Arguments: {Arguments} still has `$(` Check if variable exists");
        }
        if(WorkingDir.Contains("$(")){
            throw new InvalidOperationException($"WorkingDir: {WorkingDir} still has `$(` Check if variable exists");
        }
    }
}