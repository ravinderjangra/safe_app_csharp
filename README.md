# safe_app_csharp [![NuGet](https://img.shields.io/nuget/v/MaidSafe.SafeApp.svg)](https://www.nuget.org/packages/MaidSafe.SafeApp)

.NET wrapper package for [safe-api](https://github.com/maidsafe/safe-api/).

> [safe_app](https://github.com/maidsafe/safe-api/) is a native library which exposes high level API for application development on SAFE Network. It exposes API for authorisation and to manage data on the network.

**Maintainer:** Ravinder Jangra (ravinder.jangra@maidsafe.net)

## Build Status

| CI service | Platform | Status |
|---|---|---|
| Azure DevOps | .NET Core MacOS, Android x86_64, iOS | [![Build status](https://dev.azure.com/maidsafe/SafeApp/_apis/build/status/SafeApp-Mobile-CI?branchName=master)](https://dev.azure.com/maidsafe/SafeApp/_build/latest?definitionId=7&branchName=master) |
| Azure DevOps | .NET Core Linux | [![Build Status](https://dev.azure.com/maidsafe/SafeApp/_apis/build/status/SafeApp-Linux-CI?branchName=master)](https://dev.azure.com/maidsafe/SafeApp/_build/latest?definitionId=12&branchName=master) |
| AppVeyor | .NET Core Windows | [![Build status](https://ci.appveyor.com/api/projects/status/x3m722rvosw2coao/branch/master?svg=true)](https://ci.appveyor.com/project/MaidSafe-QA/safe-app-csharp/branch/master) [![Coverage Status](https://coveralls.io/repos/github/maidsafe/safe_app_csharp/badge.svg?branch=master)](https://coveralls.io/github/maidsafe/safe_app_csharp?branch=master)| |

## Table of Contents

1. [Overview](#Overview)
2. [Supported Platforms](#Supported-Platforms)
3. [API Usage](#API-Usage)
4. [Documentation](#Documentation)
    * [Tutorials & example applications](#Tutorials-and-example-applications)
5. [Development](#Development)
    * [Project Structure](#Project-structure)
    * [Platform Invoke](#Interoperability-between-C-managed-and-unmanaged-code)
    * [Interfacing with SCL](#Interfacing-with-Safe-Client-Libs)
    * [Tests](#Tests)
    * [Packaging](#Packaging)
    * [Tools required](#Tools-required)
6. [Contributing](#Contributing)
7. [Useful resources](#Useful-resources)
8. [Copyrights](#Copyrights)
9. [Further Help](#Further-Help)
10. [License](#License)

This project contains the C# bindings and API wrappers for the [safe_app](https://github.com/maidsafe/safe-api/) and mock [safe_authenticator](https://github.com/maidsafe/safe_client_libs/tree/master/safe_authenticator). The native libraries, bindings and API wrapper are built and published as a NuGet package. The latest version can be fetched from the [MaidSafe.SafeApp NuGet package](https://www.nuget.org/packages/MaidSafe.SafeApp/).

At a very high level, this package includes:

* C# API for devs for easy app development.
* safe-api and mock safe_authenticator bindings. These bindings are one to one mapping to the FFI functions exposed from safe_api and safe_authenicator native libraries.
* Native libraries generated from [safe-api](https://github.com/maidsafe/safe-api) containing required logic to connect, read and write data on the SAFE Network.

## Supported Platforms

* Xamarin.Android ( >=5.0. ABI: armeabi-v7a, x86_64)
* Xamarin.iOS ( >= 1.0, ABI: ARM64, x64)
* .NET Standard 2.0 (for usage via portable libs)
* .NET Core 2.2 (for use via .NET Core targets. Runtime support limited to x64)
* .NET Framework 4.7.2 (for use via classic .NET Framework targets. Platform support limited to x64)

## API Usage

To develop desktop and mobile apps for the SAFE Network install the latest [MaidSafe.SafeApp](https://www.nuget.org/packages/MaidSafe.SafeApp/) package from NuGet.

This package provides support for mock and non-mock network. By default, non-mock API are used in the package.

### Using Mock API

* Mock API can be used by adding a `SAFE_APP_MOCK` flag in your project properties at **Properties > Build > conditional compilation symbols**.
* When the mock feature is used, a local mock vault file is generated which simulates network operations used to store and retrieve data. The app will then interface with this file rather than the live SAFE network.

### Authentication

* Applications must be authenticated via the SAFE Authenticator to work with the SAFE Network.
* The desktop authenticator is packed and shipped with the [SAFE browser](https://github.com/maidsafe/safe_browser/releases/latest).
* On mobile devices, use the [SAFE Authenticator](https://github.com/maidsafe/safe-authenticator-mobile/releases/latest) mobile application.

## Documentation

The documentation for the latest `safe_app_csharp` API is available at [docs.maidsafe.net/safe_app_csharp](http://docs.maidsafe.net/safe_app_csharp/).

We use [DocFX](https://github.com/dotnet/docfx) to generate static HTML API documentation pages from XML code comments. The API docs are generated and published automatically during the CI build.

To generate a local copy of the API docs, [install DocFX](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html#2-use-docfx-as-a-command-line-tool) and run the following command:

```
docfx .\docs\docfx.json
```

### Tutorials and example applications

The following tutorials and examples will help you get started with development of desktop and mobile applications for the SAFE network.

* [Building a SAFE CLI application using .NET Framework](https://hub.safedev.org/platform/dotnet/) - Desktop CLI application for Windows
* [Building a cross platform mobile application for SAFE](https://hub.safedev.org/platform/xamarin/) - Cross platform mobile application using Xamarin.Forms
* [safe-email-app-csharp](https://github.com/maidsafe/safe-email-app-csharp) * Example email application build using Xamarin.Forms

## Development

### Project structure

* **SafeApp:** C# API for safe_api
  * Fetch, Inspect, Files, Keys, Wallet, XorUrl
* **SafeApp.AppBindings:**
  * safe_api and safe_app IPC bindings generated from safe_api and safe_client_libs
  * Contains native libraries for the platform
* **SafeApp.MockAuthBindings:**
  * Mock Safe authentication C# API
  * mock safe_authenticator bindings generated from safe_client_libs
  * Classes required for mock auth funtionality
* **SafeApp.Core:** Contains
  * Constants used in SafeApp
  * Binding utilities and helper functions

### Interoperability between C# managed and unmanaged code

[Platform invoke](https://www.mono-project.com/docs/advanced/pinvoke/) is a service that enables managed code to call unmanaged functions that are implemented in dynamic link libraries or native libraries. It locates and invokes an exported function and marshals its arguments (integers, strings, arrays, structures, and so on) across the interoperation boundary as needed. Check links in [useful resources](#Useful-resources) section to know more about how P/Invoke works in different .NET environments and platforms.

### Interfacing with Safe Client Libs

The package uses native code written in Rust and compiled into platform specific code. Learn more about the safe_client_libs in [the SAFE client libraries wiki](https://github.com/maidsafe/safe_client_libs/wiki).

Instructions to update the bindings can be found in the [Update Bindings file](./UpdateBindings.md).

### Tests

We use shared unit tests for `safe_app` and mock `safe_authenticator` API which can be run on all supported platforms.

### Packaging

Instructions to generate the NuGet package can be found in the [Package Instructions file](
https://github.com/maidsafe/safe_app_csharp/blob/master/PackageInstructions.txt).

### Tools required

* [Visual Studio](https://visualstudio.microsoft.com/) 2017 or later editions with the following workloads installed:
  * [Mobile development with .NET (Xamarin)](https://visualstudio.microsoft.com/vs/visual-studio-workloads/)
  * [.NET desktop development (.NET framework)](https://visualstudio.microsoft.com/vs/visual-studio-workloads/)
  * [.NET Core](https://dotnet.microsoft.com/download)
* [Docfx](https://github.com/dotnet/docfx) - to generate the API documentation
* [Cake](https://cakebuild.net/) - Cross-platform build script tool used to build the projects and run the tests.

## Contributing

As an open source project we're excited to accept contributions to the code from outside of MaidSafe, and are striving to make that as easy and clean as possible.

With enforced linting and commit style clearly layed out, as well as a list of more accessible issues for any project labeled with Help Wanted.

### Project board

GitHub project boards are used by the maintainers of this repository to keep track and organise development priorities.

There could be one or more active project boards for a repository. One main project will be used to manage all tasks corresponding to the main development stream (master branch). A separate project may be used to manage each PoC and/or prototyping development, and each of them will track a dedicated development branch.

New features which imply big number of changes will be developed in a separate branch but tracked in the same main project board, re-basing it with master branch regularly, and fully testing the feature on its branch before it's merged onto the master branch after it was fully approved.

The main project contains the following Kanban columns to track the status of each development task:

* `Triage`: New issues which need to be reviewed and evaluated before taking the decision to implement it.
* `Low Priority`: Issues that will be picked up in the current milestone.
* `In Progress`: Task is assigned to a person and it's in progress.
* `Needs Review`: A Pull Request which completes the task has been sent and it needs to be reviewed.
* `Reviewer approved`: The PR sent was approved by reviewer/s and it's ready for merge.
* `Ready for QA`: The fix for the issue has been merged into master and is ready for final QA testing.
* `Done`: QA has verified that the fix is complete and does not affect anything else.

### Issues

Issues should clearly lay out the problem, platforms experienced on, as well as steps to reproduce the issue.

This aids in fixing the issues but also quality assurance, to check that the issue has indeed been fixed.

Issues are labeled in the following way depending on its type:

* `bug`: The issue is a bug in the product.
* `feature`: The issue is a new and inexistent feature to be implemented.
* `enhancement`: The issue is an enhancement to either an existing feature in the product, or to the infrastructure around the development process.
* `blocked`: The issue cannot be resolved as it is blocked by another task. In this case the task that it is blocked by should be referenced.
* `documentation`: A documentation-related task.
* `e/__` : Specifies the effort required for the task.
* `p/__` : Specifies the priority of the task.

### Commits and Pull Requests

Commit message should follow [these guidelines](https://github.com/autumnai/leaf/blob/master/CONTRIBUTING.md#git-commit-guidelines) and should therefore strive to tackle one issue/feature, and code should be pre-linted before commit.

PRs should clearly link to an issue to be tracked on the project board. A PR that implements/fixes an issue is linked using one of the [GitHub keywords](https://help.github.com/articles/closing-issues-using-keywords). Although these type of PRs will not be added themselves to a project board (just to avoid redundancy with the linked issue). However, PRs which were sent spontaneously and not linked to any existing issue will be added to the project and should go through the same process as any other tasks/issues.

Where appropriate, commits should _always_ contain tests for the code in question.

### Changelog and releases

The change log is currently maintained manually, each PR sent is expected to have the corresponding modification in the CHANGELOG file, under the 'Not released' section.

The release process is triggered by the maintainers of the package once it is merged to master.

## Useful resources

* [Using High-Performance C++ Libraries in Cross-Platform Xamarin.Forms Applications](https://devblogs.microsoft.com/xamarin/using-c-libraries-xamarin-forms-apps/)
* [Native interoperability](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/)
* [Interop with Native Libraries](https://www.mono-project.com/docs/advanced/pinvoke/)
* [Using Native Libraries in Xamarin.Android](https://docs.microsoft.com/en-us/xamarin/android/platform/native-libraries)
* [Referencing Native Libraries in Xamarin.iOS](https://docs.microsoft.com/en-us/xamarin/ios/platform/native-interop)

## Copyrights

Copyrights in the SAFE Network are retained by their contributors. No copyright assignment is required to contribute to this project.

## Further Help

Get your developer related questions clarified on [SAFE Dev Forum](https://forum.safedev.org/). If you're looking to share any other ideas or thoughts on the SAFE Network you can reach out on [SAFE Network Forum](https://safenetforum.org/)

## License

This SAFE Network library is dual-licensed under the Modified BSD ([LICENSE-BSD](LICENSE-BSD) https://opensource.org/licenses/BSD-3-Clause) or the MIT license ([LICENSE-MIT](LICENSE-MIT) https://opensource.org/licenses/MIT) at your option.
