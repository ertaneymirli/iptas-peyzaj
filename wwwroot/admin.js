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
    yetkiKontrolEt();
    anaSayfaGoster();
};

async function anaSayfaGoster() {
    const template =
        document.getElementById("anaSayfaTemplate");

    const pageContent =
        document.getElementById("pageContent");

    pageContent.innerHTML = template.innerHTML;

    const hosGeldin =
        document.getElementById("hosGeldinKullanici");

    if (hosGeldin) {
        hosGeldin.textContent =
            `Hoş geldin, ${kullanici.kullaniciAdi}`;
    }

    await anaSayfaSayilariGetir();
}

let anaSayfaMusteriler = [];
let anaSayfaBakimlar = [];

async function anaSayfaSayilariGetir() {
    const response =
        await apiFetch("/api/BakimPlanlari/dashboard");

    if (!response.ok) {
        console.error(
            "Dashboard hatası:",
            await response.text()
        );
        return;
    }

    const sonuc = await response.json();
    const toplamMusteriEl =
        document.getElementById("toplamMusteri");

    const aktifBakimEl =
        document.getElementById("aktifBakim");

    const bekleyenIsEl =
        document.getElementById("bekleyenIs");

    // Kullanıcı başka menüye geçtiyse elementler artık yoktur
    if (
        !toplamMusteriEl ||
        !aktifBakimEl ||
        !bekleyenIsEl
    ) {
        return;
    }

    // Bunlar backend'de kullanıcı yetkisine göre filtrelenmiş olmalı
    anaSayfaMusteriler = sonuc.musteriler ?? [];
    anaSayfaBakimlar = sonuc.bakimlar ?? [];

    document.getElementById("toplamMusteri").textContent =
        anaSayfaMusteriler.length;

    document.getElementById("aktifBakim").textContent =
        anaSayfaBakimlar.filter(x =>
            x.durumKodu === "B" ||
            x.durumKodu === "E"
        ).length;

    document.getElementById("bekleyenIs").textContent =
        anaSayfaBakimlar.filter(x =>
            x.durumKodu === "B"
        ).length;
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
    if (sayfa.includes("musteri-kullanicisi/musteri-kullanicilari.html")) {
        musteriKullanicilariniGetir();
    }
}

function cikis() {
    localStorage.removeItem("kullanici");
    localStorage.removeItem("rol");
    localStorage.removeItem("token");

    window.location.replace("https://iptaspeyzaj.com.tr/");
}
const rol = localStorage.getItem("rol");

function yetkiKontrolEt() {
    if (rol === "1") {
        return;
    }

    document.querySelectorAll("[data-admin]").forEach(x => {
        x.style.display = "none";
    });
}