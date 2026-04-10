// Alpine.js Slider Component - VERSIONE DINAMICA
function heroSlider(totalSlides) {
    return {
        current: 0,
        total: totalSlides,
        autoplayTimer: null,
        autoplayDelay: 6000,
        slides: [],
        initialized: false,
        
        init() {
            // Attende che i dati globali siano caricati
            this.waitForData();
        },
        
        waitForData() {
            const checkInterval = setInterval(() => {
                // Controlla se i dati dei provider sono stati caricati
                if (window.allGamesData && 
                    window.allGamesData.steamrip && window.allGamesData.steamrip.length > 0 &&
                    window.allGamesData.onlinefix && window.allGamesData.onlinefix.length > 0 &&
                    window.allGamesData.fitgirl && window.allGamesData.fitgirl.length > 0) {
                    clearInterval(checkInterval);
                    this.generateSlidesFromData();
                    this.startAutoplay();
                }
            }, 500);
            
            // Timeout dopo 10 secondi
            setTimeout(() => {
                clearInterval(checkInterval);
                if (!this.initialized) {
                    this.generateFallbackSlides();
                    this.startAutoplay();
                }
            }, 10000);
        },
        
        generateSlidesFromData() {
            // Prende 1 gioco RANDOM da SteamRip
            const steamripGames = window.allGamesData.steamrip || [];
            const randomSteamrip = steamripGames.length > 0 ? 
                steamripGames[Math.floor(Math.random() * steamripGames.length)] : null;
            
            // Prende 1 gioco RANDOM da OnlineFix
            const onlinefixGames = window.allGamesData.onlinefix || [];
            const randomOnlinefix = onlinefixGames.length > 0 ? 
                onlinefixGames[Math.floor(Math.random() * onlinefixGames.length)] : null;
            
            // Prende 1 gioco RANDOM da FitGirl
            const fitgirlGames = window.allGamesData.fitgirl || [];
            const randomFitgirl = fitgirlGames.length > 0 ? 
                fitgirlGames[Math.floor(Math.random() * fitgirlGames.length)] : null;
            
            this.slides = [];
            
            if (randomSteamrip) {
                this.slides.push(this.createSlideFromGame(randomSteamrip, 'steamrip'));
            }
            if (randomOnlinefix) {
                this.slides.push(this.createSlideFromGame(randomOnlinefix, 'onlinefix'));
            }
            if (randomFitgirl) {
                this.slides.push(this.createSlideFromGame(randomFitgirl, 'fitgirl'));
            }
            
            // Se non ci sono abbastanza slides, usa fallback
            if (this.slides.length === 0) {
                this.generateFallbackSlides();
            }
            
            this.total = this.slides.length;
            this.initialized = true;
            
            // Forza l'aggiornamento della view
            this.$nextTick(() => {
                this.current = 0;
            });
        },
        
        createSlideFromGame(game, source) {
            const title = this.cleanDisplayTitle(game.title || game.name || 'Sconosciuto');
            const version = this.extractVersion(game);
            const imageUrl = this.getGameImageUrl(game);
            const description = this.truncateDescription(game.description || this.getDefaultDescription(source), 120);
            
            const sourceNames = {
                steamrip: 'SteamRip',
                onlinefix: 'OnlineFix',
                fitgirl: 'FitGirl'
            };
            
            return {
                title: title,
                version: version,
                image: imageUrl,
                logo: imageUrl,
                description: description,
                link: "#",
                downloadUrl: "#",
                gameData: game,
                source: source,
                sourceName: sourceNames[source] || source.toUpperCase()
            };
        },
        
        getGameImageUrl(game) {
            if (game.hero_image) return game.hero_image;
            if (game.grid_image) return game.grid_image;
            if (game.icon_image) return game.icon_image;
            
            // Placeholder colorato basato sul titolo
            const title = this.cleanDisplayTitle(game.title || game.name || 'Game');
            const colors = ['f97316', '10b981', '8b5cf6', 'ef4444', '3b82f6', 'ec4899'];
            const color = colors[Math.floor(Math.random() * colors.length)];
            return `https://via.placeholder.com/1920x800/1a1e26/${color}?text=${encodeURIComponent(title.substring(0, 30))}`;
        },
        
        cleanDisplayTitle(title) {
            if (!title) return '';
            return title
                .replace(/\bFree Download\b/gi, '')
                .replace(/\bDownload Free\b/gi, '')
                .replace(/\bFREE\b/gi, '')
                .replace(/\bDOWNLOAD\b/gi, '')
                .replace(/\s+/g, ' ')
                .trim()
                .substring(0, 50);
        },
        
        extractVersion(game) {
            // Cerca di estrarre la versione dal titolo o dai metadati
            const title = game.title || game.name || '';
            const versionMatch = title.match(/[Vv]?\s*(\d+\.\d+(?:\.\d+)?)/);
            if (versionMatch) return `V ${versionMatch[1]}`;
            if (game.version) return game.version;
            if (game.build) return `Build ${game.build}`;
            return "Ultima versione";
        },
        
        getDefaultDescription(source) {
            const descriptions = {
                steamrip: "Gioco portatile SteamRip, pronto all'uso. Nessuna installazione richiesta, estrai e gioca!",
                onlinefix: "Versione OnlineFix con supporto multiplayer. Gioca online con i tuoi amici!",
                fitgirl: "Repack FitGirl altamente compresso. Installazione guidata e risparmio di spazio!"
            };
            return descriptions[source] || "Scarica e gioca subito a questo fantastico titolo!";
        },
        
        truncateDescription(desc, maxLength) {
            if (!desc) return '';
            if (desc.length <= maxLength) return desc;
            return desc.substring(0, maxLength) + '...';
        },
        
        generateFallbackSlides() {
            // Fallback nel caso i dati non siano ancora caricati
            this.slides = [
                {
                    title: "Caricamento in corso...",
                    version: "V 1.0",
                    image: "https://via.placeholder.com/1920x800/1a1e26/f97316?text=SteamRip",
                    logo: "https://via.placeholder.com/400x100/1a1e26/f97316?text=SteamRip",
                    description: "Attendi il caricamento dei giochi SteamRip",
                    link: "#",
                    downloadUrl: "#",
                    source: "steamrip",
                    sourceName: "SteamRip"
                },
                {
                    title: "Caricamento in corso...",
                    version: "V 1.0",
                    image: "https://via.placeholder.com/1920x800/1a1e26/10b981?text=OnlineFix",
                    logo: "https://via.placeholder.com/400x100/1a1e26/10b981?text=OnlineFix",
                    description: "Attendi il caricamento dei giochi OnlineFix",
                    link: "#",
                    downloadUrl: "#",
                    source: "onlinefix",
                    sourceName: "OnlineFix"
                },
                {
                    title: "Caricamento in corso...",
                    version: "V 1.0",
                    image: "https://via.placeholder.com/1920x800/1a1e26/8b5cf6?text=FitGirl",
                    logo: "https://via.placeholder.com/400x100/1a1e26/8b5cf6?text=FitGirl",
                    description: "Attendi il caricamento dei giochi FitGirl",
                    link: "#",
                    downloadUrl: "#",
                    source: "fitgirl",
                    sourceName: "FitGirl"
                }
            ];
            this.total = this.slides.length;
            this.initialized = true;
        },
        
        startAutoplay() {
            if (this.autoplayTimer) clearInterval(this.autoplayTimer);
            this.autoplayTimer = setInterval(() => this.next(), this.autoplayDelay);
        },
        
        stopAutoplay() {
            if (this.autoplayTimer) {
                clearInterval(this.autoplayTimer);
                this.autoplayTimer = null;
            }
        },
        
        next() {
            this.current = (this.current + 1) % this.total;
            this.restartAutoplay();
        },
        
        prev() {
            this.current = this.current === 0 ? this.total - 1 : this.current - 1;
            this.restartAutoplay();
        },
        
        goTo(index) {
            this.current = index;
            this.restartAutoplay();
        },
        
        restartAutoplay() {
            this.stopAutoplay();
            this.startAutoplay();
        },
        
        showGameDetails(slide) {
            if (slide.gameData) {
                if (window.showGameDetailsModal) {
                    window.showGameDetailsModal(slide.gameData);
                } else {
                    console.log("Dettagli gioco:", slide.title);
                    alert(`🎮 ${slide.title}\n📦 Fonte: ${slide.sourceName}\n🔗 Disponibile su ${slide.sourceName}`);
                }
            }
        },
        
        downloadGame(slide) {
            if (slide.gameData) {
                if (window.showGameDetailsModal) {
                    window.showGameDetailsModal(slide.gameData);
                } else {
                    const links = slide.gameData.uris || slide.gameData.links || [];
                    if (links.length > 0) {
                        window.open(links[0], '_blank');
                    } else {
                        alert(`🔗 Link download per ${slide.title} disponibili nel dettaglio.`);
                    }
                }
            }
        }
    }
}

// Riferimento globale per i dati
window.allGamesData = {
    steamrip: [],
    onlinefix: [],
    fitgirl: [],
    altro: []
};

// Riferimento alla funzione showGameDetails
window.showGameDetailsModal = null;

// Cache per le immagini
let imageCache = new Map();

let categories = new Set();
let currentSource = "steamrip";
let currentFilter = "all";
let currentSort = "date-desc";
let currentView = "grid";
let maxFileSize = 0;

const GAMES_PER_PAGE = 30;
let currentPage = 1;
let totalPages = 1;
let filteredGames = [];

const SITE_PRIORITY = {
    'buzzheavier.com': { name: 'Buzzheavier', class: 'buzzheavier', icon: 'fas fa-bolt', priority: 1 },
    'vikingfile.com': { name: 'Vikingfile', class: 'vikingfile', icon: 'fas fa-shield-alt', priority: 2 },
    'gofile.io': { name: 'Gofile', class: 'gofile', icon: 'fas fa-file-archive', priority: 3 },
    '1drv.ms': { name: 'OneDrive', class: 'onedrive', icon: 'fas fa-cloud', priority: 1 },
    'onedrive.live.com': { name: 'OneDrive', class: 'onedrive', icon: 'fas fa-cloud', priority: 1 },
    'mega.nz': { name: 'MEGA', class: 'mega', icon: 'fas fa-database', priority: 2 },
    'magnet': { name: 'Torrent Magnet', class: 'magnet', icon: 'fas fa-magnet', priority: 1 }
};

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
const faqSection = document.getElementById('faq-section');
const faqToggleBtn = document.getElementById('faq-toggle');
const closeFaqBtn = document.getElementById('close-faq');
const sourceSteamripBtn = document.getElementById('source-steamrip');
const sourceOnlinefixBtn = document.getElementById('source-onlinefix');
const sourceFitgirlBtn = document.getElementById('source-fitgirl');
const sourceAltroBtn = document.getElementById('source-altro');

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    loadAllGamesData();
    setupEventListeners();
});

// Load all games data
async function loadAllGamesData() {
    try {
        // Carica i JSON con le immagini (nuovo formato)
        const [steamripImageData, onlinefixImageData, fitgirlImageData, altroImageData] = await Promise.all([
            loadImageData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamIDSteamRip.json'),
            loadImageData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamIDOnlineFix.json'),
            loadImageData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamIDFitGirl.json'),
            loadImageData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamIDAIMODS.json')
        ]);
        
        // Carica i dati originali dei giochi
        const [steamripData, onlinefixData, fitgirlData, altroData] = await Promise.all([
            loadSourceData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/steamrip.json', 'steamrip'),
            loadSourceData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/onlinefix.json', 'onlinefix'),
            loadSourceData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/fitgirl.json', 'fitgirl'),
            loadSourceData('https://raw.githubusercontent.com/MrNico98/PhoenixPlay/refs/heads/main/IDapp/aimods.json', 'altro')
        ]);
        
        // Unisci i dati dei giochi con le immagini
        window.allGamesData.steamrip = mergeGamesWithImages(steamripData, steamripImageData, 'steamrip');
        window.allGamesData.onlinefix = mergeGamesWithImages(onlinefixData, onlinefixImageData, 'onlinefix');
        window.allGamesData.fitgirl = mergeGamesWithImages(fitgirlData, fitgirlImageData, 'fitgirl');
        window.allGamesData.altro = mergeGamesWithImages(altroData, altroImageData, 'altro');
        
        processGamesData();
        updateStats();
        renderGames();
        setupCategories();
        updateSourceCounts();
        renderTrendingGames();
        
        // Aggiorna lo slider con i dati reali (trigger evento)
        window.dispatchEvent(new CustomEvent('gamesDataLoaded'));
        
    } catch (error) {
        console.error('Errore:', error);
        showErrorMessage('Errore nel caricamento dei dati. Riprova più tardi.');
    }
}

async function loadImageData(url) {
    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return await response.json() || [];
    } catch (error) {
        console.error(`Errore caricamento immagini:`, error);
        return [];
    }
}

function mergeGamesWithImages(games, imagesData, source) {
    if (!games || !Array.isArray(games)) return [];
    
    // Crea una mappa delle immagini per titolo
    const imageMap = new Map();
    imagesData.forEach(img => {
        if (img.title) {
            imageMap.set(img.title.toLowerCase().trim(), img);
        }
    });
    
    // Unisci i dati
    return games.map(game => {
        const gameTitle = game.title || game.name || '';
        const imageInfo = imageMap.get(gameTitle.toLowerCase().trim());
        
        return {
            ...game,
            source: source,
            hero_image: imageInfo?.hero_image || null,
            grid_image: imageInfo?.grid_image || null,
            icon_image: imageInfo?.icon_image || null
        };
    });
}

function extractBaseGameName(title) {
    if (!title) return '';
    let name = title.toLowerCase()
        .replace(/fitgirl repack|repack|complete bundle|ultimate edition|deluxe edition/gi, '')
        .replace(/\s*v\d+\.\d+(\.\d+)*(\.\d+)*/gi, '')
        .replace(/\s*\d+\.\d+(\.\d+)*(\.\d+)*/gi, '')
        .replace(/\s+build\s+\d+/gi, '')
        .replace(/\s*\[\s*multi\d*\s*\]/gi, '')
        .replace(/\s*\(\s*multi\d*\s*\)/gi, '')
        .replace(/\s*\+\s*\d+\s*dlcs?/gi, '')
        .replace(/\s*\[.*?\]/g, '')
        .replace(/\s*\(.*?\)/g, '')
        .replace(/[:–—\-]/g, ' ')
        .replace(/\s+/g, ' ')
        .trim();
    
    return name || title.toLowerCase();
}

async function loadSourceData(url, source) {
    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const json = await response.json();
        return json.downloads || [];
    } catch (error) {
        console.error(`Errore ${source}:`, error);
        return [];
    }
}

function processGamesData() {
    const games = window.allGamesData[currentSource];
    categories.clear();
    maxFileSize = 0;
    
    games.forEach(game => {
        game.numericSize = extractFileSize(game);
        if (game.numericSize && game.numericSize > maxFileSize) {
            maxFileSize = Math.ceil(game.numericSize);
        }
        assignCategory(game);
        if (game.category) categories.add(game.category);
        game.source = currentSource;
        if (!game.title && game.name) game.title = game.name;
        if (!game.uris && game.links) game.uris = game.links;
        game.baseName = extractBaseGameName(game.title);
    });
    
    if (sizeSlider) {
        sizeSlider.max = maxFileSize;
        sizeSlider.value = maxFileSize;
        updateSizeValue(maxFileSize);
    }
}

function extractFileSize(game) {
    const sizeStr = game.fileSize || game.size || '';
    const match = sizeStr.match(/([\d.]+)\s*(MB|GB)/i);
    if (match) {
        const size = parseFloat(match[1]);
        const unit = match[2].toUpperCase();
        return unit === 'GB' ? size : size / 1024;
    }
    return 0;
}

function getFormattedFileSize(game) {
    return game.fileSize || game.size || 'N/A';
}

function getUploadDate(game) {
    return game.uploadDate || game.date || new Date().toISOString();
}

function getGameTitle(game) {
    return game.title || game.name || 'Sconosciuto';
}

function getDownloadLinks(game) {
    return (game.uris && Array.isArray(game.uris)) ? game.uris : (game.links || []);
}

function assignCategory(game) {
    const title = getGameTitle(game).toLowerCase();
    const categoriesMap = {
        'simulator|simulatore': 'Simulazione',
        'horror|survival|spavento': 'Horror',
        'action|azione|combat|shooter|fight': 'Azione',
        'adventure|avventura|story|narrative': 'Avventura',
        'rpg|role': 'RPG',
        'strategy|strategia|tactical': 'Strategia',
        'indie|platform': 'Indie',
        'sports|sport|football|basketball|tennis': 'Sport',
        'racing|drive|car|automobil': 'Corse',
        'open world|sandbox': 'Open World',
        'puzzle|casual|family': 'Puzzle'
    };
    
    for (const [pattern, category] of Object.entries(categoriesMap)) {
        if (new RegExp(pattern).test(title)) {
            game.category = category;
            return;
        }
    }
    game.category = 'Altro';
}

function updateStats() {
    const games = window.allGamesData[currentSource];
    if (totalGamesEl) totalGamesEl.textContent = games.length;
    
    let totalSizeGB = 0;
    games.forEach(game => { if (game.numericSize) totalSizeGB += game.numericSize; });
    if (totalSizeEl) totalSizeEl.textContent = totalSizeGB.toFixed(1) + ' GB';
    
    if (games.length > 0) {
        const dates = games.map(game => new Date(getUploadDate(game))).filter(d => !isNaN(d.getTime()));
        if (dates.length) {
            dates.sort((a, b) => b - a);
            const diffDays = Math.floor((new Date() - dates[0]) / (1000 * 60 * 60 * 24));
            const lastUpdateEl = document.getElementById('last-update');
            if (lastUpdateEl) {
                if (diffDays === 0) lastUpdateEl.textContent = 'Oggi';
                else if (diffDays === 1) lastUpdateEl.textContent = 'Ieri';
                else lastUpdateEl.textContent = `${diffDays} giorni fa`;
            }
        }
    }
}

function updateSourceCounts() {
    const steamripCount = document.getElementById('steamrip-count');
    const onlinefixCount = document.getElementById('onlinefix-count');
    const fitgirlCount = document.getElementById('fitgirl-count');
    const altroCount = document.getElementById('altro-count');
    
    if (steamripCount) steamripCount.textContent = window.allGamesData.steamrip.length;
    if (onlinefixCount) onlinefixCount.textContent = window.allGamesData.onlinefix.length;
    if (fitgirlCount) fitgirlCount.textContent = window.allGamesData.fitgirl.length;
    if (altroCount) altroCount.textContent = window.allGamesData.altro.length;
}

function setupCategories() {
    const games = window.allGamesData[currentSource];
    const categoryCounts = {};
    games.forEach(game => {
        const cat = game.category || 'Altro';
        categoryCounts[cat] = (categoryCounts[cat] || 0) + 1;
    });
    
    let allBtn = categoryList.querySelector('.category-btn.active');
    if (!allBtn || !allBtn.hasAttribute('data-category')) {
        allBtn = document.createElement('button');
        allBtn.className = 'category-btn active';
        allBtn.setAttribute('data-category', 'all');
        allBtn.innerHTML = `Tutti <span class="category-count">${games.length}</span>`;
    } else {
        allBtn.innerHTML = `Tutti <span class="category-count">${games.length}</span>`;
    }
    
    categoryList.innerHTML = '';
    const allLi = document.createElement('li');
    allLi.appendChild(allBtn);
    categoryList.appendChild(allLi);
    
    Array.from(categories).sort().forEach(category => {
        const li = document.createElement('li');
        const count = categoryCounts[category] || 0;
        li.innerHTML = `<button class="category-btn" data-category="${category}">${category} <span class="category-count">${count}</span></button>`;
        categoryList.appendChild(li);
    });
    
    document.querySelectorAll('.category-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            document.querySelectorAll('.category-btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            currentFilter = this.dataset.category;
            currentPage = 1;
            renderGames();
        });
    });
}

function renderGames() {
    let gamesToFilter;
    
    if (searchBox && searchBox.value.trim() !== '') {
        gamesToFilter = [
            ...window.allGamesData.steamrip.map(g => ({ ...g, source: 'steamrip' })),
            ...window.allGamesData.onlinefix.map(g => ({ ...g, source: 'onlinefix' })),
            ...window.allGamesData.fitgirl.map(g => ({ ...g, source: 'fitgirl' })),
            ...window.allGamesData.altro.map(g => ({ ...g, source: 'altro' }))
        ];
        gamesToFilter = filterCrossSourceDuplicates(gamesToFilter);
    } else {
        gamesToFilter = [...window.allGamesData[currentSource]];
    }
    
    filteredGames = gamesToFilter.filter(game => {
        if (searchBox && searchBox.value.trim() === '' && currentFilter !== 'all' && game.category !== currentFilter) return false;
        if (searchBox && searchBox.value) {
            const originalTitle = getGameTitle(game).toLowerCase();
            if (!originalTitle.includes(searchBox.value.toLowerCase())) return false;
        }
        const maxSize = parseInt(sizeSlider?.value || maxFileSize);
        if (maxSize < maxFileSize && (!game.numericSize || game.numericSize > maxSize)) return false;
        return true;
    });
    
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
    
    totalPages = Math.ceil(filteredGames.length / GAMES_PER_PAGE);
    if (currentPage > totalPages) currentPage = totalPages || 1;
    
    const startIndex = (currentPage - 1) * GAMES_PER_PAGE + 1;
    const endIndex = Math.min(currentPage * GAMES_PER_PAGE, filteredGames.length);
    if (gamesCountEl) gamesCountEl.textContent = `Mostrando ${startIndex}-${endIndex} di ${filteredGames.length} giochi`;
    
    const sourceNames = { steamrip: 'SteamRip', onlinefix: 'OnlineFix', fitgirl: 'FitGirl', altro: 'Altro Provider' };
    const sourceIcons = { steamrip: 'cloud-download-alt', onlinefix: 'wifi', fitgirl: 'female', altro: 'plus-circle' };
    
    if (sourceIndicator) {
        if (searchBox && searchBox.value.trim() !== '') {
            sourceIndicator.innerHTML = `<i class="fas fa-search"></i><span>Risultati da tutte le fonti</span>`;
        } else {
            sourceIndicator.innerHTML = `<i class="fas fa-${sourceIcons[currentSource]}"></i><span>${sourceNames[currentSource]}</span>`;
        }
    }
    
    if (!gamesContainer) return;
    gamesContainer.innerHTML = '';
    
    if (filteredGames.length === 0) {
        gamesContainer.innerHTML = `<div class="loading"><i class="fas fa-search"></i><h3>Nessun gioco trovato</h3><p>Prova a modificare i filtri</p></div>`;
        if (pagination) pagination.style.display = 'none';
        return;
    }
    
    const start = (currentPage - 1) * GAMES_PER_PAGE;
    const end = start + GAMES_PER_PAGE;
    const gamesToShow = filteredGames.slice(start, end);
    
    gamesToShow.forEach(game => {
        const gameCard = createGameCard(game);
        gamesContainer.appendChild(gameCard);
    });
    
    if (pagination) pagination.style.display = filteredGames.length > GAMES_PER_PAGE ? 'flex' : 'none';
    updatePagination();
}

function getGameImageUrl(game) {
    if (game.hero_image) {
        return game.hero_image;
    }
    if (game.grid_image) {
        return game.grid_image;
    }
    const title = getGameTitle(game);
    return getSvgPlaceholder(title.substring(0, 30));
}

function filterCrossSourceDuplicates(games) {
    if (!games || games.length === 0) return [];
    const gameMap = new Map();
    
    games.forEach(game => {
        const baseName = game.baseName || extractBaseGameName(game.title);
        const uploadDate = new Date(game.uploadDate || game.date || '1970-01-01');
        const existingGame = gameMap.get(baseName);
        if (!existingGame || uploadDate > existingGame.uploadDate) {
            gameMap.set(baseName, { ...game, baseName, uploadDate });
        }
    });
    
    return Array.from(gameMap.values());
}

function createGameCard(game) {
    const card = document.createElement('div');
    card.className = 'game-card';
    card.setAttribute('data-game-title', getGameTitle(game));
    
    const originalTitle = getGameTitle(game);
    const displayTitle = cleanDisplayTitle(originalTitle);
    const fileSize = getFormattedFileSize(game);
    const imageUrl = getGameImageUrl(game);
    
    let formattedDate = 'Data sconosciuta';
    try {
        const uploadDate = new Date(getUploadDate(game));
        if (!isNaN(uploadDate.getTime())) {
            formattedDate = uploadDate.toLocaleDateString('it-IT');
        }
    } catch (e) {}
    
    const sourceBadgeClass = `${game.source}-badge-card`;
    
    card.innerHTML = `
        <div class="game-image" style="background-image: url('${imageUrl}'); background-size: cover; background-position: center;">
            <div class="game-source-badge ${sourceBadgeClass}">${game.source === 'altro' ? 'AIMODS' : game.source.toUpperCase()}</div>
        </div>
        <div class="game-info">
            <h3 class="game-title" title="${displayTitle.replace(/"/g, '&quot;')}">${displayTitle}</h3>
            <div class="game-meta">
                <div class="game-date"><i class="far fa-calendar-alt"></i> ${formattedDate}</div>
                <div class="game-size"><i class="fas fa-hdd"></i> ${fileSize}</div>
            </div>
            <button class="game-download-btn" data-game-id="${originalTitle.replace(/"/g, '&quot;')}">
                <i class="fas fa-download"></i> Vedi Download
            </button>
        </div>
    `;
    
    card.querySelector('.game-download-btn').addEventListener('click', (e) => {
        e.stopPropagation();
        showGameDetails(game);
    });
    
    return card;
}

function cleanDisplayTitle(title) {
    if (!title) return '';
    return title
        .replace(/\bFree Download\b/gi, '')
        .replace(/\bDownload Free\b/gi, '')
        .replace(/\bFREE\b/gi, '')
        .replace(/\bDOWNLOAD\b/gi, '')
        .replace(/\bFree\b/g, '')
        .replace(/\bDownload\b/g, '')
        .replace(/\s+/g, ' ')
        .trim();
}

function getSvgPlaceholder(text) {
    const encodedText = encodeURIComponent(text.substring(0, 50));
    return `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='200' viewBox='0 0 400 200'%3E%3Crect width='400' height='200' fill='%231a1e26'/%3E%3Ctext x='50%25' y='50%25' font-size='14' fill='%23f97316' text-anchor='middle' dy='.3em'%3E${encodedText}%3C/text%3E%3C/svg%3E`;
}

function updatePagination() {
    if (!pagination) return;
    
    if (filteredGames.length > GAMES_PER_PAGE) {
        pagination.style.display = 'flex';
    } else {
        pagination.style.display = 'none';
        return;
    }
    
    if (currentPageEl) currentPageEl.textContent = currentPage;
    if (totalPagesEl) totalPagesEl.textContent = totalPages;
    
    if (firstPageBtn) firstPageBtn.disabled = currentPage === 1;
    if (prevPageBtn) prevPageBtn.disabled = currentPage === 1;
    if (nextPageBtn) nextPageBtn.disabled = currentPage === totalPages;
    if (lastPageBtn) lastPageBtn.disabled = currentPage === totalPages;
}

// Funzione esportata globalmente per lo slider
window.showGameDetailsModal = function(game) {
    showGameDetails(game);
};

function showGameDetails(game) {
    if (!gameDetailsModal) return;
    
    const originalTitle = getGameTitle(game);
    const displayTitle = cleanDisplayTitle(originalTitle);
    const fileSize = getFormattedFileSize(game);
    const imageUrl = getGameImageUrl(game);
    
    if (modalTitleText) modalTitleText.textContent = displayTitle;
    if (modalSourceBadge) {
        modalSourceBadge.textContent = game.source.toUpperCase();
        modalSourceBadge.className = `modal-badge ${game.source}-badge-card`;
    }
    
    if (modalImage) {
        modalImage.style.backgroundImage = `url('${imageUrl}')`;
        modalImage.style.backgroundSize = 'cover';
        modalImage.style.backgroundPosition = 'center';
    }
    
    let formattedDate = 'Data sconosciuta';
    try {
        const uploadDate = new Date(getUploadDate(game));
        if (!isNaN(uploadDate.getTime())) {
            formattedDate = uploadDate.toLocaleDateString('it-IT', {
                weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
            });
        }
    } catch (e) {}
    
    setDownloadInstructions(game.source);
    
    if (gameProperties) {
        gameProperties.innerHTML = `
            <div class="property"><div class="property-label">Categoria</div><div class="property-value">${game.category || 'N/A'}</div></div>
            <div class="property"><div class="property-label">Dimensione</div><div class="property-value">${fileSize}</div></div>
            <div class="property"><div class="property-label">Data Upload</div><div class="property-value">${formattedDate}</div></div>
            <div class="property"><div class="property-label">Fonte</div><div class="property-value">${game.source.toUpperCase()}</div></div>
            <div class="property"><div class="property-label">Titolo Originale</div><div class="property-value" style="font-size:0.85rem;color:var(--text-secondary)">${originalTitle}</div></div>
        `;
    }
    
    const links = getDownloadLinks(game);
    displayDownloadLinks(links, game.source);
    
    gameDetailsModal.style.display = 'flex';
    document.body.style.overflow = 'hidden';
}

function setDownloadInstructions(source) {
    if (!downloadInstructions) return;
    const instructions = {
        steamrip: '<p>I link sono mostrati in ordine di preferenza: <strong>buzzheavier</strong> → <strong>vikingfile</strong> → <strong>gofile</strong></p>',
        onlinefix: '<p><strong>🔴 IMPORTANTE:</strong> Giochi <strong>MULTIPLAYER</strong> con supporto online.</p>',
        fitgirl: '<p>FitGirl repacks sono altamente compressi. Disabilita l\'antivirus durante l\'installazione.</p>',
        altro: '<p><strong>🎮 MISTO:</strong> Giochi da varie fonti. Segui le istruzioni specifiche.</p>'
    };
    downloadInstructions.innerHTML = instructions[source] || '<p>Clicca su un link per avviare il download.</p>';
}

function displayDownloadLinks(links, source) {
    if (!downloadLinks) return;
    downloadLinks.innerHTML = '';
    
    if (!links || links.length === 0) {
        downloadLinks.innerHTML = `<div style="text-align:center;padding:30px;color:var(--text-secondary)"><i class="fas fa-exclamation-circle"></i><p>Nessun link disponibile</p></div>`;
        return;
    }
    
    links.forEach((link, index) => {
        if (!link) return;
        const linkInfo = getLinkInfo(link);
        let safeHref = link;
        if (link.startsWith('magnet:') || link.startsWith('http')) {
            safeHref = link;
        } else if (link.startsWith('?xt=')) {
            safeHref = 'magnet:' + link;
        } else if (!link.startsWith('http') && !link.startsWith('magnet:')) {
            safeHref = 'https://' + link;
        }
        
        const linkDiv = document.createElement('div');
        linkDiv.className = 'download-link';
        linkDiv.innerHTML = `
            <div class="link-info">
                <div class="link-icon ${linkInfo.class}"><i class="${linkInfo.icon}"></i></div>
                <div><div class="link-name">${linkInfo.name} ${links.length > 1 ? `(#${index + 1})` : ''}</div></div>
            </div>
            <div style="display:flex;align-items:center;gap:15px">
                ${index === 0 ? '<span class="preferred-badge"><i class="fas fa-crown"></i> Preferito</span>' : ''}
                <a href="${safeHref}" target="_blank" rel="noopener noreferrer" class="link-btn"><i class="fas fa-external-link-alt"></i> Download</a>
            </div>
        `;
        downloadLinks.appendChild(linkDiv);
    });
}

function getLinkInfo(link) {
    if (!link) return { name: 'Download', class: 'other', icon: 'fas fa-download', priority: 99 };
    if (link.startsWith('magnet:')) return { name: 'Torrent Magnet', class: 'magnet', icon: 'fas fa-magnet', priority: 1 };
    if (link.startsWith('http')) {
        try {
            const hostname = new URL(link).hostname.replace('www.', '');
            const siteKey = Object.keys(SITE_PRIORITY).find(key => hostname.includes(key));
            if (siteKey) return SITE_PRIORITY[siteKey];
            return { name: hostname.split('.')[0].charAt(0).toUpperCase() + hostname.split('.')[0].slice(1), class: 'direct', icon: 'fas fa-download', priority: 3 };
        } catch(e) { return { name: 'Download', class: 'other', icon: 'fas fa-download', priority: 4 }; }
    }
    return { name: 'Download', class: 'other', icon: 'fas fa-download', priority: 5 };
}

function switchSource(source) {
    document.querySelectorAll('.source-btn').forEach(btn => btn.classList.remove('active'));
    const sourceBtn = document.getElementById(`source-${source}`);
    if (sourceBtn) sourceBtn.classList.add('active');
    
    currentSource = source;
    currentFilter = "all";
    currentPage = 1;
    
    const allCategoryBtn = document.querySelector('.category-btn');
    if (allCategoryBtn) {
        document.querySelectorAll('.category-btn').forEach(b => b.classList.remove('active'));
        allCategoryBtn.classList.add('active');
    }
    
    processGamesData();
    updateStats();
    renderGames();
    setupCategories();
}

function updateSizeValue(value) {
    if (!sizeValue) return;
    sizeValue.textContent = value == maxFileSize ? 'Tutti' : `Fino a ${value} GB`;
}

function closeModal() {
    if (gameDetailsModal) gameDetailsModal.style.display = 'none';
    document.body.style.overflow = 'auto';
}

function showErrorMessage(message) {
    if (gamesContainer) {
        gamesContainer.innerHTML = `<div class="loading"><i class="fas fa-exclamation-triangle"></i><h3>Errore</h3><p>${message}</p></div>`;
    }
}

function toggleFAQ() {
    if (faqSection) faqSection.classList.toggle('active');
}

function renderTrendingGames() {
    const trendingGrid = document.getElementById('trending-grid');
    if (!trendingGrid) return;
    
    const allGames = [
        ...window.allGamesData.steamrip.slice(0, 8),
        ...window.allGamesData.onlinefix.slice(0, 4),
        ...window.allGamesData.fitgirl.slice(0, 4)
    ].slice(0, 12);
    
    trendingGrid.innerHTML = '';
    allGames.forEach(game => {
        const card = document.createElement('div');
        card.className = 'game-card';
        const imageUrl = getGameImageUrl(game);
        card.innerHTML = `
            <div class="game-image" style="background-image: url('${imageUrl}'); background-size: cover; background-position: center;">
                <div class="game-source-badge ${game.source}-badge-card">${game.source.toUpperCase()}</div>
            </div>
            <div class="game-info">
                <h3 class="game-title">${cleanDisplayTitle(getGameTitle(game))}</h3>
                <div class="game-meta">
                    <span><i class="fas fa-hdd"></i> ${getFormattedFileSize(game)}</span>
                </div>
            </div>
        `;
        card.addEventListener('click', () => showGameDetails(game));
        trendingGrid.appendChild(card);
    });
}

function setupEventListeners() {
    if (searchBox) searchBox.addEventListener('input', () => { currentPage = 1; renderGames(); });
    
    sortButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            sortButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            currentSort = this.dataset.sort;
            currentPage = 1;
            renderGames();
        });
    });
    
    if (sizeSlider) sizeSlider.addEventListener('input', function() { updateSizeValue(this.value); currentPage = 1; renderGames(); });
    
    if (viewGridBtn) viewGridBtn.addEventListener('click', () => {
        viewGridBtn.classList.add('active');
        viewListBtn?.classList.remove('active');
        currentView = 'grid';
        if (gamesContainer) gamesContainer.classList.remove('list-view');
    });
    
    if (viewListBtn) viewListBtn.addEventListener('click', () => {
        viewListBtn.classList.add('active');
        viewGridBtn?.classList.remove('active');
        currentView = 'list';
        if (gamesContainer) gamesContainer.classList.add('list-view');
    });
    
    if (sourceSteamripBtn) sourceSteamripBtn.addEventListener('click', () => switchSource('steamrip'));
    if (sourceOnlinefixBtn) sourceOnlinefixBtn.addEventListener('click', () => switchSource('onlinefix'));
    if (sourceFitgirlBtn) sourceFitgirlBtn.addEventListener('click', () => switchSource('fitgirl'));
    if (sourceAltroBtn) sourceAltroBtn.addEventListener('click', () => switchSource('altro'));
    
    if (closeModalBtn) closeModalBtn.addEventListener('click', closeModal);
    if (gameDetailsModal) gameDetailsModal.addEventListener('click', (e) => { if (e.target === gameDetailsModal) closeModal(); });
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape') closeModal(); });
    
    if (firstPageBtn) firstPageBtn.addEventListener('click', () => { if (currentPage > 1) { currentPage = 1; renderGames(); gamesContainer?.scrollIntoView({ behavior: 'smooth' }); } });
    if (prevPageBtn) prevPageBtn.addEventListener('click', () => { if (currentPage > 1) { currentPage--; renderGames(); gamesContainer?.scrollIntoView({ behavior: 'smooth' }); } });
    if (nextPageBtn) nextPageBtn.addEventListener('click', () => { if (currentPage < totalPages) { currentPage++; renderGames(); gamesContainer?.scrollIntoView({ behavior: 'smooth' }); } });
    if (lastPageBtn) lastPageBtn.addEventListener('click', () => { if (currentPage < totalPages) { currentPage = totalPages; renderGames(); gamesContainer?.scrollIntoView({ behavior: 'smooth' }); } });
    
    if (faqToggleBtn) faqToggleBtn.addEventListener('click', toggleFAQ);
    if (closeFaqBtn) closeFaqBtn.addEventListener('click', toggleFAQ);
}
