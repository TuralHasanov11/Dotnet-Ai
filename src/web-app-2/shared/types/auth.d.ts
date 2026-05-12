declare module '#auth-utils' {
  interface User {
    id: string
    name?: string | null
    email?: string | null
    username?: string | null
    preferredUsername?: string | null,
    roles?: string[] | null,
    groups?: string[] | null,
  }

  interface UserSession {
    provider?: 'keycloak'
    loggedInAt?: number
  }
}

export {}