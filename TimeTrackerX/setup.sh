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

echo "Setup complete."
