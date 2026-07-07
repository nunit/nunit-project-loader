#tool nuget:?package=NUnit.ConsoleRunner&version=4.0.0-beta.2.2
#tool nuget:?package=NUnit.ConsoleRunner.NetCore&version=4.0.0-beta.2.2

// NOTE: Because of permission issues in installing the chocolatey
// versions of the NUnit console runner, all tests use the nuget
// packages. The test runners included in the recipe adjust the
// search path for extensions accordingly.

// StandardRunners is used as the default setting for our tests,
// since the standard runner can execute all of them
var StandardRunners = new IPackageTestRunner[] {
    new NUnitConsoleRunner("NUnit.ConsoleRunner", "4.0.0-beta.2.2")
};

var NetCoreRunners = new IPackageTestRunner[] {
	new NUnit3NetCoreConsoleRunner("NUnit.ConsoleRunner.NetCore", "4.0.0-beta.2.2", "tools/net8.0/any/nunit-netcore-console.dll")
};

// For .NET Core tests, we override the default and use AllRunners,
// since both the standard and netcore runners can execute them.
var AllRunners =  new List<IPackageTestRunner>( StandardRunners.Concat(NetCoreRunners) ).ToArray();

// Load the recipe 
#load nuget:?package=NUnit.Cake.Recipe&version=2.0.0-beta.3.3
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../NUnit.Cake.Recipe/src/NUnit.Cake.Recipe/content/*.cake

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

var SingleAssembly_AllTestsPass = new PackageTest (1, "SingleAssembly_AllTestsPass")
{
	Description = "Project with one assembly, all tests pass",
	Arguments = "../../PassingAssembly.nunit",
	ExpectedResult = new ExpectedResult("Passed") {
		Total = 4, Passed = 4, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
		Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "net-4.6.2") } }
};

var SingleAssembly_SomeTestsFail = new PackageTest (1, "SingleAssembly_SomeTestsFail")
{
    Description = "Project with one assembly, some failures",
    Arguments = "../../FailingAssembly.nunit",
    ExpectedResult = new ExpectedResult("Failed") {
		Total = 9, Passed = 4, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
		Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "net-4.6.2") } }
};

var BothAssembliesTogether = new PackageTest (1, "BothAssembliesTogether")
{
    Description = "Project with both assemblies",
    Arguments = "../../BothAssemblies.nunit",
    ExpectedResult = new ExpectedResult("Failed") {
		Total = 13, Passed = 8, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
		Assemblies = new[] {
			new ExpectedAssemblyResult("test-lib-1.dll", "net-4.6.2"),
			new ExpectedAssemblyResult("test-lib-2.dll", "net-4.6.2") } }
};

var SingleNetCoreAssembly_AllTestsPass = new PackageTest (1, "SingleNetCoreAssembly_AllTestsPass")
{
    Description = "Project with one .NET Core assembly, all tests pass",
    Arguments = "../../PassingAssemblyNetCore.nunit",
    ExpectedResult = new ExpectedResult("Passed") {
		Total = 4, Passed = 4, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
		Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "netcore-6.0") } },
	TestRunners = AllRunners
};

var SingleNetCoreAssembly_SomeTestsFail = new PackageTest (1, "SingleNetCoreAssembly_SomeTestsFail")
{
    Description = "Project with one .NET Core assembly, some failures",
    Arguments = "../../FailingAssemblyNetCore.nunit",
    ExpectedResult = new ExpectedResult("Failed") {
		Total = 9, Passed = 4, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
		Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "netcore-6.0") } },
	TestRunners = AllRunners
};

var BothNetCoreAssembliesTogether = new PackageTest (1, "BothNetCoreAssembliesTogether")
{
    Description = "Project with both .NET Core assemblies",
    Arguments = "../../BothAssembliesNetCore.nunit",
    ExpectedResult = new ExpectedResult("Failed") {
		Total = 13, Passed = 8, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
		Assemblies = new[] {
			new ExpectedAssemblyResult("test-lib-1.dll", "netcore-6.0"),
			new ExpectedAssemblyResult("test-lib-2.dll", "netcore-6.0") } },
	TestRunners = AllRunners
};

var ListExtensions = new PackageTest(1, "ListExtensions")
{
    Description = "List all extensions in case of failure",
    Arguments = "--list-extensions"
};

var AllTests = new PackageTest[] {
	SingleAssembly_AllTestsPass,
	SingleAssembly_SomeTestsFail,
	BothAssembliesTogether,
	SingleNetCoreAssembly_AllTestsPass,
	SingleNetCoreAssembly_SomeTestsFail,
	BothNetCoreAssembliesTogether,
	ListExtensions
};

//////////////////////////////////////////////////////////////////////
// NUGET PACKAGE
//////////////////////////////////////////////////////////////////////

BuildSettings.Packages.Add(
	new NuGetPackage(
		"NUnit.Extension.NUnitProjectLoader",
		source: "nuget/nunit-project-loader.nuspec",
		checks: new PackageCheck[] {
			HasFiles("LICENSE.txt", "nunit_256.png"),
			HasDirectory("tools").WithFile("nunit-project-loader.legacy.addins"),
			HasDirectory("tools/net462").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll"),
			HasDirectory("tools/net8.0").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll") },
		tests: AllTests,
		testRunners: StandardRunners
	));

//////////////////////////////////////////////////////////////////////
// CHOCOLATEY PACKAGE
//////////////////////////////////////////////////////////////////////

BuildSettings.Packages.Add(
	new ChocolateyPackage(
		"nunit-extension-nunit-project-loader",
		source: "choco/nunit-project-loader.nuspec",
		checks: new PackageCheck[] {
			HasDirectory("tools").WithFiles("LICENSE.txt", "VERIFICATION.txt", "nunit-project-loader.legacy.addins"),
			HasDirectory("tools/net462").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll"),
			HasDirectory("tools/net8.0").WithFiles("nunit-project-loader.dll", "nunit.engine.api.dll") },
		tests: AllTests,
		testRunners: StandardRunners
	));

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
