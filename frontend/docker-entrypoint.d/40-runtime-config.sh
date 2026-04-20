#!/bin/sh
set -eu

if [ -z "${API_BASE_URL:-}" ]; then
  echo "ERROR: API_BASE_URL is required"
  exit 1
fi

cat > /usr/share/nginx/html/config.js <<EOF
window.__APP_CONFIG__ = {
  API_BASE_URL: "${API_BASE_URL}"
};
EOF
