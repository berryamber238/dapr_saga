#!/bin/bash

# Nacos Server Address (Localhost for running from host machine)
NACOS_URL="http://localhost:8848"
GROUP="DEFAULT_GROUP"

echo "=========================================="
echo "Starting Nacos Configuration Upload"
echo "Target Server: $NACOS_URL"
echo "=========================================="

# Function to publish config
publish_config() {
    local data_id=$1
    local content=$2
    
    echo "Uploading $data_id..."
    
    response=$(curl -s -X POST "$NACOS_URL/nacos/v1/cs/configs" \
        -d "dataId=$data_id" \
        -d "group=$GROUP" \
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
# Contains RetryPolicy
CONTENT_SAGA='{
  "RetryPolicy": {
    "MaxRetries": 3,
    "InitialDelayMilliseconds": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "saga-coordinator.json" "$CONTENT_SAGA"

# 2. Business Coordinator
CONTENT_BUSINESS='{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "business-coordinator.json" "$CONTENT_BUSINESS"

# 3. Service CTA
CONTENT_CTA='{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-cta.json" "$CONTENT_CTA"

# 4. Service Genesis
CONTENT_GENESIS='{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-genesis.json" "$CONTENT_GENESIS"

# 5. Service PerfectCage
CONTENT_PERFECTCAGE='{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-perfectcage.json" "$CONTENT_PERFECTCAGE"

# 6. Service Query
CONTENT_QUERY='{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-query.json" "$CONTENT_QUERY"

# 7. Service Notification
CONTENT_NOTIFICATION='{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-notification.json" "$CONTENT_NOTIFICATION"

# 8. Service OutboxWorker
CONTENT_OUTBOX='{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}'
publish_config "service-outbox-worker.json" "$CONTENT_OUTBOX"

# 9. Shared Configuration (MongoDB)
# Note: For Docker environment, the host is 'mongodb', not 'localhost'
CONTENT_SHARED='{
  "MongoDB": {
    "ConnectionString": "mongodb://mongodb:27017",
    "DatabaseName": "DaprSagaDB"
  }
}'
publish_config "shared-config.json" "$CONTENT_SHARED"

echo "=========================================="
echo "Upload Complete!"
echo "You can verify at $NACOS_URL/nacos"
echo "=========================================="
