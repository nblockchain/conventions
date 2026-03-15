#!/usr/bin/env bash
#
# Pre-commit hook to run code formatting
#
set -euo pipefail

echo "Running pre-commit format check..."

# Run prettier format
npm run format

echo "✅ Formatting completed successfully."
