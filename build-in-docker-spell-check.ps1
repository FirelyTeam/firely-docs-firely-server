$docsroot = Resolve-Path $PSScriptRoot
Write-Information -Message "Building documentation in folder ${docsroot}" -InformationAction Continue
$buildfolder = "$docsroot\_build\spelling"
#RD $buildfolder -Recurse
#docker run --rm -v ${docsroot}:/docs firely.azurecr.io/firely/docs-sphinx:latest
docker run --rm -v ${docsroot}:/docs -v ${docsroot}/tmp:/tmp firely.azurecr.io/firely/docs-sphinx:latest sphinx-build -b spelling . ./_build/spelling
if ($LASTEXITCODE -ne 0) {
    Write-Error -Message "Docker run failed. Is Docker active?" -RecommendedAction "Start Docker Desktop" -InformationAction Stop
    Exit
}
$index = "${docsroot}\_build\html\index.html"
Write-Information -Message "Build ready, showing output from ${index}" -InformationAction Continue
#explorer.exe ${index}
Write-Information -Message "Done" -InformationAction Continue


# Step 1: Run Sphinx to generate the documentation with the spelling plugin enabled.
# Replace 'path/to/your/docs' with the actual path to your Sphinx documentation project.
$docsPath = $docsroot
$outputPath = "$docsroot/_build/SpellingReport"
RD $outputPath -Recurse
New-Item -Path $outputPath -ItemType Directory
#sphinx-build -b html $docsPath $docsPath/_build

# Step 2: Locate all the spelling output files generated by 'sphinxcontrib.spelling'.
$outputFiles = Get-ChildItem -Path $buildfolder -Recurse -Filter "*.spelling"

# Step 3: Create a hashtable to store the spelling issue counts and details.
$spellingCounts = @{}
$spellingDetails = @{}

# Step 4: Aggregate the content from all spelling output files, count the issues, and generate details.
foreach ($file in $outputFiles) {
    $content = Get-Content $file.FullName
    $spellingIssue = ""

    foreach ($line in $content) {
        if ($line -match '\(([^)]+)\)') {
            $spellingIssue = $matches[1]            

            #if($spellingIssue -eq "Vonk")
            #{
                $spellingCounts[$spellingIssue]++
                $spellingIssue
                $spellingDetails[$spellingIssue] += "<tr><td><pre>$line</pre></td></tr>" 
             #   break;
            #}  
            
            #$spellingDetails[$spellingIssue] = "<tr><td><pre>$line</pre></td></tr>"           
        }
        
    }
}
$spellingDetails


# Step 5: Sort the spelling issues by count in descending order.
$sortedSpellingCounts = $spellingCounts.GetEnumerator() | Sort-Object Value -Descending
$sortedSpellingCounts
#return

# Step 6: Create an HTML file to display the summary.
$htmlFile = "$outputPath/spelling_summary.html"

# Step 7: Generate the HTML summary with spelling issue counts and links to details.
$htmlContent = @"
<!DOCTYPE html>
<html>
<head>
  <title>Sphinx Spelling Summary</title>
</head>
<body>
  <h1>Sphinx Spelling Summary</h1>
"@

foreach ($entry in $sortedSpellingCounts) {
    $spellingIssue = $entry.Key
    $issueCount = $entry.Value

    # Generate the link to the spelling issue details file.
    $detailFilename = "$outputPath/spelling_detail_$spellingIssue.html"
    
    # Create the spelling issue details file with the content from the .spelling files.
    $detailsContent = $spellingDetails[$spellingIssue] #-join "`r`n"

    $detailsContent

    $htmlDetailsContent = @"
<!DOCTYPE html>
<html>
<head>
  <title>Sphinx Spelling Details - $spellingIssue</title>
  <style>
    pre {
      background-color: #f0f0f0;
      padding: 5px;
    }
  </style>
</head>
<body>
  <h1>Sphinx Spelling Details - $spellingIssue</h1>
  <table border="1">
    $($spellingDetails[$spellingIssue] -join "`r`n")
  </table>
</body>
</html>
"@
    $htmlDetailsContent | Out-File -FilePath $detailFilename

    $htmlContent += @"
    <h2>$spellingIssue - Count: $issueCount</h2>
    <a href="$detailFilename">Details</a>
"@
}

$htmlContent += @"
</body>
</html>
"@

$htmlContent | Out-File -FilePath $htmlFile

# Step 8: Open the HTML file in your preferred web browser.
Start-Process $htmlFile