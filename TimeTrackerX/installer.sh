#!/bin/bash

# Step 1: Build the application
echo "[INFO] Building the application..."
dotnet build TimeTrackerX.csproj -c Release


# Step 2: Publish the application
echo "[INFO] Publishing the application..."
dotnet publish TimeTrackerX.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ./publish/universal


# Step 3: Create .app bundle
#!/bin/bash

# Define the app bundle name and output directory
APP_BUNDLE="TimeTrackerX.app"
OUTPUT_DIR="./publish/universal"

# Step 3: Create .app bundle
echo "[INFO] Creating .app bundle..."

# Delete existing bundle if it exists
if [ -d "$APP_BUNDLE" ]; then
    echo "[INFO] Removing existing $APP_BUNDLE..."
    rm -rf "$APP_BUNDLE"
fi

# Recreate bundle structure
mkdir -p "${APP_BUNDLE}/Contents/MacOS"
mkdir -p "${APP_BUNDLE}/Contents/Resources"

# Copy built files into the bundle
cp -r "$OUTPUT_DIR/"* "${APP_BUNDLE}/Contents/MacOS/"

echo "[INFO] .app bundle created at: $APP_BUNDLE"

# Step 3.5: Remove Windows-only files not valid on macOS
echo "[INFO] Removing .pdb files and .dll.config files..."
find "$APP_BUNDLE" -name "*.pdb" -delete
find "$APP_BUNDLE" -name "*.dll.config" -delete

