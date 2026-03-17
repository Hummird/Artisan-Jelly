echo "==> Removing /bin and /obj directories..."
rm -rf bin/ obj/

echo "==> Rebuilding the project..."
dotnet publish -c Release -o bin/publish

echo "==> Moving the plugin to the Jellyfin plugins directory..."
sudo cp bin/publish/Jellyfin.Plugin.ArtisanJelly.dll /var/lib/jellyfin/plugins/ArtisanJelly/

echo "==> Restarting Jellyfin service..."
sudo systemctl restart jellyfin

echo "==> Rebuild and installation complete!"
