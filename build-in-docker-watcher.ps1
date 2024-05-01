$docsroot = Resolve-Path $PSScriptRoot

$path = $docsroot

# Define subfolders to exclude
$excludedSubfolders = @(".vs")

# Define included file extensions
$includedExtensions = @(".rst")  # Example: Include only .pdf and .docx files

# Create a FileSystemWatcher object
$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = $path

# Set properties to watch for changes
$watcher.IncludeSubdirectories = $true
$watcher.EnableRaisingEvents = $true

# Define the actions to take when events occur
$action = {
    $eventArgs = $Event.SourceEventArgs
    $changedPath = $eventArgs.FullPath

    # Check if the changed path is within excluded subfolders
    foreach ($subfolder in $excludedSubfolders) {
        if ($changedPath.StartsWith("$path\$subfolder\", [System.StringComparison]::OrdinalIgnoreCase)) {
            return  # Skip processing if the changed path is within excluded subfolders
        }
    }
    # Check if the changed file has an included extension
    $extension = [System.IO.Path]::GetExtension($changedPath)
    if (-not ($includedExtensions -contains $extension.ToLower())) {
        return  # Skip processing if the changed file has an excluded extension
    }

    Write-Host "Change detected: $changedPath - $($eventArgs.ChangeType)"

    Write-Information -Message "Building documentation in folder ${docsroot}" -InformationAction Continue
    docker run --rm -v ${docsroot}:/docs firely.azurecr.io/firely/docs-sphinx:latest
    Write-Host "Doc complete"
}

# Register event handlers
Register-ObjectEvent -InputObject $watcher -EventName "Changed" -Action $action
Register-ObjectEvent -InputObject $watcher -EventName "Created" -Action $action
Register-ObjectEvent -InputObject $watcher -EventName "Deleted" -Action $action
Register-ObjectEvent -InputObject $watcher -EventName "Renamed" -Action $action

Write-Host "File watcher started for $path. Press Ctrl+C to exit."

# Keep the script running until manually stopped
try {
    while ($true) {
        # Wait for events
        Wait-Event -Timeout 1
    }
} finally {
    # Clean up resources when exiting
    $watcher.Dispose()
}
