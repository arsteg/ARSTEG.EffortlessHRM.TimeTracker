#!/bin/bash

# Step 1: Build the application
echo "[INFO] Building the application..."
dotnet build TimeTrackerX.csproj -c Release


# Step 2: Publish the application
echo "[INFO] Publishing the application..."
dotnet publish TimeTrackerX.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -o ./publish/universal


# Step 3: Create .app bundle
#!/bin/bash

# Define the app bundle name and output directory
#APP_BUNDLE="TimeTrackerX.app"
#OUTPUT_DIR="./publish/universal"

# Step 3: Create .app bundle
#echo "[INFO] Creating .app bundle..."

# Delete existing bundle if it exists
#if [ -d "$APP_BUNDLE" ]; then
   # echo "[INFO] Removing existing $APP_BUNDLE..."
  #  rm -rf "$APP_BUNDLE"
#fi

# Recreate bundle structure
#mkdir -p "${APP_BUNDLE}/Contents/MacOS"
#mkdir -p "${APP_BUNDLE}/Contents/Resources"

# Copy Info.plist to the Contents directory
#if [ -f "./info.plist" ]; then
#    echo "[INFO] Adding Info.plist to bundle..."
#    cp ./info.plist "${APP_BUNDLE}/Contents/Info.plist"
#else
#    echo "[WARNING] info.plist not found!"
#fi

# Copy built files into the bundle
#cp -r "$OUTPUT_DIR/"* "${APP_BUNDLE}/Contents/MacOS/"

#echo "[INFO] .app bundle created at: $APP_BUNDLE"

#!/bin/bash

APP_BUNDLE="TimeTrackerX.app"
OUTPUT_DIR="./publish/universal"
APP_NAME="TimeTrackerX"

echo "[INFO] Creating .app bundle..."

# Delete existing bundle if it exists
if [ -d "$APP_BUNDLE" ]; then
    echo "[INFO] Removing existing $APP_BUNDLE..."
    rm -rf "$APP_BUNDLE"
fi

# Recreate bundle structure
mkdir -p "${APP_BUNDLE}/Contents/MacOS"
mkdir -p "${APP_BUNDLE}/Contents/Resources"

# Step 1: Copy Info.plist (or generate a default one)
PLIST_PATH="./info.plist"
if [ -f "$PLIST_PATH" ]; then
    echo "[INFO] Adding Info.plist to bundle..."
    cp "$PLIST_PATH" "${APP_BUNDLE}/Contents/Info.plist"
else
    echo "[INFO] Generating default Info.plist..."
    cat > "${APP_BUNDLE}/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleExecutable</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>com.arsteg.$APP_NAME</string>
    <key>CFBundleVersion</key>
    <string>1.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
</dict>
</plist>
EOF
fi

# Step 2: Copy published files into Contents/MacOS
echo "[INFO] Copying published files into bundle..."
cp -R "$OUTPUT_DIR/"* "${APP_BUNDLE}/Contents/MacOS/"

# Step 3: Ensure the main binary is executable
chmod +x "${APP_BUNDLE}/Contents/MacOS/$APP_NAME"

# Step 4: (Recommended) Sign the bundle for native library support
echo "[INFO] Signing the app bundle (ad-hoc)..."
codesign --force --deep --sign - "$APP_BUNDLE"

echo "[INFO] .app bundle created at: $APP_BUNDLE"




