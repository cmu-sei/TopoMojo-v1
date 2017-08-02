# TopoMojo

A virtual lab builder and player

#### Build notes

It *should* be automatic, but some notes if you run into trouble:

*   TopoMojo.Web needs a non-standard nuget feed.  See the Nuget.Config in that project.  (Nuget clients *should* respect that as the final cascading config.)
*   TopoMojo.vSphere needs vmware-api-6.0.2 from http://nuget.x.cwd.local/v3/index.json
*  TopoMojo.Web needs a webpack vendor bundle initially (and any time you change the vendor list).  A task is available to build it: `dotnet msbuild /t:WebpackDev`.  This also runs `npm install` to ensure everything is installed.

#### Build Steps for Visual Studio

1.  Add `http://nuget.x.cwd.local/v3/index.json` to the nuget package sources list
2.  Run `Restore NuGet packages` in VS Solution
3.  Exit VPN at this time so that npm can talk to the web properly
4.  In Package Manager Console, select TopoMojo.Web project, run `dotnet msbuild /t:WebpackDev` to run webpack
5.  Disable `http://nuget.x.cwd.local/v3/index.json` nuget feed
6.  Run `Install-Package Microsoft.AspNetCore.SignalR.Server -Version 0.2.0-rtm-22752 -Source https://dotnet.myget.org/F/aspnetcore-master/api/v3/index.json` in Package Manager Console
7.  Re-enable feed from step 5
8.  Run a build, npm will first be pulling down packages before the build begins
9.  Connect back to VPN
9.  If Kestrel is desired, all the following to the launchSettings.json file in the TopoMojo.Web project:
    "Kestrel": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "http://localhost:5004",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }