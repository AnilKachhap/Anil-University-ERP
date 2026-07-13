
// ================================
// Anil University ERP - site.js
// ================================

document.addEventListener("DOMContentLoaded", function () {

    // ============================
    // Sidebar Toggle
    // ============================

    const menuBtn = document.getElementById("menuBtn");
    const sidebar = document.getElementById("sidebar");

    if (menuBtn && sidebar) {
        menuBtn.addEventListener("click", function () {
            sidebar.classList.toggle("show");
        });
    }

    // ============================
    // Auto Active Menu
    // ============================

    const currentUrl = window.location.pathname.toLowerCase();

    document.querySelectorAll(".sidebar a").forEach(link => {

        const href = link.getAttribute("href");

        if (href && currentUrl.includes(href.toLowerCase())) {

            document
                .querySelectorAll(".sidebar a")
                .forEach(x => x.classList.remove("active"));

            link.classList.add("active");
        }

    });

    // ============================
    // Counter Animation
    // ============================

    document.querySelectorAll(".counter").forEach(counter => {

        const target = parseInt(counter.innerText);

        if (isNaN(target)) return;

        counter.innerText = 0;

        let count = 0;

        const speed = target / 80;

        const update = () => {

            count += speed;

            if (count < target) {

                counter.innerText = Math.floor(count);

                requestAnimationFrame(update);

            }
            else {

                counter.innerText = target;

            }

        };

        update();

    });

    // ============================
    // Fade Animation
    // ============================

    document.querySelectorAll(".card").forEach(card => {

        card.style.opacity = "0";
        card.style.transform = "translateY(20px)";

        setTimeout(() => {

            card.style.transition = ".5s";
            card.style.opacity = "1";
            card.style.transform = "translateY(0px)";

        }, 150);

    });

    // ============================
    // Back To Top Button
    // ============================

    const topBtn = document.createElement("button");

    topBtn.innerHTML = '<i class="fa-solid fa-arrow-up"></i>';

    topBtn.className = "btn btn-primary";

    topBtn.style.position = "fixed";
    topBtn.style.right = "20px";
    topBtn.style.bottom = "20px";
    topBtn.style.display = "none";
    topBtn.style.zIndex = "999";
    topBtn.style.borderRadius = "50%";
    topBtn.style.width = "50px";
    topBtn.style.height = "50px";

    document.body.appendChild(topBtn);

    window.addEventListener("scroll", function () {

        if (window.scrollY > 250) {

            topBtn.style.display = "block";

        }
        else {

            topBtn.style.display = "none";

        }

    });

    topBtn.addEventListener("click", function () {

        window.scrollTo({

            top: 0,
            behavior: "smooth"

        });

    });

});


// ========================================
// Bootstrap Tooltips
// ========================================

const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));

tooltipTriggerList.map(function (tooltipTriggerEl) {

    return new bootstrap.Tooltip(tooltipTriggerEl);

});


// ========================================
// Auto Hide Alerts
// ========================================

setTimeout(function () {

    document.querySelectorAll(".alert").forEach(function (alert) {

        alert.style.transition = ".5s";
        alert.style.opacity = "0";

        setTimeout(() => alert.remove(), 500);

    });

}, 4000);


// ========================================
// Loading Spinner Helper
// ========================================

function showLoader() {

    document.body.classList.add("loading");

}

function hideLoader() {

    document.body.classList.remove("loading");

}
```
