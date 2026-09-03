namespace BlazorWasmPortfolioGhAction.Data;

public record Profile(
    LocalizedText Name,
    string FullName,
    LocalizedText Role,
    string[] PhoneNumbers,
    string[] Emails,
    string FacebookUrl,
    string LinkedInUrl,
    string TelegramHandle,
    string GitHubUrl,
    string PdfResumeUrl,
    string CertificateUrl,
    string TranscriptUrl,
    LocalizedParagraphs About
);

public record ExperienceEntry(
    string Title,
    string Company,
    string Role,
    LocalizedText Period,
    LocalizedText Description,
    string[] TechStack,
    string? DetailLink
);

public record ProjectEntry(
    string Type,
    int Number,
    string Name,
    LocalizedText Description,
    string RepoUrl,
    string? DetailLink,
    string[] TechStack
);

public record EducationEntry(
    string Period,
    LocalizedText Name,
    LocalizedText Program,
    string Url,
    string Gpa,
    string DocumentUrl,
    LocalizedText DocumentLabel
);

public record SkillEntry(
    string Name,
    int Level
);

public record SkillCategory(
    LocalizedText Name,
    bool UseMeters,
    SkillEntry[]? Skills,
    string[]? Tags
);

public record ToeicScore(
    int Score,
    int Year
);

public record CourseEntry(
    string Code,
    LocalizedText Title
);

public record Resume(
    Profile Profile,
    EducationEntry[] Education,
    ExperienceEntry[] Experience,
    ProjectEntry[] Projects,
    SkillCategory[] Skills,
    ToeicScore[] ToeicScores,
    CourseEntry[] Courses
);

public static class ResumeData
{
    public static Resume Current => new(
        Profile: new Profile(
            Name: new LocalizedText(Vi: "Khoi Duc", En: "Khoi Duc"),
            FullName: "Nguyen Minh Duc Khoi",
            Role: new LocalizedText(Vi: "Lập trình viên", En: "Developer"),
            PhoneNumbers: new[] { "+84938751116", "+84384223897" },
            Emails: new[] { "khoi.duc.dev@gmail.com", "khoimessi99@gmail.com" },
            FacebookUrl: "https://www.facebook.com/messi.khoi.9",
            LinkedInUrl: "https://www.linkedin.com/in/%C4%91%E1%BB%A9c-kh%C3%B4i-354b77269/",
            TelegramHandle: "warman99",
            GitHubUrl: "https://github.com/KhoiDuc",
            PdfResumeUrl: "files/resume.pdf",
            CertificateUrl: "https://drive.google.com/file/d/17NgDYao2f84W4U346hyuSWZkDIkg612e/view?usp=sharing",
            TranscriptUrl: "files/transcript.pdf",
            About: new LocalizedParagraphs(
                Vi: new[] {
                    "Tôi là Nguyễn Minh Đức Khôi, một lập trình viên phần mềm đam mê và tận tâm, chuyên về các công nghệ .NET, với trọng tâm xây dựng các giải pháp có khả năng mở rộng, dễ bảo trì và giải quyết các vấn đề thực tế thông qua mã lệnh.",
                    "Là một Lập trình viên Backend có kinh nghiệm 5 năm với C# và JavaScript, thành thạo trong việc xây dựng các ứng dụng ASP.NET Core hiệu suất cao, có khả năng mở rộng và tối ưu hóa truy vấn cơ sở dữ liệu (MySQL, PostgreSQL, Elasticsearch, SQL Server). Với kỹ năng phân tích mạnh mẽ và kiến thức thực tiễn về các mẫu thiết kế kiến trúc, tôi có kinh nghiệm thiết kế và phát triển các ứng dụng web/API tuân thủ WCAG 2.1 sử dụng .NET MVC và .NET Core."
                },
                En: new[] {
                    "I am Nguyen Minh Duc Khoi, a passionate and dedicated software developer specializing in .NET technologies, with a strong focus on building scalable, maintainable solutions and solving real-world problems through code.",
                    "A highly skilled Backend Developer with 5 years of experience in C# and JavaScript, proficient in building high-performance, scalable ASP.NET Core applications and optimizing database queries (MySQL, PostgreSQL, Elasticsearch, SQL Server). Possessing strong analytical skills and practical knowledge of architecture design patterns, this developer has experience designing and developing WCAG 2.1 compliant web/API applications using .NET MVC and .NET Core, with domain expertise in Data Security, E-Commerce & Retail, and Human Resource Management."
                }
            )
        ),
        Education: new[]
        {
            new EducationEntry(
                Period: "2017 — 2021",
                Name: new LocalizedText(Vi: "Đại học HUFLIT (ĐÃ TỐT NGHIỆP)", En: "HUFLIT University (GRADUATED)"),
                Program: new LocalizedText(Vi: "Kỹ thuật Phần mềm", En: "Software Engineering"),
                Url: "https://huflit.edu.vn/",
                Gpa: "GPA: 3.33/4.0 (8.32/10)",
                DocumentUrl: "https://drive.google.com/file/d/17NgDYao2f84W4U346hyuSWZkDIkg612e/view?usp=sharing",
                DocumentLabel: new LocalizedText(Vi: "Xem Tài liệu", En: "View Document")
            )
        },
        Experience: new[]
        {
            new ExperienceEntry(
                Title: "Site Manager Plus Plus (SM++)",
                Company: "S3Corp",
                Role: "Full Stack Developer",
                Period: new LocalizedText(Vi: "2023 — Hiện tại", En: "2023 — Now"),
                Description: new LocalizedText(
                    Vi: "Phiên bản nâng cấp của Site Manager chạy trên .NET 10. Nền tảng thiết lập và quản lý trang web nhanh chóng với kiến trúc trang mô-đun. Hỗ trợ tích hợp SSO với các nhà cung cấp danh tính bên thứ ba bao gồm Citi và Wells Fargo.",
                    En: "Upgraded version of Site Manager running on .NET 10. Platform for rapid website setup and management with modular page architecture. Supports SSO integration with third-party identity providers including Citi and Wells Fargo."
                ),
                TechStack: new[] { ".NET 10 MVC", "jQuery", "EF", "SQL Server", "Redis", "Azure CDN", "Kontent.AI", "WCAG 2.2" },
                DetailLink: "/project/smpp"
            ),
            new ExperienceEntry(
                Title: "Site Manager",
                Company: "S3Corp",
                Role: "Full Stack Developer",
                Period: new LocalizedText(Vi: "2022 — 2024", En: "2022 — 2024"),
                Description: new LocalizedText(
                    Vi: "Nền tảng tạo và triển khai trang web nhanh chóng. Cho phép người dùng tìm kiếm, khám phá và đặt dịch vụ (ăn uống, khách sạn, chuyến bay, spa, xe limousine). Xây dựng tuân thủ WCAG 2.1, PCI DSS và quét bảo mật Veracode.",
                    En: "Platform for creating and deploying websites quickly. Enables users to search, explore, and book services (dining, hotels, flights, spas, limousines). Built to comply with WCAG 2.1, PCI DSS, and Veracode security scanning."
                ),
                TechStack: new[] { ".NET 4 MVC", "jQuery", "EF", "SQL Server", "Redis", "Azure CDN", "Kontent.AI", "WCAG 2.1" },
                DetailLink: "/project/sm"
            ),
            new ExperienceEntry(
                Title: "Payment Portal",
                Company: "S3Corp",
                Role: "Full Stack Developer",
                Period: new LocalizedText(Vi: "2022 — Hiện tại", En: "2022 — Now"),
                Description: new LocalizedText(
                    Vi: "Quản lý giao dịch quốc tế thông qua tích hợp hệ thống thanh toán Adyen. Đang nâng cấp từ .NET MVC lên .NET 10.",
                    En: "Manages international transactions through Adyen payment system integration. Currently upgrading from .NET MVC to .NET 10."
                ),
                TechStack: new[] { ".NET MVC 4", "EF", "SQL Server", "JavaScript", "Webhook" },
                DetailLink: "/project/payment-portal"
            ),
            new ExperienceEntry(
                Title: "Simply Blast",
                Company: "S3Corp",
                Role: "Full Stack Developer",
                Period: new LocalizedText(Vi: "06/2022 — 09/2022", En: "06/2022 — 09/2022"),
                Description: new LocalizedText(
                    Vi: "Ứng dụng web cho khách hàng tạo và quản lý chiến dịch quảng cáo qua tin nhắn WhatsApp.",
                    En: "Web application for clients to create and manage advertising campaigns through WhatsApp messages."
                ),
                TechStack: new[] { ".NET 6", "Blazor", "EF Core", "SQL Server" },
                DetailLink: "/project/simply-blast"
            )
        },
        Projects: new[]
        {
            new ProjectEntry(
                Type: "Personal",
                Number: 1,
                Name: "BlazorWasmPortfolioGhAction",
                Description: new LocalizedText(
                    Vi: "Site cá nhân built bằng Blazor WebAssembly, gồm công cụ DevOps, email composer, và 40+ tool.",
                    En: "Personal portfolio site built with Blazor WebAssembly, featuring DevOps utilities, email composer, and 40+ tools."
                ),
                RepoUrl: "https://github.com/KhoiDuc/BlazorWasmPortfolioGhAction",
                DetailLink: null,
                TechStack: new[] { "Blazor WASM", ".NET 10", "Fluxor", "GitHub Actions" }
            ),
            new ProjectEntry(
                Type: "School",
                Number: 2,
                Name: "Food Ordering System",
                Description: new LocalizedText(
                    Vi: "Đặt đồ ăn full-stack với backend .NET 5 và frontend Flutter.",
                    En: "Full-stack food ordering with .NET 5 backend and Flutter frontend."
                ),
                RepoUrl: "https://github.com/khoinmdIT99/FoodOrder_NET5_FLUTTER",
                DetailLink: "/project/food-ordering",
                TechStack: new[] { ".NET 5", "Flutter" }
            ),
            new ProjectEntry(
                Type: "School",
                Number: 3,
                Name: "Building Material E-Commerce",
                Description: new LocalizedText(
                    Vi: "Nền tảng TMĐT vật liệu xây dựng full-stack với .NET 5 và Angular.",
                    En: "Full-stack e-commerce platform for building materials with .NET 5 backend and Angular frontend."
                ),
                RepoUrl: "https://github.com/khoinmdIT99/BuildingMaterialShop_NET5_ANUGLAR",
                DetailLink: "/project/building-material",
                TechStack: new[] { ".NET 5", "Angular" }
            ),
            new ProjectEntry(
                Type: "School",
                Number: 4,
                Name: "Payroll Management System",
                Description: new LocalizedText(
                    Vi: "Hệ thống quản lý lương doanh nghiệp built với .NET Core MVC.",
                    En: "Enterprise payroll management system built with .NET Core MVC."
                ),
                RepoUrl: "https://github.com/khoinmdIT99/Payroll-Manager/",
                DetailLink: null,
                TechStack: new[] { ".NET Core MVC" }
            )
        },
        Skills: new[]
        {
            new SkillCategory(
                Name: new LocalizedText(Vi: "Ngôn ngữ lập trình", En: "Programming Languages"),
                UseMeters: true,
                Skills: new[]
                {
                    new SkillEntry("C# / .NET", 90),
                    new SkillEntry("JavaScript", 75),
                    new SkillEntry("Java", 60),
                    new SkillEntry("Dart (Flutter)", 55)
                },
                Tags: null
            ),
            new SkillCategory(
                Name: new LocalizedText(Vi: "Frontend", En: "Frontend"),
                UseMeters: false,
                Skills: null,
                Tags: new[] { "HTML5", "CSS3", "jQuery", "Vanilla JS", "React", "Angular", "Blazor" }
            ),
            new SkillCategory(
                Name: new LocalizedText(Vi: "Cơ sở dữ liệu", En: "Databases"),
                UseMeters: true,
                Skills: new[]
                {
                    new SkillEntry("SQL Server", 88),
                    new SkillEntry("MySQL", 80),
                    new SkillEntry("PostgreSQL", 70),
                    new SkillEntry("MongoDB", 65)
                },
                Tags: null
            ),
            new SkillCategory(
                Name: new LocalizedText(Vi: "Framework", En: "Frameworks"),
                UseMeters: false,
                Skills: null,
                Tags: new[] { ".NET Core 6/8/9/10", ".NET Framework 4.8", ".NET MVC", "WPF", "Web API", "WinForms" }
            ),
            new SkillCategory(
                Name: new LocalizedText(Vi: "Công cụ & DevOps", En: "Tools & DevOps"),
                UseMeters: false,
                Skills: null,
                Tags: new[] { "Visual Studio", "VS Code", "ReSharper", "Redis", "Docker", "Azure", "GitHub Actions" }
            ),
            new SkillCategory(
                Name: new LocalizedText(Vi: "Kiến trúc & Thực hành", En: "Architecture & Practices"),
                UseMeters: false,
                Skills: null,
                Tags: new[] { "Microservices", "Monolithic", "Database Design", "WCAG 2.1/2.2", "PCI DSS" }
            ),
            new SkillCategory(
                Name: new LocalizedText(Vi: "Tích hợp thanh toán", En: "Payment Integration"),
                UseMeters: false,
                Skills: null,
                Tags: new[] { "Adyen", "PayPal", "Momo" }
            )
        },
        ToeicScores: new[]
        {
            new ToeicScore(600, 2024),
            new ToeicScore(686, 2023),
            new ToeicScore(595, 2022)
        },
        Courses: new[]
        {
            new CourseEntry("[TEDU-42]", new LocalizedText(Vi: "Làm chủ Docker để chinh phục DevOps", En: "Master Docker to Conquer DevOps")),
            new CourseEntry("[TEDU-47]", new LocalizedText(Vi: "Làm chủ đám mây Azure", En: "Mastering Microsoft Azure Cloud")),
            new CourseEntry("[TEDU-35]", new LocalizedText(Vi: "ASP.NET Core Web API + Identity Server + Angular", En: "ASP.NET Core Web API + Identity Server + Angular")),
            new CourseEntry("[TEDU-43]", new LocalizedText(Vi: "API DDD, MongoDB và Blazor", En: "DDD API, MongoDB and Blazor")),
            new CourseEntry("[TEDU-49]", new LocalizedText(Vi: "Kiến trúc Micro-service", En: "Micro-service Architecture"))
        }
    );

    public static int YearsShipping => DateTime.Now.Year - 2021;
}
