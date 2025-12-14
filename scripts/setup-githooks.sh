#!/usr/bin/env bash
set -euo pipefail

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

if [[ ! -d ".githooks" ]]; then
  echo "Expected '.githooks' directory to exist at repo root." >&2
  exit 1
fi

git config core.hooksPath .githooks

chmod +x .githooks/pre-commit 2>/dev/null || true

echo "Git hooks enabled: core.hooksPath=.githooks"
echo "Pre-commit will run 'dotnet format' on staged C# files and re-stage them."
