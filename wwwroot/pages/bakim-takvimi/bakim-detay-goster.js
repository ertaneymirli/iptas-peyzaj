async function bakimDetayGoster(e, bakimId) {
    e.stopPropagation();

    const bakim = bakimlar.find(x => x.id == bakimId);

    const response = await apiFetch(`/api/BakimPlanlari/${bakimId}/detaylar`);

    let detaylar = [];

    if (response.ok) {
        detaylar = await response.json();
    }

    const icerik = document.getElementById("bakimDetayIcerik");

    const oncesiResim = detaylar.find(x => x.resimTip === "O");
    const sonrasiResim = detaylar.find(x => x.resimTip === "S");

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
                    ${oncesiResim?.resimUrl
            ? `<img src="${oncesiResim.resimUrl}" class="bakim-resim" />`
            : `<p class="resim-yok">Fotoğraf eklenmemiş.</p>`
        }
                </div>

                <div>
                    <h4>Sonrası</h4>
                    ${sonrasiResim?.resimUrl
            ? `<img src="${sonrasiResim.resimUrl}" class="bakim-resim" />`
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