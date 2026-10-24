using System.Text.Encodings.Web;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Pages;
using Xunit;

namespace BlazorWasmPortfolioGhAction.Tests;

public class MicroservicesSeriesBuilder
{
    public static DevSeries BuildSeries()
    {
        var series = new DevSeries
        {
            Id = "dotnet-microservices-docker-k8s-ecosystem-series",
            Category = "devops",
            Title = "The Production .NET Microservices Survival Kit: Seq, Kubernetes Probes, Polly Resilience, MassTransit & OpenTelemetry",
            TitleVi = "Bộ Thư Viện Sống Còn Cho .NET Microservices Trong Docker & Kubernetes: Seq, K8s Probes, Polly, MassTransit & OpenTelemetry",
            Icon = "bi-boxes",
            Color = "#0ea5e9",
            Badge = "Microservices & K8s Ecosystem",
            Description = "Essential production library stack for containerized .NET microservices in Docker and Kubernetes: Centralized structured logging with Serilog & Seq, robust Kubernetes Liveness/Readiness/Startup health probes, fault tolerance with Polly & Microsoft.Extensions.Resilience, event-driven transactional Outbox with MassTransit, and distributed tracing with OpenTelemetry.",
            DescriptionVi = "Bộ thư viện thiết yếu chuẩn Production cho các dự án .NET Microservices đóng gói Docker và triển khai Kubernetes: Quản lý log có cấu trúc tập trung với Serilog & Seq, hệ thống đầu dò sức khỏe Liveness/Readiness/Startup chuẩn K8s, khả năng chịu lỗi liên dịch vụ với Polly & Microsoft.Extensions.Resilience, mẫu thiết kế Transactional Outbox đảm bảo tin cậy với MassTransit, và phân tích truy vết phân tán với OpenTelemetry.",
            Articles = new List<DevArticle>()
        };

        // BÀI 1
        var article1 = new DevArticle
        {
            Id = "dotnet-microservices-article-1-serilog-seq-in-containers",
            SeriesId = series.Id,
            LessonNumber = "Bài 1",
            Level = "Beginner",
            LevelVi = "Cơ bản",
            LevelOrder = 1,
            Title = "Seq & Serilog in Containers: Centralized Structured Logging & Instant Microservice Troubleshooting",
            TitleVi = "Seq & Serilog Trong Container: Tập Trung Hóa Log Có Cấu Trúc & Truy Vết Sự Cố Microservices",
            ReadingTime = "15 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Eliminate the pain of debugging 50 scattered containers. Master structured logging in ASP.NET Core with Serilog and Seq: Understanding Compact Log Event Format (CLEF), non-blocking asynchronous buffering (WriteTo.Async), auto-enriching Pod metadata (ContainerId, MachineName), and writing SQL-like queries in Seq to isolate production incidents in seconds.",
            SummaryVi = "Xóa bỏ cơn ác mộng debug giữa hàng chục container phân tán. Làm chủ hệ thống ghi log có cấu trúc trong ASP.NET Core với Serilog và Seq: Bản chất định dạng Compact Log Event Format (CLEF), cơ chế đệm bất đồng bộ (WriteTo.Async) không nghẽn luồng xử lý, tự động gắn nhãn metadata của Pod (ContainerId, MachineName) và truy vấn SQL-like trong Seq để bắt lỗi chỉ trong vài giây.",
            Tags = new List<string> { "Serilog", "Seq", "Docker", "Kubernetes", "Microservices", "Structured Logging" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Container Logging Dilemma: Why Plain Text Logs Fail in Microservices",
                    HeadingVi = "1. Nghịch Lý Ghi Log Trong Container: Vì Sao Log Văn Bản Truyền Thống Thất Bại",
                    Paragraphs = new List<string>
                    {
                        "In a monolithic application running on a single server, writing plaintext logs to disk or console was often sufficient. A developer could SSH into the VM and run 'grep error /var/log/app.log' to understand what happened.",
                        "In a distributed microservice architecture running inside Docker and Kubernetes, that approach collapses completely. You may have 50 different microservice pods dynamically scaling, restarting, and sharing workload. A single user order might trigger API calls across 6 services. If one service fails, grepping plaintext logs across 50 ephemeral containers is impossible.",
                        "Furthermore, plaintext logs lose type information. If you log 'User 123 purchased item 456 for $99.50', a machine cannot easily query all orders where Price > 100 without brittle regex parsing. The solution is Structured Logging with Serilog, emitting Compact Log Event Format (CLEF) JSON events where parameters remain typed first-class citizens."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong một ứng dụng Monolith truyền thống chạy trên một máy chủ duy nhất, việc ghi log dưới dạng văn bản thuần túy (Plaintext) ra file hoặc console thường là đủ dùng. Lập trình viên chỉ cần SSH vào máy chủ và gõ lệnh 'grep error /var/log/app.log' là có thể nắm được sự cố.",
                        "Thế nhưng trong kiến trúc Microservices phân tán trên Docker và Kubernetes, cách làm này hoàn toàn phá sản. Hệ thống có thể gồm 50 Pods khác nhau liên tục co giãn, tự động khởi động lại và luân chuyển tải. Một thao tác đặt hàng của người dùng có thể kích hoạt chuỗi gọi API xuyên suốt 6 dịch vụ. Khi một mắt xích bị gãy, việc lục tìm file log văn bản giữa 50 container tạm bợ là nhiệm vụ bất khả thi.",
                        "Thêm vào đó, log văn bản làm mất hoàn toàn kiểu dữ liệu. Khi bạn ghi log 'Khách hàng 123 mua đơn 456 với giá 99.50', máy móc không thể nào lọc nhanh các giao dịch có giá > 100 mà không phải viết regex chắp vá. Giải pháp sống còn là Ghi Log Có Cấu Trúc (Structured Logging) với Serilog, xuất ra định dạng JSON chuẩn CLEF (Compact Log Event Format) bảo toàn nguyên vẹn kiểu dữ liệu của từng tham số."
                    },
                    CodeTitle = "Plaintext Fragile Log vs Structured Typed Event (CLEF)",
                    CodeLanguage = "json",
                    CodeContent = """
// ==================== CÁCH GHI LOG VĂN BẢN TRUYỀN THỐNG ====================
// Console/File: [2026-10-24 21:00:01 INFO] User 8821 created order 5012 with amount 450.00
// Nhược điểm: Không thể truy vấn toán tử: Amount > 400 mà phải viết Regular Expression cồng kềnh!

// ==================== ĐỊNH DẠNG LOG CÓ CẤU TRÚC CLEF VỚI SERILOG ====================
{
  "@t": "2026-10-24T14:00:01.1245000Z",
  "@l": "Information",
  "@mt": "User {UserId} created order {OrderId} with amount {Amount}",
  "@m": "User 8821 created order 5012 with amount 450.00",
  "UserId": 8821,
  "OrderId": 5012,
  "Amount": 450.00,
  "Currency": "USD",
  "Environment": "Production",
  "ContainerId": "order-service-7d8f99c8-4x2lm",
  "TraceId": "4bf92f3577b34da6a3ce929d0e0e4736"
}
// Ưu điểm: Seq hiểu rõ UserId là Integer, Amount là Decimal, có thể lọc: Amount >= 400 ngay tức khắc!
""",
                    CalloutType = "warning",
                    CalloutTitle = "Never String-Interpolate in Logger Calls",
                    CalloutTitleVi = "Cảnh Báo: Tuyệt Đối Không Dùng String Interpolation Khi Ghi Log",
                    CalloutText = "Never write: logger.LogInformation($\"User {userId} bought {orderId}\"). Using the '$' dollar sign formats the string immediately into plaintext before Serilog can capture parameter names and data types. Always use message templates: logger.LogInformation(\"User {UserId} bought {OrderId}\", userId, orderId).",
                    CalloutTextVi = "Tuyệt đối không viết: logger.LogInformation($\"User {userId} bought {orderId}\"). Ký tự '$' sẽ ép chuỗi thành văn bản thuần trước khi Serilog kịp lưu lại tên và kiểu dữ liệu của biến. Luôn dùng cú pháp Template: logger.LogInformation(\"User {UserId} bought {OrderId}\", userId, orderId)."
                },
                new DevArticleSection
                {
                    Heading = "2. Configuring Serilog with Seq Sink & Async Non-Blocking Buffers",
                    HeadingVi = "2. Cấu Hình Serilog Với Seq Sink & Đệm Bất Đồng Bộ Không Chặn Luồng",
                    Paragraphs = new List<string>
                    {
                        "Seq is a purpose-built, high-performance structured log aggregation server tailored specifically for .NET and modern container environments. Unlike the heavy ElasticSearch / Logstash / Kibana (ELK) stack which demands gigabytes of RAM just to boot, Seq runs in a lightweight container using under 250MB of memory.",
                        "When shipping logs from a high-throughput microservice, you must never allow HTTP network latency to block user request threads. Serilog solves this via the 'Serilog.Sinks.Async' wrapper: log events are placed into an in-memory ring buffer and flushed in background batches to Seq.",
                        "By utilizing Serilog Enrichers, every single log event automatically inherits contextual metadata: Pod Name (via Environment.MachineName), Application Name, Environment, and Correlation/Trace IDs without manual logger parameters."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Seq là máy chủ quản lý và thu thập log có cấu trúc chuyên dụng, được tối ưu hóa đặc biệt cho .NET và môi trường container hiện đại. Khác với bộ ba cồng kềnh ELK (Elasticsearch/Logstash/Kibana) ngốn hàng gigabyte RAM chỉ để khởi động, Seq chạy trong container siêu nhẹ với mức tiêu thụ RAM thông thường chỉ dưới 250MB.",
                        "Khi truyền tải log từ các microservice có lượng truy cập lớn, bạn tuyệt đối không được để độ trễ mạng HTTP ghi log làm nghẽn luồng xử lý (threads) của khách hàng. Serilog giải quyết xuất sắc điều này qua gói 'Serilog.Sinks.Async': các log event được đưa vào hàng đợi bộ nhớ (Ring Buffer) và được một luồng nền đẩy theo từng đợt (batch) về Seq.",
                        "Nhờ cơ chế Serilog Enrichers, từng dòng log sinh ra sẽ tự động thừa hưởng toàn bộ siêu dữ liệu ngữ cảnh: Tên Pod (qua Environment.MachineName), Tên Dịch Vụ, Môi Trường và Trace ID phân tán mà lập trình viên không cần truyền tay."
                    },
                    CodeTitle = "Production Program.cs Configuration for Serilog with Async Seq Sink",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Cài đặt NuGet:
// dotnet add package Serilog.AspNetCore
// dotnet add package Serilog.Sinks.Seq
// dotnet add package Serilog.Sinks.Async
// dotnet add package Serilog.Enrichers.Environment
// dotnet add package Serilog.Enrichers.Process

using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Khởi tạo Serilog trước khi nạp Web Host để bắt được cả lỗi khởi động sớm
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    // Tự động gắn thêm MachineName (Pod name trên K8s), ProcessId, ThreadId
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Application", "OrderService")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    // Ghi log ra Console chuẩn cho Docker daemon đọc (kubectl logs)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Application}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    // Đẩy log bất đồng bộ về Seq server: Không bao giờ làm chậm request HTTP!
    .WriteTo.Async(wt => wt.Seq(
        serverUrl: builder.Configuration["Seq:ServerUrl"] ?? "http://seq:5341",
        apiKey: builder.Configuration["Seq:ApiKey"], // API Key phân quyền theo từng Service
        bufferBaseFilename: Path.Combine(AppContext.BaseDirectory, "logs", "seq-buffer"), // Lưu file đệm nếu rớt mạng
        batchPostingLimit: 100,
        period: TimeSpan.FromSeconds(2)
    ))
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

app.MapPost("/api/orders", (OrderRequest request, ILogger<Program> logger) =>
{
    logger.LogInformation("Bắt đầu xử lý đơn hàng {OrderId} của khách hàng {CustomerId} với tổng tiền {TotalAmount}",
        request.OrderId, request.CustomerId, request.TotalAmount);

    // Business logic...
    return Results.Created($"/api/orders/{request.OrderId}", new { status = "Success" });
});

app.Run();

record OrderRequest(string OrderId, string CustomerId, decimal TotalAmount);
""",
                    CalloutType = "tip",
                    CalloutTitle = "Disk-Backed Buffer for 100% Zero Log Loss",
                    CalloutTitleVi = "Bộ Đệm Ổ Đĩa Dự Phòng Chống Mất Log Khi Rớt Mạng",
                    CalloutText = "Configuring bufferBaseFilename instructs Serilog to spill unsent logs into local container storage if the Seq server or internal network temporarily fails. As soon as Seq connectivity is restored, all buffered logs are automatically replayed.",
                    CalloutTextVi = "Tham số bufferBaseFilename hướng dẫn Serilog tự động ghi tạm log ra ổ đĩa local của container nếu Seq Server hoặc mạng nội bộ gặp sự cố. Ngay khi kết nối được tái lập, Serilog sẽ tự động đồng bộ lại toàn bộ dữ liệu log còn thiếu."
                },
                new DevArticleSection
                {
                    Heading = "3. Docker Compose Setup for Seq & High-Speed SQL-Like Filtering",
                    HeadingVi = "3. Triển Khai Docker Compose Cho Seq & Kỹ Thuật Truy Vấn SQL-Like Tốc Độ Cao",
                    Paragraphs = new List<string>
                    {
                        "Spinning up Seq locally or in your staging Kubernetes cluster is remarkably straightforward. A single official Docker image 'datalust/seq:latest' provides the ingestion engine, web UI dashboard, and alerting triggers.",
                        "In Seq's web dashboard, filtering does not require cumbersome search syntax. You write standard SQL-like expressions: 'Application = \"OrderService\" and TotalAmount > 500 and @Level = \"Error\"'.",
                        "You can also group events, calculate 95th percentile latency over time, and configure Webhook alerts to notify your engineering Slack or Discord channel the moment an unhandled exception spikes."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Triển khai Seq trên máy phát triển hoặc trên cụm Kubernetes nội bộ cực kỳ đơn giản. Chỉ với một image chính thức 'datalust/seq:latest', bạn đã có ngay hệ thống tiếp nhận log, giao diện web dashboard trực quan và hệ thống bắn cảnh báo tự động.",
                        "Trong giao diện web của Seq, việc tìm kiếm sự cố không đòi hỏi cú pháp phức tạp. Bạn viết các mệnh đề tương tự ngôn ngữ SQL: 'Application = \"OrderService\" and TotalAmount > 500 and @Level = \"Error\"'.",
                        "Bạn cũng có thể nhóm sự kiện theo thời gian, tính toán độ trễ trung vị P95 của API, và cấu hình Webhook tự động bắn tin nhắn cảnh báo về kênh Slack hoặc Discord của đội ngũ kỹ thuật ngay khi phát hiện số lượng ngoại lệ tăng đột biến."
                    },
                    CodeTitle = "Docker Compose Configuration & Common Seq Query Cheat Sheet",
                    CodeLanguage = "yaml",
                    CodeContent = """
# docker-compose.yml khởi chạy Seq Server nội bộ
version: '3.8'

services:
  seq:
    image: datalust/seq:latest
    container_name: dev-seq
    restart: unless-stopped
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:80" # Cổng truy cập Web Dashboard UI của Seq
    volumes:
      - seq_data:/data

volumes:
  seq_data:

# ==================== CÚ PHÁP TRUY VẤN SQL-LIKE TRONG SEQ ====================
# 1. Tìm tất cả lỗi nghiêm trọng trong OrderService:
# Application = 'OrderService' and @Level = 'Error'

# 2. Lọc đơn hàng có giá trị cao của khách hàng VIP:
# CustomerId = 'cust-99' and TotalAmount >= 1000

# 3. Đếm số lượng lỗi theo từng Exception Type trong 1 giờ qua:
# select count(*) from stream where @Level = 'Error' group by Exception.Type

# 4. Tìm kiếm request có thời gian xử lý chậm hơn 2 giây:
# ElapsedMilliseconds > 2000
""",
                    CalloutType = "tip",
                    CalloutTitle = "Instant Incident Resolution",
                    CalloutTitleVi = "Giải Cứu Sự Cố Trong Chớp Mắt",
                    CalloutText = "When customer support reports an issue with Order #5012, simply type 'OrderId = 5012' in Seq. You will instantly see every single log statement emitted by all 5 microservices that touched that order, in chronological millisecond precision.",
                    CalloutTextVi = "Khi bộ phận chăm sóc khách hàng báo đơn #5012 gặp lỗi, bạn chỉ cần gõ 'OrderId = 5012' trên thanh tìm kiếm của Seq. Toàn bộ hành trình của đơn hàng đi qua 5 dịch vụ khác nhau sẽ hiển thị rành mạch theo thứ tự thời gian tới từng mili-giây."
                }
            }
        };
        series.Articles.Add(article1);

        // BÀI 2
        var article2 = new DevArticle
        {
            Id = "dotnet-microservices-article-2-k8s-probes-healthchecks",
            SeriesId = series.Id,
            LessonNumber = "Bài 2",
            Level = "Intermediate",
            LevelVi = "Trung cấp",
            LevelOrder = 2,
            Title = "Kubernetes Probes & ASP.NET Core Health Checks: Liveness, Readiness, Startup & Real-Time Monitoring",
            TitleVi = "Kubernetes Probes & ASP.NET Core Health Checks: Liveness, Readiness, Startup & Giám Sát Thời Gian Thực",
            ReadingTime = "16 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Keep Kubernetes from killing your pods prematurely or routing traffic to dead services. Deep dive into AspNetCore.Diagnostics.HealthChecks: Implementing the tri-probe architecture (/healthz/live, /healthz/ready, /healthz/startup), checking SQL/Redis/RabbitMQ dependencies, and embedding the HealthChecks-UI dashboard.",
            SummaryVi = "Ngăn chặn Kubernetes tiêu diệt Pod nhầm lúc khởi động hoặc điều hướng người dùng vào Pod bị mất kết nối DB. Tìm hiểu sâu thư viện AspNetCore.Diagnostics.HealthChecks: Hiện thực kiến trúc 3 đầu dò chuẩn K8s (/healthz/live, /healthz/ready, /healthz/startup), kiểm tra sức khỏe phụ thuộc SQL/Redis/RabbitMQ, và nhúng bảng điều khiển trực quan HealthChecks-UI.",
            Tags = new List<string> { "Kubernetes", "Health Checks", "Liveness", "Readiness", "Startup Probe", "DevOps" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Demystifying the K8s Tri-Probe Model: Liveness vs Readiness vs Startup",
                    HeadingVi = "1. Giải Mã Bản Chất 3 Đầu Dò K8s: Liveness vs Readiness vs Startup",
                    Paragraphs = new List<string>
                    {
                        "Kubernetes relies on active probes (Kubelet sending periodic HTTP requests) to make critical life-cycle decisions about your containerized pods. A fundamental mistake made by many engineering teams is implementing a single '/health' endpoint that checks everything.",
                        "If your single health check tests the database, and the database suffers a temporary network hiccup, Kubernetes interprets the failure as a dead container and restarts the pod. Restarting the pod does not fix the database; instead, it triggers a cascading restart storm across your entire cluster.",
                        "Kubernetes explicitly provides three distinct probe types to separate these concerns: Startup Probe (has the application finished initializing?), Readiness Probe (is the application currently able to serve incoming traffic?), and Liveness Probe (is the process deadlocked or stuck in an unrecoverable state?)."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Kubernetes vận hành dựa vào các đầu dò định kỳ (Kubelet gửi request HTTP kiểm tra) để đưa ra quyết định sống còn đối với các Pod trong cụm. Sai lầm kinh điển của rất nhiều đội ngũ là chỉ tạo duy nhất một endpoint '/health' chung chung để kiểm tra tất cả mọi thứ.",
                        "Nếu endpoint chung này kiểm tra cả cơ sở dữ liệu, và database bị nghẽn mạng trong 5 giây, Kubernetes sẽ hiểu nhầm rằng bản thân Pod ứng dụng đã chết và ra lệnh cưỡng chế khởi động lại (restart) Pod. Việc restart Pod không hề giải quyết được sự cố của database; ngược lại, nó tạo ra một cơn bão khởi động lại liên hoàn (Restart Storm) làm tê liệt toàn bộ hệ thống.",
                        "Kubernetes cung cấp 3 cơ chế đầu dò độc lập để giải quyết triệt để vấn đề này: Startup Probe (Ứng dụng đã nạp xong hết dữ liệu khởi động chưa?), Readiness Probe (Ứng dụng hiện tại có đủ điều kiện để nhận traffic từ người dùng không?), và Liveness Probe (Tiến trình có bị deadlock/treo ngầm không?)."
                    },
                    CodeTitle = "Kubernetes Pod Specification with Tri-Probe Architecture",
                    CodeLanguage = "yaml",
                    CodeContent = """
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-service
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: order-api
        image: company/order-service:1.0.0
        ports:
        - containerPort: 8080
        
        # 1. STARTUP PROBE: Cho phép ứng dụng có tối đa 60s để nạp dữ liệu/cache ban đầu
        startupProbe:
          httpGet:
            path: /healthz/startup
            port: 8080
          failureThreshold: 30
          periodSeconds: 2 # 30 * 2s = 60s tối đa
          
        # 2. LIVENESS PROBE: Chỉ kiểm tra tiến trình có sống không. Nếu fail -> RESTART POD!
        livenessProbe:
          httpGet:
            path: /healthz/live
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 10
          failureThreshold: 3
          
        # 3. READINESS PROBE: Kiểm tra kết nối DB/Redis. Nếu fail -> NGẮT TRAFFIC, KHÔNG RESTART!
        readinessProbe:
          httpGet:
            path: /healthz/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
          failureThreshold: 2
""",
                    CalloutType = "warning",
                    CalloutTitle = "Golden Rule of Liveness Probes",
                    CalloutTitleVi = "Nguyên Tắc Vàng Của Liveness Probe",
                    CalloutText = "NEVER check external dependencies (Database, Cache, Message Broker) in your Liveness Probe. The Liveness Probe should only verify that your ASP.NET Core Kestrel process is alive and responsive. If your database goes down, your Readiness probe should fail to stop traffic, NOT your Liveness probe.",
                    CalloutTextVi = "TUYỆT ĐỐI KHÔNG kiểm tra các thành phần bên ngoài (DB, Redis, RabbitMQ) trong Liveness Probe. Liveness Probe chỉ được phép kiểm tra xem tiến trình ASP.NET Core có bị treo đơ hay không. Khi DB rớt mạng, chỉ Readiness Probe được phép báo đỏ để cắt traffic, còn Liveness Probe phải tiếp tục sống."
                },
                new DevArticleSection
                {
                    Heading = "2. Implementing Multi-Tier Health Checks in ASP.NET Core",
                    HeadingVi = "2. Hiện Thực Hệ Thống Health Check Đa Tầng Trong ASP.NET Core",
                    Paragraphs = new List<string>
                    {
                        "The official community package 'AspNetCore.Diagnostics.HealthChecks' provides ready-made, battle-tested health probes for virtually every enterprise service: PostgreSQL, SQL Server, Redis, RabbitMQ, Kafka, AWS S3, and Elasticsearch.",
                        "Using Health Check Tags, we classify each probe into its corresponding tier: Tag 'live' for the internal self-check, Tag 'ready' for external database and queue connectivity, and Tag 'startup' for long-running initialization tasks.",
                        "By configuring MapHealthChecks with custom ResponseWriters, we can format health outputs into rich JSON structures containing response times and error descriptions for monitoring agents."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Hệ sinh thái thư viện 'AspNetCore.Diagnostics.HealthChecks' cung cấp sẵn các bộ kiểm tra sức khỏe được tôi luyện qua môi trường Production cho hầu hết mọi dịch vụ doanh nghiệp: PostgreSQL, SQL Server, Redis, RabbitMQ, Kafka, AWS S3 và Elasticsearch.",
                        "Bằng cách sử dụng cơ chế Tags trong Health Check, chúng ta dễ dàng phân loại từng đầu dò: Tag 'live' dành cho kiểm tra nội tại của ứng dụng, Tag 'ready' dành cho kiểm tra kết nối cơ sở dữ liệu và hàng đợi, và Tag 'startup' dành cho các tác vụ khởi động ban đầu.",
                        "Thông qua việc cấu hình MapHealthChecks với ResponseWriter tùy biến, chúng ta có thể định dạng kết quả trả về dưới dạng JSON chi tiết chứa thời gian phản hồi (latency) và thông điệp lỗi để các công cụ giám sát phân tích."
                    },
                    CodeTitle = "Configuring Multi-Tier Tagged Probes in Program.cs",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Nuget:
// dotnet add package AspNetCore.HealthChecks.Npgsql
// dotnet add package AspNetCore.HealthChecks.Redis
// dotnet add package AspNetCore.HealthChecks.RabbitMQ

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ Health Checks với các Tags phân loại rành mạch
builder.Services.AddHealthChecks()
    // 1. Kiểm tra nội tại ứng dụng (Liveness)
    .AddCheck("self", () => HealthCheckResult.Healthy("Tiến trình Web API đang hoạt động bình thường"), tags: ["live"])
    
    // 2. Kiểm tra các Dependency cơ sở hạ tầng (Readiness)
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("Postgres")!,
        name: "PostgreSQL Database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"])
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
        name: "Redis Cache Cluster",
        failureStatus: HealthStatus.Degraded, // Redis lỗi chỉ xem là Degraded, không nhất thiết Unhealthy
        tags: ["ready"]);

var app = builder.Build();

// Endpoint LIVENESS: Trả về 200 OK nếu self check thành công
app.MapHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

// Endpoint READINESS: Chỉ trả về 200 OK nếu toàn bộ Postgres và Redis đều sẵn sàng
app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                component = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

app.Run();
""",
                    CalloutType = "info",
                    CalloutTitle = "Understanding HealthStatus Levels",
                    CalloutTitleVi = "Hiểu Rõ 3 Cấp Độ HealthStatus",
                    CalloutText = "ASP.NET Core defines three health statuses: Healthy (everything is optimal), Degraded (a non-critical service like Redis cache is down, but the API can still function using database fallback), and Unhealthy (a critical dependency like Primary SQL is unavailable).",
                    CalloutTextVi = "ASP.NET Core định nghĩa 3 cấp độ sức khỏe: Healthy (mọi thứ hoàn hảo), Degraded (thành phần phụ như Redis cache bị rớt nhưng hệ thống vẫn chạy được bằng cách đọc thẳng từ DB), và Unhealthy (thành phần sống còn như Database chính bị mất kết nối hoàn toàn)."
                },
                new DevArticleSection
                {
                    Heading = "3. Visualizing Microservice Health with HealthChecks-UI Dashboard",
                    HeadingVi = "3. Trực Quan Hóa Sức Khỏe Toàn Cụm Bằng Bảng Điều Khiển HealthChecks-UI",
                    Paragraphs = new List<string>
                    {
                        "While Kubernetes probes operate behind the scenes, operations and DevOps engineers require a visual status dashboard to immediately spot degraded services across multiple clusters.",
                        "The package 'AspNetCore.HealthChecks.UI' provides a standalone, beautifully styled dashboard that periodically polls the readiness endpoints of all your microservices and renders their historical uptime and latency charts.",
                        "It can be deployed as an independent lightweight monitoring container or integrated with Webhooks to send instant push notifications to Microsoft Teams or Slack when a service drops below acceptable SLAs."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong khi các đầu dò Kubernetes hoạt động tự động ở hậu trường, đội ngũ vận hành và DevOps cần một bảng điều khiển trực quan để theo dõi tức thời trạng thái của toàn bộ các dịch vụ trên cụm.",
                        "Gói thư viện 'AspNetCore.HealthChecks.UI' cung cấp một bảng điều khiển web độc lập với giao diện hiện đại, tự động thăm dò định kỳ endpoint readiness của toàn bộ microservices và hiển thị biểu đồ lịch sử hoạt động cùng độ trễ.",
                        "Dashboard này có thể được triển khai như một Pod giám sát độc lập, hỗ trợ lưu trữ lịch sử kiểm tra vào SQLite/Postgres và tích hợp Webhook để gửi cảnh báo tức thì về Slack hoặc Teams khi có bất kỳ service nào chuyển sang trạng thái đỏ."
                    },
                    CodeTitle = "HealthChecks UI Dashboard Setup in appsettings.json",
                    CodeLanguage = "json",
                    CodeContent = """
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "Identity Service",
        "Uri": "http://identity-service:8080/healthz/ready"
      },
      {
        "Name": "Order Service",
        "Uri": "http://order-service:8080/healthz/ready"
      },
      {
        "Name": "Payment Service",
        "Uri": "http://payment-service:8080/healthz/ready"
      }
    ],
    "EvaluationTimeInSeconds": 15,
    "MinimumSecondsBetweenFailureNotifications": 60,
    "Webhooks": [
      {
        "Name": "Slack Alert",
        "Uri": "https://hooks.slack.com/services/YOUR/WEBHOOK/URL",
        "Payload": "{\"text\": \"Cảnh báo: Dịch vụ [[LNAME]] trên môi trường K8s đang bị UNHEALTHY! Chi tiết: [[FAILURE]]\"}"
      }
    ]
  }
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Dedicated Monitoring Pod",
                    CalloutTitleVi = "Tách Riêng Pod Giám Sát Sức Khỏe",
                    CalloutText = "In enterprise production architectures, run HealthChecks-UI as its own isolated deployment in a 'monitoring' namespace rather than embedding it inside business microservices, guaranteeing dashboard availability even if business services are under heavy load.",
                    CalloutTextVi = "Trong môi trường doanh nghiệp, hãy chạy HealthChecks-UI thành một Pod độc lập nằm trong namespace 'monitoring' thay vì nhúng trực tiếp vào các microservice nghiệp vụ, đảm bảo bạn luôn vào được bảng điều khiển kể cả khi các service nghiệp vụ đang bị quá tải."
                }
            }
        };
        series.Articles.Add(article2);

        // BÀI 3
        var article3 = new DevArticle
        {
            Id = "dotnet-microservices-article-3-polly-resilience-fault-tolerance",
            SeriesId = series.Id,
            LessonNumber = "Bài 3",
            Level = "Intermediate",
            LevelVi = "Trung cấp",
            LevelOrder = 2,
            Title = "Polly & Microsoft.Extensions.Resilience: Circuit Breaker, Exponential Backoff & Fault Tolerance",
            TitleVi = "Polly & Microsoft.Extensions.Resilience: Circuit Breaker, Exponential Backoff & Khả Năng Chịu Lỗi",
            ReadingTime = "17 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Stop cascading failures from bringing down your microservice cluster. Master modern resilience in .NET with Polly v8 and Microsoft.Extensions.Resilience: Exponential backoff with jitter to solve the thundering herd problem, Circuit Breaker state machines, Hedging for low-latency queries, and standard resilience pipeline configuration.",
            SummaryVi = "Chặn đứng hiện tượng lỗi dây chuyền (Cascading Failure) làm sập toàn bộ cụm microservices. Làm chủ khả năng chịu lỗi hiện đại trong .NET với Polly v8 và Microsoft.Extensions.Resilience: Thử lại số mũ có độ lệch ngẫu nhiên (Exponential Backoff + Jitter) chống bão request, cỗ máy trạng thái Circuit Breaker, kỹ thuật Hedging cho truy vấn độ trễ thấp và đường ống phục hồi chuẩn hóa.",
            Tags = new List<string> { "Polly", "Resilience", "Circuit Breaker", "Retry", "Microservices", ".NET" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Cascading Failure Disaster: Why Microservice Networks Are Fragile",
                    HeadingVi = "1. Thảm Họa Lỗi Dây Chuyền: Vì Sao Mạng Lưới Microservice Rất Dễ Tổn Thương",
                    Paragraphs = new List<string>
                    {
                        "In a distributed system, network latency, temporary connection resets, and container redeployments are everyday facts of life. When Service A makes a synchronous HTTP call to Service B, it assumes a fragile dependency.",
                        "If Service B begins slowing down under heavy database lock contention, incoming requests in Service A will begin waiting. As more user requests arrive, threads in Service A are blocked waiting for Service B. Within minutes, Service A exhausts its .NET Thread Pool, causing Service A to become completely unresponsive.",
                        "This domino effect is known as Cascading Failure. A minor bug in a non-essential service (like Recommendation or Reviews) can easily bring down your critical Checkout and Payment flows unless strict fault isolation policies are enforced."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong hệ thống phân tán, độ trễ mạng bất thường, mất kết nối chớp nhoáng và việc container tự động tái khởi động là những điều xảy ra như cơm bữa. Khi Service A thực hiện một cuộc gọi HTTP đồng bộ sang Service B, nó đã thiết lập một mắt xích phụ thuộc đầy rủi ro.",
                        "Nếu Service B bị chậm do nghẽn khóa dữ liệu dưới database, các request gửi từ Service A sẽ phải xếp hàng chờ đợi. Khi lượng người dùng tiếp tục ồ ạt đổ vào, toàn bộ luồng xử lý (Threads) trong Service A sẽ bị khóa cứng để đợi Service B. Chỉ trong vài phút, Service A sẽ cạn kiệt Thread Pool và tê liệt hoàn toàn.",
                        "Hiệu ứng domino này được gọi là Thảm họa lỗi dây chuyền (Cascading Failure). Một trục trặc nhỏ ở một dịch vụ phụ (như Gợi ý sản phẩm hay Đánh giá) hoàn toàn có thể kéo sập luôn cả quy trình Đặt hàng và Thanh toán cốt lõi nếu không có cơ chế cô lập lỗi nghiêm ngặt."
                    },
                    CodeTitle = "Timeline of a Cascading Failure Disaster",
                    CodeLanguage = "text",
                    CodeContent = """
[Người Dùng] ---> [Order Service] ---> [Inventory Service] (Bị quá tải DB, phản hồi mất 30s)
      |                  |                       |
(100 req/s)       (100 luồng bị khóa)      (Tắc nghẽn kết nối)
      |                  |                       |
      v                  v                       v
[Order Service cạn Thread Pool] ---> [Kubelet Liveness Probe gửi request vào /healthz bị Timeout!]
      |
      v
[K8s tưởng Order Service chết, tiêu diệt Pod và tạo mới] ---> [Pod mới khởi động lại tiếp tục bị treo!]
      |
      v
=====> TOÀN BỘ HỆ THỐNG SẬP ĐỒNG LOẠT (RESTART STORM CASCADING COLLAPSE) <=====
""",
                    CalloutType = "warning",
                    CalloutTitle = "The Danger of Naive Fixed Retries",
                    CalloutTitleVi = "Cạm Bẫy Của Thử Lại (Retry) Cố Định Thô Sơ",
                    CalloutText = "Never retry failed HTTP calls immediately with a fixed interval (e.g., retrying every 500ms). If 1,000 requests fail because downstream Service B is overloaded, immediately retrying multiplied by 3 attempts turns 1,000 requests into a 4,000-request tsunami, permanently hammering the failing service into oblivion (Thundering Herd Problem).",
                    CalloutTextVi = "Tuyệt đối không thử lại (Retry) với khoảng thời gian cố định. Nếu 1.000 requests thất bại vì Service B đang bị quá tải, việc retry dồn dập ngay tức khắc sẽ biến 1.000 requests thành một cơn sóng thần 4.000 requests dội thẳng vào Service B, khiến dịch vụ này chìm sâu vào tê liệt vĩnh viễn (Vấn nạn Thundering Herd)."
                },
                new DevArticleSection
                {
                    Heading = "2. Polly v8 & Microsoft.Extensions.Resilience: The Next-Gen Pipeline",
                    HeadingVi = "2. Làm Chủ Polly v8 & Microsoft.Extensions.Resilience: Đường Ống Phục Hồi Thế Hệ Mới",
                    Paragraphs = new List<string>
                    {
                        "For years, Polly was the gold standard resilience library in .NET, but configuring complex policies in Polly v7 was verbose and suffered from object allocations. With Polly v8 (Polly.Core), the team completely re-architected the library from scratch for zero allocations and exceptional performance.",
                        "Microsoft officially partnered with the Polly team to ship 'Microsoft.Extensions.Resilience', introducing 'AddStandardResilienceHandler()'. With a single line of code, your HttpClient is shielded with an optimal, production-tuned resilience pipeline.",
                        "The Standard Resilience Pipeline stacks 5 essential defense layers in precise mathematical order: Total Request Timeout -> Retry with Exponential Backoff and Jitter -> Circuit Breaker -> Per-Attempt Timeout -> Rate Limiter."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Suốt nhiều năm, Polly là tiêu chuẩn vàng về khả năng phục hồi lỗi trong .NET, nhưng cách cấu hình ở phiên bản Polly v7 khá rườm rà và tốn tài nguyên cấp phát bộ nhớ. Với phiên bản Polly v8 (Polly.Core), đội ngũ phát triển đã viết lại toàn bộ từ gốc, tối ưu hóa triệt để không cấp phát bộ nhớ rác (Zero Allocations) và đạt hiệu năng đỉnh cao.",
                        "Microsoft đã chính thức bắt tay với Polly để phát hành gói 'Microsoft.Extensions.Resilience' tích hợp sẵn phương thức 'AddStandardResilienceHandler()'. Chỉ với duy nhất một dòng code, HttpClient của bạn sẽ được bảo vệ bởi một đường ống phòng thủ chuẩn mực nhất của các kỹ sư Microsoft.",
                        "Đường ống phục hồi tiêu chuẩn này sắp xếp 5 lớp lá chắn theo thứ tự toán học tối ưu: Timeout toàn cục -> Thử lại số mũ kèm độ lệch ngẫu nhiên (Exponential Backoff + Jitter) -> Cắt mạch (Circuit Breaker) -> Timeout cho từng lần thử -> Giới hạn tần suất (Rate Limiter)."
                    },
                    CodeTitle = "Configuring AddStandardResilienceHandler in Program.cs (.NET 8/9+)",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Cài đặt NuGet:
// dotnet add package Microsoft.Extensions.Http.Resilience

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký HttpClient có gắn sẵn Standard Resilience Pipeline
builder.Services.AddHttpClient("InventoryClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:InventoryUrl"] ?? "http://inventory-service:8080");
})
.AddStandardResilienceHandler(options =>
{
    // 1. CẤU HÌNH TIMEOUT TOÀN BỘ CUỘC GỌI (Bao gồm cả các lần retry)
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

    // 2. CẤU HÌNH THỬ LẠI EXPONENTIAL BACKOFF + JITTER
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = DelayBackoffType.Exponential;
    options.Retry.Delay = TimeSpan.FromMilliseconds(500); // Thử lại sau: ~500ms, ~1s, ~2s (+/- ngẫu nhiên vài chục ms)
    options.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<HttpRequestException>()
        .HandleResult(res => res.StatusCode == HttpStatusCode.RequestTimeout ||
                             res.StatusCode == HttpStatusCode.BadGateway ||
                             res.StatusCode == HttpStatusCode.ServiceUnavailable);

    // 3. CẤU HÌNH CIRCUIT BREAKER (NGẮT MẠCH BẢO VỆ DỊCH VỤ)
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(15);
    options.CircuitBreaker.FailureRatio = 0.5; // Nếu >= 50% request bị lỗi trong 15s -> NGẮT MẠCH (Open)
    options.CircuitBreaker.MinimumThroughput = 10; // Phải có ít nhất 10 request trong cửa sổ mới tính tỷ lệ
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30); // Giữ mạch ngắt trong 30s trước khi thử lại (Half-Open)

    // 4. TIMEOUT CHO TỪNG LẦN GỌI ĐƠN LẺ
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2.5);
});

var app = builder.Build();

app.MapGet("/api/products/{id}", async (string id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("InventoryClient");
    
    try
    {
        var response = await client.GetAsync($"/api/inventory/{id}");
        var data = await response.Content.ReadAsStringAsync();
        return Results.Ok(data);
    }
    catch (BrokenCircuitException)
    {
        // Khi mạch đang ngắt (Circuit Open), trả về dữ liệu fallback thay vì để người dùng chờ đợi!
        return Results.Ok(new { productId = id, stock = 0, isFallback = true, message = "Kho hàng tạm bảo trì" });
    }
});

app.Run();
""",
                    CalloutType = "tip",
                    CalloutTitle = "Understanding Circuit Breaker States",
                    CalloutTitleVi = "Hiểu Rõ 3 Trạng Thái Của Circuit Breaker",
                    CalloutText = "Closed (Normal operation, requests pass through) -> Open (Error threshold breached, requests fail-fast immediately without touching the network, protecting the downstream service) -> Half-Open (Break duration expires, a trial request is sent; if it succeeds, the circuit heals back to Closed).",
                    CalloutTextVi = "Closed (Bình thường, cho mọi request đi qua) -> Open (Mạch ngắt vì lỗi vượt ngưỡng, lập tức trả về lỗi ngay tức khắc không gửi qua mạng nữa, cho phép service kia hồi phục) -> Half-Open (Hết thời gian ngắt, gửi thử 1 request thăm dò; nếu thành công thì đóng mạch về lại Closed)."
                },
                new DevArticleSection
                {
                    Heading = "3. Advanced Strategies: Hedging & Graceful Fallbacks",
                    HeadingVi = "3. Chiến Lược Nâng Cao: Kỹ Thuật Hedging Dự Phòng & Phục Hồi Duyên Dáng (Graceful Fallback)",
                    Paragraphs = new List<string>
                    {
                        "While retries help recover from hard failures, they significantly increase latency. For read-heavy, idempotent endpoints (like retrieving product catalogs, currency exchange rates, or recommendation cards), Hedging is a superior pattern.",
                        "Hedging works by initiating a parallel backup request if the original request does not return within a tight threshold (e.g., 800ms). Whichever request responds first is accepted, and the slower one is cancelled, effectively shaving off the P99 latency tail.",
                        "Combined with Graceful Fallbacks (returning cached Redis data or static defaults when circuits are open), your user experience remains buttery smooth even when half of your cloud dependencies are having an outage."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Mặc dù cơ chế thử lại (Retry) giúp vượt qua các lỗi tạm thời, nó lại làm gia tăng độ trễ cho người dùng. Đối với các thao tác chỉ đọc và an toàn (Idempotent) như xem danh mục hàng hóa, tỷ giá hối đoái hoặc gợi ý sản phẩm, kỹ thuật Hedging là vũ khí lợi hại hơn rất nhiều.",
                        "Hedging hoạt động bằng cách kích hoạt thêm một request dự phòng chạy song song nếu request đầu tiên chưa kịp phản hồi sau một khoảng thời gian ngắn (ví dụ sau 800ms). Request nào trả về kết quả trước sẽ được chấp nhận ngay lập tức, và request chậm hơn sẽ bị hủy bỏ, giúp loại bỏ triệt để độ trễ đuôi P99.",
                        "Khi kết hợp với Graceful Fallbacks (trả về dữ liệu lưu tạm trong cache Redis hoặc giá trị mặc định khi mạch đang ngắt), trải nghiệm của khách hàng vẫn mượt mà không hề hay biết rằng một vài dịch vụ đám mây phía sau đang gặp sự cố."
                    },
                    CodeTitle = "Configuring Hedging with Custom Fallback Strategy",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Cấu hình Hedging Pipeline cho các thao tác đọc dữ liệu quan trọng
var resiliencePipeline = new ResiliencePipelineBuilder<ProductDto>()
    .AddFallback(new FallbackStrategyOptions<ProductDto>
    {
        FallbackAction = args =>
        {
            // Trả về sản phẩm mặc định từ bộ nhớ đệm In-Memory khi tất cả cuộc gọi đều thất bại
            var cached = new ProductDto("prod-default", "Sản phẩm đang cập nhật", 0);
            return Outcome.FromResultAsValueTask(cached);
        }
    })
    .AddHedging(new HedgingStrategyOptions<ProductDto>
    {
        MaxHedgedAttempts = 2,
        Delay = TimeSpan.FromMilliseconds(700), // Nếu sau 700ms chưa có kết quả, tự động bắn thêm request thứ 2
        ShouldHandle = new PredicateBuilder<ProductDto>().Handle<Exception>()
    })
    .Build();

record ProductDto(string Id, string Name, decimal Price);
""",
                    CalloutType = "tip",
                    CalloutTitle = "Hedging Is Only For Idempotent Calls",
                    CalloutTitleVi = "Hedging Chỉ Áp Dụng Cho Thao Tác Đọc (Idempotent)",
                    CalloutText = "Never use Hedging on POST or mutating requests (like 'ChargeCreditCard' or 'CreateOrder'), as it could result in charging the customer twice. Restrict Hedging strictly to GET, idempotent, and side-effect-free operations.",
                    CalloutTextVi = "Tuyệt đối không dùng Hedging cho các request POST làm thay đổi trạng thái (như Trừ tiền thẻ hay Đặt hàng), vì nó có thể khiến khách hàng bị trừ tiền hai lần. Chỉ dùng Hedging cho các thao tác GET đọc dữ liệu thuần túy."
                }
            }
        };
        series.Articles.Add(article3);

        // BÀI 4
        var article4 = new DevArticle
        {
            Id = "dotnet-microservices-article-4-masstransit-outbox-event-driven",
            SeriesId = series.Id,
            LessonNumber = "Bài 4",
            Level = "Advanced",
            LevelVi = "Nâng cao",
            LevelOrder = 3,
            Title = "MassTransit & Transactional Outbox Pattern: Event-Driven Microservices Without Data Loss",
            TitleVi = "MassTransit & Transactional Outbox: Microservices Hướng Sự Kiện Không Sợ Mất Dữ Liệu",
            ReadingTime = "19 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Solve the dreaded Dual-Write problem in distributed architectures. Learn how MassTransit with Entity Framework Core implements the Transactional Outbox Pattern over RabbitMQ/Kafka: Atomic database-and-event commits, Idempotent Consumers to prevent duplicate processing, and Dead Letter Queues (DLQ) for self-healing message pipelines.",
            SummaryVi = "Giải quyết triệt để vấn nạn ghi kép (Dual-Write Problem) trong kiến trúc phân tán. Tìm hiểu cách MassTransit kết hợp Entity Framework Core hiện thực mẫu thiết kế Transactional Outbox trên nền RabbitMQ/Kafka: Cam kết đồng thời đơn hàng và event trong một transaction duy nhất, Idempotent Consumer chống trùng lặp dữ liệu, và Dead Letter Queue (DLQ) tự phục hồi lỗi.",
            Tags = new List<string> { "MassTransit", "RabbitMQ", "Kafka", "Outbox Pattern", "Event-Driven", "Idempotency" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Dual-Write Trap: Why DB + Message Broker Commits Secretly Fail",
                    HeadingVi = "1. Cạm Bẫy Ghi Kép (Dual-Write): Vì Sao Vừa Lưu DB Vừa Bắn Message Thường Xuyên Lỗi",
                    Paragraphs = new List<string>
                    {
                        "In modern event-driven microservices, services communicate asynchronously via messages. For instance, when an order is submitted, the Order Service must persist the order in its database and publish an 'OrderCreatedEvent' to RabbitMQ or Kafka so that Payment and Email services can react.",
                        "Almost every developer initially writes code like this: save to database via DbContext.SaveChangesAsync(), then call messageBus.PublishAsync(event). This is known as the Dual-Write Problem, and it is mathematically broken.",
                        "What happens if the database commit succeeds, but the pod crashes, the network disconnects, or RabbitMQ is temporarily unreachable right before PublishAsync()? The order exists in the database, but the event is lost forever! The customer was charged, but the warehouse never ships the item.",
                        "Conversely, if you publish the event first and the database transaction rolls back due to a constraint violation, other microservices process a ghost order that does not exist in the database!"
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong kiến trúc Microservices hướng sự kiện (Event-Driven), các dịch vụ giao tiếp bất đồng bộ qua thông điệp. Ví dụ khi người dùng tạo đơn hàng, Order Service vừa phải lưu đơn vào database của mình, vừa phải phát sự kiện 'OrderCreatedEvent' lên RabbitMQ hoặc Kafka để Payment Service và Email Service cùng xử lý.",
                        "Hầu hết lập trình viên lúc đầu đều viết theo bản năng: gọi DbContext.SaveChangesAsync() để lưu vào cơ sở dữ liệu, sau đó gọi messageBus.PublishAsync(event). Đây chính là cạm bẫy Ghi Kép (Dual-Write Problem) kinh điển trong hệ thống phân tán.",
                        "Điều gì xảy ra nếu đơn hàng vừa lưu vào DB xong thì Pod bất ngờ bị restart, sập nguồn, hoặc kết nối tới RabbitMQ bị chập chờn ngay trước dòng PublishAsync()? Đơn hàng đã nằm trong DB nhưng sự kiện bị bốc hơi vĩnh viễn! Khách hàng đã thanh toán nhưng kho hàng không hề nhận được thông báo để xuất kho.",
                        "Ngược lại, nếu bạn bắn event lên RabbitMQ trước rồi mới lưu DB, và câu lệnh DB bị lỗi rollback do vi phạm khóa ngoại, các microservice khác sẽ xử lý một đơn hàng ma không hề tồn tại trong cơ sở dữ liệu!"
                    },
                    CodeTitle = "Naive Dual-Write Bug vs Transactional Outbox Solution",
                    CodeLanguage = "csharp",
                    CodeContent = """
// ==================== CÁCH VIẾT SAI KINH ĐIỂN (DUAL-WRITE BUG) ====================
public async Task CreateOrderNaive(Order order)
{
    _dbContext.Orders.Add(order);
    await _dbContext.SaveChangesAsync(); // Bước 1: Lưu thành công vào SQL
    
    // NGUY HIỂM: Nếu Pod bị kill tại đây hoặc mạng RabbitMQ chập chờn -> Event mất vĩnh viễn!
    await _bus.Publish(new OrderCreatedEvent(order.Id, order.Total)); // Bước 2
}

// ==================== GIẢI PHÁP TRANSACTIONAL OUTBOX ====================
// Đơn hàng VÀ Sự kiện cùng được ghi vào CÙNG MỘT DATABASE TRANSACTION!
// Nếu thành công -> Cả 2 cùng lưu. Nếu lỗi -> Cả 2 cùng rollback.
// Một Worker nền ngầm sẽ đọc bảng Outbox để đẩy lên RabbitMQ đảm bảo 100% không mất tin nhắn.
""",
                    CalloutType = "warning",
                    CalloutTitle = "2-Phase Commit (2PC) Is An Anti-Pattern",
                    CalloutTitleVi = "Không Được Dùng 2-Phase Commit (2PC) Phân Tán",
                    CalloutText = "Do not try to solve this with distributed transactions (like MSDTC or 2PC). Distributed locking kills performance, creates tight coupling, and is completely unsupported in cloud-native containerized architectures.",
                    CalloutTextVi = "Đừng cố giải quyết bằng giao dịch phân tán 2-Phase Commit (như MSDTC). Việc khóa tài nguyên phân tán sẽ bóp nghẹt hiệu năng, gây phụ thuộc chặt chẽ và không thể hoạt động được trong môi trường container đám mây."
                },
                new DevArticleSection
                {
                    Heading = "2. Configuring MassTransit with RabbitMQ & EF Core Outbox",
                    HeadingVi = "2. Cấu Hình MassTransit Kết Hợp RabbitMQ & EF Core Outbox Chuẩn Chỉ",
                    Paragraphs = new List<string>
                    {
                        "MassTransit is the most advanced, enterprise-proven message bus framework for .NET. It abstracts messaging topologies across RabbitMQ, Azure Service Bus, Amazon SQS, and Apache Kafka.",
                        "Most crucially, MassTransit includes native, battle-tested support for the Transactional Outbox Pattern integrated directly with Entity Framework Core.",
                        "When you call IPublishEndpoint.Publish() within an EF Core transaction, MassTransit does not talk to the network immediately. Instead, it serializes the event into an 'OutboxMessages' table within the exact same SQL transaction. An integrated background worker pulls committed outbox messages and reliably dispatches them to RabbitMQ."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "MassTransit là framework hướng sự kiện và Message Bus mạnh mẽ nhất cho hệ sinh thái .NET. Nó trừu tượng hóa toàn bộ kiến trúc kết nối trên RabbitMQ, Azure Service Bus, Amazon SQS và Apache Kafka.",
                        "Đặc biệt nhất, MassTransit tích hợp sẵn mẫu thiết kế Transactional Outbox Pattern chuẩn mực kết hợp trực tiếp với Entity Framework Core.",
                        "Khi bạn gọi IPublishEndpoint.Publish() bên trong một transaction của EF Core, MassTransit không hề gửi qua mạng ngay. Thay vào đó, nó lưu sự kiện vào bảng 'OutboxMessages' trong cùng một SQL Transaction với đơn hàng. Một tiến trình chạy ngầm (Outbox Delivery Service) sẽ định kỳ quét các tin nhắn trong bảng này và đẩy lên RabbitMQ với độ tin cậy tuyệt đối."
                    },
                    CodeTitle = "MassTransit Setup with RabbitMQ and EF Core Outbox in Program.cs",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Nuget:
// dotnet add package MassTransit
// dotnet add package MassTransit.RabbitMQ
// dotnet add package MassTransit.EntityFrameworkCore

using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Cấu hình MassTransit với Transactional Outbox
builder.Services.AddMassTransit(x =>
{
    // Đăng ký Consumer nhận message
    x.AddConsumer<PaymentCompletedConsumer>();

    // CẤU HÌNH EF CORE TRANSACTIONAL OUTBOX
    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UsePostgres(); // Hỗ trợ Postgres, SqlServer, MySql, Sqlite...
        o.UseBusOutbox(); // Tự động chặn các lệnh publish để lưu vào bảng Outbox
        o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30); // Chống gửi đúp message trong 30 phút
    });

    // Cấu hình kết nối tới RabbitMQ Container
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:User"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Pass"] ?? "guest");
        });

        // Tự động cấu hình endpoint nhận message cho các Consumer đã đăng ký
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapPost("/api/orders/checkout", async (CheckoutRequest request, OrderDbContext db, IPublishEndpoint publishEndpoint) =>
{
    // Tạo đơn hàng
    var order = new Order { Id = Guid.NewGuid(), CustomerId = request.CustomerId, Total = request.Total };
    db.Orders.Add(order);

    // Bắn event qua MassTransit: Event sẽ tự động lưu vào bảng Outbox trong cùng DbContext!
    await publishEndpoint.Publish(new OrderCreatedEvent(order.Id, order.CustomerId, order.Total));

    // LƯU CẢ 2 VÀO POSTGRES TRONG MỘT TRANSACTION DUY NHẤT!
    await db.SaveChangesAsync();

    return Results.Ok(new { orderId = order.Id, status = "OrderCreated" });
});

app.Run();

public record OrderCreatedEvent(Guid OrderId, string CustomerId, decimal Total);
public class Order { public Guid Id { get; set; } public string CustomerId { get; set; } = ""; public decimal Total { get; set; } }
public class OrderDbContext : DbContext {
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) {}
    public DbSet<Order> Orders => Set<Order>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        // Tự động tạo các bảng Outbox: OutboxMessage, OutboxState, InboxState
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Outbox Table Schema Migrations",
                    CalloutTitleVi = "Tự Động Tạo Bảng Outbox Bằng EF Migrations",
                    CalloutText = "Calling modelBuilder.AddOutboxMessageEntity() enables EF Core to automatically create the necessary Outbox tables when you run 'dotnet ef migrations add AddOutbox'. No manual SQL script authoring is needed.",
                    CalloutTextVi = "Lệnh modelBuilder.AddOutboxMessageEntity() giúp EF Core tự động sinh cấu trúc bảng Outbox khi bạn chạy 'dotnet ef migrations add AddOutbox'. Bạn không cần phải viết tay bất kỳ dòng lệnh SQL nào."
                },
                new DevArticleSection
                {
                    Heading = "3. Idempotent Consumer & Dead Letter Queue (DLQ) Handling",
                    HeadingVi = "3. Người Nhận Bất Biến (Idempotent Consumer) & Xử Lý Tin Nhắn Độc Với Dead Letter Queue",
                    Paragraphs = new List<string>
                    {
                        "In distributed messaging, the gold standard guarantee is 'At-Least-Once Delivery'. This means RabbitMQ guarantees your message will arrive, but in rare network reconnections, a message might arrive twice.",
                        "If your Payment Service charges a credit card upon receiving 'OrderCreatedEvent', processing that message twice would cause a double charge! Therefore, every Consumer in a microservice must be Idempotent (processing the same message twice produces the exact same outcome).",
                        "MassTransit provides an Inbox State pattern that tracks processed MessageIds. Furthermore, if a poisoned message persistently throws unhandled exceptions, MassTransit routes it to an '_error' Dead Letter Queue (DLQ) after configured retries, preventing the queue from being blocked."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong kiến trúc hướng sự kiện phân tán, tiêu chuẩn truyền tải thực tế luôn là 'At-Least-Once Delivery' (Giao ít nhất một lần). Điều đó có nghĩa là Message Broker cam kết tin nhắn sẽ đến đích, nhưng khi có sự cố chập chờn mạng, một tin nhắn có thể bị gửi lặp lại 2 lần.",
                        "Nếu Payment Service thực hiện trừ tiền thẻ tín dụng khi nhận sự kiện 'OrderCreatedEvent', việc xử lý lặp lại tin nhắn này sẽ khiến khách hàng bị trừ tiền 2 lần! Do đó, mọi Consumer trong microservice bắt buộc phải đạt tính chất Bất Biến (Idempotent: xử lý một tin nhắn nhiều lần cũng chỉ tạo ra một kết quả duy nhất).",
                        "MassTransit cung cấp sẵn cơ chế Inbox State tự động kiểm tra MessageId đã từng xử lý chưa để bỏ qua tin nhắn trùng lặp. Đồng thời, nếu gặp tin nhắn độc hại liên tục văng lỗi, MassTransit sẽ tự động chuyển tin nhắn đó sang hàng đợi lỗi Dead Letter Queue ('_error') sau khi đã thử lại đủ số lần, giữ cho luồng thông điệp chính luôn thông suốt."
                    },
                    CodeTitle = "Idempotent Consumer Implementation with Retry and Dead Letter Routing",
                    CodeLanguage = "csharp",
                    CodeContent = """
public class PaymentCompletedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<PaymentCompletedConsumer> _logger;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentCompletedConsumer(ILogger<PaymentCompletedConsumer> logger, IPaymentGateway paymentGateway)
    {
        _logger = logger;
        _paymentGateway = paymentGateway;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Đang xử lý thanh toán cho đơn hàng {OrderId}, MessageId: {MessageId}", 
            message.OrderId, context.MessageId);

        // Giả lập thanh toán qua cổng ngân hàng
        var isSuccess = await _paymentGateway.ProcessPaymentAsync(message.OrderId, message.Total);

        if (!isSuccess)
        {
            _logger.LogError("Thanh toán thất bại cho đơn hàng {OrderId}. Ném ngoại lệ để kích hoạt retry...", message.OrderId);
            throw new PaymentFailedException($"Cổng thanh toán từ chối đơn hàng {message.OrderId}");
            // Khi ném ngoại lệ: MassTransit sẽ retry theo cấu hình.
            // Nếu retry 3 lần vẫn lỗi -> Tự động đưa message vào 'order-created_error' (Dead Letter Queue)!
        }

        _logger.LogInformation("Thanh toán thành công mỹ mãn cho đơn hàng {OrderId}!", message.OrderId);
    }
}

public interface IPaymentGateway { Task<bool> ProcessPaymentAsync(Guid orderId, decimal amount); }
public class PaymentFailedException : Exception { public PaymentFailedException(string msg) : base(msg) {} }
""",
                    CalloutType = "tip",
                    CalloutTitle = "Zero-Stuck Queues with DLQ",
                    CalloutTitleVi = "Hàng Đợi Không Bao Giờ Bị Tắc Nhờ DLQ",
                    CalloutText = "Dead Letter Queues isolate problematic messages into a separate queue without halting the processing of healthy messages. Engineers can inspect the payload in RabbitMQ UI, fix the root bug, and click 'Move messages' to replay them back into the main pipeline.",
                    CalloutTextVi = "Dead Letter Queue giúp cô lập các tin nhắn bị lỗi sang một hàng đợi riêng biệt mà không làm tắc nghẽn các tin nhắn khỏe mạnh phía sau. Kỹ sư có thể vào giao diện RabbitMQ xem nội dung tin nhắn lỗi, sửa bug và bấm nút 'Move' để bắn lại vào hệ thống xử lý bình thường."
                }
            }
        };
        series.Articles.Add(article4);

        // BÀI 5
        var article5 = new DevArticle
        {
            Id = "dotnet-microservices-article-5-opentelemetry-tracing-and-k8s-config",
            SeriesId = series.Id,
            LessonNumber = "Bài 5",
            Level = "Production",
            LevelVi = "Thực chiến",
            LevelOrder = 4,
            Title = "Distributed Tracing & Cloud-Native Observability: OpenTelemetry, W3C Trace Context & K8s ConfigMaps",
            TitleVi = "Distributed Tracing & Khả Năng Giám Sát Phân Tán: OpenTelemetry, W3C Trace Context & K8s ConfigMaps",
            ReadingTime = "20 phút đọc",
            PublishedDate = "24/10/2026",
            Summary = "Track end-to-end user transactions across 20 containers without missing a beat. Implement OpenTelemetry (OTel) in .NET: W3C traceparent header propagation across HTTP & RabbitMQ, exporting telemetry to Seq / Jaeger / Prometheus / Aspire Dashboard, and hot-reloading Kubernetes ConfigMaps without restarting containers using KeyPerFile.",
            SummaryVi = "Theo dõi hành trình của một request từ cổng Ingress xuyên qua 20 container dịch vụ mà không đứt gãy. Triển khai OpenTelemetry (OTel) trong .NET: Cơ chế lan truyền W3C traceparent header qua HTTP & RabbitMQ, xuất dữ liệu Trace/Metrics về Seq / Jaeger / Prometheus / .NET Aspire Dashboard, và kỹ thuật nạp lại cấu hình ConfigMap động không cần restart Pod bằng KeyPerFile.",
            Tags = new List<string> { "OpenTelemetry", "Distributed Tracing", "Jaeger", "Prometheus", "ConfigMaps", "Aspire Dashboard" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Mystery of Distributed Latency Spikes: Why Server Metrics Fall Short",
                    HeadingVi = "1. Bí Ẩn Tắc Nghẽn Phân Tán: Vì Sao Chỉ Đo CPU/RAM Là Hoàn Toàn Bất Lực",
                    Paragraphs = new List<string>
                    {
                        "When an end-user clicks 'Place Order' on your website and the spinning wheel spins for 8 seconds, looking at individual CPU or RAM utilization graphs in Kubernetes will not tell you which component caused the delay.",
                        "That single button click traversed an Ingress Gateway, routed to the Order Service, validated user claims against Identity Service, verified product inventory in Inventory Service, issued a SQL command, and published an event.",
                        "Which specific link in that chain took 7.5 seconds? Was it a slow SQL query in Inventory, or network latency between clusters? The only way to answer this is Distributed Tracing with OpenTelemetry (OTel), stitching together every asynchronous span using W3C Trace Context headers ('traceparent')."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khi một người dùng bấm nút 'Đặt hàng' trên trang web và vòng xoay tải dữ liệu quay tít mù suốt 8 giây, việc nhìn vào biểu đồ tiêu thụ CPU hay RAM của từng Pod trong Kubernetes hoàn toàn không thể chỉ ra được mắt xích nào đang gây nghẽn cổ chai.",
                        "Một cú click chuột đó đã đi qua Ingress Gateway, chuyển tiếp đến Order Service, gọi sang Identity Service để kiểm tra quyền, truy vấn Inventory Service để kiểm tra tồn kho, thực thi một câu lệnh SQL và bắn event qua hàng đợi.",
                        "Mắt xích cụ thể nào trong chuỗi mắt xích trên đã ngốn mất 7.5 giây? Do câu lệnh SQL bị thiếu index ở Inventory, hay do đường truyền mạng nội bộ giữa các cụm? Câu trả lời duy nhất nằm ở Kỹ Thuật Truy Vết Phân Tán (Distributed Tracing) với chuẩn OpenTelemetry (OTel), gắn kết từng Span xử lý thông qua header W3C 'traceparent'."
                    },
                    CodeTitle = "W3C Traceparent Header Anatomy & Trace Hierarchy",
                    CodeLanguage = "text",
                    CodeContent = """
HTTP Header: traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
                          |  |                                |                |
                       Version TraceId (Định danh toàn chuỗi)  Parent SpanId    Flags (01 = Sampled)

[Ingress Gateway] -------------------- (Tổng thời gian: 8.2s) --------------------> [TraceId: 4bf9...]
       |
       +---> [Order Service HTTP POST] (Mất: 8.1s)
                  |
                  +---> [Identity Check] (Mất: 45ms)
                  |
                  +---> [Inventory Service HTTP GET] (Mất: 7.8s)  <==== THỦ PHẠM NẰM Ở ĐÂY!
                             |
                             +---> [SQL Query: SELECT * FROM Products] (Mất: 7.75s - THIẾU INDEX!)
""",
                    CalloutType = "info",
                    CalloutTitle = "W3C Trace Context Standard",
                    CalloutTitleVi = "Chuẩn Quốc Tế W3C Trace Context",
                    CalloutText = "The W3C Trace Context specification ensures that whether your services are written in .NET, Go, Java, or Node.js, the 'traceparent' header is transparently understood and propagated across process boundaries and messaging brokers.",
                    CalloutTextVi = "Chuẩn quốc tế W3C Trace Context đảm bảo rằng dù các microservice của bạn viết bằng .NET, Go, Java hay Node.js, header 'traceparent' vẫn được nhận diện và truyền tải xuyên suốt qua các ranh giới tiến trình và message broker."
                },
                new DevArticleSection
                {
                    Heading = "2. Configuring OpenTelemetry in ASP.NET Core: Traces, Metrics & OTLP",
                    HeadingVi = "2. Cấu Hình OpenTelemetry Toàn Diện: Traces, Metrics & Xuất Ra OTLP Exporter",
                    Paragraphs = new List<string>
                    {
                        "OpenTelemetry is the vendor-neutral cloud-native observability standard governed by the CNCF (Cloud Native Computing Foundation). In .NET, OpenTelemetry provides seamless auto-instrumentation for incoming HTTP requests, outgoing HttpClient calls, Entity Framework Core queries, and MassTransit messages.",
                        "Using the OTLP (OpenTelemetry Protocol) exporter, telemetry is exported over gRPC to any backend of your choice: modern Seq (which natively supports OTel traces in recent versions), Jaeger, Prometheus, or the phenomenal new .NET Aspire Dashboard.",
                        "Auto-instrumentation means you rarely need to write manual span creation code; .NET automatically records endpoint routes, HTTP status codes, SQL query parameters, and unhandled exceptions."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "OpenTelemetry là tiêu chuẩn quan sát đám mây phi lợi nhuận do tổ chức CNCF (Cloud Native Computing Foundation) quản lý. Trong .NET, OpenTelemetry hỗ trợ tự động thu thập (Auto-instrumentation) mọi request HTTP gửi đến, các cuộc gọi HttpClient ra ngoài, câu lệnh truy vấn Entity Framework Core và thông điệp qua MassTransit.",
                        "Thông qua giao thức chuẩn OTLP (OpenTelemetry Protocol), dữ liệu quan sát được xuất qua gRPC về bất kỳ hệ thống nào: Seq thế hệ mới (hỗ trợ hiển thị OTel Traces trực quan), Jaeger, Prometheus hoặc bảng điều khiển .NET Aspire Dashboard cực kỳ hiện đại.",
                        "Nhờ cơ chế tự động hóa, bạn hầu như không cần phải viết code tạo Span thủ công; .NET tự động ghi lại URL endpoint, mã lỗi HTTP, thời gian thực thi câu lệnh SQL và toàn bộ dấu vết ngoại lệ."
                    },
                    CodeTitle = "OpenTelemetry Setup with Traces, Metrics & OTLP Exporter in Program.cs",
                    CodeLanguage = "csharp",
                    CodeContent = """
// Nuget:
// dotnet add package OpenTelemetry.Extensions.Hosting
// dotnet add package OpenTelemetry.Instrumentation.AspNetCore
// dotnet add package OpenTelemetry.Instrumentation.Http
// dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
// dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "OrderService";

// CẤU HÌNH TOÀN DIỆN OPENTELEMETRY TRACES & METRICS
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: "1.0.0")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["k8s.pod.name"] = Environment.MachineName
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(opts => opts.RecordException = true) // Tự động đo lường request API
        .AddHttpClientInstrumentation() // Tự động đo lường và đính kèm traceparent khi gọi service khác
        .AddEntityFrameworkCoreInstrumentation(opts => opts.SetDbStatementForText = true) // Tự động đo lường SQL
        .AddSource("MassTransit") // Tự động đo lường message qua RabbitMQ
        .AddOtlpExporter(opts =>
        {
            // Trỏ về OTel Collector, Seq OTLP endpoint (port 5341), hoặc .NET Aspire Dashboard (port 4317)
            opts.Endpoint = new Uri(builder.Configuration["OTel:Endpoint"] ?? "http://aspire-dashboard:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation() // Đo lường bộ nhớ GC, ThreadPool, CPU usage của .NET runtime
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(builder.Configuration["OTel:Endpoint"] ?? "http://aspire-dashboard:4317");
        }));

var app = builder.Build();

app.MapGet("/api/checkout", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient();
    // Header W3C 'traceparent' sẽ được HttpClient tự động sinh và đính kèm hoàn toàn ngầm định!
    var paymentResponse = await client.GetAsync("http://payment-service:8080/api/pay");
    return Results.Ok(new { status = "CheckedOut" });
});

app.Run();
""",
                    CalloutType = "tip",
                    CalloutTitle = "Experience .NET Aspire Dashboard",
                    CalloutTitleVi = "Trải Nghiệm .NET Aspire Dashboard",
                    CalloutText = "You can run the official .NET Aspire Dashboard standalone in Docker: 'docker run -p 18888:18888 -p 4317:4317 mcr.microsoft.com/dotnet/aspire-dashboard:latest'. It provides the slickest, most intuitive distributed trace and structured log visualization interface ever created for .NET developers.",
                    CalloutTextVi = "Bạn có thể chạy bảng điều khiển .NET Aspire Dashboard độc lập qua Docker: 'docker run -p 18888:18888 -p 4317:4317 mcr.microsoft.com/dotnet/aspire-dashboard:latest'. Đây là giao diện xem Trace phân tán và Log có cấu trúc đẹp mắt và mượt mà nhất từng được tạo ra cho lập trình viên .NET."
                },
                new DevArticleSection
                {
                    Heading = "3. Dynamic Kubernetes ConfigMap & Secret Reloads with KeyPerFile",
                    HeadingVi = "3. Nạp Lại Cấu Hình Động K8s ConfigMaps & Secrets Bằng KeyPerFile Không Cần Restart Pod",
                    Paragraphs = new List<string>
                    {
                        "In production Kubernetes, configuration settings (endpoints, feature flags, thresholds) and sensitive secrets (connection strings, API keys) are managed in ConfigMaps and Secrets, mounted as volume files inside container directories.",
                        "When an engineer updates a ConfigMap via 'kubectl apply', Kubernetes updates the mounted files inside the container. However, by default, ASP.NET Core only reads configuration once at startup; without dynamic reloading, you must restart your pods, causing unnecessary downtime and cache invalidation.",
                        "The built-in package 'Microsoft.Extensions.Configuration.KeyPerFile' solves this gracefully. It watches the mounted directory for file changes and automatically updates 'IOptionsMonitor<T>' or 'IConfiguration' in real time, enabling zero-downtime hot-reloads of microservice configurations."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trên môi trường Kubernetes thực tế, các biến cấu hình (địa chỉ service, cờ bật tắt tính năng, ngưỡng chịu lỗi) và thông tin mật (chuỗi kết nối DB, API Key) được quản lý qua ConfigMap và Secret, rồi gắn (mount) dưới dạng thư mục file vào trong container.",
                        "Khi kỹ sư cập nhật ConfigMap qua lệnh 'kubectl apply', Kubernetes sẽ tự động cập nhật nội dung các file bên trong container. Tuy nhiên, mặc định ASP.NET Core chỉ đọc cấu hình một lần lúc khởi động; nếu không có cơ chế nạp lại động, bạn bắt buộc phải restart Pod, gây gián đoạn dịch vụ và xóa sạch bộ nhớ đệm cache.",
                        "Gói chính chủ 'Microsoft.Extensions.Configuration.KeyPerFile' giải quyết bài toán này một cách duyên dáng. Nó liên tục theo dõi thư mục mounted và tự động cập nhật giá trị trong 'IOptionsMonitor<T>' hoặc 'IConfiguration' theo thời gian thực, giúp thay đổi cấu hình nóng (Hot-reload) mà không cần chạm vào tiến trình Pod."
                    },
                    CodeTitle = "Adding KeyPerFile Configuration Provider & K8s Volume Mount YAML",
                    CodeLanguage = "csharp",
                    CodeContent = """
// 1. CẤU HÌNH TRONG C# PROGRAM.CS
var builder = WebApplication.CreateBuilder(args);

// Đọc thư mục ConfigMap được Kubernetes mount vào container
var configMapPath = "/etc/config/app";
if (Directory.Exists(configMapPath))
{
    builder.Configuration.AddKeyPerFile(directoryPath: configMapPath, optional: true, reloadOnChange: true);
}

// Khi inject IOptionsMonitor<PaymentSettings>, settings.CurrentValue sẽ tự động cập nhật ngay khi file đổi!

// ==================== KUBERNETES DEPLOYMENT MOUNT YAML ====================
// spec:
//   containers:
//   - name: order-api
//     volumeMounts:
//     - name: config-volume
//       mountPath: /etc/config/app
//   volumes:
//   - name: config-volume
//     configMap:
//       name: order-service-config
""",
                    CalloutType = "tip",
                    CalloutTitle = "Zero-Downtime Dynamic Configuration",
                    CalloutTitleVi = "Cấu Hình Động Không Downtime",
                    CalloutText = "By combining reloadOnChange: true with IOptionsMonitor<T>, you can tune circuit breaker failure thresholds or adjust logging levels from Information to Debug in live production pods without a single millisecond of downtime.",
                    CalloutTextVi = "Bằng cách kết hợp reloadOnChange: true với IOptionsMonitor<T>, bạn có thể điều chỉnh ngưỡng chịu lỗi của Circuit Breaker hoặc nâng log level từ Information lên Debug trên cụm Production đang chạy mà không gây ra bất kỳ một mili-giây downtime nào."
                }
            }
        };
        series.Articles.Add(article5);

        return series;
    }

    [Fact]
    public void TestMicroservicesSeriesExistsAndValid()
    {
        var path = Path.GetFullPath("../../../../../wwwroot/sample-data/dev-learning.json");
        Assert.True(File.Exists(path), $"File not found at: {path}");

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var data = JsonSerializer.Deserialize<DevBlogData>(json, options);
        Assert.NotNull(data);

        var series = data.SeriesList.FirstOrDefault(s => s.Id == "dotnet-microservices-docker-k8s-ecosystem-series");
        Assert.NotNull(series);
        Assert.Equal("devops", series.Category);
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
