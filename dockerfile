# Fase 1: Build del frontend
FROM node:18-alpine AS frontend-builder
WORKDIR /app/frontend
COPY ./frontend/package*.json ./
RUN npm install
COPY ./frontend/ ./
RUN npm run build

# Fase 2: Build del backend
FROM node:18-alpine AS backend-builder
WORKDIR /app/backend
COPY ./backend/package*.json ./
RUN npm install
COPY ./backend/ ./

# Fase 3: Immagine finale di produzione
FROM node:18-alpine
WORKDIR /app
COPY --from=backend-builder /app/backend ./backend
COPY --from=frontend-builder /app/frontend/build ./backend/public
WORKDIR /app/backend
EXPOSE 3000
CMD ["node", "server.js"]