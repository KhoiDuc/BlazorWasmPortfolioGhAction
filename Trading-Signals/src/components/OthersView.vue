<template>
  <div class="d-flex flex-column min-vh-100">
    <NavBar />
    <div class="container mt-4 flex-grow-1">
      <!-- Title -->
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2 pt-2">
        <h2 class="mb-0 fw-bold d-flex align-items-center gap-2 text-slate-800">
          <span>🌐</span> Useful Links & Resources
        </h2>
        <span class="badge bg-primary px-3 py-2 shadow-sm" style="background-color: #3b82f6 !important; font-size: 0.88rem; font-weight: 600;">
          Resource Hub
        </span>
      </div>

      <!-- Introduction Panel -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card border-0 shadow-sm glass-panel border-glass" style="border-left: 4px solid #3b82f6 !important;">
            <div class="card-body py-3">
              <p class="mb-0" style="color: #475569; font-size: 0.95rem; line-height: 1.6;">
                A curated directory of useful macroeconomic websites, charting platforms, and financial portals. These external resources provide deeper insights into global monetary policy and economic trends to refine your trading analysis.
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Group Filter Tabs -->
      <div class="d-flex flex-wrap gap-2 mb-4">
        <button
          v-for="group in groups"
          :key="group.id"
          @click="activeGroup = group.id"
          class="btn filter-btn px-4 py-2 d-flex align-items-center gap-2"
          :class="activeGroup === group.id ? 'btn-active shadow-sm' : 'btn-inactive'"
        >
          <span>{{ group.icon }}</span> {{ group.name }}
          <span class="badge count-badge ms-1" :class="activeGroup === group.id ? 'bg-white text-primary' : 'bg-secondary text-white'">
            {{ getLinkCount(group.id) }}
          </span>
        </button>
      </div>

      <!-- Grid of Links -->
      <div class="row">
        <div class="col-12 col-md-6 col-lg-4 mb-4" v-for="(link, index) in filteredLinks" :key="index">
          <a :href="link.url" target="_blank" class="resource-card-link">
            <div class="card h-100 border-0 shadow-sm resource-card">
              <div class="card-body p-4 d-flex flex-column justify-content-between">
                <div>
                  <!-- Header Area -->
                  <div class="d-flex align-items-center justify-content-between mb-3">
                    <span class="resource-icon">{{ link.icon }}</span>
                    <span class="badge bg-light text-primary border px-2 py-1" style="font-size: 0.75rem; font-weight: 600;">
                      {{ link.category }}
                    </span>
                  </div>
                  
                  <!-- Title and Description -->
                  <h5 class="fw-bold text-dark mb-2 resource-title d-flex align-items-center gap-1">
                    {{ link.title }}
                  </h5>
                  <p class="resource-desc text-muted mb-4">
                    {{ link.description }}
                  </p>
                </div>
                
                <!-- Action Area -->
                <div class="d-flex align-items-center justify-content-between pt-2 border-top">
                  <span class="resource-domain text-truncate" style="color: #718096; font-size: 0.8rem; max-width: 60%;">
                    {{ getDomain(link.url) }}
                  </span>
                  <span class="btn-visit">
                    Visit Site ↗
                  </span>
                </div>
              </div>
            </div>
          </a>
        </div>
      </div>
    </div>
    <AppFooter class="mt-5" />
  </div>
</template>

<script>
import NavBar from './NavBar.vue';
import AppFooter from './AppFooter.vue';
import { ref, computed } from 'vue';

export default {
  name: 'OthersView',
  components: {
    NavBar,
    AppFooter
  },
  setup() {
    const activeGroup = ref('all');

    const groups = ref([
      { id: 'all', name: 'All Resources', icon: '🌐' },
      { id: 'macro', name: 'Macro & Central Banks', icon: '🏛️' },
      { id: 'charts', name: 'Market & Yield Charts', icon: '📈' },
      { id: 'realestate', name: 'Real Estate & Planning', icon: '🗺️' }
    ]);

    const links = ref([
      {
        title: 'TradingView World Economy',
        description: 'Global economic indicators, banking charts, interest rates, and financial datasets in interactive formats.',
        url: 'https://www.tradingview.com/markets/world-economy/charts-banking-finance',
        icon: '📊',
        category: 'Market Charts',
        groupId: 'charts'
      },
      {
        title: 'Central Bank Watch',
        description: 'Track rate probability charts, interest rate decisions, meeting schedules, and historical data across major central banks.',
        url: 'https://centralbank.watch/',
        icon: '🏛️',
        category: 'Monetary Policy',
        groupId: 'macro'
      },
      {
        title: 'State Bank of Vietnam (SBV)',
        description: 'Ngân hàng Nhà nước Việt Nam. Cập nhật chính sách tiền tệ chính thức, tỷ giá trung tâm, lãi suất điều hành và thông cáo báo chí.',
        url: 'https://www.sbv.gov.vn/',
        icon: '🏛️',
        category: 'Central Banks',
        groupId: 'macro'
      },
      {
        title: 'Real Interest Rates Tracker',
        description: 'Compare inflation-adjusted interest rates across major economies to see the true cost of borrowing.',
        url: 'https://centralbank.watch/tools/real-interest-rates/',
        icon: '📈',
        category: 'Monetary Policy',
        groupId: 'macro'
      },
      {
        title: 'Recession Probability Tool',
        description: 'Gauge potential economic downturn risks using quantitative models and yield-based forecast probabilities.',
        url: 'https://centralbank.watch/tools/recession-probability/',
        icon: '🚨',
        category: 'Economic Indicators',
        groupId: 'macro'
      },
      {
        title: 'Yield Curve Visualization',
        description: 'Interact with and track sovereign yield curves to identify economic cycle expansion and inversion phases.',
        url: 'https://centralbank.watch/tools/yield-curve/',
        icon: '📉',
        category: 'Market Charts',
        groupId: 'charts'
      },
      {
        title: 'Global Inflation Tracker',
        description: 'Monitor consumer price index changes (CPI), core inflation rates, and price level trajectories worldwide.',
        url: 'https://centralbank.watch/tools/inflation-tracker/',
        icon: '💸',
        category: 'Economic Indicators',
        groupId: 'macro'
      },
      {
        title: 'HCMC Planning Information Portal',
        description: 'Bản đồ thông tin quy hoạch chính thức của TP. Hồ Chí Minh. Tra cứu thông tin quy hoạch đất đai chi tiết.',
        url: 'https://thongtinquyhoach.hochiminhcity.gov.vn/',
        icon: '🗺️',
        category: 'Real Estate & Planning',
        groupId: 'realestate'
      },
      {
        title: 'Mogi Real Estate',
        description: 'Cổng thông tin rao vặt bất động sản hàng đầu, mua bán cho thuê nhà đất và cập nhật giá nhà đất thực tế tại Việt Nam.',
        url: 'https://mogi.vn/',
        icon: '🏠',
        category: 'Real Estate & Planning',
        groupId: 'realestate'
      },
      {
        title: 'OneHousing Real Estate',
        description: 'Nền tảng giao dịch bất động sản toàn diện, định giá nhà đất trực tuyến chuyên nghiệp và hỗ trợ tư vấn tài chính mua nhà.',
        url: 'https://onehousing.vn/',
        icon: '🏢',
        category: 'Real Estate & Planning',
        groupId: 'realestate'
      },
      {
        title: 'Mortgage Rates Comparison',
        description: 'Follow residential and commercial mortgage rates to analyze real estate debt markets and consumer health.',
        url: 'https://centralbank.watch/tools/mortgage-rates/',
        icon: '🏠',
        category: 'Real Estate & Planning',
        groupId: 'realestate'
      },
      {
        title: 'Review Bất Động Sản',
        description: 'Tra cứu thông tin quy hoạch, tiến độ dự án, và đánh giá các dự án bất động sản mở bán toàn quốc.',
        url: 'https://reviewbatdongsan.vn/du-an',
        icon: '🏢',
        category: 'Real Estate & Planning',
        groupId: 'realestate'
      },
      {
        title: 'Mở Bán BĐS Dự Án',
        description: 'Cung cấp danh sách căn hộ, nhà phố biệt thự mới nhất đang mở bán và thông tin từ chủ đầu tư.',
        url: 'https://mobanbds.com/du-an',
        icon: '🏘️',
        category: 'Real Estate & Planning',
        groupId: 'realestate'
      },
      {
        title: 'Bất Động Sản Việt Dự Án',
        description: 'Kênh thông tin và đánh giá các dự án bất động sản tiềm năng tại thị trường Việt Nam.',
        url: 'https://batdongsanviet.com.vn/du-an.html',
        icon: '🏗️',
        category: 'Real Estate & Planning',
        groupId: 'realestate'
      },
      {
        title: 'Batdongsan.com.vn Dự Án',
        description: 'Hệ thống danh mục các dự án bất động sản toàn quốc đầy đủ và cập nhật giá cả thị trường.',
        url: 'https://batdongsan.com.vn/du-an-bat-dong-san',
        icon: '🏢',
        category: 'Real Estate & Planning',
        groupId: 'realestate'
      },
      {
        title: 'Macro Economic Calendar',
        description: 'Never miss high-impact monetary policy statements, job reports, GDP data, and major global macro events.',
        url: 'https://centralbank.watch/tools/economic-calendar/',
        icon: '📅',
        category: 'Calendar',
        groupId: 'macro'
      },
      {
        title: 'Financial Juice',
        description: 'Real-time financial news, audio squawk feed, and macroeconomic alerts tailored for active traders.',
        url: 'https://www.financialjuice.com/',
        icon: '🔊',
        category: 'News & Alerts',
        groupId: 'macro'
      }
    ]);

    const getDomain = (url) => {
      try {
        const parsed = new URL(url);
        return parsed.hostname.replace('www.', '');
      } catch (e) {
        return url;
      }
    };

    const getLinkCount = (groupId) => {
      if (groupId === 'all') {
        return links.value.length;
      }
      return links.value.filter(link => link.groupId === groupId).length;
    };

    const filteredLinks = computed(() => {
      if (activeGroup.value === 'all') {
        return links.value;
      }
      return links.value.filter(link => link.groupId === activeGroup.value);
    });

    return {
      activeGroup,
      groups,
      links,
      filteredLinks,
      getDomain,
      getLinkCount
    };
  }
};
</script>

<style scoped>
.glass-panel {
  background: #ffffff !important;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.04) !important;
}
.border-glass {
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
}

.filter-btn {
  font-size: 0.9rem;
  font-weight: 600;
  border-radius: 40px;
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
}

.btn-inactive {
  background-color: #f1f5f9;
  color: #475569;
}

.btn-inactive:hover {
  background-color: #e2e8f0;
  color: #0f172a;
  border-color: rgba(0, 0, 0, 0.15) !important;
}

.btn-active {
  background-color: #3b82f6 !important;
  color: #ffffff !important;
  border-color: #3b82f6 !important;
}

.count-badge {
  font-size: 0.75rem;
  border-radius: 10px;
  padding: 3px 8px;
}

.resource-card-link {
  text-decoration: none;
  display: block;
  height: 100%;
}

.resource-card {
  border-radius: 12px;
  background-color: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  overflow: hidden;
}

.resource-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 25px rgba(59, 130, 246, 0.1) !important;
  border-color: #3b82f6 !important;
}

.resource-icon {
  font-size: 2rem;
  line-height: 1;
}

.resource-title {
  color: #0f172a !important;
  font-size: 1.15rem;
  transition: color 0.2s;
  font-family: 'Outfit', sans-serif;
}

.resource-card:hover .resource-title {
  color: #2563eb !important;
}

.resource-desc {
  font-size: 0.9rem;
  color: #475569 !important;
  line-height: 1.5;
}

.btn-visit {
  font-size: 0.85rem;
  font-weight: 600;
  color: #3b82f6;
  transition: all 0.2s;
}

.resource-card:hover .btn-visit {
  color: #1d4ed8;
  text-decoration: underline;
}

/* Category Badge Outline style override */
.resource-card :deep(.badge.bg-light) {
  background-color: rgba(59, 130, 246, 0.08) !important;
  color: #2563eb !important;
  border: 1px solid rgba(59, 130, 246, 0.15) !important;
}
</style>
