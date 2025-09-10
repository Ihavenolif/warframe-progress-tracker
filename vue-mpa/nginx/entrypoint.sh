#!/bin/sh
set -e
rm -rf /etc/nginx/conf.d/*
envsubst '${ORIGIN_URL}' < /etc/nginx/nginx.templates.d/frontend.conf > /etc/nginx/conf.d/frontend.conf
exec nginx -g 'daemon off;'