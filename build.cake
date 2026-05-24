#tool nuget:?package=NUnit.ConsoleRunner&version=4.0.0-beta.2.2
#tool dotnet:?package=NUnit.ConsoleRunner.NetCore&version=4.0.0-beta.2.2

// Load the recipe 
#load nuget:?package=NUnit.Cake.Recipe&version=2.0.0-beta.3.1
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../NUnit.Cake.Recipe/src/NUnit.Cake.Recipe/content/*.cake

// Initialize BuildSettings
BuildSettings.Initialize(
    Context,
    "NUnit Project Loader",
    "nunit-project-loader",
    solutionFile: "nunit-project-loader.sln",
	standardHeader: new string[] { "// Copyright (c) Charlie Poole and Contributors. MIT License - see LICENSE.txt" },
	unitTestRunner: new NUnitLiteRunner(),
	unitTests: "**/*.Tests.exe");

IPackageTestRunner StandardRunner = new NUnitConsoleRunner("NUnit.ConsoleRunner", "4.0.0-beta.2.2");
IPackageTestRunner DotNetRunner = new NUnit4DotNetRunner("NUnit.ConsoleRunner.NetCore", "4.0.0-beta.2.2");

//////////////////////////////////////////////////////////////////////
// PACKAGE TESTS
//////////////////////////////////////////////////////////////////////

PackageTest[] packageTests = new PackageTest[]
{
	new PackageTest (1, "SingleAssembly_AllTestsPass")
	{
		Description = "Project with one assembly, all tests pass",
		Arguments = "../../PassingAssembly.nunit",
		ExpectedResult = new ExpectedResult("Passed") {
			Total = 4, Passed = 4, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "net-4.6.2") } },
		TestRunner = StandardRunner
    },

	new PackageTest (1, "SingleAssembly_SomeTestsFail")
	{
		Description = "Project with one assembly, some failures",
		Arguments = "../../FailingAssembly.nunit",
		ExpectedResult = new ExpectedResult("Failed") {
			Total = 9, Passed = 4, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "net-4.6.2") } },
		TestRunner = StandardRunner
	},

	new PackageTest (1, "BothAssembliesTogether")
	{
		Description = "Project with both assemblies",
		Arguments = "../../BothAssemblies.nunit",
		ExpectedResult = new ExpectedResult("Failed") {
			Total = 13, Passed = 8, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] {
				new ExpectedAssemblyResult("test-lib-1.dll", "net-4.6.2"),
				new ExpectedAssemblyResult("test-lib-2.dll", "net-4.6.2") } },
			TestRunner = StandardRunner
    },

	new PackageTest (1, "SingleNetCoreAssembly_AllTestsPass")
	{
		Description = "Project with one .NET Core assembly, all tests pass",
		Arguments = "../../PassingAssemblyNetCore.nunit",
		ExpectedResult = new ExpectedResult("Passed") {
			Total = 4, Passed = 4, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "netcore-6.0") } }
	},

	new PackageTest (1, "SingleNetCoreAssembly_SomeTestsFail")
	{
		Description = "Project with one .NET Core assembly, some failures",
		Arguments = "../../FailingAssemblyNetCore.nunit",
		ExpectedResult = new ExpectedResult("Failed") {
			Total = 9, Passed = 4, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "netcore-6.0") } }
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
	}
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
		tests: packageTests,
		testRunners: [ StandardRunner, DotNetRunner]
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
		tests: packageTests,
		testRunners: [ StandardRunner, DotNetRunner]
	));

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
