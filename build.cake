#tool NuGet.CommandLine&version=6.9.1
#tool nuget:?package=GitVersion.CommandLine&version=5.6.3
#tool nuget:?package=GitReleaseManager&version=0.17.0
#tool nuget:?package=NUnit.ConsoleRunner&version=3.17.0
#tool nuget:?package=NUnit.ConsoleRunner&version=3.15.5
#tool nuget:?package=NUnit.ConsoleRunner&version=3.18.0-dev00037
#tool nuget:?package=NUnit.ConsoleRunner.NetCore&version=3.18.0-dev00037

// Load scripts 
#load cake/build-settings.cake

//////////////////////////////////////////////////////////////////////
// ARGUMENTS  
//////////////////////////////////////////////////////////////////////

var target = Argument("target", Argument("t", "Default"));

// Additional arguments defined in the cake scripts:
//   --configuration
//   --packageVersion

//////////////////////////////////////////////////////////////////////
// SETUP AND TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup<BuildSettings>((context) =>
{
	var settings = BuildSettings.Create(context);

	if (BuildSystem.IsRunningOnAppVeyor)
		AppVeyor.UpdateBuildVersion(settings.PackageVersion + "-" + AppVeyor.Environment.Build.Number);

	Information("Building {0} version {1} of NUnit Project Loader.", settings.Configuration, settings.PackageVersion);

	return settings;
});

//////////////////////////////////////////////////////////////////////
// DUMP SETTINGS
//////////////////////////////////////////////////////////////////////

Task("DumpSettings")
	.Does<BuildSettings>((settings) =>
	{
		settings.DumpSettings();
	});

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does<BuildSettings>((settings) =>
	{
		Information("Cleaning " + settings.OutputDirectory);
		CleanDirectory(settings.OutputDirectory);
		Information("Cleaning " + settings.PackageDirectory);
		CleanDirectory(settings.PackageDirectory);
		Information("Deleting log files");
		DeleteFiles(settings.ProjectDirectory + "*.log");
	});

//////////////////////////////////////////////////////////////////////
// INITIALIZE FOR BUILD
//////////////////////////////////////////////////////////////////////

Task("NuGetRestore")
    .Does(() =>
	{
		NuGetRestore(SOLUTION_FILE, new NuGetRestoreSettings()
		{
			Source = new string[]
			{
				"https://www.nuget.org/api/v2",
				"https://www.myget.org/F/nunit/api/v2"
			}
		});
	});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
	.IsDependentOn("Clean")
    .IsDependentOn("NuGetRestore")
    .Does<BuildSettings>((settings) =>
    {
		if(IsRunningOnWindows())
		{
			MSBuild(SOLUTION_FILE, new MSBuildSettings()
				.SetConfiguration(settings.Configuration)
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
				.WithProperty("Configuration", settings.Configuration)
				.SetVerbosity(Verbosity.Minimal)
			);
		}
    });

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("Test")
	.IsDependentOn("Build")
	.IsDependentOn("TestNet462")
	.IsDependentOn("TestNet60");

Task("TestNet462")
	.Does<BuildSettings>((settings) =>
	{
		string runner = settings.ToolsDirectory + $"NUnit.ConsoleRunner.{CONSOLE_VERSION_FOR_UNIT_TESTS}/tools/nunit3-console.exe";
		StartProcess(runner, settings.OutputDirectory + "net462/nunit-project-loader.tests.dll");
		// We will have empty logs so long as we test against 3.15.5 and 3.17.0
		DeleteEmptyLogFiles(settings.ProjectDirectory);
	});

Task("TestNet60")
	.Does<BuildSettings>((settings) =>
	{
		// Need to use dev build for this test
		string runner = settings.ToolsDirectory + $"NUnit.ConsoleRunner.3.18.0-dev00037/tools/nunit3-console.exe";
		StartProcess(runner, settings.OutputDirectory + "net6.0/nunit-project-loader.tests.dll");
	});

//////////////////////////////////////////////////////////////////////
// PACKAGE
//////////////////////////////////////////////////////////////////////

Task("BuildNuGetPackage")
	.Does<BuildSettings>((settings) =>
	{
		CreateDirectory(settings.PackageDirectory);

		BuildNuGetPackage(settings);
	});

Task("InstallNuGetPackage")
	.Does<BuildSettings>((settings) =>
	{
		// Ensure we aren't inadvertently using the chocolatey install
		if (DirectoryExists(settings.ChocolateyInstallDirectory))
			DeleteDirectory(settings.ChocolateyInstallDirectory, new DeleteDirectorySettings() { Recursive = true });

		CleanDirectory(settings.NuGetInstallDirectory);
		Unzip(settings.NuGetPackage, settings.NuGetInstallDirectory);

		Information($"Unzipped {settings.NuGetPackageName} to { settings.NuGetInstallDirectory}");
	});

Task("VerifyNuGetPackage")
	.IsDependentOn("InstallNuGetPackage")
	.Does<BuildSettings>((settings) =>
	{
		Check.That(settings.NuGetInstallDirectory,
			HasFiles("LICENSE.txt", "nunit_256.png"),
			HasDirectory("tools/net20").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll"));
		Information("Verification was successful!");
	});

Task("TestNuGetPackage")
	.IsDependentOn("InstallNuGetPackage")
	.Does<BuildSettings>((settings) =>
	{
		new NuGetPackageTester(settings).RunPackageTests(PackageTests);

		// We will have empty logs so long as we test against 3.15.5 and 3.17.0
		DeleteEmptyLogFiles(settings.ProjectDirectory);
	});

Task("BuildChocolateyPackage")
	.IsDependentOn("Build")
	.Does<BuildSettings>((settings) =>
	{
		CreateDirectory(settings.PackageDirectory);

		BuildChocolateyPackage(settings);
	});

Task("InstallChocolateyPackage")
	.Does<BuildSettings>((settings) =>
	{
		// Ensure we aren't inadvertently using the nuget install
		if (DirectoryExists(settings.NuGetInstallDirectory))
			DeleteDirectory(settings.NuGetInstallDirectory, new DeleteDirectorySettings() { Recursive = true });

		CleanDirectory(settings.ChocolateyInstallDirectory);
		Unzip(settings.ChocolateyPackage, settings.ChocolateyInstallDirectory);

		Information($"Unzipped {settings.ChocolateyPackageName} to { settings.ChocolateyInstallDirectory}");
	});

Task("VerifyChocolateyPackage")
	.IsDependentOn("InstallChocolateyPackage")
	.Does<BuildSettings>((settings) =>
	{
		Check.That(settings.ChocolateyInstallDirectory,
			HasDirectory("tools").WithFiles(
				"LICENSE.txt", "VERIFICATION.txt"),
			HasDirectory("tools/net20").WithFiles(
				"nunit-project-loader.dll", "nunit.engine.api.dll"));
		Information("Verification was successful!");
	});

Task("TestChocolateyPackage")
	.IsDependentOn("InstallChocolateyPackage")
	.Does<BuildSettings>((settings) =>
	{
		new ChocolateyPackageTester(settings).RunPackageTests(PackageTests);

		// We will have empty logs so long as we test against 3.15.5 and 3.17.0
		DeleteEmptyLogFiles(settings.ProjectDirectory);
	});

PackageTest[] PackageTests = new PackageTest[]
{
	new PackageTest()
	{
		Description = "Project with one assembly, all tests pass",
		Arguments = "PassingAssembly.nunit",
		TestConsoleVersions =  CONSOLE_VERSIONS_FOR_PACKAGE_TESTS,
		ExpectedResult = new ExpectedResult("Passed")
		{
			Total = 4,
			Passed = 4,
			Failed = 0,
			Warnings = 0,
			Inconclusive = 0,
			Skipped = 0,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "net-2.0") }
		}
	},
	new PackageTest()
	{
		Description = "Project with one assembly, some failures",
		Arguments = "FailingAssembly.nunit",
		TestConsoleVersions =  CONSOLE_VERSIONS_FOR_PACKAGE_TESTS,
		ExpectedResult = new ExpectedResult("Failed")
		{
			Total = 9,
			Passed = 4,
			Failed = 2,
			Warnings = 0,
			Inconclusive = 1,
			Skipped = 2,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "net-2.0") }
		}
	},
	new PackageTest()
	{
		Description = "Project with both assemblies",
		Arguments = "BothAssemblies.nunit",
		TestConsoleVersions =  CONSOLE_VERSIONS_FOR_PACKAGE_TESTS,
		ExpectedResult = new ExpectedResult("Failed")
		{
			Total = 13,
			Passed = 8,
			Failed = 2,
			Warnings = 0,
			Inconclusive = 1,
			Skipped = 2,
			Assemblies = new[] {
				new ExpectedAssemblyResult("test-lib-1.dll", "net-2.0"),
				new ExpectedAssemblyResult("test-lib-2.dll", "net-2.0")
			}
		}
	},
	new PackageTest()
	{
		Description = "Project with one assembly, all tests pass",
		Arguments = "PassingAssemblyNetCore.nunit",
		TestConsoleVersions =  new string[] { "NetCore.3.18.0-dev00037" },
		ExpectedResult = new ExpectedResult("Passed")
		{
			Total = 4,
			Passed = 4,
			Failed = 0,
			Warnings = 0,
			Inconclusive = 0,
			Skipped = 0,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "net-2.0") }
		}
	},
	new PackageTest()
	{
		Description = "Project with one assembly, some failures",
		Arguments = "FailingAssemblyNetCore.nunit",
		TestConsoleVersions =  new string[] { "NetCore.3.18.0-dev00037" },
		ExpectedResult = new ExpectedResult("Failed")
		{
			Total = 9,
			Passed = 4,
			Failed = 2,
			Warnings = 0,
			Inconclusive = 1,
			Skipped = 2,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "net-2.0") }
		}
	},
	new PackageTest()
	{
		Description = "Project with both assemblies",
		Arguments = "BothAssembliesNetCore.nunit",
		TestConsoleVersions =  new string[] { "NetCore.3.18.0-dev00037" },
		ExpectedResult = new ExpectedResult("Failed")
		{
			Total = 13,
			Passed = 8,
			Failed = 2,
			Warnings = 0,
			Inconclusive = 1,
			Skipped = 2,
			Assemblies = new[] {
				new ExpectedAssemblyResult("test-lib-1.dll", "net-2.0"),
				new ExpectedAssemblyResult("test-lib-2.dll", "net-2.0")
			}
		}
	}
};

//////////////////////////////////////////////////////////////////////
// PUBLISH
//////////////////////////////////////////////////////////////////////

static bool hadPublishingErrors = false;

Task("PublishPackages")
	.Description("Publish nuget and chocolatey packages according to the current settings")
	.IsDependentOn("PublishToMyGet")
	.IsDependentOn("PublishToNuGet")
	.IsDependentOn("PublishToChocolatey")
	.Does(() =>
	{
		if (hadPublishingErrors)
			throw new Exception("One of the publishing steps failed.");
	});

// This task may either be run by the PublishPackages task,
// which depends on it, or directly when recovering from errors.
Task("PublishToMyGet")
	.Description("Publish packages to MyGet")
	.Does<BuildSettings>((settings) =>
	{
		if (!settings.ShouldPublishToMyGet)
			Information("Nothing to publish to MyGet from this run.");
		else
			try
			{
				PushNuGetPackage(settings.NuGetPackage, settings.MyGetApiKey, settings.MyGetPushUrl);
				PushChocolateyPackage(settings.ChocolateyPackage, settings.MyGetApiKey, settings.MyGetPushUrl);
			}
			catch (Exception)
			{
				hadPublishingErrors = true;
			}
	});

// This task may either be run by the PublishPackages task,
// which depends on it, or directly when recovering from errors.
Task("PublishToNuGet")
	.Description("Publish packages to NuGet")
	.Does<BuildSettings>((settings) =>
	{
		if (!settings.ShouldPublishToNuGet)
			Information("Nothing to publish to NuGet from this run.");
		else
			try
			{
				PushNuGetPackage(settings.NuGetPackage, settings.NuGetApiKey, settings.NuGetPushUrl);
			}
			catch (Exception)
			{
				hadPublishingErrors = true;
			}
	});

// This task may either be run by the PublishPackages task,
// which depends on it, or directly when recovering from errors.
Task("PublishToChocolatey")
	.Description("Publish packages to Chocolatey")
	.Does<BuildSettings>((settings) =>
	{
		if (!settings.ShouldPublishToChocolatey)
			Information("Nothing to publish to Chocolatey from this run.");
		else
			try
			{
				PushChocolateyPackage(settings.ChocolateyPackage, settings.ChocolateyApiKey, settings.ChocolateyPushUrl);
			}
			catch (Exception)
			{
				hadPublishingErrors = true;
			}
	});

//////////////////////////////////////////////////////////////////////
// CREATE A DRAFT RELEASE
//////////////////////////////////////////////////////////////////////

Task("CreateDraftRelease")
	.Does<BuildSettings>((settings) =>
	{
		if (settings.IsReleaseBranch)
		{
			// NOTE: Since this is a release branch, the pre-release label
			// is "pre", which we don't want to use for the draft release.
			// The branch name contains the full information to be used
			// for both the name of the draft release and the milestone,
			// i.e. release-2.0.0, release-2.0.0-beta2, etc.
			string milestone = settings.BranchName.Substring(8);
			string releaseName = $"NUnit Project Loader Extension {milestone}";

			Information($"Creating draft release...");

			try
			{
				GitReleaseManagerCreate(settings.GitHubAccessToken, GITHUB_OWNER, GITHUB_REPO, new GitReleaseManagerCreateSettings()
				{
					Name = releaseName,
					Milestone = milestone
				});
			}
			catch
			{
				Error($"Unable to create draft release for {releaseName}.");
				Error($"Check that there is a {milestone} milestone with at least one closed issue.");
				Error("");
				throw;
			}
		}
		else
		{
			Information("Skipping Release creation because this is not a release branch");
		}
	});

//////////////////////////////////////////////////////////////////////
// CREATE A PRODUCTION RELEASE
//////////////////////////////////////////////////////////////////////

Task("CreateProductionRelease")
	.Does<BuildSettings>((settings) =>
	{
		if (settings.IsProductionRelease)
		{
			string token = settings.GitHubAccessToken;
			string tagName = settings.PackageVersion;
			string assets = $"\"{settings.NuGetPackage},{settings.ChocolateyPackage}\"";

			Information($"Publishing release {tagName} to GitHub");

			GitReleaseManagerAddAssets(token, GITHUB_OWNER, GITHUB_REPO, tagName, assets);
			GitReleaseManagerClose(token, GITHUB_OWNER, GITHUB_REPO, tagName);
		}
		else
		{
			Information("Skipping CreateProductionRelease because this is not a production release");
		}
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
	.IsDependentOn("VerifyNuGetPackage")
	.IsDependentOn("TestNuGetPackage");

Task("PackageChocolatey")
	.IsDependentOn("BuildChocolateyPackage")
	.IsDependentOn("VerifyChocolateyPackage")
	.IsDependentOn("TestChocolateyPackage");

Task("BuildTestAndPackage")
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
	.IsDependentOn("Package")
	.IsDependentOn("PublishPackages")
	.IsDependentOn("CreateDraftRelease")
	.IsDependentOn("CreateProductionRelease");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
