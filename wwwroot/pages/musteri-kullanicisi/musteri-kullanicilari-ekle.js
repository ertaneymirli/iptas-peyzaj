function mkEditorAlan(nesne, camelAlan, pascalAlan) {
    return nesne?.[camelAlan] ?? nesne?.[pascalAlan];
}

function mkKullaniciGosterim(item) {
    const adSoyad = mkAdSoyad(item);
    const kullaniciAdi = mkEditorAlan(item, "kullaniciAdi", "KullaniciAdi") ?? "";
    return kullaniciAdi ? `${adSoyad} (${kullaniciAdi})` : adSoyad;
}

function mkMusteriGosterim(item) {
    const adSoyad = mkAdSoyad(item);
    const musteriNo = mkEditorAlan(item, "musteriNo", "MusteriNo");
    return musteriNo !== null && musteriNo !== undefined
        ? `${musteriNo} - ${adSoyad}`
        : adSoyad;
}

function musteriKullaniciEditorAc(kayit) {
    const popup = document.getElementById("musteriKullaniciEditorPopup");
    const form = document.getElementById("musteriKullaniciEditorForm");

    if (!popup || !form) return;

    form.reset();
    document.getElementById("musteriKullaniciBaglantiId").value = "";
    document.getElementById("musteriKullaniciKullaniciId").value = "";
    document.getElementById("musteriKullaniciMusteriId").value = "";
    document.getElementById("musteriKullaniciEditorMesaj").textContent = "";
    musteriKullaniciSonuclariKapat();

    const duzenlemeMi = Boolean(kayit?.id);
    document.getElementById("musteriKullaniciEditorBaslik").textContent =
        duzenlemeMi ? "Müşteri Kullanıcısı Düzenle" : "Müşteri Kullanıcısı Ekle";

    if (duzenlemeMi) {
        document.getElementById("musteriKullaniciBaglantiId").value = kayit.id;
        document.getElementById("musteriKullaniciKullaniciId").value = kayit.kullaniciId;
        document.getElementById("musteriKullaniciMusteriId").value = kayit.musteriId;

        const kullanici = musteriKullaniciKullanicilar.find(function (item) {
            return String(mkEditorAlan(item, "id", "Id")) === String(kayit.kullaniciId);
        });

        const musteri = musteriKullaniciMusteriler.find(function (item) {
            return String(mkEditorAlan(item, "id", "Id")) === String(kayit.musteriId);
        });

        document.getElementById("musteriKullaniciKullaniciArama").value =
            kullanici ? mkKullaniciGosterim(kullanici) : kayit.kullaniciAdSoyad;

        document.getElementById("musteriKullaniciMusteriArama").value =
            musteri ? mkMusteriGosterim(musteri) : kayit.musteriAdSoyad;
    }

    popup.classList.remove("hidden");
}

function musteriKullaniciEditorKapat() {
    document.getElementById("musteriKullaniciEditorPopup")?.classList.add("hidden");
    musteriKullaniciSonuclariKapat();
}

function musteriKullaniciSecimiTemizleVeAra(tur) {
    const idElementi = document.getElementById(
        tur === "kullanici" ? "musteriKullaniciKullaniciId" : "musteriKullaniciMusteriId"
    );

    idElementi.value = "";
    musteriKullaniciSecenekleriniGoster(tur);
}

function musteriKullaniciSecenekleriniGoster(tur) {
    const kullaniciMi = tur === "kullanici";
    const input = document.getElementById(
        kullaniciMi ? "musteriKullaniciKullaniciArama" : "musteriKullaniciMusteriArama"
    );
    const sonucAlani = document.getElementById(
        kullaniciMi ? "musteriKullaniciKullaniciSonuclari" : "musteriKullaniciMusteriSonuclari"
    );
    const kaynak = kullaniciMi ? musteriKullaniciKullanicilar : musteriKullaniciMusteriler;
    const gosterim = kullaniciMi ? mkKullaniciGosterim : mkMusteriGosterim;
    const arama = input.value.trim().toLocaleLowerCase("tr-TR");

    const filtreli = kaynak.filter(function (item) {
        return gosterim(item).toLocaleLowerCase("tr-TR").includes(arama);
    }).slice(0, 50);

    sonucAlani.innerHTML = "";

    if (filtreli.length === 0) {
        const mesaj = document.createElement("div");
        mesaj.className = "arama-sonuc-yok";
        mesaj.textContent = "Sonuç bulunamadı.";
        sonucAlani.appendChild(mesaj);
    } else {
        filtreli.forEach(function (item) {
            const buton = document.createElement("button");
            buton.type = "button";
            buton.className = "arama-sonuc-item";
            buton.textContent = gosterim(item);
            buton.onclick = function () {
                musteriKullaniciSecenekSec(tur, item, gosterim(item));
            };
            sonucAlani.appendChild(buton);
        });
    }

    sonucAlani.classList.remove("hidden");
}

function musteriKullaniciSecenekSec(tur, item, metin) {
    const kullaniciMi = tur === "kullanici";
    const input = document.getElementById(
        kullaniciMi ? "musteriKullaniciKullaniciArama" : "musteriKullaniciMusteriArama"
    );
    const idElementi = document.getElementById(
        kullaniciMi ? "musteriKullaniciKullaniciId" : "musteriKullaniciMusteriId"
    );

    input.value = metin;
    idElementi.value = mkEditorAlan(item, "id", "Id") ?? "";
    musteriKullaniciSonuclariKapat();
}

function musteriKullaniciSonuclariKapat() {
    document.querySelectorAll(".arama-sonuclari").forEach(function (alan) {
        alan.classList.add("hidden");
    });
}

document.addEventListener("click", function (event) {
    if (!event.target.closest(".arama-secim")) {
        musteriKullaniciSonuclariKapat();
    }
});

function musteriKullaniciEditorFormunuBagla() {
    const form = document.getElementById("musteriKullaniciEditorForm");

    if (!form || form.dataset.bagli === "1") return;

    form.dataset.bagli = "1";
    form.addEventListener("submit", async function (event) {
        event.preventDefault();

        const baglantiId = document.getElementById("musteriKullaniciBaglantiId").value;
        const kullaniciId = document.getElementById("musteriKullaniciKullaniciId").value;
        const musteriId = document.getElementById("musteriKullaniciMusteriId").value;
        const mesaj = document.getElementById("musteriKullaniciEditorMesaj");

        if (!kullaniciId || !musteriId) {
            mesaj.textContent = "Kullanıcı ve müşteri seçmelisin.";
            return;
        }

        const duzenlemeMi = Boolean(baglantiId);
        const url = duzenlemeMi
            ? `/api/MusteriKullanicilari/${encodeURIComponent(baglantiId)}`
            : "/api/MusteriKullanicilari";

        const response = await apiFetch(url, {
            method: duzenlemeMi ? "PUT" : "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                kullaniciId: Number(kullaniciId),
                musteriId: Number(musteriId)
            })
        });

        if (!response.ok) {
            mesaj.textContent = await response.text() || "Kayıt işlemi başarısız.";
            return;
        }

        mesaj.classList.add("success");
        mesaj.textContent = duzenlemeMi ? "Bağlantı güncellendi." : "Bağlantı eklendi.";

        await musteriKullanicilariniGetir();
        musteriKullaniciEditorKapat();
    });
}

musteriKullaniciEditorFormunuBagla();
