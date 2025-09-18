FROM node:24-alpine as build

WORKDIR /app

COPY . .

RUN npm install
RUN npm run build

FROM nginx:latest as deploy

RUN apt update && apt install -y curl

COPY --from=build /app/dist /usr/share/nginx/html