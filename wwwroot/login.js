
/*const API_URL = "https://localhost:61019/api/Kullanicilar/login";*/
const API_URL = "/api/Kullanicilar/login";
// Port sende farklıysa Swagger adresindeki portu yaz.

document.getElementById("loginForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const kullaniciAdi = document.getElementById("kullaniciAdi").value;
    const sifre = document.getElementById("sifre").value;
    const mesaj = document.getElementById("mesaj");

    mesaj.innerHTML = "Kontrol ediliyor...";

    try {
        const response = await fetch(API_URL, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                kullaniciAdi: kullaniciAdi,
                sifre: sifre
            })
        });

        const data = await response.json();

        if (response.ok) {
            mesaj.innerHTML = "Giriş başarılı ✅";

            const token = data.token || data.Token;
            const kullanici = data.kullanici || data.Kullanici;

            localStorage.setItem("token", token);
            localStorage.setItem("kullanici", JSON.stringify(kullanici));
            /*localStorage.setItem("rol", kullanici.rol || kullanici.Rol);*/

            setTimeout(() => {
                window.location.href = "/admin.html";
            }, 800);
        
        } else {
            mesaj.innerHTML = data || "Kullanıcı adı veya şifre hatalı.";
        }
    } catch (error) {
        mesaj.innerHTML = "API bağlantı hatası. Backend çalışıyor mu?";
        console.error(error);
    }
});