using System.IO;
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
            Title = "Modern Infrastructure Tools Masterclass: Containerization (Docker, OCI), Orchestration (Kubernetes, Docker Swarm), Ingress (Kong, NGINX), Message Queues (RabbitMQ, Kafka, SQS) & Observability (Prometheus, Grafana, ELK)",
            TitleVi = "Hệ Sinh Thái Công Cụ Hạ Tầng Hiện Đại: OCI, Docker, Kubernetes, Docker Swarm, Kong, NGINX, RabbitMQ, Kafka & Prometheus, Grafana, ELK",
            Icon = "bi-diagram-3-fill",
            Color = "#0284c7",
            Badge = "Modern Infrastructure & Cloud-Native",
            Description = "A comprehensive masterclass on core modern infrastructure tools for distributed systems and microservices: Low-level containerization with Docker & OCI (runc/containerd), cluster orchestration with Kubernetes vs Docker Swarm, edge API gateways & reverse proxies with Kong & NGINX, asynchronous message queues with RabbitMQ, Kafka & AWS SQS, and centralized observability stacks with Prometheus, Grafana & ELK Stack.",
            DescriptionVi = "Chuyên đề phân tích toàn diện hệ sinh thái công cụ hạ tầng hiện đại cho kiến trúc Microservices và Cloud-Native: Đóng gói Container tầng sâu với Docker & OCI (runc/containerd), điều phối cụm phân tán với Kubernetes vs Docker Swarm, cổng vào API Gateway & Ingress với Kong & NGINX, truyền thông điệp bất đồng bộ tin cậy với RabbitMQ, Kafka & AWS SQS, cùng hạ tầng giám sát toàn diện với Prometheus, Grafana & ELK Stack.",
            Articles = new List<DevArticle>()
        };

        // =========================================================================
        // BÀI 1: CONTAINERIZATION & OCI
        // =========================================================================
        var article1 = new DevArticle
        {
            Id = "infra-tools-article-1-containerization-oci-docker-runtime",
            SeriesId = series.Id,
            LessonNumber = "Bài 1",
            Level = "Beginner",
            LevelVi = "Cơ bản",
            LevelOrder = 1,
            Title = "Deep-Dive Containerization & OCI: Linux Namespaces, cgroups v2, containerd, runc & Production Packaging",
            TitleVi = "Containerization & Chuẩn OCI Tầng Sâu: Namespaces, cgroups v2, containerd, runc & Kỹ Thuật Đóng Gói Container Chuẩn Production",
            ReadingTime = "16 phút đọc",
            PublishedDate = "25/10/2026",
            Summary = "Look beyond Docker as a black box. Understand how Linux Kernel primitives (Namespaces, cgroups v2, OverlayFS) isolate processes, the Open Container Initiative (OCI) Image and Runtime specifications, how Docker CLI delegates to containerd and runc, and master production multi-stage Dockerfile packaging using Distroless/Chisel minimal base images.",
            SummaryVi = "Vượt ra ngoài việc xem Docker như một chiếc hộp đen thần bí. Nắm vững bản chất cô lập tiến trình của Linux Kernel (Namespaces, cgroups v2, OverlayFS), sự ra đời của chuẩn OCI (Open Container Initiative) tách biệt Image Spec và Runtime Spec, luồng thực thi Docker CLI → containerd → runc, cùng kỹ thuật tối ưu hóa Dockerfile đa tầng (Multi-stage build) kết hợp Distroless/Chisel giảm kích thước tới 90% và triệt tiêu lỗ hổng bảo mật CVE.",
            Tags = new List<string> { "Docker", "OCI", "containerd", "runc", "Linux Namespaces", "cgroups", "Multi-Stage Build" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Container Foundations: Why Containers Are Just Isolated Linux Processes",
                    HeadingVi = "1. Bản Chất Của Container: Vì Sao Container Không Phải Là Máy Ảo (VM)?",
                    Paragraphs = new List<string>
                    {
                        "A common misconception among developers is that containers are lightweight virtual machines. In reality, a container does not run a hypervisor, does not boot a guest operating system, and possesses no virtual hardware. A container is simply a standard Linux process isolated by the host Linux kernel using three core primitives: Namespaces, Control Groups (cgroups), and OverlayFS.",
                        "Linux Namespaces partition kernel resources so that a process inside a container believes it has its own dedicated system. PID namespace ensures the process sees itself as PID 1; Mount namespace provides an isolated filesystem mount table; Network namespace gives a dedicated virtual ethernet interface (veth) and routing table; IPC and UTS namespaces isolate inter-process communication and hostnames.",
                        "Control Groups (specifically cgroups v2) enforce resource governance: restricting maximum CPU shares, capping memory ceilings to prevent Out-Of-Memory (OOM) cascades, and throttling disk I/O. OverlayFS combines read-only lower image layers with a thin writable container layer via Copy-on-Write (CoW), enabling instant startup and microscopic storage overhead."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Một hiểu lầm rất phổ biến của nhiều kỹ sư là coi container như một chiếc máy ảo (VM) thu nhỏ. Trên thực tế, container không có Hypervisor, không khởi động hệ điều hành khách (Guest OS) và không có phần cứng ảo hóa. Một container về bản chất chỉ là một tiến trình (process) Linux bình thường nhưng được cách ly bởi Kernel thông qua 3 trụ cột: Namespaces, Control Groups (cgroups) và OverlayFS.",
                        "Linux Namespaces phân mảnh tài nguyên của kernel để tiến trình bên trong container tin rằng nó đang sở hữu toàn bộ hệ điều hành: PID namespace biến tiến trình chính thành PID 1; Mount namespace cô lập cấu trúc thư mục; Network namespace cung cấp card mạng ảo (veth) và bảng định tuyến riêng; IPC và UTS namespace cô lập giao tiếp liên tiến trình và hostname.",
                        "Control Groups (đặc biệt là cgroups v2 hiện đại) đảm nhận vai trò quản trị tài nguyên: giới hạn xung nhịp CPU, thiết lập trần bộ nhớ RAM để ngăn chặn lỗi sập toàn bộ máy chủ (Out-Of-Memory), và điều tiết băng thông đọc ghi đĩa. OverlayFS xếp chồng các layer chỉ đọc (read-only) với một layer ghi tạm thời (read-write) theo cơ chế Copy-on-Write (CoW), giúp container khởi động chỉ trong vài phần nghìn giây."
                    },
                    CodeTitle = "Linux Kernel Primitives: Inspecting Container Namespaces & cgroups v2",
                    CodeLanguage = "bash",
                    CodeContent = """
# 1. Kiểm tra các Namespaces mà một tiến trình Docker container đang sở hữu:
# Lấy PID thực tế của container trên Linux Host:
CONTAINER_PID=$(docker inspect --format '{{.State.Pid}}' my-web-app)

# Liệt kê tất cả các namespace được ánh xạ trong thư mục /proc:
ls -l /proc/$CONTAINER_PID/ns
# Kết quả:
# ipc -> ipc:[4026532450]
# mnt -> mnt:[4026532448]
# net -> net:[4026532453]   <-- Card mạng ảo riêng biệt
# pid -> pid:[4026532451]   <-- PID 1 bên trong, nhưng là $CONTAINER_PID trên Host!
# uts -> uts:[4026532449]

# 2. Quan sát giới hạn bộ nhớ qua cgroups v2 trên Host:
cat /sys/fs/cgroup/system.slice/docker-*.scope/memory.max
# Trả về giới hạn RAM tính bằng bytes (ví dụ: 536870912 = 512MB)

# 3. Thử nghiệm tạo 'container thủ công' bằng lệnh unshare của Linux kernel:
sudo unshare --fork --pid --mount-proc --net /bin/bash
# Ngay lập tức, bạn ở trong một shell mới có PID = 1 và không thấy tiến trình của Host!
""",
                    CalloutType = "warning",
                    CalloutTitle = "Root Inside Container is Root on Host Kernel",
                    CalloutTitleVi = "Cảnh Báo Sống Còn: Quyền Root Trong Container Có Thể Xâm Nhập Host",
                    CalloutText = "Because containers share the host Linux kernel directly, running an application as root inside a container grants potential root-level access to the host machine if a container escape vulnerability exists. Always create a dedicated non-root user (e.g. USER appuser) in your production Dockerfiles.",
                    CalloutTextVi = "Vì container chia sẻ trực tiếp Kernel của máy chủ, việc chạy ứng dụng dưới quyền root bên trong container đồng nghĩa với nguy cơ kẻ tấn công chiếm toàn quyền kiểm soát Host nếu xảy ra lỗ hổng thoát container (Container Escape). Luôn khai báo user thường (ví dụ: USER appuser) trong Dockerfile Production."
                },
                new DevArticleSection
                {
                    Heading = "2. The OCI Standard: How Docker CLI, containerd & runc Work Together",
                    HeadingVi = "2. Chuẩn OCI (Open Container Initiative): Docker, containerd & runc Liên Kết Thế Nào?",
                    Paragraphs = new List<string>
                    {
                        "In the early days, Docker was a monolithic daemon combining image building, network routing, storage management, and process execution into a single binary. To foster an open ecosystem, Docker and industry leaders founded the Open Container Initiative (OCI) in 2015, establishing two critical open standards: the OCI Image Specification and the OCI Runtime Specification.",
                        "Today, when you run 'docker run', execution flows through a decoupled architecture. The Docker CLI talks via REST API to the Docker Daemon (dockerd). The daemon delegates container lifecycle management to 'containerd' via gRPC. 'containerd' pulls the OCI image layers, unpacks them into an OCI bundle (rootfs + config.json), and invokes 'runc'.",
                        "'runc' is the lightweight reference CLI implementation of the OCI Runtime Spec. It executes the low-level Linux syscalls (clone, pivot_root, setns), attaches cgroups, starts the container process, and immediately exits. A tiny daemon called 'containerd-shim' remains to hold open standard I/O streams and collect the container's exit code without keeping the Docker daemon tied up."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Thời kỳ đầu, Docker là một khối daemon nguyên khối (Monolithic daemon) gộp chung mọi thứ từ build image, quản lý network, storage đến khởi chạy tiến trình. Để phá vỡ thế độc quyền và tiêu chuẩn hóa ngành công nghiệp, Docker cùng các ông lớn công nghệ đã thành lập OCI (Open Container Initiative) vào năm 2015 với 2 chuẩn mực cốt lõi: OCI Image Spec và OCI Runtime Spec.",
                        "Ngày nay, khi bạn gõ 'docker run', luồng xử lý diễn ra qua một kiến trúc tách rời cực kỳ tinh gọn. Docker CLI gửi lệnh qua REST API tới Docker Daemon (dockerd). Daemon này chuyển tiếp yêu cầu sang 'containerd' qua giao thức gRPC. 'containerd' tải các image layers chuẩn OCI, giải nén thành một thư mục OCI Bundle (gồm rootfs và file config.json) rồi triệu gọi 'runc'.",
                        "'runc' là công cụ thực thi tiêu chuẩn của OCI Runtime. Nó trực tiếp gọi các Linux syscalls (clone, pivot_root, setns), gắn cgroups, trao quyền cho ứng dụng rồi lập tức thoát ra. Một tiến trình siêu nhỏ mang tên 'containerd-shim' sẽ ở lại để duy trì đường ống I/O (stdout/stderr) và lắng nghe exit code của container mà không làm gián đoạn hệ thống khi daemon khởi động lại."
                    },
                    CodeTitle = "The Container Execution Chain & Direct containerd (ctr) Usage",
                    CodeLanguage = "bash",
                    CodeContent = """
# ==================== LUỒNG THỰC THI CHUẨN OCI ====================
# [Developer CLI] (docker run)
#       │ (REST / Unix Socket)
#       ▼
# [Docker Daemon] (dockerd)
#       │ (gRPC)
#       ▼
# [containerd] (Quản lý images, bundles, lifecycle)
#       │ (triệu gọi runc CLI)
#       ▼
# [runc] (Tương tác Linux Kernel Syscalls: clone, pivot_root)
#       │
#       ▼
# [containerd-shim] ──> [Tiến trình Ứng Dụng của bạn]

# ==================== THỰC HÀNH CHẠY CONTAINER VỚI CTR (CONTAINERD) ====================
# Trên các cụm Kubernetes hiện đại, Docker daemon đã bị gỡ bỏ, chỉ còn containerd:
# Tải OCI image trực tiếp bằng containerd:
sudo ctr images pull docker.io/library/alpine:latest

# Khởi chạy container trực tiếp qua containerd và runc:
sudo ctr run --rm -t docker.io/library/alpine:latest my-isolated-task sh
""",
                    CalloutType = "tip",
                    CalloutTitle = "Why Kubernetes Deprecated Dockershim",
                    CalloutTitleVi = "Vì Sao Kubernetes Khai Tử Dockershim?",
                    CalloutText = "Kubernetes communicates with container runtimes using the Container Runtime Interface (CRI). Because Docker was not CRI-compliant, K8s had to maintain a translation layer called 'dockershim'. In Kubernetes v1.24+, dockershim was permanently removed, allowing K8s to communicate directly with containerd or CRI-O, eliminating memory overhead and boosting pod startup speed.",
                    CalloutTextVi = "Kubernetes giao tiếp với các bộ thực thi qua chuẩn CRI (Container Runtime Interface). Vì Docker Engine không tuân theo chuẩn CRI, K8s từng phải duy trì một cầu nối trung gian gọi là 'dockershim'. Từ bản K8s v1.24+, dockershim bị loại bỏ hoàn toàn để K8s gọi thẳng vào containerd hoặc CRI-O, giúp tiết kiệm bộ nhớ và tăng tốc độ khởi tạo Pod."
                },
                new DevArticleSection
                {
                    Heading = "3. Production Packaging: Multi-Stage Builds, Distroless/Chisel & Security Hardening",
                    HeadingVi = "3. Đóng Gói Dockerfile Chuẩn Production: Multi-Stage Build, Chisel & Bảo Mật Non-Root",
                    Paragraphs = new List<string>
                    {
                        "Writing a naive Dockerfile (e.g. copying source code and running build tools inside the final runtime image) is an anti-pattern. The resulting image often bloats to 800MB - 1.5GB, containing compilers, package managers (apt, apk), shells (bash, sh), and dozens of critical CVE vulnerabilities that malicious actors can exploit.",
                        "Production Dockerfiles must leverage Multi-Stage Builds: Phase 1 utilizes a heavy SDK image to compile binaries and run tests; Phase 2 copies only the compiled output into an ultra-minimal runtime base image such as Google's Distroless or Ubuntu Chisel.",
                        "By stripping away the shell, curl, package managers, and running as an unprivileged user (UID 10001), your final container size shrinks to under 50MB, attack surface drops to near zero, and container start times become virtually instantaneous."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Viết Dockerfile ngây thơ (copy toàn bộ mã nguồn và chạy lệnh build bên trong cùng một image vận hành) là một sai lầm phổ biến. Kích thước image sinh ra thường phình to từ 800MB đến 1.5GB, chứa đầy bộ biên dịch, công cụ quản lý gói (apt, apk), shell (bash, sh) và hàng chục lỗ hổng bảo mật CVE nguy hiểm.",
                        "Dockerfile chuẩn Production bắt buộc phải sử dụng kỹ thuật Đa tầng (Multi-Stage Build): Giai đoạn 1 sử dụng Base Image SDK đầy đủ để biên dịch mã nguồn và chạy unit tests; Giai đoạn 2 chỉ copy file nhị phân (binary) đã đóng gói sang một Base Image Runtime tối giản như Google Distroless hoặc Ubuntu Chisel.",
                        "Nhờ việc loại bỏ hoàn toàn shell, curl, trình quản lý gói và thiết lập quyền người dùng không đặc quyền (UID 10001), kích thước image giảm xuống dưới 50MB, triệt tiêu gần như 100% bề mặt tấn công của hacker và giúp container khởi động tức thì."
                    },
                    CodeTitle = "Hardened Multi-Stage Dockerfile with Non-Root User & Resource Constraints",
                    CodeLanguage = "dockerfile",
                    CodeContent = """
# =========================================================================
# GIAI ĐOẠN 1: BUILD & COMPILE (Sử dụng SDK Image đầy đủ công cụ)
# =========================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

# Tối ưu hóa Docker Layer Cache: Copy file project và restore trước
COPY ["OrderService.csproj", "./"]
RUN dotnet restore "OrderService.csproj"

# Copy toàn bộ mã nguồn và biên dịch Release
COPY . .
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =========================================================================
# GIAI ĐOẠN 2: RUNTIME CHUẨN PRODUCTION (Siêu nhẹ, bảo mật, non-root)
# =========================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS final
WORKDIR /app

# Tạo biến môi trường chuẩn Production
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    ASPNETCORE_URLS=http://+:8080

# Chisel image đã tự động loại bỏ bash, apt và chạy dưới user UID 1654 (non-root)
USER $APP_UID

# Chỉ copy file nhị phân đã build từ Giai đoạn 1
COPY --from=build-env --chown=$APP_UID:$APP_UID /app/publish .

# Mở cổng giao tiếp không đặc quyền (> 1024)
EXPOSE 8080

# Khởi chạy ứng dụng
ENTRYPOINT ["dotnet", "OrderService.dll"]
""",
                    CalloutType = "tip",
                    CalloutTitle = "Enforce CPU and Memory Constraints in Production",
                    CalloutTitleVi = "Bắt Buộc Giới Hạn Tài Nguyên Khi Khởi Chạy Container",
                    CalloutText = "Never run a container without resource constraints. A memory leak in one unconstrained container can consume the entire host's RAM, triggering the Linux kernel OOM Killer to terminate critical system services. Use 'docker run --memory=512m --cpus=1.5' or Kubernetes resource limits.",
                    CalloutTextVi = "Tuyệt đối không chạy container mà không giới hạn tài nguyên. Một lỗi rò rỉ bộ nhớ ở một container vô danh có thể ngốn cạn kiệt RAM của máy chủ, khiến Kernel kích hoạt OOM Killer tiêu diệt các dịch vụ trọng yếu của hệ thống. Luôn dùng 'docker run --memory=512m --cpus=1.5' hoặc Kubernetes Resource Limits."
                }
            }
        };
        series.Articles.Add(article1);

        // =========================================================================
        // BÀI 2: ORCHESTRATION: KUBERNETES VS DOCKER SWARM
        // =========================================================================
        var article2 = new DevArticle
        {
            Id = "infra-tools-article-2-orchestration-k8s-vs-docker-swarm",
            SeriesId = series.Id,
            LessonNumber = "Bài 2",
            Level = "Intermediate",
            LevelVi = "Trung cấp",
            LevelOrder = 2,
            Title = "Container Orchestration: Kubernetes (K8s) vs Docker Swarm – Architectural Breakdown & Production Strategy",
            TitleVi = "Orchestration & Điều Phối Cụm: Kubernetes (K8s) vs Docker Swarm – So Sánh Kiến Trúc & Chiến Lược Vận Hành",
            ReadingTime = "18 phút đọc",
            PublishedDate = "25/10/2026",
            Summary = "Moving from a single host to a distributed container cluster. Compare the architectural philosophies of Kubernetes (industry standard for complex, hyperscale workloads) versus Docker Swarm (lightweight, zero-learning-curve native clustering). Master core K8s objects (Pods, Deployments, Services, HPA) alongside Swarm stacks and learn how to make the right choice.",
            SummaryVi = "Bước chuyển dịch từ máy chủ đơn lẻ lên cụm phân tán nhiều Node. So sánh chi tiết triết lý kiến trúc giữa Kubernetes (chuẩn công nghiệp cho hệ thống quy mô lớn, tính mở rộng vô hạn) và Docker Swarm (nhẹ, tích hợp sẵn, dễ vận hành cho team vừa và nhỏ). Làm chủ các thành phần cốt lõi của K8s (Pod, Deployment, Service, HPA) song song với Docker Swarm Stacks và chiến lược lựa chọn nền tảng tối ưu chi phí.",
            Tags = new List<string> { "Kubernetes", "Docker Swarm", "Orchestration", "Cluster", "Deployment", "HPA", "High Availability" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Orchestration Dilemma: Architectural Philosophies of K8s vs Swarm",
                    HeadingVi = "1. Cuộc Chiến Orchestration: Triết Lý Kiến Trúc K8s vs Docker Swarm",
                    Paragraphs = new List<string>
                    {
                        "When your application outgrows a single virtual machine, running independent 'docker run' commands across 10 servers becomes unmanageable. If a host crashes at 3:00 AM, who detects the failure, reschedules 15 containers onto surviving servers, updates load balancers, and maintains zero downtime? This is the mission of Container Orchestration.",
                        "Kubernetes (K8s) was designed by Google based on 15 years of operating the Borg system. It features an extensive, distributed Control Plane: 'etcd' provides a distributed transactional key-value store with Raft consensus; 'kube-apiserver' validates and configures state; 'kube-scheduler' assigns pods to nodes based on resource topology; 'kube-controller-manager' regulates desired states. On worker nodes, 'kubelet' manages container runtimes and 'kube-proxy' manages IP tables.",
                        "Docker Swarm, conversely, takes a minimalist, batteries-included approach built directly into the Docker Engine. It requires zero external tools or complex manifests: a single command 'docker swarm init' turns an engine into a Manager Node. Swarm Manager nodes maintain internal Raft consensus without an external database, orchestrating Worker Nodes through the familiar Docker CLI."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khi hệ thống vượt khỏi quy mô một máy chủ, việc gõ lệnh 'docker run' thủ công rải rác trên 10 server trở thành thảm họa vận hành. Nếu một server đột ngột cháy nguồn lúc 3 giờ sáng, ai sẽ phát hiện sự cố, tự động di dời 15 container sang các server còn sống, cập nhật bảng định tuyến cân bằng tải và duy trì tính liên tục của dịch vụ? Đó chính là sứ mệnh của Container Orchestration (Điều phối cụm).",
                        "Kubernetes (K8s) được Google thiết kế dựa trên hơn 15 năm kinh nghiệm vận hành hệ thống Borg huyền thoại. Nó sở hữu Control Plane phân tán hùng mạnh: 'etcd' đóng vai trò cơ sở dữ liệu key-value phân tán đạt đồng thuận Raft; 'kube-apiserver' tiếp nhận mọi yêu cầu; 'kube-scheduler' tính toán vị trí đặt Pod tối ưu; 'kube-controller-manager' giám sát trạng thái mong muốn. Ở các Worker Nodes, 'kubelet' chỉ huy runtime và 'kube-proxy' điều phối mạng.",
                        "Ngược lại hoàn toàn, Docker Swarm đi theo triết lý tối giản, tích hợp sẵn ngay trong Docker Engine. Bạn không cần cài đặt thêm bất kỳ phần mềm phức tạp nào: chỉ một lệnh 'docker swarm init' là biến máy chủ thành Manager Node. Các Manager Node tự duy trì thuật toán đồng thuận Raft nội bộ mà không cần cơ sở dữ liệu ngoài, điều phối toàn bộ Worker Nodes thông qua các lệnh quen thuộc của Docker."
                    },
                    CodeTitle = "Architecture Comparison Matrix: Kubernetes vs Docker Swarm",
                    CodeLanguage = "json",
                    CodeContent = """
{
  "Criteria": "So Sánh Kubernetes vs Docker Swarm",
  "Metrics": [
    {
      "Feature": "Độ phức tạp cài đặt & vận hành",
      "DockerSwarm": "Cực kỳ thấp (Tích hợp sẵn trong Docker, 5 phút là có cụm)",
      "Kubernetes": "Rất cao (Đòi hỏi chuyên môn DevOps, etcd, chứng chỉ TLS, CNI network)"
    },
    {
      "Feature": "Tiêu hao tài nguyên Control Plane",
      "DockerSwarm": "Siêu nhẹ (~100MB RAM cho Manager node)",
      "Kubernetes": "Khá nặng (Yêu cầu tối thiểu 2-4GB RAM cho Master/Control Plane)"
    },
    {
      "Feature": "Cơ chế tự động mở rộng (Autoscaling)",
      "DockerSwarm": "Thủ công (docker service scale) hoặc cần script bên thứ ba",
      "Kubernetes": "Tự động đỉnh cao với HPA (CPU/RAM/Custom metrics) và VPA"
    },
    {
      "Feature": "Quy mô phù hợp (Target Scale)",
      "DockerSwarm": "1 đến 30 Nodes, đội ngũ kỹ thuật nhỏ, ngân sách tối ưu",
      "Kubernetes": "Từ hàng chục đến 5000+ Nodes, hệ sinh thái Cloud-Native lớn"
    }
  ]
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "When to Choose Which in Real Projects",
                    CalloutTitleVi = "Chiến Lược Lựa Chọn Thực Chiến: Swarm Hay K8s?",
                    CalloutText = "If you have a team of 3-10 engineers without dedicated platform/SRE engineers and your infrastructure consists of 2-15 VPS instances, Docker Swarm provides 80% of orchestration benefits with 5% of K8s operational complexity. Choose Kubernetes when you require automatic autoscaling (HPA), GitOps (ArgoCD), multi-cloud portability, or manage hundreds of microservices.",
                    CalloutTextVi = "Nếu bạn có một đội ngũ 3-10 kỹ sư không có DevOps/SRE chuyên trách và cụm máy chủ chỉ từ 2-15 VPS, Docker Swarm mang lại 80% sức mạnh điều phối với chỉ 5% độ phức tạp của K8s. Hãy chọn Kubernetes khi bạn bắt buộc phải có tính năng tự động co giãn theo tải (HPA), quy trình GitOps (ArgoCD) hoặc quản lý hàng trăm dịch vụ quy mô lớn."
                },
                new DevArticleSection
                {
                    Heading = "2. Core Kubernetes Architecture: Pods, Deployments, Services & Ingress",
                    HeadingVi = "2. Các Khối Hạt Nhân Của Kubernetes: Pod, Deployment, Service & Ingress",
                    Paragraphs = new List<string>
                    {
                        "Kubernetes does not run containers directly; it wraps them in an abstraction called a 'Pod'. A Pod is the atomic unit of deployment in K8s, representing one or more containers that share the same network namespace (same IP address and port space) and storage volumes.",
                        "To manage Pods reliably, K8s provides higher-level declarative controllers. A 'Deployment' manages a 'ReplicaSet', declaring how many identical replicas of a pod should run. Deployments handle zero-downtime rolling updates, pause/resume rollouts, and automatic rollbacks if new versions crash.",
                        "Because Pods are ephemeral with dynamic IP addresses that change on restart, the 'Service' abstraction provides a stable virtual IP (ClusterIP) and internal DNS entry. Services load-balance network traffic across matching healthy pods using label selectors."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Kubernetes không bao giờ chạy trực tiếp từng container đơn lẻ; nó bọc container trong một đối tượng cơ sở gọi là 'Pod'. Pod là đơn vị triển khai nguyên tử nhỏ nhất trong K8s, đại diện cho một hoặc nhiều container cùng chia sẻ một không gian mạng (chung một địa chỉ IP và dải cổng Port) cũng như chia sẻ ổ đĩa lưu trữ (Volumes).",
                        "Để quản lý các Pod này một cách bền vững, K8s cung cấp các bộ điều khiển khai báo cấp cao. 'Deployment' quản lý 'ReplicaSet', định nghĩa số lượng bản sao mong muốn. Deployment chịu trách nhiệm cập nhật phiên bản không gián đoạn (Rolling Update), tạm dừng hoặc hoàn tác tự động (Rollback) nếu phiên bản mới gặp sự cố CrashLoopBackOff.",
                        "Vì các Pod có tính chất tạm thời (ephemeral) với IP thay đổi liên tục mỗi khi khởi động lại, đối tượng 'Service' ra đời để cung cấp một IP ảo cố định (ClusterIP) và tên miền DNS nội bộ ổn định. Service tự động cân bằng tải lưu lượng truy cập tới các Pod khỏe mạnh thông qua cơ chế Label Selector."
                    },
                    CodeTitle = "Production Kubernetes Manifest: Deployment, Service & Resource Limits",
                    CodeLanguage = "yaml",
                    CodeContent = """
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-service-deployment
  namespace: production
  labels:
    app: order-service
spec:
  replicas: 3 # Luôn duy trì 3 Pods trên các Node khác nhau
  selector:
    matchLabels:
      app: order-service
  template:
    metadata:
      labels:
        app: order-service
    spec:
      containers:
      - name: order-service
        image: myregistry.azurecr.io/order-service:v2.1.0
        ports:
        - containerPort: 8080
        resources: # BẮT BUỘC để ngăn ngừa OOMKilled trên Worker Node
          requests:
            memory: "256Mi"
            cpu: "200m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        readinessProbe: # K8s chỉ chuyển traffic khi Pod đã sẵn sàng
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 10
        livenessProbe: # Tự động restart Pod nếu ứng dụng bị treo (deadlock)
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 15
          periodSeconds: 20
---
apiVersion: v1
kind: Service
metadata:
  name: order-service-clusterip
  namespace: production
spec:
  type: ClusterIP
  selector:
    app: order-service
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
""",
                    CalloutType = "warning",
                    CalloutTitle = "Never Omit Resource Limits in Kubernetes",
                    CalloutTitleVi = "Cảnh Báo: Tuyệt Đối Không Bỏ Quên Requests và Limits",
                    CalloutText = "Deploying a Pod without 'resources.limits' allows an uncapped memory spike in that pod to starve neighboring pods on the same Kubernetes worker node. When node RAM exhausts, the Linux kernel OOM Killer terminates pods indiscriminately, causing cascading cluster instability.",
                    CalloutTextVi = "Triển khai Pod mà không cấu hình 'resources.limits' sẽ khiến một Pod bị tràn bộ nhớ có thể nuốt trọn toàn bộ RAM của Worker Node. Khi máy chủ cạn RAM, Linux OOM Killer sẽ tiêu diệt ngẫu nhiên các Pod lân cận, gây ra phản ứng dây chuyền làm sập toàn cụm K8s."
                },
                new DevArticleSection
                {
                    Heading = "3. Practical Deployment: Docker Swarm Stack vs Kubernetes HPA Manifests",
                    HeadingVi = "3. Thực Chiến So Sánh: Docker Swarm Stack Deploy vs Kubernetes Manifests",
                    Paragraphs = new List<string>
                    {
                        "Let's see how high-availability multi-replica microservices are deployed in both environments. Docker Swarm uses standard Compose syntax enriched with the 'deploy:' key, defining placement constraints (e.g. node.role == worker) and rolling update parameters (update_config with parallelism and delay).",
                        "In Kubernetes, deployments are orchestrated alongside Horizontal Pod Autoscalers (HPA). An HPA controller continuously queries metrics APIs to dynamically scale pod count between minReplicas (e.g. 3) and maxReplicas (e.g. 20) when average CPU or custom request latency thresholds are crossed."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Hãy cùng so sánh cách triển khai một dịch vụ có khả năng chịu lỗi cao trên cả 2 nền tảng. Docker Swarm sử dụng trực tiếp cú pháp file Compose được mở rộng với khối 'deploy:', cho phép định nghĩa các ràng buộc vị trí đặt (placement constraints ví dụ: chỉ chạy trên worker node) cùng cấu hình cập nhật cuốn chiếu (parallelism: 2, delay: 10s).",
                        "Trong khi đó, Kubernetes kết hợp Deployment với Horizontal Pod Autoscaler (HPA). Bộ điều khiển HPA liên tục truy vấn API Metrics Server để tự động co giãn số lượng Pod linh hoạt từ tối thiểu (ví dụ 3 Pods) lên tối đa (ví dụ 20 Pods) ngay khi mức tiêu thụ CPU trung bình vượt ngưỡng 75%."
                    },
                    CodeTitle = "Docker Swarm Stack File vs Kubernetes Horizontal Pod Autoscaler",
                    CodeLanguage = "yaml",
                    CodeContent = """
# ==================== CÁCH 1: DOCKER SWARM (docker-stack.yml) ====================
# Triển khai bằng lệnh: docker stack deploy -c docker-stack.yml ecom-stack
version: '3.8'
services:
  order-service:
    image: myregistry.azurecr.io/order-service:v2.1.0
    ports:
      - "8080:8080"
    deploy:
      replicas: 4 # Phân bổ 4 bản sao đều khắp cụm Swarm
      update_config:
        parallelism: 1       # Cập nhật từng container một để không gián đoạn
        delay: 10s           # Chờ 10 giây giữa các đợt cập nhật
        failure_action: rollback # Tự động quay về phiên bản cũ nếu lỗi
      restart_policy:
        condition: on-failure
      placement:
        constraints:
          - node.role == worker # Chỉ chạy trên các máy Worker

# ==================== CÁCH 2: KUBERNETES AUTOSCALER (k8s-hpa.yaml) ====================
# Triển khai bằng lệnh: kubectl apply -f k8s-hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: order-service-hpa
  namespace: production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: order-service-deployment
  minReplicas: 3  # Luôn có tối thiểu 3 Pods trong giờ thấp điểm
  maxReplicas: 15 # Tự động mở rộng lên tới 15 Pods trong đợt siêu khuyến mãi Flash Sale
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 75 # Khi CPU trung bình vượt 75%, tự động nhân thêm Pod!
""",
                    CalloutType = "tip",
                    CalloutTitle = "Zero-Downtime Rolling Update Parameters",
                    CalloutTitleVi = "Bí Quyết Cập Nhật Zero-Downtime Chuẩn Chỉ",
                    CalloutText = "In Kubernetes, configure 'maxSurge: 25%' and 'maxUnavailable: 0'. This ensures Kubernetes will always spin up and verify new healthy pods before terminating even a single old pod, guaranteeing 100% uninterrupted availability during production deployments.",
                    CalloutTextVi = "Trong Kubernetes, hãy cấu hình 'maxSurge: 25%' và 'maxUnavailable: 0' trong phần rollingUpdate. Điều này đảm bảo K8s luôn tạo Pod mới và kiểm tra sức khỏe thành công trước khi xóa bất kỳ một Pod cũ nào, duy trì dịch vụ 100% không gián đoạn."
                }
            }
        };
        series.Articles.Add(article2);

        // =========================================================================
        // BÀI 3: API GATEWAY & REVERSE PROXY: NGINX VS KONG
        // =========================================================================
        var article3 = new DevArticle
        {
            Id = "infra-tools-article-3-api-gateway-nginx-kong",
            SeriesId = series.Id,
            LessonNumber = "Bài 3",
            Level = "Intermediate",
            LevelVi = "Trung cấp",
            LevelOrder = 3,
            Title = "API Gateway & Ingress: NGINX vs Kong – Edge Traffic Routing, Load Balancing & Cloud-Native Security",
            TitleVi = "API Gateway & Ingress: NGINX & Kong – Cửa Ngõ Vào Ra, Điều Hướng Lưu Lượng & Bảo Vệ Toàn Diện Microservices",
            ReadingTime = "17 phút đọc",
            PublishedDate = "25/10/2026",
            Summary = "Master the frontline of modern architectures. Understand the critical distinction between a high-performance Reverse Proxy (NGINX) and an extensible Cloud-Native API Gateway (Kong). Dive into NGINX event-driven architecture, upstream load balancing, and SSL offloading. Then explore Kong's Lua/OpenResty foundation, DB-less declarative routing (decK), and enterprise plugins (Rate Limiting, JWT Auth, Prometheus).",
            SummaryVi = "Làm chủ phòng tuyến tiền tuyến của mọi kiến trúc hệ thống hiện đại. Phân biệt rạch ròi giữa một Reverse Proxy hiệu năng siêu việt (NGINX) và một API Gateway Cloud-Native mở rộng cao cấp (Kong). Giải phẫu kiến trúc hướng sự kiện bất đồng bộ của NGINX, kỹ thuật upstream load balancing và offload SSL/TLS. Tiếp đó khám phá nền tảng OpenResty của Kong, cấu hình Declarative không cơ sở dữ liệu (DB-less) và hệ thống Plugin thực chiến (Rate Limiting chống DDoS, JWT Authentication, Prometheus Metrics).",
            Tags = new List<string> { "NGINX", "Kong", "API Gateway", "Reverse Proxy", "Load Balancing", "Rate Limiting", "Ingress Controller" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Strategic Role of Gateways: Reverse Proxy vs Full-Featured API Gateway",
                    HeadingVi = "1. Vì Sao Microservices Bắt Buộc Phải Có Reverse Proxy Hoặc API Gateway?",
                    Paragraphs = new List<string>
                    {
                        "Exposing internal microservices directly to the public internet is an architectural catastrophe. If a client app connects directly to 30 microservices, internal IP addresses are exposed, CORS must be configured in 30 places, SSL certificates must be rotated across 30 services, and malicious clients can overwhelm backend databases with unthrottled requests.",
                        "An Edge Gateway provides a Single Point of Ingress. It handles SSL/TLS Termination, L4 (TCP/UDP) and L7 (HTTP/gRPC) path-based routing, Centralized Authentication, Rate Limiting, and hides the internal network topology.",
                        "However, engineers must distinguish between a Reverse Proxy and an API Gateway. A Reverse Proxy (like NGINX) focuses on high-throughput network plumbing: static file serving, SSL offloading, and upstream connection pooling. An API Gateway (like Kong) operates at the API lifecycle layer: dynamic consumer authentication, per-client rate limits, request/response transformations, and real-time observability plugins."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Để các dịch vụ Microservices nội bộ giao tiếp trực tiếp với môi trường Internet công cộng là một thảm họa kiến trúc. Nếu ứng dụng di động của người dùng kết nối thẳng tới 30 microservice khác nhau: toàn bộ IP mạng nội bộ bị lộ, cấu hình CORS phải viết lại ở 30 nơi, chứng chỉ SSL phải cài đặt trên 30 dịch vụ, và kẻ tấn công có thể dễ dàng dập tắt cơ sở dữ liệu backend bằng các đợt spam request liên tục.",
                        "Hạ tầng Cửa ngõ (Edge Gateway) giải quyết triệt để điều này bằng cách tạo ra một Điểm tiếp nhận duy nhất (Single Point of Ingress). Nó chịu trách nhiệm giải mã chứng chỉ SSL/TLS, định tuyến lưu lượng tầng L4 (TCP/UDP) và L7 (HTTP/gRPC) theo đường dẫn, xác thực tập trung, giới hạn tần suất gọi (Rate Limiting) và che giấu hoàn toàn mạng nội bộ.",
                        "Dù vậy, cần phân biệt rõ giữa Reverse Proxy và API Gateway. Một Reverse Proxy thuần túy (như NGINX) tập trung tối đa vào hiệu năng mạng: phục vụ file tĩnh, offload SSL và gộp kết nối upstream. Trong khi đó, một API Gateway chuyên dụng (như Kong) vận hành ở tầng nghiệp vụ API: phân quyền theo từng khách hàng (Consumer), giới hạn hạn ngạch gọi API động, biến đổi dữ liệu request/response và gắn các plugin đo lường thời gian thực."
                    },
                    CodeTitle = "Architectural Comparison: NGINX vs Kong API Gateway",
                    CodeLanguage = "json",
                    CodeContent = """
{
  "Comparison": "NGINX vs Kong API Gateway",
  "KeyDifferences": [
    {
      "Dimension": "Nền tảng cốt lõi (Core Engine)",
      "NGINX": "C thuần túy, hướng sự kiện bất đồng bộ (epoll), hiệu năng thô đỉnh cao",
      "Kong": "Xây dựng trên NGINX + OpenResty (LuaJIT) + Plugin Framework"
    },
    {
      "Dimension": "Quản lý cấu hình (Configuration)",
      "NGINX": "File văn bản nginx.conf tĩnh (cần nginx -s reload khi đổi)",
      "Kong": "Động 100%: Quản lý qua Admin REST API hoặc Declarative YAML (decK)"
    },
    {
      "Dimension": "Hệ sinh thái Plugin mở rộng",
      "NGINX": "Cần biên dịch lại C module hoặc dùng NGINX Plus trả phí",
      "Kong": "Kho Plugin khổng lồ: Rate-limiting, JWT, OAuth2, Prometheus, CORS, v.v."
    },
    {
      "Dimension": "Trường hợp sử dụng lý tưởng",
      "NGINX": "Cân bằng tải cụm máy chủ, Reverse Proxy, Ingress Controller cơ bản, Web server",
      "Kong": "API Gateway doanh nghiệp, quản lý hàng trăm API, phân quyền Consumer đa tầng"
    }
  ]
}
""",
                    CalloutType = "warning",
                    CalloutTitle = "Beware of Smart Gateways, Dumb Pipes Anti-Pattern",
                    CalloutTitleVi = "Cảnh Báo: Tuyệt Đối Không Viết Business Logic Vào Gateway",
                    CalloutText = "An API Gateway should handle cross-cutting infrastructure concerns (TLS, Auth, Rate Limiting, Logging). Never implement complex business logic, database queries, or data merging inside the Gateway. Doing so turns the gateway into an unmaintainable distributed monolith bottleneck.",
                    CalloutTextVi = "API Gateway chỉ nên xử lý các mối quan tâm hạ tầng dùng chung (TLS, Auth, Rate Limiting, Logging). Tuyệt đối không nhồi nhét nghiệp vụ kinh doanh, truy vấn cơ sở dữ liệu hay tính toán phức tạp vào Gateway. Làm vậy sẽ biến Gateway thành một khối Monolith thắt nút cổ chai khó bảo trì."
                },
                new DevArticleSection
                {
                    Heading = "2. NGINX Under the Hood: Event-Driven Core & Upstream Load Balancing",
                    HeadingVi = "2. NGINX Cốt Lõi: Cấu Trúc Hướng Sự Kiện, Upstream Load Balancing & Ingress K8s",
                    Paragraphs = new List<string>
                    {
                        "NGINX's legendary performance stems from its asynchronous, event-driven, non-blocking architecture. Traditional web servers (like classic Apache) spawned a new thread or process for each incoming HTTP connection, causing context-switching overhead and memory exhaustion at high concurrency.",
                        "NGINX uses a single Master process and a small number of Worker processes (typically one per CPU core). Each Worker process utilizes kernel multiplexing mechanisms like 'epoll' (Linux) or 'kqueue' (BSD) to process tens of thousands of simultaneous connections within a single thread without blocking.",
                        "In microservice environments, NGINX acts as a battle-tested load balancer. It supports multiple upstream distribution algorithms: Round Robin (default), Least Connections ('least_conn' for long-running transactions), and IP Hash ('ip_hash' for stateful sessions). When configured with 'keepalive', NGINX reuses TCP connections to backend services, slashing latency."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Hiệu năng huyền thoại của NGINX bắt nguồn từ kiến trúc hướng sự kiện bất đồng bộ không chặn (Asynchronous Event-Driven Non-blocking). Các web server truyền thống (như Apache đời cũ) tạo một thread hoặc process riêng cho mỗi kết nối HTTP gửi đến, dẫn đến chi phí chuyển đổi ngữ cảnh (Context Switching) khổng lồ và làm sập RAM khi có hàng vạn người truy cập đồng thời.",
                        "NGINX sử dụng một Master process duy nhất và một số lượng Worker processes rất nhỏ (thông thường bằng đúng số lõi CPU của máy chủ). Mỗi Worker process tận dụng cơ chế dồn kênh của Kernel như 'epoll' (trên Linux) để xử lý đồng thời hàng chục ngàn kết nối song song trong một luồng duy nhất mà không bao giờ bị nghẽn.",
                        "Trong môi trường Microservices, NGINX là bộ cân bằng tải hàng đầu. Nó hỗ trợ nhiều thuật toán phân bổ tải upstream: Round Robin (mặc định xoay vòng), Least Connections ('least_conn' ưu tiên server đang rảnh rỗi nhất) và IP Hash ('ip_hash' duy trì phiên). Khi được cấu hình 'keepalive', NGINX tái sử dụng các kết nối TCP tới backend, triệt tiêu độ trễ bắt tay mạng (TCP Handshake)."
                    },
                    CodeTitle = "Production nginx.conf: Upstream Keepalive, Rate Limiting & SSL Offloading",
                    CodeLanguage = "nginx",
                    CodeContent = """
events {
    worker_connections 10240; # Mỗi worker có thể giữ tới 10,240 kết nối đồng thời
    multi_accept on;
    use epoll;
}

http {
    # 1. Định nghĩa bộ đệm Rate Limiting chống spam/DDoS (10 requests/giây theo IP)
    limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;

    # 2. Cấu hình Upstream Pool với cơ chế Keepalive tái sử dụng TCP socket
    upstream backend_order_cluster {
        least_conn; # Chuyển request tới instance đang có ít kết nối nhất
        server order-service-1:8080 max_fails=3 fail_timeout=10s;
        server order-service-2:8080 max_fails=3 fail_timeout=10s;
        server order-service-3:8080 max_fails=3 fail_timeout=10s;
        keepalive 64; # Giữ 64 kết nối TCP nhàn rỗi sẵn sàng phục vụ
    }

    server {
        listen 443 ssl http2;
        server_name api.mycompany.com;

        # Cấu hình SSL/TLS Hardening
        ssl_certificate /etc/nginx/certs/fullchain.pem;
        ssl_certificate_key /etc/nginx/certs/privkey.pem;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;

        location /api/v1/orders/ {
            # Kích hoạt Rate Limit với burst buffer lên tới 20 req
            limit_req zone=api_limit burst=20 nodelay;

            proxy_pass http://backend_order_cluster;
            proxy_http_version 1.1;
            proxy_set_header Connection ""; # Cho phép keepalive upstream hoạt động

            # Bảo toàn thông tin Client thực tế gửi tới Microservice
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;

            proxy_connect_timeout 5s;
            proxy_read_timeout 30s;
        }
    }
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Always Preserve Client IP Headers",
                    CalloutTitleVi = "Bí Quyết: Không Bao Giờ Làm Mất IP Thật Của Khách Hàng",
                    CalloutText = "When traffic passes through NGINX, backend microservices see NGINX's IP address rather than the end-user's IP. Always pass 'proxy_set_header X-Real-IP $remote_addr;' and 'proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;' so downstream audit logs and fraud detection filters retain real client provenance.",
                    CalloutTextVi = "Khi lưu lượng đi qua NGINX, các microservice backend sẽ chỉ nhìn thấy IP nội bộ của NGINX. Luôn thiết lập 'proxy_set_header X-Real-IP $remote_addr;' và 'proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;' để hệ thống ghi log và các bộ lọc chống gian lận bảo toàn được IP thật của người dùng."
                },
                new DevArticleSection
                {
                    Heading = "3. Kong API Gateway: OpenResty Engine, DB-less Mode & Enterprise Plugins",
                    HeadingVi = "3. Kong API Gateway: Nền Tảng OpenResty, DB-less Mode & Hệ Sinh Thái Plugin Thực Chiến",
                    Paragraphs = new List<string>
                    {
                        "Kong builds directly on top of NGINX and OpenResty, embedding LuaJIT into NGINX worker processes. This architecture gives Kong the raw, blistering execution speed of NGINX while providing the dynamic programmability needed for modern cloud-native API management.",
                        "Kong can run in two modes: with a PostgreSQL database backing its cluster configuration, or in modern 'DB-less mode' using declarative YAML configuration files (managed via decK or GitOps pipelines). DB-less mode is especially beloved in Kubernetes environments because it eliminates database operational dependencies, prevents configuration drift, and restarts in sub-seconds.",
                        "Kong's true superpower lies in its Plugin architecture. Rather than writing custom authentication, rate limiting, and observability logic in every microservice, you configure plugins at the Service, Route, or Global scope: 'key-auth', 'jwt', 'rate-limiting', 'request-transformer', and 'prometheus'."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Kong được xây dựng trực tiếp trên nền tảng NGINX và OpenResty, nhúng trình thông dịch LuaJIT vào các worker process của NGINX. Kiến trúc này mang lại cho Kong cả hai ưu điểm tối thượng: tốc độ truyền tải mạng cực nhanh của NGINX kết hợp cùng khả năng lập trình động linh hoạt của ngôn ngữ Lua.",
                        "Kong hỗ trợ 2 chế độ vận hành: Chế độ có cơ sở dữ liệu (sử dụng PostgreSQL lưu trữ cấu hình cluster) hoặc Chế độ Declarative không cơ sở dữ liệu (DB-less mode) sử dụng file YAML (quản lý qua công cụ decK hoặc GitOps CI/CD). Chế độ DB-less cực kỳ được ưa chuộng trên Kubernetes vì loại bỏ hoàn toàn gánh nặng vận hành database, chống sai lệch cấu hình và khởi động chỉ trong chớp mắt.",
                        "Sức mạnh thực sự của Kong nằm ở Hệ sinh thái Plugin. Thay vì phải tự viết code xác thực token, chống DDoS và xuất metrics trong từng microservice, bạn chỉ cần bật các Plugin ở tầng Gateway: 'key-auth', 'jwt', 'rate-limiting', 'request-transformer' và 'prometheus'."
                    },
                    CodeTitle = "Declarative Kong Configuration (kong.yml) with Rate Limiting & Auth Plugins",
                    CodeLanguage = "yaml",
                    CodeContent = """
# ==================== KONG DECLARATIVE CONFIGURATION (DB-LESS) ====================
_format_version: "3.0"

# 1. Định nghĩa các Backend Microservices (Upstream Services)
services:
  - name: order-microservice
    url: http://order-service:8080/api/v1
    connect_timeout: 5000
    read_timeout: 30000
    # 2. Định nghĩa các đường dẫn công khai (Routes)
    routes:
      - name: order-routes
        paths:
          - /orders
        strip_path: true
        protocols:
          - http
          - https
    # 3. Gắn các Plugin chuyên dụng bảo vệ Service này
    plugins:
      # Plugin Giới hạn tần suất gọi chống spam/DDoS
      - name: rate-limiting
        config:
          minute: 60       # Tối đa 60 requests / phút
          hour: 2000       # Tối đa 2000 requests / giờ
          policy: local    # Lưu bộ đếm trong bộ nhớ của Kong node
          hide_client_headers: false # Trả về X-RateLimit-Remaining cho client biết

      # Plugin Xác thực API Key trước khi cho phép vào backend
      - name: key-auth
        config:
          key_names:
            - apikey
            - X-API-Key

# 4. Định nghĩa Khách Hàng (Consumers) và cấp phát API Key
consumers:
  - username: mobile-app-partner
    keyauth_credentials:
      - key: sec_live_98a7sd8f7a6sd5f87as6df5 # API Key hợp lệ
""",
                    CalloutType = "tip",
                    CalloutTitle = "Run Kong in DB-Less Mode with Docker",
                    CalloutTitleVi = "Khởi Chạy Kong Gateway DB-Less Siêu Nhanh Bằng Docker",
                    CalloutText = "You can run a production-ready Kong API Gateway using Docker with a single command without installing PostgreSQL: 'docker run -d --name kong -e KONG_DATABASE=off -e KONG_DECLARATIVE_CONFIG=/kong/kong.yml -v ./kong.yml:/kong/kong.yml -p 8000:8000 -p 8001:8001 kong:latest'.",
                    CalloutTextVi = "Bạn có thể khởi chạy Kong API Gateway chế độ DB-less bằng một container Docker duy nhất mà không cần cài đặt PostgreSQL: 'docker run -d --name kong -e KONG_DATABASE=off -e KONG_DECLARATIVE_CONFIG=/kong/kong.yml -v ./kong.yml:/kong/kong.yml -p 8000:8000 -p 8001:8001 kong:latest'."
                }
            }
        };
        series.Articles.Add(article3);

        // =========================================================================
        // BÀI 4: MESSAGE QUEUES & EVENT STREAMING: RABBITMQ VS KAFKA VS AWS SQS
        // =========================================================================
        var article4 = new DevArticle
        {
            Id = "infra-tools-article-4-message-queues-rabbitmq-kafka-sqs",
            SeriesId = series.Id,
            LessonNumber = "Bài 4",
            Level = "Advanced",
            LevelVi = "Nâng cao",
            LevelOrder = 4,
            Title = "Message Queues & Event Streaming: RabbitMQ vs Apache Kafka vs AWS SQS – Comprehensive Architectural Comparison",
            TitleVi = "Message Queue & Event Streaming: RabbitMQ vs Apache Kafka vs AWS SQS – So Sánh Toàn Diện & Lựa Chọn Chuẩn Hạ Tầng",
            ReadingTime = "20 phút đọc",
            PublishedDate = "25/10/2026",
            Summary = "Demystify asynchronous communication in distributed systems. Contrast the three foundational messaging paradigms: Smart Broker with Dumb Consumers (RabbitMQ / AMQP), Dumb Broker with Smart Consumers using Distributed Append-Only Logs (Apache Kafka), and Fully-Managed Cloud Queues (AWS SQS). Master Exchanges, Partitions, Consumer Groups, DLQs, and trade-off matrices.",
            SummaryVi = "Giải mã các cơ chế truyền thông điệp bất đồng bộ trong hệ thống phân tán. So sánh đối đầu 3 mô hình kiến trúc cốt lõi: Message Broker truyền thống có bộ định tuyến thông minh (RabbitMQ / AMQP), Nhật ký phân tán hiệu năng cực đại với Consumer tự quản lý vị trí đọc (Apache Kafka), và Hàng đợi đám mây Serverless không cần quản trị máy chủ (AWS SQS). Làm chủ Exchanges, Partitions, Consumer Groups, Dead Letter Queues (DLQ) và ma trận ra quyết định kiến trúc.",
            Tags = new List<string> { "RabbitMQ", "Apache Kafka", "AWS SQS", "Message Queue", "Event Streaming", "AMQP", "Dead Letter Queue" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. Three Messaging Paradigms: Traditional Broker vs Distributed Log vs Cloud Serverless",
                    HeadingVi = "1. Ba Triết Lý Nhắn Tin Khác Biệt: Broker Truyền Thống vs Distributed Log vs Cloud Serverless",
                    Paragraphs = new List<string>
                    {
                        "Synchronous HTTP REST communication binds microservices in tight temporal coupling: if Service A calls Service B, and Service B calls Service C, any slow database query or network hiccup in Service C immediately blocks threads across the entire chain, creating cascading failures. Asynchronous messaging decouples systems in both time and space, smoothing out traffic spikes through peak shaving.",
                        "However, distributed infrastructure utilizes three profoundly different messaging architectures:",
                        "1) Traditional Message Broker (RabbitMQ): Built on the AMQP standard with a 'Smart Broker, Dumb Consumer' philosophy. RabbitMQ actively routes messages using exchange rules, tracks acknowledgment per message, and deletes messages as soon as consumers acknowledge them.",
                        "2) Distributed Append-Only Commit Log (Apache Kafka): Built with a 'Dumb Broker, Smart Consumer' philosophy. Kafka does not treat messages as transient queue items; it appends them sequentially to immutable, partitioned commit logs on disk. Messages are retained for days or months, allowing consumers to read at their own pace and replay history arbitrarily.",
                        "3) Fully-Managed Cloud Serverless Queue (AWS SQS): Zero server maintenance, infinite auto-scaling, pay-per-request pricing, and native integration with cloud services like AWS Lambda, ECS, and S3."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Giao tiếp đồng bộ qua HTTP REST ràng buộc các microservice chặt chẽ về mặt thời gian: Nếu Dịch vụ A gọi Dịch vụ B, và Dịch vụ B lại gọi Dịch vụ C, thì bất kỳ một truy vấn chậm chạp hay sự cố mạng nào tại C cũng sẽ lập tức khóa chặt các luồng xử lý (threads) của toàn bộ chuỗi dịch vụ. Giao tiếp bất đồng bộ qua thông điệp (Asynchronous Messaging) giải phóng sự phụ thuộc thời gian và san phẳng các đỉnh tải đột biến (Peak Shaving).",
                        "Tuy nhiên, thế giới hạ tầng phân tán được định hình bởi 3 trường phái kiến trúc hoàn toàn khác biệt:",
                        "1) Message Broker Truyền Thống (RabbitMQ): Xây dựng trên chuẩn AMQP với triết lý 'Smart Broker, Dumb Consumer'. RabbitMQ chủ động định tuyến thông điệp qua các Exchange, theo dõi từng xác nhận (ACK), và tự động xóa tin nhắn ngay khi consumer xử lý xong.",
                        "2) Nhật Ký Phân Tán Bất Biến (Apache Kafka): Vận hành theo triết lý 'Dumb Broker, Smart Consumer'. Kafka không xem tin nhắn là các mục hàng đợi tạm thời; nó ghi nối tiếp thông điệp vào các file Commit Log phân tán trên ổ đĩa. Dữ liệu được lưu trữ nhiều ngày hoặc nhiều tháng, cho phép Consumer tự quản lý vị trí đọc (Offset) và tua lại (Replay) dữ liệu lịch sử bất cứ lúc nào.",
                        "3) Hàng Đợi Đám Mây Serverless (AWS SQS): Không cần cài đặt hay bảo trì máy chủ, tự động mở rộng không giới hạn, tính tiền theo số lượng request và tích hợp chặt chẽ với hệ sinh thái Cloud."
                    },
                    CodeTitle = "10-Point Decision Matrix: RabbitMQ vs Apache Kafka vs AWS SQS",
                    CodeLanguage = "json",
                    CodeContent = """
{
  "InfrastructureComparison": "RabbitMQ vs Apache Kafka vs AWS SQS",
  "Matrix": [
    {
      "Criteria": "Triết lý kiến trúc lõi",
      "RabbitMQ": "Queue-based (Xóa message sau khi ACK)",
      "ApacheKafka": "Append-Only Commit Log (Lưu trữ bền vững)",
      "AWSSQS": "Distributed Serverless Queue (Managed)"
    },
    {
      "Criteria": "Thông lượng (Throughput)",
      "RabbitMQ": "Trung bình (20K - 80K msg/giây)",
      "ApacheKafka": "Cực khủng (Hàng triệu event/giây)",
      "AWSSQS": "Gần như vô hạn (Tự động scale)"
    },
    {
      "Criteria": "Khả năng đọc lại dữ liệu (Replayability)",
      "RabbitMQ": "Không hỗ trợ (Đã đọc là xóa khỏi queue)",
      "ApacheKafka": "Hoàn hảo (Tua lại Offset về quá khứ bất kỳ lúc nào)",
      "AWSSQS": "Không hỗ trợ (Xóa sau khi hoàn tất)"
    },
    {
      "Criteria": "Định tuyến phức tạp (Complex Routing)",
      "RabbitMQ": "Vô địch (Topic, Direct, Fanout, Headers)",
      "ApacheKafka": "Cơ bản (Dựa trên Topic và Key Partitioning)",
      "AWSSQS": "Cơ bản (Cần kết hợp với AWS SNS để Fanout)"
    },
    {
      "Criteria": "Gánh nặng vận hành (Ops Overhead)",
      "RabbitMQ": "Trung bình (Cần giám sát RAM/Erlang cluster)",
      "ApacheKafka": "Cao (Cần tinh chỉnh JVM, Disk I/O, Partitions)",
      "AWSSQS": "Zero Ops (Amazon quản lý 100%)"
    }
  ]
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Rule of Thumb for Technology Selection",
                    CalloutTitleVi = "Quy Tắc Vàng Khi Lựa Chọn Hạ Tầng Hàng Đợi",
                    CalloutText = "Choose RabbitMQ for transactional microservice workflows requiring complex routing, instant acknowledgments, and priority queues (e.g. Order processing, Payment notifications). Choose Kafka for high-throughput event streaming, telemetry ingestion, event sourcing, or real-time analytics. Choose AWS SQS if your architecture is entirely hosted on AWS and you want zero operational maintenance.",
                    CalloutTextVi = "Chọn RabbitMQ cho các quy trình giao dịch Microservices cần định tuyến phức tạp, xác nhận tin nhắn tức thì và hàng đợi ưu tiên (ví dụ: Xử lý đơn hàng, Gửi email, Thông báo thanh toán). Chọn Kafka cho luồng dữ liệu thông lượng cực lớn, thu thập telemetry, Event Sourcing hoặc phân tích dữ liệu thời gian thực. Chọn AWS SQS nếu toàn bộ hệ thống của bạn nằm trên AWS và không muốn tốn một giây nào cho việc bảo trì máy chủ."
                },
                new DevArticleSection
                {
                    Heading = "2. Deep Dive: RabbitMQ Exchanges vs Kafka Partitions & Consumer Groups",
                    HeadingVi = "2. Giải Phẫu Chuyên Sâu: Cơ Chế Routing Trong RabbitMQ vs Phân Vùng Trong Kafka",
                    Paragraphs = new List<string>
                    {
                        "In RabbitMQ, producers never publish directly to a queue. Instead, they publish to an 'Exchange'. The Exchange inspects message attributes and routing keys, distributing copies into queues based on 'Bindings'. A 'Direct' exchange matches exact routing keys; a 'Topic' exchange matches wildcards (e.g. 'order.*.europe'); a 'Fanout' exchange broadcasts to all bound queues. RabbitMQ's Dead Letter Exchange (DLX) automatically captures rejected, unacknowledged, or expired messages for safety.",
                        "In Apache Kafka, a 'Topic' is divided into multiple 'Partitions' spread across cluster brokers. Each message has a key: messages with the same key are guaranteed to land on the exact same partition in strict chronological order. Parallelism is achieved through 'Consumer Groups': each partition is read by exactly one consumer within the group, enabling seamless linear scaling."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong RabbitMQ, Producer không bao giờ gửi tin nhắn trực tiếp vào Queue mà gửi qua 'Exchange'. Exchange phân tích routing key và chuyển tiếp tin nhắn vào các Queue dựa trên 'Bindings'. Exchange loại 'Direct' so khớp chính xác key; loại 'Topic' so khớp ký tự đại diện (ví dụ: 'order.*.europe'); loại 'Fanout' phát tán (broadcast) tới mọi queue. Cơ chế Dead Letter Exchange (DLX) tự động hứng các tin nhắn bị từ chối hoặc hết hạn để xử lý sự cố.",
                        "Trong Apache Kafka, mỗi 'Topic' được phân tách thành nhiều 'Partitions' lưu trữ trên các Broker khác nhau. Mỗi tin nhắn có một Key: các tin nhắn cùng Key luôn được đưa vào cùng một Partition và bảo toàn thứ tự thời gian tuyệt đối. Khả năng xử lý song song đạt được nhờ 'Consumer Groups': mỗi partition chỉ được một consumer trong cùng group đọc tại một thời điểm, cho phép mở rộng thông lượng theo cấp số nhân."
                    },
                    CodeTitle = "Docker Compose for Production-Ready RabbitMQ (Management UI) & Kafka (KRaft)",
                    CodeLanguage = "yaml",
                    CodeContent = """
version: '3.8'

services:
  # ==================== 1. RABBITMQ VỚI MANAGEMENT DASHBOARD ====================
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: dev-rabbitmq
    restart: unless-stopped
    ports:
      - "5672:5672"   # Cổng kết nối AMQP cho ứng dụng Microservices
      - "15672:15672" # Cổng Web UI quản trị (User/Pass: admin/admin123)
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin123
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq

  # ==================== 2. APACHE KAFKA (CHẾ ĐỘ KRAFT - KHÔNG CẦN ZOOKEEPER) ====================
  kafka:
    image: bashj79/kafka-kraft:latest
    container_name: dev-kafka
    restart: unless-stopped
    ports:
      - "9092:9092"   # Cổng kết nối Kafka Broker nội bộ
    environment:
      KAFKA_NODE_ID: 1
      KAFKA_PROCESS_ROLES: 'broker,controller'
      KAFKA_CONTROLLER_QUORUM_VOTERS: '1@localhost:9093'
      KAFKA_LISTENERS: 'PLAINTEXT://0.0.0.0:9092,CONTROLLER://0.0.0.0:9093'
      KAFKA_ADVERTISED_LISTENERS: 'PLAINTEXT://localhost:9092'
      KAFKA_CONTROLLER_LISTENER_NAMES: 'CONTROLLER'
      KAFKA_LOG_DIRS: '/tmp/kraft-combined-logs'
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
    volumes:
      - kafka_data:/tmp/kraft-combined-logs

volumes:
  rabbitmq_data:
  kafka_data:
""",
                    CalloutType = "warning",
                    CalloutTitle = "Monitor Kafka Consumer Lag Constantly",
                    CalloutTitleVi = "Cảnh Báo: Luôn Giám Sát Consumer Lag Trong Kafka",
                    CalloutText = "Consumer Lag is the delta between the latest offset produced in a partition and the offset committed by your consumer. A spiking consumer lag indicates your backend services cannot keep up with traffic, heralding message processing delays or eventual data dropouts.",
                    CalloutTextVi = "Consumer Lag là khoảng cách giữa Offset tin nhắn mới nhất được ghi vào Kafka và Offset mà ứng dụng của bạn đã đọc. Khi Consumer Lag tăng vọt, điều đó báo hiệu backend của bạn đang quá tải nghiêm trọng, dẫn đến độ trễ xử lý dữ liệu kéo dài."
                },
                new DevArticleSection
                {
                    Heading = "3. AWS SQS & Dead Letter Queues (DLQ): Poison Message Resilience & Idempotency",
                    HeadingVi = "3. AWS SQS & Mô Hình Dead-Letter Queue (DLQ) Chống Mất Mát Dữ Liệu",
                    Paragraphs = new List<string>
                    {
                        "AWS SQS offers two queue types: Standard Queues (nearly unlimited throughput, at-least-once delivery, best-effort ordering) and FIFO Queues (guaranteed first-in-first-out ordering and strictly exactly-once processing, limited to 300 - 3000 msg/sec).",
                        "A critical operational concept in message processing is the 'Poison Message'—a malformed payload that triggers an unhandled exception every time a consumer processes it. Without safety nets, the consumer fails, the message returns to the queue via Visibility Timeout expiry, and an infinite crash loop ensues.",
                        "The solution is a Dead Letter Queue (DLQ). When a message fails processing more than MaxReceiveCount (e.g. 5 times), AWS SQS automatically routes it into the DLQ, isolating the defect while allowing healthy business traffic to proceed unimpeded."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "AWS SQS cung cấp 2 loại hàng đợi: Standard Queue (thông lượng gần như vô hạn, cơ chế At-Least-Once Delivery, thứ tự tương đối) và FIFO Queue (đảm bảo chuẩn xác thứ tự First-In-First-Out và Exactly-Once Processing qua Message Deduplication ID, giới hạn 300 - 3000 msg/giây).",
                        "Một khái niệm sống còn trong vận hành hàng đợi là 'Poison Message' (Tin nhắn độc)—đây là những tin nhắn bị sai cấu trúc hoặc chứa dữ liệu bất thường khiến consumer ném ngoại lệ mỗi khi đọc. Nếu không có cơ chế bảo vệ, tin nhắn này sẽ quay lại hàng đợi sau khi hết thời gian Visibility Timeout, tạo thành vòng lặp sập ứng dụng vô tận (Crash Loop).",
                        "Giải pháp chuẩn là thiết lập Hàng Đợi Chết (Dead Letter Queue - DLQ). Khi một tin nhắn xử lý thất bại vượt quá số lần quy định (MaxReceiveCount ví dụ 5 lần), AWS SQS sẽ tự động cô lập nó sang DLQ, giúp luồng nghiệp vụ chính tiếp tục vận hành thông suốt."
                    },
                    CodeTitle = "AWS SQS Redrive Policy (DLQ) Configuration & Consumer Idempotency Pattern",
                    CodeLanguage = "json",
                    CodeContent = """
{
  "QueueName": "production-orders-queue",
  "Attributes": {
    "VisibilityTimeout": "30",
    "MessageRetentionPeriod": "345600",
    "RedrivePolicy": "{\"deadLetterTargetArn\":\"arn:aws:sqs:ap-southeast-1:123456789012:production-orders-dlq\",\"maxReceiveCount\":5}"
  },
  "IdempotencyNotice": {
    "Problem": "Vì mạng phân tán sử dụng cơ chế At-Least-Once Delivery, một tin nhắn có thể được gửi tới Consumer 2 lần!",
    "Solution": "Consumer bắt buộc phải kiểm tra MessageId hoặc Business TransactionId trong Redis/Database trước khi trừ tiền!",
    "PseudoCode": "if (await redis.SetNxAsync(message.TransactionId, 'PROCESSED', TimeSpan.FromHours(24)) == false) return; // Đã xử lý rồi, bỏ qua!"
  }
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Never Trust Once-Only Delivery Blindly",
                    CalloutTitleVi = "Nguyên Tắc Bất Di Bất Dịch: Mọi Consumer Đều Phải Idempotent",
                    CalloutText = "In distributed systems, hardware failures and network timeouts make true once-and-only-once delivery physically impossible without consumer-side coordination. Always make your consumers idempotent by deduplicating idempotency keys in Redis or a relational database.",
                    CalloutTextVi = "Trong hệ thống mạng phân tán, lỗi kết nối khiến cơ chế gửi đúng-duy-nhất-1-lần (Once-Only Delivery) là điều bất khả thi về mặt vật lý nếu không có sự phối hợp từ Consumer. Luôn thiết kế Consumer có tính chất Idempotent bằng cách kiểm tra trùng lặp Idempotency Key trong Redis hoặc Database."
                }
            }
        };
        series.Articles.Add(article4);

        // =========================================================================
        // BÀI 5: OBSERVABILITY & MONITORING: PROMETHEUS, GRAFANA & ELK STACK
        // =========================================================================
        var article5 = new DevArticle
        {
            Id = "infra-tools-article-5-observability-prometheus-grafana-elk",
            SeriesId = series.Id,
            LessonNumber = "Bài 5",
            Level = "Production",
            LevelVi = "Thực chiến",
            LevelOrder = 5,
            Title = "Production Observability: Prometheus, Grafana & ELK Stack – Metrics Monitoring, Alerts & Centralized Log Analytics",
            TitleVi = "Observability & Giám Sát Hạ Tầng: Prometheus, Grafana & ELK Stack – Hệ Thống Thu Thập Metrics & Log Tập Trung",
            ReadingTime = "22 phút đọc",
            PublishedDate = "25/10/2026",
            Summary = "Construct a battle-tested observability infrastructure for containerized distributed systems. Master Prometheus Time-Series Database (TSDB) metrics scraping and PromQL querying alongside Node Exporter and cAdvisor. Build Grafana operational dashboards with proactive alerting. Then deploy the ELK Stack (Elasticsearch, Filebeat/Logstash, Kibana) for full-text distributed log aggregation and instant incident triage.",
            SummaryVi = "Thiết lập hệ thống hạ tầng giám sát và quan sát chuẩn thực chiến cho cụm container phân tán. Làm chủ cơ chế kéo dữ liệu (Pull model) của Prometheus Time-Series Database (TSDB), viết các câu truy vấn PromQL phân tích hiệu năng (P90/P99 latency, rate, counter) cùng Node Exporter và cAdvisor. Xây dựng giao diện Dashboard trực quan và hệ thống cảnh báo tự động trên Grafana. Tiếp đó làm chủ bộ ba ELK Stack (Elasticsearch, Filebeat/Logstash, Kibana) để gom log tập trung từ hàng trăm container và truy vết sự cố trong vài nốt nhạc.",
            Tags = new List<string> { "Prometheus", "Grafana", "ELK Stack", "Elasticsearch", "Logstash", "Kibana", "Filebeat", "PromQL", "Monitoring" },
            Sections = new List<DevArticleSection>
            {
                new DevArticleSection
                {
                    Heading = "1. The Pillars of Observability: Metrics vs Logs in Modern Distributed Systems",
                    HeadingVi = "1. Ba Trụ Cột Của Observability: Metrics, Logs & Vai Trò Của Hạ Tầng Giám Sát",
                    Paragraphs = new List<string>
                    {
                        "In monolithic applications, checking server CPU and reading local log files was often sufficient. In microservice and Kubernetes clusters running hundreds of ephemeral containers, traditional monitoring fails. When an incident occurs, you need full Observability—the capability to infer the internal health of a distributed system from its external telemetry outputs.",
                        "Observability rests on three foundational pillars:",
                        "1) Metrics: Numeric time-series values representing system behavior over time (e.g. CPU load, memory consumption, request rates, error counters). Metrics are cheap to store, aggregate efficiently across thousands of pods, and trigger instant alerts when system health degrades.",
                        "2) Logs: Rich, timestamped event records containing deep context (exception traces, customer IDs, SQL queries). Logs are high-volume and expensive to index, but they are essential for root-cause analysis after an alert fires.",
                        "3) Traces: Distributed call chains tracking a single user request as it traverses across multiple microservices. A complete production stack combines Prometheus for metrics, Grafana for visualization, and the ELK Stack (Elasticsearch, Logstash, Kibana) for centralized full-text log analytics."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Trong kiến trúc Monolith cổ điển, việc mở task manager xem CPU và đọc file log trên đĩa là đủ dùng. Thế nhưng trong cụm Kubernetes chạy hàng trăm container tạm bợ liên tục khởi tạo rồi biến mất, giám sát kiểu cũ hoàn toàn tê liệt. Khi có sự cố, bạn cần một Hệ Thống Quan Sát Toàn Diện (Observability)—khả năng suy luận trạng thái nội tại của hệ sinh thái phân tán dựa trên các dữ liệu viễn trắc đầu ra.",
                        "Observability được xây dựng trên 3 trụ cột vững chắc:",
                        "1) Metrics (Chỉ số): Các giá trị định lượng theo chuỗi thời gian (như phần trăm CPU, dung lượng RAM, số lượng request/giây, bộ đếm lỗi 5xx). Metrics lưu trữ rất nhẹ, tổng hợp cực nhanh trên hàng ngàn Pods, giúp phát hiện ngay lập tức 'Hệ thống có đang bất thường không?'.",
                        "2) Logs (Nhật ký): Bản ghi chi tiết có dấu thời gian (timestamp) chứa đầy đủ ngữ cảnh sự kiện (Stacktrace lỗi, ID khách hàng, câu lệnh SQL). Logs dung lượng lớn và tốn dung lượng ổ đĩa, nhưng là chìa khóa để trả lời 'Chuyện gì đã xảy ra và lỗi do đâu?'.",
                        "3) Traces (Truy vết phân tán): Hành trình một request xuyên qua chuỗi các microservice. Một hạ tầng Production hoàn chỉnh kết hợp Prometheus để gom Metrics, Grafana để trực quan hóa cảnh báo, và bộ ba ELK Stack (Elasticsearch, Logstash, Kibana) để lập chỉ mục và tìm kiếm log tập trung."
                    },
                    CodeTitle = "Observability Pipeline Architecture: Metrics vs Logs Flow",
                    CodeLanguage = "json",
                    CodeContent = """
{
  "ObservabilityPipeline": {
    "MetricsStream": {
      "Collector": "Prometheus Scraper (Kéo dữ liệu định kỳ mỗi 15s)",
      "Sources": ["Node Exporter (Host)", "cAdvisor (Containers)", "App /metrics"],
      "Storage": "Prometheus TSDB (Time-Series Database)",
      "Visualization": "Grafana Dashboards & Alertmanager"
    },
    "LogsStream": {
      "Collector": "Filebeat DaemonSet (Đọc file /var/lib/docker/containers/*/*.log)",
      "Processing": "Logstash (Chuyển đổi, lọc PII nhạy cảm, enrich GeoIP)",
      "Storage": "Elasticsearch Cluster (Lập chỉ mục Inverted Index phân tán)",
      "Visualization": "Kibana Discover & Heatmap Dashboards"
    }
  }
}
""",
                    CalloutType = "tip",
                    CalloutTitle = "Metrics Find the Fire, Logs Identify the Match",
                    CalloutTitleVi = "Triết Lý Giám Sát: Metrics Chỉ Ra Đám Cháy, Logs Chỉ Ra Kẻ Bật Que Diêm",
                    CalloutText = "Never attempt to store detailed contextual data (like user emails or stack traces) inside Prometheus metric labels. High-cardinality label explosion will crash Prometheus's in-memory index. Use Prometheus to detect anomalies, then pivot into Elasticsearch/Kibana using timestamp and TraceId to inspect the underlying logs.",
                    CalloutTextVi = "Tuyệt đối không lưu dữ liệu chi tiết (như email người dùng hoặc stacktrace lỗi) vào labels của Prometheus. Việc bùng nổ số lượng chiều (High-Cardinality) sẽ làm sập RAM của Prometheus. Hãy dùng Prometheus để phát hiện sự cố, rồi dùng TraceId và mốc thời gian sang Kibana để đọc chi tiết log."
                },
                new DevArticleSection
                {
                    Heading = "2. Prometheus & Grafana: Pull Model, Node Exporter & PromQL Analytics",
                    HeadingVi = "2. Prometheus & Grafana: Thu Thập Metrics Với Exporters & Truy Vấn PromQL Thần Tốc",
                    Paragraphs = new List<string>
                    {
                        "Unlike push-based monitoring systems, Prometheus uses a 'Pull Model': it periodically scrapes HTTP '/metrics' endpoints exposed by target systems at configured intervals (e.g. scrape_interval: 15s). This design ensures that a slow or unresponsive microservice cannot overwhelm the monitoring server.",
                        "To monitor infrastructure components that don't natively expose Prometheus metrics, 'Exporters' act as translation proxies. 'Node Exporter' exposes Linux host OS metrics (CPU, RAM, disk I/O, network bandwidth); 'cAdvisor' (Container Advisor) exposes per-container resource metrics in Docker and Kubernetes.",
                        "Engineers write queries using PromQL (Prometheus Query Language) to compute critical operational signals: error rates over 5-minute rolling windows, and 95th/99th percentile latencies using histogram quantiles. Grafana connects to Prometheus as a datasource, rendering real-time dashboards and dispatching alerts to Slack or Telegram."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khác với các hệ thống đẩy dữ liệu thụ động, Prometheus vận hành theo cơ chế Kéo (Pull Model): Định kỳ mỗi chu kỳ (ví dụ 15 giây), Prometheus chủ động gửi HTTP GET request tới các endpoint '/metrics' của từng dịch vụ để thu thập dữ liệu. Thiết kế này giúp máy chủ giám sát luôn kiểm soát được tốc độ nạp dữ liệu và không bị nghẽn khi hệ thống gặp tải lớn.",
                        "Để giám sát các thành phần hạ tầng không có sẵn endpoint Prometheus, các 'Exporters' đóng vai trò là cầu nối chuyển đổi. 'Node Exporter' thu thập thông số phần cứng của máy chủ Linux (CPU, RAM, băng thông mạng, IOPS ổ đĩa); 'cAdvisor' thu thập thông số chi tiết của từng container đang chạy trên Docker hoặc K8s.",
                        "Kỹ sư sử dụng ngôn ngữ PromQL (Prometheus Query Language) để tính toán các chỉ số vận hành sống còn: Tỉ lệ lỗi 5xx trên cửa sổ trượt 5 phút, và độ trễ phân vị P95/P99. Grafana kết nối trực tiếp với Prometheus để vẽ biểu đồ và tự động bắn cảnh báo về Slack, Discord hoặc Telegram."
                    },
                    CodeTitle = "Production prometheus.yml Configuration & Essential PromQL Cheat Sheet",
                    CodeLanguage = "yaml",
                    CodeContent = """
# ==================== FILE CẤU HÌNH PROMETHEUS (prometheus.yml) ====================
global:
  scrape_interval: 15s     # Thu thập dữ liệu mỗi 15 giây
  evaluation_interval: 15s # Đánh giá các luật cảnh báo mỗi 15 giây

# Danh sách các mục tiêu cần giám sát (Scrape Targets)
scrape_configs:
  - job_name: 'node-exporter-host'
    static_configs:
      - targets: ['node-exporter:9100']

  - job_name: 'cadvisor-containers'
    static_configs:
      - targets: ['cadvisor:8080']

  - job_name: 'microservices-app'
    metrics_path: '/metrics'
    static_configs:
      - targets: ['order-service:8080', 'payment-service:8081']

# ==================== CÁC CÂU TRUY VẤN PROMQL THẦN TỐC ====================
# 1. Tính toán tỉ lệ Request Lỗi HTTP 5xx trên tổng số Request trong 5 phút qua (% Lỗi):
# sum(rate(http_requests_total{status=~"5.."}[5m])) / sum(rate(http_requests_total[5m])) * 100

# 2. Tính độ trễ phân vị P99 (99% người dùng trải nghiệm độ trễ dưới mức này):
# histogram_quantile(0.99, sum(rate(http_request_duration_seconds_bucket[5m])) by (le))

# 3. Phát hiện máy chủ sắp cạn kiệt RAM (Dung lượng RAM còn trống < 10%):
# (node_memory_MemAvailable_bytes / node_memory_MemTotal_bytes) * 100 < 10
""",
                    CalloutType = "warning",
                    CalloutTitle = "Never Use High-Cardinality Labels in Prometheus",
                    CalloutTitleVi = "Cảnh Báo: Tuyệt Đối Tránh High-Cardinality Labels Trong Metrics",
                    CalloutText = "Do not attach dynamic, unbounded values (such as UserId, OrderId, or Email) as Prometheus metric labels. Each unique label combination creates a brand new Time-Series in memory. 100,000 unique UserIds will generate 100,000 time series, causing Prometheus to run out of RAM and crash instantly.",
                    CalloutTextVi = "Không gắn các giá trị động không giới hạn (như UserId, OrderId, UUID hay Email) vào nhãn (labels) của Prometheus. Mỗi tổ hợp nhãn duy nhất sẽ tạo ra một chuỗi thời gian (Time-Series) độc lập trong bộ nhớ. 100,000 User ID khác nhau sẽ tạo ra 100,000 time series làm Prometheus cạn kiệt RAM và sập ngay lập tức."
                },
                new DevArticleSection
                {
                    Heading = "3. The ELK Stack in Production: Elasticsearch, Filebeat & Kibana Log Analytics",
                    HeadingVi = "3. ELK Stack Trong Thực Chiến: Filebeat, Elasticsearch & Kibana Phân Tích Log Cụm",
                    Paragraphs = new List<string>
                    {
                        "When an application scales across 50 container instances, finding why an order failed using 'docker logs' is impossible. The ELK Stack solves this by centralizing and indexing all container logs:",
                        "1) Filebeat: A lightweight, low-footprint log shipper written in Go. Installed as a daemon on each host node, it tails container log files from disk and forwards them with zero impact on host CPU/memory.",
                        "2) Logstash: An optional data processing pipeline that ingests data from Filebeat, parses complex formats (Grok patterns), masks sensitive customer information (PII), and normalizes structures before shipping to storage.",
                        "3) Elasticsearch: A distributed, JSON-based search engine built on Apache Lucene. It shards log data across nodes, builds inverted indexes, and delivers sub-second full-text searches across billions of log events.",
                        "4) Kibana: The web-based visualization UI where developers explore logs, filter by TraceId or Container Name, visualize error spikes, and isolate incidents in seconds."
                    },
                    ParagraphsVi = new List<string>
                    {
                        "Khi ứng dụng mở rộng ra hàng chục container phân tán, việc tìm kiếm nguyên nhân lỗi bằng lệnh 'docker logs' trên từng máy chủ là điều không tưởng. Bộ ba ELK Stack giải quyết triệt để vấn đề này bằng cách tập trung hóa và đánh chỉ mục toàn bộ log của hệ thống:",
                        "1) Filebeat: Tác nhân thu thập log siêu nhẹ viết bằng ngôn ngữ Go. Chạy dưới dạng daemon trên từng máy chủ, nó trực tiếp đọc các file log của Docker container trên đĩa cứng và đẩy đi mà gần như không chiếm dụng CPU hay RAM.",
                        "2) Logstash: Trạm trung chuyển và biến đổi dữ liệu, tiếp nhận log từ Filebeat, bóc tách cấu trúc (Grok filter), ẩn giấu thông tin thanh toán nhạy cảm (PII masking) và chuẩn hóa dữ liệu trước khi nạp vào kho.",
                        "3) Elasticsearch: Cỗ máy tìm kiếm phân tán dựa trên Apache Lucene. Nó phân mảnh dữ liệu (Sharding) trên nhiều node, xây dựng chỉ mục đảo (Inverted Index) và cho phép tìm kiếm văn bản toàn văn (Full-Text Search) trên hàng tỉ bản ghi chỉ trong vài mili-giây.",
                        "4) Kibana: Giao diện web trực quan giúp lập trình viên tìm kiếm log, lọc theo TraceId hoặc Tên Dịch Vụ, vẽ biểu đồ nhiệt sự cố và cô lập nguyên nhân lỗi chỉ trong vài thao tác."
                    },
                    CodeTitle = "Complete Production Observability Stack: Prometheus, Grafana & ELK in Docker Compose",
                    CodeLanguage = "yaml",
                    CodeContent = """
version: '3.8'

services:
  # ==================== 1. HỆ THỐNG GIÁM SÁT METRICS (PROMETHEUS & GRAFANA) ====================
  prometheus:
    image: prom/prometheus:latest
    container_name: obs-prometheus
    restart: unless-stopped
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus

  node-exporter:
    image: prom/node-exporter:latest
    container_name: obs-node-exporter
    restart: unless-stopped
    ports:
      - "9100:9100"

  grafana:
    image: grafana/grafana:latest
    container_name: obs-grafana
    restart: unless-stopped
    ports:
      - "3000:3000" # Web Dashboard (User/Pass mặc định: admin/admin)
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana_data:/var/lib/grafana

  # ==================== 2. HỆ THỐNG QUẢN LÝ LOG TẬP TRUNG (ELASTICSEARCH & KIBANA) ====================
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.13.0
    container_name: obs-elasticsearch
    restart: unless-stopped
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false # Tắt xác thực cho môi trường Dev/Staging
      - "ES_JAVA_OPTS=-Xms1g -Xmx1g" # Giới hạn 1GB Heap RAM
    ports:
      - "9200:9200"
    volumes:
      - es_data:/usr/share/elasticsearch/data

  kibana:
    image: docker.elastic.co/kibana/kibana:8.13.0
    container_name: obs-kibana
    restart: unless-stopped
    ports:
      - "5601:5601" # Giao diện Web Khám Phá Log
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    depends_on:
      - elasticsearch

volumes:
  prometheus_data:
  grafana_data:
  es_data:
""",
                    CalloutType = "tip",
                    CalloutTitle = "Configure Index Lifecycle Management (ILM) in Elasticsearch",
                    CalloutTitleVi = "Thiết Lập Vòng Đời Chỉ Mục (ILM) Chống Tràn Ổ Cứng",
                    CalloutText = "In high-throughput microservice clusters, log data can consume hundreds of gigabytes per week. Always configure Index Lifecycle Management (ILM) in Elasticsearch to automatically roll over indices daily, transition data from Hot (SSD) to Cold (HDD) tiers, and delete logs older than 30 days.",
                    CalloutTextVi = "Trong cụm microservices có lưu lượng lớn, dữ liệu log có thể ngốn hàng trăm gigabytes mỗi tuần. Luôn cấu hình Index Lifecycle Management (ILM) trong Elasticsearch để tự động gom log theo ngày, chuyển log cũ sang ổ cứng giá rẻ và tự động xóa dữ liệu sau 30 ngày để chống tràn ổ cứng máy chủ."
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

    // Helper method to regenerate or update json if needed
    internal static void UpdateDevLearningJsonWithModernInfrastructureSeries()
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
            data.SeriesList.Insert(0, newSeries);
        }

        var updatedJson = JsonSerializer.Serialize(data, options);
        File.WriteAllText(path, updatedJson);
    }
}
