# TopoMojo

A virtual lab builder and player

#### Build notes

It *should* be automatic, but some notes if you run into trouble:

*   TopoMojo.Web needs a non-standard nuget feed.  See the Nuget.Config in that project.  (Nuget clients *should* respect that as the final cascading config.)
*   TopoMojo.vSphere needs vmware-api-6.0.2 from http://nuget.x.cwd.local/v3/index.json
*  TopoMojo.Web needs a webpack vendor bundle initially (and any time you change the vendor list).  A task is available to build it: `dotnet msbuild /t:WebpackDev`.  This also runs `npm install` to ensure everything is installed.


If running in Visual Studio and Kestrel is desired, all the following to the launchSettings.json file in the TopoMojo.Web project:
```
    "Kestrel": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "http://localhost:5004",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
```

(Hot-Module replacement is currently broken until Microsoft.AspNetCore.SpaServices catches up with webpack 4.  I didn't revert back to webpack 3 because I don't have much ui dev planned and want to be "leaning forward".)