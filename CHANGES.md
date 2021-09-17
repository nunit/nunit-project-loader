## NUnit Project Loader Extension 3.7.1 - September 17, 2021

This release serves as a final test of our build automation. 
There are no changes other than those related to the build.

### Build

* 35 Update Cake to version 1.2.0
* 37 Change default branch from master to main
* 38 Standardize build scripts for extensions
* 41 Automate the GitHub release process

## NUnit Project Loader Extension 3.7.0 - August 29, 2021

No new features or bug fixes but includes changes in the build script
to facilitate moving to the next major version of the extension, v4.0.

This is expected to be the final release of the 3.x series.
  
### Issues Resolved

* 17 Add functional package tests to build
* 18 Automate publication of packages
* 20 Update Cake version and get build working again
* 23 Refactor Cake scripts
* 33 Specify console versions for each test separately

## NUnit Project Loader Extension 3.6.0 - July 28, 2017

Fixes several packaging errors and adds a new chocolatey package. Runners and engines
installed under chocolatey will see and make use of this package.

### Issues Resolved

 * 2 Change API reference to released version
 * 4 Create file of constants for XML element and attribute names
 * 7 No license file in NuGet package
 * 8 Integrate chocolatey package in release build script

## NUnit Project Loader Extension 3.5.0 - October 6, 2016

The first independent release of the nunit-project-loader extension.
