import requests
import json
import re
import base64
import os
from concurrent.futures import ThreadPoolExecutor, as_completed

# ----------------------------
# CONFIG
# ----------------------------

GITHUB_TOKEN = os.getenv("GITHUB_TOKEN")
OWNER = "MrNico98"
REPO = "PhoenixPlay"
BRANCH = "main"
BASE_PATH = "IDapp"

# SteamGridDB API
STEAMGRIDDB_API_KEY = "6104c407ab88f159ec34420579c6a21e"
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
    """Recupera JSON da URL"""
    r = session.get(url, timeout=30)
    r.raise_for_status()
    return r.json()

def clean_title(title):
    """Pulisce il titolo del gioco"""
    title = re.sub(r"Free Download.*", "", title, flags=re.IGNORECASE)
    title = re.sub(r"\(v.*?\)", "", title, flags=re.IGNORECASE)
    title = re.sub(r"\[.*?\]", "", title)
    title = re.sub(r"Download$", "", title, flags=re.IGNORECASE)
    title = re.sub(r"^\d+\.", "", title)
    return title.strip()

# ----------------------------
# STEAM SEARCH CON STEAMGRIDDB
# ----------------------------

def find_steam_appid_via_sgdb(session, game_title):
    """
    Trova lo Steam App ID usando SteamGridDB API
    
    Args:
        session: requests.Session
        game_title: titolo del gioco da cercare
    
    Returns:
        str: Steam App ID o None se non trovato
    """
    headers = {
        "Authorization": f"Bearer {STEAMGRIDDB_API_KEY}",
        "Accept": "application/json",
        "User-Agent": "PhoenixPlay-Sync/1.0"
    }
    
    # Pulisci il titolo per migliorare la ricerca
    clean_title_str = clean_title(game_title)
    
    try:
        # Step 1: Cerca il gioco per nome
        search_url = f"{STEAMGRIDDB_BASE}/search/autocomplete/{requests.utils.quote(clean_title_str)}"
        resp = session.get(search_url, headers=headers, timeout=15)
        
        if resp.status_code == 429:
            print("  ⚠️ Rate limit API, aspetto...")
            return None
        elif resp.status_code != 200:
            return None
        
        data = resp.json()
        if not data.get("success") or not data.get("data"):
            return None
        
        # Prendi il miglior match (primo risultato)
        best_match = data["data"][0]
        game_id = best_match["id"]
        
        # Step 2: Ottieni i dettagli del gioco con informazioni sulle piattaforme
        details_url = f"{STEAMGRIDDB_BASE}/game/id/{game_id}?platformdata=steam"
        details_resp = session.get(details_url, headers=headers, timeout=15)
        
        if details_resp.status_code != 200:
            return None
        
        details = details_resp.json()
        
        if not details.get("success") or not details.get("data"):
            return None
        
        game_data = details["data"]
        
        # Step 3: Cerca l'ID Steam nei dati della piattaforma
        if "platforms" in game_data and game_data["platforms"]:
            platforms = game_data["platforms"]
            if "steam" in platforms and platforms["steam"]:
                steam_id = platforms["steam"].get("id")
                if steam_id:
                    return str(steam_id)
        
        # Step 4: Metodo alternativo - cerca nell'URL o in altri campi
        if "steam_app_id" in game_data:
            return str(game_data["steam_app_id"])
        
        # Step 5: Prova con ricerca diretta per Steam ID
        steam_url = f"{STEAMGRIDDB_BASE}/game/steam/{requests.utils.quote(clean_title_str)}"
        steam_resp = session.get(steam_url, headers=headers, timeout=15)
        
        if steam_resp.status_code == 200:
            steam_data = steam_resp.json()
            if steam_data.get("success") and steam_data.get("data"):
                return str(steam_data["data"]["id"])
        
        return None
        
    except Exception as e:
        # Silenzioso per non intasare il log
        return None

def find_steam_appid_fallback(session, game_title):
    """Fallback: ricerca HTML Steam (metodo originale)"""
    query = clean_title(game_title)
    url = f"https://store.steampowered.com/search/results/?term={requests.utils.quote(query)}&category1=998"
    
    try:
        html = session.get(url, timeout=20, headers={"User-Agent": "Mozilla/5.0"}).text
        match = re.search(r"/app/(\d+)/", html)
        if match:
            return match.group(1)
    except:
        pass
    
    return None

def find_steam_appid(session, game_title):
    """
    Wrapper: prova prima SteamGridDB, poi fallback su Steam
    """
    # Prova con SteamGridDB
    appid = find_steam_appid_via_sgdb(session, game_title)
    
    # Se fallisce, usa il metodo originale come fallback
    if not appid:
        appid = find_steam_appid_fallback(session, game_title)
    
    return appid

# ----------------------------
# GITHUB API
# ----------------------------

def github_headers():
    """Headers per GitHub API"""
    return {
        "Authorization": f"token {GITHUB_TOKEN}",
        "Accept": "application/vnd.github+json"
    }

def get_github_file(path):
    """Recupera file da GitHub"""
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    r = requests.get(url, headers=github_headers())
    
    if r.status_code == 200:
        data = r.json()
        content = base64.b64decode(data["content"]).decode("utf-8")
        return json.loads(content), data["sha"]
    
    return None, None

def upload_to_github(path, new_data, message):
    """Carica o aggiorna file su GitHub"""
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    
    old_data, sha = get_github_file(path)
    
    if old_data:
        existing_titles = {g["title"].strip().lower() for g in old_data}
        existing_appids = {str(g["steam_appid"]) for g in old_data if g["steam_appid"]}
        
        filtered = [
            g for g in new_data
            if g["title"].strip().lower() not in existing_titles
            and (not g["steam_appid"] or str(g["steam_appid"]) not in existing_appids)
        ]
        
        if not filtered:
            print("ℹ️ Nessun nuovo gioco da aggiungere.")
            return
        
        final_data = old_data + filtered
        print(f"➕ Aggiunti {len(filtered)} nuovi giochi su {len(old_data)} esistenti.")
        
        # Statistiche successo ricerca
        found = sum(1 for g in filtered if g["steam_appid"])
        print(f"📊 Di cui {found} con ID Steam trovato ({found*100//len(filtered) if filtered else 0}%)")
        
    else:
        final_data = new_data
        print("📄 File non esistente → creato nuovo.")
        
        # Statistiche
        found = sum(1 for g in final_data if g["steam_appid"])
        print(f"📊 {found} ID trovati su {len(final_data)} giochi ({found*100//len(final_data) if final_data else 0}%)")
    
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
    print(f"✅ Upload completato: {path}")

# ----------------------------
# MAIN PROCESS
# ----------------------------

def process_source(name, url, output_file):
    """Processa una singola fonte di giochi"""
    print(f"\n{'='*60}")
    print(f"📦 Fonte: {name}")
    print(f"{'='*60}")
    
    session = requests.Session()
    session.headers.update({"User-Agent": "PhoenixPlay-Sync/1.0"})
    
    try:
        data = fetch_json(url, session)
    except Exception as e:
        print(f"❌ Errore nel fetch della fonte {name}: {e}")
        return
    
    games = [item["title"] for item in data.get("downloads", [])]
    print(f"🎮 Trovati {len(games)} giochi da processare")
    
    results = []
    found_count = 0
    
    with ThreadPoolExecutor(max_workers=5) as executor:  # 5 workers per evitare rate limit
        futures = {
            executor.submit(find_steam_appid, session, title): title
            for title in games
        }
        
        for i, future in enumerate(as_completed(futures), 1):
            title = futures[future]
            appid = future.result()
            
            if appid:
                found_count += 1
                print(f"✅ [{i:3d}/{len(games)}] {title[:45]:45s} → {appid}")
            else:
                print(f"❌ [{i:3d}/{len(games)}] {title[:45]:45s} → Non trovato")
            
            results.append({
                "title": title,
                "steam_appid": appid
            })
    
    # Statistiche finali
    success_rate = (found_count * 100 // len(games)) if games else 0
    print(f"\n📊 Statistiche {name}: {found_count}/{len(games)} ID trovati ({success_rate}%)")
    
    # Upload su GitHub
    github_path = f"{BASE_PATH}/{output_file}"
    try:
        upload_to_github(github_path, results, f"Update {output_file} - {found_count}/{len(games)} IDs found")
    except Exception as e:
        print(f"❌ Errore upload su GitHub: {e}")

def main():
    """Funzione principale"""
    print("🚀 Avvio sincronizzazione ID Steam\n")
    
    if not GITHUB_TOKEN or "METTI_QUI" in GITHUB_TOKEN or GITHUB_TOKEN == "":
        print("❌ ERRORE: Devi impostare un GITHUB_TOKEN valido come variabile d'ambiente")
        print("   Esportalo con: export GITHUB_TOKEN='il_tuo_token'")
        return
    
    if not STEAMGRIDDB_API_KEY or STEAMGRIDDB_API_KEY == "6104c407ab88f159ec34420579c6a21e":
        print("⚠️ ATTENZIONE: Usando API key SteamGridDB di default")
        print("   Per migliori risultati, genera la tua chiave su https://www.steamgriddb.com/preferences")
    
    print(f"📁 Repository: {OWNER}/{REPO}")
    print(f"🌿 Branch: {BRANCH}")
    print(f"📂 Path base: {BASE_PATH}")
    print()
    
    for name, info in SOURCES.items():
        process_source(name, info["url"], info["output"])
    
    print(f"\n{'='*60}")
    print("🎉 Tutte le fonti sono state sincronizzate con successo!")
    print(f"{'='*60}")

if __name__ == "__main__":
    main()
