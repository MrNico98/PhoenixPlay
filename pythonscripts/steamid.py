import requests
import json
import re
import base64
import time
import os
from concurrent.futures import ThreadPoolExecutor, as_completed
from urllib.parse import quote
from datetime import datetime

# ----------------------------
# CONFIG
# ----------------------------

GITHUB_TOKEN = os.getenv("GITHUB_TOKEN")
OWNER = "MrNico98"
REPO = "PhoenixPlay"
BRANCH = "main"
BASE_PATH = "IDapp"

# SteamGridDB API
STEAMGRIDDB_API_KEY = os.getenv("STEAMGRIDKEY")
STEAMGRIDDB_BASE = "https://www.steamgriddb.com/api/v2"

SOURCES = {
    "SteamRip": {
        "url": "https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamrip.json",
        "output": "steamIDSteamRip.json"
    },
    "FitGirl": {
        "url": "https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/fitgirl.json",
        "output": "steamIDFitGirl.json"
    },
    "OnlineFix": {
        "url": "https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/onlinefix.json",
        "output": "steamIDOnlineFix.json"
    }
}

# ----------------------------
# UTIL
# ----------------------------

def fetch_json(url, session):
    r = session.get(url, timeout=30)
    r.raise_for_status()
    return r.json()

def clean_title(title):
    """Pulisci titolo per la ricerca"""
    if not title:
        return ""
    title = re.sub(r'\s*Free\s+Download\s*$', '', title, flags=re.IGNORECASE)
    title = re.sub(r'\s*Download\s*$', '', title, flags=re.IGNORECASE)
    return title.strip()

# ----------------------------
# STEAMGRIDDB (SOLO IMMAGINI)
# ----------------------------

def get_game_images(session, game_id):
    """Recupera TUTTE le immagini in parallelo"""
    headers = {
        "Authorization": f"Bearer {STEAMGRIDDB_API_KEY}",
        "Accept": "application/json"
    }
    
    images = {"grid": None, "hero": None, "icon": None}
    
    # Richieste in parallelo
    urls = {
        "grid": f"{STEAMGRIDDB_BASE}/grids/game/{game_id}",
        "hero": f"{STEAMGRIDDB_BASE}/heroes/game/{game_id}",
        "icon": f"{STEAMGRIDDB_BASE}/icons/game/{game_id}"
    }
    
    def fetch_image(image_type, url):
        try:
            resp = session.get(url, headers=headers, timeout=10)
            if resp.status_code == 200:
                data = resp.json()
                if data.get("success") and data.get("data") and len(data["data"]) > 0:
                    return image_type, data["data"][0].get("url")
        except:
            pass
        return image_type, None
    
    # Esegui richieste in parallelo
    with ThreadPoolExecutor(max_workers=3) as executor:
        futures = {executor.submit(fetch_image, img_type, url): img_type for img_type, url in urls.items()}
        for future in as_completed(futures):
            img_type, url = future.result()
            images[img_type] = url
    
    return images

def find_game_images(session, game_title):
    """Trova game su SteamGridDB e recupera solo le immagini"""
    headers = {
        "Authorization": f"Bearer {STEAMGRIDDB_API_KEY}",
        "Accept": "application/json"
    }
    
    search_term = clean_title(game_title)
    if len(search_term) < 3:
        return None, None, None
    
    try:
        # Search
        search_url = f"{STEAMGRIDDB_BASE}/search/autocomplete/{quote(search_term)}"
        resp = session.get(search_url, headers=headers, timeout=15)
        
        if resp.status_code != 200:
            return None, None, None
        
        data = resp.json()
        if not data.get("success") or not data.get("data"):
            return None, None, None
        
        # Trova miglior match
        best_match = data["data"][0]
        search_lower = search_term.lower()
        
        for match in data["data"]:
            match_name = match.get("name", "").lower()
            if match_name == search_lower or match_name.startswith(search_lower):
                best_match = match
                break
        
        game_id = best_match.get("id")
        
        if not game_id:
            return None, None, None
        
        # Recupera solo le immagini
        images = get_game_images(session, game_id)
        
        return images["hero"], images["grid"], images["icon"]
        
    except Exception as e:
        return None, None, None

# ----------------------------
# GITHUB API
# ----------------------------

def github_headers():
    return {
        "Authorization": f"token {GITHUB_TOKEN}",
        "Accept": "application/vnd.github+json"
    }

def get_github_file(path):
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    r = requests.get(url, headers=github_headers())
    
    if r.status_code == 200:
        data = r.json()
        content = base64.b64decode(data["content"]).decode("utf-8")
        return json.loads(content), data["sha"]
    
    return None, None

def upload_to_github(path, new_data, message):
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    
    old_data, sha = get_github_file(path)
    
    if old_data:
        existing_map = {g["title"].strip().lower(): g for g in old_data}
        new_map = {g["title"].strip().lower(): g for g in new_data}
        
        final_map = {**existing_map, **new_map}
        final_data = list(final_map.values())
        
        new_count = sum(1 for k in new_map if k not in existing_map)
        if new_count > 0:
            print(f"   ➕ Nuovi: {new_count}")
    else:
        final_data = new_data
        print("   📄 Nuovo file creato")
    
    # Statistiche immagini
    with_hero = sum(1 for g in final_data if g.get("hero_image"))
    with_grid = sum(1 for g in final_data if g.get("grid_image"))
    print(f"   🖼️ Hero: {with_hero}/{len(final_data)} ({with_hero*100//len(final_data) if final_data else 0}%)")
    print(f"   🖼️ Grid: {with_grid}/{len(final_data)} ({with_grid*100//len(final_data) if final_data else 0}%)")
    
    final_data = sorted(final_data, key=lambda x: x["title"].lower())
    
    encoded = base64.b64encode(
        json.dumps(final_data, ensure_ascii=False, indent=2).encode("utf-8")
    ).decode("utf-8")
    
    payload = {
        "message": message,
        "content": encoded,
        "branch": BRANCH
    }
    
    if sha:
        payload["sha"] = sha
    
    res = requests.put(url, headers=github_headers(), json=payload)
    res.raise_for_status()
    print(f"   ✅ Upload completato")

# ----------------------------
# MAIN VELOCE
# ----------------------------

def process_source(name, url, output_file):
    print(f"\n{'='*50}")
    print(f"📦 {name}")
    print(f"{'='*50}")
    
    session = requests.Session()
    session.headers.update({"User-Agent": "PhoenixPlay-Sync/2.0"})
    
    try:
        data = fetch_json(url, session)
    except Exception as e:
        print(f"❌ Errore: {e}")
        return
    
    games = [item["title"] for item in data.get("downloads", [])]
    print(f"🎮 {len(games)} giochi")
    
    results = []
    images_found = 0
    
    # Processa in parallelo (10 alla volta)
    with ThreadPoolExecutor(max_workers=10) as executor:
        futures = {executor.submit(find_game_images, session, title): title for title in games}
        
        for i, future in enumerate(as_completed(futures), 1):
            title = futures[future]
            hero, grid, icon = future.result()
            
            if hero or grid:
                images_found += 1
            
            # Progresso ogni 100 giochi
            if i % 100 == 0:
                print(f"   📍 Progresso: {i}/{len(games)} (immagini: {images_found})")
            
            results.append({
                "title": title,
                "hero_image": hero,
                "grid_image": grid,
                "icon_image": icon
            })
    
    print(f"\n📊 Immagini trovate: {images_found}/{len(games)} ({images_found*100//len(games) if games else 0}%)")
    
    github_path = f"{BASE_PATH}/{output_file}"
    upload_to_github(github_path, results, f"Update {output_file}")

def main():
    print("🚀 Avvio sincronizzazione immagini SteamGridDB\n")
    
    for name, info in SOURCES.items():
        process_source(name, info["url"], info["output"])
    
    print(f"\n{'='*50}")
    print("🎉 Completato!")
    print(f"{'='*50}")

if __name__ == "__main__":
    main()