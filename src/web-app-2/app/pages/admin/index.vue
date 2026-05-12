<template>
  <div>
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-slate-900">Administrator paneli</h1>
      <p class="text-slate-600 mt-2">Baytib idarəetmə panelinə xoş gəlmisiniz</p>
    </div>

    <!-- Stats Grid -->
    <div class="grid gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-8">
      <div class="bg-white rounded-lg shadow-sm p-6 border border-slate-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-slate-600">Ümumi paylaşımlar</p>
            <p class="text-2xl font-bold text-slate-900 mt-2">24</p>
          </div>
          <div class="text-4xl text-slate-200">📝</div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-6 border border-slate-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-slate-600">Ümumi istifadəçilər</p>
            <p class="text-2xl font-bold text-slate-900 mt-2">156</p>
          </div>
          <div class="text-4xl text-slate-200">👥</div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-6 border border-slate-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-slate-600">Yayımlanmış paylaşımlar</p>
            <p class="text-2xl font-bold text-slate-900 mt-2">18</p>
          </div>
          <div class="text-4xl text-slate-200">✅</div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-6 border border-slate-200">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-slate-600">Qaralama paylaşımlar</p>
            <p class="text-2xl font-bold text-slate-900 mt-2">6</p>
          </div>
          <div class="text-4xl text-slate-200">📋</div>
        </div>
      </div>
    </div>

    
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: 'auth' // Protect with auth middleware
})

useHead({
  title: 'Administrator paneli'
})
</script>
