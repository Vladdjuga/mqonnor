#!/usr/bin/env bash
set -euo pipefail

PORT=8080
export RUN_ID="$(date -u '+%Y-%m-%d_%H-%M-%S')"
REPORT_DIR="stress-reports/${RUN_ID}"

# --- kill host processes on the API port ---
echo ">> Checking port $PORT..."
pids=$(lsof -ti tcp:"$PORT" 2>/dev/null || true)
if [ -n "$pids" ]; then
    echo "   Killing PIDs on port $PORT: $pids"
    kill -9 $pids
else
    echo "   Port $PORT is free"
fi

# --- tear down any previous stack ---
echo ">> Tearing down previous stack..."
docker compose down --remove-orphans

# --- build & start the full stack (mongo + API + stress-tester) ---
echo ">> Building and starting stack (run: $RUN_ID)..."
docker compose up --build --abort-on-container-exit --exit-code-from stress-tester

# --- show stress-tester output cleanly ---
echo ""
echo ">> Stress-tester results:"
docker compose logs stress-tester

# --- report location ---
echo ""
echo ">> Report saved → ${REPORT_DIR}/report.html"
echo ">> Done. API is still up at http://localhost:$PORT"
echo "   Run 'docker compose down' to stop everything."
