#!/usr/bin/env bash
set -euxo pipefail

# cd to directory of this script
cd "$(dirname "$0")"
npm install --verbose
source ./commitlint/version.cfg
npm install --verbose @commitlint/config-conventional@$COMMITLINT_VERSION
npx commitlint --version
npx commitlint $@
