let bakimlar = [];
let seciliBakim = null;
let tumBakimListesi = [];
let aktifBakimListesi = [];
let siralamaKolon = "";
let siralamaYon = "asc"; // asc | desc
let personeller = [];
let seciliPersoneller = [];
let bakimSayfaNo = 1;
let bakimSayfaBoyutu = 10;
let bakimPagerListesi = [];

async function bakimlariGetir() {
    const response = await apiFetch("/api/BakimPlanlari");

    if (!response.ok) {
        const hata = await response.text();
        console.error("Bakım listesi hatası:", hata);
        alert("Bakım listesi alınamadı.");
        return;
    }

    bakimlar = await response.json();
    tumBakimListesi = bakimlar;
    aktifBakimListesi = bakimlar;
    bakimSayfaNo = 1;
    bakimlariTabloyaBas(bakimlar);
}

async function bakimlariDurumaGoreGetir(durumKodu) {
    if (!bakimlar || bakimlar.length === 0) {
        await bakimlariGetir();
    }

    seciliBakimDurumu = durumKodu;

    const filtreliListe = bakimlar.filter(function (bakim) {
        return bakim.durumKodu === durumKodu;
    });

    aktifBakimListesi = filtreliListe;
    bakimSayfaNo = 1;
    bakimlariTabloyaBas(filtreliListe);
}
var seciliBakimDurumu = null;

function kullaniciRolunuGetir() {
    const dogrudanRol = localStorage.getItem("rol");

    if (dogrudanRol !== null && dogrudanRol !== "") {
        return Number(dogrudanRol);
    }

    const kullaniciJson = localStorage.getItem("kullanici");

    if (!kullaniciJson) {
        return null;
    }

    const kullanici = JSON.parse(kullaniciJson);
    return Number(kullanici.rol ?? kullanici.Rol);
}

function yoneticiMi() {
    try {
        return kullaniciRolunuGetir() === 1;
    } catch (e) {
        console.error("Kullanıcı rolü okunamadı:", e);
        return false;
    }
}

function yoneticiYetkiKontrolu() {
    if (yoneticiMi()) {
        return true;
    }

    alert("Bu işlem için yönetici yetkiniz bulunmuyor.");
    return false;
}
function bakimFiltresiSec(durumKodu, buton) {
    seciliBakimDurumu = durumKodu;

    document
        .querySelectorAll(".bakim-filters button")
        .forEach(function (item) {
            item.classList.remove("active");
        });

    buton.classList.add("active");

    // Sekme değiştirilince aramayı temizlemek istersen:
    document.getElementById("bakimArama").value = "";

    if (durumKodu) {
        bakimlariDurumaGoreGetir(durumKodu);
    } else {
        bakimlariGetir();
    }
}

function bakimlariTabloyaBas(liste) {
    bakimPagerListesi = liste;

    const tbody = document.getElementById("bakimListe");
    const templateElement =
        document.getElementById("bakimSatirTemplate");

    if (!tbody || !templateElement) {
        return;
    }

    tbody.innerHTML = "";
    seciliBakim = null;

    const baslangic =
        (bakimSayfaNo - 1) * bakimSayfaBoyutu;

    const bitis =
        baslangic + bakimSayfaBoyutu;

    const sayfaListesi =
        liste.slice(baslangic, bitis);

    sayfaListesi.forEach(function (bakim) {
        const parca =
            templateElement.content.cloneNode(true);

        const satir =
            parca.querySelector("tr");

        satir.dataset.id = bakim.id;

        satir
            .querySelectorAll("[data-bind-text]")
            .forEach(function (alan) {
                const alanAdi =
                    alan.dataset.bindText;

                const formatterAdi =
                    alan.dataset.formatter;

                let deger =
                    bakim[alanAdi];

                if (
                    formatterAdi &&
                    typeof window[formatterAdi] === "function"
                ) {
                    deger =
                        window[formatterAdi](deger);
                }

                alan.textContent =
                    deger ?? "";

                const classFormatterAdi =
                    alan.dataset.classFormatter;

                if (
                    classFormatterAdi &&
                    typeof window[classFormatterAdi] === "function"
                ) {
                    alan.className =
                        window[classFormatterAdi](
                            bakim[alanAdi]
                        );
                }
            });

        satir
            .querySelectorAll(".yonetici-islem")
            .forEach(function (buton) {
                buton.hidden = !yoneticiMi();
            });

        tbody.appendChild(parca);
    });

    bakimPagerBas();
}

function bakimSatirSec(satir, id) {
    document
        .querySelectorAll("#bakimListe tr")
        .forEach(function (item) {
            item.classList.remove("selected");
        });

    satir.classList.add("selected");

    seciliBakim = bakimlar.find(function (bakim) {
        return String(bakim.id) === String(id);
    }) ?? null;
}
function bakimPagerBas() {
    const pager = document.getElementById("bakimPager");
    if (!pager) return;

    const toplamSayfa = Math.ceil(bakimPagerListesi.length / bakimSayfaBoyutu);

    if (toplamSayfa <= 1) {
        pager.innerHTML = "";
        return;
    }

    let html = "";

    html += `
        <button onclick="bakimSayfaDegistir(1)" ${bakimSayfaNo === 1 ? "disabled" : ""}>
            <<
        </button>
    `;

    html += `
        <button onclick="bakimSayfaDegistir(${bakimSayfaNo - 1})" ${bakimSayfaNo === 1 ? "disabled" : ""}>
            <
        </button>
    `;

    let baslangic = Math.max(1, bakimSayfaNo - 5);
    let bitis = Math.min(toplamSayfa, bakimSayfaNo + 5);

    if (bakimSayfaNo <= 5) {
        baslangic = 1;
        bitis = Math.min(10, toplamSayfa);
    }

    if (bakimSayfaNo > toplamSayfa - 5) {
        baslangic = Math.max(1, toplamSayfa - 9);
        bitis = toplamSayfa;
    }

    if (baslangic > 1) {
        html += `<span class="pager-dots">...</span>`;
    }

    for (let i = baslangic; i <= bitis; i++) {
        html += `
            <button
                class="${bakimSayfaNo === i ? "active-page" : ""}"
                onclick="bakimSayfaDegistir(${i})">
                ${i}
            </button>
        `;
    }

    if (bitis < toplamSayfa) {
        html += `<span class="pager-dots">...</span>`;
    }

    html += `
        <button onclick="bakimSayfaDegistir(${bakimSayfaNo + 1})" ${bakimSayfaNo === toplamSayfa ? "disabled" : ""}>
            >
        </button>
    `;

    html += `
        <button onclick="bakimSayfaDegistir(${toplamSayfa})" ${bakimSayfaNo === toplamSayfa ? "disabled" : ""}>
            >>
        </button>
    `;

    pager.innerHTML = html;
}
function bakimSayfaDegistir(yeniSayfa) {
    const toplamSayfa = Math.ceil(bakimPagerListesi.length / bakimSayfaBoyutu);

    if (yeniSayfa < 1 || yeniSayfa > toplamSayfa) return;

    bakimSayfaNo = yeniSayfa;
    bakimlariTabloyaBas(bakimPagerListesi);
}
function bakimTamamlaPopupAc(id) {
    if (!yoneticiYetkiKontrolu()) return;

    const bakim = bakimlar.find(x => x.id == id);

    if (!bakim) return;

    // DURUM KONTROL
    if (bakim.durumKodu === "T") {
        alert("Bu bakım zaten tamamlanmış.");
        return;
    }

    if (bakim.durumKodu === "I") {
        alert("İptal edilen bakım tamamlanamaz.");
        return;
    }

    if (bakim.durumKodu === "E") {
        alert("Ertelenen bakım önce aktif hale getirilmeli.");
        return;
    }

    seciliBakim = bakim;

    document.getElementById("tamamlaPopup")
        .classList.remove("hidden");

    seciliPersoneller = [];

    document.getElementById("tamamlaForm").reset();

    personelleriGetir();
    seciliPersonelTabloBas();
}
function tamamlaPopupKapat() {
    document.getElementById("tamamlaPopup").classList.add("hidden");
    document.getElementById("tamamlaForm").reset();
    document.getElementById("tamamlaMesaj").innerHTML = "";
}

document.addEventListener("submit", async function (e) {
    if (e.target && e.target.id === "tamamlaForm") {
        e.preventDefault();

        if (!yoneticiYetkiKontrolu()) return;

        const formData = new FormData();

        // 🔥 PERSONEL KONTROL
        if (seciliPersoneller.length === 0) {
            alert("En az 1 personel seçmelisin.");
            return;
        }

        const personelIdleri = seciliPersoneller.map(x => x.personelId).join(",");
        formData.append("PersonelIdleri", personelIdleri);

        // 🔥 NOT KONTROL
        const not = document.getElementById("tamamlaNotu").value.trim();

        if (!not) {
            alert("İşlem notu boş olamaz.");
            return;
        }

        formData.append("IslemNotu", not);

        // 🔥 RESİMLER
        const oncesi = document.getElementById("oncesiResim").files[0];
        const sonrasi = document.getElementById("sonrasiResim").files[0];

        if (oncesi) formData.append("OncesiResim", oncesi);
        if (sonrasi) formData.append("SonrasiResim", sonrasi);

        const response = await apiFetch(`/api/BakimPlanlari/${seciliBakim.id}/tamamla`, {
            method: "PUT",
            body: formData
        });

        if (!response.ok) {
            const hata = await response.text();
            console.error("Tamamla hatası:", hata);
            alert(hata);
            return;
        }

        const mesaj = document.getElementById("tamamlaMesaj");

        mesaj.innerHTML = "Bakım tamamlandı ✅";

        setTimeout(() => {
            tamamlaPopupKapat();
            bakimlariGetir();
        }, 600);
    }
});

async function bakimIptalEt(id) {
    if (!yoneticiYetkiKontrolu()) return;

    const bakim = bakimlar.find(x => x.id == id);

    if (!bakim) return;

    // DURUM KONTROL
    if (bakim.durumKodu === "I") {
        alert("Bu bakım zaten iptal edilmiş.");
        return;
    }

    if (bakim.durumKodu === "T") {
        alert("Tamamlanan bakım iptal edilemez.");
        return;
    }

    const not = prompt("İptal nedeni:", "Bakım iptal edildi.");

    if (!not) return;

    await apiFetch(`/api/BakimPlanlari/${id}/durum`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            durumKodu: "I",
            islemNotu: not
        })
    });

    bakimlariGetir();
}

function bakimErtelePopupAc(id) {
    if (!yoneticiYetkiKontrolu()) return;

    const bakim = bakimlar.find(x => x.id == id);

    if (!bakim) return;

    if (bakim.durumKodu === "T") {
        alert("Tamamlanan bakım ertelenemez.");
        return;
    }

    if (bakim.durumKodu === "I") {
        alert("İptal edilen bakım ertelenemez.");
        return;
    }

    seciliBakim = bakim;

    document.getElementById("ertelePopup")
        .classList.remove("hidden");
}

function bakimErtelePopupKapat() {
    document.getElementById("ertelePopup").classList.add("hidden");
    document.getElementById("yeniBakimTarihi").value = "";
    document.getElementById("erteleNotu").value = "";
}

async function bakimErtele() {
    if (!yoneticiYetkiKontrolu()) return;

    const yeniTarih = document.getElementById("yeniBakimTarihi").value;
    const not = document.getElementById("erteleNotu").value;

    if (!yeniTarih) {
        alert("Yeni bakım tarihi seç.");
        return;
    }

    await apiFetch(`/api/BakimPlanlari/${seciliBakim.id}/ertele`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            yeniTarih: yeniTarih + "T00:00:00Z",
            islemNotu: not || "Bakım ertelendi."
        })
    });

    bakimErtelePopupKapat();
    bakimlariGetir();
}

function durumText(kod) {
    switch (kod) {
        case "B": return "Bekliyor";
        case "T": return "Tamamlandı";
        case "I": return "İptal";
        case "E": return "Ertelendi";
        default: return "-";
    }
}

function durumClass(kod) {
    switch (kod) {
        case "B": return "badge badge-wait";
        case "T": return "badge badge-success";
        case "I": return "badge badge-danger";
        case "E": return "badge badge-warning";
        default: return "badge";
    }
}

function tarihGoster(value) {
    if (!value) return "-";
    return new Date(value).toLocaleDateString("tr-TR");
}
function bakimAra() {
    const aramaMetni = document
        .getElementById("bakimArama")
        .value
        .trim()
        .toLocaleLowerCase("tr-TR");

    let kaynakListe = bakimlar;

    // Önce seçili sekmeye göre filtrele
    if (seciliBakimDurumu) {
        kaynakListe = kaynakListe.filter(function (bakim) {
            return bakim.durumKodu === seciliBakimDurumu;
        });
    }

    // Sonra arama metnine göre filtrele
    const filtrelenmisListe = kaynakListe.filter(function (bakim) {
        return (
            !aramaMetni ||
            String(bakim.musteriNo || "")
                .toLocaleLowerCase("tr-TR")
                .includes(aramaMetni) ||

            String(bakim.adSoyad || "")
                .toLocaleLowerCase("tr-TR")
                .includes(aramaMetni) ||

            String(bakim.telefon || "")
                .toLocaleLowerCase("tr-TR")
                .includes(aramaMetni) ||

            durumText(bakim.durumKodu)
                .toLocaleLowerCase("tr-TR")
                .includes(aramaMetni) ||

            String(bakim.aciklama || "")
                .toLocaleLowerCase("tr-TR")
                .includes(aramaMetni) ||

            String(bakim.islemNotu || "")
                .toLocaleLowerCase("tr-TR")
                .includes(aramaMetni)
        );
    });

    aktifBakimListesi = filtrelenmisListe;
    bakimSayfaNo = 1;
    bakimlariTabloyaBas(filtrelenmisListe);
}
function sirala(kolon) {

    if (siralamaKolon === kolon) {
        // aynı kolona tekrar basıldı → yön değiştir
        siralamaYon = siralamaYon === "asc" ? "desc" : "asc";
    } else {
        siralamaKolon = kolon;
        siralamaYon = "asc";
    }

    const liste = [...aktifBakimListesi];

    liste.sort((a, b) => {
        let valA = a[kolon];
        let valB = b[kolon];

        // null kontrol
        if (!valA) return 1;
        if (!valB) return -1;

        // tarih kontrol
        if (kolon === "bakimTarihi") {
            valA = new Date(valA);
            valB = new Date(valB);
        }

        // string ise küçült
        if (typeof valA === "string") {
            valA = valA.toLowerCase();
            valB = valB.toLowerCase();
        }

        if (valA > valB) return siralamaYon === "asc" ? 1 : -1;
        if (valA < valB) return siralamaYon === "asc" ? -1 : 1;

        return 0;
    });
    bakimSayfaNo = 1;
    bakimlariTabloyaBas(liste);
    baslikIconGuncelle(kolon);
}
function baslikIconGuncelle(kolon) {
    const thler = document.querySelectorAll("th[data-field]");

    thler.forEach(th => {
        const field = th.getAttribute("data-field");

        if (field === kolon) {
            th.innerHTML = th.innerText.split(" ")[0] + " " + (siralamaYon === "asc" ? "↑" : "↓");
        } else {
            th.innerHTML = th.innerText.split(" ")[0] + " ⬍";
        }
    });
}

async function personelleriGetir() {
    const response = await apiFetch("/api/Personeller");

    if (!response.ok) {
        alert("Personel listesi alınamadı.");
        return;
    }

    personeller = await response.json();

    const select = document.getElementById("personelSelect");
    if (!select) return;

    select.innerHTML = `<option value="">Personel seçiniz</option>`;

    personeller.forEach(p => {
        const option = document.createElement("option");

        option.value = p.id;
        option.textContent = `${p.personelNo} - ${p.ad} ${p.soyad}`;

        select.appendChild(option);
    });
}
function personelEkle() {
    const select = document.getElementById("personelSelect");
    const personelId = Number(select.value);

    if (!personelId) {
        alert("Lütfen personel seç.");
        return;
    }

    const personel = personeller.find(x => Number(x.id ?? x.ID) === personelId);

    if (!personel) {
        alert("Personel bulunamadı.");
        return;
    }

    const zatenVar = seciliPersoneller.some(x => x.personelId === personelId);

    if (zatenVar) {
        alert("Bu personel zaten eklendi.");
        return;
    }

    seciliPersoneller.push({
        personelId: personelId,
        adSoyad: `${personel.ad ?? personel.AD} ${personel.soyad ?? personel.SOYAD}`
    });

    seciliPersonelTabloBas();
}

function personelCikar(personelId) {
    seciliPersoneller = seciliPersoneller.filter(x => x.personelId !== personelId);
    seciliPersonelTabloBas();
}

function seciliPersonelTabloBas() {
    const tbody = document.getElementById("seciliPersonelListe");
    if (!tbody) return;

    tbody.innerHTML = "";

    seciliPersoneller.forEach(p => {
        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${p.personelId}</td>
            <td>${p.adSoyad}</td>
            <td>
                <button type="button" class="btn-delete" onclick="personelCikar(${p.personelId})">
                    Sil
                </button>
            </td>
        `;

        tbody.appendChild(tr);
    });
}
async function bakimDetayGoster(e, bakimId) {
    e.stopPropagation();

    const bakim = bakimlar.find(x => x.id == bakimId);

    const response = await apiFetch(`/api/BakimPlanlari/${bakimId}/detaylar`);

    let detaylar = [];

    if (response.ok) {
        detaylar = await response.json();
    }

    const icerik = document.getElementById("bakimDetayIcerik");

    let html = `
        <div class="detay-card">
            <h3>Bakım Bilgileri</h3>
            <p><b>Müşteri:</b> ${bakim?.adSoyad ?? "-"}</p>
            <p><b>Telefon:</b> ${bakim?.telefon ?? "-"}</p>
            <p><b>Durum:</b> ${durumText(bakim?.durumKodu)}</p>
            <p><b>Bakım Tarihi:</b> ${tarihGoster(bakim?.bakimTarihi)}</p>
            <p><b>Açıklama:</b> ${bakim?.aciklama ?? "-"}</p>
            <p><b>İşlem Notu:</b> ${bakim?.islemNotu ?? "-"}</p>
        </div>
    `;

    if (detaylar && detaylar.length > 0) {
        html += `
            <div class="detay-card">
                <h3>Görevli Personeller</h3>
                ${detaylar.map(d => `
                    <div class="personel-detay">
                        <p><b>Personel No:</b> ${d.personelNo ?? "-"}</p>
                        <p><b>Personel:</b> ${d.personelAdSoyad ?? d.personelAdi ?? "-"}</p>
                        <p><b>Resim Tipi:</b> ${d.resimTip ?? "Resim yok"}</p>

                ${d.resimUrl && d.resimUrl !== ""
                ? `<img src="${d.resimUrl}" class="bakim-resim" />`
                : `<p class="resim-yok">Fotoğraf eklenmemiş.</p>`
            }
                    </div>
                `).join("")}
            </div>
        `;
    } else {
        html += `
            <div class="detay-card">
                <h3>Görevli Personeller</h3>
                <p>Fotoğraf eklenmemiş olabilir.</p>
                <p><b>İşlem Notu:</b> ${bakim?.islemNotu ?? "-"}</p>
            </div>
        `;
    }

    icerik.innerHTML = html;

    document.getElementById("bakimDetayPopup").classList.remove("hidden");
}
function bakimDetayKapat() {
    document.getElementById("bakimDetayPopup").classList.add("hidden");
    document.getElementById("bakimDetayIcerik").innerHTML = "";
}
