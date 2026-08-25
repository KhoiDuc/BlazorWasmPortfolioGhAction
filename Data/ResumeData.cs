namespace BlazorWasmPortfolioGhAction.Data;

public record Profile(
    string NameEn,
    string NameVn,
    string FullName,
    string RoleEn,
    string RoleVn,
    string[] PhoneNumbers,
    string[] Emails,
    string FacebookUrl,
    string LinkedInUrl,
    string TelegramHandle,
    string GitHubUrl,
    string PdfResumeUrl,
    string CertificateUrl,
    string TranscriptUrl,
    string[] AboutEn,
    string[] AboutVn
);

public record ExperienceEntry(
    string Title,
    string Company,
    string Role,
    string PeriodEn,
    string PeriodVn,
    string DescriptionEn,
    string DescriptionVn,
    string[] TechStack,
    string? DetailLink
);

public record ProjectEntry(
    string Type,
    int Number,
    string Name,
    string DescriptionEn,
    string DescriptionVn,
    string RepoUrl,
    string? DetailLink,
    string[] TechStack
);

public record EducationEntry(
    string Period,
    string NameEn,
    string NameVn,
    string ProgramEn,
    string ProgramVn,
    string Url,
    string Gpa,
    string DocumentUrl,
    string DocumentLabelEn,
    string DocumentLabelVn
);

public record SkillEntry(
    string Name,
    int Level
);

public record SkillCategory(
    string NameEn,
    string NameVn,
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
    string TitleEn,
    string TitleVn
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
            NameEn: "Khoi Duc",
            NameVn: "Khoi Duc",
            FullName: "Nguyen Minh Duc Khoi",
            RoleEn: "Junior Developer",
            RoleVn: "Lập trình viên",
            PhoneNumbers: new[] { "+84938751116", "+84384223897" },
            Emails: new[] { "khoi.duc.dev@gmail.com", "khoimessi99@gmail.com" },
            FacebookUrl: "https://www.facebook.com/messi.khoi.9",
            LinkedInUrl: "https://www.linkedin.com/in/%C4%91%E1%BB%A9c-kh%C3%B4i-354b77269/",
            TelegramHandle: "warman99",
            GitHubUrl: "https://github.com/KhoiDuc",
            PdfResumeUrl: "files/resume.pdf",
            CertificateUrl: "https://drive.google.com/file/d/17NgDYao2f84W4U346hyuSWZkDIkg612e/view?usp=sharing",
            TranscriptUrl: "files/transcript.pdf",
            AboutEn: new[] {
                "I am Nguyen Minh Duc Khoi, a passionate and dedicated software developer specializing in .NET technologies, with a strong focus on building scalable, maintainable solutions and solving real-world problems through code.",
                "A highly skilled Backend Developer with 3 years of experience in C# and JavaScript, proficient in building high-performance, scalable ASP.NET Core applications and optimizing database queries (MySQL, PostgreSQL, Elasticsearch, SQL Server). Possessing strong analytical skills and practical knowledge of architecture design patterns, this developer has experience designing and developing WCAG 2.1 compliant web/API applications using .NET MVC and .NET Core, with domain expertise in Data Security, E-Commerce & Retail, and Human Resource Management."
            },
            AboutVn: new[] {
                "Tôi là Nguyễn Minh Đức Khôi, một lập trình viên phần mềm đam mê và tận tâm, chuyên về các công nghệ .NET, với trọng tâm xây dựng các giải pháp có khả năng mở rộng, dễ bảo trì và giải quyết các vấn đề thực tế thông qua mã lệnh.",
                "Là một Lập trình viên Backend có kinh nghiệm 3 năm với C# và JavaScript, thành thạo trong việc xây dựng các ứng dụng ASP.NET Core hiệu suất cao, có khả năng mở rộng và tối ưu hóa truy vấn cơ sở dữ liệu (MySQL, PostgreSQL, Elasticsearch, SQL Server). Với kỹ năng phân tích mạnh mẽ và kiến thức thực tiễn về các mẫu thiết kế kiến trúc, tôi có kinh nghiệm thiết kế và phát triển các ứng dụng web/API tuân thủ WCAG 2.1 sử dụng .NET MVC và .NET Core."
            }
        ),
        Education: new[]
        {
            new EducationEntry(
                Period: "2017 — 2021",
                NameEn: "HUFLIT University (GRADUATED)",
                NameVn: "Đại học HUFLIT (ĐÃ TỐT NGHIỆP)",
                ProgramEn: "Software Engineering",
                ProgramVn: "Kỹ thuật Phần mềm",
                Url: "https://huflit.edu.vn/",
                Gpa: "GPA: 3.33/4.0 (8.32/10)",
                DocumentUrl: "https://drive.google.com/file/d/17NgDYao2f84W4U346hyuSWZkDIkg612e/view?usp=sharing",
                DocumentLabelEn: "View Document",
                DocumentLabelVn: "Xem Tài liệu"
            )
        },
        Experience: new[]
        {
            new ExperienceEntry(
                Title: "Site Manager Plus Plus (SM++)",
                Company: "S3Corp",
                Role: "Full Stack Developer",
                PeriodEn: "2023 — Now",
                PeriodVn: "2023 — Hiện tại",
                DescriptionEn: "Upgraded version of Site Manager running on .NET 8. Platform for rapid website setup and management with modular page architecture. Supports SSO integration with third-party identity providers including Citi and Wells Fargo.",
                DescriptionVn: "Phiên bản nâng cấp của Site Manager chạy trên .NET 8. Nền tảng thiết lập và quản lý trang web nhanh chóng với kiến trúc trang mô-đun. Hỗ trợ tích hợp SSO với các nhà cung cấp danh tính bên thứ ba bao gồm Citi và Wells Fargo.",
                TechStack: new[] { ".NET 8 MVC", "jQuery", "EF", "SQL Server", "Redis", "Azure CDN", "Kontent.AI", "WCAG 2.2" },
                DetailLink: "/project/smpp"
            ),
            new ExperienceEntry(
                Title: "Site Manager",
                Company: "S3Corp",
                Role: "Full Stack Developer",
                PeriodEn: "2022 — 2024",
                PeriodVn: "2022 — 2024",
                DescriptionEn: "Platform for creating and deploying websites quickly. Enables users to search, explore, and book services (dining, hotels, flights, spas, limousines). Built to comply with WCAG 2.1, PCI DSS, and Veracode security scanning.",
                DescriptionVn: "Nền tảng tạo và triển khai trang web nhanh chóng. Cho phép người dùng tìm kiếm, khám phá và đặt dịch vụ (ăn uống, khách sạn, chuyến bay, spa, xe limousine). Xây dựng tuân thủ WCAG 2.1, PCI DSS và quét bảo mật Veracode.",
                TechStack: new[] { ".NET 4 MVC", "jQuery", "EF", "SQL Server", "Redis", "Azure CDN", "Kontent.AI", "WCAG 2.1" },
                DetailLink: "/project/sm"
            ),
            new ExperienceEntry(
                Title: "Payment Portal",
                Company: "S3Corp",
                Role: "Full Stack Developer",
                PeriodEn: "2022 — Now",
                PeriodVn: "2022 — Hiện tại",
                DescriptionEn: "Manages international transactions through Adyen payment system integration. Currently upgrading from .NET MVC to .NET 8.",
                DescriptionVn: "Quản lý giao dịch quốc tế thông qua tích hợp hệ thống thanh toán Adyen. Đang nâng cấp từ .NET MVC lên .NET 8.",
                TechStack: new[] { ".NET MVC 4", "EF", "SQL Server", "JavaScript", "Webhook" },
                DetailLink: "/project/payment-portal"
            ),
            new ExperienceEntry(
                Title: "Simply Blast",
                Company: "S3Corp",
                Role: "Full Stack Developer",
                PeriodEn: "06/2022 — 09/2022",
                PeriodVn: "06/2022 — 09/2022",
                DescriptionEn: "Web application for clients to create and manage advertising campaigns through WhatsApp messages.",
                DescriptionVn: "Ứng dụng web cho khách hàng tạo và quản lý chiến dịch quảng cáo qua tin nhắn WhatsApp.",
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
                DescriptionEn: "Personal portfolio site built with Blazor WebAssembly, featuring DevOps utilities, email composer, and 40+ tools.",
                DescriptionVn: "Site cá nhân built bằng Blazor WebAssembly, gồm công cụ DevOps, email composer, và 40+ tool.",
                RepoUrl: "https://github.com/KhoiDuc/BlazorWasmPortfolioGhAction",
                DetailLink: null,
                TechStack: new[] { "Blazor WASM", ".NET 9", "Fluxor", "GitHub Actions" }
            ),
            new ProjectEntry(
                Type: "School",
                Number: 2,
                Name: "Food Ordering System",
                DescriptionEn: "Full-stack food ordering with .NET 5 backend and Flutter frontend.",
                DescriptionVn: "Đặt đồ ăn full-stack với backend .NET 5 và frontend Flutter.",
                RepoUrl: "https://github.com/khoinmdIT99/FoodOrder_NET5_FLUTTER",
                DetailLink: "/project/food-ordering",
                TechStack: new[] { ".NET 5", "Flutter" }
            ),
            new ProjectEntry(
                Type: "School",
                Number: 3,
                Name: "Building Material E-Commerce",
                DescriptionEn: "Full-stack e-commerce platform for building materials with .NET 5 backend and Angular frontend.",
                DescriptionVn: "Nền tảng TMĐT vật liệu xây dựng full-stack với .NET 5 và Angular.",
                RepoUrl: "https://github.com/khoinmdIT99/BuildingMaterialShop_NET5_ANUGLAR",
                DetailLink: "/project/building-material",
                TechStack: new[] { ".NET 5", "Angular" }
            ),
            new ProjectEntry(
                Type: "School",
                Number: 4,
                Name: "Payroll Management System",
                DescriptionEn: "Enterprise payroll management system built with .NET Core MVC.",
                DescriptionVn: "Hệ thống quản lý lương doanh nghiệp built với .NET Core MVC.",
                RepoUrl: "https://github.com/khoinmdIT99/Payroll-Manager/",
                DetailLink: null,
                TechStack: new[] { ".NET Core MVC" }
            )
        },
        Skills: new[]
        {
            new SkillCategory("Programming Languages", "Ngôn ngữ lập trình", true, new[]
            {
                new SkillEntry("C# / .NET", 90),
                new SkillEntry("JavaScript", 75),
                new SkillEntry("Java", 60),
                new SkillEntry("Dart (Flutter)", 55)
            }, null),
            new SkillCategory("Frontend", "Frontend", false, null, new[] { "HTML5", "CSS3", "jQuery", "Vanilla JS", "React", "Angular", "Blazor" }),
            new SkillCategory("Databases", "Cơ sở dữ liệu", true, new[]
            {
                new SkillEntry("SQL Server", 88),
                new SkillEntry("MySQL", 80),
                new SkillEntry("PostgreSQL", 70),
                new SkillEntry("MongoDB", 65)
            }, null),
            new SkillCategory("Frameworks", "Framework", false, null, new[] { ".NET Core 6/8/9", ".NET Framework 4.8", ".NET MVC", "WPF", "Web API", "WinForms" }),
            new SkillCategory("Tools & DevOps", "Công cụ & DevOps", false, null, new[] { "Visual Studio", "VS Code", "ReSharper", "Redis", "Docker", "Azure", "GitHub Actions" }),
            new SkillCategory("Architecture & Practices", "Kiến trúc & Thực hành", false, null, new[] { "Microservices", "Monolithic", "Database Design", "WCAG 2.1/2.2", "PCI DSS" }),
            new SkillCategory("Payment Integration", "Tích hợp thanh toán", false, null, new[] { "Adyen", "PayPal", "Momo" })
        },
        ToeicScores: new[]
        {
            new ToeicScore(600, 2024),
            new ToeicScore(686, 2023),
            new ToeicScore(595, 2022)
        },
        Courses: new[]
        {
            new CourseEntry("[TEDU-42]", "Master Docker to Conquer DevOps", "Làm chủ Docker để chinh phục DevOps"),
            new CourseEntry("[TEDU-47]", "Mastering Microsoft Azure Cloud", "Làm chủ đám mây Azure"),
            new CourseEntry("[TEDU-35]", "ASP.NET Core Web API + Identity Server + Angular", "ASP.NET Core Web API + Identity Server + Angular"),
            new CourseEntry("[TEDU-43]", "DDD API, MongoDB and Blazor", "API DDD, MongoDB và Blazor"),
            new CourseEntry("[TEDU-49]", "Micro-service Architecture", "Kiến trúc Micro-service")
        }
    );

    public static int YearsShipping => DateTime.Now.Year - 2022;
}