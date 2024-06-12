//////////////////////////////////////////////////////////////////////
// GLOBALLY ACCESSIBLE UTILITY METHODS
//////////////////////////////////////////////////////////////////////

// These stand-alone utility methods are only visible from within the
// code for defined Tasks.

public void DeleteObjectDirectories(BuildParameters parameters)
{
    string pattern = parameters.SourceDirectory + "**/obj/";

    foreach (var dir in GetDirectories(pattern))
        DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
}

private void PushNuGetPackage(FilePath package, string apiKey, string url)
{
	CheckPackageExists(package);
	NuGetPush(package, new NuGetPushSettings() { ApiKey = apiKey, Source = url });
}

private void PushChocolateyPackage(FilePath package, string apiKey, string url)
{
	CheckPackageExists(package);
	ChocolateyPush(package, new ChocolateyPushSettings() { ApiKey = apiKey, Source = url });
}

private void CheckPackageExists(FilePath package)
{
	if (!FileExists(package))
		throw new InvalidOperationException(
			$"Package not found: {package.GetFilename()}.\nCode may have changed since package was last built.");
}

private void DeleteEmptyLogFiles(string directory)
{
	DirectoryInfo info = new DirectoryInfo(directory);

	foreach (var file in info.GetFiles("*.log"))
		if (file.Length == 0)
			System.IO.File.Delete(file.FullName);
}
