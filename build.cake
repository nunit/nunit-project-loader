#tool nuget:?package=NUnit.ConsoleRunner&version=3.8.0
#tool nuget:?package=NUnit.Extension.VSProjectLoader&version=3.7.0
using System.Linq;

//////////////////////////////////////////////////////////////////////
// PROJECT-SPECIFIC
//////////////////////////////////////////////////////////////////////

// When copying the script to support a different extension, the
// main changes needed should be in this section.

var SOLUTION_FILE = "nunit-project-loader.sln";
var UNIT_TEST_ASSEMBLY = "nunit-project-loader.tests.dll";
var GITHUB_SITE = "https://github.com/nunit/nunit-project-loader";
var WIKI_PAGE = "https://github.com/nunit/docs/wiki/Console-Command-Line";
var NUGET_ID = "NUnit.Extension.NUnitProjectLoader";
var CHOCO_ID = "nunit-extension-nunit-project-loader";
var VERSION = "3.6.0";
var TARGET_FRAMEWORKS = new [] { "net20", "net45" };

// Metadata used in the nuget and chocolatey packages
var TITLE = "NUnit 3 - NUnit Project Loader Extension";
var AUTHORS = new [] { "Charlie Poole" };
var OWNERS = new [] { "Charlie Poole" };
var DESCRIPTION = "This extension allows NUnit to recognize and load NUnit projects, file type .nunit.";
var SUMMARY = "NUnit Engine extension for loading NUnit projects.";
var COPYRIGHT = "Copyright (c) 2016 Charlie Poole";
var RELEASE_NOTES = new [] { "See https://raw.githubusercontent.com/nunit/nunit-project-loader/master/CHANGES.txt" };
var TAGS = new [] { "nunit", "test", "testing", "tdd", "runner" };

//////////////////////////////////////////////////////////////////////
// ARGUMENTS  
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

// Special (optional) arguments for the script. You pass these
// through the Cake bootscrap script via the -ScriptArgs argument
// for example: 
//   ./build.ps1 -t RePackageNuget -ScriptArgs --nugetVersion="3.9.9"
//   ./build.ps1 -t RePackageNuget -ScriptArgs '--binaries="rel3.9.9" --nugetVersion="3.9.9"'
var nugetVersion = Argument("nugetVersion", (string)null);
var chocoVersion = Argument("chocoVersion", (string)null);
var binaries = Argument("binaries", (string)null);

//////////////////////////////////////////////////////////////////////
// SET PACKAGE VERSION
//////////////////////////////////////////////////////////////////////

var dbgSuffix = configuration == "Debug" ? "-dbg" : "";
var packageVersion = VERSION + dbgSuffix;

if (BuildSystem.IsRunningOnAppVeyor)
{
	var tag = AppVeyor.Environment.Repository.Tag;

	if (tag.IsTag)
	{
		packageVersion = tag.Name;
	}
	else
	{
		var buildNumber = AppVeyor.Environment.Build.Number.ToString("00000");
		var branch = AppVeyor.Environment.Repository.Branch;
		var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

		if (branch == "master" && !isPullRequest)
		{
			packageVersion = VERSION + "-dev-" + buildNumber + dbgSuffix;
		}
		else
		{
			var suffix = "-ci-" + buildNumber + dbgSuffix;

			if (isPullRequest)
				suffix += "-pr-" + AppVeyor.Environment.PullRequest.Number;
			else
				suffix += "-" + branch;

			// Nuget limits "special version part" to 20 chars. Add one for the hyphen.
			if (suffix.Length > 21)
				suffix = suffix.Substring(0, 21);

            suffix = suffix.Replace(".", "");

			packageVersion = VERSION + suffix;
		}
	}

	AppVeyor.UpdateBuildVersion(packageVersion);
}

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

// Directories
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";
var BIN_SRC = BIN_DIR; // Source of binaries used in packaging
var OUTPUT_DIR = PROJECT_DIR + "output/";

// Adjust BIN_SRC if --binaries option was given
if (binaries != null)
{
	BIN_SRC = binaries;
	if (!System.IO.Path.IsPathRooted(binaries))
	{
		BIN_SRC = PROJECT_DIR + binaries;
		if (!BIN_SRC.EndsWith("/"))
			BIN_SRC += "/";
	}
}

// Package sources for nuget restore
var PACKAGE_SOURCE = new string[]
	{
		"https://www.nuget.org/api/v2",
		"https://www.myget.org/F/nunit/api/v2"
	};

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(BIN_DIR);
});


//////////////////////////////////////////////////////////////////////
// INITIALIZE FOR BUILD
//////////////////////////////////////////////////////////////////////

Task("NuGetRestore")
    .Does(() =>
{
    NuGetRestore(SOLUTION_FILE, new NuGetRestoreSettings()
	{
		Source = PACKAGE_SOURCE
	});
});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("NuGetRestore")
    .Does(() =>
    {
		if (binaries != null)
		    throw new Exception("The --binaries option may only be specified when re-packaging an existing build.");

		MSBuild(SOLUTION_FILE, new MSBuildSettings()
			.SetConfiguration(configuration)
			.SetMSBuildPlatform(MSBuildPlatform.Automatic)
			.SetVerbosity(Verbosity.Minimal)
			.SetNodeReuse(false)
			.SetPlatformTarget(PlatformTarget.MSIL)
		);
    });

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		foreach(var framework in TARGET_FRAMEWORKS)
		{
			NUnit3(System.IO.Path.Combine(BIN_DIR, framework, UNIT_TEST_ASSEMBLY));
		}
	});

//////////////////////////////////////////////////////////////////////
// PACKAGE
//////////////////////////////////////////////////////////////////////

// Additional package metadata
var PROJECT_URL = new Uri("http://nunit.org");
var ICON_URL = new Uri("https://cdn.rawgit.com/nunit/resources/master/images/icon/nunit_256.png");
var LICENSE_URL = new Uri("http://nunit.org/nuget/nunit3-license.txt");
var PROJECT_SOURCE_URL = new Uri( GITHUB_SITE );
var PACKAGE_SOURCE_URL = new Uri( GITHUB_SITE );
var BUG_TRACKER_URL = new Uri(GITHUB_SITE + "/issues");
var DOCS_URL = new Uri(WIKI_PAGE);
var MAILING_LIST_URL = new Uri("https://groups.google.com/forum/#!forum/nunit-discuss");

Task("RePackageNuGet")
	.Does(() => 
	{
		CreateDirectory(OUTPUT_DIR);

		var files = new List<NuSpecContent>();
		files.Add(new NuSpecContent { Source = PROJECT_DIR + "LICENSE.txt" });
		files.Add(new NuSpecContent { Source = PROJECT_DIR + "CHANGES.txt" });

		foreach(var framework in TARGET_FRAMEWORKS)
		{
			files.Add(new NuSpecContent { Source = BIN_SRC + framework + "/nunit-project-loader.dll", Target = "tools/" + framework });
		}

        NuGetPack(
			new NuGetPackSettings()
			{
				Id = NUGET_ID,
				Version = nugetVersion ?? packageVersion,
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
				OutputDirectory = OUTPUT_DIR,
				Files = files.ToArray()
			});
	});

Task("RePackageChocolatey")
	.Does(() =>
	{
		CreateDirectory(OUTPUT_DIR);

		var files = new List<ChocolateyNuSpecContent>();
		files.Add(new ChocolateyNuSpecContent { Source = PROJECT_DIR + "LICENSE.txt", Target = "tools" });
		files.Add(new ChocolateyNuSpecContent { Source = PROJECT_DIR + "CHANGES.txt", Target = "tools" });
		files.Add(new ChocolateyNuSpecContent { Source = PROJECT_DIR + "VERIFICATION.txt", Target = "tools" });

		foreach(var framework in TARGET_FRAMEWORKS)
		{
			files.Add(new ChocolateyNuSpecContent { Source = BIN_SRC + framework + "/nunit-project-loader.dll", Target = "tools/" + framework });
		}

		ChocolateyPack(
			new ChocolateyPackSettings()
			{
				Id = CHOCO_ID,
				Version = chocoVersion ?? packageVersion,
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
    			DocsUrl= DOCS_URL,
    			BugTrackerUrl = BUG_TRACKER_URL,
    			PackageSourceUrl = PACKAGE_SOURCE_URL,
    			MailingListUrl = MAILING_LIST_URL,
				ReleaseNotes = RELEASE_NOTES,
				Tags = TAGS,
				//Language = "en-US",
				OutputDirectory = OUTPUT_DIR,
				Files = files.ToArray(),
				Verbose = true
			});
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Rebuild")
    .IsDependentOn("Clean")
	.IsDependentOn("Build");

Task("Package")
	.IsDependentOn("Build")
	.IsDependentOn("RePackage");

Task("RePackage")
	.IsDependentOn("RePackageNuGet")
	.IsDependentOn("RePackageChocolatey");

Task("Appveyor")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

Task("Travis")
	.IsDependentOn("Build")
	.IsDependentOn("Test");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
