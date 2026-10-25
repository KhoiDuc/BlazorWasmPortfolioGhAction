using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Pages;
using Xunit;

namespace BlazorWasmPortfolioGhAction.Tests;

public class AuthSeriesBuilder
{
    public static DevSeries BuildSeries()
    {
        var series = new DevSeries
        {
            Id = "dotnet-auth-monolith-to-microservices-series",
            Category = "backend",
            Title = "Authentication & Security Architecture in .NET: From Monolith to Microservices (Cookies, JWT, IdP, Gateway & BFF Pattern)",
            TitleVi = "Bảo Mật & Xác Thực Trong .NET: Từ Monolith Đến Microservices (Session Cookie, Stateless JWT, IdP, Gateway & Mẫu Thiết Kế BFF)",
            Icon = "bi-shield-check",
            Color = "#8b5cf6",
            Badge = "Security & Identity Architecture",
            Description = "Master the complete authentication and authorization evolution in .NET: Stateful session cookies and ASP.NET Core Identity in Monoliths, stateless JWT with Refresh Token Rotation (RTR), centralized identity management with OpenID Connect / OAuth 2.0 (OpenIddict, Keycloak, Auth0, Okta), API Gateway-level token validation, and the production-grade Backend-For-Frontend (BFF) pattern.",
            DescriptionVi = "Làm chủ toàn bộ lộ trình tiến hóa xác thực và phân quyền trong .NET: Từ Stateful Session Cookie và ASP.NET Core Identity trong Monolith, chuyển dịch sang Stateless JWT kết hợp Refresh Token Rotation (RTR), tập trung hóa định danh với OAuth 2.0 / OIDC (OpenIddict, Keycloak, Auth0, Okta), xác thực tầng API Gateway đến mẫu thiết kế phòng thủ tối thượng Backend-For-Frontend (BFF).",
            Articles = new List<DevArticle>()
        };

        // =========================================================================
        // BÀI 1: MONOLITH AUTHENTICATION
        // =========================================================================
        var article1 = new DevArticle
        {
            Id = "dotnet-auth-article-1-monolith-session-cookie-identity",
            SeriesId = series.Id,
            LessonNumber = "Bài 1",
            Level = "Beginner",
            LevelVi = "Cơ bản",
            LevelOrder = 1,
            Title = "Authentication in .NET Monoliths: Session Cookies, ASP.NET Core Identity & Protection Against XSS/CSRF",
            TitleVi = "Xác Thực Trong .NET Monolith: Session Cookie, ASP.NET Core Identity & Phòng Thủ Lỗ Hổng XSS/CSRF",
            ReadingTime = "15 phút đọc",
            PublishedDate = "26/10/2026",
            Summary = "Master foundational security in monolithic architectures: Stateful session cookies, ASP.NET Core Identity, Data Protection API cryptographic key management, security flags (HttpOnly, SameSite, Secure), Anti-Forgery tokens against CSRF, and horizontal session scaling with Redis.",
            SummaryVi = "Khám phá nền tảng bảo mật trong ứng dụng Monolith: Cơ chế Stateful Session Cookie, ASP.NET Core Identity, Data Protection API mã hóa cookie, thiết lập cờ bảo mật HttpOnly, SameSite, Secure, kỹ thuật chống tấn công CSRF bằng Anti-Forgery Token và chiến lược scale session phân tán với Redis.",
            Tags = new List<string> { "Authentication", "ASP.NET Core Identity", "Session Cookie", "XSS", "CSRF", "Redis Session", "Data Protection" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Stateful Session & Cookie Authentication in ASP.NET Core",
                    HeadingVi = "1. Bản Chất Stateful Session & Cookie Authentication Trong ASP.NET Core",
                    Paragraphs = new List<string>
                    {
                        "In traditional monolithic web applications (ASP.NET Core MVC, Razor Pages, or server-rendered apps), Cookie-based Authentication is the de facto standard. When a user logs in with their credentials, the server verifies the user via ASP.NET Core Identity, creates a ClaimsPrincipal, and serializes this identity into an encrypted authentication cookie named '.AspNetCore.Cookies'.",
                        "Crucially, the cookie payload is not plaintext JSON. ASP.NET Core utilizes the built-in Data Protection API to symmetrically encrypt and sign the cookie payload. The client browser merely stores this opaque blob and automatically includes it in the 'Cookie' request header for all subsequent HTTP requests to the same origin domain.",
                        "Because the server maintains session state or decrypts the self-contained ticket on every request, revoking a session upon password change or administrative account lockout is instantaneous: clearing the session key in Redis or invalidating the SecurityStamp immediately blocks the user."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong các ứng dụng Monolith truyền thống (ASP.NET Core MVC, Razor Pages hoặc ứng dụng Render phía máy chủ), cơ chế Xác thực bằng Cookie (Cookie-based Authentication) là tiêu chuẩn vàng. Khi người dùng nhập tên đăng nhập và mật khẩu, máy chủ kiểm tra thông tin qua ASP.NET Core Identity, tạo đối tượng ClaimsPrincipal chứa danh sách quyền, rồi tuần tự hóa danh tính này vào một Cookie mã hóa mang tên '.AspNetCore.Cookies'.",
                        "Điểm cốt lõi là dữ liệu cookie không phải chuỗi JSON thông thường. ASP.NET Core sử dụng hệ thống Data Protection API để mã hóa đối xứng và ký số toàn bộ nội dung ticket. Trình duyệt client chỉ lưu trữ chuỗi nhị phân mờ đục này và tự động đính kèm vào header 'Cookie' trong mọi request tiếp theo gửi về cùng tên miền.",
                        "Nhờ duy trì trạng thái phiên (Stateful) hoặc kiểm tra SecurityStamp mỗi khi giải mã cookie, việc thu hồi phiên đăng nhập khi người dùng đổi mật khẩu hoặc bị quản trị viên khóa tài khoản diễn ra tức thì: chỉ cần xóa session key trong Redis hoặc cập nhật SecurityStamp là user bị đá ra ngay lập tức."
                    },
                    CodeTitle = "Hardened Cookie Authentication Configuration in ASP.NET Core Program.cs",
                    CodeLanguage = "csharp",
                    CodeContent = """
var builder = WebApplication.CreateBuilder(args);

// Cấu hình Cookie Authentication với các cờ bảo mật tối thượng
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-AuthSession"; // Tiền tố __Host- ngăn ngừa tấn công subdomain injection
        options.Cookie.HttpOnly = true;             // CHỐNG XSS: Tuyệt đối không cho JavaScript (document.cookie) đọc được
        options.Cookie.SameSite = SameSiteMode.Lax; // CHỐNG CSRF: Không tự động gửi cookie khi cross-site POST
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Bắt buộc HTTPS, không bao giờ truyền qua HTTP thường
        
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Phiên đăng nhập hết hạn sau 8 tiếng
        options.SlidingExpiration = true;               // Tự động gia hạn nếu người dùng còn hoạt động
        options.LoginPath = "/auth/login";
        options.AccessDeniedPath = "/auth/access-denied";
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
""",
                    CalloutType = "tip",
                    CalloutTitle = "Why the __Host- Cookie Prefix Matters",
                    CalloutTitleVi = "Bí Quyết: Sử Dụng Tiền Tố __Host- Cho Cookie",
                    CalloutText = "By prefixing your cookie name with '__Host-', modern browsers enforce three strict rules: the cookie must be sent only over HTTPS, cannot include subdomains (must match the exact host), and the path must be '/'. This prevents attackers on compromised subdomains from injecting rogue session cookies.",
                    CalloutTextVi = "Bằng cách đặt tiền tố '__Host-' cho tên cookie, các trình duyệt hiện đại sẽ ép buộc 3 quy tắc thép: cookie chỉ được gửi qua HTTPS, không cho phép chia sẻ sang subdomain (phải khớp chính xác tên miền gốc) và đường dẫn phải là '/'. Điều này triệt tiêu hoàn toàn nguy cơ kẻ gian lợi dụng subdomain bị lộ để giả mạo session."
                },
                new DevArticleSection
                {
                    Heading = "2. Shielding Against the Dual Nemeses: XSS and CSRF",
                    HeadingVi = "2. Lá Chắn Phòng Thủ Kép: Chống Lỗ Hổng XSS và CSRF",
                    Paragraphs = new List<string>
                    {
                        "Cookie-based authentication has one monumental advantage over client-stored tokens: when configured with 'HttpOnly = true', malicious JavaScript injected via Cross-Site Scripting (XSS) cannot access, read, or export the session cookie.",
                        "However, cookies introduce a different threat: Cross-Site Request Forgery (CSRF). Because browsers automatically attach cookies to cross-origin requests, a malicious website could trick a logged-in victim's browser into submitting a hidden form to 'POST /api/bank/transfer' without the user's consent.",
                        "ASP.NET Core eliminates CSRF through two synchronized layers: setting 'SameSite=Lax' (or 'Strict') so the browser refuses to send cookies on cross-site POSTs, and enforcing Anti-Forgery Tokens (Synchronizer Token Pattern). A cryptographic token is rendered inside an HTML form and validated on form submission using '[ValidateAntiForgeryToken]'."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Xác thực bằng Cookie có một ưu điểm vượt trội không thể chối cãi so với việc lưu Token phía client: Khi được bật cờ 'HttpOnly = true', các đoạn mã JavaScript độc hại được kẻ tấn công chèn vào qua lỗ hổng XSS (Cross-Site Scripting) hoàn toàn không có cách nào đọc hoặc trích xuất được cookie phiên đăng nhập.",
                        "Tuy nhiên, điểm yếu cố hữu của Cookie lại nằm ở nguy cơ Tấn Công Giả Mạo Yêu Cầu (CSRF - Cross-Site Request Forgery). Vì trình duyệt tự động đính kèm cookie mỗi khi gửi request, một trang web độc hại có thể lừa nạn nhân bấm vào một đường link kích hoạt form ẩn 'POST /api/bank/transfer' mà người dùng không hề hay biết.",
                        "ASP.NET Core bảo vệ hệ thống trước CSRF bằng 2 lớp phòng thủ đồng bộ: thiết lập cờ 'SameSite=Lax' (hoặc 'Strict') ngăn trình duyệt tự động nhả cookie trong các request cross-site, kết hợp cùng cơ chế Anti-Forgery Token (Mẫu Synchronizer Token). Một mã băm bảo mật được nhúng vào form HTML và được kiểm tra nghiêm ngặt phía server qua thuộc tính '[ValidateAntiForgeryToken]'."
                    },
                    CodeTitle = "Anti-Forgery Token Verification & Auto-Validation in ASP.NET Core",
                    CodeLanguage = "csharp",
                    CodeContent = """
// 1. Tự động áp dụng kiểm tra Anti-Forgery Token cho TẤT CẢ các phương thức POST, PUT, DELETE
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// 2. Trong Form HTML Razor (Tự động sinh hidden input __RequestVerificationToken):
// <form asp-action="TransferMoney" method="post">
//     @Html.AntiForgeryToken()
//     <input name="amount" value="500" />
//     <button type="submit">Chuyển Tiền</button>
// </form>

// 3. Trong Controller C#:
[HttpPost]
[ValidateAntiForgeryToken] // Kiểm tra token mã hóa khớp với cookie chống CSRF
public async Task<IActionResult> TransferMoney(decimal amount)
{
    // Xử lý chuyển tiền an toàn 100% không sợ giả mạo request
    return Ok();
}
""",
                    CalloutType = "warning",
                    CalloutTitle = "SameSite=None Requires Explicit CSRF Defenses",
                    CalloutTitleVi = "Cảnh Báo: Tuyệt Đối Không Dùng SameSite=None Bừa Bãi",
                    CalloutText = "Setting 'SameSite = SameSiteMode.None' disables browser-level cross-origin cookie restrictions entirely. Only use this if your API is specifically designed to be embedded in third-party iframes, and ensure strict Anti-Forgery token or custom header checks (e.g. X-Requested-With) are rigorously verified.",
                    CalloutTextVi = "Thiết lập 'SameSite = SameSiteMode.None' sẽ vô hiệu hóa hoàn toàn hàng rào bảo vệ chống cross-site của trình duyệt. Chỉ sử dụng tùy chọn này nếu trang web của bạn bắt buộc phải nhúng trong iframe của bên thứ ba, và phải luôn có lớp bảo vệ Anti-Forgery Token hoặc kiểm tra Header tùy biến chặt chẽ."
                },
                new DevArticleSection
                {
                    Heading = "3. Horizontal Scaling for Monoliths: Redis Session & Data Protection Clustering",
                    HeadingVi = "3. Mở Rộng Cụm Monolith (Scale-Out): Đồng Bộ Session & Khóa Data Protection Qua Redis",
                    Paragraphs = new List<string>
                    {
                        "When your Monolith scales from 1 server to a load-balanced cluster of 5 servers behind NGINX, standard Cookie Authentication instantly breaks. If Server A encrypts the cookie using its local in-memory machine key, Server B cannot decrypt it, resulting in users randomly getting logged out on every subsequent request.",
                        "To resolve this, you must decouple two stateful elements: 1) Persist and synchronize the ASP.NET Core Data Protection cryptographic keyring in a centralized storage (such as Redis or Azure Blob Storage), and 2) Store user session data in a distributed cache like Redis.",
                        "With synchronized Data Protection keys and Redis distributed caching, any server in the cluster can seamlessly decrypt, validate, and renew authentication cookies, achieving linear horizontal scaling without sticky sessions."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khi ứng dụng Monolith mở rộng từ 1 máy chủ lên cụm 5 máy chủ phía sau NGINX, cơ chế Cookie Authentication mặc định sẽ bị gãy vụn. Nếu Máy chủ A mã hóa cookie bằng khóa riêng lưu trong RAM cục bộ của nó, Máy chủ B sẽ không thể giải mã được, khiến người dùng bị văng đăng nhập liên tục sau mỗi lần F5 hoặc chuyển trang.",
                        "Để giải quyết triệt để, bạn bắt buộc phải tách rời 2 thành phần trạng thái: 1) Lưu trữ và đồng bộ hóa chùm chìa khóa mã hóa (Keyring) của Data Protection API tại một nơi tập trung (như Redis hoặc Azure Blob), và 2) Lưu trữ dữ liệu phiên (Session data) trên cụm Redis Distributed Cache.",
                        "Khi tất cả các máy chủ cùng đọc chung bộ khóa mã hóa từ Redis, bất kỳ máy chủ nào trong cụm cũng có thể giải mã, xác thực và gia hạn cookie một cách mượt mà, cho phép hệ thống mở rộng tải không giới hạn mà không cần cấu hình Sticky Sessions."
                    },
                    CodeTitle = "Clustering Data Protection Keys with StackExchange.Redis in ASP.NET Core",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Cài đặt NuGet:
// dotnet add package Microsoft.AspNetCore.DataProtection.StackExchangeRedis
// dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis

using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 1. Kết nối cụm Redis tập trung
var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// 2. Đồng bộ hóa chùm khóa mã hóa Data Protection cho toàn bộ cụm Monolith
builder.Services.AddDataProtection()
    .SetApplicationName("EnterpriseMonolithApp") // Tên ứng dụng phải KHỚP NHAU trên mọi máy chủ
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-MasterKeys");

// 3. Cấu hình Distributed Session lưu trữ trên Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MonolithSession_";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
""",
                    CalloutType = "tip",
                    CalloutTitle = "SetApplicationName is Mandatory Across Clustered Nodes",
                    CalloutTitleVi = "Lưu Ý Sống Còn: Phải Khai Báo SetApplicationName Giống Nhau",
                    CalloutText = "By default, ASP.NET Core isolates Data Protection keys based on the physical content root path of the application. When deploying to multiple servers or Docker containers, explicitly specify SetApplicationName(\"MyApp\") so all instances share the exact same cryptographic key partition.",
                    CalloutTextVi = "Mặc định, ASP.NET Core tự phân chia khóa Data Protection dựa theo đường dẫn thư mục cài đặt ứng dụng. Khi deploy lên nhiều server hoặc các container Docker khác nhau, bạn bắt buộc phải gọi SetApplicationName(\"MyApp\") để mọi node cùng chia sẻ chung một vùng khóa mã hóa."
                }
            }
        };
        series.Articles.Add(article1);

        // =========================================================================
        // BÀI 2: STATELESS JWT & REFRESH TOKEN ROTATION
        // =========================================================================
        var article2 = new DevArticle
        {
            Id = "dotnet-auth-article-2-stateless-jwt-refresh-token-rotation",
            SeriesId = series.Id,
            LessonNumber = "Bài 2",
            Level = "Intermediate",
            LevelVi = "Trung cấp",
            LevelOrder = 2,
            Title = "The Shift to Stateless JWT: Access Tokens, Refresh Token Rotation (RTR) & Secure Storage Strategies",
            TitleVi = "Bước Chuyển Dịch Sang Stateless JWT: Access Token, Refresh Token Rotation (RTR) & Chiến Lược Lưu Trữ An Toàn",
            ReadingTime = "17 phút đọc",
            PublishedDate = "26/10/2026",
            Summary = "Why monolithic session cookies fail when separating frontend SPAs (React, Vue, Blazor) and Native Mobile Apps. Master RFC 7519 Stateless JWT mechanics, Access Token vs Refresh Token lifecycles, OAuth 2.1 Refresh Token Rotation (RTR) detecting replay attacks, SHA-256 token hashing, and resolving the eternal debate: HttpOnly Cookie versus LocalStorage.",
            SummaryVi = "Khi tách biệt Frontend SPA (React/Vue/Blazor) và Mobile App: Vì sao Monolith Session phá sản? Bản chất Stateless JWT (RFC 7519), chu kỳ sống Access Token vs Refresh Token, cơ chế Refresh Token Rotation (RTR) theo chuẩn OAuth 2.1 phát hiện Replay Attack, băm SHA-256 bảo vệ database và cuộc chiến lưu trữ: HttpOnly Cookie vs LocalStorage.",
            Tags = new List<string> { "JWT", "OAuth 2.1", "Refresh Token Rotation", "Stateless", "Security", "Token Storage", "SPA" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Why SPAs & Mobile Apps Break Monolithic Sessions: The Rise of Stateless JWT",
                    HeadingVi = "1. Vì Sao SPA & Mobile Đòi Hỏi Stateless Authentication? Giải Phẫu JWT (RFC 7519)",
                    Paragraphs = new List<string>
                    {
                        "As architectures evolve toward decoupled Single Page Applications (React, Vue, Blazor WASM) and Native Mobile Apps (iOS, Android), traditional server-rendered sessions struggle. Mobile operating systems do not manage browser cookies transparently, cross-origin API calls trigger complex CORS preflight checks, and querying a centralized database on every single API request creates severe database bottlenecks.",
                        "JSON Web Tokens (JWT - RFC 7519) solve this through cryptographic self-containment. A JWT consists of three Base64Url-encoded segments: Header (specifying signing algorithm like RS256), Payload (storing claims such as sub, exp, role), and Signature (cryptographic proof generated by the issuer's private key).",
                        "Because the token is cryptographically signed, any resource server can verify its authenticity and read user claims using only the issuer's public key. The server does not maintain session state or query databases to authenticate requests: this is the essence of Stateless Authentication."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khi kiến trúc chuyển dịch sang mô hình tách biệt Frontend SPA (React, Vue, Blazor WASM) và Native Mobile App (iOS, Android), cơ chế Session truyền thống bộc lộ nhiều hạn chế. Ứng dụng di động không quản lý cookie tự nhiên như trình duyệt, các lệnh gọi API khác tên miền (cross-origin) vướng kiểm tra CORS phức tạp, và việc phải truy vấn database để kiểm tra session trong từng request tạo ra nút thắt cổ chai hiệu năng nghiêm trọng.",
                        "JSON Web Token (JWT - RFC 7519) giải quyết triệt để vấn đề này nhờ cơ chế tự chứa dữ liệu (Cryptographically Self-Contained). Một chuỗi JWT gồm 3 phần mã hóa Base64Url ghép lại: Header (khai báo thuật toán ký ví dụ RS256), Payload (chứa các thông tin định danh như sub, exp, role), và Chữ ký số Signature (được tạo bởi Private Key của máy chủ phát hành).",
                        "Vì token đã được ký số chống sửa đổi, bất kỳ máy chủ API nào cũng có thể kiểm tra tính hợp lệ và đọc trực tiếp thông tin người dùng chỉ bằng việc dùng Public Key giải mã chữ ký. Máy chủ hoàn toàn không cần lưu trạng thái phiên (Stateless) hay truy vấn cơ sở dữ liệu."
                    },
                    CodeTitle = "Stateless JWT Anatomy & Asymmetric RS256 Verification in .NET",
                    CodeLanguage = "csharp",
                    CodeContent = """
// ==================== CẤU HÌNH JWT BEARER TRONG ASP.NET CORE ====================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://auth.mycompany.com",
            ValidateAudience = true,
            ValidAudience = "api://order-service",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsaPublicKey), // Dùng khóa bất đối xứng RS256
            ClockSkew = TimeSpan.FromSeconds(30) // Giảm độ trễ dung sai từ 5 phút mặc định xuống 30s
        };
    });

// ==================== CẤU TRÚC CHUẨN CỦA MỘT CHUỖI JWT ====================
// eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.          <-- HEADER (Thuật toán RS256)
// eyJzdWIiOiIxMjM0NTYiLCJuYW1lIjoiS2hvaSIs...    <-- PAYLOAD (Claims định danh)
// D6bY7yZ0J5...                                  <-- SIGNATURE (Chữ ký chống giả mạo)
""",
                    CalloutType = "warning",
                    CalloutTitle = "Beware of the Stateless Revocation Paradox",
                    CalloutTitleVi = "Nghịch Lý Của Stateless: Không Thể Thu Hồi Token Bị Lộ Ngay Lập Tức",
                    CalloutText = "The biggest strength of JWT is also its greatest danger: because servers do not check a database, if an Access Token is stolen, the attacker can use it freely until it expires. Never issue long-lived Access Tokens (keep them strictly between 5 to 15 minutes).",
                    CalloutTextVi = "Ưu điểm lớn nhất của JWT cũng chính là hiểm họa lớn nhất: vì máy chủ không kiểm tra database khi xác thực, nếu một Access Token bị kẻ gian đánh cắp, chúng có thể sử dụng hợp lệ cho đến khi token hết hạn. Tuyệt đối không cấp Access Token sống lâu (chỉ nên từ 5 đến 15 phút)."
                },
                new DevArticleSection
                {
                    Heading = "2. OAuth 2.1 Refresh Token Rotation (RTR): Catching Token Theft in the Act",
                    HeadingVi = "2. Cơ Chế Refresh Token Rotation (RTR) & Băm Token Bảo Vệ Cơ Sở Dữ Liệu",
                    Paragraphs = new List<string>
                    {
                        "To balance usability and security, modern architectures use a dual-token system: a short-lived Access Token (10 minutes) and a long-lived Refresh Token (7 days). When the Access Token expires, the client silently presents the Refresh Token to obtain a fresh Access Token.",
                        "However, what happens if the Refresh Token itself is intercepted? OAuth 2.1 mandates 'Refresh Token Rotation' (RTR). Every time a Refresh Token is redeemed, the server immediately invalidates it and issues a brand-new Refresh Token, maintaining an unbroken cryptographic 'Token Family'.",
                        "If a malicious actor steals a Refresh Token and tries to redeem it after the legitimate user already rotated it, the server detects a 'Replay Attack'. The system instantly revokes the entire Token Family, invalidating all active sessions for that account and forcing a full re-login."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Để cân bằng giữa trải nghiệm người dùng và tính bảo mật, hệ thống hiện đại áp dụng mô hình 2 token: Access Token sống ngắn (10 phút) và Refresh Token sống dài (7 ngày). Khi Access Token hết hạn, client âm thầm gửi Refresh Token lên server để nhận lại Access Token mới mà người dùng không bị văng ra.",
                        "Thế nhưng, điều gì xảy ra nếu chính Refresh Token bị đánh cắp? Chuẩn bảo mật OAuth 2.1 bắt buộc phải triển khai cơ chế 'Refresh Token Rotation' (RTR). Mỗi lần Refresh Token được sử dụng để lấy token mới, máy chủ lập tức hủy bỏ nó và cấp lại một Refresh Token hoàn toàn mới, duy trì một dòng dõi liên kết gọi là 'Token Family'.",
                        "Nếu kẻ gian đánh cắp Refresh Token và cố tình gửi lên máy chủ sau khi người dùng hợp lệ đã thực hiện xoay vòng token trước đó, hệ thống sẽ phát hiện hành vi tấn công lặp lại (Replay Attack). Máy chủ sẽ lập tức thu hồi toàn bộ gia đình token (Token Family), vô hiệu hóa mọi phiên đăng nhập của tài khoản đó ngay lập tức."
                    },
                    CodeTitle = "Refresh Token Rotation with Token Family Revocation & SHA-256 Hashing",
                    CodeLanguage = "csharp",
                    CodeContent = """
public async Task<TokenResponse> RotateTokenAsync(string rawRefreshToken, string ipAddress)
{
    // 1. Luôn băm Refresh Token bằng SHA-256 trước khi tìm kiếm trong Database
    var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));
    var existingToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    if (existingToken == null)
        throw new SecurityTokenException("Token không tồn tại hoặc không hợp lệ.");

    // 2. PHÁT HIỆN TẤN CÔNG REPLAY: Token đã từng bị thu hồi nhưng lại được gửi lên lần nữa!
    if (existingToken.IsRevoked)
    {
        // Kẻ trộm đang cố dùng token cũ! HỦY TOÀN BỘ GIA ĐÌNH TOKEN CỦA TÀI KHOẢN NÀY!
        await RevokeEntireTokenFamilyAsync(existingToken.FamilyId, "Phát hiện Replay Attack - Nghi vấn lộ Token");
        throw new SecurityException("Cảnh báo bảo mật: Phiên đăng nhập đã bị vô hiệu hóa toàn bộ.");
    }

    // 3. Vô hiệu hóa token hiện tại
    existingToken.IsRevoked = true;
    existingToken.RevokedAt = DateTime.UtcNow;

    // 4. Sinh cặp Token mới và gắn vào cùng FamilyId
    var newRawRefreshToken = GenerateCryptoRandomString(64);
    var newTokenRecord = new RefreshToken
    {
        FamilyId = existingToken.FamilyId,
        UserId = existingToken.UserId,
        TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(newRawRefreshToken))),
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        CreatedByIp = ipAddress
    };

    _db.RefreshTokens.Add(newTokenRecord);
    await _db.SaveChangesAsync();

    var newAccessToken = _jwtService.GenerateAccessToken(existingToken.UserId);
    return new TokenResponse(newAccessToken, newRawRefreshToken);
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Never Store Raw Refresh Tokens in Plaintext",
                    CalloutTitleVi = "Bắt Buộc Băm Refresh Token Bằng SHA-256 Trước Khi Lưu DB",
                    CalloutText = "Treat Refresh Tokens with the exact same gravity as user passwords. Always hash them with SHA-256 before persisting to your database. If an attacker dumps your SQL database via an injection vulnerability, hashed tokens cannot be used to forge authentication requests.",
                    CalloutTextVi = "Hãy đối xử với Refresh Token với mức độ nghiêm ngặt tương tự như mật khẩu người dùng. Luôn băm bằng thuật toán SHA-256 trước khi lưu vào Database hoặc Redis. Nếu cơ sở dữ liệu bị rò rỉ qua lỗ hổng SQL Injection, kẻ tấn công cũng không thể mang các chuỗi băm đó đi đổi lấy Access Token mới."
                },
                new DevArticleSection
                {
                    Heading = "3. The Storage Dilemma: LocalStorage vs HttpOnly Cookie in SPAs",
                    HeadingVi = "3. Cuộc Chiến Lưu Trữ: LocalStorage vs HttpOnly Cookie Trong SPA & Mobile",
                    Paragraphs = new List<string>
                    {
                        "Where should a Single Page Application store tokens? Storing tokens in browser 'localStorage' or 'sessionStorage' is notoriously hazardous: any Cross-Site Scripting (XSS) vulnerability (even in a third-party npm package) gives attackers single-line access: 'localStorage.getItem(\"token\")'.",
                        "The hardened approach is storing the long-lived Refresh Token inside a 'SameSite=Strict, HttpOnly, Secure' Cookie. JavaScript cannot read this cookie; the browser automatically presents it only to the '/api/auth/refresh' endpoint.",
                        "The short-lived Access Token is kept strictly in-memory (in a JavaScript variable or state management store). When the page refreshes, the app silently calls '/api/auth/refresh' to retrieve a fresh in-memory Access Token without ever exposing long-lived credentials to DOM-based script attacks."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trình duyệt nên lưu trữ Token ở đâu là câu hỏi gây tranh cãi lớn nhất. Lưu token trong 'localStorage' hoặc 'sessionStorage' tiềm ẩn rủi ro tử huyệt: bất kỳ một lỗ hổng XSS nào (thậm chí xuất phát từ một thư viện npm bên thứ ba) cũng cho phép kẻ gian chỉ bằng 1 dòng lệnh JavaScript cướp sạch token: 'localStorage.getItem(\"token\")'.",
                        "Chiến lược chuẩn hóa là lưu trữ Refresh Token sống dài bên trong một Cookie có cờ 'SameSite=Strict, HttpOnly, Secure'. Mã JavaScript hoàn toàn không thể chạm vào cookie này; trình duyệt chỉ tự động gửi nó lên đúng endpoint duy nhất là '/api/auth/refresh'.",
                        "Trong khi đó, Access Token sống ngắn (10 phút) được lưu trực tiếp trong bộ nhớ RAM của ứng dụng (biến In-Memory). Mỗi khi người dùng F5 tải lại trang, ứng dụng sẽ âm thầm gọi API refresh để nhận lại Access Token mới trong bộ nhớ mà không bao giờ phơi bày chìa khóa phiên trước các cuộc tấn công qua mã độc script."
                    },
                    CodeTitle = "Hardened Token Storage Architecture for React/Blazor Applications",
                    CodeLanguage = "text",
                    CodeContent = """
=========================================================================
CHIẾN LƯỢC LƯU TRỮ TOKEN TỐI ƯU CHỐNG 100% NGUY CƠ ĐÁNH CẮP XSS
=========================================================================
[Browser DOM / JavaScript]
   │
   ├── Access Token (Sống 10 phút) ──► LƯU TRONG BỘ NHỚ RAM (IN-MEMORY)
   │                                   (Mất đi khi F5, JS đọc được nhưng rủi ro thấp)
   │
   └── Refresh Token (Sống 7 ngày)  ──► LƯU TRONG HTTPONLY COOKIE
                                       (JavaScript KHÔNG THỂ ĐỌC ĐƯỢC!)
                                       (Trình duyệt tự đính kèm khi gọi /api/auth/refresh)

Khi F5 hoặc Token hết hạn:
App ──(Tự động gửi HttpOnly Cookie)──► POST /api/auth/refresh ──► Nhận lại Access Token mới!
""",
                    CalloutType = "warning",
                    CalloutTitle = "Never Store Access Tokens in LocalStorage in Production",
                    CalloutTitleVi = "Cảnh Báo: Tuyệt Đối Tránh Lưu Token Trong LocalStorage Trên Production",
                    CalloutText = "Over 80% of Web security breaches targeting client tokens originate from XSS reading localStorage. Storing credentials in HttpOnly cookies or utilizing the BFF pattern eliminates this attack vector completely.",
                    CalloutTextVi = "Hơn 80% các vụ tấn công đánh cắp phiên đăng nhập nhắm vào client bắt nguồn từ việc hacker đọc trộm localStorage qua XSS. Sử dụng HttpOnly Cookie hoặc mẫu thiết kế BFF sẽ loại bỏ hoàn toàn bề mặt tấn công nguy hiểm này."
                }
            }
        };
        series.Articles.Add(article2);

        // =========================================================================
        // BÀI 3: CENTRALIZED IDENTITY PROVIDERS (OIDC & OAUTH 2.0)
        // =========================================================================
        var article3 = new DevArticle
        {
            Id = "dotnet-auth-article-3-centralized-idp-oidc-oauth2",
            SeriesId = series.Id,
            LessonNumber = "Bài 3",
            Level = "Advanced",
            LevelVi = "Nâng cao",
            LevelOrder = 3,
            Title = "Centralized Identity with OAuth 2.0 & OIDC: Self-Hosted (OpenIddict/Keycloak) vs Cloud IdP (Auth0/Okta)",
            TitleVi = "Tập Trung Hóa Định Danh Với OAuth 2.0 & OIDC: Tự Host (OpenIddict/Keycloak) vs Dùng Đám Mây (Auth0/Okta)",
            ReadingTime = "18 phút đọc",
            PublishedDate = "26/10/2026",
            Summary = "Solve the multi-service identity fragmentation crisis: Stop maintaining scattered Users tables across 15 microservices. Master OpenID Connect (OIDC) & OAuth 2.0 foundations, contrast Self-Hosted solutions (OpenIddict in C#, Keycloak in Docker) with Cloud-Managed IdPs (Auth0, Okta, Microsoft Entra ID), and automate JWT public-key validation via JWKS endpoints.",
            SummaryVi = "Giải quyết bài toán 10 microservices không thể dùng chung 10 bảng Users: Chuẩn OpenID Connect (OIDC) & OAuth 2.0, vai trò Identity Provider (IdP) độc lập. So sánh trực diện Tự host (OpenIddict thuần .NET, Keycloak Docker) vs Đám mây quản trị sẵn (Auth0, Okta, Microsoft Entra ID). Cấu hình xác thực tự động qua endpoint JWKS.",
            Tags = new List<string> { "OAuth 2.0", "OpenID Connect", "OIDC", "Keycloak", "OpenIddict", "Auth0", "Okta", "JWKS" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Microservices Identity Crisis: Decoupling Authentication from Authorization",
                    HeadingVi = "1. Cuộc Khủng Hoảng Định Danh Trong Microservices: Tách Rời AuthN Khỏi AuthZ",
                    Paragraphs = new List<string>
                    {
                        "When an engineering organization splits a monolith into 15 independent microservices, a recurring anti-pattern emerges: each microservice creates its own 'Users' table, manages its own password hashing, and attempts to authenticate users independently. This leads to configuration drift, duplicate credentials, security patch nightmares, and impossible Single Sign-On (SSO).",
                        "The architectural remedy is a Centralized Identity Provider (IdP) operating on OpenID Connect (OIDC) and OAuth 2.0. Authentication ('AuthN' - verifying WHO the user is) is completely delegated to the IdP. Microservices act strictly as Resource Servers, focusing purely on Authorization ('AuthZ' - verifying WHAT the user is allowed to do).",
                        "The gold-standard protocol is Authorization Code Flow with PKCE (Proof Key for Code Exchange). Users authenticate at the IdP's login page, MFA is evaluated, and the IdP issues cryptographically signed Access Tokens (RS256) which downstream microservices validate statelessly."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khi hệ thống phân tách thành hàng chục microservice độc lập, một sai lầm chết người thường xảy ra: mỗi dịch vụ tự tạo một bảng 'Users' riêng, tự băm mật khẩu và tự xử lý đăng nhập. Điều này dẫn đến sự phân mảnh dữ liệu, tài khoản trùng lặp, ác mộng khi vá lỗ hổng bảo mật và hoàn toàn bất lực trong việc triển khai Đăng Nhập Một Lần (Single Sign-On - SSO).",
                        "Giải pháp chuẩn mực là tách riêng một máy chủ Định Danh Tập Trung (Identity Provider - IdP) vận hành theo chuẩn OpenID Connect (OIDC) và OAuth 2.0. Bài toán Xác thực ('AuthN' - xác minh NGƯỜI ĐÓ LÀ AI) được ủy quyền hoàn toàn cho IdP. Các Microservices chỉ đóng vai trò là các Resource Server thuần túy, chỉ tập trung vào Phân quyền ('AuthZ' - người này ĐƯỢC PHÉP LÀM GÌ).",
                        "Luồng xác thực chuẩn quốc tế hiện nay là Authorization Code Flow with PKCE (Proof Key for Code Exchange). Người dùng đăng nhập tại giao diện tập trung của IdP, vượt qua các bước bảo vệ MFA/Passkeys, và IdP sẽ cấp phát Access Token được ký số (RS256) để các Microservices kiểm tra một cách độc lập."
                    },
                    CodeTitle = "Authorization Code Flow with PKCE Protocol Architecture",
                    CodeLanguage = "text",
                    CodeContent = """
=========================================================================
LUỒNG XÁC THỰC CHUẨN QUỐC TẾ: AUTHORIZATION CODE FLOW VỚI PKCE
=========================================================================
[Browser / Mobile App]               [Identity Provider (IdP)]            [Microservices]
        │                                       │                                │
        │─── 1. Sinh Code Challenge & Login ───►│                                │
        │       (Chuyển hướng đến IdP Login)   │                                │
        │                                       │ (Người dùng nhập mật khẩu/MFA) │
        │◄── 2. Trả về Authorization Code ──────│                                │
        │                                       │                                │
        │─── 3. Đổi Code + Code Verifier ──────►│ (Kiểm tra khớp Challenge)     │
        │◄── 4. Cấp Access Token + ID Token ────│                                │
        │                                                                        │
        │─── 5. Gửi Request: Header Authorization: Bearer <AccessToken> ────────►│
        │                                                                        │ (Tự validate qua JWKS)
        │◄── 6. Trả về dữ liệu JSON ─────────────────────────────────────────────│
""",
                    CalloutType = "tip",
                    CalloutTitle = "PKCE is Now Mandatory for All Applications",
                    CalloutTitleVi = "Chuẩn OAuth 2.1: Bắt Buộc Áp Dụng PKCE Cho Mọi Loại Ứng Dụng",
                    CalloutText = "Previously, PKCE was recommended only for public clients (SPAs and mobile apps). OAuth 2.1 mandates PKCE for all authorization code flows, including server-side confidential web apps, completely preventing authorization code interception attacks.",
                    CalloutTextVi = "Trước đây, PKCE chỉ được khuyến nghị cho Public Clients (SPA và Mobile). Chuẩn OAuth 2.1 mới nhất bắt buộc áp dụng PKCE cho TẤT CẢ các luồng Authorization Code (kể cả web app truyền thống), loại bỏ hoàn toàn nguy cơ mã ủy quyền bị kẻ gian đánh chặn giữa đường."
                },
                new DevArticleSection
                {
                    Heading = "2. Decision Matrix: Self-Hosted (OpenIddict/Keycloak) vs Cloud IdP (Auth0/Okta)",
                    HeadingVi = "2. Ma Trận Quyết Định: Tự Host (OpenIddict/Keycloak) vs Đám Mây (Auth0/Okta)",
                    Paragraphs = new List<string>
                    {
                        "When choosing an IdP, engineering teams face a crucial architectural crossroad: host their own identity server or purchase a cloud-managed SaaS IdP.",
                        "Self-Hosted OpenIddict is an open-source, flexible, high-performance OAuth 2.0 / OIDC framework built natively for ASP.NET Core and EF Core. It gives complete code-level customization with zero licensing costs. Keycloak (by Red Hat) is a battle-tested enterprise standalone Docker server providing out-of-the-box user federation (LDAP/Active Directory), administrative UIs, and realm management.",
                        "Cloud-Managed IdPs like Auth0 and Okta offer zero maintenance, 99.99% SLA uptime, automated anomaly detection (blocking credential stuffing attacks), and turnkey biometric WebAuthn/Passkey integration. However, their monthly pricing scales sharply as Monthly Active Users (MAU) climb into the tens of thousands."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khi lựa chọn Identity Provider, đội ngũ kiến trúc đứng trước ngã rẽ quan trọng: Tự vận hành máy chủ định danh (Self-Hosted) hay mua dịch vụ SaaS trên đám mây (Cloud-Managed IdP).",
                        "OpenIddict là framework OAuth 2.0 / OIDC mã nguồn mở đỉnh cao được viết thuần túy cho ASP.NET Core và EF Core. Nó cho phép tùy biến mã nguồn sâu sắc, kiểm soát 100% cơ sở dữ liệu và hoàn toàn miễn phí bản quyền. Keycloak (của Red Hat) là máy chủ chạy container Docker độc lập với đầy đủ giao diện web quản trị, liên kết danh mục người dùng (LDAP/AD) và phân chia vùng quản trị (Realms) cực mạnh.",
                        "Ngược lại, các giải pháp đám mây như Auth0 và Okta mang lại sự an tâm tuyệt đối: cam kết 99.99% Uptime, không tốn nhân lực bảo trì, tự động phát hiện đăng nhập bất thường và hỗ trợ sẵn đăng nhập sinh trắc học Passkeys. Dù vậy, chi phí hàng tháng sẽ tăng rất nhanh khi số lượng người dùng hoạt động (MAU) vượt ngưỡng hàng chục ngàn tài khoản."
                    },
                    CodeTitle = "Comprehensive Comparison: Self-Hosted vs Cloud Managed Identity Providers",
                    CodeLanguage = "json",
                    CodeContent = """
{
  "IdentityComparison": "Self-Hosted vs Cloud Managed IdP",
  "Platforms": [
    {
      "Name": "OpenIddict (.NET Native)",
      "Type": "Self-Hosted C# Library",
      "Cost": "100% Miễn phí (Mã nguồn mở)",
      "BestFor": "Team .NET muốn nhúng trực tiếp IdP vào hệ thống, toàn quyền kiểm soát SQL",
      "Maintenance": "Team tự lo sao lưu DB, vá lỗi và mở rộng cụm"
    },
    {
      "Name": "Keycloak (Red Hat)",
      "Type": "Self-Hosted Standalone Docker",
      "Cost": "100% Miễn phí (Mã nguồn mở)",
      "BestFor": "Hệ thống Microservices đa ngôn ngữ, cần giao diện Admin UI và đồng bộ LDAP",
      "Maintenance": "Cần kỹ sư DevOps vận hành cụm Keycloak + PostgreSQL"
    },
    {
      "Name": "Auth0 / Okta",
      "Type": "Cloud Managed SaaS",
      "Cost": "Tính tiền theo MAU (Miễn phí 7,000 MAU, sau đó tăng dần)",
      "BestFor": "Startup cần Go-Live ngay, Doanh nghiệp lớn cần đạt chuẩn SOC2/ISO 27001",
      "Maintenance": "Zero-Ops (Nhà cung cấp quản lý 100% hạ tầng)"
    }
  ]
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Data Sovereignty Law Considerations",
                    CalloutTitleVi = "Yếu Tố Pháp Lý & Chủ Quyền Dữ Liệu (Data Sovereignty)",
                    CalloutText = "In regulated sectors (banking, healthcare, government), legal frameworks often forbid storing citizen identity and password hashes outside sovereign national borders. In these scenarios, self-hosting Keycloak or OpenIddict within local data centers is often legally mandated.",
                    CalloutTextVi = "Trong các ngành tài chính, ngân hàng và y tế, luật an ninh mạng quốc gia thường nghiêm cấm việc lưu trữ dữ liệu định danh và thông tin xác thực của công dân trên các đám mây đặt ngoài lãnh thổ. Trong trường hợp đó, tự host Keycloak hoặc OpenIddict tại các Data Center nội địa là yêu cầu pháp lý bắt buộc."
                },
                new DevArticleSection
                {
                    Heading = "3. Zero-Secret Resource Server Setup via Automatic JWKS Endpoints",
                    HeadingVi = "3. Cấu Hình Resource Server Không Cần Secret Key Qua JWKS Endpoint",
                    Paragraphs = new List<string>
                    {
                        "In legacy systems, verifying a token required sharing a symmetric secret key across all microservices: if Service 1 was compromised, the attacker extracted the secret and forged tokens for Service 2 through 10.",
                        "Modern OIDC architectures utilize Asymmetric Key Cryptography (RS256/ES256) combined with the JSON Web Key Set (JWKS) standard. The IdP holds the Private Signing Key; downstream microservices only need the IdP's base URL ('Authority').",
                        "When an ASP.NET Core microservice boots up, Microsoft's JwtBearer middleware queries 'https://idp.example.com/.well-known/openid-configuration' to automatically discover the JWKS URI. It caches the public RSA keys in-memory. When the IdP rotates signing keys, the middleware automatically refreshes the public keys without downtime or code deployment."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong các hệ thống đời cũ, việc kiểm tra token đòi hỏi phải chia sẻ chung một chuỗi Secret đối xứng cho tất cả các microservice: Nếu Microservice 1 bị hack, kẻ tấn công sẽ cướp được secret này và tự tạo token giả mạo để xâm nhập vào toàn bộ các service từ 2 đến 10.",
                        "Kiến trúc OIDC hiện đại sử dụng Ký Bất Đối Xứng (RS256/ES256) kết hợp với chuẩn JWKS (JSON Web Key Set). Máy chủ IdP giữ bí mật tuyệt đối Private Key; các microservice chỉ cần biết địa chỉ gốc ('Authority') của IdP.",
                        "Khi một microservice ASP.NET Core khởi động, middleware JwtBearer sẽ tự động truy vấn endpoint 'https://idp.example.com/.well-known/openid-configuration' để tìm địa chỉ JWKS. Nó tải về và lưu tạm Public Key vào bộ nhớ RAM. Khi IdP xoay vòng đổi chìa khóa mới định kỳ, microservice sẽ tự động đồng bộ lại Public Key mà không cần khởi động lại ứng dụng hay deploy code."
                    },
                    CodeTitle = "Production Resource Server Configuration with Automatic Key Discovery",
                    CodeLanguage = "csharp",
                    CodeContent = """
var builder = WebApplication.CreateBuilder(args);

// Cấu hình Microservice làm Resource Server: KHÔNG CẦN HARD-CODE BẤT KỲ SECRET NÀO!
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Địa chỉ máy chủ IdP (Auth0, Okta hoặc Keycloak)
        options.Authority = builder.Configuration["Authentication:Authority"] ?? "https://idp.company.com/realms/production";
        options.Audience = "api://order-microservice";
        
        // Tự động tải Public Key qua JWKS và xác thực chữ ký
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Bảo vệ endpoint bằng Policy
app.MapGet("/api/orders", () => Results.Ok(new[] { "Order 101", "Order 102" }))
   .RequireAuthorization();

app.Run();
""",
                    CalloutType = "tip",
                    CalloutTitle = "JWKS Keys Are Cached Automatically",
                    CalloutTitleVi = "Cơ Chế Bộ Đệm Khóa Tự Động Không Tốn Băng Thông",
                    CalloutText = "The ASP.NET Core JwtBearer handler automatically caches downloaded public keys and only refetches from the IdP if an incoming token presents a Key ID ('kid') not present in the local cache, ensuring zero performance penalty on standard requests.",
                    CalloutTextVi = "Middleware JwtBearer của ASP.NET Core tự động lưu bộ đệm các khóa công khai và chỉ gửi request tải lại từ IdP khi gặp một token chứa mã Key ID ('kid') mới tinh chưa có trong RAM, đảm bảo không gây bất kỳ độ trễ mạng nào cho các request thông thường."
                }
            }
        };
        series.Articles.Add(article3);

        // =========================================================================
        // BÀI 4: GATEWAY AUTHENTICATION & SERVICE-TO-SERVICE SECURITY
        // =========================================================================
        var article4 = new DevArticle
        {
            Id = "dotnet-auth-article-4-gateway-auth-token-exchange-mtls",
            SeriesId = series.Id,
            LessonNumber = "Bài 4",
            Level = "Production",
            LevelVi = "Thực chiến",
            LevelOrder = 4,
            Title = "Microservices Gateway Security: API Gateway Authentication, Token Exchange & Service-to-Service mTLS",
            TitleVi = "Bảo Mật Cửa Ngõ Microservices: Xác Thực Tầng API Gateway (Kong/YARP), Token Exchange & Giao Tiếp Nội Bộ mTLS",
            ReadingTime = "19 phút đọc",
            PublishedDate = "26/10/2026",
            Summary = "Eliminate redundant token validation across 40 microservices. Centralize edge authentication at the API Gateway (Kong, NGINX, YARP), securely forward user identity headers using HMAC internal signatures, implement Token Exchange (RFC 8693) for downscoped permissions, and secure service-to-service communication using OAuth 2.0 Client Credentials and Mutual TLS (mTLS).",
            SummaryVi = "Không để từng microservice tự đi validate token bên ngoài. Kiến trúc xác thực tập trung tại API Gateway (Kong/YARP), kỹ thuật chuyển đổi Claims sang Header nội bộ an toàn (HMAC Internal Signature), mô hình Token Exchange (RFC 8693) và bảo vệ giao tiếp liên dịch vụ (Service-to-Service) bằng OAuth2 Client Credentials & Mutual TLS (mTLS).",
            Tags = new List<string> { "API Gateway", "YARP", "Kong", "Token Exchange", "mTLS", "Client Credentials", "Microservices" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Gateway-Level Authentication Offloading: Defense at the Perimeter",
                    HeadingVi = "1. Chốt Chặn Tiền Tuyến: Xác Thực Tập Trung Tại API Gateway",
                    Paragraphs = new List<string>
                    {
                        "Allowing public internet traffic to penetrate directly into internal microservices is a catastrophic security vulnerability. If you have 40 microservices, each service must handle CORS, validate SSL, negotiate JWT signatures, and manage IP rate limits independently.",
                        "The battle-tested architectural pattern is Perimeter Authentication at the API Gateway (using tools like Kong or .NET YARP). The Gateway acts as the sole public-facing bastion: it inspects incoming requests, terminates SSL, validates the Bearer JWT against the IdP, enforces global rate-limiting, and rejects unauthorized traffic before it ever touches your internal network.",
                        "Once authenticated, the Gateway transforms the external token into trusted internal identity headers (such as 'X-User-Id', 'X-User-Roles', 'X-Tenant-Id') and proxies the request into the private microservice network."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Để lưu lượng truy cập từ Internet công cộng đi thẳng vào từng microservice nội bộ là một lỗ hổng kiến trúc nghiêm trọng. Nếu hệ thống có 40 dịch vụ, từng dịch vụ phải tự xử lý CORS, kiểm tra chứng chỉ SSL, xác thực chữ ký JWT và tự quản lý chặn IP độc hại một cách rời rạc.",
                        "Mẫu thiết kế chuẩn hóa của các hệ thống lớn là Xác Thực Tiền Tuyến tại API Gateway (sử dụng Kong hoặc .NET YARP). Gateway đóng vai trò là pháo đài duy nhất tiếp xúc với Internet: nó giải mã SSL, xác thực chuỗi Bearer JWT với IdP, áp dụng hạn ngạch gọi API (Rate Limiting) và thẳng tay từ chối các request không hợp lệ ngay từ cửa ngõ trước khi chúng kịp chạm vào mạng nội bộ.",
                        "Sau khi xác thực thành công, Gateway bóc tách các Claims quan trọng thành các Header nội bộ tin cậy (như 'X-User-Id', 'X-User-Roles', 'X-Tenant-Id') rồi mới chuyển tiếp request tới các microservice phía sau."
                    },
                    CodeTitle = "YARP API Gateway Configuration Offloading Authentication & Enriching Headers",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Cấu hình YARP Gateway làm chốt chặn xác thực tập trung
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        // Khi request đã qua kiểm tra Authentication tại Gateway:
        builderContext.AddRequestTransform(async transformContext =>
        {
            var user = transformContext.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                // Bóc tách claims và gắn vào Header nội bộ cho microservices backend đọc
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var roles = string.Join(",", user.FindAll(ClaimTypes.Role).Select(r => r.Value));
                
                transformContext.ProxyRequest.Headers.Add("X-User-Id", userId);
                transformContext.ProxyRequest.Headers.Add("X-User-Roles", roles);
                
                // Ký bảo vệ Header nội bộ bằng HMAC để chống Pod giả mạo
                var signature = ComputeHmacSignature(userId, roles, "InternalSharedSecret123!");
                transformContext.ProxyRequest.Headers.Add("X-Internal-Signature", signature);
            }
            await Task.CompletedTask;
        });
    });

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.Run();
""",
                    CalloutType = "warning",
                    CalloutTitle = "Never Trust Raw Internal Headers Without Verification",
                    CalloutTitleVi = "Cảnh Báo: Luôn Ký Bảo Vệ Header Nội Bộ Chống Giả Mạo",
                    CalloutText = "Never blindly trust 'X-User-Id' headers in backend services without verifying their origin. Malicious actors who breach perimeter routing could inject rogue 'X-User-Id: admin' headers. Always strip external X- headers at the gateway and use cryptographic HMAC signatures or internal mTLS.",
                    CalloutTextVi = "Tuyệt đối không nhắm mắt tin tưởng các header 'X-User-Id' ở các service phía sau mà không kiểm tra nguồn gốc. Nếu một kẻ tấn công gửi thẳng request kèm header 'X-User-Id: admin', hệ thống sẽ bị chiếm quyền. Hãy luôn xóa sạch các header X- gửi từ ngoài Internet tại Gateway và sử dụng chữ ký HMAC nội bộ hoặc mTLS."
                },
                new DevArticleSection
                {
                    Heading = "2. Service-to-Service Security: Client Credentials Grant vs Mutual TLS (mTLS)",
                    HeadingVi = "2. Giao Tiếp Liên Dịch Vụ (Service-to-Service): Client Credentials vs Mutual TLS (mTLS)",
                    Paragraphs = new List<string>
                    {
                        "In microservices, services frequently communicate with one another without an active end-user context (e.g. Order Service calling Payment Service, or background reconciliation jobs). How do you authenticate machine-to-machine (M2M) communication?",
                        "Method 1: OAuth 2.0 Client Credentials Grant. Service A authenticates against the IdP using its own ClientId and ClientSecret, receiving an M2M Access Token scoped specifically for Service B.",
                        "Method 2: Mutual TLS (mTLS) with Service Mesh (Istio / Linkerd). Rather than handling token negotiation in application code, cryptographic authentication occurs at the transport layer: each pod has an x509 certificate automatically issued and rotated by the mesh. The sidecar proxies enforce bidirectional cryptographic identity verification, encrypting 100% of internal traffic with zero application code overhead."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong kiến trúc Microservices, các dịch vụ liên tục gọi chéo lẫn nhau mà không có sự hiện diện của người dùng (ví dụ: Order Service tự gọi Payment Service để thanh toán, hoặc các tác vụ chạy ngầm định kỳ). Làm thế nào để xác thực an toàn giữa máy với máy (Machine-to-Machine - M2M)?",
                        "Phương pháp 1: OAuth 2.0 Client Credentials Grant. Service A tự xác thực với máy chủ IdP bằng ClientId và ClientSecret riêng của nó để nhận về một Access Token M2M có phạm vi (scope) dành riêng cho Service B.",
                        "Phương pháp 2: Mutual TLS (mTLS) kết hợp Service Mesh (Istio / Linkerd). Thay vì phải viết code xin token trong ứng dụng, việc xác thực diễn ra ở tầng truyền tải mạng: mỗi Pod được cấp một chứng chỉ số x509 tự động quay vòng. Các Sidecar Proxy tự động bắt tay mã hóa 2 chiều, đảm bảo 100% lưu lượng mạng nội bộ đạt chuẩn Zero-Trust mà không tốn một dòng code nào của lập trình viên."
                    },
                    CodeTitle = "Service-to-Service Authentication: OAuth2 Client Credentials Handler in C#",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Đăng ký HttpClient gọi liên dịch vụ tự động kèm M2M Bearer Token
builder.Services.AddHttpClient("PaymentServiceClient", client =>
{
    client.BaseAddress = new Uri("http://payment-service");
})
.AddHttpMessageHandler<ClientCredentialsTokenHandler>();

public class ClientCredentialsTokenHandler : DelegatingHandler
{
    private readonly ITokenCacheService _tokenCache;
    public ClientCredentialsTokenHandler(ITokenCacheService tokenCache) => _tokenCache = tokenCache;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Lấy token M2M từ cache (nếu hết hạn thì tự động gọi IdP xin token mới qua Client Credentials)
        var m2mToken = await _tokenCache.GetOrCreateM2MTokenAsync("payment-api-scope");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", m2mToken);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Token Exchange (RFC 8693) for Downscoped Delegation",
                    CalloutTitleVi = "Mẫu Token Exchange (RFC 8693) Khi Cần Hạ Quyền Nội Bộ",
                    CalloutText = "When Service A needs to call Service B on behalf of a user, forwarding the user's original broad token violates the Principle of Least Privilege. Use OAuth 2.0 Token Exchange (RFC 8693) to trade the user's broad token for a new token restricted strictly to Service B's narrow scope.",
                    CalloutTextVi = "Khi Service A cần thay mặt người dùng gọi sang Service B, việc chuyển tiếp nguyên vẹn Access Token gốc sẽ vi phạm nguyên tắc đặc quyền tối thiểu (Least Privilege). Hãy áp dụng mẫu Token Exchange (RFC 8693) để đổi token ban đầu lấy một token mới chỉ có quyền hạn tối thiểu cần thiết cho Service B."
                }
            }
        };
        series.Articles.Add(article4);

        // =========================================================================
        // BÀI 5: THE BFF PATTERN (BACKEND-FOR-FRONTEND)
        // =========================================================================
        var article5 = new DevArticle
        {
            Id = "dotnet-auth-article-5-bff-pattern-zero-trust-spa",
            SeriesId = series.Id,
            LessonNumber = "Bài 5",
            Level = "Advanced",
            LevelVi = "Nâng cao",
            LevelOrder = 5,
            Title = "The BFF (Backend-For-Frontend) Pattern: The Ultimate Security Architecture for SPAs & Cloud-Native Microservices",
            TitleVi = "Mẫu Thiết Kế BFF (Backend-For-Frontend): Tuyệt Kỹ Bảo Mật Tối Thượng Cho SPA, Mobile & Cloud-Native Microservices",
            ReadingTime = "20 phút đọc",
            PublishedDate = "26/10/2026",
            Summary = "End the token storage war once and for all. Master the Backend-For-Frontend (BFF) pattern: Transitioning browser SPAs from vulnerable Public Clients into secure Confidential Clients. The BFF server acts as a security mediator, keeping tokens safely hidden in backend memory while communicating with browsers via encrypted SameSite HttpOnly cookies.",
            SummaryVi = "Giải pháp dứt điểm tranh cãi lưu token ở đâu trên trình duyệt. Mẫu thiết kế Backend-For-Frontend (BFF): Sử dụng YARP hoặc Duende BFF làm cầu nối, biến SPA từ Public Client rủi ro thành Confidential Client an toàn, lưu trữ Token bí mật ở server và chỉ trao đổi với trình duyệt bằng SameSite HttpOnly Cookie mã hóa.",
            Tags = new List<string> { "BFF Pattern", "Backend-For-Frontend", "YARP", "Duende.BFF", "Zero-Trust", "SPA Security", "Microservices" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The SPA Security Dilemma: Why Public Clients Cannot Keep Secrets",
                    HeadingVi = "1. Nan Đề Bảo Mật Của SPA: Vì Sao Public Client Không Bao Giờ Giữ Được Bí Mật?",
                    Paragraphs = new List<string>
                    {
                        "For years, standard industry guidance recommended Single Page Applications (React, Vue, Angular, Blazor WASM) act as OAuth 2.0 Public Clients using Authorization Code Flow with PKCE. Tokens were received directly in browser JavaScript and stored in memory or localStorage.",
                        "However, browser environments are inherently hostile and untrusted. Modern SPAs import hundreds of third-party npm dependencies; a single supply-chain injection vulnerability or XSS exploit grants attackers instant programmatic access to in-flight tokens.",
                        "Furthermore, browsers cannot store Client Secrets. If an API requires a confidential client secret, embedding it in JavaScript exposes it to anyone using Chrome DevTools. The industry consensus (advocated by OWASP and OAuth Working Group) has shifted decisively toward the Backend-For-Frontend (BFF) Pattern."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Suốt nhiều năm qua, cộng đồng từng khuyến nghị các ứng dụng Single Page Application (React, Vue, Angular, Blazor WASM) đóng vai trò là OAuth 2.0 Public Client sử dụng Authorization Code với PKCE. Các chuỗi Access Token và Refresh Token được nhận trực tiếp trong JavaScript của trình duyệt và lưu trong bộ nhớ hoặc localStorage.",
                        "Tuy nhiên, môi trường trình duyệt bản chất là một vùng đất không đáng tin cậy (Untrusted Environment). Một dự án SPA hiện đại nạp hàng trăm thư viện npm từ bên ngoài; chỉ cần một lỗ hổng chuỗi cung ứng hoặc một kẽ hở XSS là kẻ tấn công đã có thể chiếm quyền điều khiển và gửi trộm token về máy chủ của chúng.",
                        "Thêm vào đó, trình duyệt không thể giữ Client Secret. Nếu một hệ thống đòi hỏi độ bảo mật Confidential Client, việc nhét Secret vào code JavaScript chẳng khác nào phơi bày nó cho bất kỳ ai mở Chrome DevTools (F12). Hiệp hội Bảo mật OWASP và Nhóm công tác OAuth quốc tế hiện nay đều thống nhất giải pháp tối thượng: Áp dụng Mẫu thiết kế BFF (Backend-For-Frontend)."
                    },
                    CodeTitle = "The Paradigm Shift: Pure SPA Architecture vs Backend-For-Frontend (BFF) Pattern",
                    CodeLanguage = "text",
                    CodeContent = """
=========================================================================
MÔ HÌNH 1: SPA TRUYỀN THỐNG (RỦI RO CAO - BROWSER CẦM TOKEN)
=========================================================================
[Browser (React / Blazor)] ──(Cầm Access Token thô)──► [Microservices]
  ⚠️ Nguy cơ: XSS đọc trộm Token trong RAM/LocalStorage, phơi bày API nội bộ!

=========================================================================
MÔ HÌNH 2: BACKEND-FOR-FRONTEND (BFF) PATTERN (BẢO MẬT CHUẨN ZERO-TRUST)
=========================================================================
[Browser (React / Blazor)]
   │
   │ (100% Giao tiếp qua Encrypted SameSite=Strict HttpOnly Cookie)
   ▼
[BFF SERVER (ASP.NET Core + YARP)] ◄── Giữ kín ClientSecret an toàn ở Backend!
   │                               └── Quản lý Access Token & Refresh Token trong RAM/Redis
   │
   │ (BFF tự động bóc Cookie ──► Gắn Authorization: Bearer <AccessToken>)
   ▼
[API GATEWAY & MICROSERVICES]
""",
                    CalloutType = "warning",
                    CalloutTitle = "Tokens Never Touch Browser JavaScript",
                    CalloutTitleVi = "Nguyên Tắc Vàng: Token Không Bao Giờ Chạm Vào Trình Duyệt",
                    CalloutText = "In the BFF pattern, the browser never receives, sees, or handles an Access Token or Refresh Token. The browser is only aware of an opaque, encrypted HttpOnly session cookie, making token exfiltration via client-side script exploits impossible.",
                    CalloutTextVi = "Trong mẫu thiết kế BFF, mã JavaScript của trình duyệt không bao giờ nhìn thấy hay chạm được vào Access Token hay Refresh Token. Trình duyệt chỉ biết đến một Cookie phiên HttpOnly được mã hóa, triệt tiêu 100% nguy cơ rò rỉ token qua các kẽ hở mã độc phía client."
                },
                new DevArticleSection
                {
                    Heading = "2. Production BFF Implementation with .NET YARP & Automated Token Management",
                    HeadingVi = "2. Hiện Thực Hóa BFF Hoàn Chỉnh Với ASP.NET Core & YARP Reverse Proxy",
                    Paragraphs = new List<string>
                    {
                        "Implementing a BFF in .NET is exceptionally clean using Microsoft's YARP (Yet Another Reverse Proxy) alongside standard OpenID Connect middleware.",
                        "1) The BFF server initiates the OIDC flow as a true Confidential Client, securely storing the ClientSecret in server configuration.",
                        "2) When the user logs in, the BFF saves the tokens in encrypted server session storage and responds to the browser with an encrypted '__Host-BFF-Session' cookie.",
                        "3) When the SPA makes an API call (e.g. 'GET /api/orders'), the browser includes the cookie. YARP's request transform interceptor validates the cookie, extracts the corresponding Access Token from backend cache, checks token expiration (triggering automated silent refresh via Refresh Token if expired), and attaches 'Authorization: Bearer <token>' before proxying to internal microservices."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Hiện thực hóa kiến trúc BFF trong hệ sinh thái .NET trở nên vô cùng mạnh mẽ và thanh lịch khi kết hợp thư viện Microsoft YARP (Yet Another Reverse Proxy) cùng middleware OpenID Connect tiêu chuẩn.",
                        "1) Máy chủ BFF thực hiện luồng OIDC với tư cách là một Confidential Client thực thụ, lưu trữ ClientSecret an toàn trong biến môi trường của server.",
                        "2) Khi người dùng đăng nhập thành công, BFF lưu trữ Access Token và Refresh Token vào bộ nhớ bảo mật phía server, và chỉ trả về trình duyệt một Cookie phiên '__Host-BFF-Session' duy nhất.",
                        "3) Khi ứng dụng SPA gửi request (ví dụ 'GET /api/orders'), trình duyệt tự gửi cookie. Bộ điều phối YARP chặn request, giải mã cookie, lấy Access Token tương ứng từ cache, tự động kiểm tra hạn dùng (tự kích hoạt Refresh Token đổi token mới nếu sắp hết hạn), rồi gắn header 'Authorization: Bearer <token>' chuyển tiếp tới Microservice."
                    },
                    CodeTitle = "Complete Production BFF Server with Automated Silent Token Refresh in .NET",
                    CodeLanguage = "csharp",
                    CodeContent = """
var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình OIDC Confidential Client trên máy chủ BFF
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
{
    opt.Cookie.Name = "__Host-BFF-Session";
    opt.Cookie.HttpOnly = true;
    opt.Cookie.SameSite = SameSiteMode.Strict;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, opt =>
{
    opt.Authority = "https://idp.company.com";
    opt.ClientId = "spa-bff-client";
    opt.ClientSecret = builder.Configuration["OIDC:ClientSecret"]; // Lưu an toàn ở Backend!
    opt.ResponseType = "code";
    opt.SaveTokens = true; // Lưu Access Token & Refresh Token trong AuthenticationProperties
    opt.Scope.Add("api://microservices-scope");
    opt.Scope.Add("offline_access"); // Yêu cầu cấp Refresh Token
});

// 2. Tích hợp YARP Proxy tự động gắn Bearer Token cho Microservices
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            // Trích xuất Access Token an toàn từ Session Cookie phía Backend
            var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                transformContext.ProxyRequest.Headers.Authorization = 
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        });
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Các endpoint quản trị phiên của BFF cho SPA gọi
app.MapGet("/bff/user", (ClaimsPrincipal user) => Results.Ok(new { Name = user.Identity?.Name }))
   .RequireAuthorization();

app.MapGet("/bff/login", () => Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }));
app.MapGet("/bff/logout", () => Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }));

app.MapReverseProxy();
app.Run();
""",
                    CalloutType = "tip",
                    CalloutTitle = "Zero-Trust Perfection for Modern Microservices",
                    CalloutTitleVi = "Đỉnh Cao Kiến Trúc Zero-Trust Cho Ứng Dụng Hiện Đại",
                    CalloutText = "By implementing the BFF pattern with YARP, your frontend developers focus purely on building exceptional UI/UX without wrestling with cryptographic token refresh timers, while your security architects guarantee that zero API keys or bearer tokens ever leak into untrusted browser memory.",
                    CalloutTextVi = "Bằng việc triển khai mẫu thiết kế BFF với YARP, các kỹ sư Frontend có thể tập trung 100% vào việc xây dựng giao diện người dùng mà không cần bận tâm đến việc viết hàm đếm giờ refresh token, trong khi các chuyên gia bảo mật hoàn toàn yên tâm vì không có bất kỳ Access Token nào bị rò rỉ ra môi trường trình duyệt."
                }
            }
        };
        series.Articles.Add(article5);

        return series;
    }

    [Fact]
    public void TestAuthSeriesExistsAndValid()
    {
        var path = Path.GetFullPath("../../../../../wwwroot/sample-data/dev-learning.json");
        Assert.True(File.Exists(path), $"File not found at: {path}");

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var data = JsonSerializer.Deserialize<DevBlogData>(json, options);
        Assert.NotNull(data);

        var series = data.SeriesList.FirstOrDefault(s => s.Id == "dotnet-auth-monolith-to-microservices-series");
        Assert.NotNull(series);
        Assert.Equal("backend", series.Category);
        Assert.Equal(5, series.Articles.Count);

        for (int i = 0; i < series.Articles.Count; i++)
        {
            var art = series.Articles[i];
            Assert.Equal($"Bài {i + 1}", art.LessonNumber);
            Assert.NotEmpty(art.Sections);
            Assert.False(string.IsNullOrWhiteSpace(art.TitleVi));
            Assert.False(string.IsNullOrWhiteSpace(art.SummaryVi));
        }
    }

    // Helper method to regenerate or update json if needed
    internal static void UpdateDevLearningJsonWithAuthSeries()
    {
        var path = Path.GetFullPath("../../../../../wwwroot/sample-data/dev-learning.json");
        Assert.True(File.Exists(path), $"File not found at: {path}");

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var data = JsonSerializer.Deserialize<DevBlogData>(json, options);
        Assert.NotNull(data);

        var newSeries = BuildSeries();
        var index = data.SeriesList.FindIndex(s => s.Id == newSeries.Id);
        if (index >= 0)
        {
            data.SeriesList[index] = newSeries;
        }
        else
        {
            // Insert right after the first series (infrastructure) or at index 1
            data.SeriesList.Insert(1, newSeries);
        }

        var updatedJson = JsonSerializer.Serialize(data, options);
        File.WriteAllText(path, updatedJson);
    }
}
