// Read start arguments
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument("verbosity", "Diagnostic");

// Paths to the root directories
var sourceDirectory = Directory("../.Src");
var toolDirectory = Directory("../.Tools");
var buildDirectory = Directory("../.Build");
var outputDirectory = Directory("../.Output");
var outputNuGetDirectory = Directory("../.Output/NuGet");

// The path to the solution file
var solution = sourceDirectory + File("Stomp.Net.sln");

// Executables
var nuGet = toolDirectory + File("NuGet/nuget.exe");
var nUnit = toolDirectory + File("NUnit/nunit3-console.exe");

// Clean all build output
Task("Clean")
    .Does(() =>
{	
    CleanDirectory( sourceDirectory + Directory("Stomp.Net/bin") );
    CleanDirectory( sourceDirectory + Directory("Stomp.Net.Test/bin") );
    CleanDirectory( outputDirectory );
});

// Restore all NuGet packages
Task("RestorePackages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solution, new NuGetRestoreSettings
        { 
            ToolPath = nuGet
        } );
});

// Build the solution
Task("Build")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{	
    MSBuild(solution, settings => 
        settings.SetConfiguration(configuration)
            .SetVerbosity( Verbosity.Minimal ) );
});

// Run the unit tests
Task("RunTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3( sourceDirectory.ToString() + "/**/bin/Release/*.Test.dll", new NUnit3Settings
        { 
            NoResults = true,
            ToolPath = nUnit
        });
});

// Copy the build output
Task("CopyBuildOutput")
    .IsDependentOn("RunTests")
    .Does(() =>
{
    // Create the directory
    CreateDirectory( outputDirectory );
    var nugetDirectory = outputDirectory + Directory("NuGet/lib/461");
    CreateDirectory( nugetDirectory );
    
    // Fetch and copy the build output
    var buildOutputDirectory = 	sourceDirectory + Directory("Stomp.Net/bin") + Directory(configuration);
    var files = System.IO.Directory.EnumerateFiles(buildOutputDirectory, "Stomp.Net.*").Where(x => x.Contains(".XML") || x.Contains(".dll")).ToList();
    foreach(var file in files) 
    {
        System.IO.File.Copy(file, nugetDirectory + File(System.IO.Path.GetFileName(file)));
    }
});

// Creates the NuGet package
Task("CreateNuGetPackage")
    .IsDependentOn("CopyBuildOutput")
    .Does(() =>
{
    // Copy the NuGet file
    var nuspecSource = buildDirectory + Directory("NuGet") + File("Stomp.Net.nuspec");
    var nuspec = outputNuGetDirectory + File("Stomp.Net.nuspec");
    CopyFile(nuspecSource, nuspec);
    
    // Create the NuGet package
    NuGetPack(nuspec, new NuGetPackSettings 
        {
            ToolPath = nuGet,
            Version = GetBuildVersion(),
            OutputDirectory = outputNuGetDirectory 
        } );
});

// Default task
Task("Default")
  .IsDependentOn("CreateNuGetPackage")
  .Does(() =>
{
    Information("Default task started");
});

RunTarget(target);

/// <summary>
/// Gets the version of the current build.
/// </summary>
/// <returns>Returns the version of the current build.</returns>
private String GetBuildVersion()
{
	var version = String.Empty;
	
    // Try to get the version from AppVeyor
    var appVeyorProvider = BuildSystem.AppVeyor;
    if( appVeyorProvider.IsRunningOnAppVeyor )
        version = appVeyorProvider.Environment.Build.Version;
	else
	{
	    // Get the version from the built DLL
		var outputDll = System.IO.Directory.EnumerateFiles( outputNuGetDirectory, "*", System.IO.SearchOption.AllDirectories).First( x => x.Contains( ".dll" ) );
		var assembly = System.Reflection.Assembly.LoadFile(  MakeAbsolute( File( outputDll ) ).ToString() );
		var version = assembly.GetName().Version;
		version = version.ToString();
	}
	
	return version + "-alpha";
}