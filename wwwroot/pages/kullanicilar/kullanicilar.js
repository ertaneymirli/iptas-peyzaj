let kullaniciListesi = [];
let aktifKullaniciListesi = [];
let kullaniciSayfaNo = 1;
const kullaniciSayfaBoyutu = 10;
let aktifKullaniciFiltresi = true;

function kullaniciSayfasiAcikMi() {
    return document.getElementById("kullaniciListe") !== null;
}

async function kullaniciSayfasiGetir() {
    aktifKullaniciFiltresi = true;

    const response = await apiFetch("/api/Kullanicilar");

    if (!response.ok) {
        if (kullaniciSayfasiAcikMi()) {
            alert("Kullanıcılar alınamadı.");
        }
        return;
    }

    const liste = await response.json();

    // API cevabı gelmeden başka menüye geçildiyse ekrana yazma.
    if (!kullaniciSayfasiAcikMi()) {
        return;
    }

    kullaniciListesi = liste.filter(x => x.aktifMi === true);
    kullaniciSayfaNo = 1;
    kullanicilariTabloyaBas(kullaniciListesi);
}

async function kullaniciSayfasiDurumaGoreGetir(aktifMi) {
    aktifKullaniciFiltresi = aktifMi;

    const response = await apiFetch("/api/Kullanicilar");

    if (!response.ok) {
        if (kullaniciSayfasiAcikMi()) {
            alert("Kullanıcılar alınamadı.");
        }
        return;
    }

    const liste = await response.json();

    // API cevabı gelmeden başka menüye geçildiyse ekrana yazma.
    if (!kullaniciSayfasiAcikMi()) {
        return;
    }

    kullaniciListesi = liste.filter(x => x.aktifMi === aktifMi);
    kullaniciSayfaNo = 1;
    kullanicilariTabloyaBas(kullaniciListesi);
}

function kullanicilariTabloyaBas(liste) {
    const tbody = document.getElementById("kullaniciListe");

    // Kullanıcı başka menüye geçtiyse tablo artık ekranda değildir.
    if (!tbody) {
        return;
    }

    aktifKullaniciListesi = Array.isArray(liste) ? liste : [];

    const toplamSayfa = Math.max(
        1,
        Math.ceil(aktifKullaniciListesi.length / kullaniciSayfaBoyutu)
    );

    if (kullaniciSayfaNo > toplamSayfa) {
        kullaniciSayfaNo = toplamSayfa;
    }

    tbody.innerHTML = "";

    if (aktifKullaniciListesi.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7">Kayıt bulunamadı.</td>
            </tr>
        `;

        kullaniciPagerBas();
        return;
    }

    const baslangic =
        (kullaniciSayfaNo - 1) * kullaniciSayfaBoyutu;

    const bitis = baslangic + kullaniciSayfaBoyutu;

    aktifKullaniciListesi
        .slice(baslangic, bitis)
        .forEach(k => {
            const tr = document.createElement("tr");
            const id = k.id ?? k.docId ?? "";

            tr.innerHTML = `
                <td>
                    <div class="kullanici-actions">
                        <button
                            type="button"
                            class="btn-edit"
                            onclick="kullaniciDuzenle('${id}')">
                            ✏️
                        </button>

                        ${k.aktifMi
                    ? `<button
                                   type="button"
                                   class="btn-delete"
                                   onclick="kullaniciDurumGuncelle('${id}', false)">
                                   Pasif Yap
                               </button>`
                    : `<button
                                   type="button"
                                   class="btn-active"
                                   onclick="kullaniciDurumGuncelle('${id}', true)">
                                   Aktif Et
                               </button>`
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

function kullaniciPagerBas() {
    const pager = document.getElementById("kullaniciPager");

    // Kullanıcı başka menüye geçtiyse pager artık ekranda değildir.
    if (!pager) {
        return;
    }

    pager.innerHTML = "";

    const toplamSayfa = Math.ceil(
        aktifKullaniciListesi.length / kullaniciSayfaBoyutu
    );

    if (toplamSayfa <= 1) {
        return;
    }

    for (let sayfa = 1; sayfa <= toplamSayfa; sayfa++) {
        const buton = document.createElement("button");

        buton.type = "button";
        buton.textContent = sayfa;
        buton.className = sayfa === kullaniciSayfaNo
            ? "pager-button active"
            : "pager-button";

        buton.addEventListener("click", function () {
            kullaniciSayfaNo = sayfa;
            kullanicilariTabloyaBas(aktifKullaniciListesi);
        });

        pager.appendChild(buton);
    }
}

async function kullaniciPopupAc() {
    const response = await apiFetch(
        "/pages/kullanicilar/kullanici-ekle.html"
    );

    if (!response.ok) {
        if (kullaniciSayfasiAcikMi()) {
            alert("Kullanıcı formu yüklenemedi.");
        }
        return;
    }

    const html = await response.text();
    const popupIcerik = document.getElementById("kullaniciPopupIcerik");
    const popupBaslik = document.getElementById("kullaniciPopupBaslik");
    const popup = document.getElementById("kullaniciPopup");

    if (!popupIcerik || !popupBaslik || !popup) {
        return;
    }

    popupIcerik.innerHTML = html;
    popupBaslik.textContent = "Yeni Kullanıcı";
    popup.classList.remove("hidden");
}

function kullaniciPopupKapat() {
    const popup = document.getElementById("kullaniciPopup");
    const popupIcerik = document.getElementById("kullaniciPopupIcerik");

    if (!popup || !popupIcerik) {
        return;
    }

    popup.classList.add("hidden");
    popupIcerik.innerHTML = "";
}

async function kullaniciDuzenle(id) {
    const k = aktifKullaniciListesi.find(
        x => String(x.id ?? x.docId) === String(id)
    ) || kullaniciListesi.find(
        x => String(x.id ?? x.docId) === String(id)
    );

    if (!k) {
        return;
    }

    const response = await apiFetch(
        "/pages/kullanicilar/kullanici-ekle.html"
    );

    if (!response.ok) {
        if (kullaniciSayfasiAcikMi()) {
            alert("Kullanıcı formu yüklenemedi.");
        }
        return;
    }

    const html = await response.text();
    const popupIcerik = document.getElementById("kullaniciPopupIcerik");
    const popupBaslik = document.getElementById("kullaniciPopupBaslik");
    const popup = document.getElementById("kullaniciPopup");

    if (!popupIcerik || !popupBaslik || !popup) {
        return;
    }

    popupIcerik.innerHTML = html;
    popupBaslik.textContent = "Kullanıcı Düzenle";

    const kullaniciDocId = document.getElementById("kullaniciDocId");
    const kullaniciAdi = document.getElementById("kullaniciAdi");
    const ad = document.getElementById("ad");
    const soyad = document.getElementById("soyad");
    const cepTelefonNo = document.getElementById("cepTelefonNo");
    const mail = document.getElementById("mail");
    const rol = document.getElementById("rol");

    if (
        !kullaniciDocId ||
        !kullaniciAdi ||
        !ad ||
        !soyad ||
        !cepTelefonNo ||
        !mail ||
        !rol
    ) {
        console.error("Kullanıcı düzenleme formundaki alanlar bulunamadı.");
        return;
    }

    kullaniciDocId.value = id;
    kullaniciAdi.value = k.kullaniciAdi ?? "";
    ad.value = k.ad ?? "";
    soyad.value = k.soyad ?? "";
    cepTelefonNo.value = k.cepTelefonNo ?? "";
    mail.value = k.mail ?? "";
    rol.value = k.rol ?? "2";

    popup.classList.remove("hidden");
}

document.addEventListener("submit", async function (e) {
    if (!e.target || e.target.id !== "kullaniciForm") {
        return;
    }

    e.preventDefault();

    const kullaniciDocId = document.getElementById("kullaniciDocId");

    if (!kullaniciDocId) {
        return;
    }

    const id = kullaniciDocId.value;
    const veri = kullaniciFormVerisiAl();
    const url = id
        ? `/api/Kullanicilar/${id}`
        : "/api/Kullanicilar";
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

    if (!kullaniciSayfasiAcikMi()) {
        return;
    }

    if (aktifKullaniciFiltresi) {
        kullaniciSayfasiGetir();
    } else {
        kullaniciSayfasiDurumaGoreGetir(false);
    }
});

async function kullaniciDurumGuncelle(id, aktifMi) {
    const onay = confirm(
        aktifMi
            ? "Kullanıcı aktif edilsin mi?"
            : "Kullanıcı pasif yapılsın mı?"
    );

    if (!onay) {
        return;
    }

    const response = await apiFetch(
        `/api/Kullanicilar/${id}/durum`,
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ aktifMi })
        }
    );

    if (!response.ok) {
        if (kullaniciSayfasiAcikMi()) {
            alert("Durum güncellenemedi.");
        }
        return;
    }

    if (!kullaniciSayfasiAcikMi()) {
        return;
    }

    if (aktifKullaniciFiltresi) {
        kullaniciSayfasiGetir();
    } else {
        kullaniciSayfasiDurumaGoreGetir(false);
    }
}

function kullaniciAra() {
    const aramaInput = document.getElementById("kullaniciArama");

    if (!aramaInput) {
        return;
    }

    const arama = aramaInput.value.toLowerCase().trim();

    if (!arama) {
        kullaniciSayfaNo = 1;
        kullanicilariTabloyaBas(kullaniciListesi);
        return;
    }

    const filtreli = kullaniciListesi.filter(k =>
        (k.kullaniciAdi ?? "").toLowerCase().includes(arama) ||
        (`${k.ad ?? ""} ${k.soyad ?? ""}`)
            .toLowerCase()
            .includes(arama) ||
        (k.cepTelefonNo ?? "").toLowerCase().includes(arama) ||
        (k.mail ?? "").toLowerCase().includes(arama)
    );

    kullaniciSayfaNo = 1;
    kullanicilariTabloyaBas(filtreli);
}

function rolText(rol) {
    if (rol == "1") {
        return "Admin";
    }

    if (rol == "2") {
        return "Kullanıcı";
    }

    return rol ?? "-";
}
