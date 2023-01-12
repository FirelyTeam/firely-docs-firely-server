$docsroot = Resolve-Path $PSScriptRoot
Write-Information -Message "Building documentation in folder ${docsroot}" -InformationAction Continue
docker run --rm -v ${docsroot}:/docs firely.azurecr.io/firely/docs-sphinx:latest
if ($LASTEXITCODE -ne 0) {
    Write-Error -Message "Docker run failed. Is Docker active?" -RecommendedAction "Start Docker Desktop" -InformationAction Stop
    Exit
}
$index = "${docsroot}\_build\html\index.html"
Write-Information -Message "Build ready, showing output from ${index}" -InformationAction Continue
explorer.exe ${index}
Write-Information -Message "Done" -InformationAction Continue
