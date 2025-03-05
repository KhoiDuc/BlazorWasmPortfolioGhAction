using BlazorWasmPortfolioGhAction.Shared.Model;
using QRCoder;

namespace BlazorWasmPortfolioGhAction.Store.Services;

public class QRCodeService
{
    public QRCodeResponseModel GenerateQR(QRCodeRequestModel requestModel)
    {
        var responseModel = new QRCodeResponseModel();
        var qrCodeData = GetQRCodeData(requestModel.QRValue, requestModel.QRType);

        SvgQRCode svgQrCode = new SvgQRCode(qrCodeData);

        var svgImg = svgQrCode.GetGraphic(20, darkColorHex: requestModel.WhiteColorHex,
            lightColorHex: requestModel.DarkColorHex,
            logo: requestModel.Logo);
        responseModel.SvgString = svgImg;
        //SaveImage(responseModel.ByteData!);
        return responseModel;
    }

    private QRCodeData GetQRCodeData(string text, EnumQrType qrType)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrType switch
        {
            EnumQrType.Url => qrGenerator.CreateQrCode(new PayloadGenerator.Url(text), QRCodeGenerator.ECCLevel.Q),
            EnumQrType.PhoneNumber => qrGenerator.CreateQrCode(new PayloadGenerator.PhoneNumber(text),
                QRCodeGenerator.ECCLevel.Q),
            EnumQrType.Text or _ => qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q)
        };

        return qrCodeData;
    }

    public void SaveImage(byte[] qrCodeImage)
    {
        var filePath = "D:\\QRImage.png";
        File.WriteAllBytes(filePath, qrCodeImage);
    }
}
