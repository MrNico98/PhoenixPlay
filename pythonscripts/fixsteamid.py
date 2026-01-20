import os
import requests
import json
import re
import base64
from concurrent.futures import ThreadPoolExecutor, as_completed
from difflib import SequenceMatcher
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry

# ============================
# CONFIG
# ============================

GITHUB_TOKEN = os.getenv("GITHUB_TOKEN")
OWNER = "MrNico98"
REPO = "PhoenixPlay"
BRANCH = "main"
BASE_PATH = "IDapp"

FILES = [
    "steamIDFitGirl.json",
    "steamIDOnlineFix.json",
    "steamIDSteamRip.json"
]

HEADERS = {
    "Authorization": f"token {GITHUB_TOKEN}",
    "Accept": "application/vnd.github+json"
}

NOT_FOUND_FILE = r"C:\Users\utente\Downloads\not_found.json"
SIMILARITY_THRESHOLD = 0.65
MAX_WORKERS = 4

# ============================
# UTILS
# ============================

def create_session():
    session = requests.Session()
    session.headers.update({
        "User-Agent": "Mozilla/5.0",
        "Connection": "close"
    })

    retry = Retry(
        total=3,
        backoff_factor=1,
        status_forcelist=[429, 500, 502, 503, 504]
    )

    adapter = HTTPAdapter(max_retries=retry, pool_connections=1, pool_maxsize=1)
    session.mount("https://", adapter)
    return session


def similarity(a, b):
    return SequenceMatcher(None, a.lower(), b.lower()).ratio()


def clean_title(title):
    t = title.lower()

    for sep in [" – ", " - ", " | ", " — ", ",", "/"]:
        if sep in t:
            t = t.split(sep)[0]

    t = re.sub(r"\(.*?\)", "", t)
    t = re.sub(r"\[.*?\]", "", t)

    patterns = [
        r"v\d+(\.\d+)*", r"build\s*\d+", r"\+\s*\d+.*",
        r"ultimate bundle", r"complete bundle", r"double pack", r"bundle",
        r"ultimate edition", r"digital deluxe edition", r"deluxe edition",
        r"collector.?s edition", r"enhanced", r"remake",
        r"free download.*", r"all dlcs?", r"dlcs?", r"bonus.*",
        r"ryujinx", r"yuzu", r"switch", r"gog", r"uwp", r"windows store",
        r"multiplayer", r"edition"
    ]

    for p in patterns:
        t = re.sub(p, "", t)

    t = re.sub(r"[^a-z0-9:!'. ]", " ", t)
    t = re.sub(r"\s+", " ", t).strip()

    return t.title()


def build_attempts(clean):
    parts = clean.split()
    attempts = [
        clean,
        clean.replace(":", ""),
        " ".join(parts[:4]),
        " ".join(parts[:3]),
        " ".join(parts[:2]),
        parts[0]
    ]
    return list(dict.fromkeys(a for a in attempts if len(a) > 2))

# ============================
# STORES
# ============================

def find_steam_appid(session, game_title):
    base = clean_title(game_title)
    best_match = None
    best_score = 0

    for attempt in build_attempts(base):
        try:
            params = {"term": attempt, "l": "english", "cc": "US"}
            r = session.get("https://store.steampowered.com/api/storesearch/", params=params, timeout=10)
            items = r.json().get("items", [])

            for item in items[:10]:
                name = item.get("name", "")
                score = similarity(base, name)
                if score > best_score:
                    best_score = score
                    best_match = item.get("id")

        except Exception:
            pass

    if best_score >= SIMILARITY_THRESHOLD:
        return str(best_match), base

    return None, base


def find_epic_game(session, game_title):
    base = clean_title(game_title)

    try:
        r = session.get("https://store-site-backend-static.ak.epicgames.com/freeGamesPromotions", timeout=10)
        elements = r.json()["data"]["Catalog"]["searchStore"]["elements"]

        for attempt in build_attempts(base):
            for game in elements:
                title = game.get("title", "").lower()
                if attempt.lower() in title:
                    slug = game.get("productSlug") or game.get("urlSlug")
                    if slug:
                        return f"EPIC:{slug}", base
    except Exception:
        pass

    return None, base


def find_gog_game(session, game_title):
    base = clean_title(game_title)

    for attempt in build_attempts(base):
        try:
            params = {"query": attempt, "page": 1}
            r = session.get("https://www.gog.com/games/ajax/filtered", params=params, timeout=10)
            products = r.json().get("products", [])
            if products:
                return f"GOG:{products[0]['id']}", base
        except Exception:
            pass

    return None, base


def find_game_any_store(game_title):
    session = create_session()

    appid, base = find_steam_appid(session, game_title)
    if appid:
        return appid, base

    epic_id, base = find_epic_game(session, game_title)
    if epic_id:
        return epic_id, base

    gog_id, base = find_gog_game(session, game_title)
    if gog_id:
        return gog_id, base

    return None, base

# ============================
# GITHUB
# ============================

def get_github_file(path):
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    r = requests.get(url, headers=HEADERS, timeout=15)
    r.raise_for_status()
    return r.json()


def update_github_file(path, new_content, sha):
    url = f"https://api.github.com/repos/{OWNER}/{REPO}/contents/{path}"
    payload = {
        "message": "Auto-fix steam_appid null (Steam/Epic/GOG)",
        "content": base64.b64encode(new_content.encode("utf-8")).decode("utf-8"),
        "sha": sha,
        "branch": BRANCH
    }
    r = requests.put(url, headers=HEADERS, json=payload, timeout=20)
    r.raise_for_status()

# ============================
# MAIN
# ============================

def process_file(filename, not_found_log, used_ids):
    print(f"\n📄 File: {filename}")

    path = f"{BASE_PATH}/{filename}"
    file_data = get_github_file(path)
    content = base64.b64decode(file_data["content"]).decode("utf-8")
    games = json.loads(content)

    to_fix = [g for g in games if not g.get("steam_appid")]
    print(f"🔧 Da correggere: {len(to_fix)}")

    with ThreadPoolExecutor(max_workers=MAX_WORKERS) as executor:
        futures = {
            executor.submit(find_game_any_store, g["title"]): g
            for g in to_fix
        }

        for i, future in enumerate(as_completed(futures), 1):
            game = futures[future]

            try:
                appid, cleaned = future.result(timeout=40)
            except Exception as e:
                print(f"⚠️ BLOCCATO: {game['title']} → {e}")
                not_found_log.append({
                    "original": game["title"],
                    "cleaned": game["title"],
                    "file": filename,
                    "error": str(e)
                })
                continue

            if appid and appid not in used_ids:
                game["steam_appid"] = appid
                used_ids.add(appid)
                print(f"🔎 [{i}/{len(to_fix)}] {game['title']} → {appid}")
            else:
                print(f"❌ [{i}/{len(to_fix)}] {game['title']} → NON TROVATO / DUPLICATO")
                not_found_log.append({
                    "original": game["title"],
                    "cleaned": cleaned,
                    "file": filename
                })

    new_json = json.dumps(games, ensure_ascii=False, indent=2)
    update_github_file(path, new_json, file_data["sha"])
    print("✅ Caricato su GitHub")


def main():
    if not GITHUB_TOKEN or "INSERISCI" in GITHUB_TOKEN:
        print("❌ Inserisci il GitHub token")
        return

    not_found_log = []
    used_ids = set()

    for file in FILES:
        process_file(file, not_found_log, used_ids)

    if not_found_log:
        with open(NOT_FOUND_FILE, "w", encoding="utf-8") as f:
            json.dump(not_found_log, f, ensure_ascii=False, indent=2)
        print(f"\n📁 Salvati {len(not_found_log)} non trovati in {NOT_FOUND_FILE}")

    print("\n🎉 Tutti i file aggiornati.")


if __name__ == "__main__":
    main()
