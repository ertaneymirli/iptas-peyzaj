using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace IptasPeyzajApi.Backend.BakimPlanlari.Helpers;

public sealed class GoogleDriveStorage
{
    private readonly DriveService _drive;
    private readonly string _folderId;

    public GoogleDriveStorage(IConfiguration configuration)
    {
        string clientId = AyarGetir(
            configuration,
            "GoogleDrive:ClientId",
            "GOOGLE_DRIVE_CLIENT_ID");

        string clientSecret = AyarGetir(
            configuration,
            "GoogleDrive:ClientSecret",
            "GOOGLE_DRIVE_CLIENT_SECRET");

        string refreshToken = AyarGetir(
            configuration,
            "GoogleDrive:RefreshToken",
            "GOOGLE_DRIVE_REFRESH_TOKEN");

        _folderId = AyarGetir(
            configuration,
            "GoogleDrive:FolderId",
            "GOOGLE_DRIVE_FOLDER_ID");

        var flow = new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { DriveService.Scope.Drive }
            });

        var credential = new UserCredential(
            flow,
            "iptas-peyzaj-drive",
            new TokenResponse
            {
                RefreshToken = refreshToken
            });

        _drive = new DriveService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "IPTAS Peyzaj"
            });
    }

    public async Task<string> JpegYukleAsync(
        Stream jpeg,
        string dosyaAdi,
        CancellationToken cancellationToken = default)
    {
        var metadata = new DriveFile
        {
            Name = dosyaAdi,
            Parents = new List<string> { _folderId }
        };

        var request = _drive.Files.Create(
            metadata,
            jpeg,
            "image/jpeg");

        request.Fields = "id";

        IUploadProgress progress =
            await request.UploadAsync(cancellationToken);

        if (progress.Status != UploadStatus.Completed ||
            string.IsNullOrWhiteSpace(request.ResponseBody?.Id))
        {
            throw progress.Exception
                ?? new Exception("Resim Google Drive'a yüklenemedi.");
        }

        return request.ResponseBody.Id;
    }

    public async Task<DriveResim> ResimIndirAsync(
        string dosyaId,
        CancellationToken cancellationToken = default)
    {
        var bilgiIstegi = _drive.Files.Get(dosyaId);
        bilgiIstegi.Fields = "name,mimeType";
        DriveFile bilgi =
            await bilgiIstegi.ExecuteAsync(cancellationToken);

        using var stream = new MemoryStream();
        var indirmeIstegi = _drive.Files.Get(dosyaId);
        var progress = await indirmeIstegi.DownloadAsync(
            stream,
            cancellationToken);

        if (progress.Status != Google.Apis.Download.DownloadStatus.Completed)
        {
            throw progress.Exception
                ?? new Exception("Resim Google Drive'dan indirilemedi.");
        }

        return new DriveResim(
            stream.ToArray(),
            string.IsNullOrWhiteSpace(bilgi.MimeType)
                ? "image/jpeg"
                : bilgi.MimeType,
            bilgi.Name ?? "bakim-resmi.jpg");
    }

    private static string AyarGetir(
        IConfiguration configuration,
        string configurationKey,
        string environmentKey)
    {
        string? deger = configuration[configurationKey]
            ?? Environment.GetEnvironmentVariable(environmentKey);

        if (string.IsNullOrWhiteSpace(deger))
        {
            throw new InvalidOperationException(
                $"Eksik Google Drive ayarı: {environmentKey}");
        }

        return deger;
    }
}

public sealed record DriveResim(
    byte[] Bytes,
    string ContentType,
    string DosyaAdi);
