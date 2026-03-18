# Artisan Jelly

![artisan-jelly-logo](https://github.com/user-attachments/assets/3f245886-2f76-42fb-8b1a-e4d7f11f6cee)

An Jellyfin toolkit designed to audit your media library, uncover missing artwork, and easily edit items' images through a powerful, integrated dashboard.

## Installation

1. Download the latest release
2. Unpack the contents of the release into your Jellyfin plugins directory.
3. Restart your Jellyfin server.

## Building from Source

1. Make sure you have the .NET SDK installed on your machine.
2. Make build.sh executable by running `chmod +x build.sh`.
3. Build the plugin using `./build.sh --install` to compile, move all the necessary files to the plugins directory, and restart the server automatically.

---

## Web UI

### Filters

The filtering section lets you locate specific media items by setting custom criteria:

- **Title Contains:** Search for a specific word or phrase in the media title.
- **Type:** Filter by media type, allowing you to view "All," "Movies," or "TV Shows."
- **Backdrops Fewer Than:** Specify a maximum number of backdrops to find items lacking sufficient background variety.
- **Missing Image Types:** Select specific missing artwork (like Logo, Banner, Thumb, Clearart, Disc, BoxRear, or Primary) to surface items that need updating.

### Hide & Ignore Capabilities

You can tailor the results view and filters by hiding specific image types.  
**Hidden Items (⚙):** Any image type you mark as hidden is entirely excluded from both the filtering options and the final results grid.  
This allows you to ignore obscure image types you don't care about (e.g., BoxRear or Disc) and keep your workflow clean.

### On-The-Go Image Editing

The interface lets you fix missing images directly without leaving the plugin page:

- **Edit Images Panel:** Once you locate an item missing an image, you can click to open the editing tools.
- **Search & Upload:** You can either search for new images online or upload a local file directly from your computer to instantly update the media item.
- **Refresh:** A refresh button (↻) allows you to manually reload the data if for some reason is wasn't reloaded automatically

---

## API Documentation

The plugin exposes several endpoints under the `/Plugins/ArtisanJelly` route.  
All endpoints (except the initial login) require an active Jellyfin API token.

### 1. Authentication

Before interacting with the plugin, you must authenticate with Jellyfin to get an Access Token.
Jellyfin strictly requires the `X-Emby-Authorization` header with specific client details to prevent `400 Bad Request` or `401 Unauthorized` errors.

**Request:**

```bash
curl -X POST "http://localhost:8096/Users/AuthenticateByName" \
  -H "Content-Type: application/json" \
  -H 'X-Emby-Authorization: MediaBrowser Client="API Script", Device="Script", DeviceId="123", Version="1.0"' \
  -d '{"Username": "admin", "Pw": "your_password"}'
```

**Response:**
Extract the `AccessToken` from the JSON response. For all subsequent requests, pass this token in the header as:
`Authorization: MediaBrowser Token="YOUR_ACCESS_TOKEN"`

### 2. Plugin Endpoints

#### Trigger Library Scan

Scans the Jellyfin library for image and metadata status.

```bash
curl -X POST "http://localhost:8096/Plugins/ArtisanJelly/Scan?forceRefresh=true" \
  -H "Authorization: MediaBrowser Token=\"YOUR_ACCESS_TOKEN\""
```

#### Fetch Library Statistics

Returns aggregated data about the scanned items (e.g., total items scanned).

```bash
curl -X GET "http://localhost:8096/Plugins/ArtisanJelly/Statistics" \
  -H "Authorization: MediaBrowser Token=\"YOUR_ACCESS_TOKEN\""
```

#### Get All Cached Results

Retrieves the complete list of scanned items from the plugin's cache.

```bash
curl -X GET "http://localhost:8096/Plugins/ArtisanJelly/Results" \
  -H "Authorization: MediaBrowser Token=\"YOUR_ACCESS_TOKEN\""
```

#### Filter Scanned Items

Allows complex querying of the scanned library based on missing images, item types, or backdrop counts. Requires a JSON payload.

**Request:**

```bash
curl -X POST "http://localhost:8096/Plugins/ArtisanJelly/Filter" \
  -H "Authorization: MediaBrowser Token=\"YOUR_ACCESS_TOKEN\"" \
  -H "Content-Type: application/json" \
  -d '{
    "criteria": {
      "itemType": "Movie",
      "missingImages": ["Logo"],
      "maxBackdrops": 0,
      "titleFilter": "Matrix"
    },
    "pageNumber": 1,
    "pageSize": 50
  }'
```

#### Get Single Item Status

Fetch detailed image and metadata status for a specific Jellyfin Item ID.

```bash
curl -X GET "http://localhost:8096/Plugins/ArtisanJelly/Item/{itemId}" \
  -H "Authorization: MediaBrowser Token=\"YOUR_ACCESS_TOKEN\""
```
