// This file contains both constants and static readonly values, which
// are used as constants. The latter must not depend in any way on the
// contents of other cake files, which are loaded after this one.

// Files
const string SOLUTION_FILE = "nunit-project-loader.sln";
const string OUTPUT_ASSEMBLY = "nunit-project-loader.dll";
const string UNIT_TEST_ASSEMBLY = "nunit-project-loader.tests.dll";
const string MOCK_ASSEMBLY = "mock-assembly.dll";

// Directories
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";
var OUTPUT_DIR = PROJECT_DIR + "output/";

// Packaging
const string NUGET_ID = "NUnit.Extension.NUnitProjectLoader";
const string CHOCO_ID = "nunit-extension-nunit-project-loader";
const string GITHUB_SITE = "https://github.com/nunit/nunit-project-loader";
const string WIKI_PAGE = "https://github.com/nunit/docs/wiki/Console-Command-Line";

// Metadata used in the nuget and chocolatey packages
var TITLE = "NUnit 3 - NUnit Project Loader Extension";
var AUTHORS = new[] { "Charlie Poole" };
var OWNERS = new[] { "Charlie Poole" };
var DESCRIPTION = "This extension allows NUnit to recognize and load NUnit projects, file type .nunit.";
var SUMMARY = "NUnit Engine extension for loading NUnit projects.";
var COPYRIGHT = "Copyright (c) 2016 Charlie Poole";
var RELEASE_NOTES = new[] { "See https://raw.githubusercontent.com/nunit/nunit-project-loader/master/CHANGES.txt" };
var TAGS = new[] { "nunit", "test", "testing", "tdd", "runner" };
var PROJECT_URL = new Uri("http://nunit.org");
var ICON_URL = new Uri("https://cdn.rawgit.com/nunit/resources/master/images/icon/nunit_256.png");
var LICENSE_URL = new Uri("http://nunit.org/nuget/nunit3-license.txt");
var PROJECT_SOURCE_URL = new Uri(GITHUB_SITE);
var PACKAGE_SOURCE_URL = new Uri(GITHUB_SITE);
var BUG_TRACKER_URL = new Uri(GITHUB_SITE + "/issues");
var DOCS_URL = new Uri(WIKI_PAGE);
var MAILING_LIST_URL = new Uri("https://groups.google.com/forum/#!forum/nunit-discuss");

// Package sources for nuget restore
static readonly string[] PACKAGE_SOURCES = new string[]
{
	"https://www.nuget.org/api/v2",
	"https://www.myget.org/F/nunit/api/v2"
};

// Not yet used
var TARGET_FRAMEWORKS = new[] { "net20", "netcoreapp2.1" };

// We don't support running tests built with .net core yet
// var TEST_TARGET_FRAMEWORKS = TARGET_FRAMEWORKS
var TEST_TARGET_FRAMEWORKS = new[] { "net20" };

//const string DEFAULT_TEST_RESULT_FILE = "TestResult.xml";

//// URLs for uploading packages
//private const string MYGET_PUSH_URL = "https://www.myget.org/F/testcentric/api/v2";
//private const string NUGET_PUSH_URL = "https://api.nuget.org/v3/index.json";
//private const string CHOCO_PUSH_URL = "https://push.chocolatey.org/";

//// Environment Variable names holding API keys
//private const string MYGET_API_KEY = "MYGET_API_KEY";
//private const string NUGET_API_KEY = "NUGET_API_KEY";
//private const string CHOCO_API_KEY = "CHOCO_API_KEY";

//// Environment Variable names holding GitHub identity of user
//private const string GITHUB_OWNER = "TestCentric";
//private const string GITHUB_REPO = "testcentric-gui";	
//// Access token is used by GitReleaseManager
//private const string GITHUB_ACCESS_TOKEN = "GITHUB_ACCESS_TOKEN";

//// Pre-release labels that we publish
//private static readonly string[] LABELS_WE_PUBLISH_ON_MYGET = { "dev", "pre" };
//private static readonly string[] LABELS_WE_PUBLISH_ON_NUGET = { "alpha", "beta", "rc" };
//private static readonly string[] LABELS_WE_PUBLISH_ON_CHOCOLATEY = { "alpha", "beta", "rc" };
