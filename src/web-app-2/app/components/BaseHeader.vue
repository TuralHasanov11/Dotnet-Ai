<template>
  <header class="border-b border-white/10 bg-slate-950/70 backdrop-blur">
    <div class="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
      <NuxtLink to="/" class="flex items-center gap-3 text-white no-underline">
        <img
          src="/logo.jpeg"
          alt="BAYTIB loqosu"
          class="h-10 w-10 rounded-2xl object-cover shadow-lg shadow-emerald-400/20"
        >
        <span class="leading-tight">
          <span class="block text-sm font-semibold uppercase tracking-[0.28em] text-emerald-300/90">Baytib</span>
          <span class="block text-xs text-slate-300">{{ $t('header.subtitle') }}</span>
        </span>
      </NuxtLink>

      <nav class="hidden items-center gap-2 sm:flex">
        <NuxtLink
          to="/"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.home') }}
        </NuxtLink>

        <div class="group relative">
          <button
            type="button"
            class="inline-flex items-center gap-1 rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
          >
            {{ $t('header.groupOrganization') }}
            <span class="text-xs">▾</span>
          </button>
          <div class="invisible absolute left-0 top-full z-20 mt-2 w-56 rounded-2xl border border-white/10 bg-slate-950/95 p-2 opacity-0 shadow-xl transition group-hover:visible group-hover:opacity-100 group-focus-within:visible group-focus-within:opacity-100">
            <NuxtLink to="/about" class="block rounded-xl px-3 py-2 text-sm text-slate-200 transition hover:bg-white/10 hover:text-white">{{ $t('header.about') }}</NuxtLink>
            <NuxtLink to="/history" class="block rounded-xl px-3 py-2 text-sm text-slate-200 transition hover:bg-white/10 hover:text-white">{{ $t('header.history') }}</NuxtLink>
            <NuxtLink to="/membership" class="block rounded-xl px-3 py-2 text-sm text-slate-200 transition hover:bg-white/10 hover:text-white">{{ $t('header.membership') }}</NuxtLink>
            <NuxtLink to="/governance" class="block rounded-xl px-3 py-2 text-sm text-slate-200 transition hover:bg-white/10 hover:text-white">{{ $t('header.governance') }}</NuxtLink>
          </div>
        </div>

        <div class="group relative">
          <button
            type="button"
            class="inline-flex items-center gap-1 rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
          >
            {{ $t('header.groupDocuments') }}
            <span class="text-xs">▾</span>
          </button>
          <div class="invisible absolute left-0 top-full z-20 mt-2 w-56 rounded-2xl border border-white/10 bg-slate-950/95 p-2 opacity-0 shadow-xl transition group-hover:visible group-hover:opacity-100 group-focus-within:visible group-focus-within:opacity-100">
            <NuxtLink to="/statute" class="block rounded-xl px-3 py-2 text-sm text-slate-200 transition hover:bg-white/10 hover:text-white">{{ $t('header.statute') }}</NuxtLink>
            <NuxtLink to="/reports" class="block rounded-xl px-3 py-2 text-sm text-slate-200 transition hover:bg-white/10 hover:text-white">{{ $t('header.reports') }}</NuxtLink>
          </div>
        </div>

        <NuxtLink
          to="/blog"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.blog') }}
        </NuxtLink>
        <NuxtLink
          to="/veterinarians"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.veterinarians') }}
        </NuxtLink>
        <!-- <NuxtLink
          to="/dashboard"
          class="rounded-full px-4 py-2 text-sm font-medium text-slate-200 transition hover:bg-white/10 hover:text-white"
        >
          {{ $t('header.dashboard') }}
        </NuxtLink> -->
      </nav>

      <AuthState>
        <template #default="{ loggedIn, user, clear }">
          <div class="flex items-center gap-3">
            <span class="hidden rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-medium text-slate-200 sm:inline-flex">
              {{ user?.name || user?.preferredUsername || user?.email || 'Doğrulanmış istifadəçi' }}
            </span>
            <NuxtLink
              v-if="!loggedIn"
              to="/login"
              class="rounded-full bg-emerald-400 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-emerald-300"
            >
              Daxil ol
            </NuxtLink>
            <button
              v-else
              type="button"
              class="rounded-full bg-white/10 px-4 py-2 text-sm font-semibold text-white transition hover:bg-white/15"
              @click="clear"
            >
              Çıxış
            </button>
          </div>
        </template>
        <template #placeholder>
          <div class="h-10 w-24 rounded-full bg-white/10" />
        </template>
      </AuthState>
    </div>
  </header>
</template>

<script setup lang="ts">
// Header component uses AuthState for auth UI
// Auto-imported from components
</script>