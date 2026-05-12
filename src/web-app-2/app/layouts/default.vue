<template>
  <div class="flex flex-col flex-1">
    <BaseHeader />
    <main class="flex-1 mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8 lg:py-12 w-full">
      <slot />
    </main>
    <BaseFooter />
  </div>
</template>
