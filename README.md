# Khoi Duc — Modern Blazor WebAssembly Portfolio & Engineering Hub

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![Deploy to GitHub Pages](https://github.com/KhoiDuc/BlazorWasmPortfolioGhAction/actions/workflows/main.yml/badge.svg)](https://github.com/KhoiDuc/BlazorWasmPortfolioGhAction/actions)
[![xUnit Tests](https://img.shields.io/badge/Unit%20Tests-32%20Passed-22c55e?logo=xunit&logoColor=white)](tests/BlazorWasmPortfolioGhAction.Tests)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Live Demo](https://img.shields.io/badge/Live-Demo-ca8a04?logo=google-chrome&logoColor=white)](https://khoiduc.github.io/BlazorWasmPortfolioGhAction/)

An enterprise-grade, high-performance personal portfolio, EdTech learning hub, financial trading desk, and developer utility suite built with **Blazor WebAssembly (.NET 10)**, C# 13, Fluxor (Redux State Management), and modern browser Web APIs.

---

## 🌟 Bảng Mục Lục (Table of Contents)

1. [Tổng Quan Kiến Trúc (Architecture Overview)](#-tổng-quan-kiến-trúc-architecture-overview)
2. [Các Phân Hệ Nổi Bật (Key Modules)](#-các-phân-hệ-nổi-bật-key-modules)
   - [1. TOEIC & EdTech Learning Hub](#1--cẩm-nang-ôn-thi-toeic-4-skills--edtech-hub)
   - [2. Speaking Studio & Dev Learning](#2--speaking-studio--dev-learning-roadmaps)
   - [3. Trading & Financial Desk (VnDesk)](#3--trung-tâm-giao-dịch-chứng-khoán--tài-chính-trading-desk)
   - [4. Developer Utilities (40+ Công Cụ)](#4--bộ-công-cụ-lập-trình-viên-developer-utilities-40-tools)
   - [5. Interactive CV & Personal Portfolio](#5--hồ-sơ-năng-lực-song-ngữ-interactive-portfolio--cv)
   - [6. Wiki CMS](#6--hệ-thống-quản-trị-tri-thức-wiki-cms)
3. [Công Nghệ & Kỹ Thuật (Tech Stack & Architecture)](#-công-nghệ--kỹ-thuật-tech-stack)
4. [Kiểm Thử Tự Động (Automated Testing)](#-kiểm-thử-tự-động-automated-testing)
5. [Quy Trình CI/CD (GitHub Actions Pipeline)](#-quy-trình-cicd-github-actions-pipeline)
6. [Hướng Dẫn Cài Đặt & Chạy Local (Getting Started)](#-hướng-dẫn-cài-đặt--chạy-local-getting-started)
7. [Cấu Hình Hệ Thống (Configuration Reference)](#-cấu-hình-hệ-thống-configuration-reference)

---

## 🏛️ Tổng Quan Kiến Trúc (Architecture Overview)

Dự án được xây dựng theo mô hình **Client-side Single Page Application (SPA)** chạy hoàn toàn trên trình duyệt qua WebAssembly, tối ưu hóa thời gian phản hồi, khả năng hoạt động offline linh hoạt, và giao diện song ngữ mượt mà.

- **Đa ngôn ngữ & Địa phương hóa (i18n)**: Hỗ trợ chuyển đổi song ngữ Anh - Việt tức thì trên toàn bộ trang qua `CultureAwareComponent` và tiền tố URL (`/vn/...` và `/en/...`).
- **Giao diện Theme System**: Hỗ trợ 2 chế độ màu chuẩn UI/UX: **Midnight Gold Dark Mode** và **Crisp Slate Light Mode**.
- **State Management**: Sử dụng **Fluxor** (Redux pattern cho Blazor) với hỗ trợ Redux DevTools khi debug.
- **Tích hợp Web APIs hiện đại**: Web Audio API (bộ tạo âm thanh tần số cao), SpeechSynthesis (Text-to-Speech), MediaRecorder (ghi âm giọng nói trực tiếp), LocalStorage, WebGL canvas shaders.

---

## 🚀 Các Phân Hệ Nổi Bật (Key Modules)

### 1. 🎓 Cẩm Nang Ôn Thi TOEIC 4-Skills & EdTech Hub
*Đường dẫn: `/learning/toeic`*

Phân hệ ôn thi TOEIC toàn diện được thiết kế chuẩn hóa theo cấu trúc của ETS và IIG Việt Nam:
- **Cấu trúc 7 Part & Phân bổ 120 phút**: Phân tích chi tiết format đề thi, phân bổ thời gian từng giây cho 200 câu hỏi nhằm tránh thiếu giờ ở Part 7, cẩm nang phòng thi IIG (DOs & DON'Ts) và nhận diện 4 ngữ điệu phát âm (Mỹ, Anh, Úc, Canada).
- **Interactive Score Calculator (10–990)**: Bộ chuyển đổi điểm thô Listening & Reading sang điểm chuẩn ETS theo thuật toán đường cong phân vị thực tế, tự động quy đổi sang khung năng lực Châu Âu **CEFR (A1–C1)** và đưa ra nhận xét định hướng cá nhân hóa.
- **Kho Từ Vựng 2.050 Từ & Word Family Đầy Đủ**:
  - 41 chủ đề cốt lõi (50 từ/chủ đề) với đầy đủ 4 vị trí họ từ (`v`, `n`, `adj`, `adv`), phiên âm IPA, audio phát âm và collocations.
  - **Instant Search**: Thanh tra cứu siêu tốc toàn cục trên cả 2.050 từ theo tiếng Anh, tiếng Việt, cụm từ hoặc loại từ.
  - **Thẻ nhớ Flashcard 3D**: Lật thẻ 3D trực quan, hỗ trợ phím tắt bàn phím (<kbd>Space</kbd>, <kbd>←</kbd>, <kbd>→</kbd>, <kbd>M</kbd>), lọc riêng từ chưa thuộc / đã thuộc.
  - **Game Nối Từ (Match Pairs)**: Mini-game tính điểm phản xạ từ vựng.
  - **AI Daily Vocab**: Tích hợp Google Gemini AI tạo 10 từ vựng mỗi ngày kèm chuỗi Streak học tập.
- **12 Trụ Cột Ngữ Pháp Part 5 & Mini Quiz Tương Tác**: Tổng hợp 12 chủ điểm ngữ pháp chắc chắn xuất hiện trong đề thi, bảng cheat công thức, bài tập trắc nghiệm tương tác chấm điểm tức thì và lưu trữ tiến độ bền vững vào `localStorage`.
- **Bóc Tách Cạm Bẫy Đề Thi (Traps Part 1–7)**: So sánh trực quan bẫy đề thi vs đáp án chuẩn, mẹo phản xạ 5s và công cụ theo dõi bẫy đã thành thạo (Mastery Tracker).
- **Phòng Luyện Nói (Speaking Lab)**: Bộ đếm thời gian thi Prep/Speak chuẩn ETS có âm thanh **BEEP!**, dàn ý Blueprint, bài mẫu điểm tối đa có Audio và **Bộ thu âm trực tiếp (Voice Recorder)** để nghe lại giọng mình.
- **Xưởng Luyện Viết (Writing Sandbox)**: Phòng thực hành viết câu theo tranh (Task 1), email phản hồi (Task 2) và bài luận 300+ từ (Task 3) với **bộ đếm từ trực tiếp**, lưu nháp tự động và tính năng **So sánh song song (Split-View)** với bài mẫu điểm tuyệt đối.

---

### 2. 🎙️ Speaking Studio & Dev Learning Roadmaps
*Đường dẫn: `/learning/speaking` và `/learning/dev-learning`*

- **Speaking Studio**: Phòng luyện nói tiếng Anh giao tiếp chuyên sâu với hơn 500+ bài tập phản xạ, kỹ thuật Shadowing, hỗ trợ ghi âm và phát âm chuẩn bản ngữ.
- **Dev Learning Hub**: Hệ thống giáo trình và lộ trình kỹ thuật phần mềm chất lượng cao bao gồm nhiều series bài học tuần tự:
  - Microservices Architecture & Distributed Systems.
  - Authentication, JWT, OAuth2, OpenID Connect & Security Best Practices.
  - DevOps, Docker, Kubernetes, CI/CD Pipelines & Cloud Architecture.
  - RESTful, gRPC, OpenAPI Specification & API Design Standards.
  - AI & Machine Learning Integrations.

---

### 3. 📈 Trung Tâm Giao Dịch Chứng Khoán & Tài Chính (Trading Desk)
*Đường dẫn: `/trading/vndesk`*

Hệ thống công cụ tài chính chuyên nghiệp tối ưu riêng cho thị trường chứng khoán Việt Nam:
- **VnDesk**: 
  - Tính toán chi phí giao dịch chính xác (phí môi giới, thuế TNCN 0.1%, phí lưu ký, lãi vay Margin T+).
  - Quy chuẩn thị trường Việt Nam: Biên độ dao động trần/sàn theo từng sàn (HOSE ±7%, HNX ±10%, UPCoM ±15%), bước giá quy định, quản trị phiên ATO/ATC.
  - Bộ tính điểm sức mạnh cổ phiếu (Watchlist Scorer), tín hiệu kỹ thuật (MA, RSI, MACD, Bollinger Bands) và công cụ Break-even Analysis.
- **Broker Portfolio & TCBS**:
  - Quản lý danh mục đầu tư thực chiến, theo dõi lãi/lỗ chưa thực hiện (Unrealized PnL), định giá danh mục theo thời gian thực.
  - Xác thực bảo mật tập trung qua JWT kết nối với [broker-api](https://github.com/KhoiDuc/BROKER-API).

---

### 4. 🛠️ Bộ Công Cụ Lập Trình Viên (Developer Utilities: 40+ Tools)
*Đường dẫn: `/Utility`*

Không gian làm việc tất cả-trong-một (All-in-One Utility Workspace) cho developer, phân loại thành 4 nhóm:

| Phân nhóm | Danh sách công cụ tiêu biểu |
| :--- | :--- |
| **Text & Code** | • **Markdown Converter**: Trình soạn thảo & chuyển đổi Markdown sang HTML real-time.<br>• **HTML / Base64 / Hex Encoder & Decoder**.<br>• **JSON Stringify / YAML / XML Formatter**.<br>• **Gemini Spell Checker**: Trợ lý kiểm tra chính tả thông minh qua AI.<br>• **Unicode Character Picker & ASCII Art Generator**. |
| **Converters & Encoding** | • **JWT Debugger & Inspector**: Giải mã Header, Payload, Signature và kiểm tra tính hợp lệ của JWT Token.<br>• **CIDR / Subnet Calculator**: Phân tích dải IP, subnet mask, số lượng host.<br>• **Luhn Algorithm Validator**: Kiểm tra đầu số và tính hợp lệ của thẻ thanh toán.<br>• **Color Converter (HEX, RGB, HSL)**, Binary / Decimal / Hex Converter, Unit Converters. |
| **DevOps & APIs** | • **GitHub Searcher & Inspector**: Tìm kiếm người dùng, tổ chức, repository qua cả GitHub GraphQL & REST API.<br>• **Trending Repositories & Gist Creator**: Khám phá repo nổi bật và tạo Gist trực tiếp từ trình duyệt.<br>• **ChatOps Notifiers**: Tích hợp gửi thông báo Webhook đến **Discord, Slack, Microsoft Teams**.<br>• **Email Composer**: Soạn và gửi email trực tiếp qua tích hợp EmailJS. |
| **Demos & Interactive** | • **SVG Diagram Editor**: Trình vẽ và biên tập sơ đồ vector SVG tương tác.<br>• **WebGL Canvas Shader**: Trình diễn hiệu ứng đồ họa 3D WebGL.<br>• **Interactive Map & QR Code Generator**.<br>• **Caro Game (Gomoku)**: Trò chơi cờ caro đấu với thuật toán minimax.<br>• **Math & Physics Simulators**: BigBang Counter, Binding Energy Calculator, Overnight Min Temp. |

---

### 5. 💼 Hồ Sơ Năng Lực Song Ngữ (Interactive Portfolio & CV)
*Đường dẫn: `/` và `/resume`*

- Trình bày thông tin cá nhân, kinh nghiệm làm việc, kỹ năng chuyên môn, quá trình học vấn và các dự án tiêu biểu.
- Hỗ trợ xem song ngữ tức thì (Tiếng Việt & Tiếng Anh).
- Tích hợp biểu mẫu liên hệ trực tiếp, tải xuống CV định dạng chuẩn và liên kết mạng xã hội chuyên nghiệp.

---

### 6. 📝 Hệ Thống Quản Trị Tri Thức (Wiki CMS)
*Đường dẫn: `/wiki`*

- Nền tảng CMS tài liệu tích hợp trực tiếp với GitHub Repository API.
- Hỗ trợ chế độ xem tài liệu công khai (Read-only) và chế độ chỉnh sửa bảo mật (Authenticated Editor) cho người quản trị qua JWT token.

---

## 💻 Công Nghệ & Kỹ Thuật (Tech Stack)

- **Ngôn ngữ & Runtime**: C# 13, .NET 10.0 SDK.
- **Frontend Framework**: Blazor WebAssembly (WASM AOT-compatible).
- **Quản lý trạng thái (State Management)**: Fluxor (Redux pattern cho C#).
- **Giao diện & Thiết kế**: Bootstrap 5, Bootstrap Icons, CSS Isolation, Theme CSS Variables (`.tool-*`, `.toeic-*`).
- **Xác thực & Bảo mật (Auth)**: JWT Token Authentication qua Broker API.
- **Testing**: xUnit 2.9, bUnit 1.40 (Blazor Component Testing), AngleSharp.
- **Hosting & Triển khai**: GitHub Pages CI/CD tự động.

---

## 🧪 Kiểm Thử Tự Động (Automated Testing)

Dự án duy trì bộ kiểm thử tự động nghiêm ngặt trong thư mục `tests/BlazorWasmPortfolioGhAction.Tests`:

```bash
dotnet test tests/BlazorWasmPortfolioGhAction.Tests/BlazorWasmPortfolioGhAction.Tests.csproj --configuration Release
```

- **ToeicJsonTest**: Kiểm tra tính toàn vẹn của file dữ liệu TOEIC (đảm bảo đủ 2.050 từ/cụm từ, 100% có Word Family hợp lệ, không có ký tự ngoại lai/rò rỉ CJK/Cyrillic, không trùng lặp nghĩa trong cùng chủ đề).
- **DevLearningJsonTest**: Kiểm định tính tuần tự của các bài giảng, mapping category của series và loại bỏ ký tự rò rỉ LaTeX arrow.
- **FinanceMathTests & VnMarketRulesTests**: Kiểm chứng độ chính xác của các thuật toán tính thuế phí, biên độ giá và quản trị danh mục chứng khoán.
- **NavButtonTests & Component Tests**: Kiểm tra khả năng render và tiêu chuẩn trợ năng (Accessibility aria-labels) bằng bUnit.

---

## 🔄 Quy Trình CI/CD (GitHub Actions Pipeline)

Mỗi khi mã nguồn được push lên nhánh `master`, pipeline `.github/workflows/main.yml` sẽ tự động thực hiện:

1. Thiết lập môi trường **.NET 10.0** và cài đặt công cụ WebAssembly Workload (`wasm-tools`).
2. Tải và phục hồi các thư viện phụ thuộc (Cache NuGet Packages).
3. **Chạy toàn bộ bộ test tự động (Automated Tests)** ở chế độ `Release`.
4. Build và xuất bản dự án (`dotnet publish --configuration Release`).
5. Chuẩn hóa thẻ `<base href="/BlazorWasmPortfolioGhAction/" />` và tạo cấu hình điều hướng SPA 404 (`404.html`, `.nojekyll`).
6. Kiểm tra tính toàn vẹn (Health Check) bằng headless server trước khi triển khai.
7. Tự động commit và đẩy bản build lên nhánh `gh-pages` để phục vụ người dùng.

---

## 💻 Hướng Dẫn Cài Đặt & Chạy Local (Getting Started)

### Điều kiện tiên quyết
- Cài đặt [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

### Khởi chạy dự án
```bash
# 1. Clone repository
git clone https://github.com/KhoiDuc/BlazorWasmPortfolioGhAction.git
cd BlazorWasmPortfolioGhAction

# 2. Chạy ứng dụng
dotnet run --project BlazorWasmPortfolioGhAction.csproj
```

Mở trình duyệt và truy cập: `http://localhost:5000` (hoặc cổng hiển thị trên terminal).

---

## ⚙️ Cấu Hình Hệ Thống (Configuration Reference)

Tệp cấu hình client nằm tại [`wwwroot/appsettings.json`](file:///d:/BlazorWasmPortfolioGhAction/wwwroot/appsettings.json):

```json
{
  "BrokerApi": {
    "BaseUrl": "https://your-broker-api-service.com"
  },
  "DevOps": {
    "GitHubToken": "",
    "MockApi": true
  },
  "Wiki": {
    "RepoOwner": "KhoiDuc",
    "RepoName": "BlazorWasmPortfolioGhAction"
  },
  "EmailJs": {
    "ServiceId": "YOUR_SERVICE_ID",
    "TemplateId": "YOUR_TEMPLATE_ID",
    "PublicKey": "YOUR_PUBLIC_KEY"
  }
}
```

*Lưu ý bảo mật: `appsettings.json` được đóng gói trực tiếp vào ứng dụng WebAssembly tải về trình duyệt của người dùng. Không lưu trữ các secret nhạy cảm có quyền quản trị cao vào file này.*

---

## 📄 Bản Quyền (License)

Dự án được phát hành theo giấy phép **MIT License**. Xem thêm chi tiết tại file [LICENSE](LICENSE).

---

**Tác giả**: [Nguyễn Minh Đức Khôi (Khoi Duc)](https://github.com/KhoiDuc)  
*Copyright © 2026 Khoi Nguyen Minh Duc. All rights reserved.*
