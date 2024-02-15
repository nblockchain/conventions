#!/usr/bin/env bash
set -euxo pipefail

# cd to directory of this script
cd "$(dirname "$0")"
npm install
npm install conventional-changelog-conventionalcommits commitlint@latest
npx commitlint --version
npx commitlint $@
cd ..
