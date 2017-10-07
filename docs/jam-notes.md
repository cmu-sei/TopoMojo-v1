# TODO
audit logging
investigate console keepalive
workspace messaging
admin templates, order by topo,template
admin users, manage ui
admin vm page, order by topo, instance
profile page to change name
topo deployment stats
multi-page docs

# DONE
display enlistment errors
improve doc layout
review blank.vmdk workflow
cleanup structure
set/upload iso
better 'loading' indicator
gamespace messaging
handle external auth error if not present (maybe try handler local jwt first)
jwt auth for console requests
jwt access_token duration events
clean up options/settings
hide admin nav if not admin
publish topo
invite user to gamespace
invite user to workspace
don't allow removing owners
hide save button for linked templates
profile dashboard (my topos)
db seed admin
lab document
external authentication
update project dependencies
browser/launch


System.Net.Http.HttpMessageHandler
HttpClientHandler handler = new HttpClientHandler();
handler.Proxy = new WebProxy("http://proxy.sei.cmu.edu:8080");
IdentityServerBearerTokenAuthenticationOptions.IntrospectionHttpHandler