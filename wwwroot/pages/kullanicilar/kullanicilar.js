let kullaniciListesi = [];
let aktifKullaniciListesi = [];
let kullaniciSayfaNo = 1;
let kullaniciSayfaBoyutu = 10;
let aktifKullaniciFiltresi = true;

async function kullaniciSayfasiGetir() {
    aktifKullaniciFiltresi = true;

    const response = await apiFetch("/api/Kullanicilar");

    if (!response.ok) {
        alert("Kullanıcılar alınamadı.");
        return;
    }

    const liste = await response.json();

    kullaniciListesi = liste.filter(x => x.aktifMi === true);
    kullaniciSayfaNo = 1;
    kullanicilariTabloyaBas(kullaniciListesi);
}

async function kullaniciSayfasiDurumaGoreGetir(aktifMi) {
    aktifKullaniciFiltresi = aktifMi;

    const response = await apiFetch("/api/Kullanicilar");

    if (!response.ok) {
        alert("Kullanıcılar alınamadı.");
        return;
    }

    const liste = await response.json();

    kullaniciListesi = liste.filter(x => x.aktifMi === aktifMi);
    kullaniciSayfaNo = 1;
    kullanicilariTabloyaBas(kullaniciListesi);
}

function kullanicilariTabloyaBas(liste) {
    aktifKullaniciListesi = liste;

    const tbody = document.getElementById("kullaniciListe");
    if (!tbody) return;

    tbody.innerHTML = "";

    const baslangic = (kullaniciSayfaNo - 1) * kullaniciSayfaBoyutu;
    const bitis = baslangic + kullaniciSayfaBoyutu;

    liste.slice(baslangic, bitis).forEach(k => {
        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>
                <div class="kullanici-actions">
                    <button class="btn-edit" onclick="kullaniciDuzenle('${k.id ?? k.docId}')">✏️</button>

                    ${k.aktifMi
                ? `<button class="btn-delete" onclick="kullaniciDurumGuncelle('${k.id ?? k.docId}', false)">Pasif Yap</button>`
                : `<button class="btn-active" onclick="kullaniciDurumGuncelle('${k.id ?? k.docId}', true)">Aktif Et</button>`
            }
                </div>
            </td>
            <td>${k.kullaniciAdi ?? ""}</td>
            <td>${k.ad ?? ""} ${k.soyad ?? ""}</td>
            <td>${k.cepTelefonNo ?? ""}</td>
            <td>${k.mail ?? ""}</td>
            <td>${rolText(k.rol)}</td>
            <td>${k.aktifMi ? "Aktif" : "Pasif"}</td>
        `;

        tbody.appendChild(tr);
    });

    kullaniciPagerBas();
}

async function kullaniciPopupAc() {
    const response = await apiFetch("/pages/kullanicilar/kullanici-ekle.html");
    const html = await response.text();

    document.getElementById("kullaniciPopupIcerik").innerHTML = html;
    document.getElementById("kullaniciPopupBaslik").textContent = "Yeni Kullanıcı";
    document.getElementById("kullaniciPopup").classList.remove("hidden");
}

function kullaniciPopupKapat() {
    document.getElementById("kullaniciPopup").classList.add("hidden");
    document.getElementById("kullaniciPopupIcerik").innerHTML = "";
}

async function kullaniciDuzenle(id) {
    const k = aktifKullaniciListesi.find(x => (x.id ?? x.docId) === id)
        || kullaniciListesi.find(x => (x.id ?? x.docId) === id);

    if (!k) return;

    const response = await apiFetch("/pages/kullanicilar/kullanici-ekle.html");
    const html = await response.text();

    document.getElementById("kullaniciPopupIcerik").innerHTML = html;
    document.getElementById("kullaniciPopupBaslik").textContent = "Kullanıcı Düzenle";

    document.getElementById("kullaniciDocId").value = id;
    document.getElementById("kullaniciAdi").value = k.kullaniciAdi ?? "";
    document.getElementById("ad").value = k.ad ?? "";
    document.getElementById("soyad").value = k.soyad ?? "";
    document.getElementById("cepTelefonNo").value = k.cepTelefonNo ?? "";
    document.getElementById("mail").value = k.mail ?? "";
    document.getElementById("rol").value = k.rol ?? "2";

    document.getElementById("kullaniciPopup").classList.remove("hidden");
}

document.addEventListener("submit", async function (e) {
    if (e.target && e.target.id === "kullaniciForm") {
        e.preventDefault();

        const id = document.getElementById("kullaniciDocId").value;
        const veri = kullaniciFormVerisiAl();

        const url = id ? `/api/Kullanicilar/${id}` : "/api/Kullanicilar";
        const method = id ? "PUT" : "POST";

        const response = await apiFetch(url, {
            method,
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(veri)
        });

        if (!response.ok) {
            alert(await response.text());
            return;
        }

        kullaniciPopupKapat();

        if (aktifKullaniciFiltresi) {
            kullaniciSayfasiGetir();
        } else {
            kullaniciSayfasiDurumaGoreGetir(false);
        }
    }
});

async function kullaniciDurumGuncelle(id, aktifMi) {
    if (!confirm(aktifMi ? "Kullanıcı aktif edilsin mi?" : "Kullanıcı pasif yapılsın mı?"))
        return;

    const response = await apiFetch(`/api/Kullanicilar/${id}/durum`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ aktifMi })
    });

    if (!response.ok) {
        alert("Durum güncellenemedi.");
        return;
    }

    if (aktifKullaniciFiltresi) {
        kullaniciSayfasiGetir();
    } else {
        kullaniciSayfasiDurumaGoreGetir(false);
    }
}

function kullaniciAra() {
    const arama = document.getElementById("kullaniciArama").value.toLowerCase().trim();

    if (!arama) {
        kullaniciSayfaNo = 1;
        kullanicilariTabloyaBas(kullaniciListesi);
        return;
    }

    const filtreli = kullaniciListesi.filter(k =>
        (k.kullaniciAdi ?? "").toLowerCase().includes(arama) ||
        (`${k.ad ?? ""} ${k.soyad ?? ""}`).toLowerCase().includes(arama) ||
        (k.cepTelefonNo ?? "").toLowerCase().includes(arama) ||
        (k.mail ?? "").toLowerCase().includes(arama)
    );

    kullaniciSayfaNo = 1;
    kullanicilariTabloyaBas(filtreli);
}

function rolText(rol) {
    if (rol == "1") return "Admin";
    if (rol == "2") return "Kullanıcı";
    return rol ?? "-";
}