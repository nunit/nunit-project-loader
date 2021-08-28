#tool nuget:?package=NUnit.ConsoleRunner&version=3.12.0
#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1
#tool nuget:?package=NUnit.ConsoleRunner&version=3.10.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS  
//////////////////////////////////////////////////////////////////////

// NOTE: These two constants are set here because constants.cake
// isn't loaded until after the arguments are parsed.
//
// Since GitVersion is only used when running under
// Windows, the default version should be updated to the
// next version after each release.
const string DEFAULT_VERSION = "3.7.0";
const string DEFAULT_CONFIGURATION = "Release"; 

var target = Argument("target", "Default");
var packageVersion = Argument("version", DEFAULT_VERSION);

// Load additional cake files here since some of them
// depend on the arguments provided.
#load cake/parameters.cake

//////////////////////////////////////////////////////////////////////
// SETUP AND TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup<BuildParameters>((context) =>
{
	var parameters = new BuildParameters(context);

	Information("Building {0} version {1} of NUnit Project Loader.", parameters.Configuration, parameters.PackageVersion);

	return parameters;
});

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does<BuildParameters>((parameters) =>
{
    CleanDirectory(parameters.OutputDirectory);
});


//////////////////////////////////////////////////////////////////////
// INITIALIZE FOR BUILD
//////////////////////////////////////////////////////////////////////

Task("NuGetRestore")
    .Does(() =>
{
    NuGetRestore(SOLUTION_FILE, new NuGetRestoreSettings()
	{
		Source = PACKAGE_SOURCES
	});
});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("NuGetRestore")
    .Does<BuildParameters>((parameters) =>
    {
		if(IsRunningOnWindows())
		{
			MSBuild(SOLUTION_FILE, new MSBuildSettings()
				.SetConfiguration(parameters.Configuration)
				.SetMSBuildPlatform(MSBuildPlatform.Automatic)
				.SetVerbosity(Verbosity.Minimal)
				.SetNodeReuse(false)
				.SetPlatformTarget(PlatformTarget.MSIL)
			);
		}
		else
		{
			XBuild(SOLUTION_FILE, new XBuildSettings()
				.WithTarget("Build")
				.WithProperty("Configuration", parameters.Configuration)
				.SetVerbosity(Verbosity.Minimal)
			);
		}
    });

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("Test")
	.IsDependentOn("Build")
	.Does<BuildParameters>((parameters) =>
	{
		NUnit3(TEST_TARGET_FRAMEWORKS.Select(framework => System.IO.Path.Combine(
			parameters.OutputDirectory, framework, UNIT_TEST_ASSEMBLY)));
	});

//////////////////////////////////////////////////////////////////////
// PACKAGE
//////////////////////////////////////////////////////////////////////

Task("BuildNuGetPackage")
	.Does<BuildParameters>((parameters) =>
	{
		CreateDirectory(parameters.PackageDirectory);

		BuildNuGetPackage(parameters);
	});

Task("TestNuGetPackage")
	.Does<BuildParameters>((parameters) =>
	{
		var tester = new NuGetPackageTester(parameters)
		{
			PackageChecks = new PackageCheck[]
			{
				HasFiles("CHANGES.txt", "LICENSE.txt"),
				HasDirectory("tools").WithFile("nunit-project-loader.dll")
			}
		};

		tester.InstallPackage();
		tester.VerifyPackage();
		tester.RunPackageTests();

		// In case of error, this will not be executed, leaving the directory available for examination
		tester.UninstallPackage();
	});

Task("BuildChocolateyPackage")
	.IsDependentOn("Build")
	.Does<BuildParameters>((parameters) =>
	{

		CreateDirectory(parameters.PackageDirectory);

		BuildChocolateyPackage(parameters);
	});

Task("TestChocolateyPackage")
	.Does<BuildParameters>((parameters) =>
	{
		var tester = new ChocolateyPackageTester(parameters)
		{
			PackageChecks = new PackageCheck[]
			{
				HasDirectory("tools").WithFiles(
					"CHANGES.txt", "LICENSE.txt", "VERIFICATION.txt", "nunit-project-loader.dll")
			}
		};

		tester.InstallPackage();
		tester.VerifyPackage();
		tester.RunPackageTests();

		// In case of error, this will not be executed, leaving the directory available for examination
		tester.UninstallPackage();
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.IsDependentOn("PackageNuGet")
	.IsDependentOn("PackageChocolatey");

Task("PackageNuGet")
	.IsDependentOn("BuildNuGetPackage")
	.IsDependentOn("TestNuGetPackage");

Task("PackageChocolatey")
	.IsDependentOn("BuildChocolateyPackage")
	.IsDependentOn("TestChocolateyPackage");

Task("Full")
	.IsDependentOn("Clean")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

Task("Travis")
	.IsDependentOn("Build")
	.IsDependentOn("Test");

Task("Appveyor")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
