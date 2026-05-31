let personelListesi = [];
let aktifPersonelListesi = [];
let personelSayfaNo = 1;
let personelSayfaBoyutu = 10;

async function personelSayfasiGetir() {
    const response = await apiFetch("/api/Personeller");

    if (!response.ok) {
        alert("Personeller alınamadı.");
        return;
    }

    personelListesi = await response.json();
    personelSayfaNo = 1;
    personelleriTabloyaBas(personelListesi);
}

async function personelSayfasiDurumaGoreGetir(durumKodu) {
    const response = await apiFetch(`/api/Personeller/durum/${durumKodu}`);

    if (!response.ok) {
        alert("Personeller alınamadı.");
        return;
    }

    personelListesi = await response.json();
    personelSayfaNo = 1;
    personelleriTabloyaBas(personelListesi);
}

function personelleriTabloyaBas(liste) {
    aktifPersonelListesi = liste;

    const tbody = document.getElementById("personelListe");
    if (!tbody) return;

    tbody.innerHTML = "";

    const baslangic = (personelSayfaNo - 1) * personelSayfaBoyutu;
    const bitis = baslangic + personelSayfaBoyutu;
    const sayfaListesi = liste.slice(baslangic, bitis);

    sayfaListesi.forEach(p => {
        const isPasif = p.durumKodu === "P";

        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>
                <div class="personel-actions">
                    <button class="btn-edit" onclick="personelDuzenle('${p.docId}')">✏️</button>

                    ${isPasif
                ? `<button class="btn-active" onclick="personelDurumGuncelle('${p.docId}', 'A')">Aktif Et</button>`
                : `<button class="btn-delete" onclick="personelDurumGuncelle('${p.docId}', 'P')">Pasif Yap</button>`
            }
                </div>
            </td>
            <td>${p.id ?? ""}</td>
            <td>${p.personelNo ?? ""}</td>
            <td>${p.ad ?? ""} ${p.soyad ?? ""}</td>
            <td>${p.telefon ?? ""}</td>
            <td>${p.gorev ?? ""}</td>
            <td>${p.durumKodu === "P" ? "Pasif" : "Aktif"}</td>
        `;

        tbody.appendChild(tr);
    });

    personelPagerBas();
}

async function personelPopupAc() {

    const response = await apiFetch(
        "/pages/personeller/personel-ekle.html"
    );

    const html = await response.text();

    document.getElementById(
        "personelPopupIcerik"
    ).innerHTML = html;

    document.getElementById(
        "personelPopupBaslik"
    ).textContent = "Yeni Personel";

    document.getElementById(
        "personelPopup"
    ).classList.remove("hidden");
}

function personelPopupKapat() {
    document.getElementById("personelPopup").classList.add("hidden");
    document.getElementById("personelForm").reset();
}

async function personelDuzenle(docId) {
    const response = await apiFetch(
        "/pages/personeller/personel-ekle.html"
    );

    const html = await response.text();

    document.getElementById(
        "personelPopupIcerik"
    ).innerHTML = html;
}

document.addEventListener("submit", async function (e) {
    if (e.target && e.target.id === "personelForm") {
        e.preventDefault();

        const docId = document.getElementById("personelDocId").value;

        const personel = {
            id: Number(document.getElementById("personelId").value),
            personelNo: Number(document.getElementById("personelNo").value),
            ad: document.getElementById("personelAd").value.trim(),
            soyad: document.getElementById("personelSoyad").value.trim(),
            telefon: document.getElementById("personelTelefon").value.trim(),
            gorev: document.getElementById("personelGorev").value.trim()
        };

        const url = docId
            ? `/api/Personeller/${docId}`
            : "/api/Personeller";

        const method = docId ? "PUT" : "POST";

        const response = await apiFetch(url, {
            method,
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(personel)
        });

        if (!response.ok) {
            alert("Personel kaydedilemedi.");
            return;
        }

        personelPopupKapat();
        personelSayfasiGetir();
    }
});

async function personelDurumGuncelle(docId, durumKodu) {

    const mesaj =
        durumKodu === "P"
            ? "Personel pasif yapılsın mı?"
            : "Personel aktif yapılsın mı?";

    if (!confirm(mesaj))
        return;

    const response = await apiFetch(
        `/api/Personeller/${docId}/durum`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                durumKodu: durumKodu
            })
        });

    if (!response.ok) {
        alert("Durum güncellenemedi.");
        return;
    }

    // 🔥 TABLOYU YENİLE

    const pasifListeAcikMi =
        document.querySelector(".btn-active");

    if (durumKodu === "P") {

        // aktiflerden pasife geçtiyse
        await personelSayfasiGetir();

    } else {

        // pasiften aktife geçtiyse
        await personelSayfasiDurumaGoreGetir("P");
    }
}

function personelAra() {
    const arama = document.getElementById("personelArama").value.toLowerCase().trim();

    if (!arama) {
        personelSayfaNo = 1;
        personelleriTabloyaBas(personelListesi);
        return;
    }

    const filtreli = personelListesi.filter(p =>
        (p.id?.toString() || "").includes(arama) ||
        (p.personelNo?.toString() || "").includes(arama) ||
        (`${p.ad ?? ""} ${p.soyad ?? ""}`).toLowerCase().includes(arama) ||
        (p.telefon ?? "").toLowerCase().includes(arama) ||
        (p.gorev ?? "").toLowerCase().includes(arama)
    );

    personelSayfaNo = 1;
    personelleriTabloyaBas(filtreli);
}

function personelPagerBas() {
    const pager = document.getElementById("personelPager");
    if (!pager) return;

    const toplamSayfa = Math.ceil(aktifPersonelListesi.length / personelSayfaBoyutu);

    if (toplamSayfa <= 1) {
        pager.innerHTML = "";
        return;
    }

    let html = "";

    html += `
        <button onclick="personelSayfaDegistir(1)" ${personelSayfaNo === 1 ? "disabled" : ""}>
            <<
        </button>
    `;

    html += `
        <button onclick="personelSayfaDegistir(${personelSayfaNo - 1})" ${personelSayfaNo === 1 ? "disabled" : ""}>
            <
        </button>
    `;

    let baslangic = Math.max(1, personelSayfaNo - 5);
    let bitis = Math.min(toplamSayfa, personelSayfaNo + 5);

    if (personelSayfaNo <= 5) {
        baslangic = 1;
        bitis = Math.min(10, toplamSayfa);
    }

    if (personelSayfaNo > toplamSayfa - 5) {
        baslangic = Math.max(1, toplamSayfa - 9);
        bitis = toplamSayfa;
    }

    if (baslangic > 1) {
        html += `<span class="pager-dots">...</span>`;
    }

    for (let i = baslangic; i <= bitis; i++) {
        html += `
            <button
                class="${personelSayfaNo === i ? "active-page" : ""}"
                onclick="personelSayfaDegistir(${i})">
                ${i}
            </button>
        `;
    }

    if (bitis < toplamSayfa) {
        html += `<span class="pager-dots">...</span>`;
    }

    html += `
        <button onclick="personelSayfaDegistir(${personelSayfaNo + 1})" ${personelSayfaNo === toplamSayfa ? "disabled" : ""}>
            >
        </button>
    `;

    html += `
        <button onclick="personelSayfaDegistir(${toplamSayfa})" ${personelSayfaNo === toplamSayfa ? "disabled" : ""}>
            >>
        </button>
    `;

    pager.innerHTML = html;
}

function personelSayfaDegistir(yeniSayfa) {
    const toplamSayfa = Math.ceil(aktifPersonelListesi.length / personelSayfaBoyutu);

    if (yeniSayfa < 1 || yeniSayfa > toplamSayfa) return;

    personelSayfaNo = yeniSayfa;
    personelleriTabloyaBas(aktifPersonelListesi);
}