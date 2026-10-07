#!/bin/sh
set -e
CONFIG_DIR="wwwroot/configuration"

envsubst < $CONFIG_DIR/configuration.json > $CONFIG_DIR/configuration.production.json

dotnet NotoriousSkeleton.API.dll