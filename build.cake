#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1

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

Task("PackageNuGet")
	.Does<BuildParameters>((parameters) => 
	{
		var content = new List<NuSpecContent>();
		content.Add(new NuSpecContent { Source = parameters.ProjectDirectory + "LICENSE.txt" });
		content.Add(new NuSpecContent { Source = parameters.ProjectDirectory + "CHANGES.txt" });
		foreach (string framework in TARGET_FRAMEWORKS)
			content.Add(new NuSpecContent {
				Source = $"{parameters.OutputDirectory}{framework}/nunit-project-loader.dll",
				Target = $"tools/{framework}" });

		CreateDirectory(parameters.PackageDirectory);

        NuGetPack(
			new NuGetPackSettings()
			{
				Id = NUGET_ID,
				Version = parameters.PackageVersion,
				Title = TITLE,
				Authors = AUTHORS,
				Owners = OWNERS,
				Description = DESCRIPTION,
				Summary = SUMMARY,
				ProjectUrl = PROJECT_URL,
				IconUrl = ICON_URL,
				LicenseUrl = LICENSE_URL,
				RequireLicenseAcceptance = false,
				Copyright = COPYRIGHT,
				ReleaseNotes = RELEASE_NOTES,
				Tags = TAGS,
				//Language = "en-US",
				OutputDirectory = parameters.PackageDirectory,
				KeepTemporaryNuSpecFile =false,
				Files = content
			});
	});

Task("PackageChocolatey")
	.Does<BuildParameters>((parameters) =>
	{
		CreateDirectory(parameters.PackageDirectory);

		var content = new List<ChocolateyNuSpecContent>();
		content.Add(new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "LICENSE.txt", Target = "tools" });
		content.Add(new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "CHANGES.txt", Target = "tools" });
		content.Add(new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "VERIFICATION.txt", Target = "tools" });
		foreach (string framework in TARGET_FRAMEWORKS)
			content.Add(new ChocolateyNuSpecContent {
				Source = $"{parameters.OutputDirectory}{framework}/nunit-project-loader.dll",
				Target = $"tools/{framework}" });

		ChocolateyPack(
			new ChocolateyPackSettings()
			{
				Id = CHOCO_ID,
				Version = parameters.PackageVersion,
				Title = TITLE,
				Authors = AUTHORS,
				Owners = OWNERS,
				Description = DESCRIPTION,
				Summary = SUMMARY,
				ProjectUrl = PROJECT_URL,
				IconUrl = ICON_URL,
				LicenseUrl = LICENSE_URL,
				RequireLicenseAcceptance = false,
				Copyright = COPYRIGHT,
				ProjectSourceUrl = PROJECT_SOURCE_URL,
				DocsUrl = DOCS_URL,
				BugTrackerUrl = BUG_TRACKER_URL,
				PackageSourceUrl = PACKAGE_SOURCE_URL,
				MailingListUrl = MAILING_LIST_URL,
				ReleaseNotes = RELEASE_NOTES,
				Tags = TAGS,
				//Language = "en-US",
				OutputDirectory = parameters.PackageDirectory,
				Files = content
			});
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.IsDependentOn("PackageNuGet")
	.IsDependentOn("PackageChocolatey");

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
