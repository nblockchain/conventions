#!/usr/bin/env bash
#
# Pre-commit hook to run code formatting check
#
set -euo pipefail

echo "Running pre-commit format check..."

if ! npm run format:check > /dev/null 2>&1; then
    echo ""
    echo "❌ Prettier check failed. Please run 'npm run format' to fix formatting issues, then stage the changes and try committing again."
    npm run format:check
    exit 1
fi

echo "✅ Format check passed."
