#!/bin/bash

# Usage: ./upload-nacos-config.sh [dev|qa|prod]
ENV=${1:-dev}
ENV=$(echo "$ENV" | tr '[:upper:]' '[:lower:]')

# Nacos Server Address
NACOS_URL="http://localhost:8848"
GROUP="DEFAULT_GROUP"

# Define Namespaces and Log Levels based on Environment
case $ENV in
  dev)
    NAMESPACE="DEV_NAMESPACE"
    LOG_LEVEL="Debug"
    ;;
  qa)
    NAMESPACE="QA_NAMESPACE"
    LOG_LEVEL="Information"
    ;;
  prod)
    NAMESPACE="PROD_NAMESPACE"
    LOG_LEVEL="Warning"
    ;;
  *)
    echo "Invalid environment: $ENV. Use dev, qa, or prod."
    exit 1
    ;;
esac

echo "=========================================="
echo "Starting Nacos Configuration Upload"
echo "Environment: $ENV"
echo "Namespace:   $NAMESPACE"
echo "Log Level:   $LOG_LEVEL"
echo "Target:      $NACOS_URL"
echo "=========================================="

# Create Namespace (Idempotent-ish check)
echo "Creating Namespace $NAMESPACE..."
curl -s -X POST "$NACOS_URL/nacos/v1/console/namespaces" \
    -d "customNamespaceId=$NAMESPACE" \
    -d "namespaceName=$NAMESPACE" \
    -d "namespaceDesc=Namespace for $ENV environment"
echo -e "\nNamespace creation request sent."

# Function to publish config
publish_config() {
    local service_name=$1
    local content=$2
    
    # Data ID convention: service-name-env.json
    local data_id="${service_name}-${ENV}.json"
    
    echo "Uploading $data_id to namespace $NAMESPACE..."
    
    response=$(curl -s -X POST "$NACOS_URL/nacos/v1/cs/configs" \
        -d "dataId=$data_id" \
        -d "group=$GROUP" \
        -d "tenant=$NAMESPACE" \
        -d "content=$content" \
        -d "type=json")

    if [[ "$response" == "true" ]]; then
        echo "✅ Success: $data_id"
    else
        echo "❌ Failed: $data_id (Response: $response)"
    fi
    echo "------------------------------------------"
}

# 1. Saga Coordinator
CONTENT_SAGA='{
  "RetryPolicy": {
    "MaxRetries": 3,
    "InitialDelayMilliseconds": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "'$LOG_LEVEL'",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "saga-coordinator" "$CONTENT_SAGA"

# 2. Business Coordinator
CONTENT_BUSINESS='{
  "Logging": {
    "LogLevel": {
      "Default": "'$LOG_LEVEL'",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "business-coordinator" "$CONTENT_BUSINESS"

# 3. Service CTA
CONTENT_CTA='{
  "Logging": {
    "LogLevel": {
      "Default": "'$LOG_LEVEL'",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-cta" "$CONTENT_CTA"

# 4. Service Genesis
CONTENT_GENESIS='{
  "Logging": {
    "LogLevel": {
      "Default": "'$LOG_LEVEL'",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-genesis" "$CONTENT_GENESIS"

# 5. Service PerfectCage
CONTENT_PERFECTCAGE='{
  "Logging": {
    "LogLevel": {
      "Default": "'$LOG_LEVEL'",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-perfectcage" "$CONTENT_PERFECTCAGE"

# 6. Service Query
CONTENT_QUERY='{
  "Logging": {
    "LogLevel": {
      "Default": "'$LOG_LEVEL'",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-query" "$CONTENT_QUERY"

# 7. Service Notification
CONTENT_NOTIFICATION='{
  "Logging": {
    "LogLevel": {
      "Default": "'$LOG_LEVEL'",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-notification" "$CONTENT_NOTIFICATION"

# 8. Service OutboxWorker
CONTENT_OUTBOX='{
  "Logging": {
    "LogLevel": {
      "Default": "'$LOG_LEVEL'",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-outbox-worker" "$CONTENT_OUTBOX"

# 9. Shared Configuration
# Note: Hostnames might differ per env, here we use defaults or inject vars if needed
# For now we assume consistent internal DNS or override via other means
CONTENT_SHARED='{
  "MongoDB": {
    "ConnectionString": "mongodb://mongodb:27017",
    "DatabaseName": "DaprSagaDB"
  }
}'
publish_config "shared-config" "$CONTENT_SHARED"

echo "=========================================="
echo "Upload Complete for $ENV!"
echo "Verify at $NACOS_URL/nacos (Namespace: $NAMESPACE)"
echo "=========================================="
