<template>
  <section class="mx-auto max-w-3xl rounded-4xl border border-slate-200/80 bg-white p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.14)] sm:p-10">
    <div class="space-y-4">
      <p class="text-sm font-semibold uppercase tracking-[0.28em] text-emerald-700">{{ $t('login.members') }}</p>
      <h1 class="text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">{{ $t('login.title') }}</h1>
      <p class="max-w-2xl text-base leading-7 text-slate-600">{{ $t('login.text') }}</p>
    </div>

    <div class="mt-8 flex flex-wrap gap-3">
      <a
        href="/auth/keycloak"
        class="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:bg-slate-800"
      >
        {{ $t('login.continue') }}
      </a>
      <NuxtLink
        to="/"
        class="rounded-full border border-slate-300 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:border-slate-400 hover:bg-slate-50"
      >
        {{ $t('login.back') }}
      </NuxtLink>
    </div>
  </section>
</template>

<script setup lang="ts">
useHead({
  title: 'Giriş'
})
</script>