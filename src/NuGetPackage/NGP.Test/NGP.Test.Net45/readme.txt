This is a .NET Framework 4.5 Unit Test project for the Portable Class Library for SQLite NuGet Package v3.8.2.0

In order to build and run the project, download the SQLite precompiled binary for Windows v3.8.2 from the SQLite.org site (http://sqlite.org/2013/sqlite-dll-win32-x86-3080200.zip), extract the sqlite3.dll file, and add it as Copy-Always Content to the root folder of the project.
You should target x86 platform architecture, or Any CPU with Prefer 32-bit option marked, and verify that the Default Processor Architecture option is set to X86 (Test -> Test Settings). 
Targeting x64 platform architecture is not supported.