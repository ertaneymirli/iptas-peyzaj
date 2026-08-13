const year = document.getElementById("year");

if (year) {
    year.textContent = new Date().getFullYear();
}

const revealEls =
    document.querySelectorAll(".reveal, .vine-divider");

if ("IntersectionObserver" in window) {
    const observer = new IntersectionObserver(
        (entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("in-view");
                    observer.unobserve(entry.target);
                }
            });
        },
        { threshold: 0.15 }
    );

    revealEls.forEach((el) => observer.observe(el));
} else {
    revealEls.forEach((el) =>
        el.classList.add("in-view")
    );
}

document
    .querySelectorAll(".nav-links-mobile a")
    .forEach((link) => {
        link.addEventListener("click", () => {
            const navToggle =
                document.getElementById("nav-toggle");

            if (navToggle) {
                navToggle.checked = false;
            }
        });
    });

/* Local ve canlı panel yönlendirmesi */
const localMi =
    window.location.hostname === "localhost" ||
    window.location.hostname === "127.0.0.1";

const girisAdresi = localMi
    ? "/login.html"
    : "https://panel.iptaspeyzaj.com.tr/login.html";

document
    .querySelectorAll(".panel-giris-link")
    .forEach((link) => {
        link.href = girisAdresi;
    });