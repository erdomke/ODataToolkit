#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configs = new string[] { "NET40", "NET35" };
var version = string.Format("0.7.{0}.{1}"
	, (int)((DateTime.UtcNow - new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalDays)
	, (int)((DateTime.UtcNow - DateTime.UtcNow.Date).TotalSeconds / 2));

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  DeleteFiles("./ODataToolkit.*.nupkg");
  EnsureDirectoryExists("./artifacts");
  CleanDirectory("./publish/ODataToolkit/lib");
  CleanDirectory("./artifacts");
});

Task("Patch-Version")
  .IsDependentOn("Clean")
  .Does(() =>
{
  var file = "./ODataToolkit/Properties/AssemblyInfo.cs";
  var info = ParseAssemblyInfo(file);
  CreateAssemblyInfo(file, new AssemblyInfoSettings {
    Product = "ODataToolkit",
    Version = version,
    FileVersion = version,
    Description = "Toolkit for developing OData web services.  Can be used from Web API, Nancy, or the platform of your choice."
  });
});

Task("Restore-NuGet-Packages")
  .IsDependentOn("Patch-Version")
  .Does(() =>
{
  NuGetRestore("./ODataToolkit.sln");
});

Task("Build")
  .IsDependentOn("Restore-NuGet-Packages")
  .Does(() =>
{
  foreach (var config in configs)
  {
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./ODataToolkit/ODataToolkit.csproj", settings =>
        settings.SetConfiguration(config));
    }
    else
    {
      // Use XBuild
      XBuild("./ODataToolkit/ODataToolkit.csproj", settings =>
        settings.SetConfiguration(config));
    }
  }
});

Task("NuGet-Pack")
  .IsDependentOn("Build")
  .Does(() =>
{
  var nuGetPackSettings = new NuGetPackSettings {
    Version = version,
	OutputDirectory = "./artifacts"
  };
  NuGetPack("./publish/ODataToolkit/ODataToolkit.nuspec", nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("NuGet-Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
