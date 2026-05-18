#!/usr/bin/env bash
set -euo pipefail

which npx || (echo '\nPlease install `npx`, maybe installing NPM?' >&2 && exit 1)

npx --no-install prettier --version || (echo '\nPlease install `prettier` via `npm install` first; if this problem persists, try `npm rebuild` or `git clean -fdx` before `npm install`' >&2 && exit 1)

npx --no-install prettier $@ --quote-props=consistent './**/*.{yml,ts}'

