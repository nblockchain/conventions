#!/usr/bin/env bash
set -euxo pipefail

# cd to directory of this script
cd "$(dirname "$0")"
npm install
npm install conventional-changelog-conventionalcommits commitlint@latest
./node_modules/commitlint/cli.js --version
./node_modules/commitlint/cli.js $@
cd ..
