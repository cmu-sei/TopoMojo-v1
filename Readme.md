# TopoMojo

A virtual lab builder and player

#### Build notes

It *should* be automatic, but some notes if you run into trouble:

*   TopoMojo.Web needs a non-standard nuget feed.  See the Nuget.Config in that project.  (Nuget clients *should* respect that as the final cascading config.)
*   TopoMojo.vSphere needs vmware-api-6.0.2 from http://nuget.x.cwd.local/v3/index.json
*  TopoMojo.Web needs a webpack vendor bundle initially (and any time you change the vendor list).  A task is available to build it: `dotnet msbuild /t:WebpackDev`.  This also runs `npm install` to ensure everything is installed.
