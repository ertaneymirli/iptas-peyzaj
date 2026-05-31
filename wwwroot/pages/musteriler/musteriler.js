let musteriler = [];
let seciliMusteri = null;
let formMode = "ekle";

async function musterileriGetir() {
    const response = await apiFetch("/api/Musteriler");

    if (!response.ok) {
        alert("Müşteriler alınamadı.");
        return;
    }

    musteriler = await response.json();

    tabloyaBas(musteriler);
}
async function musteriEklePopupAc() {
    const response = await apiFetch("/pages/musteriler/musteri-ekle.html");
    const html = await response.text();

    document.getElementById("musteriEkleIcerik").innerHTML = html;
    document.getElementById("musteriEklePopup").classList.remove("hidden");

    musteriEkleFormHazirla();
}
async function pasifMusterileriGetir() {
    const response = await apiFetch("/api/Musteriler/durum/P");

    if (!response.ok) {
        alert("Pasif müşteriler alınamadı.");
        return;
    }

    musteriler = await response.json();
    tabloyaBas(musteriler);
}
function satirAktifEt(e, id) {
    e.stopPropagation();

    const musteri = musteriler.find(x => x.id === id);
    seciliMusteri = musteri;

    musteriAktifEt();
}

async function musteriAktifEt() {
    if (!seciliMusteri) {
        alert("Lütfen aktif yapmak için müşteri seç.");
        return;
    }

    if (!confirm(`${seciliMusteri.ad} ${seciliMusteri.soyad} tekrar aktif yapılsın mı?`)) {
        return;
    }

    const response = await apiFetch(`/api/Musteriler/${seciliMusteri.id}/durum`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            durumKodu: "A"
        })
    });

    if (response.ok) {
        alert("Müşteri aktif yapıldı.");
        seciliMusteri = null;
        pasifMusterileriGetir();
    } else {
        alert("Aktif etme işlemi başarısız.");
    }
}
function musteriEklePopupKapat() {
    document.getElementById("musteriEklePopup").classList.add("hidden");
    document.getElementById("musteriEkleIcerik").innerHTML = "";
}

function musteriDegistir() {
    if (!seciliMusteri) {
        alert("Lütfen listeden bir müşteri seç.");
        return;
    }

    alert("Değiştir ekranını sonra bağlayacağız.");
}

async function musteriSil() {
    if (!seciliMusteri) {
        alert("Lütfen pasif yapmak için müşteri seç.");
        return;
    }

    if (!confirm(`${seciliMusteri.ad} ${seciliMusteri.soyad} pasif yapılsın mı?`)) return;

    const response = await apiFetch(`/api/Musteriler/${seciliMusteri.id}/durum`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            durumKodu: "P"
        })
    });

    if (response.ok) {
        alert("Müşteri pasif yapıldı.");
        seciliMusteri = null;
        musterileriGetir();
    } else {
        alert("İşlem başarısız.");
    }
}

function tarihGoster(value) {
    if (!value) return "-";
    return new Date(value).toLocaleDateString("tr-TR");
}
async function musteriDegistir() {
    if (!seciliMusteri) {
        alert("Lütfen listeden müşteri seç.");
        return;
    }

    formMode = "duzenle";

    const response = await apiFetch("/pages/musteriler/musteri-ekle.html");
    const html = await response.text();

    document.getElementById("musteriEkleIcerik").innerHTML = html;
    document.getElementById("musteriEklePopup").classList.remove("hidden");
   
    musteriEkleFormHazirla();
    formuDoldur(seciliMusteri);
}

async function musteriEklePopupAc() {
    formMode = "ekle";

    const response = await apiFetch("/pages/musteriler/musteri-ekle.html");
    const html = await response.text();

    document.getElementById("musteriEkleIcerik").innerHTML = html;
    document.getElementById("musteriEklePopup").classList.remove("hidden");

    musteriEkleFormHazirla();
}

function formuDoldur(m) {
    document.getElementById("ad").value = m.ad || "";
    document.getElementById("soyad").value = m.soyad || "";
    document.getElementById("tc").value = m.tc || "";
    document.getElementById("telefon").value = m.telefon || "";
    document.getElementById("sehir").value = m.sehir || "";
    document.getElementById("mahalle").value = m.mahalle || "";
    document.getElementById("caddeSokak").value = m.caddeSokak || "";
    document.getElementById("no").value = m.no || "";
    document.getElementById("daire").value = m.daire || "";
    document.getElementById("cinsiyet").value = m.cinsiyet || "";
    document.getElementById("mekanTipi").value = m.mekanTipi || "";

    tarihSet("dogumTarihi", m.dogumTarihi);
    tarihSet("sozlesmeTarihi", m.sozlesmeTarihi);
    tarihSet("gorusmeTarihi", m.gorusmeTarihi);
    tarihSet("baslangicTarihi", m.baslangicTarihi);
    tarihSet("bitisTarihi", m.bitisTarihi);
    tarihSet("bakimTarihi", m.bakimTarihi);

    document.getElementById("periyodikBakim").value = m.periyodikBakim || "";
    document.getElementById("periyodikBakimTuru").value = m.periyodikBakimTuru || "";
    document.getElementById("aciklama").value = m.aciklama || "";
}

function tarihSet(id, value) {
    const input = document.getElementById(id);
    if (!input || !value) return;

    input.value = new Date(value).toISOString().split("T")[0];
}
function satirDuzenle(e, id) {
    e.stopPropagation();

    const musteri = musteriler.find(x => x.id === id);
    seciliMusteri = musteri;

    musteriDegistir();
}

function satirSil(e, id) {
    e.stopPropagation();

    const musteri = musteriler.find(x => x.id === id);
    seciliMusteri = musteri;

    musteriSil();
}
function tabloyaBas(liste) {
    const tbody = document.getElementById("musteriListe");
    tbody.innerHTML = "";

    liste.forEach(m => {
        const tr = document.createElement("tr");

        tr.onclick = function () {
            document.querySelectorAll("#musteriListe tr").forEach(x => x.classList.remove("selected"));
            tr.classList.add("selected");
            seciliMusteri = m;
        };

        const isPasif = m.durumKodu === "P";

        tr.innerHTML = `
            <td class="islem-btns">
                <button class="btn-edit" onclick="satirDuzenle(event, '${m.id}')">✏️</button>

                ${isPasif
                ? `<button class="btn-active" onclick="satirAktifEt(event, '${m.id}')">Aktif Et</button>`
                : `<button class="btn-delete" onclick="satirSil(event, '${m.id}')">Pasif Yap</button>`
            }
            </td>

            <td>${m.musteriNo ?? ""}</td>
            <td>${m.ad ?? ""} ${m.soyad ?? ""}</td>
            <td>${m.telefon ?? ""}</td>
            <td>${m.sehir ?? ""}</td>
            <td>${m.mekanTipi ?? ""}</td>
            <td>${tarihGoster(m.bakimTarihi)}</td>
        `;

        tbody.appendChild(tr);
    });
}