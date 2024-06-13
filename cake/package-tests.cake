using System.Xml;

//////////////////////////////////////////////////////////////////////
// PACKAGE TEST
//////////////////////////////////////////////////////////////////////

public class PackageTest
{
	public string Description { get; set; }
	public string[] TestConsoleVersions { get; set; }
	public string Arguments { get; set; }
	public ExpectedResult ExpectedResult { get; set; }
}

//////////////////////////////////////////////////////////////////////
// PACKAGE TESTER
//////////////////////////////////////////////////////////////////////

const string DEFAULT_TEST_RESULT_FILE = "TestResult.xml";

public abstract class PackageTester
{
	protected BuildSettings _settings;
	protected ICakeContext _context;

	public PackageTester(BuildSettings settings)
    {
		_settings = settings;
		_context = settings.Context;
	}

	protected abstract string PackageName { get; }
	protected abstract string PackageUnderTest { get; }
	public abstract string InstallDirectory { get; }

	public void RunPackageTests(IList<PackageTest> packageTests)
    {
		var reporter = new ResultReporter(PackageName);

		foreach (var packageTest in packageTests)
		{
			var resultFile = _settings.ProjectDirectory + DEFAULT_TEST_RESULT_FILE;

			foreach (var consoleVersion in packageTest.TestConsoleVersions)
			{
				// Delete result file ahead of time so we don't mistakenly
				// read a left-over file from another test run. Leave the
				// file after the run in case we need it to debug a failure.
				if (_context.FileExists(resultFile))
					_context.DeleteFile(resultFile);

				DisplayBanner(packageTest.Description + " - Console Version " + consoleVersion);

				RunConsoleTest(consoleVersion, packageTest.Arguments);

				try
				{
					var result = new ActualResult(_settings.ProjectDirectory + DEFAULT_TEST_RESULT_FILE);
					var report = new PackageTestReport(packageTest, consoleVersion, result);
					reporter.AddReport(report);

					Console.WriteLine(report.Errors.Count == 0
						? "\nSUCCESS: Test Result matches expected result!"
						: "\nERROR: Test Result not as expected!");
				}
				catch (Exception ex)
				{
					reporter.AddReport(new PackageTestReport(packageTest, consoleVersion, ex));

					Console.WriteLine($"\nERROR: No result found.");
					Console.WriteLine(ex.ToString());
				}
			}
		}

		bool anyErrors = reporter.ReportResults();
		Console.WriteLine();

		// All package tests are run even if one of them fails. If there are
		// any errors,  we stop the run at this point.
		if (anyErrors)
			throw new Exception("One or more package tests had errors!");
	}

	private void RunConsoleTest(string consoleVersion, string arguments)
    {
		bool isNetCoreRunner = consoleVersion.StartsWith("NetCore.");

		string runnerDir = _settings.ToolsDirectory + $"NUnit.ConsoleRunner.{consoleVersion}/tools/";
		if (isNetCoreRunner) runnerDir += "net6.0/";

		var runner = runnerDir + "nunit3-console.exe";

		if (InstallDirectory.EndsWith(CHOCO_ID + "/"))
		{
			// We are using nuget packages for the runner, so it won't normally recognize
			// chocolatey packages. We add an extra addins file for that purpose.
			using (var writer = new StreamWriter(runnerDir + "/choco.engine.addins"))
			{
				writer.WriteLine("../../nunit-extension-*/tools/");
				writer.WriteLine("../../nunit-extension-*/tools/*/");
				writer.WriteLine("../../../nunit-extension-*/tools/");
				writer.WriteLine("../../../nunit-extension-*/tools/*/");
			}
		}
		
		//if (isNetCoreRunner)
		//	_context.StartProcess("dotnet", $"\"{runnerDir}nunit3-console.exe\" {arguments}");
		//else
			_context.StartProcess(runner, arguments);
		// We don't check the error code because we know that
		// mock-assembly returns -4 due to a bad fixture.

		// Should have created the result file
		if (!_context.FileExists(DEFAULT_TEST_RESULT_FILE))
			throw new System.Exception("The result file was not created.");
	}

	private void DisplayBanner(string message)
	{
		Console.WriteLine();
		Console.WriteLine("=======================================================");
		Console.WriteLine(message);
		Console.WriteLine("=======================================================");
	}
}

public class NuGetPackageTester : PackageTester
{
    public NuGetPackageTester(BuildSettings settings) : base(settings) { }

	protected override string PackageName => _settings.NuGetPackageName;
	protected override string PackageUnderTest => _settings.NuGetPackage;
	public override string InstallDirectory => _settings.NuGetInstallDirectory;
}

public class ChocolateyPackageTester : PackageTester
{
    public ChocolateyPackageTester(BuildSettings settings) : base(settings) { }

	protected override string PackageName => _settings.ChocolateyPackageName;
	protected override string PackageUnderTest => _settings.ChocolateyPackage;
	public override string InstallDirectory => _settings.ChocolateyInstallDirectory;
}
