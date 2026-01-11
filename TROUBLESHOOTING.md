# Troubleshooting Guide

## Common Issues

### 1. Service Startup Failures
*   **Symptom**: `dapr run` fails or service exits immediately.
*   **Check**:
    *   Port conflicts: Ensure ports 5001-5006 and 3501-3506/50001-50006 are free.
    *   `components` path: Ensure you run the script from the project root so `./components` is found.
    *   NuGet restore: Run `dotnet restore` manually if build fails.

### 2. Kafka Connection Issues
*   **Symptom**: Logs show "Error connecting to broker" or "Topic not found".
*   **Check**:
    *   Is Kafka running? (`docker ps` should show kafka container).
    *   Is the port correct? `pubsub.yaml` uses `localhost:9092`.
    *   If running inside Docker (not host), use `host.docker.internal:9092` in yaml.

### 3. MongoDB Connection Issues
*   **Symptom**: "Timeout connecting to server".
*   **Check**:
    *   Is MongoDB running? (`docker ps`).
    *   Connection string in `appsettings.json`. By default code assumes `mongodb://localhost:27017`.

### 4. WebSocket Not Connecting
*   **Symptom**: Test page says "Disconnected".
*   **Check**:
    *   Is Service.Notification (Port 5006) running?
    *   Check browser console for CORS errors (though allowed by default in dev).

## Log Inspection

### View Dapr Sidecar Logs
```bash
dapr logs --app-id saga-coordinator
dapr logs --app-id service-cta
# ... etc
```

### View Application Logs (StdOut)
The start scripts redirect output to the `logs/` directory.
```bash
tail -f logs/coordinator.log
tail -f logs/notification.log
```

## Manual Verification Steps

1.  **Check Dapr Dashboard** (if installed): `dapr dashboard` (usually port 8080).
2.  **Verify Topics**: Use a Kafka tool (like Offset Explorer or CLI) to see if `saga-status` topic exists.
3.  **Verify DB**: Use Compass or Mongo Shell to check `SagaTransactions` collection.
