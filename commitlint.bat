@ECHO ON

# cd to directory of this script
cd %~dp0

npm install
npm install @commitlint/config-conventional @commitlint/cli
.\node_modules\@commitlint\cli\cli.js --version
.\node_modules\@commitlint\cli\cli.js %*

cd ..

