let musteriKullaniciBaglantilari = [];
let musteriKullaniciGorunenListe = [];
let musteriKullaniciKullanicilar = [];
let musteriKullaniciMusteriler = [];
let seciliMusteriKullanici = null;

function mkAlan(nesne, camelAlan, pascalAlan) {
    return nesne?.[camelAlan] ??
        nesne?.[pascalAlan];
}

function mkAdSoyad(nesne) {
    const ad =
        mkAlan(nesne, "ad", "Ad") ?? "";

    const soyad =
        mkAlan(nesne, "soyad", "Soyad") ?? "";

    return `${ad} ${soyad}`.trim();
}

async function musteriKullanicilariniGetir() {
    try {
        const [
            baglantiResponse,
            kullaniciResponse,
            musteriResponse
        ] = await Promise.all([
            apiFetch("/api/MusteriKullanicilari"),
            apiFetch("/api/Kullanicilar"),
            apiFetch("/api/Musteriler")
        ]);

        if (!baglantiResponse.ok) {
            throw new Error(
                "Müşteri-kullanıcı bağlantıları alınamadı."
            );
        }

        if (!kullaniciResponse.ok) {
            throw new Error(
                "Kullanıcılar alınamadı."
            );
        }

        if (!musteriResponse.ok) {
            throw new Error(
                "Müşteriler alınamadı."
            );
        }

        musteriKullaniciBaglantilari =
            await baglantiResponse.json();

        musteriKullaniciKullanicilar =
            await kullaniciResponse.json();

        musteriKullaniciMusteriler =
            await musteriResponse.json();

        musteriKullaniciGorunenListe =
            musteriKullaniciBaglantilariniBirlestir();

        seciliMusteriKullanici = null;

        musteriKullaniciTabloyaBas(
            musteriKullaniciGorunenListe
        );

    } catch (hata) {
        console.error(
            "Müşteri kullanıcı listesi hatası:",
            hata
        );

        alert(
            hata.message ||
            "Müşteri kullanıcı listesi yüklenemedi."
        );
    }
}

function musteriKullaniciBaglantilariniBirlestir() {
    return musteriKullaniciBaglantilari.map(
        function (baglanti) {
            const kullaniciId = String(
                mkAlan(
                    baglanti,
                    "kullaniciId",
                    "KullaniciId"
                ) ?? ""
            );

            const musteriId = String(
                mkAlan(
                    baglanti,
                    "musteriId",
                    "MusteriId"
                ) ?? ""
            );

            const kullanici =
                musteriKullaniciKullanicilar.find(
                    function (item) {
                        return String(
                            mkAlan(item, "id", "Id")
                        ) === kullaniciId;
                    }
                );

            const musteri =
                musteriKullaniciMusteriler.find(
                    function (item) {
                        return String(
                            mkAlan(item, "id", "Id")
                        ) === musteriId;
                    }
                );

            return {
                id: mkAlan(
                    baglanti,
                    "id",
                    "Id"
                ),

                kullaniciId: Number(kullaniciId),

                musteriId: Number(musteriId),

                kullaniciAdSoyad: kullanici
                    ? mkAdSoyad(kullanici)
                    : "Kullanıcı bulunamadı",

                musteriAdSoyad: musteri
                    ? mkAdSoyad(musteri)
                    : "Müşteri bulunamadı"
            };
        }
    );
}

function musteriKullaniciTabloyaBas(liste) {
    const tbody = document.getElementById(
        "musteriKullaniciListe"
    );

    const template = document.getElementById(
        "musteriKullaniciSatirTemplate"
    );

    const bosMesaj = document.getElementById(
        "musteriKullaniciBosMesaj"
    );

    if (!tbody || !template) {
        return;
    }

    tbody.innerHTML = "";

    liste.forEach(function (kayit) {
        const parca =
            template.content.cloneNode(true);

        const satir =
            parca.querySelector("tr");

        satir.dataset.id = kayit.id;

        satir
            .querySelectorAll("[data-bind-text]")
            .forEach(function (alan) {
                const alanAdi =
                    alan.dataset.bindText;

                alan.textContent =
                    kayit[alanAdi] ?? "";
            });

        tbody.appendChild(parca);
    });

    if (bosMesaj) {
        bosMesaj.classList.toggle(
            "hidden",
            liste.length > 0
        );
    }
}

function musteriKullaniciSatirSec(satir) {
    document
        .querySelectorAll(
            "#musteriKullaniciListe tr"
        )
        .forEach(function (item) {
            item.classList.remove("selected");

            const radio =
                item.querySelector(
                    "input[type='radio']"
                );

            if (radio) {
                radio.checked = false;
            }
        });

    satir.classList.add("selected");

    const radio =
        satir.querySelector(
            "input[type='radio']"
        );

    if (radio) {
        radio.checked = true;
    }

    seciliMusteriKullanici =
        musteriKullaniciGorunenListe.find(
            function (item) {
                return String(item.id) ===
                    String(satir.dataset.id);
            }
        ) ?? null;
}

function musteriKullanicisiAra() {
    const aramaInput = document.getElementById(
        "musteriKullaniciArama"
    );

    if (!aramaInput) {
        return;
    }

    const metin = aramaInput.value
        .trim()
        .toLocaleLowerCase("tr-TR");

    const tumListe =
        musteriKullaniciBaglantilariniBirlestir();

    musteriKullaniciGorunenListe =
        tumListe.filter(function (item) {
            const kullanici =
                item.kullaniciAdSoyad
                    .toLocaleLowerCase("tr-TR");

            const musteri =
                item.musteriAdSoyad
                    .toLocaleLowerCase("tr-TR");

            return !metin ||
                kullanici.includes(metin) ||
                musteri.includes(metin);
        });

    seciliMusteriKullanici = null;

    musteriKullaniciTabloyaBas(
        musteriKullaniciGorunenListe
    );
}

async function musteriKullaniciPopupAc(kayit) {
    try {
        const response = await apiFetch(
            "/pages/musteri-kullanicisi/musteri-kullanicilari-ekle.html"
        );

        if (!response.ok) {
            alert(
                "Müşteri kullanıcı ekleme ekranı yüklenemedi."
            );

            return;
        }

        const html =
            await response.text();

        const popupHost =
            document.getElementById(
                "musteriKullaniciPopupHost"
            );

        if (!popupHost) {
            alert("Popup alanı bulunamadı.");
            return;
        }

        popupHost.innerHTML = html;

        if (
            typeof musteriKullaniciEditorFormunuBagla !==
            "function"
        ) {
            alert(
                "musteri-kullanicilari-ekle.js dosyası yüklenmemiş."
            );

            return;
        }

        if (
            typeof musteriKullaniciEditorAc !==
            "function"
        ) {
            alert(
                "Popup açma fonksiyonu bulunamadı."
            );

            return;
        }

        musteriKullaniciEditorFormunuBagla();
        musteriKullaniciEditorAc(kayit);

    } catch (hata) {
        console.error(
            "Popup açma hatası:",
            hata
        );

        alert(
            "Müşteri kullanıcı ekranı açılamadı."
        );
    }
}

async function musteriKullanicisiEkle() {
    await musteriKullaniciPopupAc(null);
}

async function musteriKullanicisiSil() {
    if (!seciliMusteriKullanici) {
        alert(
            "Silmek için bir kayıt seçmelisin."
        );

        return;
    }

    const onay = confirm(
        "Seçili müşteri-kullanıcı bağlantısı silinsin mi?"
    );

    if (!onay) {
        return;
    }

    try {
        const id = encodeURIComponent(
            seciliMusteriKullanici.id
        );

        const response = await apiFetch(
            `/api/MusteriKullanicilari/${id}`,
            {
                method: "DELETE"
            }
        );

        if (!response.ok) {
            const hata =
                await response.text();

            alert(
                hata ||
                "Bağlantı silinemedi."
            );

            return;
        }

        alert("Bağlantı silindi.");

        seciliMusteriKullanici = null;

        await musteriKullanicilariniGetir();

    } catch (hata) {
        console.error(
            "Bağlantı silme hatası:",
            hata
        );

        alert(
            "Bağlantı silinirken hata oluştu."
        );
    }
}
