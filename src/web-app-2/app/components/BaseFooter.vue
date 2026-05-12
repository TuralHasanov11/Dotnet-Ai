<template>
  <footer class="border-t border-white/10 bg-slate-950/50 backdrop-blur">
    <div class="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
      <div class="grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
        <!-- Brand -->
        <div>
          <div class="flex items-center gap-3 mb-4">
            <img
              src="/logo.jpeg"
              alt="BAYTIB loqosu"
              class="h-10 w-10 rounded-2xl object-cover"
            >
            <div>
              <p class="font-semibold text-white">Baytib</p>
              <p class="text-xs text-slate-400">Üzvlük Portalı</p>
            </div>
          </div>
          <p class="text-xs text-slate-400">Təhlükəsiz identifikasiya və üzv idarəetmə platforması.</p>
        </div>

        <!-- Links -->
        <div>
          <h3 class="text-sm font-semibold text-white mb-4">Məhsul</h3>
          <ul class="space-y-2 text-xs">
            <li><NuxtLink to="/" class="text-slate-400 hover:text-emerald-300 transition">Ana səhifə</NuxtLink></li>
            <li><NuxtLink to="/blog" class="text-slate-400 hover:text-emerald-300 transition">Bloq</NuxtLink></li>
            <li><NuxtLink to="/dashboard" class="text-slate-400 hover:text-emerald-300 transition">İdarə paneli</NuxtLink></li>
          </ul>
        </div>

        <!-- Company -->
        <div>
          <h3 class="text-sm font-semibold text-white mb-4">Təşkilat</h3>
          <ul class="space-y-2 text-xs">
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Haqqımızda</a></li>
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Bloq</a></li>
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Əlaqə</a></li>
          </ul>
        </div>

        <!-- Legal -->
        <div>
          <h3 class="text-sm font-semibold text-white mb-4">Hüquqi</h3>
          <ul class="space-y-2 text-xs">
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Məxfilik</a></li>
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Şərtlər</a></li>
            <li><a href="#" class="text-slate-400 hover:text-emerald-300 transition">Kukilər</a></li>
          </ul>
        </div>
      </div>

      <!-- Bottom -->
      <div class="mt-8 border-t border-white/10 pt-8 flex items-center justify-between text-xs text-slate-400">
        <p>&copy; 2026 Baytib. Bütün hüquqlar qorunur.</p>
        <div class="flex gap-4">
          <a href="#" class="hover:text-emerald-300 transition">X (Twitter)</a>
          <a href="#" class="hover:text-emerald-300 transition">GitHub</a>
          <a href="#" class="hover:text-emerald-300 transition">LinkedIn</a>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
// Footer component
// Auto-imported from components
</script>