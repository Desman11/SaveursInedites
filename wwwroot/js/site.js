
    const images = [
        { src: "/images/categorie/aperitif.png", alt: "Apéritif", link: "aperitif" },
        { src: "/images/categorie/soupe.webp", alt: "Soupe", link: "soupe" },
        { src: "/images/categorie/entrees.png", alt: "Entrées", link: "entrees" },
        { src: "/images/categorie/plats.png", alt: "Plats", link: "plats" },
        { src: "/images/categorie/desserts.png", alt: "Desserts", link: "desserts" },
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
    slideElements = []; // Réinitialise au cas où

        images.forEach((image, index) => {
            const div = document.createElement('div');
    div.className = getClassName(index);

    div.innerHTML = `
    <div class="img-wrap">
        <a href="/Recette/Recherche?categorie=${encodeURIComponent(image.link)}">
            <span class="img-text">${image.alt}</span>
            <img src="${image.src}" alt="${image.alt}">
        </a>
    </div>`;

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

        slideElements.forEach((element, index) => {
            element.className = getClassName(index);
        });
    }

    // Initialise le carrousel au chargement de la page
    window.onload = createCarousel;


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

// appel de la fonction quand on clique sur le bouton
let btnSearch = document.getElementById("btnrecherche");
if (btnSearch != null) {

    btnSearch.addEventListener("click", recherche);
}


// partie suppression
let suppressionButtons = document.getElementsByClassName("suppressionButton");
if (suppressionButtons.length > 0) {

    Array.from(suppressionButtons).forEach(button => {
        button.addEventListener("click", (e) => {

            let recetteName = e.target.parentElement.parentElement.parentElement.children[1].textContent;
            let rep = confirm("Voulez vous vraiment supprimer la recette suivant :\n" + recetteName);
            if (rep) {
                let form = e.target.parentElement;
                form.submit();
            }
        })
    });
}
function recherche() {
    // récupération de la valeur de la recherche
    let chaine = document.getElementById("recherche").value;

    // appel du serveur pour récupéré les livres correspondant à la recherche
    fetch("/recettes/recherche?recherche=" + chaine).then((reponse) => {
        return reponse.json();
    }).then((json) => {
        console.log(json);
        for (let i = 0; i < json.length; i++) {
            afficherRecette(json[i]);

        }
    })

}