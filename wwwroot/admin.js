const token = localStorage.getItem("token");

if (!token) {
    window.location.href = "index.html";
}
const kullanici = JSON.parse(localStorage.getItem("kullanici"));
async function apiFetch(url, options = {}) {

    const token = localStorage.getItem("token");

    options.headers = {
        ...(options.headers || {}),
        "Authorization": `Bearer ${token}`
    };

    const response = await fetch(url, options);

    // TOKEN GEÇERSİZSE
    if (response.status === 401) {

        localStorage.clear();

        alert("Oturum süresi doldu.");

        window.location.href = "index.html";
    }

    return response;
}

if (!kullanici) {
    window.location.href = "index.html";
}

window.onload = function () {
    anaSayfaGoster();
};

async function anaSayfaGoster() {
    document.getElementById("pageContent").innerHTML = `
        <div class="topbar">
            <div>
                <h1>Ana Sayfa</h1>
                <p>Hoş geldin, ${kullanici.kullaniciAdi}</p>
            </div>
        </div>

       <section class="cards">
    <div class="card" onclick="dashboardListeAc('musteri')">
        <h3>Toplam Müşteri</h3>
        <strong id="toplamMusteri">0</strong>
    </div>

    <div class="card" onclick="dashboardListeAc('aktif')">
        <h3>Aktif Bakım</h3>
        <strong id="aktifBakim">0</strong>
    </div>

    <div class="card" onclick="dashboardListeAc('bekleyen')">
        <h3>Bekleyen İş</h3>
        <strong id="bekleyenIs">0</strong>
    </div>
</section>

<div id="dashboardPopup" class="modal hidden">
    <div class="modal-box">
        <div class="modal-header">
            <h2 id="dashboardPopupBaslik">Liste</h2>
            <button type="button" onclick="dashboardPopupKapat()">✕</button>
        </div>

        <div id="dashboardPopupIcerik"></div>
    </div>
</div>
    `;

    await anaSayfaSayilariGetir();
}

let anaSayfaMusteriler = [];
let anaSayfaBakimlar = [];

async function anaSayfaSayilariGetir() {
    const musteriResponse = await apiFetch("/api/Musteriler");
    const bakimResponse = await apiFetch("/api/BakimPlanlari");


    anaSayfaMusteriler = musteriResponse.ok ? await musteriResponse.json() : [];
    anaSayfaBakimlar = bakimResponse.ok ? await bakimResponse.json() : [];

    const toplamMusteriEl = document.getElementById("toplamMusteri");
    const aktifBakimEl = document.getElementById("aktifBakim");
    const bekleyenIsEl = document.getElementById("bekleyenIs");

    if (!toplamMusteriEl || !aktifBakimEl || !bekleyenIsEl) {
        return;
    }

    toplamMusteriEl.textContent = anaSayfaMusteriler.length;

    aktifBakimEl.textContent =
        anaSayfaBakimlar.filter(x => x.durumKodu === "B" || x.durumKodu === "E").length;

    bekleyenIsEl.textContent =
        anaSayfaBakimlar.filter(x => x.durumKodu === "B").length;

    document.getElementById("aktifBakim").textContent =
        anaSayfaBakimlar.filter(x => x.durumKodu === "B" || x.durumKodu === "E").length;

    document.getElementById("bekleyenIs").textContent =
        anaSayfaBakimlar.filter(x => x.durumKodu === "B").length;
}

function dashboardListeAc(tip) {
    const baslik = document.getElementById("dashboardPopupBaslik");
    const icerik = document.getElementById("dashboardPopupIcerik");

    let liste = [];

    if (tip === "musteri") {
        baslik.textContent = "Toplam Müşteri Listesi";
        liste = anaSayfaMusteriler;

        icerik.innerHTML = liste.map(x => `
            <div class="dashboard-list-item">
                <b>${x.ad ?? ""} ${x.soyad ?? ""}</b>
                <span>${x.telefon ?? "-"}</span>
            </div>
        `).join("");
    }

    if (tip === "aktif") {
        baslik.textContent = "Aktif Bakım Listesi";
        liste = anaSayfaBakimlar.filter(x => x.durumKodu === "B" || x.durumKodu === "E");

        icerik.innerHTML = liste.map(x => `
            <div class="dashboard-list-item">
                <b>${x.adSoyad ?? "-"}</b>
                <span>${x.telefon ?? "-"}</span>
                <small>${tarihGoster(x.bakimTarihi)} - ${durumText(x.durumKodu)}</small>
            </div>
        `).join("");
    }

    if (tip === "bekleyen") {
        baslik.textContent = "Bekleyen İş Listesi";
        liste = anaSayfaBakimlar.filter(x => x.durumKodu === "B");

        icerik.innerHTML = liste.map(x => `
            <div class="dashboard-list-item">
                <b>${x.adSoyad ?? "-"}</b>
                <span>${x.telefon ?? "-"}</span>
                <small>${tarihGoster(x.bakimTarihi)} - Bekleyen</small>
            </div>
        `).join("");
    }

    if (liste.length === 0) {
        icerik.innerHTML = "<p>Kayıt bulunamadı.</p>";
    }

    document.getElementById("dashboardPopup").classList.remove("hidden");
}

function dashboardPopupKapat() {
    document.getElementById("dashboardPopup").classList.add("hidden");
}

async function sayfaYukle(sayfa) {
    const response = await fetch(sayfa);
    const html = await response.text();

    document.getElementById("pageContent").innerHTML = html;

    if (sayfa.includes("musteriler/musteriler.html")) {
        musterileriGetir();
    }
    if (sayfa.includes("bakim-takvimi/bakim-takvimi.html")) {
        bakimlariGetir();
    }
    if (sayfa.includes("teklif/teklifler.html")) {
        teklifleriGetir();
    }
    if (sayfa.includes("personeller/personeller.html")) {
        personelSayfasiGetir();
    }
    if (sayfa.includes("kullanicilar/kullanicilar.html")) {
        kullaniciSayfasiGetir();
    }
}

function cikis() {
    localStorage.removeItem("kullanici");
    localStorage.removeItem("rol");
    localStorage.removeItem("token");
    window.location.href = "index.html";
}