.PHONY: format

format:
	npm run format
	dnx --yes fantomless-tool --recurse .
