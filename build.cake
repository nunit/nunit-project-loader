#tool nuget:?package=NUnit.ConsoleRunner&version=3.15.5
#tool nuget:?package=NUnit.ConsoleRUnner&version=3.17.0
#tool nuget:?package=NUnit.ConsoleRunner&version=3.18.0-dev00037
#tool nuget:?package=NUnit.ConsoleRunner.NetCore&version=3.18.0-dev00037

// Load scripts 
#load nuget:?package=NUnit.Cake.Recipe&version=1.0.0-dev00001
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
	new PackageTest (
		1, "SingleAssembly_AllTestsPass",
		"Project with one assembly, all tests pass",
		"../../PassingAssembly.nunit",
		new ExpectedResult("Passed") {
			Total = 4, Passed = 4, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "net-4.6.2") } } ),

	new PackageTest (
		1, "SingleAssembly_SomeTestsFail",
		"Project with one assembly, some failures",
		"../../FailingAssembly.nunit",
		new ExpectedResult("Failed") {
			Total = 9, Passed = 4, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "net-4.6.2") } } ),

	new PackageTest	(
		1, "BothAssembliesTogether",
		"Project with both assemblies",
		"../../BothAssemblies.nunit",
		new ExpectedResult("Failed") {
			Total = 13, Passed = 8, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] {
				new ExpectedAssemblyResult("test-lib-1.dll", "net-4.6.2"),
				new ExpectedAssemblyResult("test-lib-2.dll", "net-4.6.2") } } ),

	new PackageTest (
		1, "SingleNetCoreAssembly_AllTestsPass",
		"Project with one .NET Core assembly, all tests pass",
		"../../PassingAssemblyNetCore.nunit",
		new ExpectedResult("Passed") {
			Total = 4, Passed = 4, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "netcore-6.0") } },
		new IPackageTestRunner[] { (IPackageTestRunner)new NUnitNetCoreConsoleRunner("3.18.0-dev00037") } ),

	new PackageTest (
		1, "SingleNetCoreAssembly_SomeTestsFail",
		"Project with one .NET Core assembly, some failures",
		"../../FailingAssemblyNetCore.nunit",
		new ExpectedResult("Failed") {
			Total = 9, Passed = 4, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] { new ExpectedAssemblyResult("test-lib-2.dll", "netcore-6.0") } },
		new IPackageTestRunner[] { (IPackageTestRunner)new NUnitNetCoreConsoleRunner("3.18.0-dev00037") } ),

	new PackageTest (
		1, "BothNetCoreAssembliesTogether",
		"Project with both .NET Core assemblies",
		"../../BothAssembliesNetCore.nunit",
		new ExpectedResult("Failed") {
			Total = 13, Passed = 8, Failed = 2, Warnings = 0, Inconclusive = 1, Skipped = 2,
			Assemblies = new[] {
				new ExpectedAssemblyResult("test-lib-1.dll", "netcore-6.0"),
				new ExpectedAssemblyResult("test-lib-2.dll", "netcore-6.0") } },
		new IPackageTestRunner[] { (IPackageTestRunner)new NUnitNetCoreConsoleRunner("3.18.0-dev00037") } )
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
		testRunnerSource: new TestRunnerSource(new NUnitConsoleRunner("3.17.0"), new NUnitConsoleRunner("3.15.5"), new NUnitConsoleRunner("3.18.0-dev00037"))
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
		testRunnerSource: new TestRunnerSource(new NUnitConsoleRunner("3.17.0"), new NUnitConsoleRunner("3.15.5"), new NUnitConsoleRunner("3.18.0-dev00037"))
	));

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
