param($installPath, $toolsPath, $package, $project)

$sqliteReference = $project.Object.References.Find("SQLite.WP80, version=3.8.2")

if ($sqliteReference -eq $null) {
    Write-Host "Unable to find a reference to the extension SDK SQLite for Windows Phone."
    Write-Host "Verify that the reference to the extension SDK SQLite for Windows Phone has already been removed."
} else {
    $sqliteReference.Remove()
    Write-Host "Successfully removed the reference to the extension SDK SQLite for Windows Phone."
}

$localPath = $project.Properties.Item("LocalPath").Value
$wmapp = ""
$wmappFull = ""

if ($project.Type -eq "C#") {
    $wmapp = "Properties\WMAppManifest.xml"
    $wmappFull = Join-Path $localPath $wmapp
} elseif ($project.Type -eq "VB.NET") {
    $wmapp = "My Project\WMAppManifest.xml"
    $wmappFull = Join-Path $localPath $wmapp
}

if ($wmappFull -ne "" -and (Test-Path $wmappFull)) {
    [xml]$wmappXML = Get-Content $wmappFull	
	
	if (($wmappXML.Deployment -eq $null) -or ($wmappXML.Deployment.App -eq $null) -or ($wmappXML.Deployment.App.ActivatableClasses -eq $null)) {
		Write-Host "Unable to unregister SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll)."
		Write-Host "Please, manually unregister SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll) in the WMAppManifest.xml file."
	} else {
	    $removeInProcessServer = $null
		$unregister = $false
		$activatableClasses = $wmappXML.Deployment.App.ActivatableClasses
		
		foreach ($inProcessServer in $activatableClasses.InProcessServer) {
			if ($inProcessServer.Path -eq "SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll") {
				$removeInProcessServer = $inProcessServer
				$unregister = $true
				break
			}			
		}
		
		if ($unregister) {
			$activatableClasses.RemoveChild($removeInProcessServer)        	
        	$wmappXML.Save($wmappFull)
        	
        	Write-Host "Succesfully unregistered SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll)."
		} else {
	       Write-Host "Already unregistered SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll)."
		}
	}
} else {
	Write-Host "Unable to unregister SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll)."
	Write-Host "Please, manually unregister SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll) in the WMAppManifest.xml file."
}