function personelFormVerisiAl() {

    return {

        id: Number(
            document.getElementById("personelId").value
        ),

        personelNo: Number(
            document.getElementById("personelNo").value
        ),

        ad: document.getElementById("personelAd").value,

        soyad: document.getElementById("personelSoyad").value,

        telefon: document.getElementById("personelTelefon").value,

        gorev: document.getElementById("personelGorev").value
    };
}