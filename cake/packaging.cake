﻿//////////////////////////////////////////////////////////////////////
// PACKAGE METADATA
//////////////////////////////////////////////////////////////////////

const string TITLE = "NUnit 3 - NUnit V2 Result Writer Extension";
static readonly string[] AUTHORS = new[] { "Charlie Poole" };
static readonly string[] OWNERS = new[] { "Charlie Poole" };
const string DESCRIPTION = "This extension allows NUnit to create result files in the V2 format, which is used by many CI servers.";
const string SUMMARY = "NUnit Engine extension for writing test result files in NUnit V2 format.";
const string COPYRIGHT = "Copyright (c) 2016 Charlie Poole";
static readonly string[] RELEASE_NOTES = new[] { "See https://raw.githubusercontent.com/nunit/nunit-v2-result-writer/main/CHANGES.txt" };
static readonly string[] TAGS = new[] { "nunit", "test", "testing", "tdd", "runner" };
static readonly Uri PROJECT_URL = new Uri("http://nunit.org");
static readonly Uri ICON_URL = new Uri("https://cdn.rawgit.com/nunit/resources/master/images/icon/nunit_256.png");
static readonly Uri LICENSE_URL = new Uri("http://nunit.org/nuget/nunit3-license.txt");
const string GITHUB_SITE = "https://github.com/nunit/nunit-v2-result-writer";
const string WIKI_PAGE = "https://github.com/nunit/docs/wiki/Console-Command-Line";
static readonly Uri PROJECT_SOURCE_URL = new Uri(GITHUB_SITE);
static readonly Uri PACKAGE_SOURCE_URL = new Uri(GITHUB_SITE);
static readonly Uri BUG_TRACKER_URL = new Uri(GITHUB_SITE + "/issues");
static readonly Uri DOCS_URL = new Uri(WIKI_PAGE);
static readonly Uri MAILING_LIST_URL = new Uri("https://groups.google.com/forum/#!forum/nunit-discuss");

//////////////////////////////////////////////////////////////////////
// BUILD NUGET PACKAGE
//////////////////////////////////////////////////////////////////////

public void BuildNuGetPackage(BuildParameters parameters)
{
		var content = new List<NuSpecContent>();
		content.Add(new NuSpecContent { Source = parameters.ProjectDirectory + "LICENSE.txt" });
		content.Add(new NuSpecContent { Source = parameters.ProjectDirectory + "CHANGES.txt" });
		foreach (string framework in TARGET_FRAMEWORKS)
			content.Add(new NuSpecContent {
				Source = $"{parameters.OutputDirectory}{framework}/nunit-project-loader.dll",
				Target = $"tools/{framework}" });

	NuGetPack(
		new NuGetPackSettings()
		{
			Id = NUGET_ID,
			Version = parameters.PackageVersion,
			Title = TITLE,
			Authors = AUTHORS,
			Owners = OWNERS,
			Description = DESCRIPTION,
			Summary = SUMMARY,
			ProjectUrl = PROJECT_URL,
			IconUrl = ICON_URL,
			//Icon = "nunit.ico", // Waiting for Cake release
			License = new NuSpecLicense() { Type = "expression", Value = "MIT" },
			//LicenseUrl = LICENSE_URL,
			RequireLicenseAcceptance = false,
			Copyright = COPYRIGHT,
			ReleaseNotes = RELEASE_NOTES,
			Tags = TAGS,
			//Language = "en-US",
			OutputDirectory = parameters.PackageDirectory,
			KeepTemporaryNuSpecFile = false,
			Files = content
		});
}

//////////////////////////////////////////////////////////////////////
// BUILD CHOCOLATEY PACKAGE
//////////////////////////////////////////////////////////////////////

public void BuildChocolateyPackage(BuildParameters parameters)
{
    var content = new List<ChocolateyNuSpecContent>();
    content.Add(new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "LICENSE.txt", Target = "tools" });
    content.Add(new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "CHANGES.txt", Target = "tools" });
    content.Add(new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "VERIFICATION.txt", Target = "tools" });
    foreach (string framework in TARGET_FRAMEWORKS)
        content.Add(new ChocolateyNuSpecContent
        {
            Source = $"{parameters.OutputDirectory}{framework}/nunit-project-loader.dll",
            Target = $"tools/{framework}"
        });

    ChocolateyPack(
		new ChocolateyPackSettings()
		{
			Id = CHOCO_ID,
			Version = parameters.PackageVersion,
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
			DocsUrl = DOCS_URL,
			BugTrackerUrl = BUG_TRACKER_URL,
			PackageSourceUrl = PACKAGE_SOURCE_URL,
			MailingListUrl = MAILING_LIST_URL,
			ReleaseNotes = RELEASE_NOTES,
			Tags = TAGS,
				//Language = "en-US",
				OutputDirectory = parameters.PackageDirectory,
			Files = content
		});
}
