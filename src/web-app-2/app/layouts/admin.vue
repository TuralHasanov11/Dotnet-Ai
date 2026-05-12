<template>
  <div class="flex flex-col min-h-screen bg-slate-50">
    <AdminHeader />
    <main class="flex-1 mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8 w-full">
      <slot />
    </main>
    <AdminFooter />
  </div>
</template>

<script setup lang="ts">
// Admin layout - used for all /admin/* routes
// Auto-imports AdminHeader and AdminFooter components
</script>

<style scoped>
/* Admin layout specific styles */
main {
  background: linear-gradient(135deg, rgba(248, 250, 252, 0.8) 0%, rgba(241, 245, 249, 0.8) 100%);
}
</style>
