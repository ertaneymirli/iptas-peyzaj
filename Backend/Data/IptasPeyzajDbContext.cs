using IptasPeyzajApi.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IptasPeyzajApi.Backend.Data;

public sealed class IptasPeyzajDbContext : DbContext
{
    public IptasPeyzajDbContext(
        DbContextOptions<IptasPeyzajDbContext> options)
        : base(options)
    {
    }

    public DbSet<MusteriEntity> Musteriler => Set<MusteriEntity>();
    public DbSet<BakimPlaniEntity> BakimPlanlari => Set<BakimPlaniEntity>();
    public DbSet<BakimDetayEntity> BakimDetaylari => Set<BakimDetayEntity>();
    public DbSet<BakimPersonelEntity> BakimPersonelleri => Set<BakimPersonelEntity>();
    public DbSet<KullaniciEntity> Kullanicilar => Set<KullaniciEntity>();
    public DbSet<PersonelEntity> Personeller => Set<PersonelEntity>();
    public DbSet<TeklifEntity> Teklifler => Set<TeklifEntity>();
    public DbSet<MusteriKullaniciEntity> MusteriKullanicilari =>
        Set<MusteriKullaniciEntity>();
    public DbSet<IsEntity> Isler => Set<IsEntity>();
    public DbSet<YapilacakIsEntity> YapilacakIsler => Set<YapilacakIsEntity>();
    public DbSet<YapilmayacakIsEntity> YapilmayacakIsler =>
        Set<YapilmayacakIsEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        MusteriAyarla(modelBuilder.Entity<MusteriEntity>());
        BakimPlaniAyarla(modelBuilder.Entity<BakimPlaniEntity>());
        BakimDetayAyarla(modelBuilder.Entity<BakimDetayEntity>());
        BakimPersonelAyarla(modelBuilder.Entity<BakimPersonelEntity>());
        KullaniciAyarla(modelBuilder.Entity<KullaniciEntity>());
        PersonelAyarla(modelBuilder.Entity<PersonelEntity>());
        TeklifAyarla(modelBuilder.Entity<TeklifEntity>());
        MusteriKullaniciAyarla(modelBuilder.Entity<MusteriKullaniciEntity>());
        IslerAyarla(modelBuilder);
    }

    private static void AnaEntityAyarla<TEntity>(
        EntityTypeBuilder<TEntity> entity,
        string tabloAdi)
        where TEntity : SqlEntity
    {
        entity.ToTable(tabloAdi);
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.Property(x => x.FirestoreId).HasMaxLength(256);
        entity.HasIndex(x => x.FirestoreId)
            .IsUnique()
            .HasFilter("[FirestoreId] IS NOT NULL");
    }

    private static void MusteriAyarla(EntityTypeBuilder<MusteriEntity> entity)
    {
        AnaEntityAyarla(entity, "Musteriler");
        entity.HasIndex(x => x.MusteriNo).IsUnique();
        entity.HasIndex(x => x.DurumKodu);
        entity.HasIndex(x => x.KayitTarihi);
        entity.Property(x => x.Ad).HasMaxLength(150);
        entity.Property(x => x.Soyad).HasMaxLength(150);
        entity.Property(x => x.Tc).HasMaxLength(11);
        entity.Property(x => x.Cinsiyet).HasMaxLength(30);
        entity.Property(x => x.Telefon).HasMaxLength(30);
        entity.Property(x => x.CaddeSokak).HasMaxLength(300);
        entity.Property(x => x.Mahalle).HasMaxLength(150);
        entity.Property(x => x.No).HasMaxLength(30);
        entity.Property(x => x.Daire).HasMaxLength(30);
        entity.Property(x => x.Sehir).HasMaxLength(100);
        entity.Property(x => x.Adres).HasMaxLength(750);
        entity.Property(x => x.MekanTipi).HasMaxLength(100);
        entity.Property(x => x.PeriyodikBakimTuru).HasMaxLength(60);
        entity.Property(x => x.BelirliGunler).HasMaxLength(200);
        entity.Property(x => x.Aciklama).HasMaxLength(2000);
        entity.Property(x => x.DurumKodu).HasMaxLength(2);
    }

    private static void BakimPlaniAyarla(
        EntityTypeBuilder<BakimPlaniEntity> entity)
    {
        AnaEntityAyarla(entity, "BakimPlanlari");
        entity.Property(x => x.AdSoyad).HasMaxLength(300);
        entity.Property(x => x.Telefon).HasMaxLength(30);
        entity.Property(x => x.DurumKodu).HasMaxLength(2);
        entity.Property(x => x.Aciklama).HasMaxLength(2000);
        entity.Property(x => x.IslemNotu).HasMaxLength(2000);
        entity.HasIndex(x => new { x.MusteriId, x.DurumKodu });
        entity.HasIndex(x => x.BakimTarihi);
        entity.HasOne(x => x.Musteri)
            .WithMany(x => x.BakimPlanlari)
            .HasForeignKey(x => x.MusteriId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void BakimDetayAyarla(
        EntityTypeBuilder<BakimDetayEntity> entity)
    {
        AnaEntityAyarla(entity, "BakimDetaylari");
        entity.Property(x => x.ResimTip).HasMaxLength(2);
        entity.Property(x => x.ResimUrl).HasMaxLength(1000);
        entity.Property(x => x.DriveDosyaId).HasMaxLength(256);
        entity.Property(x => x.LegacyKey).HasMaxLength(64);
        entity.HasIndex(x => x.LegacyKey).IsUnique();
        entity.HasIndex(x => x.BakimId);
        entity.HasOne(x => x.Bakim)
            .WithMany(x => x.Detaylar)
            .HasForeignKey(x => x.BakimId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void BakimPersonelAyarla(
        EntityTypeBuilder<BakimPersonelEntity> entity)
    {
        entity.ToTable("BakimPersonelleri");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).UseIdentityColumn();
        entity.HasIndex(x => new { x.BakimId, x.PersonelId }).IsUnique();
        entity.HasOne(x => x.Bakim)
            .WithMany(x => x.Personeller)
            .HasForeignKey(x => x.BakimId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.Personel)
            .WithMany(x => x.Bakimlar)
            .HasForeignKey(x => x.PersonelId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void KullaniciAyarla(
        EntityTypeBuilder<KullaniciEntity> entity)
    {
        AnaEntityAyarla(entity, "Kullanicilar");
        entity.Property(x => x.KullaniciAdi).HasMaxLength(150);
        entity.Property(x => x.Ad).HasMaxLength(150);
        entity.Property(x => x.Soyad).HasMaxLength(150);
        entity.Property(x => x.Tc).HasMaxLength(11);
        entity.Property(x => x.TelefonNo).HasMaxLength(30);
        entity.Property(x => x.CepTelefonNo).HasMaxLength(30);
        entity.Property(x => x.Adres).HasMaxLength(750);
        entity.Property(x => x.Mail).HasMaxLength(320);
        entity.Property(x => x.SifreHash).HasMaxLength(128);
        entity.Property(x => x.Rol).HasMaxLength(20);
        entity.HasIndex(x => x.KullaniciAdi).IsUnique();
        entity.HasIndex(x => x.AktifMi);
    }

    private static void PersonelAyarla(
        EntityTypeBuilder<PersonelEntity> entity)
    {
        AnaEntityAyarla(entity, "Personeller");
        entity.Property(x => x.Ad).HasMaxLength(150);
        entity.Property(x => x.Soyad).HasMaxLength(150);
        entity.Property(x => x.Telefon).HasMaxLength(30);
        entity.Property(x => x.Gorev).HasMaxLength(150);
        entity.Property(x => x.DurumKodu).HasMaxLength(2);
        entity.HasIndex(x => x.PersonelNo);
        entity.HasIndex(x => x.EskiPersonelId);
        entity.HasIndex(x => x.DurumKodu);
    }

    private static void TeklifAyarla(EntityTypeBuilder<TeklifEntity> entity)
    {
        AnaEntityAyarla(entity, "Teklifler");
        entity.Property(x => x.AdSoyad).HasMaxLength(300);
        entity.Property(x => x.Telefon).HasMaxLength(30);
        entity.Property(x => x.Aciklama).HasMaxLength(2000);
        entity.Property(x => x.DurumKodu).HasMaxLength(2);
        entity.Property(x => x.IslemNotu).HasMaxLength(2000);
        entity.Property(x => x.Tutar).HasPrecision(18, 2);
        entity.HasIndex(x => x.MusteriId);
        entity.HasIndex(x => x.MusteriNo);
        entity.HasIndex(x => x.DurumKodu);
        entity.HasOne(x => x.Musteri)
            .WithMany()
            .HasForeignKey(x => x.MusteriId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void MusteriKullaniciAyarla(
        EntityTypeBuilder<MusteriKullaniciEntity> entity)
    {
        AnaEntityAyarla(entity, "MusteriKullanicilari");
        entity.HasIndex(x => new { x.KullaniciId, x.MusteriId }).IsUnique();
        entity.HasOne(x => x.Kullanici)
            .WithMany(x => x.MusteriBaglantilari)
            .HasForeignKey(x => x.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.Musteri)
            .WithMany(x => x.KullaniciBaglantilari)
            .HasForeignKey(x => x.MusteriId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void IslerAyarla(ModelBuilder modelBuilder)
    {
        var isler = modelBuilder.Entity<IsEntity>();
        AnaEntityAyarla(isler, "Isler");
        isler.Property(x => x.Tanim).HasMaxLength(500);
        isler.HasIndex(x => x.EklentiNo);

        var yapilacak = modelBuilder.Entity<YapilacakIsEntity>();
        AnaEntityAyarla(yapilacak, "YapilacakIsler");
        yapilacak.Property(x => x.Not).HasMaxLength(2000);
        yapilacak.HasIndex(x => x.MusteriId);
        yapilacak.HasOne(x => x.Musteri)
            .WithMany()
            .HasForeignKey(x => x.MusteriId)
            .OnDelete(DeleteBehavior.SetNull);

        var yapilmayacak = modelBuilder.Entity<YapilmayacakIsEntity>();
        AnaEntityAyarla(yapilmayacak, "YapilmayacakIsler");
        yapilmayacak.Property(x => x.Not).HasMaxLength(2000);
        yapilmayacak.HasIndex(x => x.MusteriId);
        yapilmayacak.HasOne(x => x.Musteri)
            .WithMany()
            .HasForeignKey(x => x.MusteriId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
