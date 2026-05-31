let teklifler = [];
let aktifTeklifListesi = [];

function teklifleriTabloyaBas(liste) {
    const tbody = document.getElementById("teklifListe");
    if (!tbody) return;

    aktifTeklifListesi = liste;

    tbody.innerHTML = "";

    liste.forEach(t => {
        // mevcut tablo satır kodun burada kalacak
    });
}

function teklifleriDurumaGoreGetir(durumKodu) {
    const filtreli = teklifler.filter(x => x.durumKodu === durumKodu);
    teklifleriTabloyaBas(filtreli);
}

function teklifAra() {
    const arama = document.getElementById("teklifArama").value.toLowerCase().trim();

    if (!arama) {
        teklifleriTabloyaBas(aktifTeklifListesi);
        return;
    }

    const filtreli = aktifTeklifListesi.filter(t =>
        (t.musteriNo?.toString() || "").includes(arama) ||
        (t.adSoyad || "").toLowerCase().includes(arama) ||
        (t.telefon || "").toLowerCase().includes(arama) ||
        (t.aciklama || "").toLowerCase().includes(arama) ||
        (teklifDurumText(t.durumKodu) || "").toLowerCase().includes(arama)
    );

    teklifleriTabloyaBas(filtreli);
}
async function teklifleriGetir() {
    const response = await apiFetch("/api/Teklifler");

    if (!response.ok) {
        alert("Teklifler alınamadı.");
        return;
    }

    teklifler = await response.json();
    teklifleriTabloyaBas(teklifler);
}

function teklifleriTabloyaBas(liste) {
    const tbody = document.getElementById("teklifListe");
    if (!tbody) return;

    tbody.innerHTML = "";

    liste.forEach(t => {
        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${t.musteriNo ?? ""}</td>
            <td>${t.adSoyad ?? ""}</td>
            <td>${t.telefon ?? ""}</td>
            <td>${tarihGoster(t.teklifTarihi)}</td>
            <td>${Number(t.tutar ?? 0).toLocaleString("tr-TR")} ₺</td>
            <td><span class="${teklifDurumClass(t.durumKodu)}">${teklifDurumText(t.durumKodu)}</span></td>
            <td>${t.aciklama ?? ""}</td>
            <td>
    <div class="action-buttons">

        <button class="btn-view"
            onclick="teklifDuzenlePopupAc('${t.id}')">
            Düzenle
        </button>

        <button class="btn-success"
            onclick="teklifOnayla('${t.id}')">
            Onayla
        </button>

        <button class="btn-danger"
            onclick="teklifReddet('${t.id}')">
            Reddet
        </button>

        <button class="btn-danger"
            onclick="teklifSil('${t.id}')">
            Sil
        </button>

    </div>
</td>
        `;

        tbody.appendChild(tr);
    });
}
let teklifMusteriler = [];

async function teklifMusterileriGetir() {

    const response = await apiFetch("/api/Musteriler");

    if (!response.ok) {
        alert("Müşteriler alınamadı.");
        return;
    }

    teklifMusteriler = await response.json();

    const select = document.getElementById("musteriSelect");

    if (!select) return;

    select.innerHTML = `
        <option value="">Müşteri seçiniz</option>
    `;

    teklifMusteriler.forEach(m => {

        select.innerHTML += `
            <option value="${m.musteriNo}">
                ${m.musteriNo} - ${m.ad} ${m.soyad}
            </option>
        `;
    });
}

async function teklifEklePopupAc() {
    const container = document.getElementById("teklifPopupContainer");

    const response = await apiFetch("pages/teklif/teklif-ekle.html");
    container.innerHTML = await response.text();

    document.getElementById("teklifPopupBaslik").textContent = "Yeni Teklif";
    document.getElementById("teklifForm").reset();

    await teklifMusterileriGetir();
}
function teklifMusteriSecildi() {

    const musteriNo = Number(
        document.getElementById("musteriSelect").value
    );

    const musteri = teklifMusteriler.find(x =>
        x.musteriNo == musteriNo
    );

    if (!musteri) return;

    document.getElementById("musteriNo").value =
        musteri.musteriNo;

    document.getElementById("musteriAdSoyad").textContent =
        `${musteri.ad} ${musteri.soyad}`;

    document.getElementById("musteriTelefon").textContent =
        musteri.telefon ?? "-";
}
async function teklifDuzenlePopupAc(id) {
    const teklif = teklifler.find(x => x.id == id);
    if (!teklif) return;

    const container = document.getElementById("teklifPopupContainer");

    const response = await apiFetch("pages/teklif/teklif-ekle.html");
    container.innerHTML = await response.text();

    document.getElementById("teklifPopupBaslik").textContent = "Teklif Düzenle";

    await teklifMusterileriGetir();

    document.getElementById("teklifId").value = teklif.id;
    document.getElementById("musteriArama").value =
        teklif.adSoyad ?? "";
    document.getElementById("musteriNo").value = teklif.musteriNo ?? "";
    document.getElementById("musteriAdSoyad").textContent = teklif.adSoyad ?? "-";
    document.getElementById("musteriTelefon").textContent = teklif.telefon ?? "-";
    document.getElementById("teklifTarihi").value = tarihInputFormat(teklif.teklifTarihi);
    document.getElementById("tutar").value = teklif.tutar ?? "";
    document.getElementById("aciklama").value = teklif.aciklama ?? "";
}

function teklifPopupKapat() {
    document.getElementById("teklifPopupContainer").innerHTML = "";
}

document.addEventListener("submit", async function (e) {
    if (e.target && e.target.id === "teklifForm") {
        e.preventDefault();

        const id = document.getElementById("teklifId").value;

        const musteriNo = Number(document.getElementById("musteriNo").value);

        const musteri = teklifMusteriler.find(x => x.musteriNo == musteriNo);

        if (!musteri) {
            alert("Lütfen müşteri seçiniz.");
            return;
        }

        const teklif = {
            musteriNo: musteri.musteriNo,
            adSoyad: `${musteri.ad} ${musteri.soyad}`,
            telefon: musteri.telefon ?? "",
            teklifTarihi: document.getElementById("teklifTarihi").value + "T00:00:00Z",
            tutar: Number(document.getElementById("tutar").value),
            aciklama: document.getElementById("aciklama").value.trim()
        };

        const url = id
            ? `/api/Teklifler/${id}`
            : "/api/Teklifler";

        const method = id ? "PUT" : "POST";

        const response = await apiFetch(url, {
            method: method,
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(teklif)
        });

        if (!response.ok) {
            alert("Teklif kaydedilemedi.");
            return;
        }

        teklifPopupKapat();
        teklifleriGetir();
    }
});
    window.musteriAra = function () {
        const arama = document
            .getElementById("musteriArama")
            .value
            .toLowerCase();

        const listeDiv = document.getElementById("musteriListe");

        if (!arama) {
            listeDiv.innerHTML = "";
            return;
        }

        const filtreli = teklifMusteriler.filter(x => {
            const adSoyad = `${x.ad} ${x.soyad}`.toLowerCase();

            return (
                adSoyad.includes(arama) ||
                String(x.musteriNo).includes(arama) ||
                (x.telefon ?? "").includes(arama)
            );
        });

        listeDiv.innerHTML = filtreli.map(x => `
        <div class="musteri-item" onclick="musteriSec(${x.musteriNo})">
            <b>${x.musteriNo}</b> - ${x.ad} ${x.soyad}
            <small>${x.telefon ?? "-"}</small>
        </div>
    `).join("");
    };

    window.musteriSec = function (musteriNo) {
        const musteri = teklifMusteriler.find(x => x.musteriNo == musteriNo);

        if (!musteri) return;

        document.getElementById("musteriNo").value = musteri.musteriNo;
        document.getElementById("musteriArama").value = `${musteri.ad} ${musteri.soyad}`;
        document.getElementById("musteriAdSoyad").textContent = `${musteri.ad} ${musteri.soyad}`;
        document.getElementById("musteriTelefon").textContent = musteri.telefon ?? "-";
        document.getElementById("musteriListe").innerHTML = "";
};
function teklifDurumText(kod) {

    switch (kod) {

        case "B":
            return "Bekliyor";

        case "O":
            return "Onaylandı";

        case "R":
            return "Reddedildi";

        default:
            return "-";
    }
}

function teklifDurumClass(kod) {

    switch (kod) {

        case "B":
            return "badge badge-wait";

        case "O":
            return "badge badge-success";

        case "R":
            return "badge badge-danger";

        default:
            return "badge";
    }

}
function tarihInputFormat(tarih) {

    if (!tarih) return "";

    const d = new Date(tarih);

    const yil = d.getFullYear();

    const ay = String(d.getMonth() + 1)
        .padStart(2, "0");

    const gun = String(d.getDate())
        .padStart(2, "0");

    return `${yil}-${ay}-${gun}`;
}
window.teklifReddet = async function (id) {

    const not = prompt(
        "Red nedeni:",
        "Teklif reddedildi."
    );

    if (!not) return;

    await apiFetch(`/api/Teklifler/${id}/durum`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            durumKodu: "R",
            islemNotu: not
        })
    });

    teklifleriGetir();
};
async function silinenTeklifleriGetir() {

    const response =
        await apiFetch(`/api/Teklifler/silinenler`);

    if (!response.ok) {
        alert("Silinen teklifler alınamadı.");
        return;
    }

    const liste = await response.json();

    teklifleriTabloyaBas(liste);
}
window.teklifSil = async function (id) {

    if (!confirm("Bu teklif silinsin mi?"))
        return;

    await apiFetch(`/api/Teklifler/${id}`, {
        method: "DELETE"
    });

    teklifleriGetir();
};
window.teklifOnayla = async function (id) {

    const not = prompt(
        "Onay notu:",
        "Teklif onaylandı."
    );

    if (!not) return;

    await apiFetch(`/api/Teklifler/${id}/durum`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            durumKodu: "O",
            islemNotu: not
        })
    });

    teklifleriGetir();
};