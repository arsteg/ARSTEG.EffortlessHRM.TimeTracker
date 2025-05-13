#!/bin/bash

# Step 1: Build the application
echo "[INFO] Building the application..."
dotnet build TimeTrackerX.csproj -c Release


# Step 2: Publish the application
echo "[INFO] Publishing the application..."
dotnet publish TimeTrackerX.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./publish/universal


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

# Step 3.6: Ensure writable permissions for native libraries (in case some are read-only)
echo "[INFO] Ensuring writable permissions on native libraries..."
find "$APP_BUNDLE/Contents/MacOS" \( -name "*.dylib" -o -name "*.dll" \) -exec chmod +w {} \;

echo "[INFO] Signing native libraries inside the app bundle..."
find "$APP_BUNDLE/Contents/MacOS" \( -name "*.dylib" -o -name "*.dll" \) -exec \
  codesign --force --verify --timestamp --options=runtime \
  --sign "3rd Party Mac Developer Installer: Center for Nutritional Research Charitable Trust (D3V3FH5QFK)" {} \;
  
echo "[INFO] Re-signing the full app bundle..."
codesign --deep --force --verify --timestamp --options=runtime \
  --sign "3rd Party Mac Developer Installer: Center for Nutritional Research Charitable Trust (D3V3FH5QFK)" "$APP_BUNDLE"

if [ $? -ne 0 ]; then
    echo "[ERROR] Code-signing failed."
    exit 1
fi

# Verify signing
codesign -vvv --deep --strict "$APP_BUNDLE"

# Step 4: Notarize the .app
echo "[INFO] Zipping .app for notarization..."
ditto -c -k --keepParent "$APP_BUNDLE" "${APP_NAME}.zip"

echo "[INFO] Submitting .app for notarization..."

xcrun notarytool submit TimeTrackerX.zip --apple-id "dotnetexpertdev@gmail.com" --team-id "D3V3FH5QFK" --password "dryl-xuzx-alur-mkxz" --wait --output-format json

if [ $? -ne 0 ]; then
    echo "[ERROR] Notarization failed."
    exit 1
fi

echo "[INFO] Stapling notarization ticket to .app..."
xcrun stapler staple "$APP_BUNDLE"

# Step 5: Create .dmg
echo "[INFO] Creating .dmg installer..."
mkdir -p dmg_temp
cp -r "$APP_BUNDLE" dmg_temp/
ln -s /Applications dmg_temp/Applications

# Create .dmg
hdiutil create -srcfolder dmg_temp -volname "$DMG_VOLUME" -fs HFS+ -format UDRW -size 100m "$DMG_TEMP"

# Mount and customize .dmg
MOUNT_POINT="/Volumes/$DMG_VOLUME"
hdiutil attach "$DMG_TEMP" -readwrite -noverify -noautoopen
sleep 2

# Set background and window properties (requires background.png in dmg_temp)
if [ -f "dmg_temp/background.png" ]; then
    mkdir -p "$MOUNT_POINT/.background"
    cp dmg_temp/background.png "$MOUNT_POINT/.background/background.png"
fi

# Use AppleScript to customize the .dmg appearance
osascript << EOF
tell application "Finder"
    tell disk "$DMG_VOLUME"
        open
        set current view of container window to icon view
        set toolbar visible of container window to false
        set statusbar visible of container window to false
        set the bounds of container window to {400, 100, 900, 500}
        set theViewOptions to the icon view options of container window
        set arrangement of theViewOptions to not arranged
        set icon size of theViewOptions to 72
        if exists file "background.png" of folder ".background" then
            set background picture of theViewOptions to file ".background:background.png"
        end if
        set position of item "$APP_NAME.app" of container window to {100, 100}
        set position of item "Applications" of container window to {400, 100}
        update without registering applications
        delay 1
        close
    end tell
end tell
EOF

# Unmount .dmg
hdiutil detach "$MOUNT_POINT"

# Convert to read-only .dmg
hdiutil convert "$DMG_TEMP" -format UDZO -o "$DMG_FINAL"

# Step 6: Sign the .dmg
echo "[INFO] Signing .dmg..."
codesign --force --timestamp --sign "$SIGNING_IDENTITY" "$DMG_FINAL"
if [ $? -ne 0 ]; then
    echo "[ERROR] .dmg signing failed."
    exit 1
fi

# Step 7: Notarize the .dmg
echo "[INFO] Zipping .dmg for notarization..."
ditto -c -k --keepParent "$DMG_FINAL" "${DMG_NAME}-dmg.zip"

echo "[INFO] Submitting .dmg for notarization..."
xcrun notarytool submit "${DMG_NAME}-dmg.zip" --apple-id "$APPLE_ID" --team-id "$TEAM_ID" --password "$APP_SPECIFIC_PASSWORD" --wait
if [ $? -ne 0 ]; then
    echo "[ERROR] .dmg notarization failed."
    exit 1
fi

echo "[INFO] Stapling notarization ticket to .dmg..."
xcrun stapler staple "$DMG_FINAL"

# Clean up
rm -rf dmg_temp "$DMG_TEMP" "${APP_NAME}.zip" "${DMG_NAME}-dmg.zip"

echo "[SUCCESS] Installer created: $DMG_FINAL"