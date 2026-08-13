using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IptasPeyzajApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialAzureSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Isler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EklentiNo = table.Column<int>(type: "int", nullable: false),
                    Tanim = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Isler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tc = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    TelefonNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CepTelefonNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: false),
                    Mail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    SifreHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Musteriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriNo = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tc = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cinsiyet = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CaddeSokak = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Mahalle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    No = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Daire = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Sehir = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: false),
                    MekanTipi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SozlesmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GorusmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriyodikBakim = table.Column<int>(type: "int", nullable: false),
                    PeriyodikBakimTuru = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BelirliGunler = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurumKodu = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musteriler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EskiPersonelId = table.Column<int>(type: "int", nullable: false),
                    PersonelNo = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Gorev = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DurumKodu = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BakimPlanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriId = table.Column<int>(type: "int", nullable: false),
                    MusteriNo = table.Column<int>(type: "int", nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BakimTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurumKodu = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IslemNotu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakimPlanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BakimPlanlari_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MusteriKullanicilari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    MusteriId = table.Column<int>(type: "int", nullable: false),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriKullanicilari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusteriKullanicilari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusteriKullanicilari_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teklifler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriId = table.Column<int>(type: "int", nullable: true),
                    MusteriNo = table.Column<int>(type: "int", nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TeklifTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DurumKodu = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IslemNotu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teklifler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teklifler_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "YapilacakIsler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriId = table.Column<int>(type: "int", nullable: true),
                    MusteriNo = table.Column<int>(type: "int", nullable: false),
                    EklentiNo = table.Column<int>(type: "int", nullable: false),
                    Not = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YapilacakIsler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YapilacakIsler_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "YapilmayacakIsler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriId = table.Column<int>(type: "int", nullable: true),
                    MusteriNo = table.Column<int>(type: "int", nullable: false),
                    EklentiNo = table.Column<int>(type: "int", nullable: false),
                    Not = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YapilmayacakIsler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YapilmayacakIsler_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BakimDetaylari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BakimId = table.Column<int>(type: "int", nullable: false),
                    ResimTip = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ResimUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DriveDosyaId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LegacyKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirestoreId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakimDetaylari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BakimDetaylari_BakimPlanlari_BakimId",
                        column: x => x.BakimId,
                        principalTable: "BakimPlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BakimPersonelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BakimId = table.Column<int>(type: "int", nullable: false),
                    PersonelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakimPersonelleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BakimPersonelleri_BakimPlanlari_BakimId",
                        column: x => x.BakimId,
                        principalTable: "BakimPlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BakimPersonelleri_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BakimDetaylari_BakimId",
                table: "BakimDetaylari",
                column: "BakimId");

            migrationBuilder.CreateIndex(
                name: "IX_BakimDetaylari_FirestoreId",
                table: "BakimDetaylari",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BakimDetaylari_LegacyKey",
                table: "BakimDetaylari",
                column: "LegacyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BakimPersonelleri_BakimId_PersonelId",
                table: "BakimPersonelleri",
                columns: new[] { "BakimId", "PersonelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BakimPersonelleri_PersonelId",
                table: "BakimPersonelleri",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_BakimPlanlari_BakimTarihi",
                table: "BakimPlanlari",
                column: "BakimTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_BakimPlanlari_FirestoreId",
                table: "BakimPlanlari",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BakimPlanlari_MusteriId_DurumKodu",
                table: "BakimPlanlari",
                columns: new[] { "MusteriId", "DurumKodu" });

            migrationBuilder.CreateIndex(
                name: "IX_Isler_EklentiNo",
                table: "Isler",
                column: "EklentiNo");

            migrationBuilder.CreateIndex(
                name: "IX_Isler_FirestoreId",
                table: "Isler",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_AktifMi",
                table: "Kullanicilar",
                column: "AktifMi");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_FirestoreId",
                table: "Kullanicilar",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKullanicilari_FirestoreId",
                table: "MusteriKullanicilari",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKullanicilari_KullaniciId_MusteriId",
                table: "MusteriKullanicilari",
                columns: new[] { "KullaniciId", "MusteriId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusteriKullanicilari_MusteriId",
                table: "MusteriKullanicilari",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_Musteriler_DurumKodu",
                table: "Musteriler",
                column: "DurumKodu");

            migrationBuilder.CreateIndex(
                name: "IX_Musteriler_FirestoreId",
                table: "Musteriler",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Musteriler_KayitTarihi",
                table: "Musteriler",
                column: "KayitTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_Musteriler_MusteriNo",
                table: "Musteriler",
                column: "MusteriNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_DurumKodu",
                table: "Personeller",
                column: "DurumKodu");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_EskiPersonelId",
                table: "Personeller",
                column: "EskiPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_FirestoreId",
                table: "Personeller",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_PersonelNo",
                table: "Personeller",
                column: "PersonelNo");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_DurumKodu",
                table: "Teklifler",
                column: "DurumKodu");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_FirestoreId",
                table: "Teklifler",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_MusteriId",
                table: "Teklifler",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_MusteriNo",
                table: "Teklifler",
                column: "MusteriNo");

            migrationBuilder.CreateIndex(
                name: "IX_YapilacakIsler_FirestoreId",
                table: "YapilacakIsler",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_YapilacakIsler_MusteriId",
                table: "YapilacakIsler",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_YapilmayacakIsler_FirestoreId",
                table: "YapilmayacakIsler",
                column: "FirestoreId",
                unique: true,
                filter: "[FirestoreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_YapilmayacakIsler_MusteriId",
                table: "YapilmayacakIsler",
                column: "MusteriId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BakimDetaylari");

            migrationBuilder.DropTable(
                name: "BakimPersonelleri");

            migrationBuilder.DropTable(
                name: "Isler");

            migrationBuilder.DropTable(
                name: "MusteriKullanicilari");

            migrationBuilder.DropTable(
                name: "Teklifler");

            migrationBuilder.DropTable(
                name: "YapilacakIsler");

            migrationBuilder.DropTable(
                name: "YapilmayacakIsler");

            migrationBuilder.DropTable(
                name: "BakimPlanlari");

            migrationBuilder.DropTable(
                name: "Personeller");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Musteriler");
        }
    }
}
