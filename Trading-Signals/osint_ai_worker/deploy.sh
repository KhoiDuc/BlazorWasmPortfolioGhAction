#!/usr/bin/env bash

# ==============================================================================
#  OSINT AI Worker Deployment Script (Simplified & Robust)
#  Deploys ALL source files every time to ensure server is up-to-date.
# ==============================================================================

# ANSI Color Codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${CYAN}======================================================================${NC}"
echo -e "${CYAN}          OSINT AI Worker Deployment Script (Full Deploy)              ${NC}"
echo -e "${CYAN}======================================================================${NC}"

# Find script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
echo -e "Script directory: ${YELLOW}$SCRIPT_DIR${NC}"

# Function to load environment variables from a file
load_env() {
    local env_path="$1"
    if [ -f "$env_path" ]; then
        echo -e "${GREEN}Loading env: $env_path${NC}"
        while IFS= read -r line || [ -n "$line" ]; do
            line=$(echo "$line" | xargs | tr -d '\r')
            if [[ ! "$line" =~ ^# ]] && [[ "$line" == *=* ]]; then
                export "$line"
            fi
        done < "$env_path"
        return 0
    fi
    return 1
}

# Load configuration
echo -e "Loading configuration..."
ENV_LOADED=false
if [ -f "$SCRIPT_DIR/trading_api/.env" ]; then
    load_env "$SCRIPT_DIR/trading_api/.env" && ENV_LOADED=true
fi
if [ -f "$SCRIPT_DIR/.env" ]; then
    load_env "$SCRIPT_DIR/.env" && ENV_LOADED=true
fi

if [ "$ENV_LOADED" = false ]; then
    echo -e "${RED}ERROR: .env file containing DEPLOY_* variables not found.${NC}"
    exit 1
fi

# Validate
DEPLOY_PATH="${DEPLOY_PATH:-/home/thehaohcm/osint_ai_worker}"
DEPLOY_PORT=${DEPLOY_PORT:-22}

if [ -z "$DEPLOY_HOST" ] || [ -z "$DEPLOY_USER" ]; then
    echo -e "${RED}ERROR: DEPLOY_HOST and DEPLOY_USER must be set in .env${NC}"
    exit 1
fi

echo -e "Deploy target: ${YELLOW}$DEPLOY_USER@$DEPLOY_HOST:$DEPLOY_PATH${NC}"

# Build list of files to deploy - ALWAYS include all source files
echo -e "${CYAN}Building file list...${NC}"

# All directories to include
DIRS_TO_INCLUDE=()
for d in "agents" "collectors" "trading_api" "init-db"; do
    if [ -d "$SCRIPT_DIR/$d" ]; then
        DIRS_TO_INCLUDE+=("$d")
    fi
done

# All individual files to include  
FILES_TO_INCLUDE=()
for f in "osint_ai_worker.py" "requirements.txt" "Dockerfile" "docker-compose.yaml" ".env.example"; do
    if [ -f "$SCRIPT_DIR/$f" ]; then
        FILES_TO_INCLUDE+=("$f")
    fi
done

ALL_ITEMS=("${DIRS_TO_INCLUDE[@]}" "${FILES_TO_INCLUDE[@]}")

# Always include .env if it exists (for local secrets)
if [ -f "$SCRIPT_DIR/.env" ]; then
    ALL_ITEMS+=(".env")
fi
if [ -f "$SCRIPT_DIR/trading_api/.env" ]; then
    ALL_ITEMS+=("trading_api/.env")
fi

echo -e "${GREEN}Will deploy ${#ALL_ITEMS[@]} items (and their files):${NC}"
for item in "${ALL_ITEMS[@]}"; do
    if [ -d "$SCRIPT_DIR/$item" ]; then
        echo -e "  - $item/ (containing:)"
        find "$SCRIPT_DIR/$item" -type f | sed "s|$SCRIPT_DIR/||" | sed 's|^|    * |'
    else
        echo -e "  - $item"
    fi
done
echo ""

# Test SSH connection
echo -e "${CYAN}[1/3] Testing SSH connection...${NC}"
SSH_OPTS="-p $DEPLOY_PORT -o ConnectTimeout=10 -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null"

# Build SSH prefix for password auth
SSH_CMD="ssh $SSH_OPTS $DEPLOY_USER@$DEPLOY_HOST"
SCP_CMD="scp -P $DEPLOY_PORT -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null"

if [ -n "$DEPLOY_PASSWORD" ] && [ "$DEPLOY_PASSWORD" != "your_ssh_password" ]; then
    if command -v sshpass >/dev/null 2>&1; then
        echo -e "${GREEN}Using sshpass for authentication${NC}"
        SSH_CMD="sshpass -p $DEPLOY_PASSWORD $SSH_CMD"
        SCP_CMD="sshpass -p $DEPLOY_PASSWORD $SCP_CMD"
    else
        echo -e "${YELLOW}sshpass not found. Will use interactive SSH.${NC}"
        echo -e "${YELLOW}Install sshpass for passwordless deploy.${NC}"
    fi
fi

if ! $SSH_CMD "echo 'SSH_OK'"; then
    echo -e "${RED}ERROR: Cannot SSH to $DEPLOY_USER@$DEPLOY_HOST:$DEPLOY_PORT${NC}"
    echo -e "${YELLOW}Check: DEPLOY_HOST, DEPLOY_USER, DEPLOY_PASSWORD, DEPLOY_PORT in .env${NC}"
    exit 1
fi
echo -e "${GREEN}SSH connection OK!${NC}"
echo ""

# Ensure remote directory exists
$SSH_CMD "mkdir -p '$DEPLOY_PATH'"

# Create tarball
echo -e "${CYAN}[2/3] Creating deployment tarball...${NC}"
TARBALL="$SCRIPT_DIR/deploy.tar.gz"
tar -czf "$TARBALL" -C "$SCRIPT_DIR" "${ALL_ITEMS[@]}" 2>&1
if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: Failed to create tarball${NC}"
    exit 1
fi
echo -e "${GREEN}Tarball created ($(du -h "$TARBALL" | cut -f1))${NC}"
echo ""

# Upload and deploy
echo -e "${CYAN}[3/3] Uploading and deploying...${NC}"
$SCP_CMD "$TARBALL" "$DEPLOY_USER@$DEPLOY_HOST:$DEPLOY_PATH/deploy.tar.gz"
if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: SCP upload failed${NC}"
    rm -f "$TARBALL"
    exit 1
fi
echo -e "${GREEN}Upload OK!${NC}"

echo -e "Extracting and restarting containers (worker + api only, DB untouched)..."
if ! $SSH_CMD "cd '$DEPLOY_PATH' && tar -xzf deploy.tar.gz && rm -f deploy.tar.gz && docker compose up --build -d --no-deps worker api"; then
    echo -e "${RED}ERROR: Remote deployment failed. Check docker logs on server.${NC}"
    rm -f "$TARBALL"
    exit 1
fi

# Cleanup
rm -f "$TARBALL"

echo ""
echo -e "${CYAN}======================================================================${NC}"
echo -e "${GREEN}Deployment complete! Files on server:${NC}"
$SSH_CMD "ls -la '$DEPLOY_PATH/'*.py '$DEPLOY_PATH/'*.txt '$DEPLOY_PATH/'Dockerfile '$DEPLOY_PATH/'docker-compose.yaml 2>/dev/null"
echo -e "${CYAN}======================================================================${NC}"