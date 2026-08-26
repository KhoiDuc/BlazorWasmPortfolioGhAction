using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

var pairs = new Dictionary<string, (string Vi, string En)>
{
  // Nav / shell
  ["Nav_Home"] = ("Trang chủ", "Home"),
  ["Nav_Resume"] = ("Hồ sơ", "Resume"),
  ["Nav_Tools"] = ("Công cụ", "Tools"),
  ["Nav_Wiki"] = ("Wiki", "Wiki"),
  ["Nav_Trading"] = ("Giao dịch", "Trading"),
  ["Nav_GoHome"] = ("Về trang chủ", "Go to homepage"),
  ["Nav_LightMode"] = ("Chế độ sáng", "Light mode"),
  ["Nav_DarkMode"] = ("Chế độ tối", "Dark mode"),
  ["Nav_SwitchLight"] = ("Chuyển sang chế độ sáng", "Switch to light mode"),
  ["Nav_SwitchDark"] = ("Chuyển sang chế độ tối", "Switch to dark mode"),
  ["Nav_OpenMenu"] = ("Mở menu điều hướng", "Open navigation menu"),
  ["Nav_CloseMenu"] = ("Đóng menu điều hướng", "Close navigation menu"),
  ["Nav_SwitchToEn"] = ("English", "English"),
  ["Nav_SwitchToVi"] = ("Tiếng Việt", "Tiếng Việt"),
  ["Nav_Language"] = ("Ngôn ngữ", "Language"),

  ["Footer_Role"] = (".NET Developer", ".NET Developer"),
  ["Footer_BackToTop"] = ("Về đầu trang", "Back to top"),
  ["Footer_BuiltWith"] = ("Xây dựng bằng Blazor WebAssembly và .NET 9", "Built with Blazor WebAssembly and .NET 9"),

  ["TradingNav_Label"] = ("Điều hướng giao dịch", "Trading navigation"),
  ["TradingNav_Open"] = ("Mở menu giao dịch", "Open trading menu"),
  ["TradingNav_Close"] = ("Đóng menu giao dịch", "Close trading menu"),
  ["TradingNav_VnDesk"] = ("VnDesk", "VnDesk"),
  ["TradingNav_Stock"] = ("Chứng khoán", "Stock"),
  ["TradingNav_Broker"] = ("Broker", "Broker"),
  ["TradingNav_RealEstate"] = ("Bất động sản", "Real Estate"),

  ["NotFound_Eyebrow"] = ("Lỗi 404", "Error 404"),
  ["NotFound_Title"] = ("Không tìm thấy trang.", "Page not found."),
  ["NotFound_Lead"] = ("Trang bạn tìm không tồn tại hoặc đã được di chuyển.", "The page you're looking for doesn't exist or has been moved."),
  ["NotFound_Back"] = ("Về trang chủ", "Back to home"),

  ["App_NotAuthorized"] = ("Bạn không được phép truy cập tài nguyên này.", "You are not authorized to access this resource."),

  ["Html_Title"] = ("Khoi Duc - Portfolio .NET Developer", "Khoi Duc - .NET Developer Portfolio"),
  ["Html_Description"] = ("Khoi Nguyen Minh Duc - Portfolio .NET Developer với dự án, tiện ích và CV.", "Khoi Nguyen Minh Duc - .NET Developer Portfolio featuring projects, utilities, and CV."),

  // Contact / resume contact
  ["Contact_Label"] = ("LIÊN HỆ", "CONTACT"),
  ["Contact_Heading"] = ("Cùng xây dựng điều gì đó.", "Let's build something together."),
  ["Contact_Description"] = ("Tôi sẵn sàng cho cơ hội remote và dự án thú vị. Gửi tin nhắn, tôi sẽ phản hồi.", "I'm open to remote opportunities and interesting projects. Send me a message and I'll get back to you."),
  ["Contact_Name"] = ("Họ tên", "Name"),
  ["Contact_NamePlaceholder"] = ("Tên của bạn", "Your name"),
  ["Contact_Email"] = ("Email", "Email"),
  ["Contact_EmailPlaceholder"] = ("your@email.com", "your@email.com"),
  ["Contact_Subject"] = ("Tiêu đề", "Subject"),
  ["Contact_SubjectPlaceholder"] = ("Tiêu đề (tuỳ chọn)", "Subject (optional)"),
  ["Contact_Message"] = ("Nội dung", "Message"),
  ["Contact_MessagePlaceholder"] = ("Tin nhắn của bạn...", "Your message..."),
  ["Contact_Sent"] = ("Sẵn sàng! Ứng dụng email sẽ mở.", "Message ready! Your email client should open."),
  ["Contact_Compose"] = ("Soạn email", "Compose Email"),
  ["Contact_NameRequired"] = ("Vui lòng nhập họ tên", "Name is required"),
  ["Contact_EmailRequired"] = ("Vui lòng nhập email", "Email is required"),
  ["Contact_EmailInvalid"] = ("Email không hợp lệ", "Invalid email"),
  ["Contact_MessageRequired"] = ("Vui lòng nhập nội dung", "Message is required"),

  // Resume chrome
  ["Resume_Pdf"] = ("CV PDF", "PDF Resume"),
  ["Resume_Certificate"] = ("Chứng chỉ", "Certificate"),
  ["Resume_Transcript"] = ("Bảng điểm", "Transcript"),
  ["Resume_Profile"] = ("Hồ sơ", "Profile"),
  ["Resume_Academic"] = ("Học vấn", "Academic background"),
  ["Resume_WorkProjects"] = ("Dự án làm việc", "Work projects"),
  ["Resume_PersonalProjects"] = ("Dự án cá nhân & học tập", "Personal & school projects"),
  ["Resume_Skills"] = ("Kỹ năng", "Skills"),
  ["Resume_ToeicCourses"] = ("TOEIC & Khóa học", "TOEIC & Courses"),
  ["Resume_RelevantCourses"] = ("KHÓA HỌC LIÊN QUAN", "RELEVANT COURSES"),
  ["Resume_ViewDetails"] = ("Xem chi tiết", "View Details"),
  ["Resume_Details"] = ("Chi tiết", "Details"),
  ["Resume_AllRights"] = ("Mọi quyền được bảo lưu.", "All rights reserved."),
  ["Resume_Repository"] = ("Kho lưu trữ", "Repository"),
  ["Resume_SwitchLang"] = ("English", "Tiếng Việt"),

  // Home
  ["Home_OpenToRemote"] = ("Sẵn sàng làm remote", "Open to remote opportunities"),
  ["Home_Greeting"] = ("Xin chào, tôi là", "Hello, I'm"),
  ["Home_Role"] = (".NET Developer", ".NET Developer"),
  ["Home_Specialization"] = ("Full Stack .NET Developer · Thiên về Backend", "Full Stack .NET Developer · Backend-Focused"),
  ["Home_Description"] = ("Lập trình viên 3 năm kinh nghiệm xây dựng ứng dụng ASP.NET Core có khả năng mở rộng và tối ưu truy vấn CSDL (MySQL, PostgreSQL, Elasticsearch, SQL Server). Kinh nghiệm backend, full-stack web và công cụ DevOps.", "Software developer with 3 years of experience building scalable ASP.NET Core applications and optimizing database queries (MySQL, PostgreSQL, Elasticsearch, SQL Server). Experienced in backend development, full-stack web apps, and DevOps tooling."),
  ["Home_ContactMe"] = ("Liên hệ", "Contact Me"),
  ["Home_ViewProjects"] = ("Xem dự án", "View Projects"),
  ["Home_Resume"] = ("Hồ sơ", "Resume"),
  ["Home_CV"] = ("CV", "CV"),
  ["Home_CoreStack"] = ("STACK CHÍNH", "CORE STACK"),
  ["Home_Language"] = ("Ngôn ngữ", "Language"),
  ["Home_Platform"] = ("Nền tảng", "Platform"),
  ["Home_Web"] = ("Web", "Web"),
  ["Home_Data"] = ("Dữ liệu", "Data"),
  ["Home_BuiltWithBlazor"] = ("Portfolio này được xây bằng Blazor WebAssembly", "This portfolio is built with Blazor WebAssembly"),
  ["Home_AboutLabel"] = ("VỀ TÔI", "ABOUT ME"),
  ["Home_AboutHeading"] = ("Xây dựng phần mềm doanh nghiệp trên backend, dữ liệu và web.", "Building enterprise software across backend, data and web."),
  ["Home_AboutP1"] = ("Tôi là .NET Developer với 3 năm kinh nghiệm chuyên môn xây dựng và bảo trì ứng dụng doanh nghiệp bằng C#, .NET, ASP.NET Core và SQL Server.", "I'm a .NET Developer with 3 years of professional experience building and maintaining enterprise applications using C#, .NET, ASP.NET Core and SQL Server."),
  ["Home_AboutP2"] = ("Công việc tập trung vào thương mại điện tử, tích hợp thanh toán và hệ thống quản lý nội dung, đóng góp trên dịch vụ backend, kiến trúc CSDL, logic nghiệp vụ, báo cáo, tích hợp và ứng dụng web.", "My work focuses on e-commerce, payment integration, and content management systems where I contribute across backend services, database architecture, business logic, reporting, integrations and web applications."),
  ["Home_AboutP3"] = ("Kinh nghiệm trải rộng .NET Core 6/8/9, Blazor WebAssembly, EF Core, Redis, Azure CDN, tuân thủ WCAG 2.1/2.2, PCI DSS và phát triển full-stack hiện đại.", "My experience spans .NET Core 6/8/9, Blazor WebAssembly, EF Core, Redis, Azure CDN, WCAG 2.1/2.2 compliance, PCI DSS, and modern full-stack development."),
  ["Home_ExperienceLabel"] = ("KINH NGHIỆM", "EXPERIENCE"),
  ["Home_ExperienceHeading"] = ("Dự án công việc.", "Work projects."),
  ["Home_PeriodNow"] = ("2023 — Hiện tại", "2023 — Now"),
  ["Home_Period2022_2024"] = ("2022 — 2024", "2022 — 2024"),
  ["Home_Period2022Now"] = ("2022 — Hiện tại", "2022 — Now"),
  ["Home_PeriodInternship"] = ("06/2022 — 09/2022", "06/2022 — 09/2022"),
  ["Home_ExpSmppDesc"] = ("Phiên bản nâng cấp Site Manager trên .NET 8. Nền tảng thiết lập và quản lý website nhanh với kiến trúc trang mô-đun. Hỗ trợ SSO với nhà cung cấp danh tính bên thứ ba gồm Citi và Wells Fargo.", "Upgraded version of Site Manager running on .NET 8. Platform for rapid website setup and management with modular page architecture. Supports SSO integration with third-party identity providers including Citi and Wells Fargo."),
  ["Home_ExpSmDesc"] = ("Nền tảng tạo và triển khai website nhanh. Cho phép tìm kiếm, khám phá và đặt dịch vụ (ăn uống, khách sạn, chuyến bay, spa, limousine). Tuân thủ WCAG 2.1, PCI DSS và quét bảo mật Veracode.", "Platform for creating and deploying websites quickly. Enables users to search, explore, and book services (dining, hotels, flights, spas, limousines). Built to comply with WCAG 2.1, PCI DSS, and Veracode security scanning."),
  ["Home_ExpPaymentDesc"] = ("Quản lý giao dịch quốc tế qua tích hợp thanh toán Adyen. Đang nâng cấp từ .NET MVC lên .NET 8.", "Manages international transactions through Adyen payment system integration. Currently upgrading from .NET MVC to .NET 8."),
  ["Home_ExpBlastDesc"] = ("Ứng dụng web để khách hàng tạo và quản lý chiến dịch quảng cáo qua tin nhắn WhatsApp.", "Web application for clients to create and manage advertising campaigns through WhatsApp messages."),
  ["Home_ProjectsLabel"] = ("DỰ ÁN", "PROJECTS"),
  ["Home_ProjectsHeading"] = ("Dự án cá nhân & học tập.", "Personal & school projects."),
  ["Home_ProjectTypePersonal"] = ("Cá nhân", "Personal"),
  ["Home_ProjectTypeSchool"] = ("Học tập", "School"),
  ["Home_ProjPortfolio"] = ("Site portfolio cá nhân bằng Blazor WebAssembly, gồm tiện ích DevOps (Slack/Discord/Teams/GitHub), soạn email và 40+ công cụ.", "Personal portfolio site built with Blazor WebAssembly, featuring DevOps utilities (Slack/Discord/Teams/GitHub), email composer, and 40+ tools."),
  ["Home_ProjFood"] = ("Đặt đồ ăn full-stack với backend .NET 5 và frontend Flutter.", "Full-stack food ordering with .NET 5 backend and Flutter frontend."),
  ["Home_ProjBuilding"] = ("Nền tảng TMĐT vật liệu xây dựng full-stack với backend .NET 5 và frontend Angular.", "Full-stack e-commerce platform for building materials with .NET 5 backend and Angular frontend."),
  ["Home_ProjPayroll"] = ("Hệ thống quản lý lương doanh nghiệp xây bằng .NET Core MVC.", "Enterprise payroll management system built with .NET Core MVC."),
  ["Home_Details"] = ("Chi tiết", "Details"),
  ["Home_TechLabel"] = ("CÔNG NGHỆ", "TECHNOLOGIES"),
  ["Home_TechHeading"] = ("Công cụ tôi dùng.", "Tools I work with."),
  ["Home_TechLanguages"] = ("Ngôn ngữ", "Languages"),
  ["Home_TechFrameworks"] = ("Framework", "Frameworks"),
  ["Home_TechFrontend"] = ("Frontend", "Frontend"),
  ["Home_TechDatabases"] = ("Cơ sở dữ liệu", "Databases"),
  ["Home_TechDevOps"] = ("Công cụ & DevOps", "Tools & DevOps"),
  ["Home_TechPractices"] = ("Thực hành", "Practices"),
  ["Home_EduLabel"] = ("HỌC VẤN", "EDUCATION"),
  ["Home_EduHeading"] = ("Nền tảng học thuật.", "Academic background."),
  ["Home_EduColumn"] = ("HỌC VẤN", "EDUCATION"),
  ["Home_CertColumn"] = ("CHỨNG CHỈ", "CERTIFICATIONS"),
  ["Home_EduDesc"] = ("Đại học Ngoại ngữ - Tin học TP.HCM. GPA: 3.33/4.0 (8.32/10).", "Ho Chi Minh City University of Foreign Languages - Information Technology. GPA: 3.33/4.0 (8.32/10)."),
  ["Home_Program"] = ("Kỹ thuật Phần mềm", "Software Engineering"),
  ["Home_ToeicScore"] = ("Điểm TOEIC: {0}", "TOEIC Score: {0}"),
  ["Home_ContactHeading"] = ("Cùng xây dựng điều gì đó.", "Let's build something together."),
  ["Home_ContactDesc"] = ("Tôi sẵn sàng thảo luận cơ hội kỹ sư phần mềm, vai trò .NET và dự án backend hoặc full-stack.", "I'm open to discussing software engineering opportunities, .NET development roles and projects involving backend or full-stack application development."),
  ["Home_ComposeEmail"] = ("Soạn email", "Compose Email"),
  ["Home_ComposeViaGmail"] = ("Soạn qua Gmail", "Compose via Gmail"),
  ["Home_ExploreRepos"] = ("Xem kho mã nguồn", "Explore my repositories"),
  ["Home_Close"] = ("Đóng", "Close"),

  // Utility hub
  ["Utility_Label"] = ("TIỆN ÍCH LẬP TRÌNH", "DEVELOPER UTILITIES"),
  ["Utility_Title"] = ("Bộ công cụ", "Tool deck"),
  ["Utility_Lead"] = ("Tìm, lọc và mở converter, máy tính và trợ giúp DevOps.", "Search, filter, and launch converters, calculators, and DevOps helpers."),
  ["Utility_SearchPlaceholder"] = ("Tìm công cụ...", "Search tools..."),
  ["Utility_SearchAria"] = ("Tìm công cụ", "Search tools"),
  ["Utility_CategoriesAria"] = ("Nhóm công cụ", "Tool categories"),
  ["Utility_All"] = ("Tất cả", "All"),
  ["Utility_NoToolsTitle"] = ("Không tìm thấy công cụ", "No tools found"),
  ["Utility_NoToolsLead"] = ("Thử từ khóa hoặc bộ lọc nhóm khác.", "Try a different search term or category filter."),
  ["Utility_AllTools"] = ("Tất cả công cụ", "All tools"),
  ["Utility_SwitchTool"] = ("Đổi công cụ", "Switch tool"),
  ["Utility_Unknown"] = ("Công cụ không rõ. Chọn từ bộ công cụ.", "Unknown tool. Pick one from the tool deck."),
  ["Cat_General"] = ("Công cụ chung", "General Tools"),
  ["Cat_SQL"] = ("Công cụ SQL", "SQL Tools"),
  ["Cat_Crypto"] = ("Mã hóa", "Cryptography Tools"),
  ["Cat_TimeCalc"] = ("Thời gian & Tính toán", "Time & Calculation Tools"),
  ["Cat_Encoding"] = ("Mã hóa & Định dạng", "Encoding & Format Tools"),
  ["Cat_Testing"] = ("Thư viện thử nghiệm", "Testing Library Tools"),
  ["Cat_DevOps"] = ("Công cụ DevOps", "DevOps Tools"),

  // Wiki / Admin
  ["Wiki_Label"] = ("WIKI", "WIKI"),
  ["Wiki_Title"] = ("Kho kiến thức", "Knowledge base"),
  ["Wiki_Lead"] = ("Xem ghi chú portfolio, tài liệu dự án và các mục có thể sửa.", "Browse portfolio notes, project docs, and editable sections."),
  ["Wiki_Edit"] = ("Sửa wiki", "Edit wiki"),
  ["Wiki_ReadOnly"] = ("Xem chỉ đọc", "Read-only view"),
  ["Wiki_AdminLogin"] = ("Đăng nhập Admin", "Admin login"),
  ["Wiki_SearchPlaceholder"] = ("Tìm trang, mục hoặc nội dung...", "Search pages, sections, or content..."),
  ["Wiki_Editing"] = ("Đang sửa", "Editing"),
  ["Wiki_Sections"] = ("{0} mục", "{0} section(s)"),
  ["Wiki_Loading"] = ("Đang tải wiki...", "Loading wiki content..."),
  ["Wiki_EmptyTitle"] = ("Không có mục nào", "No sections found"),
  ["Wiki_LoadingAria"] = ("Đang tải wiki", "Loading wiki"),

  ["Admin_Label"] = ("ADMIN", "ADMIN"),
  ["Admin_SignedIn"] = ("Đã đăng nhập", "Signed in"),
  ["Admin_SignedInLead"] = ("Bạn có thể sửa nội dung wiki. Đăng xuất khi xong.", "You can edit wiki content. Sign out when finished."),
  ["Admin_SignOut"] = ("Đăng xuất", "Sign out"),
  ["Admin_SignIn"] = ("Đăng nhập", "Sign in"),
  ["Admin_SignInLead"] = ("Đăng nhập demo cục bộ để sửa wiki.", "Local demo login for wiki editing."),
  ["Admin_Username"] = ("Tên đăng nhập", "Username"),
  ["Admin_Password"] = ("Mật khẩu", "Password"),
  ["Admin_SigningIn"] = ("Đang đăng nhập...", "Signing in..."),
  ["Admin_Invalid"] = ("Sai tên đăng nhập hoặc mật khẩu.", "Invalid username or password."),

  // Common tool actions
  ["Tool_Encode"] = ("Mã hóa", "Encode"),
  ["Tool_Decode"] = ("Giải mã", "Decode"),
  ["Tool_Clear"] = ("Xóa", "Clear"),
  ["Tool_Copy"] = ("Sao chép", "Copy"),
  ["Tool_Generate"] = ("Tạo", "Generate"),
  ["Tool_Convert"] = ("Chuyển đổi", "Convert"),
  ["Tool_Submit"] = ("Gửi", "Submit"),
  ["Tool_Reset"] = ("Đặt lại", "Reset"),
  ["Tool_Input"] = ("Đầu vào", "Input"),
  ["Tool_Output"] = ("Đầu ra", "Output"),
  ["Tool_Result"] = ("Kết quả", "Result"),
  ["Tool_Loading"] = ("Đang tải...", "Loading..."),
  ["Tool_Error"] = ("Đã xảy ra lỗi", "An error occurred"),
  ["Tool_Success"] = ("Thành công", "Success"),

  // Trading chrome (pages already VN; EN when toggled)
  ["Trading_VnDeskLead"] = ("Bàn làm việc đầu tư VN — từng bước, không tự đặt lệnh.", "VN investing workbench — step by step, no auto orders."),
  ["Trading_GuidedBusy"] = ("Đang chuyển bước...", "Switching steps..."),
  ["Trading_Guided"] = ("Hướng dẫn từng bước", "Guided walkthrough"),
  ["Trading_SymbolLabel"] = ("Mã đang xem", "Active symbol"),
  ["Trading_FilterStocks"] = ("Chưa biết mua gì? Lọc CP", "Not sure what to buy? Screen stocks"),
  ["Trading_AskAi"] = ("Hỏi AI", "Ask AI"),
  ["Trading_HasTaHint"] = ("Đã có số phân tích từ bước Phân tích", "Analysis numbers ready from the Analyze step"),
  ["Trading_BackMain"] = ("← Quay lại luồng chính", "← Back to main flow"),
  ["Trading_FlowAria"] = ("Luồng VnDesk", "VnDesk flow"),
  ["Trading_StepMarket"] = ("Thị trường", "Market"),
  ["Trading_StepMarketDesc"] = ("Xem bối cảnh hôm nay", "See today's context"),
  ["Trading_StepAnalyze"] = ("Phân tích", "Analyze"),
  ["Trading_StepAnalyzeDesc"] = ("Xem chart & chỉ báo", "Charts & indicators"),
  ["Trading_StepDecide"] = ("Có nên mua?", "Should I buy?"),
  ["Trading_StepDecideDesc"] = ("Tính rủi ro & quyết định", "Risk & decision"),
  ["Trading_StepJournal"] = ("Nhật ký", "Journal"),
  ["Trading_StepJournalDesc"] = ("Ghi lại sau phiên", "Log after the session"),
};

// Tool catalog
void AddTool(string key, string titleVi, string titleEn, string descVi, string descEn)
{
  pairs[$"Tool_{key}_Title"] = (titleVi, titleEn);
  pairs[$"Tool_{key}_Desc"] = (descVi, descEn);
}

AddTool("base64", "Base64", "Base64", "Mã hóa và giải mã chuỗi Base64.", "Encode and decode Base64 strings.");
AddTool("urls", "Công cụ URL", "URL Tools", "Tách URL và mã hóa percent.", "Split URLs and percent-encode text.");
AddTool("guid", "Tạo GUID", "GUID Generator", "Tạo GUID hàng loạt theo định dạng.", "Generate formatted GUIDs in bulk.");
AddTool("html", "HTML Encode/Decode", "HTML Encode/Decode", "Chuyển entity HTML an toàn.", "Convert HTML entities safely.");
AddTool("markdown", "Chuyển Markdown", "Markdown Converter", "Biến HTML thành Markdown.", "Transform HTML into Markdown.");
AddTool("converter", "Đổi đơn vị", "Unit Converters", "Hex, bộ nhớ và thời gian.", "Hex, memory, and time converters.");
AddTool("stringconverter", "Chuyển chuỗi", "String Converter", "Đổi kiểu chữ và định dạng.", "Change text casing and formats.");
AddTool("geminispellchecker", "Kiểm tra chính tả (Gemini)", "Spell Checker (Gemini)", "Kiểm tra tiếng Việt qua Gemini.", "Vietnamese spell check via Gemini.");
AddTool("randomfact", "Sự thật ngẫu nhiên", "Random Fact", "Lấy fact ngẫu nhiên từ web.", "Fetch a random fact from the web.");
AddTool("jokequotes", "Truyện cười", "Joke Quotes", "Joke ngẫu nhiên theo chủ đề.", "Random jokes with category filters.");
AddTool("sqlite", "SQLite", "SQLite Integration", "Demo SQLite + EF Core phía client.", "Client-side SQLite + EF Core demo.");
AddTool("jwt", "JWT Debugger", "JWT Debugger", "Decode, encode và xác minh JWT.", "Decode, encode, and verify JWTs with live sync like jwt.io.");
AddTool("luhn", "Luhn Checker", "Luhn Checker", "Kiểm tra số bằng thuật toán Luhn.", "Validate numbers with Luhn algorithm.");
AddTool("cryptography", "MD5 / Hash", "MD5 / Hash", "Tạo hash MD5 từ văn bản.", "Generate MD5 hashes from text.");
AddTool("mathcalculator", "Máy tính", "Math Calculator", "Demo bình phương và chu vi.", "Squaring and perimeter demos.");
AddTool("overnightmintemp", "Nhiệt độ tối thiểu", "Min Temp Calculator", "Ước lượng nhiệt độ tối thiểu qua đêm.", "Estimate overnight minimum temperature.");
AddTool("cidrcalculator", "CIDR Calculator", "CIDR Calculator", "Chuyển dải IP và CIDR.", "Convert IP ranges and CIDR notation.");
AddTool("timerstopwatch", "Timer / Đồng hồ", "Timer / Stopwatch", "Đếm ngược và bấm giờ.", "Countdown timer and stopwatch.");
AddTool("epoch", "Epoch Converter", "Epoch Converter", "Unix timestamp sang ngày đọc được.", "Unix timestamp to readable dates.");
AddTool("currenttime", "Giờ hiện tại", "Current Time", "UTC, giờ địa phương và thế giới.", "UTC, local, and world clocks.");
AddTool("ascii", "ASCII", "ASCII", "Giá trị ASCII thập phân, hex, oct.", "Decimal, hex, and oct ASCII values.");
AddTool("colourconverter", "Đổi màu", "Colour Converter", "Chọn màu và đọc RGB.", "Pick colors and read RGB values.");
AddTool("binary", "Binary", "Binary", "Chuyển chuỗi nhị phân và văn bản.", "Convert binary strings and text.");
AddTool("unicode", "Unicode Picker", "Unicode Picker", "Duyệt và tìm ký tự Unicode.", "Browse and search Unicode chars.");
AddTool("hcf", "Tìm ký tự ẩn", "Hidden Char Finder", "Hiện ký tự Unicode vô hình.", "Reveal invisible Unicode characters.");
AddTool("fluxortest", "Fluxor Test", "Fluxor Test", "Ví dụ quản lý state Fluxor.", "Fluxor state management examples.");
AddTool("webglrender", "WebGL Canvas", "WebGL Canvas", "Thử nghiệm render WebGL.", "WebGL rendering experiments.");
AddTool("browserstorage", "Browser Storage WASM", "Browser Storage WASM", "Demo localStorage / sessionStorage.", "localStorage / sessionStorage demo.");
AddTool("qrgenerator", "Tạo QR", "QR Generator", "QR SVG tùy chỉnh kèm logo.", "Custom SVG QR codes with logo.");
AddTool("map", "Google Maps", "Google Maps", "Component Google Maps nhúng.", "Embedded Google Maps component.");
AddTool("bigbangcounter", "Big Bang Counter", "Big Bang Counter", "Số giây từ Big Bang.", "Seconds since the Big Bang.");
AddTool("svgdiagrameditor", "SVG Diagram Editor", "SVG Diagram Editor", "Tạo sơ đồ SVG tương tác.", "Create SVG diagrams interactively.");
AddTool("emailcomposer", "Soạn email", "Email Composer", "Gửi email qua EmailJS.", "Send email via EmailJS.");
AddTool("discord", "Discord Notifier", "Discord Notifier", "Tin webhook Discord phong phú.", "Rich Discord webhook messages.");
AddTool("slack", "Slack Notifier", "Slack Notifier", "Thông báo kênh Slack.", "Send Slack channel notifications.");
AddTool("teams", "Teams Notifier", "Teams Notifier", "MessageCard Microsoft Teams.", "Microsoft Teams MessageCards.");
AddTool("githubsearch", "GitHub Search", "GitHub Search", "Tìm repo GitHub công khai.", "Search public GitHub repositories.");
AddTool("githubinspector", "GitHub Inspector", "GitHub Inspector", "Tra user và repository.", "Look up users and repositories.");
AddTool("githubtrending", "GitHub Trending", "GitHub Trending", "Khám phá repo đang trending.", "Discover trending repositories.");
AddTool("gist", "Tạo Gist", "Gist Creator", "Tạo gist công khai hoặc bí mật.", "Create public or secret gists.");

string Esc(string s) => new XText(s).ToString();

XElement Resx(Func<(string Vi, string En), string> pick)
{
  var root = new XElement("root",
    new XElement("resheader", new XAttribute("name", "resmimetype"), new XElement("value", "text/microsoft-resx")),
    new XElement("resheader", new XAttribute("name", "version"), new XElement("value", "2.0")),
    new XElement("resheader", new XAttribute("name", "reader"), new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
    new XElement("resheader", new XAttribute("name", "writer"), new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))
  );
  foreach (var kv in pairs.OrderBy(k => k.Key, StringComparer.Ordinal))
  {
    root.Add(new XElement("data",
      new XAttribute("name", kv.Key),
      new XAttribute(XNamespace.Xml + "space", "preserve"),
      new XElement("value", pick(kv.Value))));
  }
  return root;
}

var dir = @"d:\BlazorWasmPortfolioGhAction\Resources";
File.WriteAllText(Path.Combine(dir, "SharedResources.resx"), Resx(p => p.Vi).ToString(), new UTF8Encoding(false));
File.WriteAllText(Path.Combine(dir, "SharedResources.en.resx"), Resx(p => p.En).ToString(), new UTF8Encoding(false));
Console.WriteLine($"Wrote {pairs.Count} keys");
