FROM node:22-alpine AS build
WORKDIR /app

# Ensure corepack (optional) and copy lockfiles if present
RUN corepack enable || true
COPY package.json package-lock.json* pnpm-lock.yaml* .npmrc* ./

# Install dependencies (use lockfile if present)
RUN if [ -f package-lock.json ]; then \
      npm ci --omit=dev --no-audit --no-fund; \
    else \
      npm install --no-audit --no-fund; \
    fi

# Copy source and build
COPY . .
RUN npm run build

FROM node:22-alpine AS production
WORKDIR /app

# Copy only the built output
COPY --from=build /app/.output/ ./

ENV PORT=3000
ENV HOST=0.0.0.0

EXPOSE 3000

CMD ["node", "/app/server/index.mjs"]
