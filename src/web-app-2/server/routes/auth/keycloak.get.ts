export default defineOAuthKeycloakEventHandler({
  config: {
    scope: ['openid', 'profile', 'email']
  },
  async onSuccess(event, { user }) {
    await setUserSession(event, {
      user: {
        id: user.sub,
        name: user.name,
        email: user.email,
        username: user.preferred_username,
        preferredUsername: user.preferred_username
      },
      provider: 'keycloak',
      loggedInAt: Date.now()
    })

    return sendRedirect(event, '/dashboard')
  }
})