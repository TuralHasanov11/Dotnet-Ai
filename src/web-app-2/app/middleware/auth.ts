export default defineNuxtRouteMiddleware((to) => {
  const { loggedIn } = useUserSession()

  if (loggedIn.value) {
    return
  }

  return navigateTo({
    path: '/login',
    query: {
      redirect: to.fullPath
    }
  })
})