$docsroot = Resolve-Path $PSScriptRoot
Write-Information -Message "Building documentation in folder ${docsroot}" -InformationAction Continue
docker run --rm -v ${docsroot}:/docs firely/docs-sphinx
$index = "${docsroot}\_build\html\index.html"
Write-Information -Message "Build ready, showing output from ${index}" -InformationAction Continue
explorer.exe ${index}
Write-Information -Message "Done" -InformationAction Continue
