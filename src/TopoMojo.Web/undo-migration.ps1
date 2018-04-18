<#
.Synopsis Remove last migration for multiple database providers
#>
Param(
    [Parameter(Mandatory = $true)]
    $context
)

$providers = @('Sqlite', 'SqlServer', 'PostgreSQL')
foreach ($provider in $providers) {
    $env:Database:Provider=$provider
    dotnet ef migrations remove --context $context --project ..\TopoMojo.Data.$provider --no-build
}
$env:Database:Provider=''
