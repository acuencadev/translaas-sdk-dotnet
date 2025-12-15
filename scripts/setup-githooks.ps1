$ErrorActionPreference = "Stop"

$repoRoot = (git rev-parse --show-toplevel).Trim()
Set-Location $repoRoot

if (-not (Test-Path ".githooks")) {
  throw "Expected '.githooks' directory to exist at repo root."
}

git config core.hooksPath .githooks | Out-Null

Write-Host "Git hooks enabled: core.hooksPath=.githooks"
Write-Host "Pre-commit will run 'dotnet format' on staged C# files and re-stage them."
