<template>
  <UApp>
    <div class="flex flex-col min-h-screen bg-[radial-gradient(circle_at_top,_rgba(34,197,94,0.16),_transparent_34%),linear-gradient(180deg,_#07111f_0%,_#0b1627_50%,_#f6f8fb_50%,_#eef2f7_100%)] text-slate-100">
      <NuxtRouteAnnouncer />
      <NuxtLayout>
        <NuxtPage />
      </NuxtLayout>
    </div>
  </UApp>
</template>

<script setup lang="ts">
useHead({
  titleTemplate: title => (title ? `${title} · Baytib` : 'Baytib')
})
</script>
