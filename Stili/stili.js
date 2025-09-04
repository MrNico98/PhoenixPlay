// Funzionalità FAQ
document.querySelectorAll('.faq-question').forEach(question => {
    question.addEventListener('click', () => {
        const item = question.parentNode;
        item.classList.toggle('faq-active');
        
        const icon = question.querySelector('i');
        if (item.classList.contains('faq-active')) {
            icon.classList.remove('fa-chevron-down');
            icon.classList.add('fa-chevron-up');
        } else {
            icon.classList.remove('fa-chevron-up');
            icon.classList.add('fa-chevron-down');
        }
    });
});

// Animazione di scroll per la navbar
window.addEventListener('scroll', () => {
    const navbar = document.querySelector('.navbar');
    if (window.scrollY > 50) {
        navbar.style.background = 'rgba(18, 18, 18, 0.95)';
        navbar.style.padding = '0.7rem 0';
    } else {
        navbar.style.background = 'rgba(18, 18, 18, 0.95)';
        navbar.style.padding = '1rem 0';
    }
});