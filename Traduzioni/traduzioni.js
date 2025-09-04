// Traduzioni
const translations = {
    it: {
        title: "Il primo launcher di giochi gratuito italiano",
        subtitle: "Scarica, installa e gioca senza pubblicità, con integrazione Steam Tools e tracciamento del tempo di gioco.",
        downloadNow: "Scarica Ora",
        discoverMore: "Scopri di più",
        whyChoose: "Perché scegliere Phoenix Play?",
        directDownloads: "Download Diretti",
        directDownloadsDesc: "Nessuna pubblicità, download veloci e disponibilità immediata di tutti i giochi.",
        autoInstall: "Installazione Automatica",
        autoInstallDesc: "Gestione automatica di installazione, avvio e disinstallazione dei giochi, proprio come Steam.",
        testedGames: "Giochi Testati",
        testedGamesDesc: "Tutti i giochi vengono testati per verificarne il funzionamento e la sicurezza prima di essere pubblicati.",
        steamIntegration: "Integrazione Steam Tools",
        steamIntegrationDesc: "Accesso a file .lua e .manifest attraverso la sezione dedicata Steam Tools.",
        timeTracking: "Tracciamento Tempo di Gioco",
        timeTrackingDesc: "Tieni traccia della durata totale delle tue sessioni di gioco.",
        personalLibrary: "Libreria Personale",
        personalLibraryDesc: "Organizza tutti i tuoi giochi in un'unica libreria, facile da navigare e personalizzare.",
        helpImportant: "Il tuo aiuto è importante!",
        helpDesc1: "Grazie a PixelDrain, tutti i download sono permanenti e senza limiti di velocità!",
        helpDesc2: "Vuoi aiutarci a mantenere questo servizio? Un caffè virtuale è il modo perfetto per sostenere il progetto!",
        shareNow: "Dona un caffè virtuale ☕",
        screenshots: "Screenshot",
        downloadTitle: "Scarica Phoenix Play",
        downloadDesc: "Scarica subito il launcher e inizia a giocare!",
        downloadForWindows: "Download per Windows",
        version: "Versione",
        faq1Question: "Phoenix Play è veramente gratuito?",
        faq1Answer: "Sì, Phoenix Play è completamente gratuito e senza pubblicità. Il nostro obiettivo è fornire un'esperienza di gaming senza costi nascosti.",
        faq2Question: "Como posso contribuire al progetto?",
        faq2Answer: "Puoi contribuire condividendo Phoenix Play con i tuoi amici, segnalando bug o suggerendo nuovi giochi da aggiungere alla piattaforma.",
        faq3Question: "I giochi sono sicuri e privi di virus?",
        faq3Answer: "Tutti i giochi vengono testati accuratamente per garantire la sicurezza e l'assenza di malware prima di essere pubblicati sulla piattaforma.",
        footerDesc: "Il primo launcher di giochi gratuito italiano",
        usefulLinks: "Link Utili",
        features: "Caratteristiche",
        download: "Download",
        support: "Supporto",
        contacts: "Contatti",
        allRights: "Tutti i diritti riservati"
    },
    en: {
        title: "The first free Italian game launcher",
        subtitle: "Download, install and play without ads, with Steam Tools integration and gameplay time tracking.",
        downloadNow: "Download Now",
        discoverMore: "Discover More",
        whyChoose: "Why choose Phoenix Play?",
        directDownloads: "Direct Downloads",
        directDownloadsDesc: "No ads, fast downloads and immediate availability of all games.",
        autoInstall: "Automatic Installation",
        autoInstallDesc: "Automatic management of game installation, launch and uninstallation, just like Steam.",
        testedGames: "Tested Games",
        testedGamesDesc: "All games are tested to verify their functionality and safety before being published.",
        steamIntegration: "Steam Tools Integration",
        steamIntegrationDesc: "Access to .lua and .manifest files through the dedicated Steam Tools section.",
        timeTracking: "Play Time Tracking",
        timeTrackingDesc: "Keep track of the total duration of your gaming sessions.",
        personalLibrary: "Personal Library",
        personalLibraryDesc: "Organize all your games in a single library, easy to navigate and customize.",
        helpImportant: "Your help is important!",
        helpDesc1: "Thanks to PixelDrain, all downloads are permanent and have no speed limits!",
        helpDesc2: "Want to help us maintain this service? A virtual coffee is the perfect way to support the project!",
        shareNow: "Donate a virtual coffee ☕",
        screenshots: "Screenshots",
        downloadTitle: "Download Phoenix Play",
        downloadDesc: "Download the launcher now and start playing!",
        downloadForWindows: "Download for Windows",
        version: "Version",
        faq1Question: "Is Phoenix Play really free?",
        faq1Answer: "Yes, Phoenix Play is completely free and without ads. Our goal is to provide a gaming experience with no hidden costs.",
        faq2Question: "How can I contribute to the project?",
        faq2Answer: "You can contribute by sharing Phoenix Play with your friends, reporting bugs or suggesting new games to add to the platform.",
        faq3Question: "Are the games safe and virus-free?",
        faq3Answer: "All games are thoroughly tested to ensure safety and absence of malware before being published on the platform.",
        footerDesc: "The first free Italian game launcher",
        usefulLinks: "Useful Links",
        features: "Features",
        download: "Download",
        support: "Support",
        contacts: "Contacts",
        allRights: "All rights reserved"
    }
};

// Funzionalità cambio lingua
document.addEventListener('DOMContentLoaded', function() {
    const languageButtons = document.querySelectorAll('.language-btn');
    let currentLang = 'it';
    
    languageButtons.forEach(button => {
        button.addEventListener('click', function() {
            const lang = this.getAttribute('data-lang');
            if (lang !== currentLang) {
                currentLang = lang;
                switchLanguage(lang);
                
                // Aggiorna pulsanti attivi
                languageButtons.forEach(btn => btn.classList.remove('active'));
                this.classList.add('active');
            }
        });
    });
    
    function switchLanguage(lang) {
        document.querySelectorAll('[data-translate]').forEach(element => {
            const key = element.getAttribute('data-translate');
            if (translations[lang][key]) {
                element.textContent = translations[lang][key];
            }
        });
    }
});