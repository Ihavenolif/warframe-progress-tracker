#!/bin/sh
echo "Starting certbot... Waiting 30 seconds for nginx to start"
sleep 30
certbot certonly --webroot -w /var/www/html --email $LETSENCRYPT_EMAIL --agree-tos --no-eff-email -d $ORIGIN_URL