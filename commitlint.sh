#!/usr/bin/env bash
set -euxo pipefail

# cd to directory of this script
cd "$(dirname "$0")"
npm install
npx commitlint --version
npx commitlint $@
cd ..
