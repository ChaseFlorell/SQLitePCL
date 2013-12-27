param($installPath, $toolsPath, $package, $project)

$sqliteReference = $project.Object.References.AddSDK("SQLite for Windows Phone", "SQLite.WP80, version=3.8.2")

Write-Host "Successfully added a reference to the extension SDK SQLite for Windows Phone."
Write-Host "Please, verify that the extension SDK SQLite for Windows Phone v3.8.2, from the SQLite.org site (http://sqlite.org/2013/sqlite-wp80-winrt-3080200.vsix), has been properly installed."

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
		Write-Host "Unable to register SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll)."
		Write-Host "Please, manually register SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll) in the WMAppManifest.xml file."
	} else {
		$register = $true
		$activatableClasses = $wmappXML.Deployment.App.ActivatableClasses
		
		foreach ($inProcessServer in $activatableClasses.InProcessServer) {
			if ($inProcessServer.Path -eq "SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll") {
				$register = $false
				break
			}			
		}
		
		if ($register) {
			$newInProcessServer = $wmappXML.CreateElement("InProcessServer")
			
			$newPath = $wmappXML.CreateElement("Path")
			$newPath.set_InnerXml("SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll")
			
			$newActivatableClass = $wmappXML.CreateElement("ActivatableClass")
			$newActivatableClassId = $wmappXML.CreateAttribute("ActivatableClassId")
			$newActivatableClassId.set_InnerXml("SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider")
			$newThreadingModel = $wmappXML.CreateAttribute("ThreadingModel")
			$newThreadingModel.set_InnerXml("both")
			
			$newActivatableClass.Attributes.Append($newActivatableClassId)
			$newActivatableClass.Attributes.Append($newThreadingModel)
			
			$newInProcessServer.AppendChild($newPath)
			$newInProcessServer.AppendChild($newActivatableClass)
			
			$activatableClasses.AppendChild($newInProcessServer)
        	
        	$wmappXML.Save($wmappFull)
        	
        	Write-Host "Succesfully registered SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll)."
		} else {
	       Write-Host "Already registered SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll)."
		}
	}
} else {
	Write-Host "Unable to register SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll)."
	Write-Host "Please, manually register SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.SQLite3RuntimeProvider (SQLitePCL.Ext.WindowsPhone8.RuntimeProxy.dll) in the WMAppManifest.xml file."
}