# İPTAŞ Peyzaj – Azure SQL Aşama 1

Bu paket, çalışan Firestore sistemini hemen kapatmadan Azure SQL tablolarını
oluşturur ve mevcut veriyi kontrollü şekilde SQL'e kopyalar.

## Yeni tablo yapısı

- Bütün ana `Id` alanları SQL `IDENTITY` özelliğine sahip otomatik artan `int`tir.
- `Musteriler` tablosunda `BakimTarihleri` dizisi ve `BakimTarihi` tutulmaz.
- Her bakım tarihi `BakimPlanlari` tablosunda ayrı bir satırdır.
- Bakımın personelleri `BakimPersonelleri(Id, BakimId, PersonelId)` tablosundadır.
- Bakım resimleri `BakimDetaylari` tablosundadır; personel sayısı kadar tekrar edilmez.
- `FirestoreId` alanları yalnızca eski kaydı eşleştirmek ve aktarımı tekrar güvenle
  çalıştırmak içindir; yeni sistemin birincil anahtarı değildir.

## 1. Dosyaları projeye kopyalayın

ZIP içindeki aşağıdaki klasörleri mevcut projenizin `Backend` klasörüne ekleyin:

- `Backend/Data`
- `Backend/VeriAktarimi`

Mevcut Firestore dosyalarını silmeyin.

## 2. Program.cs kaydı

`Program.AzureSql.snippet.txt` dosyasındaki `using` ve servis kayıtlarını
`Program.cs` dosyanıza ekleyin.

## 3. Gerekli NuGet paketleri

Projede yoksa çalıştırın:

```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.8
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.8
dotnet tool update --global dotnet-ef --version 8.0.8
```

## 4. Yerel gizli ayarlar

Azure SQL bağlantınız daha önce kaydedildiyse yeniden yazmanız gerekmez.

```powershell
dotnet user-secrets set "DataMigration:Enabled" "true"
```

Bağlantı anahtarının adı tam olarak şudur:

```text
ConnectionStrings:AzureSql
```

Parola kaynak koda, `appsettings.json` dosyasına veya Git'e konmamalıdır.

## 5. SQL tablolarını oluşturun

Proje klasöründe:

```powershell
dotnet ef migrations add InitialAzureSql
dotnet ef database update
dotnet build
```

`database update` başarılı olduktan sonra Azure SQL'de tablolar oluşur.

## 6. Veriyi aktarın

Uygulamayı yerelde başlatın ve admin kullanıcıyla giriş yapın. Panel açıkken
tarayıcı geliştirici konsolunda aşağıdaki komutu çalıştırın:

```javascript
fetch("/api/VeriAktarimi/firestore-to-sql", {
    method: "POST",
    headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`
    }
})
.then(async r => ({ status: r.status, body: await r.json() }))
.then(console.log);
```

Sonuçta `Basarili: true`, eklenen kayıt sayıları, SQL toplamları ve varsa
eşleşmeyen kayıtlar görünür. Aktarım Firestore'dan hiçbir şey silmez.

Kontrol için:

```javascript
fetch("/api/VeriAktarimi/sql-sayilari", {
    headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`
    }
})
.then(r => r.json())
.then(console.table);
```

## 7. Güvenlik ve sonraki adım

Sayılar doğrulandıktan sonra aktarım uç noktasını kapatın:

```powershell
dotnet user-secrets set "DataMigration:Enabled" "false"
```

Render'a geçerken de `DataMigration__Enabled=false` kullanın. SQL bağlantısını
Render Environment alanına `ConnectionStrings__AzureSql` adıyla ekleyin.

Bu aşamada mevcut helper/controller kodları hâlâ Firestore ile çalışır. Veri
kontrolünden sonra Aşama 2'de CRUD işlemleri SQL helper'larına çevrilmelidir.
