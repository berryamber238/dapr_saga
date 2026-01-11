#!/bin/bash

# Ensure output directory exists
mkdir -p logs

# Set Environment to Development to enable Swagger
export ASPNETCORE_ENVIRONMENT=Development

echo "Starting Dapr Demo Services..."

# 1. Saga Coordinator (Port 5001, Dapr 3501/60001)
echo "Starting Saga Coordinator..."
./dapr run --app-id saga-coordinator --app-port 5001 --dapr-http-port 3501 --dapr-grpc-port 60001 --resources-path ./components -- dotnet run --project src/Saga.Coordinator/Saga.Coordinator.csproj --urls "http://localhost:5001" --environment Development > logs/coordinator.log 2>&1 &

# 2. Service CTA (Port 5002, Dapr 3502/60002)
echo "Starting Service CTA..."
./dapr run --app-id service-cta --app-port 5002 --dapr-http-port 3502 --dapr-grpc-port 60002 --resources-path ./components -- dotnet run --project src/Service.CTA/Service.CTA.csproj --urls "http://localhost:5002" --environment Development > logs/cta.log 2>&1 &

# 3. Service Genesis (Port 5003, Dapr 3503/60003)
echo "Starting Service Genesis..."
./dapr run --app-id service-genesis --app-port 5003 --dapr-http-port 3503 --dapr-grpc-port 60003 --resources-path ./components -- dotnet run --project src/Service.Genesis/Service.Genesis.csproj --urls "http://localhost:5003" --environment Development > logs/genesis.log 2>&1 &

# 4. Service PerfectCage (Port 5004, Dapr 3504/60004)
echo "Starting Service PerfectCage..."
./dapr run --app-id service-perfectcage --app-port 5004 --dapr-http-port 3504 --dapr-grpc-port 60004 --resources-path ./components -- dotnet run --project src/Service.PerfectCage/Service.PerfectCage.csproj --urls "http://localhost:5004" --environment Development > logs/perfectcage.log 2>&1 &

# 5. Service Query (Port 5005, Dapr 3505/60005)
echo "Starting Service Query..."
./dapr run --app-id service-query --app-port 5005 --dapr-http-port 3505 --dapr-grpc-port 60005 --resources-path ./components -- dotnet run --project src/Service.Query/Service.Query.csproj --urls "http://localhost:5005" --environment Development > logs/query.log 2>&1 &

# 6. Service Notification (Port 5006, Dapr 3506/60006)
echo "Starting Service Notification..."
./dapr run --app-id service-notification --app-port 5006 --dapr-http-port 3506 --dapr-grpc-port 60006 --resources-path ./components -- dotnet run --project src/Service.Notification/Service.Notification.csproj --urls "http://localhost:5006" --environment Development > logs/notification.log 2>&1 &

echo "All services started! Check logs/ directory for output."
echo "Access Test Page at: http://localhost:5006/test-client.html (Wait... actually test page is a file, open it directly in browser)"
echo "Or use Query Service Swagger: http://localhost:5005/swagger"
