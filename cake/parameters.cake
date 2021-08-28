#load "./constants.cake"
#load "./packaging.cake"
#load "./package-tests.cake"
#load "./test-results.cake"
#load "./test-reports.cake"

using System;

public class BuildParameters
{
	private ISetupContext _context;
	private BuildSystem _buildSystem;

	public BuildParameters(ISetupContext context)
	{
		_context = context;
		_buildSystem = context.BuildSystem();

		ProjectDirectory = _context.Environment.WorkingDirectory.FullPath + "/";

		Configuration = _context.Argument("configuration", DEFAULT_CONFIGURATION);
		PackageVersion = _context.Argument("version", DEFAULT_VERSION);

		// Since we don't pass a version argument
		// in AppVeyor.yml, it will be set to the
		// default. We change it here.
		if (_context.BuildSystem().IsRunningOnAppVeyor)
		{
			var appVeyor = _context.AppVeyor();
			var tag = appVeyor.Environment.Repository.Tag;

			if (tag.IsTag)
			{
				PackageVersion = tag.Name;
			}
			else
			{
				var buildNumber = appVeyor.Environment.Build.Number.ToString("00000");
				var branch = appVeyor.Environment.Repository.Branch;
				var isPullRequest = appVeyor.Environment.PullRequest.IsPullRequest;

				if (branch == "master" && !isPullRequest)
				{
					PackageVersion = DEFAULT_VERSION + "-dev-" + buildNumber;
				}
				else
				{
					var suffix = "-ci-" + buildNumber;

					if (isPullRequest)
						suffix += "-pr-" + appVeyor.Environment.PullRequest.Number;
					else
						suffix += "-" + branch;

					// Nuget limits "special version part" to 20 chars. Add one for the hyphen.
					if (suffix.Length > 21)
						suffix = suffix.Substring(0, 21);

					suffix = suffix.Replace(".", "");

					PackageVersion = DEFAULT_VERSION + suffix;
				}

				appVeyor.UpdateBuildVersion(PackageVersion + "-" + appVeyor.Environment.Build.Number);
			}
		}
	}

	public ICakeContext Context => _context;

	public string Configuration { get; }
	public string PackageVersion { get; set; }

	// Directories
	public string ProjectDirectory { get; }
	public string OutputDirectory => ProjectDirectory + "bin/" + Configuration + "/";
	public string Net20OutputDirectory => OutputDirectory + "net20/";
	public string NetCore21OutputDirectory => OutputDirectory + "netcoreapp2.1/";
	public string PackageDirectory => ProjectDirectory + "output/";
	public string ToolsDirectory => ProjectDirectory + "tools/";
	public string NuGetInstallDirectory => ToolsDirectory + NUGET_ID + "/";
	public string ChocolateyInstallDirectory => ToolsDirectory + CHOCO_ID + "/";

	public string NuGetPackageName => NUGET_ID + "." + PackageVersion + ".nupkg";
	public string ChocolateyPackageName => CHOCO_ID + "." + PackageVersion + ".nupkg";

	public string NuGetPackage => PackageDirectory + NuGetPackageName;
	public string ChocolateyPackage => PackageDirectory + ChocolateyPackageName;

	public string GetPathToConsoleRunner(string version)
	{
		return ToolsDirectory + "NUnit.ConsoleRunner." + version + "/tools/nunit3-console.exe";
	}
}
