#!/bin/sh
set -e

echo "Starting Supervisor"
supervisord -c /etc/supervisor/conf.d/supervisord.conf
echo "Supervisor Stopped"