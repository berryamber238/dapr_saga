# Dapr & Nacos Configuration Standardization Plan

Based on the analysis of your project structure (`DaprSagaProject.sln`, `.NET` services, `docker-compose`, and existing scripts), I propose the following standardization plan.

## 1. Dapr Component Standardization (`pubsub.yaml`)

**Strategy**: Dapr components do not natively support nested "profile" keys (like `qa:`, `dev:`) within the component file itself. Doing so would cause Dapr to reject the file.
**Solution**: We will strictly use **Environment Variables** within `pubsub.yaml`. This achieves the goal of environment isolation by allowing each environment (QA, DEV, PROD) to inject its own values at runtime.

**Changes**:
- Refactor `components/pubsub.yaml` and `docker-components/pubsub.yaml` to replace hardcoded values with variables:
  ```yaml
  metadata:
    - name: connectionString
      value: "amqp://${RABBITMQ_USER}:${RABBITMQ_PASSWORD}@${RABBITMQ_HOST}:${RABBITMQ_PORT}"
  ```

## 2. Nacos Configuration Strategy

**Strategy**: We will enhance the Nacos upload script to support Namespaces and Environment-specific Data IDs.

**Changes**:
- **Refactor `upload-nacos-config.sh`**:
  - Add support for command-line arguments: `./upload-nacos-config.sh [dev|qa|prod]`.
  - **Namespaces**: Define `DEV_NAMESPACE`, `QA_NAMESPACE`, `PROD_NAMESPACE` (defaulting to public/custom IDs).
  - **Data ID Naming**: Change from `service-name.json` to `service-name-{env}.json` (e.g., `saga-coordinator-dev.json`).
  - **Dynamic Content**: Allow injecting different config values based on the selected environment.

## 3. Variable Injection & Environment Management

**Strategy**: We need to ensure the environment variables required by `pubsub.yaml` are present in both Docker and Local execution modes.

**Changes**:
- **Docker Compose (`docker-compose.yml`)**:
  - Add a common `x-dapr-environment` definition or update all `*-dapr` services to include the necessary environment variables (`RABBITMQ_HOST`, `RABBITMQ_USER`, etc.).
- **Local Script (`start-all.sh`)**:
  - Add `export` statements at the beginning of the script to set default Development values (e.g., `export RABBITMQ_HOST=localhost`).

## 4. Security & Validation

**Strategy**: Ensure configurations are valid before starting services.

**Changes**:
- **Create `scripts/validate-config.sh`**:
  - A script that checks if required environment variables (like `RABBITMQ_HOST`, `NACOS_URL`) are set.
  - Can be run as a pre-start check in `start-all.sh`.

## Implementation Steps

1.  **Modify `components/pubsub.yaml`** to use environment variables.
2.  **Update `docker-components/pubsub.yaml`** to match.
3.  **Rewrite `upload-nacos-config.sh`** to handle environments, namespaces, and Data ID suffixes.
4.  **Create `scripts/validate-config.sh`** for configuration verification.
5.  **Update `start-all.sh`** to export default variables and run validation.
6.  (Optional) Update `docker-compose.yml` to explicitly pass these variables to Dapr sidecars.

This plan aligns with your requirements for standardization while adhering to Dapr's technical constraints.