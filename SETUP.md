Install vs code through Snap store
sudo apt install git
sudo apt install dotnet6
dotnet new sln -o jonwolfdev.DeploymentTool
Open vs code
Open a new folder (jonwolfdev.DeploymentTool)
dotnet new gitignore
git init
git config user.name "jonwolfdev"
git config user.email "49768794+jonwolfdev@users.noreply.github.com"

dotnet new console -o jonwolfdev.DeploymentTool.Service

dotnet sln jonwolfdev.DeploymentTool.sln add jonwolfdev.DeploymentTool.Service/jonwolfdev.DeploymentTool.Service.csproj

# if you want to add a csproj ref to another csproj
	dotnet add .\MyApiApp.WebApi\MyApiApp.WebApi.csproj reference .\MyApiApp.Repository\MyApiApp.Repository.csproj 


# example of other templates
dotnet new webapi -o MyApiApp.WebApi 
dotnet new classlib -o MyApiApp.Repository 
dotnet new xunit -o MyApiApp.Tests

VS code (open folder where csproj is so you can run & debug)

#add package to a csproj
dotnet add package Newtonsoft.Json
