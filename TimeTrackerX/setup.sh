#!/bin/bash

set -e

echo "Installing .NET 8 SDK..."

# Check if Homebrew is installed
if ! command -v brew &>/dev/null; then
  echo "Homebrew not found. Installing Homebrew..."
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

# Install dotnet 8
brew install --cask dotnet-sdk

# Ensure .NET 8 is available
export PATH="/usr/local/share/dotnet:$PATH"

echo "Installing Avalonia templates..."
dotnet new install Avalonia.Templates

# Copy TimeTrackerX.app to Applications folder
echo "Copying TimeTrackerX.app to /Applications..."
cp -R ./TimeTrackerX.app /Applications/

# Remove the quarantine attribute
echo "Removing quarantine attribute..."
xattr -d com.apple.quarantine /Applications/TimeTrackerX.app || echo "Quarantine attribute not found or already removed."

# Ask user to grant Accessibility access
echo "Checking Accessibility access for abc.app..."

# Get the bundle identifier of the app
APP_BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleIdentifier" "./TimeTrackerX.app/Contents/Info.plist")

echo "App Bundle ID is: $APP_BUNDLE_ID"

# Check TCC database for accessibility permission
ACCESS_GRANTED=$(sqlite3 ~/Library/Application\ Support/com.apple.TCC/TCC.db \
  "SELECT allowed FROM access WHERE client='$APP_BUNDLE_ID' AND service='kTCCServiceAccessibility';")

if [ "$ACCESS_GRANTED" != "1" ]; then
  echo "?? Accessibility access is not granted to abc.app."
  echo "Opening System Settings so you can enable it manually..."
  open "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility"
  echo "?? Please add TimeTrackerX.app to the list and enable it, then re-run this script if necessary."
else
  echo "? Accessibility access already granted."
fi

echo "Setup complete."
