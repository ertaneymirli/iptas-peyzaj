function musteriEkleFormHazirla() {
    const form =
        document.getElementById("musteriEkleForm");

    if (!form) {
        console.error("musteriEkleForm bulunamadı.");
        return;
    }

    const periyodikBakimTuru =
        document.getElementById("periyodikBakimTuru");

    if (periyodikBakimTuru) {
        periyodikBakimTuru.onchange =
            periyodikBakimAlaniGuncelle;

        periyodikBakimAlaniGuncelle();
    }

    // Birden fazla event eklenmesini önlemek için
    // addEventListener yerine onsubmit kullanıyoruz.
    form.onsubmit = async function (e) {
        e.preventDefault();

        const tcInput =
            document.getElementById("tc");

        const tc = tcInput
            ? tcInput.value.trim()
            : "";

        if (tc && tc.length !== 11) {
            alert(
                "TC Kimlik No 11 haneli olmalıdır."
            );
            return;
        }

        const musteri =
            musteriFormVerisiAl();

        let url = "/api/Musteriler";
        let method = "POST";

        if (
            formMode === "duzenle" &&
            seciliMusteri
        ) {
            url =
                `/api/Musteriler/${seciliMusteri.id}`;

            method = "PUT";
        }

        const response = await apiFetch(
            url,
            {
                method: method,
                headers: {
                    "Content-Type":
                        "application/json"
                },
                body: JSON.stringify(musteri)
            }
        );

        if (response.ok) {
            musteriEklePopupKapat();
            await musterileriGetir();
        } else {
            const hata = await response.text();

            console.error(
                "Müşteri kayıt hatası:",
                hata
            );

            alert("İşlem başarısız.");
        }
    };
}

function periyodikBakimAlaniGuncelle() {
    const periyodikBakimTuru =
        document.getElementById("periyodikBakimTuru");

    const belirliGunlerDiv =
        document.getElementById("belirliGunlerDiv");

    const belirliGunler =
        document.getElementById("belirliGunler");

    if (
        !periyodikBakimTuru ||
        !belirliGunlerDiv
    ) {
        return;
    }

    const kendimBelirleyecegim =
        periyodikBakimTuru.value ===
        "Kendim Belirleyeceğim";

    belirliGunlerDiv.style.display =
        kendimBelirleyecegim
            ? "block"
            : "none";

    if (!kendimBelirleyecegim && belirliGunler) {
        belirliGunler.value = "";
    }
}

function musteriFormVerisiAl() {
    return {
        ad: document.getElementById("ad").value,
        soyad: document.getElementById("soyad").value,
        tc: document.getElementById("tc").value,

        dogumTarihi: tarihHazirla(
            document.getElementById("dogumTarihi").value
        ),

        cinsiyet:
            document.getElementById("cinsiyet").value,

        telefon:
            document.getElementById("telefon").value,

        caddeSokak:
            document.getElementById("caddeSokak").value,

        mahalle:
            document.getElementById("mahalle").value,

        no: document.getElementById("no").value,

        daire:
            document.getElementById("daire").value,

        sehir:
            document.getElementById("sehir").value,

        adres: adresOlustur(),

        mekanTipi:
            document.getElementById("mekanTipi").value,

        sozlesmeTarihi: tarihHazirla(
            document.getElementById("sozlesmeTarihi").value
        ),

        gorusmeTarihi: tarihHazirla(
            document.getElementById("gorusmeTarihi").value
        ),

        baslangicTarihi: tarihHazirla(
            document.getElementById("baslangicTarihi").value
        ),

        bitisTarihi: tarihHazirla(
            document.getElementById("bitisTarihi").value
        ),

        bakimTarihi: tarihHazirla(
            document.getElementById("bakimTarihi").value
        ),

        periyodikBakim: Number(
            document.getElementById("periyodikBakim")
                .value || 0
        ),

        periyodikBakimTuru:
            document.getElementById("periyodikBakimTuru")
                .value,

        belirliGunler:
            document.getElementById("belirliGunler")
                ?.value ?? "",

        aciklama:
            document.getElementById("aciklama").value
    };
}

function tarihHazirla(value) {
    if (!value) {
        return new Date().toISOString();
    }

    return value + "T00:00:00Z";
}

function adresOlustur() {
    const mahalle =
        document.getElementById("mahalle").value;

    const caddeSokak =
        document.getElementById("caddeSokak").value;

    const no =
        document.getElementById("no").value;

    const daire =
        document.getElementById("daire").value;

    const sehir =
        document.getElementById("sehir").value;

    return `${mahalle} Mah. ${caddeSokak} No:${no} Daire:${daire} ${sehir}`;
}