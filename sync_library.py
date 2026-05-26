import os

# Configuration: Set this to your actual Jellyfin Movie Directory path
LIBRARY_DIR = "/path/to/your/jellyfin/movies_strm"
JELLYFIN_SERVER_URL = "http://localhost:8096"

# Example hardcoded library list. 
# Tip: You can extend this to scrape the TMDb "Trending" or "Popular" API endpoints directly.
MOVIES_TO_SCRAPE = [
    {"title": "Inception", "year": 2010, "imdb_id": "tt1375666", "tmdb_id": "27205"},
    {"title": "Interstellar", "year": 2014, "imdb_id": "tt0816692", "tmdb_id": "157336"}
]

def create_strm_library():
    if not os.path.exists(LIBRARY_DIR):
        os.makedirs(LIBRARY_DIR, exist_ok=True)
        print(f"Created library base directory: {LIBRARY_DIR}")
    
    for movie in MOVIES_TO_SCRAPE:
        # Standard clean directory formatting for Jellyfin scraping identification
        folder_name = f"{movie['title']} ({movie['year']}) [imdb-{movie['imdb_id']}]"
        movie_folder = os.path.join(LIBRARY_DIR, folder_name)
        os.makedirs(movie_folder, exist_ok=True)
        
        strm_file_path = os.path.join(movie_folder, f"{movie['title']} ({movie['year']}).strm")
        
        # Link routing right into our custom C# backend API
        strm_content = f"{JELLYFIN_SERVER_URL}/MultiSourceStream/Stream?ImdbId={movie['imdb_id']}&TmdbId={movie['tmdb_id']}\n"
        
        with open(strm_file_path, "w", encoding="utf-8") as f:
            f.write(strm_content)
            
        print(f"🚀 Generated .strm mapping layout for: {movie['title']} ({movie['year']})")

if __name__ == "__main__":
    create_strm_library()