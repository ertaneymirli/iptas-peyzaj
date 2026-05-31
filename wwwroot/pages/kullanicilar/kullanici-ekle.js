function kullaniciFormVerisiAl() {
    return {
        kullaniciAdi: document.getElementById("kullaniciAdi").value.trim(),
        sifre: document.getElementById("sifre").value,
        ad: document.getElementById("ad").value.trim(),
        soyad: document.getElementById("soyad").value.trim(),
        cepTelefonNo: document.getElementById("cepTelefonNo").value.trim(),
        mail: document.getElementById("mail").value.trim(),
        rol: document.getElementById("rol").value,
        aktifMi: true
    };
}