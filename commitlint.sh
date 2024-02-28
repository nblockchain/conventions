#!/usr/bin/env bash
set -euxo pipefail

# cd to directory of this script
cd "$(dirname "$0")"
npm install --verbose
npm install --verbose @commitlint/config-conventional
npx commitlint --version
npx commitlint $@
