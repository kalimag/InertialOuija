param
(
	[Parameter(Mandatory)][string] $build,
    [Parameter()][string] $game_dir = $env:GAME_DIR
)

$ErrorActionPreference = "Inquire"

$gameproc = Get-Process "InertialDrift" -ErrorAction SilentlyContinue
if ($gameproc)
{
	$gameproc | Stop-Process -Force
	$gameproc | Wait-Process
    Start-Sleep -Seconds 1
}

#echo "build=$build"
#echo "game_dir=$game_dir"

Copy-Item -Path "$build\*" -Destination "$game_dir" -Recurse -Force

Start-Process "$game_dir\InertialDrift.exe" -WorkingDirectory "$game_dir"

#Start-Sleep -Seconds 10
