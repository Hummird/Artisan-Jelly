<h1 align="center">
  Artisan Jelly
</h1>

![artisan-jelly-logo](https://github.com/user-attachments/assets/3f245886-2f76-42fb-8b1a-e4d7f11f6cee)

An Jellyfin toolkit designed to audit your media library, uncover missing artwork, and easily edit items' images through a powerful, integrated dashboard.

## Installation

1. Add `https://hummird.online/jellyfin/manifest.json` to your jellyfin  
   `Dashboard > Plugins > Repositories`
2. Find Artisan Jelly in your availible plugins and install it.
3. Restart your Jellyfin server.

OR

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
- **Type:** Filter by media type, allowing you to view specific categories like Movies, TV Shows, Episodes, Box Sets, etc.
- **Backdrops Fewer Than:** Specify a maximum number of backdrops to find items lacking sufficient background variety.
- **Missing Image Types:** Select specific missing artwork (like Logo, Banner, Thumb, Clearart, Disc, BoxRear, or Primary) to surface items that need updating.

### Hide & Ignore Capabilities

You can tailor the results view and filters by hiding specific image types.  
**Hidden Items (⚙):** Any image or item type you mark as hidden is entirely excluded from both the filtering options and the final results grid.  
This allows you to ignore obscure image types (e.g., BoxRear or Disc) or item type (e.g., Episode or BoxSet) you don't care about and keep your workflow clean.

### Search Results

The results list provides a visual overview of all items matching your filter criteria.
Each item displays its title and the types of images that are missing.
By clicking on an item's thumbnail or name you can directly to the items page,
and by clicking on an "Edit Images" button on the right you can access a detailed view showing all the images associated with that media item.

The interface lets you fix missing images directly without leaving the plugin page:

- **Edit Images Panel:** Once you locate an item missing an image, you can click to open the editing tools.
- **Search:** You can search for new images online through your already connected metadata providers (like TMDB, TVDB, etc.) and through Openverse, which sources from multiple providers including Wikimedia Commons and Flickr.
  > [!NOTE]
  > Results from Openverse may not always be perfectly accurate or relevant, so it's recommended to review the search results carefully before applying any images to your media items.  
  > Artisan Jelly prioritizes your existing metadata providers for image searching, but Openverse can be a helpful fallback option when those providers don't have the images you need.
- **Upload:** You can upload a local file directly from your computer to instantly update the media item.
- **Refresh:** A refresh button (↻) allows you to manually reload the data if for some reason is wasn't reloaded automatically

---

## API Reference

All endpoints are prefixed with `/Plugins/ArtisanJelly` and require a Jellyfin token header:
`Authorization: MediaBrowser Token="YOUR_ACCESS_TOKEN"`

### Authenticate

```http
POST /Users/AuthenticateByName
```

| Header                 | Value                                                                          |
| :--------------------- | :----------------------------------------------------------------------------- |
| `X-Emby-Authorization` | `MediaBrowser Client="Script", Device="Script", DeviceId="123", Version="1.0"` |
| `Content-Type`         | `application/json`                                                             |

```json
{ "Username": "admin", "Pw": "your_password" }
```

Extract `AccessToken` from the response and use it in all subsequent requests.

### Trigger Library Scan

```http
POST /Plugins/ArtisanJelly/Scan
```

| Parameter      | Type   | Description                                            |
| :------------- | :----- | :----------------------------------------------------- |
| `forceRefresh` | `bool` | If `true`, discards cache and rescans the full library |

### Get Library Statistics

```http
GET /Plugins/ArtisanJelly/Statistics
```

Returns aggregated counts — total items scanned, missing image counts per type, available item types, and backdrop averages. No parameters.

### Get Cached Results

```http
GET /Plugins/ArtisanJelly/Results
```

Returns the raw full list from the last scan without any filtering or pagination. No parameters.

### Filter Items

```http
POST /Plugins/ArtisanJelly/Filter
```

| Body field                  | Type       | Description                                                        |
| :-------------------------- | :--------- | :----------------------------------------------------------------- |
| `criteria.titleFilter`      | `string`   | Case-insensitive substring match on item name                      |
| `criteria.itemType`         | `string`   | `"All"`, `"Movie"`, `"Series"`, `"Episode"`, etc.                  |
| `criteria.missingImages`    | `string[]` | Only return items missing these image types e.g. `["Logo","Disc"]` |
| `criteria.maxBackdrops`     | `int`      | Return items with a backdrop count at or below this value          |
| `criteria.ignoredItemTypes` | `string[]` | Exclude these item types when `itemType` is `"All"`                |
| `pageNumber`                | `int`      | **Required**. 1-based page index                                   |
| `pageSize`                  | `int`      | **Required**. Items per page (recommended: `50`)                   |

```json
{
  "criteria": {
    "itemType": "All",
    "missingImages": ["Logo"],
    "maxBackdrops": 0,
    "titleFilter": "Matrix",
    "ignoredItemTypes": ["Episode", "Season"]
  },
  "pageNumber": 1,
  "pageSize": 50
}
```

Response includes `TotalCount` (for pagination), `PageNumber`, `PageSize`, and `Items`.

### Get Single Item Status

```http
GET /Plugins/ArtisanJelly/Item/{itemId}
```

| Parameter | Type     | Description                          |
| :-------- | :------- | :----------------------------------- |
| `itemId`  | `string` | **Required**. The Jellyfin item GUID |
