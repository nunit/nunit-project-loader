// This file contains both constants and static readonly values, which
// are used as constants. The latter must not depend in any way on the
// contents of other cake files, which are loaded after this one.

// Files
const string SOLUTION_FILE = "nunit-project-loader.sln";
const string OUTPUT_ASSEMBLY = "nunit-project-loader.dll";
const string UNIT_TEST_ASSEMBLY = "nunit-project-loader.tests.dll";
const string MOCK_ASSEMBLY = "mock-assembly.dll";

// Packaging
const string NUGET_ID = "NUnit.Extension.NUnitProjectLoader";
const string CHOCO_ID = "nunit-extension-nunit-project-loader";
//const string GITHUB_SITE = "https://github.com/nunit/nunit-project-loader";
//const string WIKI_PAGE = "https://github.com/nunit/docs/wiki/Console-Command-Line";

// Package sources for nuget restore
static readonly string[] PACKAGE_SOURCES = new string[]
{
	"https://www.nuget.org/api/v2",
	"https://www.myget.org/F/nunit/api/v2"
};

// We don't support running tests built with .net core yet
// var TEST_TARGET_FRAMEWORKS = TARGET_FRAMEWORKS
var TEST_TARGET_FRAMEWORKS = new[] { "net20" };

// URLs for uploading packages
private const string MYGET_PUSH_URL = "https://www.myget.org/F/testcentric/api/v2";
private const string NUGET_PUSH_URL = "https://api.nuget.org/v3/index.json";
private const string CHOCO_PUSH_URL = "https://push.chocolatey.org/";

// Environment Variable names holding API keys
private const string MYGET_API_KEY = "MYGET_API_KEY";
private const string NUGET_API_KEY = "NUGET_API_KEY";
private const string CHOCO_API_KEY = "CHOCO_API_KEY";

// Environment Variable names holding GitHub identity of user
private const string GITHUB_OWNER = "NUnit";
private const string GITHUB_REPO = "nunit-project-loader";
// Access token is used by GitReleaseManager
private const string GITHUB_ACCESS_TOKEN = "GITHUB_ACCESS_TOKEN";

// Pre-release labels that we publish
private static readonly string[] LABELS_WE_PUBLISH_ON_MYGET = { "dev", "pre" };
private static readonly string[] LABELS_WE_PUBLISH_ON_NUGET = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_PUBLISH_ON_CHOCOLATEY = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_RELEASE_ON_GITHUB = { "alpha", "beta", "rc" };
