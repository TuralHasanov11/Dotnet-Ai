// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  runtimeConfig: {
    oauth: {
      keycloak: {
        clientId: '',
        clientSecret: '',
        serverUrl: '',
        realm: '',
        redirectURL: ''
      }
    },
    public: {
      apiBase: 'http://localhost:5003/api',
    },
  },

  modules: [
    '@nuxt/a11y',
    '@nuxt/eslint',
    '@nuxt/image',
    '@nuxt/test-utils',
    '@nuxt/ui',
    '@nuxt/scripts',
    '@nuxt/hints',
    'nuxt-auth-utils',
    '@nuxtjs/i18n'
  ],

  i18n: {
    locales: [
      { code: 'en', name: 'English', iso: 'en', file: 'en.json' }
    ],
    defaultLocale: 'en',
    lazy: true,
    langDir: 'locales/',
    detectBrowserLanguage: false
  },

  css: ['~/assets/css/main.css']
})