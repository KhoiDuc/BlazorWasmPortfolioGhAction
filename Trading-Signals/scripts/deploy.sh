#!/usr/bin/env bash

# ==============================================================================
#  Trading Signals Deployment Script (Flat Upload & Git-based Auto-Backup)
# ==============================================================================

# Exit immediately if a command exits with a non-zero status
set -e

# ANSI Color Codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}======================================================================${NC}"
echo -e "${CYAN}      Trading Signals Git-based Flat Deployment & Backup Script       ${NC}"
echo -e "${CYAN}======================================================================${NC}"

# Find project root directory (parent of this script's directory)
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Function to load environment variables from a file
load_env() {
    local env_path="$1"
    if [ -f "$env_path" ]; then
        echo -e "${GREEN}✓ Found env file at: $env_path${NC}"
        # Read lines and export variables (handling comments & empty lines)
        while IFS= read -r line || [ -n "$line" ]; do
            # Strip whitespace and carriage returns
            line=$(echo "$line" | xargs | tr -d '\r')
            # Skip comments and empty lines
            if [[ ! "$line" =~ ^# ]] && [[ "$line" == *=* ]]; then
                export "$line"
            fi
        done < "$env_path"
        return 0
    fi
    return 1
}

# Try loading .env from multiple potential locations
echo -e "Loading configuration..."
if load_env "$PROJECT_ROOT/.env"; then
    :
elif load_env "$PROJECT_ROOT/scripts/.env"; then
    :
elif load_env "./.env"; then
    :
else
    echo -e "${RED}✗ Error: .env file not found in project root or scripts directory.${NC}"
    echo -e "Please copy .env.example to .env and configure your DEPLOY_* server variables."
    exit 1
fi

# Validate server deploy variables
MISSING_VARS=()
[ -z "$DEPLOY_HOST" ] && MISSING_VARS+=("DEPLOY_HOST")
[ -z "$DEPLOY_USER" ] && MISSING_VARS+=("DEPLOY_USER")
[ -z "$DEPLOY_PATH" ] && MISSING_VARS+=("DEPLOY_PATH")

if [ ${#MISSING_VARS[@]} -ne 0 ]; then
    echo -e "${RED}✗ Error: Missing required deployment configuration in .env:${NC}"
    for var in "${MISSING_VARS[@]}"; do
        echo -e "  - $var"
    done
    echo -e "Please configure these in your .env file."
    exit 1
fi

# Fallback for DEPLOY_PORT
DEPLOY_PORT=${DEPLOY_PORT:-22}

# Setup SSH and SCP command prefixes based on password configuration
SSH_PREFIX=""
SCP_PREFIX=""
USE_PASSWORD=false

# Check if DEPLOY_PASSWORD is provided and is not a placeholder
if [ -n "$DEPLOY_PASSWORD" ] && [ "$DEPLOY_PASSWORD" != "your_ssh_password" ]; then
    USE_PASSWORD=true
fi

SSH_OPTS=""
if [ "$USE_PASSWORD" = true ]; then
    SSH_OPTS="-o PubkeyAuthentication=no -o PreferredAuthentications=password -o StrictHostKeyChecking=no"
    # Check if sshpass is installed
    if command -v sshpass >/dev/null 2>&1; then
        echo -e "${GREEN}✓ sshpass detected. Using password-based authentication.${NC}"
        SSH_PREFIX="sshpass -p $DEPLOY_PASSWORD"
        SCP_PREFIX="sshpass -p $DEPLOY_PASSWORD"
    else
        echo -e "${YELLOW}⚠ Warning: DEPLOY_PASSWORD is configured, but 'sshpass' is not installed.${NC}"
        echo -e "  Falling back to native interactive SSH/SCP. You may be prompted to enter your password."
        echo -e "  ${CYAN}(Tip: To automate this on macOS, run: brew install hudochenkov/sshpass/sshpass)${NC}\n"
    fi
else
    echo -e "${CYAN}ℹ Info: DEPLOY_PASSWORD not set or using default placeholder. Using SSH Key / Agent auth.${NC}"
fi

# Check if git repository exists
if [ ! -d "$PROJECT_ROOT/.git" ]; then
    echo -e "${RED}✗ Error: Project root is not a Git repository. Cannot detect diffs.${NC}"
    exit 1
fi

# Detect modified and untracked .py files inside scripts/ directory (excluding osint_ai_worker/osint_ai)
echo -e "Detecting modified python scripts inside scripts/ directory via Git..."
FILES_TO_DEPLOY=()
while IFS= read -r file; do
    if [ -n "$file" ] && [ -f "$PROJECT_ROOT/$file" ]; then
        # Ensure it is in the scripts directory and not in osint_ai_worker or osint_ai
        if [[ "$file" == scripts/* ]] && [[ "$file" != *osint_ai_worker* ]] && [[ "$file" != *osint_ai* ]]; then
            # Deduplicate
            if [[ ! " ${FILES_TO_DEPLOY[*]} " =~ " ${file} " ]]; then
                FILES_TO_DEPLOY+=("$file")
            fi
        fi
    fi
done < <( (git diff --name-only HEAD -- "$PROJECT_ROOT/scripts"; git ls-files --others --exclude-standard -- "$PROJECT_ROOT/scripts") | grep '\.py$' || true )

if [ ${#FILES_TO_DEPLOY[@]} -eq 0 ]; then
    echo -e "${YELLOW}⚠ No modified or untracked .py files detected via Git diff.${NC}"
    echo -e "Nothing to deploy. Exiting."
    exit 0
fi

echo -e "${GREEN}✓ Detected ${#FILES_TO_DEPLOY[@]} modified python script(s) to deploy:${NC}"
for file in "${FILES_TO_DEPLOY[@]}"; do
    echo -e "  - $file"
done
echo -e ""

echo -e "Deployment settings:"
echo -e "  - Server Host: ${YELLOW}$DEPLOY_HOST${NC}"
echo -e "  - SSH User   : ${YELLOW}$DEPLOY_USER${NC}"
echo -e "  - SSH Port   : ${YELLOW}$DEPLOY_PORT${NC}"
echo -e "  - Remote Path: ${YELLOW}$DEPLOY_PATH${NC} ${CYAN}(Flat, no subfolders)${NC}"
echo -e ""

# Generate timestamp in format YYYYMMDD-HHMMSS
TIMESTAMP=$(date +%Y%m%d-%H%M%S)

# Perform connection check
echo -e "${CYAN}[1/3] Testing SSH connection to server...${NC}"
TEST_CMD="echo -n Connection successful!"

if $SSH_PREFIX ssh $SSH_OPTS -p "$DEPLOY_PORT" -o ConnectTimeout=5 -o BatchMode=no "$DEPLOY_USER@$DEPLOY_HOST" "$TEST_CMD" > /dev/null; then
    echo -e "${GREEN}✓ SSH Connection successfully established.${NC}\n"
else
    echo -e "${RED}✗ Error: Cannot connect to server via SSH on $DEPLOY_HOST:$DEPLOY_PORT.${NC}"
    echo -e "Please check your credentials, network connection, or SSH key/password setup."
    exit 1
fi

# Execute remote directory check & creation
# Ensure DEPLOY_PATH directory exists on server
$SSH_PREFIX ssh $SSH_OPTS -p "$DEPLOY_PORT" "$DEPLOY_USER@$DEPLOY_HOST" "mkdir -p \"$DEPLOY_PATH\""

# Execute flat backups
echo -e "${CYAN}[2/3] Backing up existing files directly in DEPLOY_PATH...${NC}"
for file in "${FILES_TO_DEPLOY[@]}"; do
    LOCAL_FILE="$PROJECT_ROOT/$file"
    
    FILE_NAME=$(basename "$file")
    BASE_NAME="${FILE_NAME%.*}"
    EXTENSION="${FILE_NAME##*.}"
    
    REMOTE_FILE="$DEPLOY_PATH/$FILE_NAME"
    BACKUP_FILE="${BASE_NAME}_bk_${TIMESTAMP}.${EXTENSION}"
    
    echo -e "Processing: ${YELLOW}$FILE_NAME${NC}"
    
    # Check if remote file exists and copy it flat to DEPLOY_PATH to create backup
    BACKUP_CMD="if [ -f \"$REMOTE_FILE\" ]; then cp \"$REMOTE_FILE\" \"$DEPLOY_PATH/$BACKUP_FILE\" && echo 'BACKUP_DONE'; else echo 'NO_FILE'; fi"
    BACKUP_STATUS=$($SSH_PREFIX ssh $SSH_OPTS -p "$DEPLOY_PORT" "$DEPLOY_USER@$DEPLOY_HOST" "$BACKUP_CMD")
    
    if [ "$BACKUP_STATUS" = "BACKUP_DONE" ]; then
        echo -e "  - ${GREEN}Backup created flat in DEPLOY_PATH:${NC} $BACKUP_FILE"
    else
        echo -e "  - ${YELLOW}No existing file on server to backup (will perform fresh flat upload)${NC}"
    fi
done
echo -e ""

# Execute flat uploads via SCP
echo -e "${CYAN}[3/3] Uploading modified script files flat to DEPLOY_PATH via SCP & setting permissions...${NC}"
for file in "${FILES_TO_DEPLOY[@]}"; do
    LOCAL_FILE="$PROJECT_ROOT/$file"
    FILE_NAME=$(basename "$file")
    REMOTE_FILE="$DEPLOY_PATH/$FILE_NAME"
    
    echo -e "Uploading: ${YELLOW}$file${NC} → ${YELLOW}$REMOTE_FILE${NC}"
    $SCP_PREFIX scp $SSH_OPTS -P "$DEPLOY_PORT" "$LOCAL_FILE" "$DEPLOY_USER@$DEPLOY_HOST:$REMOTE_FILE"
    echo -e "  - ${GREEN}Flat upload complete ✓${NC}"
    
    # Set chmod 777 permissions
    echo -e "  - Setting permission 777..."
    $SSH_PREFIX ssh $SSH_OPTS -p "$DEPLOY_PORT" "$DEPLOY_USER@$DEPLOY_HOST" "chmod 777 \"$REMOTE_FILE\""
done

echo -e ""
echo -e "${CYAN}======================================================================${NC}"
echo -e "${GREEN}🎉 Deployment finished successfully! All modified scripts are live flat. ${NC}"
echo -e "${CYAN}======================================================================${NC}"
