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

