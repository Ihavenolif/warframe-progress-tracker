#!/bin/sh
set -e
rm -rf /etc/nginx/conf.d/*
envsubst '${ORIGIN_URL} ${CERT_FILENAME} ${CERT_KEY_FILENAME}'  < /etc/nginx/nginx.templates.d/frontend.conf > /etc/nginx/conf.d/frontend.conf
envsubst '${ORIGIN_URL}' < /etc/nginx/nginx.templates.d/http.conf > /etc/nginx/conf.d/http.conf
exec nginx -g 'daemon off;'