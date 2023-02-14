#!/usr/bin/env bash
set -euxo pipefail

# cd to directory of this script
cd "$(dirname "$0")"
npm install conventional-changelog-conventionalcommits
npm install commitlint@latest
npx commitlint --version
npx commitlint $@
cd ..
