# Generate private key
openssl genrsa -out localhost-me.key 2048

# Generate certificate signing request (CSR)
openssl req -new -key localhost-me.key -out localhost-me.csr -subj "/CN=localhost.me"

# Generate self-signed certificate
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
-keyout localhost-me.key -out localhost-me.crt \
-subj "/CN=localhost.me" \
-addext "subjectAltName=DNS:www.localhost.me,DNS:api.localhost.me"

openssl pkcs12 -export -out localhost-me.pfx -inkey localhost-me.key -in localhost-me.crt -passout pass:
