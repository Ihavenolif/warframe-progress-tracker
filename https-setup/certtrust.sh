sudo mkdir -p /usr/local/share/ca-certificates
sudo cp localhost-me.crt /usr/local/share/ca-certificates/localhost-me.crt
sudo update-ca-trust
sudo trust extract-compat
