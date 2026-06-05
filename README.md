# m3Coding: Simple Hash Checker

A Windows desktop app for quickly calculating file hashes and comparing them with an expected digest.

## Features

- Drag and drop a file into the app
- Browse for a file with the file picker
- Calculate MD5, SHA1, SHA256, and SHA512
- Copy the calculated hash to the clipboard
- Compare the calculated hash against an expected value

## Microsoft Store Identity

- Package/Identity/Name: `m3Coding.m3CodingSimpleHashChecker`
- Package/Identity/Publisher: `CN=AFF85DD5-3D92-42A5-BA39-3AF6D41B1837`
- Package/Properties/PublisherDisplayName: `m3 Coding`
- Package Family Name: `m3Coding.m3CodingSimpleHashChecker_8srffngrg4x08`
- Package SID: `S-1-15-2-2616263586-2441996366-2867164193-4037667357-3416997516-2070026731-824368873`
- Store ID: `9NH85X3KC8DQ`

## Build

```powershell
dotnet build SimpleHashChecker.sln -c Release
dotnet test SimpleHashChecker.sln -c Release
```
