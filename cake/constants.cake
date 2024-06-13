////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////

const string SOLUTION_FILE = "nunit-project-loader.sln";
const string NUGET_ID = "NUnit.Extension.NUnitProjectLoader";
const string CHOCO_ID = "nunit-extension-nunit-project-loader";
const string GITHUB_OWNER = "nunit";
const string GITHUB_REPO = "nunit-project-loader";
const string DEFAULT_CONFIGURATION = "Release";
// Make sure that all the following console versions are installed using #tool
// For the net core runner, prefix version here with "NetCore" e.g.: "NetCore.3.7.1"
static readonly string[] CONSOLE_VERSIONS_FOR_PACKAGE_TESTS = new string[] { "3.17.0", "3.15.5", "3.18.0-dev00037" };
const string CONSOLE_VERSION_FOR_UNIT_TESTS = "3.17.0";

// URLs for uploading packages
private const string MYGET_PUSH_URL = "https://www.myget.org/F/nunit/api/v2";
private const string NUGET_PUSH_URL = "https://api.nuget.org/v3/index.json";
private const string CHOCO_PUSH_URL = "https://push.chocolatey.org/";

// Environment Variable names holding API keys
private const string MYGET_API_KEY = "MYGET_API_KEY";
private const string NUGET_API_KEY = "NUGET_API_KEY";
private const string CHOCO_API_KEY = "CHOCO_API_KEY";
private const string GITHUB_ACCESS_TOKEN = "GITHUB_ACCESS_TOKEN";

// Pre-release labels that we publish
private static readonly string[] LABELS_WE_PUBLISH_ON_MYGET = { "dev", "pre" };
private static readonly string[] LABELS_WE_PUBLISH_ON_NUGET = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_PUBLISH_ON_CHOCOLATEY = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_RELEASE_ON_GITHUB = { "alpha", "beta", "rc" };
