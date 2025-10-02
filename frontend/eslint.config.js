import js from '@eslint/js';
import pluginVue from 'eslint-plugin-vue';
import prettier from 'eslint-config-prettier';
import ts from 'typescript-eslint';

export default ts.config(
  js.configs.recommended,
  ...pluginVue.configs['flat/essential'],
  ...ts.configs.recommended,
  prettier,
  {
    files: ['**/*.{ts,tsx,vue}'],
    languageOptions: {
      parserOptions: {
        parser: ts.parser,
        ecmaVersion: 'latest',
        sourceType: 'module'
      }
    },
    rules: {
      'vue/multi-word-component-names': 'off'
    }
  }
);
