using System.Xml;

//////////////////////////////////////////////////////////////////////
// PACKAGE TEST
//////////////////////////////////////////////////////////////////////

public class PackageTest
{
	public string Description { get; set; }
	public string ConsoleVersion { get; set; }
	public string Arguments { get; set; }
	public ExpectedResult ExpectedResult { get; set; }
}

//////////////////////////////////////////////////////////////////////
// PACKAGE TESTER
//////////////////////////////////////////////////////////////////////

const string DEFAULT_TEST_RESULT_FILE = "TestResult.xml";
static readonly string[] TEST_CONSOLE_VERSIONS = new[] { "3.12.0", "3.11.1", "3.10.0" };
public abstract class PackageTester
{
	protected BuildParameters _parameters;
	protected ICakeContext _context;

	public PackageTester(BuildParameters parameters)
    {
		_parameters = parameters;
		_context = parameters.Context;

		foreach (var consoleVersion in TEST_CONSOLE_VERSIONS)
		{
			PackageTests.Add(new PackageTest()
			{
				Description = "Project with one assembly, all tests pass",
				Arguments = "PassingAssembly.nunit",
				ConsoleVersion = consoleVersion,
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
			});
			PackageTests.Add(new PackageTest()
			{
				Description = "Project with one assembly, some failures",
				Arguments = "FailingAssembly.nunit",
				ConsoleVersion = consoleVersion,
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
			});
			PackageTests.Add(new PackageTest()
			{
				Description = "Project with both assemblies",
				Arguments = "BothAssemblies.nunit",
				ConsoleVersion = consoleVersion,
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
			});
		}
		//PackageTests.Add(new PackageTest()
		//{
		//	Description = "Run project with one assembly under 3.11.1 console",
		//	Arguments = "OneAssembly.nunit",
		//	ExpectedResult = new ExpectedResult("Passed")
		//	{
		//		Total = 4,
		//		Passed = 4,
		//		Failed = 0,
		//		Warnings = 0,
		//		Inconclusive = 0,
		//		Skipped = 0,
		//		Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "net-2.0") }
		//	}
		//});
		//PackageTests.Add(new PackageTest()
		//{
		//	Description = "Run project with one assembly under 3.10.0 console",
		//	Arguments = "OneAssembly.nunit",
		//	ConsoleVersion = "3.10.0",
		//	ExpectedResult = new ExpectedResult("Passed")
		//	{
		//		Total = 4,
		//		Passed = 4,
		//		Failed = 0,
		//		Warnings = 0,
		//		Inconclusive = 0,
		//		Skipped = 0,
		//		Assemblies = new[] { new ExpectedAssemblyResult("test-lib-1.dll", "net-2.0") }
		//	}
		//});
	}

	protected abstract string PackageName { get; }
	protected abstract string PackageUnderTest { get; }
	public abstract string InstallDirectory { get; }
	public abstract PackageCheck[] PackageChecks { get; }
	public List<PackageTest> PackageTests = new List<PackageTest>();

	public void InstallPackage()
	{
		_context.CleanDirectory(InstallDirectory);
		_context.Unzip(PackageUnderTest, InstallDirectory);
	}

	public void VerifyPackage()
    {
		_context.Information("Verifying NuGet package content...");
		Check.That(InstallDirectory, PackageChecks);
		_context.Information("Verification was successful!");
	}

	public void RunPackageTests()
    {
		var reporter = new ResultReporter(PackageName);

		foreach (var packageTest in PackageTests)
		{
				var resultFile = _parameters.OutputDirectory + DEFAULT_TEST_RESULT_FILE;
				// Delete result file ahead of time so we don't mistakenly
				// read a left-over file from another test run. Leave the
				// file after the run in case we need it to debug a failure.
				if (_context.FileExists(resultFile))
					_context.DeleteFile(resultFile);

				DisplayBanner(packageTest.Description + " - Console Version " + packageTest.ConsoleVersion);

				RunConsoleTest(packageTest.ConsoleVersion, packageTest.Arguments);

			try
			{
				var result = new ActualResult(_parameters.ProjectDirectory + DEFAULT_TEST_RESULT_FILE);
				var report = new PackageTestReport(packageTest, result);
				reporter.AddReport(report);

				Console.WriteLine(report.Errors.Count == 0
					? "\nSUCCESS: Test Result matches expected result!"
					: "\nERROR: Test Result not as expected!");
			}
			catch (Exception ex)
			{
				reporter.AddReport(new PackageTestReport(packageTest, ex));

				Console.WriteLine($"\nERROR: No result found.");
				Console.WriteLine(ex.ToString());
			}
		}

		bool anyErrors = reporter.ReportResults();
		Console.WriteLine();

		// All package tests are run even if one of them fails. If there are
		// any errors,  we stop the run at this point.
		if (anyErrors)
			throw new Exception("One or more package tests had errors!");
	}

	public void UninstallPackage()
	{
		_context.DeleteDirectory(InstallDirectory, new DeleteDirectorySettings() { Recursive = true });
	}

	private void RunConsoleTest(string consoleVersion, string arguments)
    {
		string runner = _parameters.GetPathToConsoleRunner(consoleVersion);

		if (InstallDirectory.EndsWith(CHOCO_ID + "/"))
		{
			// We are using nuget packages for the runner, so add an extra
			// addins file to allow detecting chocolatey packages
			string runnerDir = System.IO.Path.GetDirectoryName(runner);
			using (var writer = new StreamWriter(runnerDir + "/choco.engine.addins"))
				writer.WriteLine("../../nunit-extension-*/tools/");
		}

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
    public NuGetPackageTester(BuildParameters parameters) : base(parameters) { }

	protected override string PackageName => _parameters.NuGetPackageName;
	protected override string PackageUnderTest => _parameters.NuGetPackage;
	public override string InstallDirectory => _parameters.NuGetInstallDirectory;
	public override PackageCheck[] PackageChecks => new PackageCheck[]
	{
		HasFiles("CHANGES.txt", "LICENSE.txt"),
		HasDirectory("tools").WithFile("nunit-project-loader.dll")
    };
}

public class ChocolateyPackageTester : PackageTester
{
    public ChocolateyPackageTester(BuildParameters parameters) : base(parameters) { }

	protected override string PackageName => _parameters.ChocolateyPackageName;
	protected override string PackageUnderTest => _parameters.ChocolateyPackage;
	public override string InstallDirectory => _parameters.ChocolateyInstallDirectory;
	public override PackageCheck[] PackageChecks => new PackageCheck[]
	{
		HasDirectory("tools").WithFiles("CHANGES.txt", "LICENSE.txt", "VERIFICATION.txt"),
		HasDirectory("tools").WithFile("nunit-project-loader.dll")
    };
}

//////////////////////////////////////////////////////////////////////
// SYNTAX FOR EXPRESSING CHECKS
//////////////////////////////////////////////////////////////////////

private static class Check
{
	public static void That(string testDir, params PackageCheck[] checks)
    {
		foreach (var check in checks)
			check.ApplyTo(testDir);
    }
}

private static FileCheck HasFile(string file) => HasFiles(new[] { file });
private static FileCheck HasFiles(params string[] files) => new FileCheck(files);

private static DirectoryCheck HasDirectory(string dir) => new DirectoryCheck(dir);

//////////////////////////////////////////////////////////////////////
// PACKAGECHECK CLASS
//////////////////////////////////////////////////////////////////////

public abstract class PackageCheck
{
	public abstract void ApplyTo(string testDir);
}

//////////////////////////////////////////////////////////////////////
// FILECHECK CLASS
//////////////////////////////////////////////////////////////////////

public class FileCheck : PackageCheck
{
	string[] _files;

	public FileCheck(string[] files)
	{
		_files = files;
	}

	public override void ApplyTo(string testDir)
	{
		foreach (string file in _files)
		{
			if (!System.IO.File.Exists(System.IO.Path.Combine(testDir, file)))
				throw new Exception($"File {file} was not found.");
		}
	}
}

//////////////////////////////////////////////////////////////////////
// DIRECTORYCHECK CLASS
//////////////////////////////////////////////////////////////////////

public class DirectoryCheck : PackageCheck
{
	private string _path;
	private List<string> _files = new List<string>();

	public DirectoryCheck(string path)
	{
		_path = path;
	}

	public DirectoryCheck WithFiles(params string[] files)
	{
		_files.AddRange(files);
		return this;
	}

	public DirectoryCheck WithFile(string file)
	{
		_files.Add(file);
		return this;
	}

	public override void ApplyTo(string testDir)
	{
		string combinedPath = System.IO.Path.Combine(testDir, _path);

		if (!System.IO.Directory.Exists(combinedPath))
			throw new Exception($"Directory {_path} was not found.");

		if (_files != null)
		{
			foreach (var file in _files)
			{
				if (!System.IO.File.Exists(System.IO.Path.Combine(combinedPath, file)))
					throw new Exception($"File {file} was not found in directory {_path}.");
			}
		}
	}
}
