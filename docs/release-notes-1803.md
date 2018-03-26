# TopoMojo 1803 Release Notes

> 2018-03-23 17:00

## Summary

* Improved ISO handling
* Added gamespace print view
* Admin workspace chat
* Improved workspace ui feedback
* Added workspace limits
* Implemented vCenter client
* Added TicketUrlHandler
* Implemented AOT compilation
* Implemented Import/Export

## Detail

#### Improved ISO handling
Users can upload files to a workspace.  If the file does not have an .iso extension, an iso is generated.  If a vm exists for the workspace template, the iso will be attached to the vm.

(This could cause problems, especially on Linux, if the cd device already has an iso mounted. A vm question should be generated, but best practice is to eject any cd's before uploading a new iso.)

If a template is published with a workspace iso, the iso will be attached in any derived gamespaces.

Added a search box to filter existing isos when selecting an iso in a workspace.

Trimmed iso filenames and added a fullname tooltip in the iso selector.

#### Added gamespace print view
Browser print should hide the page header, providing a clean lab document.

#### Admin workspace chat
From the admin dashboard, admins can enter any workspace.  They are joined to the workspace chat, so can interact with the team.

Todo: add a "call-light" to attract admin notice to a workspace.

#### Improved workspace ui feedback
Added visual indicators to show button-initiated task completion.

#### Added workspace limits
Workspace Limits provide a mechanism to prevent users from creating and arbitrary number of workspaces. The default limit is configurable in appsettings, and admin's can set `ws-max` for individuals on the Admin dashboard.

Admins can create unlimited workspaces.  To give non-admin's the same, set their `ws-max` to a reasonably high integer (i.e. 99999).  You can "disable" old users by setting them to 0. You could set new users to 1, or consider having the TA generate the workspaces and invite the correct members to each.

Future work: give users an in-band ability to request a workspace.

#### Implemented vCenter client
See the [appsettings](https://code.sei.cmu.edu/bitbucket/projects/CWD/repos/topomojo/browse/src/TopoMojo.Web/appsettings.json#155) for configuration notes.

#### Added TicketUrlHandler
Added several methods to convert internal console urls to external urls to support various reverse-proxy configurations.

Additional [Notes](https://code.sei.cmu.edu/bitbucket/projects/CWD/repos/topomojo/browse/src/TopoMojo.Web/appsettings.json#183)

#### Implemented AOT compilation
Updated webpack configuration to provide faster application load times.  (Actually still leaning forward a bit here; source code references webpack 4, which breaks Hot Module Swapping until AspNetCore.SpaServices supports it.)

#### Implemented Import/Export
Added Ui for initiating a workspace export. On the Admin Dashboard, click the export button (folder icon) to select it for export. After making all your selections, click the primary export button on the right panel. This will provide you with an `export-folder` to access on the backend.

The idea is to:
1. Move your export-folder to your staging media
1. Copy the required disks into the `/staging/<workspace>` folders
    * This can be accomplished by scripting a copy/clone using the provided `/staging/<workspace>/topo.disks` file
1. Transfer your `/staging` to your destination media
    * I imagine you can combine the previous step with this if you are pushing to an online endpoint (ftp, nfs, http, etc).
1. Merge the contents of your staging-folder with the destination topomojo-folder
1. On the destination TopoMojo server, run the Import (Admin Dashboard / Settings Panel)
    * This will import the data and document, but the disks should already be in place!
