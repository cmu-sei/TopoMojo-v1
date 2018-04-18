<#
.Synopsis Add a migration for multiple database providers
.Notes Assumes project is freshly built
#>
Param(
    [Parameter(Mandatory = $true)]
    $context,
    [Parameter(Mandatory = $true)]
    $name
)

$providers = @('Sqlite', 'SqlServer', 'PostgreSQL')
foreach ($provider in $providers) {
    $env:Database:Provider=$provider
    $folder = ($context -replace "Context","")
    dotnet ef migrations add $name --context $context -o Migrations\$folder --project ..\TopoMojo.Data.$provider --no-build
}

$env:Database:Provider=''