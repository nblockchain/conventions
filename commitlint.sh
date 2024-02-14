#!/usr/bin/env bash
set -euxo pipefail

# cd to directory of this script
cd "$(dirname "$0")"
npm install
npm install @commitlint/{config-conventional@v18.6.1,cli@v18.6.1}
npx commitlint --version
npx commitlint $@
cd ..
