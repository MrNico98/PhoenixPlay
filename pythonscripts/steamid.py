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
STEAMGRIDDB_API_KEY = os.getenv("STEAMGRIDDB_API_KEY")
STEAMGRIDDB_BASE = "https://www.steamgriddb.com/api/v2"

SOURCES = {
    "SteamRip": {
        "url": "https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamrip.json",
        "output": "steamIDSteamRip.json",
        "force_recreate": False  # Merge normale
    },
    "FitGirl": {
        "url": "https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/fitgirl.json",
        "output": "steamIDFitGirl.json",
        "force_recreate": True   # Sempre ricreato da zero
    },
    "OnlineFix": {
        "url": "https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/onlinefix.json",
        "output": "steamIDOnlineFix.json",
        "force_recreate": False  # Merge normale
    },
    "AIMODS": {
        "url": "https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/aimods.json",
        "output": "steamIDAIMODS.json",
        "force_recreate": False  # Merge normale
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

def clean_title_for_search(title, source_name):
    """Pulisce il titolo per la ricerca in base alla source"""
    if not title:
        return ""
    
    title = clean_title(title)
    
    # Pulizia specifica per OnlineFix
    if source_name == "OnlineFix":
        title = re.sub(r'\s+Build\s+[\d]+', '', title, flags=re.IGNORECASE)
        title = re.sub(r'\s+v?\d+(?:\.\d+)+$', '', title)
        title = title.strip()
    
    # Pulizia specifica per FitGirl
    elif source_name == "FitGirl":
        if " – " in title:
            title = title.split(" – ")[0].strip()
        elif " - " in title:
            title = title.split(" - ")[0].strip()
    
    return title

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
    
    with ThreadPoolExecutor(max_workers=3) as executor:
        futures = {executor.submit(fetch_image, img_type, url): img_type for img_type, url in urls.items()}
        for future in as_completed(futures):
            img_type, url = future.result()
            images[img_type] = url
    
    return images

def find_game_images(session, game_title, source_name=""):
    """Trova game su SteamGridDB e recupera solo le immagini"""
    headers = {
        "Authorization": f"Bearer {STEAMGRIDDB_API_KEY}",
        "Accept": "application/json"
    }
    
    search_term = clean_title_for_search(game_title, source_name)
    if len(search_term) < 3:
        return None, None, None
    
    try:
        search_url = f"{STEAMGRIDDB_BASE}/search/autocomplete/{quote(search_term)}"
        resp = session.get(search_url, headers=headers, timeout=15)
        
        if resp.status_code != 200:
            return None, None, None
        
        data = resp.json()
        if not data.get("success") or not data.get("data"):
            return None, None, None
        
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
    """Legge file da GitHub, ritorna (data, sha) o (None, None) se errore"""
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    r = requests.get(url, headers=github_headers())
    
    if r.status_code == 200:
        data = r.json()
        content = base64.b64decode(data["content"]).decode("utf-8")
        
        # Gestisci file vuoto
        if not content or content.strip() == "":
            return None, None
        
        try:
            return json.loads(content), data["sha"]
        except json.JSONDecodeError:
            print(f"   ⚠️ JSON corrotto in {path}, verrà ricreato")
            return None, None
    
    return None, None

def upload_to_github_merge(path, new_data, message):
    """Upload con merge (per SteamRip e OnlineFix)"""
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    
    old_data, sha = get_github_file(path)
    
    if old_data and isinstance(old_data, list):
        existing_map = {g["title"].strip().lower(): g for g in old_data}
        new_map = {g["title"].strip().lower(): g for g in new_data}
        
        final_map = {**existing_map, **new_map}
        final_data = list(final_map.values())
        
        new_count = sum(1 for k in new_map if k not in existing_map)
        if new_count > 0:
            print(f"   ➕ Nuovi: {new_count}")
        else:
            print(f"   🔄 Nessun nuovo gioco")
    else:
        final_data = new_data
        print("   📄 Nuovo file creato")
    
    # Statistiche immagini
    if final_data:
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

def upload_to_github_force(path, new_data, message):
    """Upload forzato per file grandi (>1MB) usando git blob API"""
    
    final_data = sorted(new_data, key=lambda x: x["title"].lower())
    
    # Statistiche
    if final_data:
        with_hero = sum(1 for g in final_data if g.get("hero_image"))
        with_grid = sum(1 for g in final_data if g.get("grid_image"))
        print(f"   🖼️ Hero: {with_hero}/{len(final_data)} ({with_hero*100//len(final_data) if final_data else 0}%)")
        print(f"   🖼️ Grid: {with_grid}/{len(final_data)} ({with_grid*100//len(final_data) if final_data else 0}%)")
    
    # Converti a JSON
    json_content = json.dumps(final_data, ensure_ascii=False, indent=2)
    json_bytes = json_content.encode("utf-8")
    
    print(f"   📦 Dimensione JSON: {len(json_bytes) / 1024 / 1024:.2f} MB")
    
    # 1. Crea un blob su GitHub
    blob_url = f"https://api.github.com/repos/{OWNER}/{REPO}/git/blobs"
    blob_payload = {
        "content": base64.b64encode(json_bytes).decode("utf-8"),
        "encoding": "base64"
    }
    
    blob_resp = requests.post(blob_url, headers=github_headers(), json=blob_payload)
    blob_resp.raise_for_status()
    blob_sha = blob_resp.json()["sha"]
    print(f"   📦 Blob creato: {blob_sha[:8]}...")
    
    # 2. Ottieni l'albero corrente del branch
    ref_url = f"https://api.github.com/repos/{OWNER}/{REPO}/git/refs/heads/{BRANCH}"
    ref_resp = requests.get(ref_url, headers=github_headers())
    ref_resp.raise_for_status()
    current_commit_sha = ref_resp.json()["object"]["sha"]
    
    # 3. Ottieni l'albero del commit corrente
    commit_url = f"https://api.github.com/repos/{OWNER}/{REPO}/git/commits/{current_commit_sha}"
    commit_resp = requests.get(commit_url, headers=github_headers())
    commit_resp.raise_for_status()
    current_tree_sha = commit_resp.json()["tree"]["sha"]
    
    # 4. Crea un nuovo albero con il file aggiornato
    tree_url = f"https://api.github.com/repos/{OWNER}/{REPO}/git/trees"
    tree_payload = {
        "base_tree": current_tree_sha,
        "tree": [
            {
                "path": path,
                "mode": "100644",
                "type": "blob",
                "sha": blob_sha
            }
        ]
    }
    
    tree_resp = requests.post(tree_url, headers=github_headers(), json=tree_payload)
    tree_resp.raise_for_status()
    new_tree_sha = tree_resp.json()["sha"]
    
    # 5. Crea un nuovo commit
    new_commit_url = f"https://api.github.com/repos/{OWNER}/{REPO}/git/commits"
    new_commit_payload = {
        "message": f"FORCE RECREATE: {message}",
        "tree": new_tree_sha,
        "parents": [current_commit_sha]
    }
    
    new_commit_resp = requests.post(new_commit_url, headers=github_headers(), json=new_commit_payload)
    new_commit_resp.raise_for_status()
    new_commit_sha = new_commit_resp.json()["sha"]
    
    # 6. Aggiorna il branch
    update_ref_payload = {
        "sha": new_commit_sha,
        "force": True
    }
    
    update_resp = requests.patch(ref_url, headers=github_headers(), json=update_ref_payload)
    update_resp.raise_for_status()
    
    print(f"   ✅ Upload forzato completato ({len(final_data)} giochi, {len(json_bytes)/1024/1024:.2f} MB)")

# ----------------------------
# MAIN VELOCE
# ----------------------------

def process_source(name, url, output_file, force_recreate=False):
    print(f"\n{'='*50}")
    print(f"📦 {name} {'(FORCE RECREATE)' if force_recreate else '(MERGE MODE)'}")
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
        futures = {executor.submit(find_game_images, session, title, name): title for title in games}
        
        for i, future in enumerate(as_completed(futures), 1):
            title = futures[future]
            hero, grid, icon = future.result()
            
            if hero or grid:
                images_found += 1
            
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
    
    if force_recreate:
        upload_to_github_force(github_path, results, f"Force recreate {output_file}")
    else:
        upload_to_github_merge(github_path, results, f"Update {output_file}")

def main():
    print("🚀 Avvio sincronizzazione immagini SteamGridDB\n")
    
    for name, info in SOURCES.items():
        process_source(
            name, 
            info["url"], 
            info["output"], 
            info.get("force_recreate", False)
        )
    
    print(f"\n{'='*50}")
    print("🎉 Completato!")
    print(f"{'='*50}")

if __name__ == "__main__":
    main()
