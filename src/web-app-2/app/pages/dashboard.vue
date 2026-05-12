<script setup lang="ts">
definePageMeta({
  middleware: 'auth'
})

const { user, session } = useUserSession()

useHead({
  title: 'İdarə paneli'
})
</script>

<template>
  <section class="space-y-6 rounded-4xl border border-slate-200/80 bg-white/95 p-8 text-slate-900 shadow-[0_20px_80px_rgba(15,23,42,0.14)] sm:p-10">
    <div class="space-y-3">
      <p class="text-sm font-semibold uppercase tracking-[0.28em] text-emerald-700">Üzv zonası</p>
      <h1 class="text-3xl font-semibold tracking-tight text-slate-950 sm:text-4xl">
        Xoş gəldiniz, {{ user?.name || user?.preferredUsername || user?.email || 'üzv' }}
      </h1>
      <p class="max-w-2xl text-base leading-7 text-slate-600">
        Bu səhifə route middleware ilə qorunur və yalnız etibarlı Keycloak sessiyası olan doğrulanmış istifadəçilər üçün göstərilir.
      </p>
    </div>

    <div class="grid gap-4 md:grid-cols-3">
      <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
        <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">İstifadəçi ID</p>
        <p class="mt-2 break-all text-sm font-medium text-slate-900">{{ user?.id }}</p>
      </div>
      <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
        <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">E-poçt</p>
        <p class="mt-2 break-all text-sm font-medium text-slate-900">{{ user?.email || 'Təqdim edilməyib' }}</p>
      </div>
      <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
        <p class="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Daxil olma vaxtı</p>
        <p class="mt-2 text-sm font-medium text-slate-900">
          {{ session?.loggedInAt ? new Date(session.loggedInAt).toLocaleString('az-AZ') : 'Naməlum' }}
        </p>
      </div>
    </div>
  </section>
</template>