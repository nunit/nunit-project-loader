#tool nuget:?package=NUnit.ConsoleRunner&version=3.19.2
#tool nuget:?package=NUnit.ConsoleRunner&version=3.18.3
#tool nuget:?package=NUnit.ConsoleRUnner&version=3.17.0
#tool nuget:?package=NUnit.ConsoleRunner&version=3.15.5
#tool nuget:?package=NUnit.ConsoleRunner.NetCore&version=3.19.2
#tool nuget:?package=NUnit.ConsoleRunner.NetCore&version=3.18.3

// Define runners to be used for testing packages
static readonly IPackageTestRunner[] PACKAGE_TEST_RUNNERS = new IPackageTestRunner[] {
	new NUnitConsoleRunner("3.19.2"),
	new NUnitConsoleRunner("3.18.3"),
	new NUnitConsoleRunner("3.17.0"),
	new NUnitConsoleRunner("3.15.5")
	//new NUnit3NetCoreConsoleRunner("3.19.2"),
	//new NUnit3NetCoreConsoleRunner("3.18.3"),
};

// Load the recipe 
#load nuget:?package=NUnit.Cake.Recipe&version=1.4.0-alpha.4

// Comment out above line and uncomment below for local tests of recipe changes
//#load ../NUnit.Cake.Recipe/recipe/*.cake

// Initialize BuildSettings
BuildSettings.Initialize(
    Context,
    "NUnit Project Loader",
    "nunit-project-loader",
    solutionFile: "nunit-project-loader.sln",
	unitTestRunner: new NUnitLiteRunner(),
	unitTests: "**/*.Tests.exe");

//////////////////////////////////////////////////////////////////////
// PACKAGE TESTS
//////////////////////////////////////////////////////////////////////

PackageTest[] PackageTests = new PackageTest[]
{
	new PackageTest (1, "SingleAssembly_AllTestsPass")
	{
        Description = "Project with one assembly, all tests pass",
        Arguments = "../../PassingAssembly.nunit",
        ExpectedResult = new ExpectedResult("Passed") {
			Total = 4, Passed = 4, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "net-4.6.2") } }
	},

	new PackageTest (1, "SingleAssembly_SomeTestsFail")
	{
        Description = "Project with one assembly, some failures",
        Arguments = "../../FailingAssembly.nunit",
        ExpectedResult = new ExpectedResult("Failed") {
			Total = 9, Passed = 4, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "net-4.6.2") } }
	},

	new PackageTest (1, "BothAssembliesTogether")
	{
        Description = "Project with both assemblies",
        Arguments = "../../BothAssemblies.nunit",
        ExpectedResult = new ExpectedResult("Failed") {
			Total = 13, Passed = 8, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] {
				new ExpectedAssemblyResult("test-lib-1.dll", "net-4.6.2"),
				new ExpectedAssemblyResult("test-lib-2.dll", "net-4.6.2") } }
	},

	new PackageTest (1, "SingleNetCoreAssembly_AllTestsPass")
	{
        Description = "Project with one .NET Core assembly, all tests pass",
        Arguments = "../../PassingAssemblyNetCore.nunit",
        ExpectedResult = new ExpectedResult("Passed") {
			Total = 4, Passed = 4, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "netcore-6.0") } }
		//TestRunners = new IPackageTestRunner[] { (IPackageTestRunner)new NUnit3NetCoreConsoleRunner("3.19.2") }
	},

	new PackageTest (1, "SingleNetCoreAssembly_SomeTestsFail")
	{
        Description = "Project with one .NET Core assembly, some failures",
        Arguments = "../../FailingAssemblyNetCore.nunit",
        ExpectedResult = new ExpectedResult("Failed") {
			Total = 9, Passed = 4, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "netcore-6.0") } }
		//TestRunners = new IPackageTestRunner[] { (IPackageTestRunner)new NUnit3NetCoreConsoleRunner("3.19.2") }
	},

	new PackageTest (1, "BothNetCoreAssembliesTogether")
	{
        Description = "Project with both .NET Core assemblies",
        Arguments = "../../BothAssembliesNetCore.nunit",
        ExpectedResult = new ExpectedResult("Failed") {
			Total = 13, Passed = 8, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] {
				new ExpectedAssemblyResult("test-lib-1.dll", "netcore-6.0"),
				new ExpectedAssemblyResult("test-lib-2.dll", "netcore-6.0") } }
		//TestRunners = new IPackageTestRunner[] { (IPackageTestRunner)new NUnit3NetCoreConsoleRunner("3.19.2") }
	}
};

//////////////////////////////////////////////////////////////////////
// NUGET PACKAGE
//////////////////////////////////////////////////////////////////////

BuildSettings.Packages.Add(
	new NuGetPackage(
		"NUnit.Extension.NUnitProjectLoader",
		"nuget/nunit-project-loader.nuspec",
		checks: new PackageCheck[] {
			HasFiles("LICENSE.txt", "nunit_256.png"),
			HasDirectory("tools").WithFile("nunit-project-loader.legacy.addins"),
			HasDirectory("tools/net462").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll"),
			HasDirectory("tools/net6.0").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll") },
		tests: PackageTests,
		testRunners: new IPackageTestRunner[] {
			new NUnitConsoleRunner("3.19.2"),
			new NUnitConsoleRunner("3.18.3"),
			new NUnitConsoleRunner("3.17.0"),
			new NUnitConsoleRunner("3.15.5")
			//new NUnit3NetCoreConsoleRunner("3.19.2"),
			//new NUnit3NetCoreConsoleRunner("3.18.3"),
		}
	));

//////////////////////////////////////////////////////////////////////
// CHOCOLATEY PACKAGE
//////////////////////////////////////////////////////////////////////

BuildSettings.Packages.Add(
	new ChocolateyPackage(
		"nunit-extension-nunit-project-loader",
		"choco/nunit-project-loader.nuspec",
		checks: new PackageCheck[] {
			HasDirectory("tools").WithFiles("LICENSE.txt", "VERIFICATION.txt", "nunit-project-loader.legacy.addins"),
			HasDirectory("tools/net462").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll"),
			HasDirectory("tools/net6.0").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll") },
		tests: PackageTests,
		testRunners: new IPackageTestRunner[] {
			//new NUnitConsoleRunner("3.19.2"),
			new NUnitConsoleRunner("3.18.3"),
			new NUnitConsoleRunner("3.17.0"),
			new NUnitConsoleRunner("3.15.5")
			//new NUnit3NetCoreConsoleRunner("3.19.2"),
			//new NUnit3NetCoreConsoleRunner("3.18.3"),
		}
	));

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
