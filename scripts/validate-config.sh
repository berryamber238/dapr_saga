#!/bin/bash

# Configuration Validation Script
# Checks if required environment variables are set for Dapr components

REQUIRED_VARS=(
  "RABBITMQ_HOST"
  "RABBITMQ_PORT"
  "RABBITMQ_USER"
  "RABBITMQ_PASSWORD"
)

MISSING_VARS=0

echo "Validating Configuration..."

for var in "${REQUIRED_VARS[@]}"; do
  if [ -z "${!var}" ]; then
    echo "❌ Error: Environment variable '$var' is not set."
    MISSING_VARS=$((MISSING_VARS+1))
  else
    echo "✅ '$var' is set."
  fi
done

if [ $MISSING_VARS -gt 0 ]; then
  echo "---------------------------------------------------"
  echo "Validation Failed: $MISSING_VARS required variables are missing."
  echo "Please export them before starting Dapr."
  echo "Example: export RABBITMQ_HOST=localhost"
  echo "---------------------------------------------------"
  exit 1
fi

echo "Configuration Validation Passed."
exit 0
