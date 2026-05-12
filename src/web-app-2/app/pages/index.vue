<template>
  <section class="grid gap-8 lg:grid-cols-[1.3fr_0.7fr] lg:items-start">
    <div class="space-y-6 rounded-4xl border border-slate-200/80 bg-white/90 p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.16)] backdrop-blur sm:p-10">
      <div class="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-4 py-2 text-sm font-medium text-emerald-800">
        <span class="h-2 w-2 rounded-full bg-emerald-500" />
        Baytib veb tətbiqində Keycloak giriş inteqrasiyası hazırdır
      </div>

      <div class="space-y-4">
        <h1 class="max-w-2xl text-4xl font-semibold tracking-tight text-slate-950 sm:text-5xl">
           {{ $t('index.heroTitle') }}
        </h1>
        <p class="max-w-2xl text-lg leading-8 text-slate-600">
           {{ $t('index.heroText') }}
        </p>
      </div>

      <div class="flex flex-wrap gap-3">
        <NuxtLink
          to="/login"
          class="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:bg-slate-800"
        >
            {{ $t('index.ctaSignIn') }}
        </NuxtLink>
        <NuxtLink
          to="/veterinarians"
          class="rounded-full border border-slate-300 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:border-slate-400 hover:bg-slate-50"
        >
            {{ $t('index.ctaBrowse') }}
        </NuxtLink>
      </div>

      <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        
      </div>
    </div>

  </section>
</template>

<script setup lang="ts">
useHead({
  title: 'Ana səhifə'
})
</script>