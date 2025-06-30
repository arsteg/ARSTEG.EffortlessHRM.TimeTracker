#!/bin/bash

set -e  # Exit immediately if a command exits with a non-zero status

APP_NAME="TimeTrackerX.app"

echo "Re-signing the app..."
codesign --force --deep --sign - "$APP_NAME"

echo "Removing quarantine attribute..."
xattr -rd com.apple.quarantine "$APP_NAME"

echo "Running the application..."
"./$APP_NAME/Contents/MacOS/TimeTrackerX"
