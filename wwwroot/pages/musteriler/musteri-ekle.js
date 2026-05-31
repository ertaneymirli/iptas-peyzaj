function musteriEkleFormHazirla() {
    const form = document.getElementById("musteriEkleForm");

    if (!form) {
        console.error("musteriEkleForm bulunamadı.");
        return;
    }

    form.addEventListener("submit", async function (e) {
        e.preventDefault();

        const tcInput = document.getElementById("tc");
        const tc = tcInput ? tcInput.value : "";

        if (tc && tc.length !== 11) {
            alert("TC Kimlik No 11 haneli olmalıdır.");
            return;
        }

        const musteri = musteriFormVerisiAl();

        let url = "/api/Musteriler";
        let method = "POST";

        if (formMode === "duzenle") {
            url = `/api/Musteriler/${seciliMusteri.id}`;
            method = "PUT";
        }

        const response = await fetch(url, {
            method,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(musteri)
        });

        if (response.ok) {
            musteriEklePopupKapat();
            musterileriGetir();
        } else {
            alert("İşlem başarısız.");
        }
    });
}

function musteriFormVerisiAl() {
    return {
        ad: document.getElementById("ad").value,
        soyad: document.getElementById("soyad").value,
        tc: document.getElementById("tc").value,
        dogumTarihi: tarihHazirla(document.getElementById("dogumTarihi").value),
        cinsiyet: document.getElementById("cinsiyet").value,
        telefon: document.getElementById("telefon").value,
        caddeSokak: document.getElementById("caddeSokak").value,
        mahalle: document.getElementById("mahalle").value,
        no: document.getElementById("no").value,
        daire: document.getElementById("daire").value,
        sehir: document.getElementById("sehir").value,
        adres: adresOlustur(),
        mekanTipi: document.getElementById("mekanTipi").value,
        sozlesmeTarihi: tarihHazirla(document.getElementById("sozlesmeTarihi").value),
        gorusmeTarihi: tarihHazirla(document.getElementById("gorusmeTarihi").value),
        baslangicTarihi: tarihHazirla(document.getElementById("baslangicTarihi").value),
        bitisTarihi: tarihHazirla(document.getElementById("bitisTarihi").value),
        bakimTarihi: tarihHazirla(document.getElementById("bakimTarihi").value),
        periyodikBakim: Number(document.getElementById("periyodikBakim").value || 0),
        periyodikBakimTuru: document.getElementById("periyodikBakimTuru").value,
        aciklama: document.getElementById("aciklama").value
    };
}

function tarihHazirla(value) {
    if (!value) return new Date().toISOString();
    return value + "T00:00:00Z";
}

function adresOlustur() {
    return `${document.getElementById("mahalle").value} Mah. ${document.getElementById("caddeSokak").value} No:${document.getElementById("no").value} Daire:${document.getElementById("daire").value} ${document.getElementById("sehir").value}`;
}