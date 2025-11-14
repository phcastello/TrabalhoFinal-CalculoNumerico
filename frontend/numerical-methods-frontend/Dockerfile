FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
COPY tsconfig*.json ./
COPY vite.config.ts ./
COPY index.html ./
COPY .env ./
COPY src ./src
COPY public ./public
ARG VITE_API_BASE_URL=http://backend:5000
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm ci && npm run build

FROM node:20-alpine AS final
WORKDIR /app
RUN npm install -g serve@14.2.3
COPY --from=build /app/dist ./dist
EXPOSE 4173
CMD ["serve", "-s", "dist", "-l", "4173", "--single"]
