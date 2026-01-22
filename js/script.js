// Global variables
let allGamesData = {
    steamrip: [],
    onlinefix: [],
    fitgirl: [],
    altro: [] // AGGIUNTA: Nuova categoria Altro Provider
};

let steamData = {
    steamrip: [],
    onlinefix: [],
    fitgirl: [],
    altro: [] // AGGIUNTA: Nuova categoria Altro Provider
};

let categories = new Set();
let currentSource = "steamrip";
let currentFilter = "all";
let currentSort = "date-desc";
let currentView = "grid";
let maxFileSize = 0;

// Pagination variables
const GAMES_PER_PAGE = 30;
let currentPage = 1;
let totalPages = 1;
let filteredGames = [];

// DOM elements
const gamesContainer = document.getElementById('games-container');
const categoryList = document.getElementById('category-list');
const searchBox = document.getElementById('search-box');
const sortButtons = document.querySelectorAll('.sort-btn');
const sizeSlider = document.getElementById('size-slider');
const sizeValue = document.getElementById('size-value');
const totalGamesEl = document.getElementById('total-games');
const totalSizeEl = document.getElementById('total-size');
const gamesCountEl = document.getElementById('games-count');
const viewGridBtn = document.getElementById('view-grid');
const viewListBtn = document.getElementById('view-list');
const gameDetailsModal = document.getElementById('game-details-modal');
const closeModalBtn = document.getElementById('close-modal');
const modalTitleText = document.getElementById('modal-title-text');
const modalSourceBadge = document.getElementById('modal-source-badge');
const modalImage = document.getElementById('modal-image');
const gameProperties = document.getElementById('game-properties');
const downloadLinks = document.getElementById('download-links');
const downloadInstructions = document.getElementById('download-instructions');
const sourceIndicator = document.getElementById('source-indicator');
const pagination = document.getElementById('pagination');
const firstPageBtn = document.getElementById('first-page');
const prevPageBtn = document.getElementById('prev-page');
const nextPageBtn = document.getElementById('next-page');
const lastPageBtn = document.getElementById('last-page');
const currentPageEl = document.getElementById('current-page');
const totalPagesEl = document.getElementById('total-pages');
const pageNumbersEl = document.getElementById('page-numbers');

// FAQ elements
const faqSection = document.getElementById('faq-section');
const faqToggleBtn = document.getElementById('faq-toggle');
const closeFaqBtn = document.getElementById('close-faq');

// Source buttons
const sourceSteamripBtn = document.getElementById('source-steamrip');
const sourceOnlinefixBtn = document.getElementById('source-onlinefix');
const sourceFitgirlBtn = document.getElementById('source-fitgirl');
const sourceAltroBtn = document.getElementById('source-altro'); // AGGIUNTA: Pulsante Altro Provider

// Site priority order
const SITE_PRIORITY = {
    'buzzheavier.com': { name: 'Buzzheavier', class: 'buzzheavier', icon: 'fas fa-bolt', priority: 1 },
    'vikingfile.com': { name: 'Vikingfile', class: 'vikingfile', icon: 'fas fa-shield-alt', priority: 2 },
    'gofile.io': { name: 'Gofile', class: 'gofile', icon: 'fas fa-file-archive', priority: 3 },
    '1drv.ms': { name: 'OneDrive', class: 'onedrive', icon: 'fas fa-cloud', priority: 1 },
    'onedrive.live.com': { name: 'OneDrive', class: 'onedrive', icon: 'fas fa-cloud', priority: 1 },
    'mega.nz': { name: 'MEGA', class: 'mega', icon: 'fas fa-database', priority: 2 },
    'magnet': { name: 'Torrent Magnet', class: 'magnet', icon: 'fas fa-magnet', priority: 1 }
};

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    loadAllGamesData();
    setupEventListeners();
});

// Load all games data from JSON sources
async function loadAllGamesData() {
    try {
        // Carica i dati Steam per tutte le fonti
        const [steamripSteamData, onlinefixSteamData, fitgirlSteamData, altroSteamData] = await Promise.all([
            loadSteamData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamIDSteamRip.json'),
            loadSteamData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamIDOnlineFix.json'),
            loadSteamData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamIDFitGirl.json'),
            loadSteamData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamIDAltro.json') // AGGIUNTA: Dati Steam per Altro Provider
        ]);
        
        steamData.steamrip = steamripSteamData;
        steamData.onlinefix = onlinefixSteamData;
        steamData.fitgirl = fitgirlSteamData;
        steamData.altro = altroSteamData; // AGGIUNTA
        
        // Carica tutti i sorgenti in parallelo
        const [steamripData, onlinefixData, fitgirlData, altroData] = await Promise.all([
            loadSourceData('https://hydralinks.pages.dev/sources/steamrip.json', 'steamrip'),
            loadSourceData('https://hydralinks.pages.dev/sources/onlinefix.json', 'onlinefix'),
            loadSourceData('https://hydralinks.pages.dev/sources/fitgirl.json', 'fitgirl'),
            loadSourceData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/Altro/AltriGiochi.json', 'altro') // AGGIUNTA: Dati Altro Provider
        ]);
        
        // Rimuovi duplicati all'interno di ciascuna fonte
        allGamesData.steamrip = removeDuplicatesAndKeepLatest(steamripData, 'steamrip');
        allGamesData.onlinefix = removeDuplicatesAndKeepLatest(onlinefixData, 'onlinefix');
        allGamesData.fitgirl = removeDuplicatesAndKeepLatest(fitgirlData, 'fitgirl');
        allGamesData.altro = removeDuplicatesAndKeepLatest(altroData, 'altro'); // AGGIUNTA
                
        // Processa i giochi per la fonte corrente
        processGamesData();
        updateStats();
        renderGames();
        setupCategories();
        updateSourceCounts();
        
    } catch (error) {
        console.error('Errore nel caricamento dei dati:', error);
        showErrorMessage('Errore nel caricamento dei dati. Riprova più tardi.');
    }
}

// Load Steam data for a specific source
async function loadSteamData(url) {
    try {
        console.log(`Caricamento Steam data da ${url}...`);
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const json = await response.json();
        return json || [];
    } catch (error) {
        console.error(`Errore nel caricamento Steam data:`, error);
        return [];
    }
}

// Remove duplicate games and keep only the latest version
function removeDuplicatesAndKeepLatest(games, source) {
    if (!games || !Array.isArray(games)) return [];
    
    const gameMap = new Map();
    
    games.forEach(game => {
        // Estrai il nome base del gioco (per matching cross-fonte)
        const baseName = extractBaseGameName(game.title || game.name || '');
        const uploadDate = new Date(game.uploadDate || game.date || '1970-01-01');
        
        // Crea un ID unico per questo gioco (baseName + source)
        const gameId = `${source}:${baseName}`;
        
        // Se è un gioco di FitGirl, gestisci diversamente perché ha titoli diversi
        let existingGame = gameMap.get(gameId);
        
        // Verifica se questo gioco è più recente di quello esistente
        if (!existingGame || uploadDate > existingGame.uploadDate) {
            gameMap.set(gameId, {
                ...game,
                baseName: baseName,
                uploadDate: uploadDate,
                source: source
            });
        }
    });
    
    // Converti la mappa in array
    return Array.from(gameMap.values());
}

function filterCrossSourceDuplicates(games) {
    if (!games || games.length === 0) return [];
    
    const gameMap = new Map();
    
    games.forEach(game => {
        const baseName = game.baseName || extractBaseGameName(game.title);
        const uploadDate = new Date(game.uploadDate || game.date || '1970-01-01');
        
        // Crea un ID basato solo sul nome base (senza fonte)
        // Questo ci permette di identificare lo stesso gioco tra fonti diverse
        const gameId = baseName;
        
        const existingGame = gameMap.get(gameId);
        
        if (!existingGame || uploadDate > existingGame.uploadDate) {
            gameMap.set(gameId, {
                ...game,
                baseName: baseName,
                uploadDate: uploadDate
            });
        }
    });
    
    return Array.from(gameMap.values());
}

// Extract base game name by removing version info
function extractBaseGameName(title) {
    if (!title) return '';
    
    // Converte in minuscolo per il confronto
    let name = title.toLowerCase();
    
    // Rimuovi i prefissi comuni di FitGirl
    name = name
        .replace(/fitgirl repack/gi, '')
        .replace(/repack/gi, '')
        .replace(/complete bundle/gi, '')
        .replace(/ultimate edition/gi, '')
        .replace(/deluxe edition/gi, '');
    
    // Rimuovi versioni e numeri di build
    // Pattern: v1.2.3, v1.0, build 123, ecc.
    name = name
        .replace(/\s*(v\d+\.\d+(\.\d+)*(\.\d+)*)/gi, '')
        .replace(/\s*(\d+\.\d+(\.\d+)*(\.\d+)*)/gi, '')
        .replace(/\s+build\s+\d+/gi, '')
        .replace(/\s+patch\s+\d+\.\d+/gi, '');
    
    // Rimuovi DLCs e informazioni aggiuntive
    name = name
        .replace(/\s*\(\s*multi\d*\s*\)/gi, '')
        .replace(/\s*\+\s*\d+\s*dlcs?/gi, '')
        .replace(/\s*\+\s*bonuses?/gi, '')
        .replace(/\s*-\s*.*?(repack|edition|bundle)/gi, '');
    
    // Rimuovi parentesi e parentesi quadre
    name = name
        .replace(/\s*\[.*?\]/g, '')
        .replace(/\s*\(.*?\)/g, '');
    
    // Rimuovi trattini e spazi extra
    name = name
        .replace(/[:–—\-]/g, ' ')
        .replace(/\s+/g, ' ')
        .trim();
    
    // Rimuovi parole comuni che non sono parte del titolo principale
    const commonWords = [
        'the ', 'a ', 'an ', 'and ', 'or ', 'but ', 'in ', 'on ', 'at ', 'to ', 'for ',
        'edition', 'version', 'update', 'free', 'download', 'full', 'game', 'pc', 'windows'
    ];
    
    commonWords.forEach(word => {
        const regex = new RegExp(`\\b${word}\\b`, 'gi');
        name = name.replace(regex, '');
    });
    
    // Tronca a parole significative (massimo 5 parole)
    const words = name.split(/\s+/).filter(w => w.length > 0);
    if (words.length > 5) {
        name = words.slice(0, 5).join(' ');
    }
    
    return name || title.toLowerCase(); // Fallback al titolo originale
}

// Load data from a single source
async function loadSourceData(url, source) {
    try {
        console.log(`Caricamento dati da ${url} (${source})...`);
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const json = await response.json();
        
        // Different sources have different data structures
        if (source === 'steamrip' || source === 'onlinefix' || source === 'fitgirl') {
            return json.downloads || [];
        } else if (source === 'altro') {
            // AGGIUNTA: Gestione struttura dati per Altro Provider
            // Il JSON di Altro Provider ha la struttura: {"name": "AIMODS", "downloads": [...]}
            return json.downloads || [];
        }
        return [];
    } catch (error) {
        console.error(`Errore nel caricamento di ${source}:`, error);
        return [];
    }
}

// Process games data for current source
function processGamesData() {
    const games = allGamesData[currentSource];
    categories.clear();
    maxFileSize = 0;
    
    games.forEach(game => {
        // Extract file size
        game.numericSize = extractFileSize(game);
        
        // Update max file size
        if (game.numericSize && game.numericSize > maxFileSize) {
            maxFileSize = Math.ceil(game.numericSize);
        }
        
        // Assign category based on source
        assignCategory(game);
        
        // Add to categories set
        if (game.category) {
            categories.add(game.category);
        }
        
        // Add source identifier
        game.source = currentSource;
        
        // Ensure game has all required properties
        if (!game.title && game.name) {
            game.title = game.name;
        }
        if (!game.uris && game.links) {
            game.uris = game.links;
        }
        
        // Store base name for filtering
        game.baseName = extractBaseGameName(game.title);
    });
    
    // Update size slider max value
    sizeSlider.max = maxFileSize;
    sizeSlider.value = maxFileSize;
    updateSizeValue(maxFileSize);
}

// Extract file size from game object
function extractFileSize(game) {
    if (game.fileSize) {
        const sizeMatch = game.fileSize.match(/([\d.]+)\s*(MB|GB)/i);
        if (sizeMatch) {
            const size = parseFloat(sizeMatch[1]);
            const unit = sizeMatch[2].toUpperCase();
            return unit === 'GB' ? size : size / 1024;
        }
    } else if (game.size) {
        const sizeMatch = game.size.match(/([\d.]+)\s*(MB|GB)/i);
        if (sizeMatch) {
            const size = parseFloat(sizeMatch[1]);
            const unit = sizeMatch[2].toUpperCase();
            return unit === 'GB' ? size : size / 1024;
        }
    }
    return 0;
}

// Get formatted file size for display
function getFormattedFileSize(game) {
    if (game.fileSize) {
        return game.fileSize;
    } else if (game.size) {
        return game.size;
    }
    return 'N/A';
}

// Get upload date for display
function getUploadDate(game) {
    if (game.uploadDate) {
        return game.uploadDate;
    } else if (game.date) {
        return game.date;
    }
    return new Date().toISOString();
}

// Get game title
function getGameTitle(game) {
    if (game.title) {
        return game.title;
    } else if (game.name) {
        return game.name;
    }
    return 'Sconosciuto';
}

// Get download links/URIs
function getDownloadLinks(game) {
    if (game.uris && Array.isArray(game.uris)) {
        return game.uris;
    } else if (game.links && Array.isArray(game.links)) {
        return game.links;
    }
    return [];
}

// Assign category based on game title (simulated)
function assignCategory(game) {
    const title = getGameTitle(game).toLowerCase();
    
    if (title.includes('simulator') || title.includes('simulatore')) {
        game.category = 'Simulazione';
    } else if (title.includes('horror') || title.includes('survival') || title.includes('spavento')) {
        game.category = 'Horror';
    } else if (title.includes('action') || title.includes('azione') || title.includes('combat') || title.includes('shooter') || title.includes('fight')) {
        game.category = 'Azione';
    } else if (title.includes('adventure') || title.includes('avventura') || title.includes('story') || title.includes('narrative')) {
        game.category = 'Avventura';
    } else if (title.includes('rpg') || title.includes('role')) {
        game.category = 'RPG';
    } else if (title.includes('strategy') || title.includes('strategia') || title.includes('tactical')) {
        game.category = 'Strategia';
    } else if (title.includes('indie') || title.includes('platform')) {
        game.category = 'Indie';
    } else if (title.includes('sports') || title.includes('sport') || title.includes('football') || title.includes('basketball') || title.includes('tennis')) {
        game.category = 'Sport';
    } else if (title.includes('racing') || title.includes('drive') || title.includes('car') || title.includes('automobil')) {
        game.category = 'Corse';
    } else if (title.includes('open world') || title.includes('sandbox')) {
        game.category = 'Open World';
    } else if (title.includes('puzzle') || title.includes('casual') || title.includes('family')) {
        game.category = 'Puzzle';
    } else {
        game.category = 'Altro';
    }
}

// Update statistics
function updateStats() {
    const games = allGamesData[currentSource];
    totalGamesEl.textContent = games.length;
    
    // Calculate total size
    let totalSizeGB = 0;
    games.forEach(game => {
        if (game.numericSize) totalSizeGB += game.numericSize;
    });
    
    totalSizeEl.textContent = totalSizeGB.toFixed(1) + ' GB';
    
    // Get most recent upload date
    if (games.length > 0) {
        let dates = [];
        games.forEach(game => {
            const date = new Date(getUploadDate(game));
            if (!isNaN(date.getTime())) {
                dates.push(date);
            }
        });
        
        if (dates.length > 0) {
            dates.sort((a, b) => b - a);
            const lastUpdate = dates[0];
            const now = new Date();
            const diffDays = Math.floor((now - lastUpdate) / (1000 * 60 * 60 * 24));
            
            if (diffDays === 0) {
                document.getElementById('last-update').textContent = 'Oggi';
            } else if (diffDays === 1) {
                document.getElementById('last-update').textContent = 'Ieri';
            } else {
                document.getElementById('last-update').textContent = `${diffDays} giorni fa`;
            }
        }
    }
}

// Update source counts
function updateSourceCounts() {
    document.getElementById('steamrip-count').textContent = allGamesData.steamrip.length;
    document.getElementById('onlinefix-count').textContent = allGamesData.onlinefix.length;
    document.getElementById('fitgirl-count').textContent = allGamesData.fitgirl.length;
    document.getElementById('altro-count').textContent = allGamesData.altro.length; // AGGIUNTA
}

// Setup categories filter
function setupCategories() {
    const games = allGamesData[currentSource];
    
    // Clear existing categories (except "All")
    const allBtn = categoryList.querySelector('.category-btn.active');
    categoryList.innerHTML = '';
    categoryList.appendChild(allBtn);
    
    // Count games per category
    const categoryCounts = {};
    games.forEach(game => {
        const cat = game.category || 'Altro';
        categoryCounts[cat] = (categoryCounts[cat] || 0) + 1;
    });
    
    // Add category buttons
    Array.from(categories).sort().forEach(category => {
        const li = document.createElement('li');
        const count = categoryCounts[category] || 0;
        
        li.innerHTML = `
            <button class="category-btn" data-category="${category}">
                ${category} <span class="category-count">${count}</span>
            </button>
        `;
        
        categoryList.appendChild(li);
    });
    
    // Update "All" count
    document.querySelector('.category-count').textContent = games.length;
    
    // Add event listeners to category buttons
    document.querySelectorAll('.category-btn[data-category]').forEach(btn => {
        btn.addEventListener('click', function() {
            document.querySelectorAll('.category-btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            currentFilter = this.dataset.category;
            currentPage = 1; // Reset to first page when changing filter
            renderGames();
        });
    });
}

// Render games based on current filters and sort
function renderGames() {
    // Combina tutti i giochi da tutte le fonti quando si cerca
    let gamesToFilter;
    
    if (searchBox.value.trim() !== '') {
        // Se c'è una ricerca, combina tutte le fonti
        gamesToFilter = [
            ...allGamesData.steamrip.map(g => ({ ...g, source: 'steamrip' })),
            ...allGamesData.onlinefix.map(g => ({ ...g, source: 'onlinefix' })),
            ...allGamesData.fitgirl.map(g => ({ ...g, source: 'fitgirl' })),
            ...allGamesData.altro.map(g => ({ ...g, source: 'altro' })) // AGGIUNTA
        ];
        gamesToFilter = filterCrossSourceDuplicates(gamesToFilter);
    } else {
        // Altrimenti, usa solo la fonte corrente
        gamesToFilter = allGamesData[currentSource];
    }
    
    // Filter games
    filteredGames = gamesToFilter.filter(game => {
        // Category filter (solo se non stiamo cercando in tutte le fonti)
        if (searchBox.value.trim() === '' && currentFilter !== 'all' && game.category !== currentFilter) {
            return false;
        }
        
        // Search filter - cerca nel titolo ORIGINALE
        const searchTerm = searchBox.value.toLowerCase();
        const originalTitle = getGameTitle(game).toLowerCase();
        if (searchTerm && !originalTitle.includes(searchTerm)) {
            return false;
        }
        
        // Size filter
        const maxSize = parseInt(sizeSlider.value);
        if (maxSize < maxFileSize && (!game.numericSize || game.numericSize > maxSize)) {
            return false;
        }
        
        return true;
    });
    
    // Sort games
    filteredGames.sort((a, b) => {
        const dateA = new Date(getUploadDate(a));
        const dateB = new Date(getUploadDate(b));
        
        switch(currentSort) {
            case 'date-desc': return dateB - dateA;
            case 'date-asc': return dateA - dateB;
            case 'size-desc': return (b.numericSize || 0) - (a.numericSize || 0);
            case 'size-asc': return (a.numericSize || 0) - (b.numericSize || 0);
            default: return dateB - dateA;
        }
    });
    
    // Calculate pagination
    totalPages = Math.ceil(filteredGames.length / GAMES_PER_PAGE);
    if (currentPage > totalPages) {
        currentPage = totalPages || 1;
    }
    
    // Update games count
    const startIndex = (currentPage - 1) * GAMES_PER_PAGE + 1;
    const endIndex = Math.min(currentPage * GAMES_PER_PAGE, filteredGames.length);
    gamesCountEl.textContent = `Mostrando ${startIndex}-${endIndex} di ${filteredGames.length} giochi`;
    
    // Update source indicator per mostrare se stiamo cercando in tutte le fonti
    if (searchBox.value.trim() !== '') {
        sourceIndicator.innerHTML = `
            <i class="fas fa-search"></i>
            <span>Risultati da tutte le fonti</span>
        `;
    } else {
        const sourceNames = {
            steamrip: 'SteamRip',
            onlinefix: 'OnlineFix',
            fitgirl: 'FitGirl',
            altro: 'Altro Provider' // AGGIUNTA
        };
        
        const sourceIcons = {
            steamrip: 'cloud-download-alt',
            onlinefix: 'wifi',
            fitgirl: 'female',
            altro: 'plus-circle' // AGGIUNTA
        };
        
        sourceIndicator.innerHTML = `
            <i class="fas fa-${sourceIcons[currentSource]}"></i>
            <span>${sourceNames[currentSource]}</span>
        `;
    }
    
    // Clear container
    gamesContainer.innerHTML = '';
    
    // Render games for current page
    if (filteredGames.length === 0) {
        gamesContainer.innerHTML = `
            <div class="loading">
                <i class="fas fa-search"></i>
                <h3>Nessun gioco trovato</h3>
                <p>Prova a modificare i filtri o la ricerca</p>
            </div>
        `;
        pagination.style.display = 'none';
        return;
    }
    
    const start = (currentPage - 1) * GAMES_PER_PAGE;
    const end = start + GAMES_PER_PAGE;
    const gamesToShow = filteredGames.slice(start, end);
    
    gamesToShow.forEach(game => {
        const gameCard = createGameCard(game);
        gamesContainer.appendChild(gameCard);
    });
    
    // Update pagination controls
    updatePagination();
}

// Create a game card element
function createGameCard(game) {
    const card = document.createElement('div');
    card.className = 'game-card';
    
    // Get game data - usa il titolo ORIGINALE dai dati Hydralinks
    const originalTitle = getGameTitle(game); // Titolo originale di Hydralinks
    const displayTitle = cleanDisplayTitle(originalTitle); // Solo per aspetto visivo
    
    const fileSize = getFormattedFileSize(game);
    
    // Format date
    let formattedDate = 'Data sconosciuta';
    try {
        const uploadDate = new Date(getUploadDate(game));
        if (!isNaN(uploadDate.getTime())) {
            formattedDate = uploadDate.toLocaleDateString('it-IT', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            });
        }
    } catch (e) {
        console.log('Errore formattazione data:', e);
    }
    
    // Get image URL - usa il titolo ORIGINALE per cercare in steamData
    const imageUrl = getSteamImageUrl(originalTitle, game.source);
    
    // Get source badge class
    const sourceBadgeClass = `${game.source}-badge-card`;
    
    card.innerHTML = `
        <div class="game-image" style="background-image: url('${imageUrl}')">
            <div class="game-source-badge ${sourceBadgeClass}">${game.source.toUpperCase()}</div>
        </div>
        <div class="game-info">
            <h3 class="game-title" title="${displayTitle}">${displayTitle}</h3>
            <div class="game-meta">
                <div class="game-date"><i class="far fa-calendar-alt"></i> ${formattedDate}</div>
                <div class="game-size"><i class="fas fa-hdd"></i> ${fileSize}</div>
            </div>
            <button class="game-download-btn" data-game-id="${originalTitle}">
                <i class="fas fa-download"></i> Vedi Download
            </button>
        </div>
    `;
    
    // Add click event to show game details - passa il gioco ORIGINALE
    card.querySelector('.game-download-btn').addEventListener('click', () => showGameDetails(game));
    
    return card;
}

// Get Steam/GOG image URL for a game title from current source data
function getSteamImageUrl(title, source = null) {
    if (!title) return `https://via.placeholder.com/400x200/2d3748/4299e1?text=No+Image`;
    
    // Usa la fonte del gioco se specificata, altrimenti usa currentSource
    const gameSource = source || currentSource;
    
    // Try to find Steam data for this game in the specified source
    const currentSteamData = steamData[gameSource];
    
    // CERCA DIRETTAMENTE PER TITOLO ESATTO (senza normalizzazione)
    const steamInfo = currentSteamData.find(s => {
        if (!s.title) return false;
        // Confronto DIRETTO del titolo
        return s.title.toLowerCase() === title.toLowerCase();
    });
    
    if (steamInfo && steamInfo.steam_appid) {
        const appId = steamInfo.steam_appid.toString();
        
        // Check if it's a GOG ID
        if (appId.startsWith('GOG:')) {
            const gogId = appId.replace('GOG:', '');
            // GOG cover image URL
            return `https://images.gog.com/${gogId}_product_card_v2_mobile_slider_639.webp`;
        } 
        // Regular Steam ID
        else {
            return `https://cdn.cloudflare.steamstatic.com/steam/apps/${appId}/header.jpg`;
        }
    }
    
    // Fallback to placeholder
    const shortTitle = title.substring(0, 30);
    return `https://via.placeholder.com/400x200/2d3748/4299e1?text=${encodeURIComponent(shortTitle)}`;
}

// Update pagination controls
function updatePagination() {
    // Show/hide pagination
    if (filteredGames.length > GAMES_PER_PAGE) {
        pagination.style.display = 'flex';
    } else {
        pagination.style.display = 'none';
        return;
    }
    
    // Update page info
    currentPageEl.textContent = currentPage;
    totalPagesEl.textContent = totalPages;
    
    // Enable/disable buttons
    firstPageBtn.disabled = currentPage === 1;
    prevPageBtn.disabled = currentPage === 1;
    nextPageBtn.disabled = currentPage === totalPages;
    lastPageBtn.disabled = currentPage === totalPages;
    
    // Generate page numbers
    pageNumbersEl.innerHTML = '';
    
    // Show first few pages, current page, and last few pages
    const maxPageButtons = 7;
    let startPage = Math.max(1, currentPage - Math.floor(maxPageButtons / 2));
    let endPage = Math.min(totalPages, startPage + maxPageButtons - 1);
    
    // Adjust if we're near the end
    if (endPage - startPage + 1 < maxPageButtons) {
        startPage = Math.max(1, endPage - maxPageButtons + 1);
    }
    
    // Add first page if not already included
    if (startPage > 1) {
        addPageNumber(1);
        if (startPage > 2) {
            const ellipsis = document.createElement('span');
            ellipsis.textContent = '...';
            ellipsis.style.padding = '0 5px';
            pageNumbersEl.appendChild(ellipsis);
        }
    }
    
    // Add page numbers
    for (let i = startPage; i <= endPage; i++) {
        addPageNumber(i);
    }
    
    // Add last page if not already included
    if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
            const ellipsis = document.createElement('span');
            ellipsis.textContent = '...';
            ellipsis.style.padding = '0 5px';
            pageNumbersEl.appendChild(ellipsis);
        }
        addPageNumber(totalPages);
    }
}

// Add a page number button
function addPageNumber(page) {
    const pageBtn = document.createElement('div');
    pageBtn.className = `page-number ${page === currentPage ? 'active' : ''}`;
    pageBtn.textContent = page;
    pageBtn.addEventListener('click', () => {
        if (page !== currentPage) {
            currentPage = page;
            renderGames();
            // Scroll to top of games container
            gamesContainer.scrollIntoView({ behavior: 'smooth' });
        }
    });
    pageNumbersEl.appendChild(pageBtn);
}

// Show game details modal
function showGameDetails(game) {
    const originalTitle = getGameTitle(game); // Titolo originale di Hydralinks
    const displayTitle = cleanDisplayTitle(originalTitle); // Solo per aspetto visivo
    const fileSize = getFormattedFileSize(game);
    
    // Set modal title - usa il titolo VISIVO (pulito)
    modalTitleText.textContent = displayTitle;
    
    // Set source badge
    modalSourceBadge.textContent = game.source.toUpperCase();
    modalSourceBadge.className = `modal-source-badge ${game.source}-badge-card`;
    
    // Set modal image - usa il titolo ORIGINALE per cercare l'immagine
    const imageUrl = getSteamImageUrl(originalTitle, game.source);
    modalImage.style.backgroundImage = `url('${imageUrl}')`;
    
    // Format date
    let formattedDate = 'Data sconosciuta';
    try {
        const uploadDate = new Date(getUploadDate(game));
        if (!isNaN(uploadDate.getTime())) {
            formattedDate = uploadDate.toLocaleDateString('it-IT', {
                weekday: 'long',
                day: 'numeric',
                month: 'long',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
    } catch (e) {
        console.log('Errore formattazione data dettagli:', e);
    }
    
    // Set download instructions based on source
    setDownloadInstructions(game.source);
    
    // Set game properties
    gameProperties.innerHTML = `
        <div class="property">
            <div class="property-label">Categoria</div>
            <div class="property-value">${game.category}</div>
        </div>
        <div class="property">
            <div class="property-label">Dimensione</div>
            <div class="property-value">${fileSize}</div>
        </div>
        <div class="property">
            <div class="property-label">Data di Upload</div>
            <div class="property-value">${formattedDate}</div>
        </div>
        <div class="property">
            <div class="property-label">Fonte</div>
            <div class="property-value">${game.source.toUpperCase()}</div>
        </div>
        <div class="property">
            <div class="property-label">Titolo Originale</div>
            <div class="property-value" style="font-size: 0.9rem; color: #a0aec0;">${originalTitle}</div>
        </div>
    `;
    
    // Process download links
    const links = getDownloadLinks(game);
    displayDownloadLinks(links, game.source);
    
    // Show modal
    gameDetailsModal.style.display = 'block';
    document.body.style.overflow = 'hidden';
    
    if (!game.source) {
        game.source = currentSource;
    }
}

// Set download instructions based on source
function setDownloadInstructions(source) {
    let instructions = '';
    
    switch(source) {
        case 'steamrip':
            instructions = '<p>I link sono mostrati in ordine di preferenza: <strong>buzzheavier</strong> → <strong>vikingfile</strong> → <strong>gofile</strong></p>';
            break;
        case 'onlinefix':
            instructions = '<p><strong>🔴 IMPORTANTE:</strong> Tutti i giochi OnlineFix sono <strong>MULTIPLAYER</strong> e richiedono una connessione internet per funzionare correttamente. Spesso includono funzionalità multiplayer e supporto per giocare online con amici.</p>';
            break;
        case 'fitgirl':
            instructions = '<p>FitGirl repacks sono altamente compressi. Durante l\'installazione potrebbero essere necessari diversi minuti/ore. Disabilita l\'antivirus durante l\'installazione per evitare falsi positivi.</p>';
            break;
        case 'altro': // AGGIUNTA
            instructions = '<p><strong>🎮 MISTO:</strong> Questa categoria include giochi da varie fonti diverse. Segui le istruzioni specifiche per ciascun gioco. Alcuni potrebbero essere portatili, altri richiedere installazione.</p>';
            break;
        default:
            instructions = '<p>Clicca su un link per avviare il download. I link magnet possono essere aperti con un client torrent.</p>';
    }
    
    downloadInstructions.innerHTML = instructions;
}

// Display download links in modal
// Display download links in modal - VERSIONE CORRETTA
function displayDownloadLinks(links, source) {
    downloadLinks.innerHTML = '';
    
    if (!links || links.length === 0) {
        downloadLinks.innerHTML = `
            <div style="text-align: center; padding: 30px; color: #a0aec0;">
                <i class="fas fa-exclamation-circle" style="font-size: 2rem; margin-bottom: 15px;"></i>
                <p>Nessun link di download disponibile per questo gioco</p>
            </div>
        `;
        return;
    }
    
    console.log('Links da processare:', links);
    
    // Mostra ogni link
    links.forEach((link, index) => {
        if (!link) return;
        
        console.log('Processando link:', link);
        
        const linkDiv = document.createElement('div');
        linkDiv.className = 'download-link';
        
        // Ottieni informazioni sul link
        const linkInfo = getLinkInfo(link);
        
        // Crea un URI sicuro per la visualizzazione
        const safeDisplayUri = link.length > 100 ? link.substring(0, 100) + '...' : link;
        
        // Crea un href sicuro
        let safeHref = link;
        if (link.startsWith('magnet:')) {
            safeHref = link;
        } else if (link.startsWith('http://') || link.startsWith('https://')) {
            safeHref = link;
        } else {
            safeHref = link.startsWith('?xt=') ? 'magnet:' + link : link;
        }
        
        linkDiv.innerHTML = `
            <div class="link-info">
                <div class="link-icon ${linkInfo.class}">
                    <i class="${linkInfo.icon}"></i>
                </div>
                <div>
                    <div class="link-name">${linkInfo.name} ${links.length > 1 ? `(#${index + 1})` : ''}</div>
                    <div style="font-size: 0.9rem; color: #a0aec0; margin-top: 5px; word-break: break-all;">${safeDisplayUri}</div>
                </div>
            </div>
            <div style="display: flex; align-items: center; gap: 15px;">
                ${index === 0 ? '<span class="preferred-badge"><i class="fas fa-crown"></i> Preferito</span>' : ''}
                <a href="${safeHref}" target="_blank" class="link-btn">
                    <i class="fas fa-external-link-alt"></i> Vai al Download
                </a>
            </div>
        `;
        
        downloadLinks.appendChild(linkDiv);
    });
}

// Funzione per ottenere informazioni su un link (indipendente da SITE_PRIORITY)
function getLinkInfo(link) {
    if (!link) {
        return {
            name: 'Download',
            class: 'other',
            icon: 'fas fa-download',
            priority: 99
        };
    }
    
    // Per magnet links
    if (link.startsWith('magnet:')) {
        return {
            name: 'Torrent Magnet',
            class: 'magnet',
            icon: 'fas fa-magnet',
            priority: 1
        };
    }
    
    // Per HTTP/HTTPS links
    if (link.startsWith('http://') || link.startsWith('https://')) {
        try {
            const url = new URL(link);
            const hostname = url.hostname.replace('www.', '');
            
            // Cerca nella SITE_PRIORITY per nome personalizzato e priorità
            const siteKey = Object.keys(SITE_PRIORITY).find(key => hostname.includes(key));
            
            if (siteKey) {
                // Usa le informazioni dalla SITE_PRIORITY
                return SITE_PRIORITY[siteKey];
            } else {
                // Link non in SITE_PRIORITY - mostra comunque
                return {
                    name: hostname,
                    class: 'direct',
                    icon: 'fas fa-download',
                    priority: 3
                };
            }
        } catch (e) {
            // URL non valido
            return {
                name: 'Download',
                class: 'other',
                icon: 'fas fa-download',
                priority: 4
            };
        }
    }
    
    // Altri tipi di link
    return {
        name: 'Download',
        class: 'other',
        icon: 'fas fa-download',
        priority: 5
    };
}

// Clean game title for display (remove "Free Download" and other unwanted text)
function cleanDisplayTitle(title) {
    if (!title) return '';
    
    // Rimuovi "Free Download" e varianti
    let cleanTitle = title
        .replace(/\bFree Download\b/gi, '')
        .replace(/\bDownload Free\b/gi, '')
        .replace(/\bFREE\b/gi, '')
        .replace(/\bDOWNLOAD\b/gi, '')
        .replace(/\bFree\b/g, '')
        .replace(/\bDownload\b/g, '');
    
    // Rimuovi doppi spazi e trim
    cleanTitle = cleanTitle.replace(/\s+/g, ' ').trim();
    
    // Rimuovi caratteri speciali all'inizio/fine
    cleanTitle = cleanTitle.replace(/^[:\-–—\s]+|[:\-–—\s]+$/g, '');
    
    return cleanTitle || title;
}

// Switch between sources
function switchSource(source) {
    // Update active source button
    document.querySelectorAll('.source-btn').forEach(btn => btn.classList.remove('active'));
    document.getElementById(`source-${source}`).classList.add('active');
    
    // Update current source
    currentSource = source;
    
    // Update source indicator
    const sourceNames = {
        steamrip: 'SteamRip',
        onlinefix: 'OnlineFix',
        fitgirl: 'FitGirl',
        altro: 'Altro Provider' // AGGIUNTA
    };
    
    const sourceIcons = {
        steamrip: 'cloud-download-alt',
        onlinefix: 'wifi',
        fitgirl: 'female',
        altro: 'plus-circle' // AGGIUNTA
    };
    
    sourceIndicator.innerHTML = `
        <i class="fas fa-${sourceIcons[source]}"></i>
        <span>${sourceNames[source]}</span>
    `;
    
    // Reset filter and pagination
    currentFilter = "all";
    currentPage = 1;
    document.querySelectorAll('.category-btn').forEach(b => b.classList.remove('active'));
    document.querySelector('.category-btn').classList.add('active');
    
    // Process games for new source
    processGamesData();
    updateStats();
    renderGames();
    setupCategories();
}

// Update size filter value display
function updateSizeValue(value) {
    if (value == maxFileSize) {
        sizeValue.textContent = 'Tutte le dimensioni';
    } else {
        sizeValue.textContent = `Fino a ${value} GB`;
    }
}

// Close game details modal
function closeModal() {
    gameDetailsModal.style.display = 'none';
    document.body.style.overflow = 'auto';
}

// Show error message
function showErrorMessage(message) {
    const gamesContainer = document.getElementById('games-container');
    gamesContainer.innerHTML = `
        <div class="loading">
            <i class="fas fa-exclamation-triangle" style="color: var(--danger-color);"></i>
            <h3>Errore di caricamento</h3>
            <p>${message}</p>
        </div>
    `;
}

// FAQ Toggle Functions
function toggleFAQ() {
    faqSection.classList.toggle('active');
    
    // Scorri verso la FAQ quando viene aperta
    if (faqSection.classList.contains('active')) {
        setTimeout(() => {
            faqSection.scrollIntoView({ behavior: 'smooth' });
        }, 100);
    }
}

// Setup event listeners
function setupEventListeners() {
    // Search box
    searchBox.addEventListener('input', () => {
        currentPage = 1; // Reset to first page when searching
        renderGames();
    });
    
    // Sort buttons
    sortButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            sortButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            currentSort = this.dataset.sort;
            currentPage = 1; // Reset to first page when sorting
            renderGames();
        });
    });
    
    // Size slider
    sizeSlider.addEventListener('input', function() {
        updateSizeValue(this.value);
        currentPage = 1; // Reset to first page when filtering by size
        renderGames();
    });
    
    // View toggle
    viewGridBtn.addEventListener('click', function() {
        viewGridBtn.classList.add('active');
        viewListBtn.classList.remove('active');
        currentView = 'grid';
        gamesContainer.classList.remove('list-view');
    });
    
    viewListBtn.addEventListener('click', function() {
        viewListBtn.classList.add('active');
        viewGridBtn.classList.remove('active');
        currentView = 'list';
        gamesContainer.classList.add('list-view');
    });
    
    // Source switching
    sourceSteamripBtn.addEventListener('click', () => switchSource('steamrip'));
    sourceOnlinefixBtn.addEventListener('click', () => switchSource('onlinefix'));
    sourceFitgirlBtn.addEventListener('click', () => switchSource('fitgirl'));
    sourceAltroBtn.addEventListener('click', () => switchSource('altro')); // AGGIUNTA
    
    // Modal close button
    closeModalBtn.addEventListener('click', closeModal);
    
    // Close modal when clicking outside
    gameDetailsModal.addEventListener('click', function(e) {
        if (e.target === this) {
            closeModal();
        }
    });
    
    // Close modal with ESC key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeModal();
        }
    });
    
    // Pagination buttons
    firstPageBtn.addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage = 1;
            renderGames();
            gamesContainer.scrollIntoView({ behavior: 'smooth' });
        }
    });
    
    prevPageBtn.addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage--;
            renderGames();
            gamesContainer.scrollIntoView({ behavior: 'smooth' });
        }
    });
    
    nextPageBtn.addEventListener('click', () => {
        if (currentPage < totalPages) {
            currentPage++;
            renderGames();
            gamesContainer.scrollIntoView({ behavior: 'smooth' });
        }
    });
    
    lastPageBtn.addEventListener('click', () => {
        if (currentPage < totalPages) {
            currentPage = totalPages;
            renderGames();
            gamesContainer.scrollIntoView({ behavior: 'smooth' });
        }
    });
    
    // FAQ Toggle Button
    faqToggleBtn.addEventListener('click', toggleFAQ);
    closeFaqBtn.addEventListener('click', toggleFAQ);
}
