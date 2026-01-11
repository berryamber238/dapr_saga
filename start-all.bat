@echo off
if not exist logs mkdir logs

echo Starting Dapr Demo Services...

echo Starting Saga Coordinator...
start /B dapr run --app-id saga-coordinator --app-port 5001 --dapr-http-port 3501 --dapr-grpc-port 50001 --components-path ./components -- dotnet run --project src/Saga.Coordinator/Saga.Coordinator.csproj --urls "http://localhost:5001" > logs/coordinator.log 2>&1

echo Starting Service CTA...
start /B dapr run --app-id service-cta --app-port 5002 --dapr-http-port 3502 --dapr-grpc-port 50002 --components-path ./components -- dotnet run --project src/Service.CTA/Service.CTA.csproj --urls "http://localhost:5002" > logs/cta.log 2>&1

echo Starting Service Genesis...
start /B dapr run --app-id service-genesis --app-port 5003 --dapr-http-port 3503 --dapr-grpc-port 50003 --components-path ./components -- dotnet run --project src/Service.Genesis/Service.Genesis.csproj --urls "http://localhost:5003" > logs/genesis.log 2>&1

echo Starting Service PerfectCage...
start /B dapr run --app-id service-perfectcage --app-port 5004 --dapr-http-port 3504 --dapr-grpc-port 50004 --components-path ./components -- dotnet run --project src/Service.PerfectCage/Service.PerfectCage.csproj --urls "http://localhost:5004" > logs/perfectcage.log 2>&1

echo Starting Service Query...
start /B dapr run --app-id service-query --app-port 5005 --dapr-http-port 3505 --dapr-grpc-port 50005 --components-path ./components -- dotnet run --project src/Service.Query/Service.Query.csproj --urls "http://localhost:5005" > logs/query.log 2>&1

echo Starting Service Notification...
start /B dapr run --app-id service-notification --app-port 5006 --dapr-http-port 3506 --dapr-grpc-port 50006 --components-path ./components -- dotnet run --project src/Service.Notification/Service.Notification.csproj --urls "http://localhost:5006" > logs/notification.log 2>&1

echo All services started! Check logs/ directory for output.
