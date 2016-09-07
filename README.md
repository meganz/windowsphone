MEGA Windows Phone Client
=========================
A fully-featured client to access your Cloud Storage provided by MEGA.<br>
This repository contains all the development history of the official Windows Phone client of MEGA: https://mega.nz/#mobile

#### Target OS
- Windows Phone 8.0
- Windows Phone 8.1 (compatible with Windows 10 Mobile)

#### Used 3rd party controls
- Telerik UI for Windows Phone.

## Compilation
This document will guide you to build the application on a Windows machine with Microsoft Visual Studio.

#### Requirements
- Microsoft Windows machine (at least Microsoft Windows 8).
- Microsoft Visual Studio (at least Microsoft Visual Studio Express 2013 for Windows).

#### Preparation
1. Get the source code. Clone or donwload this repository.

2. Download the required third party libraries from this link: 
https://mega.nz/#!FwdUXBwJ!pzoYCkJqrJ6ilM821H9e6RFjbD7JtQ2w822sczIlUwA

3. Uncompress that file into `windowsphone\MegaSDK\bindings\wp8`

4. Install the _"Multilingual App Toolkit"_ for Visual Studio. You can download it from this link:
https://developer.microsoft.com/en-us/windows/develop/multilingual-app-toolkit

5. Open Microsoft Visual Studio and open the solution file `windowsphone\MegaApp\MegaApp.sln`

6. Install the `MockIAPLib`, `sqlite-net` and `sqlite-net-wp8` NuGet packages from `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution`, and add the needed references for each of the two projects.

7. Install the _"SQLite for Windows Phone"_ and _"SQLite for Windows Phone 8.1"_ from `Tools -> Extensions and Updates`, and add the needed references for each of the two projects.

8. Install the _"Telerik UI for Windows Phone"_ controls library and include all the needed references for each of the two projects. You can download it from this link:
http://www.telerik.com/products/windows-phone.aspx

9. Make sure the `MegaApp80` or `MegaApp81` project is selected as init or main project.

10. Build the project.

11. Enjoy!

If you want to build the third party dependencies by yourself, you can download the source code from this link:
https://mega.nz/#!QkcnxJJD!d28YoX2pYVBgo0Tg6OoAu0Cikm73uOtySLeZOQD0j14
