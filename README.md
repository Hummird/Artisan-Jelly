# Artisan Jelly

A Jellyfin plugin designed for advanced image scanning, library analysis, and metadata management.

Artisan Jelly exposes a fully functional REST API, allowing you to trigger library scans, filter missing images (like logos or backdrops), and fetch detailed statistics directly from the command line or external scripts.

## Installation

1. Build the plugin using the .NET 8 SDK: `dotnet publish -c Release`
2. Copy the resulting `.dll` files to your Jellyfin `plugins/ArtisanJelly` directory.
3. Restart your Jellyfin server.

---

## API Documentation

The plugin exposes several endpoints under the `/Plugins/ArtisanJelly` route. All endpoints (except the initial login) require an active Jellyfin API token.

### 1. Authentication

Before interacting with the plugin, you must authenticate with Jellyfin to get an Access Token. Jellyfin strictly requires the `X-Emby-Authorization` header with specific client details to prevent `400 Bad Request` or `401 Unauthorized` errors.

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

---

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
