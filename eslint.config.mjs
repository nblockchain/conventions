// @ts-check

import eslint from '@eslint/js';
import tseslint from 'typescript-eslint';

// Have to explicitly exclude .js files, see https://stackoverflow.com/a/78867272/20881435
export default tseslint.config(
  {
    ignores: [
      '**/*.js',
    ],
  },
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommended,
    ],
  }
);
