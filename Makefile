.PHONY: format

format:
	npm run format
	dnx fantomless-tool --recurse .
