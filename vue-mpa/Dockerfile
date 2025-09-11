FROM node:24-alpine as build

WORKDIR /app

COPY . .

RUN npm install
RUN npm run build

FROM nginx:latest as deploy

COPY --from=build /app/dist /usr/share/nginx/html