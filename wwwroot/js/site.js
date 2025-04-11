
    //const images = [
    //    { src: "/images/categorie/aperitif.png", alt: "Apéritif", link: "aperitif" },
    //    { src: "/images/categorie/soupe.webp", alt: "Soupe", link: "soupe" },
    //    { src: "/images/categorie/entrees.png", alt: "Entrées", link: "entrees" },
    //    { src: "/images/categorie/plats.png", alt: "Plats", link: "plats" },
    //    { src: "/images/categorie/desserts.png", alt: "Desserts", link: "desserts" },
    //];

    //let selectedIndex = 3;
    //let slideElements = [];

    //function getClassName(index) {
    //    const relativeIndex = (index - selectedIndex + images.length) % images.length;
    //if (relativeIndex === 0) return "selected";
    //if (relativeIndex === 1) return "next";
    //if (relativeIndex === 2) return "nextRightSecond";
    //if (relativeIndex === images.length - 1) return "prev";
    //if (relativeIndex === images.length - 2) return "prevLeftSecond";
    //    return relativeIndex > 2 ? "hideRight" : "hideLeft";
    //}

    //function createCarousel() {
    //    const carousel = document.getElementById('carousel');
    //carousel.innerHTML = '';
    //slideElements = []; // Réinitialise au cas où

    //    images.forEach((image, index) => {
    //        const div = document.createElement('div');
    //div.className = getClassName(index);

    //div.innerHTML = `
    //<div class="img-wrap">
    //    <a href="/Recette/Recherche?categorie=${encodeURIComponent(image.link)}">
    //        <span class="img-text">${image.alt}</span>
    //        <img src="${image.src}" alt="${image.alt}">
    //    </a>
    //</div>`;

    //        carousel.appendChild(div);
    //        slideElements.push(div);
    //    });
    //}

    //function moveToSelected(direction) {
    //    if (direction === "next") {
    //        selectedIndex = (selectedIndex + 1) % images.length;
    //    } else if (direction === "prev") {
    //        selectedIndex = selectedIndex === 0 ? images.length - 1 : selectedIndex - 1;
    //    }

    //    slideElements.forEach((element, index) => {
    //        element.className = getClassName(index);
    //    });
    //}

    //// Initialise le carrousel au chargement de la page
    //window.onload = createCarousel;


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
let barreRecherche = document.getElementById("recherche")
// Événement pour l'affichage en temps réel à chaque frappe
if (barreRecherche != null) {
    barreRecherche.addEventListener("input", () => {
        recherche(); // Affiche les résultats à chaque frappe
    });
}
//// Événement pour l'affichage en temps réel à chaque frappe
//document.getElementById("recherche").addEventListener("input", () => {
//    recherche(); // Affiche les résultats à chaque frappe
//});

//// Événement pour déclencher la recherche en appuyant sur Entrée
//document.getElementById("recherche").addEventListener("keydown", (event) => {
//    if (event.key === "Enter") {
//        event.preventDefault(); // Empêche le comportement par défaut (soumission ou rechargement)
//        recherche(); // Effectue la recherche sur la touche Entrée
//    }
//});

function recherche() {
    let chaine = document.getElementById("recherche").value;

  
    fetch("/recettes/recherche?recherche=" + encodeURIComponent(chaine))
        .then((reponse) => reponse.json())
        .then((json) => {
            const conteneur = document.getElementById("recettes");
            conteneur.innerHTML = ""; 
            json.forEach(afficherRecette);
        })
        .catch((erreur) => {
            console.error("Erreur lors de la récupération des recettes :", erreur);
        });

    function afficherRecette(recette) {
        let lien = document.createElement("a");
        lien.href = "/Recettes/Detail/" + recette.id;
        lien.style.textDecoration = "none";
        lien.style.color = "inherit";

        let div = document.createElement("div");
        div.classList.add("carte-recette");

        let nom = document.createElement("h2");
        nom.textContent = recette.nom;

        let img = document.createElement("img");
        img.src = recette.photo;
        img.alt = recette.nom;
        img.classList.add("image-recette");

        let description = document.createElement("p");
        description.textContent = recette.description;

        div.append(nom, img, description);
        lien.appendChild(div); 
        document.getElementById("recettes").appendChild(lien);
    }
   
}
// partie suppression
let suppressionButtonsU = document.getElementsByClassName("suppressionButtonU");
if (suppressionButtonsU.length > 0) {

    Array.from(suppressionButtonsU).forEach(button => {
        button.addEventListener("click", (e) => {

            let utilisateurName = e.target.parentElement.parentElement.parentElement.children[1].textContent;
            let rep = confirm("Voulez vous vraiment supprimer l'utilisateur suivant :\n" + utilisateurIdentifiant);
            if (rep) {
                let form = e.target.parentElement;
                form.submit();
            }
        })
    });
}