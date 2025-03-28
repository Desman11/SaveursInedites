
const images = [
    { src: "./images/Catégories/cat/12.png", alt: "Apéritif", link: "Apéritif" },
    { src: "./images/Catégories/cat/soupe-potimarron.webp", alt: "Soupe", link: "#Soupe" },
    { src: "./images/Catégories/cat/6.png", alt: "Entrées", link: "entrées" },
    { src: "./images/Catégories/cat/11.png", alt: "Plats", link: "Plats" },
    { src: "./images/Catégories/cat/2.png", alt: "Desserts", link: "Desserts" },
];

let selectedIndex = 3;
let slideElements = [];

function getClassName(index) {
    const relativeIndex = (index - selectedIndex + images.length) % images.length;
    if (relativeIndex === 0) return "selected";
    if (relativeIndex === 1) return "next";
    if (relativeIndex === 2) return "nextRightSecond";
    if (relativeIndex === images.length - 1) return "prev";
    if (relativeIndex === images.length - 2) return "prevLeftSecond";
    return relativeIndex > 2 ? "hideRight" : "hideLeft";
}

function createCarousel() {
    const carousel = document.getElementById('carousel');
    carousel.innerHTML = '';

    images.forEach((image, index) => {
        const div = document.createElement('div');
        div.className = getClassName(index);

        div.innerHTML = `
            <div class="img-wrap">
                <span class="img-text">${image.alt}</span>
                <img src="${image.src}" alt="${image.alt}">
            </div>   `;

        carousel.appendChild(div);
        slideElements.push(div);
    });
}

function moveToSelected(direction) {
    if (direction === "next") {
        selectedIndex = (selectedIndex + 1) % images.length;
    } else if (direction === "prev") {
        selectedIndex = selectedIndex === 0 ? images.length - 1 : selectedIndex - 1;
    }

    // Update classes for smooth transition
    slideElements.forEach((element, index) => {
        element.className = getClassName(index);
    });
}

// Initial creation
createCarousel();

//menu
function toggleDropdown(event, dropdownId) {
    event.preventDefault();
    var dropdown = document.getElementById(dropdownId);
    dropdown.style.display = dropdown.style.display === 'block' ? 'none' : 'block';
}

//mes recettes
function toggleText(event, id) {
    event.preventDefault(); // Empêche l'effet sur le formulaire

    // Récupère tous les textes associés aux boutons de coût
    let textElements = document.querySelectorAll('.containermr .textmr');

    // Cache tous les textes sauf celui que l'on souhaite afficher
    textElements.forEach(el => {
        if (el.id !== id) {
            el.style.display = 'none';
        }
    });

    // Affiche ou cache uniquement le texte correspondant au bouton cliqué
    let selectedText = document.getElementById(id);
    if (selectedText.style.display === 'block') {
        selectedText.style.display = 'none';
    } else {
        selectedText.style.display = 'block';
    }
}


