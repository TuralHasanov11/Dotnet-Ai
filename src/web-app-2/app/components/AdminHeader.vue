<template>
  <header class="border-b border-slate-200 bg-white shadow-sm">
    <div class="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
      <div class="flex items-center gap-3">
        <span class="grid h-10 w-10 place-items-center rounded-lg bg-slate-900 text-sm font-black text-white">A</span>
        <div>
          <p class="font-semibold text-slate-900">Admin</p>
          <p class="text-xs text-slate-500">Baytib</p>
        </div>
      </div>

      <nav class="hidden md:flex items-center gap-1">
        <NuxtLink
          to="/admin"
          class="px-3 py-2 rounded-md text-sm font-medium transition"
          :class="isActive('/admin') ? 'bg-slate-100 text-slate-900' : 'text-slate-600 hover:bg-slate-50'"
        >
          İdarə paneli
        </NuxtLink>
        <NuxtLink
          to="/admin/blog"
          class="px-3 py-2 rounded-md text-sm font-medium transition"
          :class="isActive('/admin/blog') ? 'bg-slate-100 text-slate-900' : 'text-slate-600 hover:bg-slate-50'"
        >
          Bloq
        </NuxtLink>
        <NuxtLink
          to="/admin/users"
          class="px-3 py-2 rounded-md text-sm font-medium transition"
          :class="isActive('/admin/users') ? 'bg-slate-100 text-slate-900' : 'text-slate-600 hover:bg-slate-50'"
        >
          İstifadəçilər
        </NuxtLink>
        <NuxtLink
          to="/admin/settings"
          class="px-3 py-2 rounded-md text-sm font-medium transition"
          :class="isActive('/admin/settings') ? 'bg-slate-100 text-slate-900' : 'text-slate-600 hover:bg-slate-50'"
        >
          Tənzimləmələr
        </NuxtLink>
      </nav>

      <AuthState>
        <template #default="{ loggedIn, user, clear }">
          <div class="flex items-center gap-4">
            <div class="hidden sm:block text-right">
              <p class="text-sm font-medium text-slate-900">{{ user?.name || 'Administrator' }}</p>
              <p class="text-xs text-slate-500">{{ user?.email }}</p>
            </div>
            <div class="flex items-center gap-2">
              <NuxtLink
                to="/"
                class="px-3 py-2 text-sm font-medium rounded-md text-slate-600 hover:bg-slate-50 transition"
              >
                ← Sayta keç
              </NuxtLink>
              <button
                v-if="loggedIn"
                @click="clear"
                class="px-4 py-2 text-sm font-medium rounded-md bg-slate-100 text-slate-900 hover:bg-slate-200 transition"
              >
                Çıxış
              </button>
            </div>
          </div>
        </template>
      </AuthState>
    </div>
  </header>
</template>

<script setup lang="ts">
const route = useRoute()
const isActive = (path: string) => route.path === path || route.path.startsWith(path + '/')
</script>
