#!/usr/bin/env bash
#
# generate-certs.sh
# Generates self-signed SSL certificates for the nginx reverse proxy
# in the docker-compose setup. Idempotent: skips generation if files exist.
#
set -euo pipefail

# ---------------------------------------------------------------------------
# Resolve paths relative to the script's own location
# ---------------------------------------------------------------------------
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="${SCRIPT_DIR}/certs/nginx"
KEY_FILE="${CERT_DIR}/server.key"
CRT_FILE="${CERT_DIR}/server.crt"
GITIGNORE_FILE="${CERT_DIR}/.gitignore"

# ---------------------------------------------------------------------------
# Certificate parameters
# ---------------------------------------------------------------------------
RSA_KEY_SIZE=2048
VALIDITY_DAYS=365
SUBJECT="/C=RU/ST=Moscow/L=Moscow/O=SmartWallet/OU=Dev/CN=localhost"
SAN_DNS="localhost"
SAN_IP="127.0.0.1"

# ---------------------------------------------------------------------------
# Idempotency check
# ---------------------------------------------------------------------------
if [[ -f "${KEY_FILE}" && -f "${CRT_FILE}" ]]; then
    echo "[SKIP] Certificates already exist:"
    echo "       ${KEY_FILE}"
    echo "       ${CRT_FILE}"
    echo "       Remove them manually to regenerate."
    exit 0
fi

# ---------------------------------------------------------------------------
# Create output directory
# ---------------------------------------------------------------------------
mkdir -p "${CERT_DIR}"

# ---------------------------------------------------------------------------
# Generate .gitignore (always refresh to ensure it is present)
# ---------------------------------------------------------------------------
cat > "${GITIGNORE_FILE}" <<'EOF'
# Auto-generated SSL certificate files — do not commit
*.key
*.crt
*.pem
*.pfx
*.csr
*.srl
EOF

echo "[INFO] .gitignore written to ${GITIGNORE_FILE}"

# ---------------------------------------------------------------------------
# Generate self-signed certificate with SAN via a temporary OpenSSL config
# ---------------------------------------------------------------------------
OPENSSL_CNF="$(mktemp)"
trap 'rm -f "${OPENSSL_CNF}"' EXIT

cat > "${OPENSSL_CNF}" <<EOF
[req]
default_bits       = ${RSA_KEY_SIZE}
prompt             = no
default_md         = sha256
distinguished_name = dn
x509_extensions    = v3_req

[dn]
C  = RU
ST = Moscow
L  = Moscow
O  = SmartWallet
OU = Dev
CN = localhost

[v3_req]
basicConstraints     = CA:FALSE
keyUsage             = digitalSignature, keyEncipherment
extendedKeyUsage     = serverAuth
subjectAltName       = @alt_names

[alt_names]
DNS.1 = ${SAN_DNS}
IP.1  = ${SAN_IP}
EOF

echo "[INFO] Generating RSA ${RSA_KEY_SIZE}-bit key..."
openssl genrsa -out "${KEY_FILE}" "${RSA_KEY_SIZE}" 2>/dev/null

echo "[INFO] Generating self-signed certificate (valid ${VALIDITY_DAYS} days)..."
openssl req -new -x509 \
    -key "${KEY_FILE}" \
    -out "${CRT_FILE}" \
    -days "${VALIDITY_DAYS}" \
    -sha256 \
    -config "${OPENSSL_CNF}" \
    -extensions v3_req

# Restrict permissions on the private key
chmod 600 "${KEY_FILE}"
chmod 644 "${CRT_FILE}"

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
echo ""
echo "============================================================"
echo " Certificates generated successfully"
echo "============================================================"
echo ""
echo " Key:   ${KEY_FILE}"
echo " Cert:  ${CRT_FILE}"
echo ""
echo " Valid for:    ${VALIDITY_DAYS} days"
echo " Key size:     RSA ${RSA_KEY_SIZE}-bit"
echo " SAN entries:  DNS:${SAN_DNS}, IP:${SAN_IP}"
echo ""
echo "------------------------------------------------------------"
echo " Usage in docker-compose.yml (nginx volume mount):"
echo ""
echo "   volumes:"
echo "     - ./certs/nginx/server.crt:/etc/nginx/ssl/server.crt:ro"
echo "     - ./certs/nginx/server.key:/etc/nginx/ssl/server.key:ro"
echo "------------------------------------------------------------"
echo ""
echo " nginx.conf snippet:"
echo ""
echo "   server {"
echo "       listen 443 ssl;"
echo "       ssl_certificate     /etc/nginx/ssl/server.crt;"
echo "       ssl_certificate_key /etc/nginx/ssl/server.key;"
echo "       ..."
echo "   }"
echo "------------------------------------------------------------"
echo ""
echo " To trust the certificate locally (optional):"
echo "   sudo cp ${CRT_FILE} /usr/local/share/ca-certificates/smartwallet.crt"
echo "   sudo update-ca-certificates"
echo ""
echo " Verify the certificate:"
echo "   openssl x509 -in ${CRT_FILE} -text -noout"
echo ""
