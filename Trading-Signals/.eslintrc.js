/* eslint-env node */
module.exports = {
    root: true,
    env: {
      node: true,
    },
    extends: [
      'plugin:vue/vue3-essential',
      'eslint:recommended'
    ],
    parserOptions: {
      ecmaVersion: 2020
    },
    rules: {
      // Cho phép dùng defineProps, defineEmits,... trong <script setup>
      'no-undef': 'off',
      'vue/setup-compiler-macros': 'off'
    }
  }
  