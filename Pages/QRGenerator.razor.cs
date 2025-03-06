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
            var buffer = new byte[LoadedFile.Size];
            await LoadedFile.OpenReadStream().ReadAsync(buffer);
            LogoPreview = $"data:{LoadedFile.ContentType};base64,{Convert.ToBase64String(buffer)}";

            var fileInByte = await GetBytesFromFile(LoadedFile);
            var logo = new SvgQRCode.SvgLogo(fileInByte);
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
    }
}
