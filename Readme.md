# TopoMojo

A virtual lab builder and player

## Build notes

The 'TopoMojo.Web' project is the api server and can be run with VSCode or Visual Studio.  The 'topomojo-app' project is the angular cli and can be run with 'npm start' or 'ng serve' from that folder.

These are dependent on IdentityServer, so fire that up locally as well.

Building 'TopoMojo.Web' *should* be automatic, but some notes if you run into trouble:

*   TopoMojo.Web needs a non-standard nuget feed.  See the Nuget.Config in the solution root.  (Nuget clients *should* respect that as it cascades configuration.)
*   TopoMojo.vSphere needs vmware-api-6.0.2 from https://nuget.cwd.local/v3/index.json
