using BlazorWasmPortfolioGhAction.Extensions;
using BlazorWasmPortfolioGhAction.Shared.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QRCoder;
using System.Text.RegularExpressions;

namespace BlazorWasmPortfolioGhAction.Pages
{
    public partial class QRGenerator
    {
        private QRCodeRequestModel Request { get; set; } = new();
        private IBrowserFile? LoadedFile { get; set; }
        private string? QRImage { get; set; }
        private byte[]? QRByte { get; set; }
        private const int MaxAllowedFileCount = 1;

        protected override void OnInitialized()
        {
            Request ??= new();
            GenerateQR();
        }

        private void OnSelectChange(ChangeEventArgs e)
        {
            var value = e?.Value?.ToString() ?? EnumQrType.Text.ToString();
            Request.QRType = value!.ToEnum<EnumQrType>();
            GenerateQR();
        }

        private void OnInputChange(ChangeEventArgs e)
        {
            Request.QRValue = e?.Value?.ToString()!;
            GenerateQR();
        }

        private void OnWhiteColorHexChange(ChangeEventArgs e)
        {
            Request.WhiteColorHex = e?.Value?.ToString()!;
            GenerateQR();
        }

        private void OnDarkColorHexChange(ChangeEventArgs e)
        {
            Request.DarkColorHex = e?.Value?.ToString()!;
            GenerateQR();
        }
        private string validationError = string.Empty;

        private void ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(Request.QRValue))
            {
                validationError = "QR Content cannot be empty!";
                return;
            }

            if (!IsValidHexColor(Request.DarkColorHex) || !IsValidHexColor(Request.WhiteColorHex))
            {
                validationError = "Invalid color format! Use hex codes (e.g. #FFFFFF)";
                return;
            }

            validationError = string.Empty;
        }
        private void RemoveLogo()
        {
            LogoPreview = null;
            Request.Logo = null;
            GenerateQR();
        }

        private bool IsValidHexColor(string color) =>
            Regex.IsMatch(color, "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
        private string? LogoPreview { get; set; }

        private async Task OnLogoChange(InputFileChangeEventArgs e)
        {
            int maxSize = 1024 * 1024 * 5;
            LoadedFile = e.GetMultipleFiles(MaxAllowedFileCount).FirstOrDefault();

            if (LoadedFile == null) return;

            if (!LoadedFile.ContentType.Contains("image/"))
            {
                validationError = "Only image files are allowed!";
                return;
            }

            if (LoadedFile.Size > maxSize)
            {
                validationError = "File size exceeds 5MB limit!";
                return;
            }
            var buffer = await GetBytesFromFile(LoadedFile);
            LogoPreview = $"data:{LoadedFile.ContentType};base64,{Convert.ToBase64String(buffer)}";

            var logo = new SvgQRCode.SvgLogo(buffer);
            Request.Logo = logo;
            GenerateQR();
        }

        private bool isLoading = false;

        private async void GenerateQR()
        {
            isLoading = true;
            StateHasChanged();

            await Task.Delay(10); // Allow UI to update

            try
            {
                var response = _qrService.GenerateQR(Request);
                QRImage = $"data:image/svg+xml;base64,{response?.Base64String}";
                QRByte = response?.ByteData;
            }
            catch (Exception ex)
            {
                // Handle error (add error message display)
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task<byte[]> GetBytesFromFile(IBrowserFile file, int maxSize = 1024 * 1024 * 5)
        {
            var fileStream = file.OpenReadStream();
            var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            return ms.ToArray();
        }

        private async Task DownloadImage()
        {
            if (QRByte is null) return;
            var fileStream = new MemoryStream(QRByte);
            var fileName = "QRCode.svg";
            using var streamRef = new DotNetStreamReference(stream: fileStream);
            await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }

        // --- Stilo.Documents Barcode Generator ---
        public enum ModeTab { Qr, Barcode }
        private ModeTab ActiveTab { get; set; } = ModeTab.Qr;

        private Stilo.Documents.Bars.BarcodeType SelectedBarcodeType { get; set; } = Stilo.Documents.Bars.BarcodeType.Code128;
        private string BarcodeValue { get; set; } = "PORTFOLIO-12345";
        private int BarcodeModuleSize { get; set; } = 3;
        private int BarcodeHeight { get; set; } = 50;
        private string? BarcodeImage { get; set; }
        private byte[]? BarcodeBytes { get; set; }
        private string? BarcodeError { get; set; }
        private bool isBarcodeLoading { get; set; }

        private readonly List<Stilo.Documents.Bars.BarcodeType> SupportedBarcodeTypes = new()
        {
            Stilo.Documents.Bars.BarcodeType.Code128,
            Stilo.Documents.Bars.BarcodeType.Code39,
            Stilo.Documents.Bars.BarcodeType.Code93,
            Stilo.Documents.Bars.BarcodeType.Ean13,
            Stilo.Documents.Bars.BarcodeType.Ean8,
            Stilo.Documents.Bars.BarcodeType.Codabar,
            Stilo.Documents.Bars.BarcodeType.I25,
            Stilo.Documents.Bars.BarcodeType.Pdf417
        };

        private void SetTab(ModeTab tab)
        {
            ActiveTab = tab;
            if (ActiveTab == ModeTab.Barcode && string.IsNullOrEmpty(BarcodeImage))
            {
                GenerateBarcode();
            }
        }

        private void OnBarcodeTypeChange(ChangeEventArgs e)
        {
            if (Enum.TryParse<Stilo.Documents.Bars.BarcodeType>(e?.Value?.ToString(), out var parsed))
            {
                SelectedBarcodeType = parsed;
                if (SelectedBarcodeType == Stilo.Documents.Bars.BarcodeType.Ean13 && (string.IsNullOrWhiteSpace(BarcodeValue) || !BarcodeValue.All(char.IsDigit) || BarcodeValue.Length != 13))
                {
                    BarcodeValue = "1234567890128";
                }
                else if (SelectedBarcodeType == Stilo.Documents.Bars.BarcodeType.Ean8 && (string.IsNullOrWhiteSpace(BarcodeValue) || !BarcodeValue.All(char.IsDigit) || BarcodeValue.Length != 8))
                {
                    BarcodeValue = "12345670";
                }
                GenerateBarcode();
            }
        }

        private void OnBarcodeValueChange(ChangeEventArgs e)
        {
            BarcodeValue = e?.Value?.ToString() ?? string.Empty;
            GenerateBarcode();
        }

        private void OnBarcodeModuleSizeChange(ChangeEventArgs e)
        {
            if (int.TryParse(e?.Value?.ToString(), out var size) && size >= 1 && size <= 10)
            {
                BarcodeModuleSize = size;
                GenerateBarcode();
            }
        }

        private void OnBarcodeHeightChange(ChangeEventArgs e)
        {
            if (int.TryParse(e?.Value?.ToString(), out var height) && height >= 20 && height <= 200)
            {
                BarcodeHeight = height;
                GenerateBarcode();
            }
        }

        private void GenerateBarcode()
        {
            BarcodeError = null;
            if (string.IsNullOrWhiteSpace(BarcodeValue))
            {
                BarcodeError = "Barcode value cannot be empty.";
                BarcodeImage = null;
                BarcodeBytes = null;
                return;
            }

            try
            {
                isBarcodeLoading = true;
                BarcodeBytes = Stilo.Documents.Bars.BarcodeImageWriter.GeneratePng(SelectedBarcodeType, BarcodeValue.Trim(), BarcodeModuleSize, BarcodeHeight);
                BarcodeImage = $"data:image/png;base64,{Convert.ToBase64String(BarcodeBytes)}";
            }
            catch (Exception ex)
            {
                BarcodeError = $"Cannot generate {SelectedBarcodeType}: {ex.Message}";
                BarcodeImage = null;
                BarcodeBytes = null;
            }
            finally
            {
                isBarcodeLoading = false;
            }
        }

        private async Task DownloadBarcode()
        {
            if (BarcodeBytes is null) return;
            var fileStream = new MemoryStream(BarcodeBytes);
            var safeVal = string.Concat(BarcodeValue.Trim().Split(Path.GetInvalidFileNameChars())).Replace(" ", "-");
            var fileName = $"Barcode_{SelectedBarcodeType}_{safeVal}.png";
            using var streamRef = new DotNetStreamReference(stream: fileStream);
            await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }
    }
}
