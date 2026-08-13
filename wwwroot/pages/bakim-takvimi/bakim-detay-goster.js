async function bakimResimUrlHazirla(resimUrl) {
    if (!resimUrl) return "";

    const response = await apiFetch(resimUrl);

    if (!response.ok) {
        console.error("Bakım resmi alınamadı:", await response.text());
        return "";
    }

    const nesneUrl = URL.createObjectURL(await response.blob());
    window.__bakimResimNesneUrlListesi ??= [];
    window.__bakimResimNesneUrlListesi.push(nesneUrl);
    return nesneUrl;
}

function bakimResimUrlTemizle() {
    (window.__bakimResimNesneUrlListesi ?? [])
        .forEach(url => URL.revokeObjectURL(url));

    window.__bakimResimNesneUrlListesi = [];
}

async function bakimDetayGoster(e, bakimId) {
    e.stopPropagation();
    bakimResimUrlTemizle();

    const bakim = bakimlar.find(x => x.id == bakimId);

    const response = await apiFetch(`/api/BakimPlanlari/${bakimId}/detaylar`);

    let detaylar = [];

    if (response.ok) {
        detaylar = await response.json();
    }

    const icerik = document.getElementById("bakimDetayIcerik");

    const oncesiResim = detaylar.find(x => x.resimTip === "O");
    const sonrasiResim = detaylar.find(x => x.resimTip === "S");

    const [oncesiGosterimUrl, sonrasiGosterimUrl] =
        await Promise.all([
            bakimResimUrlHazirla(oncesiResim?.resimUrl),
            bakimResimUrlHazirla(sonrasiResim?.resimUrl)
        ]);

    const tekilPersoneller = [
        ...new Map(detaylar.map(x => [x.personelNo, x])).values()
    ];

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

        <div class="detay-card">
            <h3>Bakım Fotoğrafları</h3>

            <div class="bakim-foto-grid">
                <div>
                    <h4>Öncesi</h4>
                    ${oncesiGosterimUrl
            ? `<img src="${oncesiGosterimUrl}" class="bakim-resim" alt="Bakım öncesi" />`
            : `<p class="resim-yok">Fotoğraf eklenmemiş.</p>`
        }
                </div>

                <div>
                    <h4>Sonrası</h4>
                    ${sonrasiGosterimUrl
            ? `<img src="${sonrasiGosterimUrl}" class="bakim-resim" alt="Bakım sonrası" />`
            : `<p class="resim-yok">Fotoğraf eklenmemiş.</p>`
        }
                </div>
            </div>
        </div>

        <div class="detay-card">
            <h3>Görevli Personeller</h3>

            ${tekilPersoneller.length > 0
            ? tekilPersoneller.map(d => `
                    <div class="personel-detay">
                        <p><b>Personel No:</b> ${d.personelNo ?? "-"}</p>
                        <p><b>Personel:</b> ${d.adSoyad ?? "-"}</p>
                    </div>
                `).join("")
            : `<p>Görevli personel bilgisi bulunamadı.</p>`
        }
        </div>
    `;

    icerik.innerHTML = html;

    document.getElementById("bakimDetayPopup").classList.remove("hidden");
}
