using System.Text.Encodings.Web;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Pages;
using Xunit;

namespace BlazorWasmPortfolioGhAction.Tests;

public class ApiDocsSeriesBuilder
{
    public static DevSeries BuildSeries()
    {
        var series = new DevSeries
        {
            Id = "dotnet-api-documentation-and-testing-series",
            Category = "backend",
            Title = "Mastering .NET API Documentation & Testing: Swashbuckle, NSwag, Scalar, OpenAPI 3.1 & Microservices Architecture",
            TitleVi = "Làm Chủ Tài Liệu & Thử Nghiệm API trong .NET: Swashbuckle, NSwag, Scalar, OpenAPI 3.1 & Kiến Trúc Microservices",
            Icon = "bi-file-earmark-code",
            Color = "#6366f1",
            Badge = "Web API & Architecture",
            Description = "Comprehensive guide to API documentation, contracts, and interactive testing in ASP.NET Core: The deprecation of Swashbuckle in .NET 9, Microsoft.AspNetCore.OpenApi built-in generator, modern UI with Scalar, automated SDK generation with NSwag/Kiota, enterprise versioning & multi-tenant auth in Monoliths, and distributed API Gateway aggregation with YARP.",
            DescriptionVi = "Cẩm nang toàn diện về tài liệu và thử nghiệm API trong ASP.NET Core: Sự ra đi của Swashbuckle trong .NET 9, bộ sinh tài liệu Microsoft.AspNetCore.OpenApi, giao diện tương tác thế hệ mới Scalar, tự động sinh Client SDK với NSwag/Kiota, quản lý đa phiên bản & bảo mật trong Monolith lớn, và tổng hợp tài liệu qua API Gateway (YARP) trong Microservices.",
            Articles = new List<DevArticle>()
        };

        // BÀI 1
        var article1 = new DevArticle
        {
            Id = "dotnet-api-docs-article-1-evolution-and-toolchain-comparison",
            SeriesId = series.Id,
            LessonNumber = "Bài 1",
            Level = "Beginner",
            LevelVi = "Cơ bản",
            LevelOrder = 1,
            Title = "The .NET 9 OpenAPI Revolution: Swashbuckle vs NSwag vs Scalar & Why Swashbuckle Was Removed",
            TitleVi = "Cuộc Cách Mạng OpenAPI Trong .NET 9: Swashbuckle vs NSwag vs Scalar & Vì Sao Swashbuckle Bị Khai Tử",
            ReadingTime = "15 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Why Microsoft officially removed Swashbuckle from the default ASP.NET Core template in .NET 9. Compare Swashbuckle, NSwag, and Scalar across OpenAPI 3.1 compliance, UI/UX, Native AOT support, and understand the decoupled modern architecture where specification generation is separated from UI rendering.",
            SummaryVi = "Phân tích nguyên nhân sâu xa vì sao Microsoft chính thức loại bỏ Swashbuckle khỏi template mặc định trong .NET 9. So sánh toàn diện Swashbuckle, NSwag và Scalar về khả năng tương thích OpenAPI 3.1, trải nghiệm UI/UX, hỗ trợ Native AOT và mô hình kiến trúc tách rời giữa sinh tài liệu đặc tả và hiển thị giao diện.",
            Tags = new List<string> { "ASP.NET Core", ".NET 9", "OpenAPI", "Swagger", "NSwag", "Scalar" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Fall of Default Swashbuckle: Why .NET 9 Broke With the Past",
                    HeadingVi = "1. Sự Diệt Vong Của Swashbuckle Mặc Định: Vì Sao .NET 9 Phải Thay Đổi",
                    Paragraphs = new List<string>
                    {
                        "For nearly a decade, every ASP.NET Core developer ran 'dotnet new webapi' and was immediately greeted by Swashbuckle.AspNetCore and its familiar green Swagger UI. It became the de facto standard for testing endpoints locally.",
                        "However, behind the scenes, Swashbuckle was virtually a single-maintainer project. Maintenance stalled, pull requests languished for months, OpenAPI 3.1 support was absent, and compatibility with modern .NET innovations—like Minimal APIs and Native AOT (Ahead-Of-Time compilation)—was severely lacking due to heavy, un-trimmable reflection.",
                        "In .NET 9, Microsoft made a decisive architectural break: Swashbuckle was completely stripped from the official ASP.NET Core templates. Instead, Microsoft introduced the official 'Microsoft.AspNetCore.OpenApi' package, shifting the ecosystem toward a clean separation of concerns."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Suốt gần một thập kỷ, hầu hết lập trình viên .NET khi gõ lệnh 'dotnet new webapi' đều thấy gói Swashbuckle.AspNetCore cùng giao diện Swagger UI màu xanh lá quen thuộc xuất hiện sẵn. Nó gần như trở thành tiêu chuẩn mặc định để kiểm thử API nội bộ.",
                        "Tuy nhiên, đằng sau sự phổ biến đó là một thực tế phũ phàng: Swashbuckle là một dự án mã nguồn mở gần như chỉ do một cá nhân duy trì. Dự án bị bỏ rơi trong thời gian dài, hàng trăm Pull Request tồn đọng, thiếu vắng chuẩn OpenAPI 3.1, và đặc biệt là xung đột nặng nề với các công nghệ tương lai của .NET như Minimal APIs và Native AOT do lạm dụng Reflection không thể tối ưu hóa cắt tỉa mã (trimming).",
                        "Đến .NET 9, Microsoft đã dũng cảm đưa ra quyết định bước ngoặt: Loại bỏ hoàn toàn Swashbuckle khỏi mọi template khởi tạo Web API. Thay vào đó, hãng ra mắt gói chính chủ 'Microsoft.AspNetCore.OpenApi' và mở ra kỷ nguyên kiến trúc tách rời giữa việc sinh dữ liệu đặc tả và hiển thị giao diện."
                    },
                    CodeTitle = "Comparing Startup Code: .NET 8 (Swashbuckle) vs .NET 9 (Microsoft.AspNetCore.OpenApi + Scalar)",
                    CodeLanguage = "csharp",
                    CodeContent = """
// ==================== .NET 8 (LEGACY SWASHBUCKLE) ====================
// Nuget: Swashbuckle.AspNetCore
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Phụ thuộc nặng vào reflection, thiếu OpenAPI 3.1

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // UI cổ điển, không còn update tính năng mới
}

// ==================== .NET 9 (MODERN DECOUPLED STACK) ====================
// Nuget: Microsoft.AspNetCore.OpenApi + Scalar.AspNetCore
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi(); // Engine chính chủ Microsoft, hỗ trợ Native AOT & OpenAPI 3.1

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Sinh file JSON đặc tả siêu tốc tại /openapi/v1.json
    app.MapScalarApiReference(); // Giao diện tương tác hiện đại tại /scalar/v1
}
""",
                    CalloutType = "warning",
                    CalloutTitle = "Breaking Change in .NET 9 Templates",
                    CalloutTitleVi = "Thay Đổi Phá Vỡ Trong Template .NET 9",
                    CalloutText = "When upgrading to .NET 9 or creating a new project with 'dotnet new webapi', AddSwaggerGen() and UseSwaggerUI() will not be found out of the box. Do not reflexively re-install Swashbuckle without evaluating the modern alternatives.",
                    CalloutTextVi = "Khi nâng cấp lên .NET 9 hoặc tạo mới dự án bằng 'dotnet new webapi', bạn sẽ không còn thấy AddSwaggerGen() hay UseSwaggerUI(). Đừng vội vàng cài lại Swashbuckle theo thói quen cũ mà hãy chuyển dịch sang hệ sinh thái OpenAPI hiện đại."
                },
                new DevArticleSection
                {
                    Heading = "2. The Big Three Face-Off: Swashbuckle vs NSwag vs Scalar",
                    HeadingVi = "2. Cuộc So Găng Tam Mã: Swashbuckle vs NSwag vs Scalar",
                    Paragraphs = new List<string>
                    {
                        "To select the ideal tooling for your .NET architecture, you must distinguish between two core responsibilities: Specification Generation (creating the openapi.json document) and User Interface / Client Testing (rendering the browser docs and making test requests).",
                        "Swashbuckle.AspNetCore is a legacy monolith that handles both spec generation and UI rendering (Swagger UI). Its primary advantage is familiarity and legacy StackOverflow answers, but its future is uncertain.",
                        "NSwag by Rico Suter is an enterprise workhorse. It generates OpenAPI specs, renders Swagger UI/ReDoc, and most importantly features a world-class code generator that produces typed C# and TypeScript client SDKs during build time.",
                        "Scalar.AspNetCore is the modern visual champion. Built specifically for OpenAPI 3.1, it provides a gorgeous three-column documentation view, an interactive REST client with environment variable support, and automatic snippet generation for over 15 programming languages."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Để đưa ra quyết định chuẩn xác cho kiến trúc phần mềm, bạn cần phân biệt rạch ròi 2 nhiệm vụ: Sinh tài liệu đặc tả (Tạo ra file openapi.json) và Giao diện thử nghiệm (Render giao diện web và gửi request test).",
                        "Swashbuckle.AspNetCore đóng gói cả hai nhiệm vụ trên theo lối cũ. Điểm mạnh duy nhất hiện nay là sự quen thuộc và kho câu hỏi khổng lồ trên StackOverflow, nhưng tương lai phát triển lâu dài không còn được đảm bảo.",
                        "NSwag (do Rico Suter phát triển) là cỗ máy chiến đấu mạnh mẽ cho doanh nghiệp. NSwag không chỉ sinh spec mà còn sở hữu bộ sinh mã (Code Generator) đỉnh cao: tự động tạo ra C# Client và TypeScript SDK chuẩn chỉ ngay trong quá trình build.",
                        "Scalar.AspNetCore là ngôi sao sáng về mặt trải nghiệm người dùng (UX). Được xây dựng tối ưu cho OpenAPI 3.1, Scalar mang đến giao diện 3 cột hiện đại chuẩn quốc tế, tích hợp sẵn trình test API như Postman có biến môi trường, và tự sinh code gọi API cho hơn 15 ngôn ngữ lập trình."
                    },
                    CodeTitle = "Ecosystem Comparison Matrix",
                    CodeLanguage = "text",
                    CodeContent = """
+-----------------------+---------------------+---------------------+---------------------+
| Tiêu chí so sánh      | Swashbuckle         | NSwag               | Scalar              |
+-----------------------+---------------------+---------------------+---------------------+
| Trạng thái .NET 9     | Bị loại bỏ template | Hỗ trợ tốt          | Khuyên dùng         |
| Chuẩn OpenAPI         | 2.0 / 3.0 (3.1 yếu) | 3.0 / 3.1           | 3.1 Native          |
| UI Rendering          | Swagger UI cổ điển  | Swagger UI / ReDoc  | Giao diện 3 cột Pro |
| Trình Test API        | Nút 'Try it out'    | Nút 'Try it out'    | Interactive Client  |
| Sinh Client SDK       | Không               | Rất mạnh (C#/TS)    | Code snippets 15+ ng|
| Tương thích NativeAOT | Rất kém             | Trung bình          | Rất tốt             |
| Cơ chế mở rộng        | Filters             | Processors          | Transformers/Themes |
+-----------------------+---------------------+---------------------+---------------------+
""",
                    CalloutType = "info",
                    CalloutTitle = "Specification Generator vs UI Viewer",
                    CalloutTitleVi = "Tách Rời: Bộ Sinh Đặc Tả vs Bộ Hiển Thị Giao Diện",
                    CalloutText = "The biggest mental shift in .NET 9 is decoupling. Microsoft.AspNetCore.OpenApi generates the raw OpenAPI JSON specification. Scalar or Swagger UI simply consumes that JSON. You are no longer locked into a single all-in-one library.",
                    CalloutTextVi = "Bước ngoặt tư duy lớn nhất của .NET 9 là sự tách rời: Microsoft.AspNetCore.OpenApi chịu trách nhiệm sinh file JSON đặc tả chuẩn xác ở backend, còn Scalar hoặc Swagger UI chỉ đóng vai trò frontend tiêu thụ file JSON đó."
                },
                new DevArticleSection
                {
                    Heading = "3. The Decoupled Architecture: Best Tool for Each Role",
                    HeadingVi = "3. Kiến Trúc Tách Rời Hiện Đại: Chọn Đúng Công Cụ Cho Đúng Vai Trò",
                    Paragraphs = new List<string>
                    {
                        "Modern ASP.NET Core allows hosting multiple documentation engines concurrently. For instance, you can use Microsoft.AspNetCore.OpenApi to generate your enterprise specification at '/openapi/v1.json', map Scalar at '/scalar/v1' for frontend developers, and simultaneously keep Swagger UI at '/swagger' for team members who prefer the traditional interface.",
                        "Furthermore, you can pair this decoupled architecture with code generation tools like NSwag CLI or Microsoft Kiota to compile your OpenAPI contract directly into reusable client libraries without touching the web application runtime.",
                        "This modular approach ensures that your API specification remains clean, standards-compliant, and fully compatible with future .NET releases."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Kiến trúc hiện đại của ASP.NET Core cho phép bạn kích hoạt nhiều công cụ tài liệu song song mà không xung đột. Bạn hoàn toàn có thể dùng Microsoft.AspNetCore.OpenApi để sinh spec chuẩn tại '/openapi/v1.json', kích hoạt Scalar tại '/scalar/v1' cho team Frontend và Product sử dụng, đồng thời giữ lại Swagger UI tại '/swagger' cho các kỹ sư quen mắt.",
                        "Bên cạnh đó, việc tách rời file đặc tả cho phép các công cụ sinh mã như NSwag CLI hoặc Microsoft Kiota đọc file JSON này trong pipeline CI/CD để đóng gói thư viện SDK cho các ứng dụng client mà không gây ảnh hưởng gì tới runtime của web service.",
                        "Cách tiếp cận này mang lại sự linh hoạt tối đa, chuẩn hóa theo chuẩn quốc tế và tương thích tuyệt đối với các bản cập nhật tương lai của .NET."
                    },
                    CodeTitle = "Hosting Scalar and Swagger UI Simultaneously in Program.cs (.NET 9)",
                    CodeLanguage = "csharp",
                    CodeContent = """
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Sinh đặc tả chuẩn chính thức bằng Microsoft.AspNetCore.OpenApi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Expose file spec tại: /openapi/v1.json
    app.MapOpenApi();

    // Giao diện hiện đại Scalar tại: /scalar/v1
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Core Platform API - Scalar View")
               .WithTheme(ScalarTheme.Moon);
    });

    // Vẫn có thể giữ Swagger UI cổ điển song song tại: /swagger (nếu cài package Swashbuckle.AspNetCore.SwaggerUI)
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Core Platform API v1");
        options.RoutePrefix = "swagger";
    });
}

app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .WithTags("System")
   .WithSummary("Kiểm tra sức khỏe hệ thống");

app.Run();
""",
                    CalloutType = "tip",
                    CalloutTitle = "Zero-Risk Migration Strategy",
                    CalloutTitleVi = "Chiến Lược Nâng Cấp Không Rủi Ro",
                    CalloutText = "Running both Scalar and Swagger UI side-by-side during sprint transitions gives your engineering organization time to adapt to Scalar's modern client without breaking existing workflows or team bookmarks.",
                    CalloutTextVi = "Chạy song song cả Scalar và Swagger UI trong giai đoạn chuyển đổi giúp toàn bộ kỹ sư có thời gian trải nghiệm tính năng của Scalar mà không làm gián đoạn thói quen kiểm thử hằng ngày của team."
                }
            }
        };
        series.Articles.Add(article1);

        // BÀI 2
        var article2 = new DevArticle
        {
            Id = "dotnet-api-docs-article-2-scalar-and-aspnetcore-openapi",
            SeriesId = series.Id,
            LessonNumber = "Bài 2",
            Level = "Intermediate",
            LevelVi = "Trung cấp",
            LevelOrder = 2,
            Title = "Hands-On with Scalar & Microsoft.AspNetCore.OpenApi in .NET 9: Modern UI & Built-In REST Client",
            TitleVi = "Thực Chiến Scalar & Microsoft.AspNetCore.OpenApi trong .NET 9: Giao Diện Hiện Đại & Trình Test Tích Hợp",
            ReadingTime = "16 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Complete setup guide for Microsoft.AspNetCore.OpenApi and Scalar.AspNetCore in .NET 9: Configuring OpenAPI 3.1, replacing legacy Swashbuckle filters with modern OpenAPI Transformers, customizing themes (Moon, Solarized, Purple), and leveraging the built-in Postman-like interactive API test runner.",
            SummaryVi = "Hướng dẫn cấu hình hoàn chỉnh Microsoft.AspNetCore.OpenApi và Scalar.AspNetCore trong .NET 9: Thiết lập OpenAPI 3.1, thay thế toàn bộ Filters cũ bằng cơ chế OpenAPI Transformers hiện đại, tùy biến giao diện phong phú (Moon, Solarized, Purple), và khai thác trình test API tương tác mạnh mẽ như Postman ngay trong trình duyệt.",
            Tags = new List<string> { "Scalar", "OpenAPI 3.1", "Transformers", ".NET 9", "Minimal APIs", "REST Client" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Setting Up Microsoft.AspNetCore.OpenApi & Scalar in Under 3 Minutes",
                    HeadingVi = "1. Cài Đặt Microsoft.AspNetCore.OpenApi & Scalar Trong Chưa Đầy 3 Phút",
                    Paragraphs = new List<string>
                    {
                        "Getting started with Scalar in .NET 9 requires just two NuGet packages: 'Microsoft.AspNetCore.OpenApi' (for spec generation) and 'Scalar.AspNetCore' (for the interactive frontend UI).",
                        "Unlike legacy Swashbuckle which required configuring complex XML doc paths and assembly reflections, Scalar works out of the box with Minimal APIs and Controller-based APIs through native ASP.NET Core Endpoint Metadata.",
                        "Once registered, calling MapScalarApiReference() will expose a lightning-fast web client loaded with full OpenAPI 3.1 capabilities."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khởi động dự án với Scalar trong .NET 9 chỉ yêu cầu 2 gói NuGet: 'Microsoft.AspNetCore.OpenApi' (phụ trách sinh file đặc tả backend) và 'Scalar.AspNetCore' (phụ trách render giao diện tương tác phía frontend).",
                        "Khác với Swashbuckle ngày xưa vốn đòi hỏi cấu hình đường dẫn file XML và reflection rườm rà, Scalar khai thác trực tiếp hệ thống Endpoint Metadata của ASP.NET Core, hoạt động mượt mà cho cả Minimal APIs lẫn Controller truyền thống.",
                        "Sau khi đăng ký, phương thức MapScalarApiReference() sẽ tự động cung cấp giao diện web tốc độ cao với đầy đủ sức mạnh của chuẩn OpenAPI 3.1 mới nhất."
                    },
                    CodeTitle = "Program.cs Setup for Microsoft.AspNetCore.OpenApi and Scalar",
                    CodeLanguage = "csharp",
                    CodeContent = """
// 1. Cài đặt NuGet:
// dotnet add package Microsoft.AspNetCore.OpenApi
// dotnet add package Scalar.AspNetCore

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ OpenAPI chính thức
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Endpoint sinh file JSON: /openapi/v1.json
    app.MapOpenApi();

    // Render giao diện tài liệu tại: /scalar/v1
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("E-Commerce Microservice API")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.MapGet("/api/orders/{id:guid}", (Guid id) => Results.Ok(new
{
    OrderId = id,
    Status = "Confirmed",
    TotalAmount = 149.99,
    CreatedAt = DateTime.UtcNow
}))
.WithName("GetOrderById")
.WithTags("Orders")
.WithSummary("Tra cứu chi tiết đơn hàng theo mã GUID")
.WithDescription("Trả về thông tin chi tiết của đơn đặt hàng bao gồm trạng thái xử lý và tổng tiền thanh toán.");

app.Run();
""",
                    CalloutType = "tip",
                    CalloutTitle = "Default Routes to Remember",
                    CalloutTitleVi = "Đường Dẫn Mặc Định Cần Ghi Nhớ",
                    CalloutText = "By default, MapOpenApi() maps to '/openapi/{documentName}.json' (e.g. /openapi/v1.json), and MapScalarApiReference() maps to '/scalar/{documentName}' (e.g. /scalar/v1). Both paths can be customized via options.",
                    CalloutTextVi = "Mặc định, MapOpenApi() phục vụ tại '/openapi/{documentName}.json' (ví dụ /openapi/v1.json), còn MapScalarApiReference() phục vụ tại '/scalar/{documentName}' (ví dụ /scalar/v1). Bạn có thể đổi đường dẫn tùy ý thông qua options."
                },
                new DevArticleSection
                {
                    Heading = "2. Mastering OpenAPI Transformers: Say Goodbye to Swashbuckle Filters",
                    HeadingVi = "2. Làm Chủ OpenAPI Transformers: Tạm Biệt Hệ Thống Filter Cổ Điển",
                    Paragraphs = new List<string>
                    {
                        "In legacy Swashbuckle, modifying generated documentation required implementing custom interfaces: IOperationFilter, ISchemaFilter, or IDocumentFilter. These filters often suffered from convoluted type hierarchies and fragile reflection.",
                        "ASP.NET Core .NET 9 introduces a cleaner, functional pattern: OpenAPI Transformers. You can register Document Transformers (for global metadata and security schemes), Operation Transformers (for per-endpoint headers and parameters), and Schema Transformers (for model validation attributes).",
                        "Transformers have direct access to ASP.NET Core Dependency Injection (IServiceProvider), allowing you to dynamically resolve configuration settings, environment flags, or database contexts when decorating your OpenAPI specification."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong Swashbuckle cũ, để can thiệp vào tài liệu API (như thêm Bearer Auth hay header tùy biến), bạn phải implement các interface phức tạp: IOperationFilter, ISchemaFilter hoặc IDocumentFilter với cú pháp reflection rất dễ gãy khi nâng cấp phiên bản .NET.",
                        ".NET 9 mang đến cơ chế Transformers chuẩn chỉ và trực quan: Document Transformers (can thiệp toàn bộ tài liệu và bảo mật), Operation Transformers (can thiệp từng endpoint cụ thể), và Schema Transformers (can thiệp cấu trúc DTOs).",
                        "Các Transformers này có quyền truy cập trực tiếp vào Dependency Injection container (IServiceProvider), cho phép bạn dễ dàng đọc IConfiguration, cờ môi trường, hoặc inject các service quản trị để trang trí tài liệu một cách động."
                    },
                    CodeTitle = "Configuring JWT Bearer Security & Metadata with DocumentTransformer",
                    CodeLanguage = "csharp",
                    CodeContent = """
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

builder.Services.AddOpenApi(options =>
{
    // 1. Document Transformer: Cấu hình thông tin tác giả và bảo mật JWT
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Payment Gateway Enterprise API",
            Version = "v1.2",
            Description = "Hệ thống cổng thanh toán quốc tế hỗ trợ thẻ tín dụng và ví điện tử.",
            Contact = new OpenApiContact
            {
                Name = "DevOps Platform Team",
                Email = "platform-team@company.com"
            }
        };

        // Khai báo lược đồ xác thực JWT Bearer
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Nhập token JWT theo định dạng: Bearer {token_cua_ban}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = securityScheme;

        // Áp dụng mặc định cho toàn bộ endpoint
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });

    // 2. Operation Transformer: Thêm Correlation ID Header cho mọi endpoint
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Trace ID phục vụ phân tích log phân tán qua OpenTelemetry",
            Schema = new OpenApiSchema { Type = "string" }
        });
        return Task.CompletedTask;
    });
});
""",
                    CalloutType = "info",
                    CalloutTitle = "Full Dependency Injection Support",
                    CalloutTitleVi = "Hỗ Trợ Toàn Diện Dependency Injection",
                    CalloutText = "You can write dedicated transformer classes implementing IOpenApiDocumentTransformer and register them via builder.Services.AddOpenApi(opt => opt.AddDocumentTransformer<MyCustomAuthTransformer>()), maintaining clean architecture and testability.",
                    CalloutTextVi = "Bạn có thể tách riêng thành các class độc lập implement interface IOpenApiDocumentTransformer và đăng ký qua DI container, đảm bảo tính sạch sẽ của kiến trúc mã nguồn và dễ dàng viết unit test kiểm thử."
                },
                new DevArticleSection
                {
                    Heading = "3. Interactive Testing Experience: Environments, Tokens & 15+ Code Snippets",
                    HeadingVi = "3. Trải Nghiệm Thử Nghiệm API Thế Hệ Mới: Biến Môi Trường, Token & Sinh Code Mẫu",
                    Paragraphs = new List<string>
                    {
                        "The primary reason development teams fall in love with Scalar is its interactive API client. While Swagger UI's 'Try it out' feels clunky and lacks authentication persistence across page refreshes, Scalar embeds a full-featured HTTP client directly into the documentation interface.",
                        "Developers can save JWT authorization tokens with a single click, define environment variables (such as local vs staging base URLs), inspect formatted JSON responses, and copy ready-to-run code snippets in over 15 client languages including C# (HttpClient, RestSharp), JavaScript (Fetch, Axios), Python (Requests), Go, and cURL.",
                        "For enterprise intranet environments without internet connectivity, Scalar also supports self-hosting all frontend JavaScript and CSS bundles locally, avoiding any CDN dependencies."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Lý do số một khiến các nhóm kỹ thuật chuyển sang Scalar chính là trình thử nghiệm API tương tác. Trong khi nút 'Try it out' của Swagger UI khá thô sơ và thường xuyên làm mất Token khi F5 tải lại trang, Scalar tích hợp nguyên một HTTP Client đa năng trực tiếp vào tài liệu.",
                        "Lập trình viên có thể lưu Token đăng nhập chỉ với một thao tác, cấu hình biến môi trường (Local, Dev, Staging), xem định dạng JSON trả về cực kỳ đẹp mắt, và sao chép mã nguồn mẫu chuẩn cho hơn 15 ngôn ngữ như C# (HttpClient, RestSharp), JavaScript (Fetch, Axios), Python (Requests), Go, và cURL.",
                        "Đối với môi trường mạng nội bộ doanh nghiệp (On-Premises / Air-gapped) không có kết nối Internet ra bên ngoài, Scalar hỗ trợ đóng gói toàn bộ file JS/CSS tĩnh chạy local mà không cần tải từ CDN công cộng."
                    },
                    CodeTitle = "Advanced Scalar Configuration: Themes, Client Customization & CDN Bundling",
                    CodeLanguage = "csharp",
                    CodeContent = """
app.MapScalarApiReference(options =>
{
    options.WithTitle("FinTech Core API Documentation")
           .WithTheme(ScalarTheme.Solarized) // Các themes: Moon, Solarized, Purple, BluePlanet, Satellites...
           .WithDarkMode(true) // Bật mặc định chế độ Dark Mode bảo vệ mắt
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient) // Code mẫu mặc định hiển thị C# HttpClient
           .WithPreferredScheme("Bearer") // Tự động điền tiền tố Bearer khi người dùng dán Token
           .WithSidebar(true) // Hiển thị thanh menu phân nhóm bên trái
           .WithSearchHotKey("k"); // Phím tắt tìm kiếm nhanh Ctrl+K / Cmd+K như IDE
});
""",
                    CalloutType = "tip",
                    CalloutTitle = "Boost Frontend Team Productivity",
                    CalloutTitleVi = "Tăng Tốc Năng Suất Cho Team Frontend & Mobile",
                    CalloutText = "With Scalar's multi-language code snippets, your frontend and mobile engineers no longer need to guess how to construct request headers or serializations; they can simply copy the generated Axios or Swift snippet directly into their codebases.",
                    CalloutTextVi = "Nhờ tính năng tự động sinh mã đa ngôn ngữ của Scalar, các lập trình viên Frontend và Mobile không còn phải mày mò đoán định dạng header hay cấu trúc body; họ chỉ cần bấm copy đoạn mã Axios hoặc Swift sinh sẵn để đưa thẳng vào dự án."
                }
            }
        };
        series.Articles.Add(article2);

        // BÀI 3
        var article3 = new DevArticle
        {
            Id = "dotnet-api-docs-article-3-nswag-client-sdk-generation",
            SeriesId = series.Id,
            LessonNumber = "Bài 3",
            Level = "Intermediate",
            LevelVi = "Trung cấp",
            LevelOrder = 2,
            Title = "NSwag & Contract-First Workflows: Automated Client SDK Generation (C# & TypeScript) in CI/CD",
            TitleVi = "NSwag & Quy Trình Contract-First: Tự Động Sinh Mã Client SDK (C# & TypeScript) trong CI/CD",
            ReadingTime = "17 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Stop writing boilerplate HttpClient and Axios code by hand. Learn how NSwag automates strongly-typed C# and TypeScript client generation during MSBuild compilation and GitHub Actions CI/CD pipelines, preventing contract drift between frontend and backend teams.",
            SummaryVi = "Xóa bỏ hoàn toàn việc viết tay HttpClient và Axios thủ công. Khám phá cách NSwag tự động hóa việc sinh mã C# và TypeScript client lúc biên dịch qua MSBuild và trong GitHub Actions CI/CD pipeline, ngăn chặn triệt để tình trạng lệch hợp đồng giữa frontend và backend.",
            Tags = new List<string> { "NSwag", "Code Generation", "TypeScript", "C# Client", "CI/CD", "Contract-First", "Kiota" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Contract Drift Nightmare: Stop Writing HttpClient by Hand",
                    HeadingVi = "1. Ác Mộng Lệch Hợp Đồng API: Đừng Bao Giờ Viết Tay HttpClient Nữa",
                    Paragraphs = new List<string>
                    {
                        "In many engineering teams, the workflow between backend and frontend goes like this: the backend developer modifies a DTO property (e.g., renaming 'CustomerEmail' to 'EmailAddress'), updates the controller, and forgets to tell the frontend team.",
                        "The frontend team's code still compiles fine because they hand-typed their TypeScript interfaces. But at runtime in production, the application silently receives undefined values, triggering catastrophic UI bugs.",
                        "This phenomenon is known as Contract Drift. The only reliable solution is automated Code Generation: treating the OpenAPI document as the single source of truth, and generating strongly typed client SDKs automatically during build time or in the CI/CD pipeline."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong rất nhiều nhóm lập trình, quy trình phối hợp thường diễn ra như sau: Lập trình viên Backend đổi tên một thuộc tính trong DTO (ví dụ từ 'CustomerEmail' thành 'EmailAddress'), cập nhật controller và quên không thông báo kịp cho team Frontend.",
                        "Ứng dụng Frontend vẫn build thành công vì họ tự viết tay các interface TypeScript. Nhưng khi chạy thực tế trên Production, giao diện người dùng đột ngột nhận giá trị undefined, gây ra lỗi nghiêm trọng mà không hề có cảnh báo trước.",
                        "Hiện tượng này được gọi là Lệch hợp đồng API (Contract Drift). Giải pháp triệt để duy nhất là Tự động hóa sinh mã (Code Generation): Coi tài liệu OpenAPI là chân lý duy nhất (Single Source of Truth), và tự động tạo ra thư viện Client SDK có kiểu dữ liệu chặt chẽ (Strongly-typed) ngay lúc build hoặc qua CI/CD."
                    },
                    CodeTitle = "Handwritten Fragile HttpClient vs Strongly-Typed Generated Client",
                    CodeLanguage = "csharp",
                    CodeContent = """
// ==================== CÁCH VIẾT TAY NGUY HIỂM ====================
// Viết tay chuỗi URL, magic strings, parse JSON thủ công: Dễ gãy, không phát hiện lỗi lúc biên dịch
public async Task<UserDto?> GetUserManualAsync(int id)
{
    var response = await _httpClient.GetAsync($"/api/v1/users/{id}");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<UserDto>(); // Nếu backend đổi tên trường -> Runtime Bug!
}

// ==================== CÁCH SINH MÃ TỰ ĐỘNG BẰNG NSWAG ====================
// Hoàn toàn type-safe, có IntelliSense, bắt lỗi ngay tại thời điểm biên dịch (Compile-time error)
public async Task<UserProfileResponse> GetUserAutomatedAsync(int id)
{
    // Client này do NSwag tự động sinh từ OpenAPI Spec
    return await _generatedApiClient.GetUserByIdAsync(id, CancellationToken.None);
}
""",
                    CalloutType = "warning",
                    CalloutTitle = "Compile-Time Safety Over Runtime Bugs",
                    CalloutTitleVi = "Bắt Lỗi Tại Thời Điểm Biên Dịch Thay Vì Sập Trên Production",
                    CalloutText = "When backend developers modify an endpoint or payload, the next build of the frontend or consumer service immediately fails compilation if breaking changes exist. This gives teams instant feedback loops before code ever reaches QA.",
                    CalloutTextVi = "Khi backend thay đổi endpoint hoặc cấu trúc dữ liệu, lần build tiếp theo của frontend sẽ báo lỗi biên dịch ngay lập tức nếu có thay đổi phá vỡ. Đội ngũ phát hiện lỗi chỉ trong vài giây thay vì chờ đến khi tester phát hiện."
                },
                new DevArticleSection
                {
                    Heading = "2. Configuring NSwag MSBuild & CLI for TypeScript & C# SDK Generation",
                    HeadingVi = "2. Cấu Hình NSwag MSBuild & CLI Tự Động Sinh Client TypeScript và C#",
                    Paragraphs = new List<string>
                    {
                        "NSwag can be executed in multiple ways: via NSwagStudio GUI, CLI commands, or directly integrated into MSBuild targets within your .csproj file so client code regenerates on every build.",
                        "By creating an 'nswag.json' configuration file in your solution, you can specify exact code generation parameters: target framework, nullable reference types, TypeScript module format (Axios, Fetch, Angular RxJS), DTO class naming rules, and output file destinations.",
                        "Because NSwag can inspect compiled assemblies (.NET to OpenAPI) without running the web server, generation runs fast and reliably inside automated build environments."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "NSwag có thể chạy theo nhiều hình thức: giao diện đồ họa NSwagStudio, câu lệnh dòng lệnh NSwag CLI, hoặc nhúng trực tiếp vào Target của MSBuild trong file .csproj để tự động sinh code mỗi khi bấm Build dự án.",
                        "Bằng cách thiết lập file cấu hình 'nswag.json', bạn có thể định nghĩa chi tiết mọi tham số sinh mã: phiên bản ngôn ngữ, hỗ trợ nullable reference types, thư viện HTTP cho TypeScript (Axios, Fetch, Angular RxJS), quy tắc đặt tên DTO, và thư mục đích xuất file.",
                        "Đặc biệt, NSwag có thể phân tích trực tiếp file assembly đã biên dịch (.dll) mà không cần bật server web chạy thật, giúp quá trình sinh mã diễn ra chớp nhoáng và cực kỳ ổn định trong môi trường tự động hóa."
                    },
                    CodeTitle = "Automated MSBuild Target in .csproj for NSwag Client Generation",
                    CodeLanguage = "xml",
                    CodeContent = """
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Cài đặt công cụ MSBuild của NSwag -->
    <PackageReference Include="NSwag.MSBuild" Version="14.2.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <!-- Tự động chạy sau khi biên dịch xong -->
  <Target Name="GenerateApiClient" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Debug'">
    <Exec ConsoleToMSBuild="true" 
          Command="$(NSwagExe_Net90) run nswag.json /variables:Configuration=$(Configuration)" />
  </Target>
</Project>
""",
                    CalloutType = "info",
                    CalloutTitle = "Sample nswag.json Configuration File",
                    CalloutTitleVi = "Mẫu Cấu Hình nswag.json Chuẩn",
                    CalloutText = "In nswag.json, configure 'openApiToTypeScriptClient' with 'template': 'Axios', 'generateClientClasses': true, and 'typeStyle': 'Interface' to generate clean, idiomatic TypeScript interfaces for your React or Blazor WASM applications.",
                    CalloutTextVi = "Trong nswag.json, cấu hình 'openApiToTypeScriptClient' với 'template': 'Axios' và 'typeStyle': 'Interface' để sinh ra các interface TypeScript chuẩn chỉ nhất cho các ứng dụng React, Vue hoặc Blazor WebAssembly."
                },
                new DevArticleSection
                {
                    Heading = "3. CI/CD Pipeline Automation: Publishing Client Packages (NuGet & npm) & Kiota Comparison",
                    HeadingVi = "3. Tự Động Hóa CI/CD: Đẩy Gói NuGet/npm & So Sánh NSwag Với Microsoft Kiota",
                    Paragraphs = new List<string>
                    {
                        "In enterprise environments, consumer teams should not even need to run code generation locally. Instead, a GitHub Actions workflow triggers whenever backend code merges into the main branch.",
                        "The pipeline generates the C# client, packs it into a private NuGet package (e.g. Company.OrderService.Client), and publishes it to Azure Artifacts or GitHub Packages. Simultaneously, it generates the TypeScript client and publishes it to a private npm registry.",
                        "It is also important to consider Microsoft Kiota: Microsoft's official, next-generation OpenAPI code generator. While NSwag remains superior for deep .NET and TypeScript projects, Kiota is ideal if your enterprise requires SDKs across Java, Python, Go, and PHP simultaneously."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Ở quy mô doanh nghiệp, các team sử dụng API thậm chí không cần phải chạy lệnh sinh mã ở máy cá nhân. Thay vào đó, quy trình được đưa lên GitHub Actions CI/CD tự động kích hoạt mỗi khi backend merge vào nhánh main.",
                        "Pipeline sẽ tự động sinh code C# client, đóng gói thành thư viện NuGet nội bộ (ví dụ: Company.OrderService.Client) và publish lên GitHub Packages hoặc Azure Artifacts. Đồng thời, nó sinh mã TypeScript và publish lên private npm registry.",
                        "Ngoài NSwag, bạn cũng nên biết đến Microsoft Kiota: Công cụ sinh mã OpenAPI chính thức của Microsoft. Trong khi NSwag vượt trội về hệ sinh thái .NET và TypeScript, Kiota là lựa chọn tuyệt vời nếu doanh nghiệp của bạn yêu cầu sinh SDK đồng thời cho nhiều ngôn ngữ như Java, Python, Go và PHP."
                    },
                    CodeTitle = "GitHub Actions Workflow Generating & Publishing SDK Packages",
                    CodeLanguage = "yaml",
                    CodeContent = """
name: Generate & Publish API Clients

on:
  push:
    branches: [ main ]
    paths:
      - 'src/Api/**'

jobs:
  build-and-publish:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 9.0.x

    - name: Build API & Generate OpenAPI Spec
      run: |
        dotnet build src/Api/Api.csproj -c Release
        # Trích xuất file openapi.json bằng công cụ Microsoft.Extensions.ApiDescription.Server
        dotnet openapi extract --project src/Api/Api.csproj --output ./contracts/openapi.json

    - name: Generate C# Client SDK & Pack NuGet
      run: |
        dotnet tool install -g NSwag.ConsoleCore
        nswag openapi2csclient /input:./contracts/openapi.json /output:./clients/OrderServiceClient.cs /namespace:Company.OrderService.Client
        dotnet pack src/Clients/OrderServiceClient.csproj -c Release -o ./artifacts

    - name: Push NuGet Package to Private Feed
      run: |
        dotnet nuget push ./artifacts/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://nuget.pkg.github.com/my-org/index.json
""",
                    CalloutType = "tip",
                    CalloutTitle = "Strict SemVer for Generated SDKs",
                    CalloutTitleVi = "Tuân Thủ Nghiêm Ngặt Semantic Versioning Cho SDK",
                    CalloutText = "Always tie the version of the generated client package to the API contract version. If an endpoint change introduces breaking changes, bump the major version (e.g. 2.0.0) so consuming services are never inadvertently broken.",
                    CalloutTextVi = "Luôn đồng bộ số phiên bản của SDK với phiên bản hợp đồng API. Nếu có thay đổi phá vỡ (breaking changes), hãy nâng Major version (ví dụ từ 1.x lên 2.0.0) để các dịch vụ đang sử dụng không bị ảnh hưởng ngoài ý muốn."
                }
            }
        };
        series.Articles.Add(article3);

        // BÀI 4
        var article4 = new DevArticle
        {
            Id = "dotnet-api-docs-article-4-enterprise-monolith-versioning-auth",
            SeriesId = series.Id,
            LessonNumber = "Bài 4",
            Level = "Advanced",
            LevelVi = "Nâng cao",
            LevelOrder = 3,
            Title = "Enterprise Monolith Documentation: API Versioning, Multi-Tenant Auth & XML Comments Mastery",
            TitleVi = "Tài Liệu Hóa Monolith Lớn: Đa Phiên Bản API Versioning, Bảo Mật Đa Tầng & XML Comments",
            ReadingTime = "18 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Architect robust documentation for large enterprise monoliths: Multi-versioning with Asp.Versioning.Http, segregating public vs internal partner APIs, configuring complex OAuth2/OIDC/JWT Bearer and API Key authentication schemes, and optimizing large schema reflection performance.",
            SummaryVi = "Kiến trúc tài liệu hóa API cho hệ thống Monolith doanh nghiệp quy mô lớn: Quản lý đa phiên bản với Asp.Versioning.Http, phân tách tài liệu Public vs Internal Partner APIs, cấu hình các lược đồ bảo mật phức tạp OAuth2/OIDC/JWT Bearer và tối ưu hóa hiệu năng phản chiếu schema.",
            Tags = new List<string> { "API Versioning", "Enterprise Monolith", "JWT Bearer", "OAuth2", "XML Comments", "Security" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Architectural Bottlenecks in Large Monoliths: Hundreds of Endpoints & Models",
                    HeadingVi = "1. Nút Thắt Kiến Trúc Trong Monolith Lớn: Hàng Trăm Endpoints & Models Phức Tạp",
                    Paragraphs = new List<string>
                    {
                        "In mature enterprise monoliths, applications frequently grow to hundreds or thousands of endpoints, serving multiple frontend platforms, third-party partners, and background workers.",
                        "Attempting to dump all endpoints into a single massive OpenAPI document creates severe performance bottlenecks: browser memory spikes to gigabytes, Swagger UI crashes or stutters, and developers struggle to navigate overwhelming endpoint lists.",
                        "The solution is document modularization: partitioning the API documentation into distinct logical documents based on API Version (v1 vs v2) and Audience Scope (Public APIs for mobile/web vs Internal Admin APIs for operations)."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong các hệ thống Monolith doanh nghiệp lâu năm, ứng dụng thường phình to lên tới hàng trăm hoặc hàng nghìn endpoints, phục vụ cùng lúc cả ứng dụng di động, đối tác bên thứ ba và các hệ thống nghiệp vụ nội bộ.",
                        "Việc cố nhồi nhét tất cả endpoint vào một file OpenAPI duy nhất sẽ dẫn đến thảm họa về hiệu năng: Trình duyệt ngốn hàng gigabyte RAM, Swagger UI giật lag hoặc bị crash, và lập trình viên như lạc vào mê cung khi tìm kiếm endpoint.",
                        "Giải pháp chuẩn mực là Phân tách tài liệu thành các phân vùng (Document Modularization): Chia tách theo Phiên bản API (v1, v2) và theo Đối tượng phục vụ (Public APIs cho khách hàng vs Internal APIs cho ban quản trị)."
                    },
                    CodeTitle = "Modularizing OpenAPI Documents by Version and Audience in Program.cs",
                    CodeLanguage = "csharp",
                    CodeContent = """
var builder = WebApplication.CreateBuilder(args);

// Đăng ký 2 tài liệu OpenAPI riêng biệt: Public API và Internal BackOffice API
builder.Services.AddOpenApi("public-v1", options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Info.Title = "Public Customer API (v1)";
        doc.Info.Description = "Dành cho ứng dụng Mobile App và Website khách hàng.";
        return Task.CompletedTask;
    });
});

builder.Services.AddOpenApi("internal-admin", options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Info.Title = "Internal BackOffice Admin API";
        doc.Info.Description = "API phân quyền nội bộ dành cho bộ phận vận hành và kế toán.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Expose 2 file JSON riêng biệt:
    // /openapi/public-v1.json
    // /openapi/internal-admin.json
    app.MapOpenApi("/openapi/{documentName}.json");

    // Scalar cho phép xem cả 2 tài liệu thông qua menu chuyển đổi
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Enterprise API Center")
               .WithEndpointPrefix("/docs/{documentName}");
    });
}
""",
                    CalloutType = "warning",
                    CalloutTitle = "The Browser Memory Trap",
                    CalloutTitleVi = "Cạm Bẫy Tràn Bộ Nhớ Trình Duyệt Khi Tải Schema Khổng Lồ",
                    CalloutText = "A single 20MB raw openapi.json file will freeze developer browser tabs during rendering. Always partition monolith APIs so that each individual document stays under 1MB to 2MB.",
                    CalloutTextVi = "Một file openapi.json có dung lượng 20MB sẽ làm đơ trình duyệt của lập trình viên khi render. Luôn chia tách tài liệu trong Monolith lớn sao cho mỗi tài liệu riêng lẻ chỉ nằm trong ngưỡng 1MB đến 2MB."
                },
                new DevArticleSection
                {
                    Heading = "2. Implementing Multi-Version Documentation with Asp.Versioning.Http",
                    HeadingVi = "2. Hiện Thực Tài Liệu Hóa Đa Phiên Bản Chuẩn Chỉ Với Asp.Versioning.Http",
                    Paragraphs = new List<string>
                    {
                        "API Versioning is mandatory for any enterprise system that cannot force third-party clients or mobile users to upgrade simultaneously. The official library for ASP.NET Core is 'Asp.Versioning.Http' and 'Asp.Versioning.Mvc.ApiExplorer'.",
                        "With Asp.Versioning, you can expose multiple versions of the same endpoint (e.g. /api/v1/orders and /api/v2/orders) using URL path segment, HTTP header (X-Api-Version), or query string parameters.",
                        "By integrating ApiExplorer with Microsoft.AspNetCore.OpenApi, endpoints are automatically grouped into their corresponding versioned OpenAPI documents without manual wiring."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Quản lý phiên bản API (API Versioning) là yêu cầu bắt buộc đối với mọi hệ thống doanh nghiệp lớn khi không thể ép buộc đối tác hoặc người dùng mobile đồng loạt cập nhật app cùng một lúc. Thư viện chính thức cho việc này là 'Asp.Versioning.Http' và 'Asp.Versioning.Mvc.ApiExplorer'.",
                        "Với Asp.Versioning, bạn có thể phục vụ nhiều phiên bản của cùng một nghiệp vụ (ví dụ: /api/v1/orders và /api/v2/orders) linh hoạt qua URL segment, HTTP header (X-Api-Version) hoặc query parameter.",
                        "Khi kết hợp ApiExplorer với Microsoft.AspNetCore.OpenApi, các endpoint sẽ được tự động phân bổ vào đúng tài liệu OpenAPI của từng phiên bản mà không cần bạn phải lọc thủ công."
                    },
                    CodeTitle = "Integrating Asp.Versioning with Microsoft.AspNetCore.OpenApi & Scalar",
                    CodeLanguage = "csharp",
                    CodeContent = """
using Asp.Versioning;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Format: v1, v2
    options.SubstituteApiVersionInUrl = true;
});

// 2. Đăng ký tài liệu cho v1 và v2
builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("v2");

var app = builder.Build();

// Tạo Version Set cho Minimal APIs
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .Build();

// Endpoint V1 (Trả về dữ liệu cơ bản)
app.MapGet("/api/v{version:apiVersion}/users", () => Results.Ok(new[] { "Nguyen Van A", "Tran Thi B" }))
   .WithApiVersionSet(versionSet)
   .MapToApiVersion(1, 0)
   .WithTags("Users");

// Endpoint V2 (Bổ sung cấu trúc đối tượng mở rộng)
app.MapGet("/api/v{version:apiVersion}/users", () => Results.Ok(new[]
{
    new { Id = 1, FullName = "Nguyen Van A", Email = "a@company.com", Tier = "VIP" },
    new { Id = 2, FullName = "Tran Thi B", Email = "b@company.com", Tier = "Standard" }
}))
.WithApiVersionSet(versionSet)
.MapToApiVersion(2, 0)
.WithTags("Users");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();
""",
                    CalloutType = "tip",
                    CalloutTitle = "Deprecating Older API Versions Gracefully",
                    CalloutTitleVi = "Đánh Dấu Deprecated Cho Phiên Bản API Cũ",
                    CalloutText = "You can flag older API versions with HasDeprecatedApiVersion(new ApiVersion(1, 0)). Scalar and Swagger UI will automatically display a visible warning badge, signaling to client developers that migration to v2 is required.",
                    CalloutTextVi = "Bạn có thể đánh dấu phiên bản cũ bằng lệnh HasDeprecatedApiVersion(new ApiVersion(1, 0)). Scalar và Swagger UI sẽ tự động hiển thị huy hiệu cảnh báo gạch ngang, nhắc nhở các đối tác khẩn trương nâng cấp sang v2."
                },
                new DevArticleSection
                {
                    Heading = "3. Enterprise Security Schemes: OAuth2, OpenID Connect, JWT & API Key Coexistence",
                    HeadingVi = "3. Lược Đồ Bảo Mật Doanh Nghiệp: Kết Hợp OAuth2, OpenID Connect, JWT & API Key",
                    Paragraphs = new List<string>
                    {
                        "Large monoliths rarely rely on a single authentication scheme. Instead, they handle three concurrent tiers: Anonymous (public health checks), Internal Partners (authenticated via 'X-Api-Key' headers), and End-Users (authenticated via OpenID Connect / OAuth2 Authorization Code flow with PKCE or JWT Bearer tokens).",
                        "Through OpenAPI Transformers, you can declare all three schemes inside OpenApiComponents, and apply them selectively to individual endpoint groups using Endpoint Metadata.",
                        "Scalar natively supports interactive OAuth2 login popups: developers can click 'Authorize', sign in via corporate Okta or Keycloak, and have the resulting bearer token automatically attached to all subsequent interactive test requests."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Hệ thống Monolith lớn hiếm khi chỉ dùng một phương thức xác thực duy nhất. Thực tế thường bao gồm 3 tầng bảo mật song song: Công khai (kiểm tra sức khỏe hệ thống), Đối tác kết nối trực tiếp (xác thực qua header 'X-Api-Key'), và Người dùng đầu cuối (xác thực qua OpenID Connect / OAuth2 với PKCE hoặc JWT Bearer token).",
                        "Thông qua OpenAPI Transformers, bạn có thể khai báo đầy đủ cả 3 lược đồ này trong OpenApiComponents, và gán linh hoạt vào từng nhóm endpoint thông qua Metadata.",
                        "Scalar hỗ trợ tuyệt vời cửa sổ đăng nhập OAuth2 tương tác: Lập trình viên chỉ cần bấm nút 'Authorize', đăng nhập qua Keycloak/Okta của công ty, và token nhận về sẽ được tự động gắn vào mọi request thử nghiệm tiếp theo."
                    },
                    CodeTitle = "Multi-Scheme Security Configuration with DocumentTransformer",
                    CodeLanguage = "csharp",
                    CodeContent = """
options.AddDocumentTransformer((document, context, cancellationToken) =>
{
    document.Components ??= new OpenApiComponents();

    // 1. Lược đồ API Key dành cho Server-to-Server
    document.Components.SecuritySchemes["ApiKeyAuth"] = new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Khóa bảo mật dành riêng cho đối tác tích hợp hệ thống"
    };

    // 2. Lược đồ OAuth2 với luồng Authorization Code + PKCE
    document.Components.SecuritySchemes["OAuth2Auth"] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://auth.company.com/connect/authorize"),
                TokenUrl = new Uri("https://auth.company.com/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    ["api.read"] = "Quyền đọc dữ liệu nghiệp vụ",
                    ["api.write"] = "Quyền tạo và chỉnh sửa giao dịch"
                }
            }
        }
    };

    return Task.CompletedTask;
});
""",
                    CalloutType = "info",
                    CalloutTitle = "Securing Documentation Endpoints in Production",
                    CalloutTitleVi = "Bảo Mật Đường Dẫn Tài Liệu Trên Môi Trường Production",
                    CalloutText = "Never expose raw internal OpenAPI specs to the public internet without protection. Protect MapOpenApi() and MapScalarApiReference() behind RequireAuthorization('AdminPolicy') or restrict access via Reverse Proxy IP whitelisting.",
                    CalloutTextVi = "Tuyệt đối không để lộ file spec và giao diện tài liệu nội bộ ra ngoài Internet công cộng trên môi trường Production. Hãy bảo vệ bằng middleware RequireAuthorization('AdminPolicy') hoặc chặn IP tại Reverse Proxy."
                }
            }
        };
        series.Articles.Add(article4);

        // BÀI 5
        var article5 = new DevArticle
        {
            Id = "dotnet-api-docs-article-5-microservices-gateway-aggregation-yarp",
            SeriesId = series.Id,
            LessonNumber = "Bài 5",
            Level = "Production",
            LevelVi = "Thực chiến",
            LevelOrder = 4,
            Title = "Microservices Documentation & Testing: Gateway Aggregation (YARP), Developer Portals & In-IDE .http Testing",
            TitleVi = "Tài Liệu & Thử Nghiệm Trong Microservices: Gom Docs Qua Gateway YARP, Developer Portal & File .http",
            ReadingTime = "20 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Build a unified Developer Portal for distributed microservices: Aggregate decentralized OpenAPI endpoints via YARP Reverse Proxy, isolate internal specs from public gateways, and implement modern team-level testing workflows with Git-tracked .http files and Bruno instead of heavy cloud tools.",
            SummaryVi = "Xây dựng Developer Portal tập trung cho kiến trúc Microservices phân tán: Gom tất cả OpenAPI endpoints của các service con qua YARP Reverse Proxy, phân tách tài liệu nội bộ với gateway công khai, và chuẩn hóa quy trình test đồng đội với file .http lưu trong Git và Bruno thay cho các công cụ cloud cồng kềnh.",
            Tags = new List<string> { "Microservices", "YARP", "API Gateway", "Developer Portal", ".http Files", "Bruno", "Testing" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Microservices Documentation Dilemma: Fragmented Ports vs Central Portal",
                    HeadingVi = "1. Nghịch Lý Tài Liệu Microservices: Cổng Phân Mảnh vs Cổng Tập Trung",
                    Paragraphs = new List<string>
                    {
                        "When moving from a monolith to microservices, documentation quickly descends into chaos. Identity Service runs on port 5001, Order Service on 5002, Payment Service on 5003, and Inventory on 5004. Engineers must bookmark a dozen browser tabs just to explore system APIs.",
                        "Furthermore, in a production Kubernetes cluster, individual microservices should never be exposed directly to the outside world; they reside on a private internal overlay network behind an Ingress or API Gateway.",
                        "The solution is Document Aggregation at the API Gateway level: each downstream microservice exposes a lightweight '/openapi/v1.json' endpoint internally, while the Gateway acts as a centralized Developer Portal, fetching and rendering all service contracts from a single unified URL."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khi chuyển từ Monolith sang Microservices, bài toán tài liệu hóa nhanh chóng rơi vào hỗn loạn: Identity Service chạy port 5001, Order Service chạy 5002, Payment Service chạy 5003, Inventory chạy 5004. Lập trình viên phải lưu hàng chục bookmark trình duyệt chỉ để tra cứu API.",
                        "Thêm vào đó, trong cụm Kubernetes thực tế, các microservice nội bộ tuyệt đối không được mở port trực tiếp ra Internet mà phải nằm an toàn sau API Gateway hoặc Ingress Controller.",
                        "Giải pháp chuẩn mực là Gom tài liệu tập trung (Documentation Aggregation) tại tầng API Gateway: Mỗi microservice chỉ cần expose file đặc tả siêu nhẹ '/openapi/v1.json' trong mạng nội bộ, còn Gateway đóng vai trò là một Developer Portal duy nhất, tập hợp và hiển thị toàn bộ tài liệu từ một địa chỉ duy nhất."
                    },
                    CodeTitle = "Decentralized Microservices vs Centralized Gateway Portal Architecture",
                    CodeLanguage = "text",
                    CodeContent = """
[Bên Ngoài / Developer / Đối Tác]
             |
             v  (Truy cập 1 địa chỉ duy nhất: https://api.company.com/scalar)
    +-------------------------------------------------------------+
    |           API GATEWAY (YARP Reverse Proxy & Scalar)         |
    |  - Dropdown Menu: [ Identity Service | Orders | Payment ]   |
    +-------------------------------------------------------------+
             |                     |                     |
   (Mạng nội bộ K8s)       (Mạng nội bộ K8s)      (Mạng nội bộ K8s)
             v                     v                     v
    +-----------------+   +-----------------+   +-----------------+
    | Identity Service|   |  Order Service  |   | Payment Service |
    | :5001/openapi   |   |  :5002/openapi  |   |  :5003/openapi  |
    +-----------------+   +-----------------+   +-----------------+
""",
                    CalloutType = "warning",
                    CalloutTitle = "Never Expose Internal Service Ports Directly",
                    CalloutTitleVi = "Tuyệt Đối Không Expose Trực Tiếp Port Từng Microservice Ra Ngoài",
                    CalloutText = "Exposing internal ports bypasses gateway rate-limiting, CORS, authentication checks, and WAF rules. All consumer interactions—including documentation exploration—must route through the Gateway.",
                    CalloutTextVi = "Mở port trực tiếp của từng service con sẽ làm vô hiệu hóa cơ chế Rate Limiting, kiểm soát CORS, xác thực tập trung và tường lửa WAF của Gateway. Mọi truy cập cần được điều hướng tập trung qua Gateway."
                },
                new DevArticleSection
                {
                    Heading = "2. Building an Aggregated Developer Portal with YARP & Scalar",
                    HeadingVi = "2. Xây Dựng Cổng Tài Liệu Tập Trung Bằng YARP & Scalar",
                    Paragraphs = new List<string>
                    {
                        "YARP (Yet Another Reverse Proxy) is Microsoft's high-performance, battle-tested reverse proxy toolkit for .NET. We can easily configure YARP to route OpenAPI spec requests from the gateway to internal service containers.",
                        "In the Gateway's appsettings.json, we define routes mapping '/services/{serviceName}/openapi.json' to the respective downstream destination addresses.",
                        "Then, in Scalar, we configure multiple document sources. Developers navigating to the Gateway will see a convenient dropdown menu allowing them to switch seamlessly between Identity, Order, and Payment APIs."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "YARP (Yet Another Reverse Proxy) là bộ công cụ reverse proxy hiệu năng cực cao do chính Microsoft phát triển cho hệ sinh thái .NET. Chúng ta có thể dùng YARP để định tuyến các request lấy file OpenAPI từ Gateway về các container nội bộ.",
                        "Trong file appsettings.json của Gateway, ta cấu hình các Route ánh xạ đường dẫn '/services/{serviceName}/openapi.json' đến từng địa chỉ cụ thể của microservice trong mạng riêng.",
                        "Tại tầng giao diện của Gateway, ta thiết lập Scalar với danh sách các document. Khi truy cập, lập trình viên sẽ thấy một menu dropdown tiện lợi cho phép chuyển đổi qua lại giữa tài liệu của Identity, Order và Payment trong nháy mắt."
                    },
                    CodeTitle = "YARP appsettings.json Configuration for Aggregating OpenAPI Specs",
                    CodeLanguage = "json",
                    CodeContent = """
{
  "ReverseProxy": {
    "Routes": {
      "identity-openapi": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/services/identity/openapi.json" },
        "Transforms": [ { "PathSet": "/openapi/v1.json" } ]
      },
      "order-openapi": {
        "ClusterId": "order-cluster",
        "Match": { "Path": "/services/orders/openapi.json" },
        "Transforms": [ { "PathSet": "/openapi/v1.json" } ]
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": { "dest1": { "Address": "http://identity-service:5001" } }
      },
      "order-cluster": {
        "Destinations": { "dest1": { "Address": "http://order-service:5002" } }
      }
    }
  }
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Gateway Program.cs Configuration",
                    CalloutTitleVi = "Cấu Hình Program.cs Tại Tầng Gateway",
                    CalloutText = "On the Gateway, configure Scalar with: options.WithTitle('Enterprise Developer Portal').AddServer('https://api.company.com'). Now developers can test all microservice endpoints against the live gateway URL.",
                    CalloutTextVi = "Tại Gateway, bạn cấu hình Scalar trỏ về địa chỉ server chung: options.AddServer('https://api.company.com'). Toàn bộ request test của mọi microservice sẽ tự động đi qua Gateway với đầy đủ chính sách xác thực và logging."
                },
                new DevArticleSection
                {
                    Heading = "3. Modern Team Testing Workflow: Version-Controlled .http Files & Bruno Over Postman",
                    HeadingVi = "3. Quy Trình Thử Nghiệm Đồng Đội Hiện Đại: Quản Lý File .http & Bruno Trong Git Thay Vì Postman",
                    Paragraphs = new List<string>
                    {
                        "While browser UIs like Scalar are great for exploring APIs, day-to-day engineering workflows require reproducible, automated, and team-shared test requests.",
                        "Many organizations are actively moving away from Postman due to mandatory cloud account logins, confidential security token sync risks, and expensive team license paywalls.",
                        "The modern alternative in .NET is two-fold: First, Visual Studio and VS Code native '.http' files stored directly inside the Git repository alongside your code. Second, Bruno: an open-source, offline-first GUI client that stores collections as plain text files in your repository, enabling seamless peer reviews and Git version control."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Mặc dù giao diện web như Scalar rất tuyệt vời để khám phá và đọc tài liệu, công việc lập trình hằng ngày của đội ngũ kỹ sư đòi hỏi các kịch bản test có thể tái lập, chạy tự động và dễ dàng chia sẻ giữa các thành viên.",
                        "Rất nhiều tổ chức công nghệ hiện nay đang rời bỏ Postman do chính sách bắt buộc đồng bộ dữ liệu lên cloud, nguy cơ rò rỉ Token bảo mật và chi phí bản quyền đắt đỏ.",
                        "Xu hướng chuẩn mực mới trong thế giới .NET bao gồm hai trụ cột: Thứ nhất là các file '.http' được hỗ trợ gốc trong Visual Studio và VS Code, lưu trực tiếp trong Git cùng mã nguồn. Thứ hai là Bruno: phần mềm mã nguồn mở chạy offline 100%, lưu toàn bộ kịch bản test dưới dạng các file văn bản thuần túy trong Git, giúp review code kịch bản test dễ dàng qua Pull Request."
                    },
                    CodeTitle = "Production-Ready .http Test File with Environment Variables & Chained Requests",
                    CodeLanguage = "http",
                    CodeContent = """
### Biến môi trường cấu hình
@gatewayUrl = https://localhost:7001
@username = dev-tester@company.com
@password = SuperSecretP@ss123

### 1. Đăng nhập lấy Bearer Token qua API Gateway
# @name loginRequest
POST {{gatewayUrl}}/api/v1/auth/token
Content-Type: application/json

{
  "email": "{{username}}",
  "password": "{{password}}"
}

### Lưu biến Token từ Response trả về
@authToken = {{loginRequest.response.body.accessToken}}

### 2. Tạo đơn hàng mới (Sử dụng Token đã lấy ở bước 1)
# @name createOrder
POST {{gatewayUrl}}/api/v1/orders
Authorization: Bearer {{authToken}}
Content-Type: application/json
X-Correlation-ID: test-run-001

{
  "customerId": "cust-8823",
  "items": [
    { "productId": "prod-101", "quantity": 2, "unitPrice": 49.99 }
  ]
}

### Lưu Order ID vừa tạo
@createdOrderId = {{createOrder.response.body.orderId}}

### 3. Tra cứu trạng thái đơn hàng vừa tạo
GET {{gatewayUrl}}/api/v1/orders/{{createdOrderId}}
Authorization: Bearer {{authToken}}
Accept: application/json
""",
                    CalloutType = "tip",
                    CalloutTitle = "Living API Testing Documentation in Git",
                    CalloutTitleVi = "Tài Liệu Thử Nghiệm Sống Động Được Quản Lý Trong Git",
                    CalloutText = "By committing .http test files into the repository root (e.g. /tests/api-tests.http), whenever an engineer changes an endpoint, they must update the test file in the exact same commit. Contract drift is permanently eradicated.",
                    CalloutTextVi = "Bằng việc lưu trữ các file test .http ngay trong repository mã nguồn (ví dụ thư mục /tests/api-tests.http), mỗi khi sửa đổi endpoint, lập trình viên bắt buộc phải cập nhật kịch bản test trong cùng một commit. Hiện tượng lệch tài liệu và hỏng test hoàn toàn biến mất."
                }
            }
        };
        series.Articles.Add(article5);

        return series;
    }

    [Fact]
    public void TestApiDocumentationSeriesExistsAndValid()
    {
        var path = Path.GetFullPath("../../../../../wwwroot/sample-data/dev-learning.json");
        Assert.True(File.Exists(path), $"File not found at: {path}");

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var data = JsonSerializer.Deserialize<DevBlogData>(json, options);
        Assert.NotNull(data);

        var series = data.SeriesList.FirstOrDefault(s => s.Id == "dotnet-api-documentation-and-testing-series");
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
}
