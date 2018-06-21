# Updates all projects with given version number
param([String]$targetVersion, [String]$releaseNotes)

$excludedCsprojs = ("TomLabs.Profiling.Tests.csproj");

# FUNCTIONS

function IncreaseVersion(){
	param(
		[Parameter(
			Position=0, 
			Mandatory=$true, 
			ValueFromPipeline=$true,
			ValueFromPipelineByPropertyName=$true)
		]
		[Alias('VersionString')]
		[String]$version
	)

	$pattern = '(\d+)(?:\.(\d+))?(?:\.(\d+))?(?:\.(\d+))?'
	$build = [convert]::ToInt32(($version | Select-String -Pattern $pattern | % {$_.matches.groups[3].Value}), 10) + 1

	return $version -replace $pattern, ('$1.$2.' + $build)
}

function Update-CsProj($csprojPath){
	Write-Host "Updating $csprojPath" -ForegroundColor Green

	$xml = New-Object XML
	$xml.Load($csprojPath)

	if(!$global:targetVersion){
		$global:targetVersion = $xml.SelectSingleNode("//AssemblyVersion").InnerText | IncreaseVersion
	}

	$xml.SelectSingleNode("//AssemblyVersion").InnerText = $global:targetVersion
	$xml.SelectSingleNode("//FileVersion").InnerText = $global:targetVersion
	$xml.SelectSingleNode("//Version").InnerText = $global:targetVersion

	$xml.SelectSingleNode("//PackageReleaseNotes").InnerText = $global:releaseNotes
	
	$xml.Save($csprojPath)
}

#END FUNCTIONS

$csProjs = Get-ChildItem $PWD -Recurse -Include *.csproj -Exclude $excludedCsprojs

foreach($csproj in $csProjs){
	Update-CsProj($csproj)
}

$appveyorYml = "$PWD\..\appveyor.yml"
(Get-Content $appveyorYml) -replace 'version: (.*)\.\{build\}', ('version: '+$targetVersion+'.{build}') | Out-File $appveyorYml -Encoding utf8

$desc = if(!$releasenotes) { '#description:  #' } else { 'description: '+$releaseNotes+' #' };
(Get-Content $appveyorYml) -replace '(#)?description: (.*) #', ($desc) | Out-File $appveyorYml -Encoding utf8

(Get-Content $appveyorYml) -replace 'release: (.*) #', ('release: v'+$targetVersion+' #') | Out-File $appveyorYml -Encoding utf8
