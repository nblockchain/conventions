#!/usr/bin/env bash
set -euxo pipefail

# cd to directory of this script
cd "$(dirname "$0")"
npm install --verbose
npm install --verbose @commitlint/config-conventional@18.6.1
npx commitlint --version
npx commitlint $@
