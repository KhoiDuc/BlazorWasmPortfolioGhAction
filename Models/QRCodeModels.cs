using QRCoder;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlazorWasmPortfolioGhAction.Shared.Model
{
    public enum EnumQrType
    {
        [Display(Name = "Normal Text")]
        Text,
        Url,
        [Display(Name = "Phone Number")]
        PhoneNumber
    }

    public enum ImageType
    {
        PNG,
        JPG,
        JPEG
    }

    public class QRCodeRequestModel
    {
        public string QRValue { get; set; } = "https://github.com/KhoiDuc";
        public EnumQrType QRType { get; set; } = EnumQrType.Url;
        public SvgQRCode.SvgLogo? Logo { get; set; }
        public string DarkColorHex { get; set; } = "#A9A9A9";
        public string WhiteColorHex { get; set; } = "#ffffff";
    }

    public class QRCodeResponseModel
    {
        public string? SvgString { get; set; }
        public byte[]? ByteData => SvgString != null ? Encoding.UTF8.GetBytes(SvgString) : null;
        public string? Base64String => ByteData is not null ? Convert.ToBase64String(ByteData) : null;
    }
}
