<template>
  <div id="app" class="d-flex flex-column min-vh-100">
    <NavBar />
    <notifications />
    
    <div class="home-view container flex-grow-1 pt-5 pb-5">
      <!-- Live Market Stream (Single Row) -->
      <div class="mb-4">
        <div class="d-flex align-items-stretch gap-2" style="background: rgba(255, 255, 255, 0.4); backdrop-filter: blur(10px); border: 1px solid rgba(0, 0, 0, 0.05); padding: 10px; border-radius: 12px;">
          <!-- Newest Item (Fixed at the beginning of the list, highlighted) -->
          <div class="market-card-wrapper market-card-wrapper--mini" v-if="marketAssets.length > 0" style="flex-shrink: 0;">
            <div class="market-card-link" @click="openChartModal(marketAssets[0])" style="cursor: pointer; height: 100%;">
              <div class="market-card market-card--mini p-3 h-100 d-flex flex-column justify-content-between" :title="marketAssets[0].message || marketAssets[0].name" style="border: 1.5px solid rgba(59, 130, 246, 0.4); box-shadow: 0 4px 12px rgba(59, 130, 246, 0.1);">
                <div>
                  <div class="d-flex justify-content-between align-items-center mb-1">
                    <span class="market-card__icon" :style="{ background: marketAssets[0].iconBg }">{{ marketAssets[0].emoji }}</span>
                    <span class="market-card__change" :class="marketAssets[0].positive ? 'text-neon-green' : 'text-neon-red'">
                      {{ marketAssets[0].change }}
                    </span>
                  </div>
                  <h4 class="market-card__title">{{ marketAssets[0].name }}</h4>
                  <p class="market-card__price mb-0">{{ marketAssets[0].price }}</p>
                  <div class="market-card__time mt-1 small" :style="{ opacity: marketAssets[0].relativeTime ? 1 : 0, color: '#64748b', 'font-size': '0.52rem', 'font-weight': '500', 'line-height': '0.8rem', 'height': '0.8rem' }">⏱️ {{ marketAssets[0].relativeTime || 'Pending' }}</div>
                </div>
                <div class="market-card__sparkline mt-1">
                  <svg viewBox="0 0 100 30" class="sparkline-svg">
                    <path :d="marketAssets[0].sparkline" fill="none" :stroke="marketAssets[0].positive ? '#10b981' : '#ef4444'" stroke-width="2" stroke-linecap="round"></path>
                  </svg>
                </div>
              </div>
            </div>
          </div>

          <!-- Auto-scrolling Marquee with manual scroll-back support (If 2 or more assets) -->
          <div class="position-relative flex-grow-1" style="min-width: 0; display: flex; align-items: stretch;" v-if="marketAssets.length > 1">
            <!-- Left Navigation Button -->
            <button 
              class="marquee-nav-btn marquee-nav-btn--left" 
              @click="scrollMarquee('left')"
              aria-label="Scroll left"
            >
              <i class="bi bi-chevron-left"></i>
            </button>

            <div 
              ref="marqueeContainer"
              class="marquee-container flex-grow-1 custom-horizontal-scroll" 
              style="overflow-x: auto; position: relative; min-width: 0; display: flex; align-items: stretch; padding-bottom: 4px;"
              @scroll="handleMarqueeScroll"
              @wheel="onMarqueeWheel"
              @mouseenter="pauseMarquee"
              @mouseleave="resumeMarquee"
              @touchstart="pauseMarquee"
              @touchend="resumeMarquee"
            >
              <div ref="marqueeContent" class="marquee-js-content" style="display: flex; align-items: stretch; width: max-content;">
                <!-- Double tracks for seamless infinite loop -->
                <div class="marquee-track marquee-track--mini" v-for="i in 2" :key="i" style="display: flex; align-items: stretch;">
                  <template v-for="(asset, idx) in scrollingAssets" :key="`marquee-group-${i}-${idx}`">
                    <div class="market-card-wrapper market-card-wrapper--mini">
                      <div class="market-card-link" @click="openChartModal(asset)" style="cursor: pointer; height: 100%;">
                        <div class="market-card market-card--mini p-3 h-100 d-flex flex-column justify-content-between" :title="asset.message || asset.name">
                          <div>
                            <div class="d-flex justify-content-between align-items-center mb-1">
                              <span class="market-card__icon" :style="{ background: asset.iconBg }">{{ asset.emoji }}</span>
                              <span class="market-card__change" :class="asset.positive ? 'text-neon-green' : 'text-neon-red'">
                                {{ asset.change }}
                              </span>
                            </div>
                            <h4 class="market-card__title">{{ asset.name }}</h4>
                            <p class="market-card__price mb-0">{{ asset.price }}</p>
                            <div class="market-card__time mt-1 small" :style="{ opacity: asset.relativeTime ? 1 : 0, color: '#64748b', 'font-size': '0.52rem', 'font-weight': '500', 'line-height': '0.8rem', 'height': '0.8rem' }">⏱️ {{ asset.relativeTime || 'Pending' }}</div>
                          </div>
                          <div class="market-card__sparkline mt-1">
                            <svg viewBox="0 0 100 30" class="sparkline-svg">
                              <path :d="asset.sparkline" fill="none" :stroke="asset.positive ? '#10b981' : '#ef4444'" stroke-width="2" stroke-linecap="round"></path>
                            </svg>
                          </div>
                        </div>
                      </div>
                    </div>
                    <!-- Separator between loop cycles (end of one cycle / start of next) -->
                    <div 
                      v-if="marketAssets.length > 2 && (idx + 1) % (marketAssets.length - 1) === 0"
                      class="marquee-separator"
                      :key="`marquee-sep-${i}-${idx}`"
                    >
                      <div class="marquee-separator-line">
                        <div class="marquee-separator-dot"></div>
                      </div>
                    </div>
                  </template>
                </div>
              </div>
            </div>

            <!-- Right Navigation Button -->
            <button 
              class="marquee-nav-btn marquee-nav-btn--right" 
              @click="scrollMarquee('right')"
              aria-label="Scroll right"
            >
              <i class="bi bi-chevron-right"></i>
            </button>
          </div>
        </div>
      </div>

      <!-- Economic Calendar Section -->
      <div class="mb-5">
        <div class="stk-panel" style="border-bottom-left-radius: 0; border-bottom-right-radius: 0; margin-bottom: 0;">
          <div class="stk-header d-flex justify-content-between align-items-center flex-wrap gap-3">
            <div class="d-flex align-items-center gap-3">
              <div class="stk-header__icon">
                <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
              </div>
              <div>
                <h2 class="stk-header__title">Economic Calendar</h2>
                <p class="stk-header__sub">Track macro events and economic indicators impact</p>
              </div>
            </div>
            
            <div class="d-flex gap-2 align-items-center">
              <button class="stk-btn stk-btn--outline py-2 px-3" @click="goToPreviousDay" :disabled="isPreviousDisabled">&lt; Previous</button>
              <div class="position-relative" style="min-width: 150px;">
                <!-- Styled visual placeholder matching calendar aesthetic -->
                <div class="stk-input py-2 px-3 d-flex align-items-center justify-content-between bg-white text-dark" style="font-size: 0.85rem; pointer-events: none; border-color: rgba(0,0,0,0.1);">
                  <span class="fw-semibold">{{ formatInputDate(selectedDate) }}</span>
                  <i class="bi bi-calendar3 text-secondary" style="font-size: 0.9rem;"></i>
                </div>
                <!-- Hidden native date input sitting on top -->
                <input 
                  type="date" 
                  id="dateFilter" 
                  v-model="selectedDate" 
                  style="position: absolute; top: 0; left: 0; width: 100%; height: 100%; opacity: 0; cursor: pointer; z-index: 2;"
                >
              </div>
              <button class="stk-btn stk-btn--outline py-2 px-3" @click="goToNextDay" :disabled="isNextDisabled">Next &gt;</button>
            </div>
          </div>
          
          <div class="px-4 py-2 border-top text-start" style="font-weight: 700; color: #475569; font-size: 0.85rem; background-color: #f8fafc;">
            📅 Selected: {{ formattedDateLong }}
          </div>
        </div>

        <div v-if="isLoadingCalendar" class="stk-loading py-5 bg-white border border-top-0 rounded-bottom-4">
          <div class="stk-spinner"></div>
        </div>
        
        <div v-else>
          <div class="stk-panel border-top-0 rounded-top-0" v-if="sortedCalendarData.length > 0" style="margin-bottom: 0;">
            <div class="stk-table-wrap" style="border: none; border-radius: 0 0 16px 16px;">
              <table class="stk-table">
                <thead>
                  <tr>
                    <th class="stk-th">Date</th>
                    <th class="stk-th">Country</th>
                    <th class="stk-th">Title</th>
                    <th class="stk-th">Impact</th>
                    <th class="stk-th stk-th--right">Forecast</th>
                    <th class="stk-th stk-th--right">Previous</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in sortedCalendarData" :key="item.date + item.title" :class="{ 'stk-row--active': item === closestCalendarItem }" class="stk-row">
                    <td class="stk-td">{{ formatCalendarDate(item.date) }}</td>
                    <td class="stk-td"><strong>{{ item.country }}</strong></td>
                    <td class="stk-td" style="text-align: left;"><strong>{{ item.title }}</strong></td>
                    <td class="stk-td">
                      <span class="stk-signal" :class="'stk-signal--' + String(item.impact).toLowerCase()">
                        {{ item.impact }}
                      </span>
                    </td>
                    <td class="stk-td stk-td--right">{{ item.forecast || '-' }}</td>
                    <td class="stk-td stk-td--right">{{ item.previous }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div v-else class="stk-message p-5 border border-top-0 rounded-bottom-4 bg-white text-center">
            No economic events scheduled for this day.
          </div>
        </div>
      </div>

      <!-- Insights Area -->
      <div class="row g-4 mb-4">
        <!-- Unified Column: Platform Intelligence & Current World State -->
        <div class="col-lg-12">
          <div class="feature-panel p-4">
            <h3 class="panel-heading mb-4 d-flex align-items-center justify-content-between flex-wrap gap-2 w-100">
              <span class="d-flex align-items-center gap-2">
                <span>🧠</span> Platform Intelligence
              </span>
              <div class="d-flex align-items-center gap-2">
                <button 
                  class="stk-btn stk-btn--outline d-flex align-items-center gap-1 py-1 px-2 rounded-3 text-success border-success" 
                  style="font-size: 0.75rem; font-weight: 600;"
                  @click="runAIAnalysis"
                  :disabled="loadingTheses || runningAI"
                  title="Yêu cầu AI phân tích tin tức và cập nhật nhận định mới nhất"
                >
                  <i v-if="!runningAI" class="bi bi-cpu" style="font-size: 0.85rem;"></i>
                  <span v-else class="spinner-border spinner-border-sm text-success" role="status" style="width: 0.85rem; height: 0.85rem; border-width: 1.5px;"></span>
                  <span>Chạy phân tích AI mới nhất</span>
                </button>
                <button 
                  class="stk-btn stk-btn--outline d-flex align-items-center gap-1 py-1 px-2 rounded-3" 
                  style="font-size: 0.75rem; font-weight: 600;"
                  @click="refreshThesesManual"
                  :disabled="loadingTheses"
                  title="Làm mới nhận định (Bỏ qua cache)"
                >
                  <i v-if="!loadingTheses" class="bi bi-arrow-clockwise" style="font-size: 0.85rem;"></i>
                  <span v-else class="spinner-border spinner-border-sm" role="status" style="width: 0.85rem; height: 0.85rem; border-width: 1.5px;"></span>
                  <span>Refresh</span>
                </button>
              </div>
            </h3>
            
            <div v-if="loadingTheses" class="text-center py-5">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
              <p class="mt-3 text-muted small">AI đang tổng hợp và phân tích dữ liệu...</p>
            </div>
            
            <div v-else-if="macroTheses && macroTheses.length > 0" class="theses-container mb-4" style="padding-right: 5px;">
              <div class="thesis-card p-4 rounded-4" style="background: linear-gradient(145deg, rgba(59, 130, 246, 0.03) 0%, rgba(59, 130, 246, 0.08) 100%); border: 1px solid rgba(59, 130, 246, 0.15); box-shadow: 0 4px 15px rgba(0,0,0,0.02);">
                <div class="d-flex justify-content-between align-items-center mb-3 pb-3 border-bottom" style="border-color: rgba(59, 130, 246, 0.1) !important;">
                  <span class="badge px-3 py-2" :class="macroTheses[0].confidence > 0.7 ? 'bg-success' : 'bg-warning text-dark'" style="font-size: 0.8rem; letter-spacing: 0.5px;">ĐỘ TIN CẬY: {{ (macroTheses[0].confidence * 100).toFixed(0) }}%</span>
                  <span class="small text-muted fw-medium"><i class="bi bi-clock-history me-1"></i>Cập nhật: {{ formatDateWithOffset(macroTheses[0].updated_at) }}</span>
                </div>
                
                <div>
                  <h5 class="feature-title text-primary fw-bold mb-3 d-flex align-items-center"><span class="fs-4 me-2">🌍</span> Tổng hợp Vĩ mô:</h5>
                  <div class="feature-desc mb-4 text-dark" style="font-size: 0.95rem; line-height: 1.7; text-align: justify;" v-html="formatThesisText(macroTheses[0].thesis)"></div>
                  
                  <h5 class="feature-title text-success fw-bold mb-3 d-flex align-items-center mt-4"><span class="fs-4 me-2">🛡️</span> Tư vấn Danh mục:</h5>
                  <div class="feature-desc mb-0 p-3 rounded-3" style="font-size: 0.95rem; line-height: 1.7; background: rgba(16, 185, 129, 0.05); border-left: 4px solid #10b981; color: #1f2937;" v-html="formatThesisText(macroTheses[0].supporting_evidence)"></div>

                  <!-- Ask AI Button linked with Telegram DB and Chat -->
                  <div class="d-flex justify-content-end mt-4 pt-3 border-top" style="border-color: rgba(59, 130, 246, 0.1) !important;">
                    <button 
                      class="stk-btn stk-btn--outline d-flex align-items-center gap-2 py-2 px-4" 
                      style="font-size: 0.85rem; font-weight: 600;"
                      @click="askAIAboutThesis(macroTheses[0])"
                      :disabled="isAskingAI"
                    >
                      <span v-if="isAskingAI" class="spinner-border spinner-border-sm me-1" role="status"></span>
                      <span v-else>💬</span>
                      Ask AI
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            <div v-else class="theses-container mb-4">
              <div class="thesis-card p-3 rounded-3" style="background: rgba(59, 130, 246, 0.03); border: 1px solid rgba(59, 130, 246, 0.1);">
                <h5 class="feature-title text-primary fw-bold mb-2"><span class="me-2">🌍</span> Nhận định Vĩ mô hiện tại:</h5>
                <p class="feature-desc mb-3" style="font-size: 0.85rem;">AI đang phân tích các luồng tin tức từ ngân hàng trung ương và thị trường tài chính để đưa ra nhận định vĩ mô mới nhất.</p>
                
                <h5 class="feature-title text-success fw-bold mb-2"><span class="me-2">🛡️</span> Chuẩn bị tài sản:</h5>
                <p class="feature-desc mb-0" style="font-size: 0.85rem;">Danh mục sẽ được tự động gợi ý điều chỉnh dựa trên rủi ro thanh khoản toàn cầu. (Đang chờ dữ liệu từ DB...)</p>
              </div>
            </div>

            <!-- Current World State Toggle & Component (OSINT) -->
            <div class="mt-4 pt-4 border-top" style="border-color: rgba(0, 0, 0, 0.06) !important;">
              <div 
                class="d-flex justify-content-between align-items-center cursor-pointer" 
                style="cursor: pointer;"
                @click="isWorldStateExpanded = !isWorldStateExpanded"
              >
                <h5 class="feature-title text-secondary fw-bold mb-0 d-flex align-items-center gap-2" style="font-size: 0.95rem;">
                  <span>🌐</span> Current World State (OSINT)
                </h5>
                <div class="d-flex align-items-center gap-1 text-primary fw-semibold" style="font-size: 0.82rem; user-select: none;">
                  <span>{{ isWorldStateExpanded ? 'Collapse' : 'Expand' }}</span>
                  <span :style="{ display: 'inline-block', transform: isWorldStateExpanded ? 'rotate(180deg)' : 'rotate(0deg)', transition: 'transform 0.2s ease' }">▼</span>
                </div>
              </div>
              
              <div v-if="isWorldStateExpanded" class="mt-4">
                <WorldStateComponent :worldState="worldState" :loading="loadingState" :borderless="true" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- RRG Section Row -->
      <div class="row mb-5">
        <div class="col-12">
          <div class="feature-panel p-0 overflow-hidden">
            <div class="panel-header-glass py-3 px-4 d-flex justify-content-between align-items-center border-bottom border-glass">
              <h3 class="panel-heading m-0 d-flex align-items-center gap-2">
                <span>🔄</span> Sector Rotation Graph (RRG)
              </h3>
              <button
                class="btn-generate d-flex align-items-center gap-2"
                @click="runSSHScript('assets_rrg')"
                :disabled="isRunningScript"
              >
                <span v-if="isRunningScript" class="spinner-border spinner-border-sm"></span>
                <span v-else style="display: inline-flex; align-items: center; gap: 6px;">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21.5 2v6h-6M21.34 15.57a10 10 0 1 1-.57-8.38l5.67-5.67"/></svg>
                  Generate RRG
                </span>
              </button>
            </div>
            
            <div class="p-4 text-center">
              <p class="text-secondary small mb-4 text-start">
                The Relative Rotation Graph (RRG) maps the relative strength and momentum of global asset classes against USD. Visualizing asset rotations helps identify leading, weakening, lagging, or improving market sectors.
              </p>
              
              <div class="rrg-frame position-relative mx-auto rounded-4 overflow-hidden shadow-lg border border-glass">
                <img :src="assetsRRGUrl" class="img-fluid rrg-image" alt="Assets RRG Chart" />
                <div class="rrg-frame-overlay"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Chart Modal -->
    <div v-if="showChartModal" class="modal-backdrop" @click="closeChartModal">
      <div class="custom-modal" @click.stop>
        <div class="modal-header d-flex justify-content-between align-items-center">
          <h5 class="mb-0">{{ selectedAsset?.name }}</h5>
          <button type="button" class="btn-close" @click="closeChartModal"></button>
        </div>
        <div class="modal-body p-0">
          <template v-if="isVnStock">
            <iframe
              :src="`https://stockchart.vietstock.vn/?stockcode=${selectedAsset.symbol}`"
              width="100%"
              height="500"
              frameborder="0"
              allowfullscreen
              style="border-radius: 0 0 16px 16px; background: #ffffff;"
            ></iframe>
          </template>
          <template v-else>
            <TradingViewChart v-if="selectedAssetChartSymbol" :coin="selectedAssetChartSymbol" :height="500" />
          </template>
        </div>
      </div>
    </div>
    <AppFooter />
  </div>
</template>

<script>
import NavBar from './NavBar.vue';
import AppFooter  from './AppFooter.vue';
import TradingViewChart from './TradingViewChart.vue';
import WorldStateComponent from './MacroIntelHub/WorldState.vue';
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useNotification } from "@kyvg/vue3-notification";
import { parseMarkdown } from '@/utils/markdown';

export default {
  name: 'HomeView',
  components: {
    NavBar,
    AppFooter,
    TradingViewChart,
    WorldStateComponent,
  },
  setup() {
    const { notify } = useNotification();
    const isRunningScript = ref(false);
    const assetsRRGKey = ref(Date.now());
    const assetsRRGUrl = computed(() => `/assets_rrgchart?t=${assetsRRGKey.value}`);
    
    // Economic Calendar state
    const calendarData = ref([]);
    const isLoadingCalendar = ref(false);
    const calendarCurrentDateTime = ref(new Date());

    const today = new Date();
    const formattedToday = today.getFullYear() + '-' + String(today.getMonth() + 1).padStart(2, '0') + '-' + String(today.getDate()).padStart(2, '0');
    const selectedDate = ref(formattedToday);

    const sortedCalendarData = computed(() => {
      let filteredData = [...calendarData.value];

      if (selectedDate.value) {
        const selected = new Date(selectedDate.value);
        filteredData = filteredData.filter(item => {
          const itemDate = new Date(item.date);
          return itemDate.getFullYear() === selected.getFullYear() &&
                 itemDate.getMonth() === selected.getMonth() &&
                 itemDate.getDate() === selected.getDate();
        });
      }

      return filteredData.sort((a, b) => new Date(a.date) - new Date(b.date));
    });

    const formatCalendarDate = (dateString) => {
      const date = new Date(dateString);
      const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
      const month = months[date.getMonth()];
      const day = String(date.getDate()).padStart(2, '0');
      const year = date.getFullYear();
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');
      return `${month} ${day}, ${year} ${hours}:${minutes}`;
    };

    const formatInputDate = (dateStr) => {
      if (!dateStr) return '';
      const parts = dateStr.split('-');
      if (parts.length !== 3) return dateStr;
      const year = parts[0];
      const monthIndex = parseInt(parts[1], 10) - 1;
      const day = parts[2];
      
      const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
      const monthName = months[monthIndex] || '';
      const daysOfWeek = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
      const date = new Date(year, monthIndex, day);
      const dayOfWeek = daysOfWeek[date.getDay()];
      return `${dayOfWeek}, ${monthName} ${day}, ${year}`;
    };

    const formattedDateLong = computed(() => {
      if (!selectedDate.value) return '';
      const parts = selectedDate.value.split('-');
      if (parts.length !== 3) return selectedDate.value;
      const date = new Date(parts[0], parts[1] - 1, parts[2]);
      return date.toLocaleDateString('en-US', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      });
    });

    const closestCalendarItem = computed(() => {
      if (sortedCalendarData.value.length === 0) {
        return null;
      }

      let minDiff = Infinity;
      let closest = null;

      for (const item of sortedCalendarData.value) {
        const itemDate = new Date(item.date);
        const diff = Math.abs(calendarCurrentDateTime.value - itemDate);
        if (diff < minDiff) {
          minDiff = diff;
          closest = item;
        }
      }
      return closest;
    });

    const isPreviousDisabled = computed(() => {
      if (!selectedDate.value) {
        return false;
      }
      const currentDate = new Date(selectedDate.value);
      currentDate.setDate(currentDate.getDate() - 1);
      const prevDateString = currentDate.getFullYear() + '-' + String(currentDate.getMonth() + 1).padStart(2, '0') + '-' + String(currentDate.getDate()).padStart(2, '0');
      return !calendarData.value.some(item => {
        const itemDate = new Date(item.date);
        const itemDateString = itemDate.getFullYear() + '-' + String(itemDate.getMonth() + 1).padStart(2, '0') + '-' + String(itemDate.getDate()).padStart(2, '0');
        return itemDateString === prevDateString;
      });
    });

    const isNextDisabled = computed(() => {
      if (!selectedDate.value) {
        return false;
      }
      const currentDate = new Date(selectedDate.value);
      currentDate.setDate(currentDate.getDate() + 1);
      const nextDateString = currentDate.getFullYear() + '-' + String(currentDate.getMonth() + 1).padStart(2, '0') + '-' + String(currentDate.getDate()).padStart(2, '0');
      return !calendarData.value.some(item => {
        const itemDate = new Date(item.date);
        const itemDateString = itemDate.getFullYear() + '-' + String(itemDate.getMonth() + 1).padStart(2, '0') + '-' + String(itemDate.getDate()).padStart(2, '0');
        return itemDateString === nextDateString;
      });
    });

    const goToPreviousDay = () => {
      if (selectedDate.value) {
        const currentDate = new Date(selectedDate.value);
        currentDate.setDate(currentDate.getDate() - 1);
        selectedDate.value = currentDate.getFullYear() + '-' + String(currentDate.getMonth() + 1).padStart(2, '0') + '-' + String(currentDate.getDate()).padStart(2, '0');
      }
    };

    const goToNextDay = () => {
      if (selectedDate.value) {
        const currentDate = new Date(selectedDate.value);
        currentDate.setDate(currentDate.getDate() + 1);
        selectedDate.value = currentDate.getFullYear() + '-' + String(currentDate.getMonth() + 1).padStart(2, '0') + '-' + String(currentDate.getDate()).padStart(2, '0');
      }
    };

    const fetchCalendarData = async () => {
      isLoadingCalendar.value = true;
      try {
        const response = await fetch('/ff_calendar_thisweek.json');
        if (response.ok) {
          calendarData.value = await response.json();
        }
      } catch (error) {
        console.error('Error fetching calendar data:', error);
      } finally {
        isLoadingCalendar.value = false;
      }
    };

    let calendarInterval = null;
    
    const showChartModal = ref(false);
    const selectedAsset = ref(null);
    const macroTheses = ref([]);
    const loadingTheses = ref(true);
    const selectedAssetChartSymbol = computed(() => {
      if (!selectedAsset.value) return '';
      let sym = selectedAsset.value.symbol;
      if (selectedAsset.value.assetType === 'futures' && sym.toUpperCase().endsWith('USDT')) {
        return `BINANCE:${sym}.P`;
      }
      if (selectedAsset.value.assetType === 'stock') {
        if (sym === 'SPX') return 'SP:SPX';
        // For non-VN stocks, return the symbol as-is (e.g. NYSE:AAPL)
      }
      if (selectedAsset.value.assetType === 'forex' && !sym.includes(':')) {
        return `FX:${sym}`;
      }
      if (selectedAsset.value.assetType === 'commodities') {
        const commodityMap = {
          'GC=F': 'OANDA:XAUUSD',
          'SI=F': 'OANDA:XAGUSD',
          'CL=F': 'TVC:USOIL',
          'BZ=F': 'TVC:UKOIL'
        };
        return commodityMap[sym] || sym;
      }
      return sym;
    });

    const isVnStock = computed(() => {
      if (!selectedAsset.value) return false;
      if (selectedAsset.value.assetType !== 'stock') return false;
      if (selectedAsset.value.isUS) return false;
      if (selectedAsset.value.message && selectedAsset.value.message.includes('Stock US')) return false;
      
      let sym = selectedAsset.value.symbol;
      return !sym.includes(':') && sym !== 'SPX';
    });

    const openChartModal = (asset) => {
      selectedAsset.value = asset;
      showChartModal.value = true;
    };

    const closeChartModal = () => {
      showChartModal.value = false;
      selectedAsset.value = null;
    };

    const defaultAssets = [
      {
        name: 'Stocks (VN-Index)',
        price: '1,280.50 pts',
        change: '+0.75%',
        positive: true,
        emoji: '📈',
        iconBg: 'rgba(16, 185, 129, 0.1)',
        link: '/stock',
        sparkline: 'M 0 25 L 20 22 L 40 18 L 60 10 L 80 15 L 100 5',
        message: 'Dữ liệu thị trường mô phỏng VN-Index',
        symbol: 'VNINDEX',
        assetType: 'stock'
      },
      {
        name: 'Crypto (BTCUSDT)',
        price: '$68,420.00',
        change: '+4.12%',
        positive: true,
        emoji: '₿',
        iconBg: 'rgba(245, 158, 11, 0.1)',
        link: '/crypto',
        sparkline: 'M 0 25 L 20 20 L 40 24 L 60 12 L 80 8 L 100 2',
        message: 'Dữ liệu thị trường mô phỏng Bitcoin Spot',
        symbol: 'BTCUSDT',
        assetType: 'crypto'
      },
      {
        name: 'Forex (EURUSD)',
        price: '1.0850',
        change: '-0.15%',
        positive: false,
        emoji: '💱',
        iconBg: 'rgba(239, 68, 68, 0.1)',
        link: '/forex',
        sparkline: 'M 0 10 L 20 15 L 40 8 L 60 18 L 80 16 L 100 24',
        message: 'Dữ liệu thị trường mô phỏng EUR/USD',
        symbol: 'FX:EURUSD',
        assetType: 'forex'
      },
      {
        name: 'Commodities (Gold)',
        price: '$2,342.50 / oz',
        change: '+1.28%',
        positive: true,
        emoji: '🏆',
        iconBg: 'rgba(234, 179, 8, 0.1)',
        link: '/commodities',
        sparkline: 'M 0 20 L 20 18 L 40 12 L 60 15 L 80 5 L 100 8',
        message: 'Dữ liệu thị trường mô phỏng Gold',
        symbol: 'GC=F',
        assetType: 'commodities'
      },
      {
        name: 'Futures (VN30F1M)',
        price: '1,295.20 pts',
        change: '+0.85%',
        positive: true,
        emoji: '📊',
        iconBg: 'rgba(59, 130, 246, 0.1)',
        link: '/futures',
        sparkline: 'M 0 24 L 20 22 L 40 16 L 60 12 L 80 18 L 100 8',
        message: 'Dữ liệu thị trường mô phỏng VN30 Phái sinh',
        symbol: 'VN30F1M',
        assetType: 'futures'
      },
      {
        name: 'Stocks (S&P 500)',
        price: '5,250.25 pts',
        change: '+0.45%',
        positive: true,
        emoji: '🏛️',
        iconBg: 'rgba(16, 185, 129, 0.1)',
        link: '/stock',
        sparkline: 'M 0 20 L 20 22 L 40 18 L 60 25 L 80 15 L 100 12',
        message: 'Dữ liệu thị trường mô phỏng S&P 500',
        symbol: 'SPX',
        assetType: 'stock'
      }
    ];

    const marketAssets = ref([...defaultAssets]);

    const positiveSparklines = [
      'M 0 25 L 20 22 L 40 18 L 60 10 L 80 15 L 100 5',
      'M 0 25 L 20 20 L 40 24 L 60 12 L 80 8 L 100 2',
      'M 0 24 L 20 22 L 40 16 L 60 12 L 80 18 L 100 8',
      'M 0 22 L 20 18 L 40 20 L 60 10 L 80 8 L 100 4'
    ];

    const negativeSparklines = [
      'M 0 10 L 20 15 L 40 8 L 60 18 L 80 16 L 100 24',
      'M 0 5 L 20 12 L 40 10 L 60 18 L 80 20 L 100 25',
      'M 0 8 L 20 14 L 40 12 L 60 22 L 80 18 L 100 26'
    ];

    const getSparkline = (symbol, positive) => {
      const list = positive ? positiveSparklines : negativeSparklines;
      let hash = 0;
      for (let i = 0; i < symbol.length; i++) {
        hash += symbol.charCodeAt(i);
      }
      return list[hash % list.length];
    };

    const getRelativeTime = (timeStr) => {
      try {
        const d = new Date(timeStr);
        const diffMs = Date.now() - d.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        if (diffMins < 1) return 'Vừa xong';
        if (diffMins < 60) return `${diffMins} phút trước`;
        const diffHours = Math.floor(diffMins / 60);
        if (diffHours < 24) return `${diffHours} giờ trước`;
        const diffDays = Math.floor(diffHours / 24);
        return `${diffDays} ngày trước`;
      } catch (e) {
        return '';
      }
    };

    const formatPrice = (price, assetType) => {
      if (assetType === 'stock') {
        if (price > 1000) {
          // VN stock price (VND)
          return `${price.toLocaleString('vi-VN')} đ`;
        } else {
          // US stock price (USD)
          return `$${price.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
        }
      } else if (assetType === 'crypto' || assetType === 'futures' || assetType === 'commodities' || assetType === 'forex') {
        let minFractionDigits = 2;
        if (assetType === 'forex') {
          minFractionDigits = 4;
        } else if (price < 1) {
          minFractionDigits = 4;
        }
        return `$${price.toLocaleString('en-US', { minimumFractionDigits: minFractionDigits })}`;
      }
      return price.toLocaleString();
    };

    const parseAlertChange = (msg) => {
      const lowerMsg = msg.toLowerCase();
      if (lowerMsg.includes('bán') || lowerMsg.includes('sell') || lowerMsg.includes('giảm')) {
        return { change: 'SELL', positive: false };
      }
      if (lowerMsg.includes('bứt phá') || lowerMsg.includes('vượt đỉnh') || lowerMsg.includes('breakout') || lowerMsg.includes('tăng')) {
        return { change: 'BREAKOUT', positive: true };
      }
      return { change: 'ALERT', positive: true };
    };

    const fetchLatestAlerts = async () => {
      try {
        const response = await fetch('/triggeredAlerts?limit=50');
        if (!response.ok) throw new Error('Failed to fetch alerts');
        const data = await response.json();
        
        if (data && data.length > 0) {
          const seenSymbols = new Set();
          const mappedAlerts = [];
          
          for (const alert of data) {
            const key = `${alert.asset_type}-${alert.symbol}`;
            if (seenSymbols.has(key)) continue;
            seenSymbols.add(key);

            const parsed = parseAlertChange(alert.message);
            
            let name = '';
            let emoji = '🔔';
            let iconBg = 'rgba(139, 92, 246, 0.1)';
            let link = '/';
            
            let isUS = false;
            if (alert.asset_type === 'stock') {
              isUS = alert.symbol.includes(':') || alert.symbol.length > 3 || alert.message.includes('Stock US');
              name = `${isUS ? 'US Stock' : 'VN Stock'} (${alert.symbol.split(':').pop()})`;
              emoji = '📈';
              iconBg = 'rgba(16, 185, 129, 0.1)';
              link = '/stock';
            } else if (alert.asset_type === 'crypto') {
              name = `Crypto (${alert.symbol})`;
              emoji = '₿';
              iconBg = 'rgba(245, 158, 11, 0.1)';
              link = '/crypto';
            } else if (alert.asset_type === 'futures') {
              name = `Futures (${alert.symbol})`;
              emoji = '📊';
              iconBg = 'rgba(59, 130, 246, 0.1)';
              link = '/futures';
            } else if (alert.asset_type === 'commodities' || alert.asset_type === 'gold' || alert.asset_type === 'silver' || alert.asset_type === 'oil') {
              const commodityNames = {
                'GC=F': 'Vàng (Gold)',
                'XAUUSD': 'Vàng (Gold)',
                'SI=F': 'Bạc (Silver)',
                'XAGUSD': 'Bạc (Silver)',
                'BZ=F': 'Dầu Brent (UKOIL)',
                'UKOIL': 'Dầu Brent (UKOIL)',
                'CL=F': 'Dầu WTI (USOIL)',
                'USOIL': 'Dầu WTI (USOIL)'
              };
              const comName = commodityNames[alert.symbol] || alert.symbol;
              name = `${comName}`;
              emoji = (alert.symbol === 'GC=F' || alert.symbol === 'XAUUSD' || alert.asset_type === 'gold') ? '🏆' : 
                      ((alert.symbol === 'SI=F' || alert.symbol === 'XAGUSD' || alert.asset_type === 'silver') ? '🥈' : '🛢️');
              iconBg = 'rgba(234, 179, 8, 0.1)';
              link = '/commodities';
            } else if (alert.asset_type === 'forex') {
              name = `Forex (${alert.symbol})`;
              emoji = '💱';
              iconBg = 'rgba(139, 92, 246, 0.1)';
              link = '/forex';
            } else {
              name = `${alert.asset_type.toUpperCase()} (${alert.symbol})`;
            }

            mappedAlerts.push({
              name,
              price: formatPrice(alert.price, alert.asset_type),
              change: parsed.change,
              positive: parsed.positive,
              emoji,
              iconBg,
              link,
              sparkline: getSparkline(alert.symbol, parsed.positive),
              message: alert.message,
              relativeTime: getRelativeTime(alert.created_at),
              symbol: alert.symbol,
              assetType: alert.asset_type,
              isUS: isUS
            });
          }

          marketAssets.value = mappedAlerts.length > 0 ? mappedAlerts : [...defaultAssets];
        } else {
          marketAssets.value = [...defaultAssets];
        }
      } catch (error) {
        console.error('Error loading latest alerts:', error);
        marketAssets.value = [...defaultAssets];
      }
    };

    let pollInterval = null;
    let thesesInterval = null;

    onMounted(() => {
      fetchLatestAlerts();
      fetchMacroTheses();
      fetchWorldState();
      fetchCalendarData();
      // Poll every 15 seconds to fetch latest real-time alerts
      pollInterval = setInterval(fetchLatestAlerts, 15000);
      // Auto refresh theses + world state every 5 minutes (300,000ms)
      thesesInterval = setInterval(() => {
        fetchMacroTheses();
        fetchWorldState();
      }, 300000);
      calendarInterval = setInterval(() => {
        calendarCurrentDateTime.value = new Date();
      }, 1000);
      // Start JS-driven marquee auto-scroll
      startMarqueeScroll();

      // Register mouse/touch drag listeners for scrollbar tracking
      if (marqueeContainer.value) {
        marqueeContainer.value.addEventListener('mousedown', onMarqueeMouseDown);
        marqueeContainer.value.addEventListener('touchstart', onMarqueeTouchStart, { passive: true });
      }
      window.addEventListener('mouseup', onWindowMouseUp);
      window.addEventListener('touchend', onWindowTouchEnd);
    });

    const fetchMacroTheses = async (forceRefresh = false) => {
      loadingTheses.value = true;
      try {
        const token = localStorage.getItem('token');
        let userId = '';
        try {
          const userInfoStr = localStorage.getItem('userInfo');
          if (userInfoStr) {
            const userInfo = JSON.parse(userInfoStr);
            userId = userInfo.id || userInfo.user_id || userInfo.userId || '';
          }
        } catch (e) {
          console.error('Error parsing userInfo', e);
        }
        const headers = token ? { 'Authorization': `Bearer ${token}` } : {};
        let url = '/api/osint/theses';
        const params = [];
        if (userId) {
          params.push(`user_id=${userId}`);
        }
        if (forceRefresh) {
          params.push('refresh=true');
        }
        if (params.length > 0) {
          url += `?${params.join('&')}`;
        }
        const response = await fetch(url, { headers });
        if (response.ok) {
          const data = await response.json();
          macroTheses.value = data || [];
        }
      } catch (error) {
        console.error('Error fetching theses:', error);
      } finally {
        loadingTheses.value = false;
      }
    };

    const runningAI = ref(false);
    const runAIAnalysis = async () => {
      runningAI.value = true;
      try {
        const response = await fetch('/api/osint/theses/trigger', {
          method: 'POST'
        });
        if (!response.ok) {
          const errData = await response.json().catch(() => ({}));
          alert(errData.message || 'Lỗi khi chạy phân tích AI');
        } else {
          // Re-fetch macro theses after successful update
          await fetchMacroTheses(true);
          await fetchWorldState();
          alert('Cập nhật nhận định vĩ mô thành công!');
        }
      } catch (error) {
        console.error('Error running AI analysis:', error);
        alert('Lỗi kết nối khi chạy phân tích AI');
      } finally {
        runningAI.value = false;
      }
    };

    const refreshThesesManual = () => {
      fetchMacroTheses(true);
      fetchWorldState();
    };

    onUnmounted(() => {
      if (pollInterval) {
        clearInterval(pollInterval);
      }
      if (calendarInterval) {
        clearInterval(calendarInterval);
      }
      if (thesesInterval) {
        clearInterval(thesesInterval);
      }
      // Stop JS-driven marquee
      stopMarqueeScroll();
      if (marqueeResumeTimeout) clearTimeout(marqueeResumeTimeout);

      // Clean up mouse/touch drag listeners
      if (marqueeContainer.value) {
        marqueeContainer.value.removeEventListener('mousedown', onMarqueeMouseDown);
        marqueeContainer.value.removeEventListener('touchstart', onMarqueeTouchStart);
      }
      window.removeEventListener('mouseup', onWindowMouseUp);
      window.removeEventListener('touchend', onWindowTouchEnd);
    });

    const runSSHScript = async (scriptType) => {
      isRunningScript.value = true;
      try {
        const response = await fetch('/runSSHScript', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ script_type: scriptType }),
        });
        const data = await response.json();
        if (response.ok && data.success) {
          notify({
            type: 'success',
            title: 'Success',
            text: 'Assets RRG Chart has been updated successfully!',
          });
          assetsRRGKey.value = Date.now();
        } else {
          throw new Error(data.error || 'Server returned an error');
        }
      } catch (error) {
        console.error('Error running SSH script:', error);
        notify({
          type: 'error',
          title: 'Execution Failed',
          text: error.message || 'Failed to connect or run the SSH script.',
        });
      } finally {
        isRunningScript.value = false;
      }
    };



    // --- JS-driven marquee with manual scroll-back support ---
    const marqueeContainer = ref(null);
    const marqueeContent = ref(null);
    let marqueeAnimFrame = null;
    const marqueeSpeed = 0.6; // pixels per frame (~36px/s at 60fps)
    let marqueePaused = false;
    let marqueeUserScrolling = false;
    let marqueeResumeTimeout = null;
    let totalTrackWidth = 0;
    // const scrollBuffer = 500; // Buffer space at ends of track to wrap seamlessly

    // Computed property to repeat assets to ensure track width is larger than screen width
    const scrollingAssets = computed(() => {
      const list = marketAssets.value.slice(1);
      if (list.length === 0) return [];
      const repeated = [];
      while (repeated.length < 15) {
        repeated.push(...list);
      }
      return repeated;
    });

    const stepMarquee = () => {
      if (!marqueeContainer.value || !marqueeContent.value) {
        marqueeAnimFrame = requestAnimationFrame(stepMarquee);
        return;
      }
      // Keep recalculating until we have a valid width
      if (totalTrackWidth === 0) {
        recalcTrackWidth();
      }
      if (totalTrackWidth === 0) {
        // Still no width, retry next frame
        marqueeAnimFrame = requestAnimationFrame(stepMarquee);
        return;
      }
      if (!marqueePaused && !marqueeUserScrolling) {
        marqueeContainer.value.scrollLeft += marqueeSpeed;
      }
      marqueeAnimFrame = requestAnimationFrame(stepMarquee);
    };

    const recalcTrackWidth = () => {
      if (!marqueeContainer.value) return;
      const el = marqueeContainer.value.querySelector('.marquee-js-content');
      if (el) {
        totalTrackWidth = el.scrollWidth;
      }
    };

    const startMarqueeScroll = () => {
      recalcTrackWidth();
      if (marqueeAnimFrame) return;
      marqueeAnimFrame = requestAnimationFrame(stepMarquee);
    };

    const stopMarqueeScroll = () => {
      if (marqueeAnimFrame) {
        cancelAnimationFrame(marqueeAnimFrame);
        marqueeAnimFrame = null;
      }
    };

    const pauseMarquee = () => {
      marqueePaused = true;
    };

    const resumeMarquee = () => {
      marqueePaused = false;
    };

    // Continuous infinite scroll wrapping logic
    const handleMarqueeScroll = () => {
      if (!marqueeContainer.value || totalTrackWidth === 0) return;
      const container = marqueeContainer.value;
      const scrollLeft = container.scrollLeft;
      const halfWidth = totalTrackWidth / 2;

      // Only wrap when user is not manually interacting to prevent scrollbar jumping
      if (!marqueeUserScrolling) {
        if (scrollLeft >= halfWidth) {
          container.scrollLeft = scrollLeft - halfWidth;
        }
      }
    };

    // Manual navigation buttons scroll method
    const scrollMarquee = (direction) => {
      if (!marqueeContainer.value) return;
      marqueeUserScrolling = true;

      const scrollAmount = 300; // Scroll by roughly 2 cards
      const container = marqueeContainer.value;
      const targetScroll = direction === 'left'
        ? container.scrollLeft - scrollAmount
        : container.scrollLeft + scrollAmount;

      container.scrollTo({
        left: targetScroll,
        behavior: 'smooth'
      });

      if (marqueeResumeTimeout) clearTimeout(marqueeResumeTimeout);
      marqueeResumeTimeout = setTimeout(() => {
        marqueeUserScrolling = false;
      }, 2000); // Resume auto-scroll after 2s of inactivity
    };

    const onMarqueeMouseDown = () => {
      marqueeUserScrolling = true;
      if (marqueeResumeTimeout) clearTimeout(marqueeResumeTimeout);
    };

    const onWindowMouseUp = () => {
      if (marqueeUserScrolling) {
        if (marqueeResumeTimeout) clearTimeout(marqueeResumeTimeout);
        marqueeResumeTimeout = setTimeout(() => {
          marqueeUserScrolling = false;
        }, 2000);
      }
    };

    const onMarqueeTouchStart = () => {
      marqueeUserScrolling = true;
      if (marqueeResumeTimeout) clearTimeout(marqueeResumeTimeout);
    };

    const onWindowTouchEnd = () => {
      if (marqueeUserScrolling) {
        if (marqueeResumeTimeout) clearTimeout(marqueeResumeTimeout);
        marqueeResumeTimeout = setTimeout(() => {
          marqueeUserScrolling = false;
        }, 2000);
      }
    };

    const onMarqueeWheel = (event) => {
      if (!marqueeContainer.value) return;
      // Convert vertical wheel to horizontal scroll
      event.preventDefault();
      marqueeUserScrolling = true;
      marqueeContainer.value.scrollLeft += event.deltaY;

      // Reset user-scrolling flag after they stop interacting
      if (marqueeResumeTimeout) clearTimeout(marqueeResumeTimeout);
      marqueeResumeTimeout = setTimeout(() => {
        marqueeUserScrolling = false;
      }, 1000);
    };
    // --- End JS marquee ---

    const isLoggedIn = ref(!!localStorage.getItem('token'));

    const worldState = ref({});
    const loadingState = ref(false);
    const isWorldStateExpanded = ref(false);

    const authHeader = () => {
      const token = localStorage.getItem('token');
      return token ? { 'Authorization': `Bearer ${token}` } : {};
    };

    const fetchWorldState = () => {
      loadingState.value = true;
      fetch('/api/osint/world-state', { headers: authHeader() })
        .then(r => r.json())
        .then(data => worldState.value = data || {})
        .catch(e => console.error('fetchWorldState error:', e))
        .finally(() => loadingState.value = false);
    };

    const isAskingAI = ref(false);

    const askAIAboutThesis = async (thesis) => {
      isAskingAI.value = true;
      try {
        const response = await fetch('/api/news/telegram');
        let telegramContext = "";
        if (response.ok) {
          const data = await response.json();
          const channels = data.channels || [];
          const news = data.news || {};
          for (const channel of channels) {
            const items = news[channel] || [];
            if (items.length > 0) {
              telegramContext += `Kênh ${channel}:\n`;
              for (let i = 0; i < Math.min(items.length, 3); i++) {
                const descClean = items[i].description ? items[i].description.replace(/<[^>]*>/g, '').replace(/\s+/g, ' ').substring(0, 150) : "";
                telegramContext += `- [${new Date(items[i].date_published).toLocaleDateString('vi-VN')}] ${items[i].title}: ${descClean}\n`;
              }
              telegramContext += `\n`;
            }
          }
        }
        
        window.dispatchEvent(new CustomEvent('open-chat-with-context', {
          detail: {
            thesis: thesis.thesis,
            advice: thesis.supporting_evidence,
            telegramContext: telegramContext.trim()
          }
        }));
      } catch (error) {
        console.error('Error fetching telegram news for chat context:', error);
        window.dispatchEvent(new CustomEvent('open-chat-with-context', {
          detail: {
            thesis: thesis.thesis,
            advice: thesis.supporting_evidence,
            telegramContext: ""
          }
        }));
      } finally {
        isAskingAI.value = false;
      }
    };

    const formatDateWithOffset = (dateString) => {
      if (!dateString) return '';
      try {
        const d = new Date(dateString);
        const formatted = d.toLocaleString(undefined, {
          year: 'numeric',
          month: 'numeric',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
        
        // Calculate dynamic timezone offset string
        const offset = -d.getTimezoneOffset();
        const sign = offset >= 0 ? '+' : '-';
        const hours = Math.floor(Math.abs(offset) / 60);
        const minutes = Math.abs(offset) % 60;
        const minutesStr = minutes > 0 ? `:${String(minutes).padStart(2, '0')}` : '';
        const offsetStr = `(UTC${sign}${hours}${minutesStr})`;
        
        return `${formatted} ${offsetStr}`;
      } catch (e) {
        return dateString;
      }
    };

    const formatThesisText = (text) => {
      if (!text) return '';
      // Preprocess to insert newlines before list items of form "**Item**:"
      let formatted = text.replace(/\s+(\*\*[^*]+\*\*:)/g, '\n$1');
      return parseMarkdown(formatted);
    };

    return {
      formatDateWithOffset,
      isRunningScript,
      assetsRRGUrl,
      runSSHScript,
      marketAssets,
      scrollingAssets,
      marqueeContainer,
      marqueeContent,
      pauseMarquee,
      resumeMarquee,
      onMarqueeWheel,
      handleMarqueeScroll,
      scrollMarquee,
      showChartModal,
      selectedAsset,
      selectedAssetChartSymbol,
      isVnStock,
      openChartModal,
      closeChartModal,
      macroTheses,
      loadingTheses,
      isLoggedIn,
      worldState,
      loadingState,
      isWorldStateExpanded,
      calendarData,
      isLoadingCalendar,
      selectedDate,
      sortedCalendarData,
      formatCalendarDate,
      formattedDateLong,
      isAskingAI,
      askAIAboutThesis,
      formatThesisText,
      closestCalendarItem,
      isPreviousDisabled,
      isNextDisabled,
      goToPreviousDay,
      goToNextDay,
      refreshThesesManual,
      formatInputDate,
      runningAI,
      runAIAnalysis
    };
  }
}
</script>

<style scoped>
/* ── Hero Section ────────────────────────────────────── */
.hero-section {
  background: linear-gradient(135deg, #eff6ff 0%, #ffffff 100%);
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
}
.hero-glow-1 {
  position: absolute;
  top: -100px;
  left: 25%;
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, rgba(59, 130, 246, 0.04) 0%, transparent 70%);
  filter: blur(50px);
}
.hero-glow-2 {
  position: absolute;
  top: -50px;
  right: 25%;
  width: 350px;
  height: 350px;
  background: radial-gradient(circle, rgba(139, 92, 246, 0.03) 0%, transparent 70%);
  filter: blur(40px);
}
.hero-badge {
  background: rgba(59, 130, 246, 0.08);
  border: 1px solid rgba(59, 130, 246, 0.15);
  color: #2563eb;
  font-weight: 700;
  letter-spacing: 0.5px;
  font-size: 0.72rem;
  border-radius: 999px;
  display: inline-block;
}
.hero-title {
  font-family: 'Outfit', sans-serif;
  font-weight: 900;
  font-size: 2.6rem;
  letter-spacing: -0.5px;
  background: linear-gradient(135deg, #0f172a 0%, #2563eb 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
.hero-subtitle {
  font-size: 1.05rem;
  color: #475569;
  max-width: 680px;
  line-height: 1.6;
}

/* ── Buttons ─────────────────────────────────────────── */
.btn-glow {
  padding: 11px 24px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.92rem;
  text-decoration: none;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.btn-glow--primary {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: #fff;
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 4px 20px rgba(37, 99, 235, 0.25);
}
.btn-glow--primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 25px rgba(37, 99, 235, 0.35);
  color: #fff;
}
.btn-glow--secondary {
  background: rgba(0, 0, 0, 0.02);
  color: #475569;
  border: 1px solid rgba(0, 0, 0, 0.06);
}
.btn-glow--secondary:hover {
  background: rgba(0, 0, 0, 0.05);
  border-color: rgba(0, 0, 0, 0.12);
  transform: translateY(-2px);
  color: #0f172a;
}

/* ── Market Grid ─────────────────────────────────────── */
.market-card-link {
  text-decoration: none;
  color: inherit;
  display: block;
}
.market-card {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: 16px;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.02);
}
.market-card:hover {
  transform: translateY(-4px);
  border-color: rgba(59, 130, 246, 0.25);
  background: #ffffff;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.06), 0 0 15px rgba(59, 130, 246, 0.04);
}
.market-card__icon {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.15rem;
}
.market-card__change {
  font-weight: 700;
  font-size: 0.8rem;
  padding: 2px 8px;
  border-radius: 20px;
  background: rgba(0,0,0,0.02);
}
.text-neon-green {
  color: #059669;
  text-shadow: none;
}
.text-neon-red {
  color: #dc2626;
  text-shadow: none;
}
.market-card__title {
  font-size: 0.82rem;
  font-weight: 600;
  color: #475569;
  margin-bottom: 4px;
}
.market-card__price {
  font-size: 1.25rem;
  font-weight: 800;
  color: #0f172a;
}
.market-card__sparkline {
  height: 30px;
}
.sparkline-svg {
  width: 100%;
  height: 100%;
  overflow: visible;
}

/* ── Group & Marquee ─────────────────────────────────── */
.group-title {
  font-family: 'Outfit', sans-serif;
  font-size: 1.25rem;
  font-weight: 700;
  color: #1e293b;
}

.marquee-container {
  overflow: hidden;
  white-space: nowrap;
  display: flex;
  width: 100%;
  position: relative;
}
.marquee-content {
  display: flex;
  width: max-content;
  height: 100%;
  animation: marquee-left linear infinite;
}
.marquee-track {
  display: flex;
  height: 100%;
  gap: 1.5rem;
  padding-right: 1.5rem;
}
.market-card-wrapper {
  width: 280px;
  flex-shrink: 0;
  white-space: normal;
}
@keyframes marquee-left {
  0% {
    transform: translateX(0%);
  }
  100% {
    transform: translateX(-50%);
  }
}


/* ── Panels ─────────────────────────────────────────── */
.feature-panel {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.03);
}
.panel-heading {
  font-family: 'Outfit', sans-serif;
  font-size: 1.15rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}
.feature-icon {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.15rem;
  flex-shrink: 0;
}
.bg-blue { background: rgba(59, 130, 246, 0.08); color: #2563eb; }
.bg-green { background: rgba(16, 185, 129, 0.08); color: #059669; }
.bg-gold { background: rgba(245, 158, 11, 0.08); color: #d97706; }

.feature-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 2px;
}
.feature-desc {
  font-size: 0.8rem;
  color: #475569;
  line-height: 1.5;
}

/* ── RRG Section ─────────────────────────────────────── */
.panel-header-glass {
  background: #f8fafc;
}
.border-bottom {
  border-bottom: 1px solid rgba(0, 0, 0, 0.06) !important;
}
.btn-generate {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: #fff;
  border: 1px solid rgba(255, 255, 255, 0.1);
  padding: 6px 16px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.8rem;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
}
.btn-generate:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 16px rgba(37, 99, 235, 0.3);
}
.btn-generate:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.rrg-frame {
  max-width: 100%;
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.06) !important;
}
.rrg-image {
  max-width: 100%;
  height: auto;
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}
.rrg-frame:hover .rrg-image {
  transform: scale(1.015);
}
.border-glass {
  border-color: rgba(0, 0, 0, 0.06) !important;
}

/* ── Modal ───────────────────────────────────────────── */
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1050;
  display: flex;
  align-items: center;
  justify-content: center;
}
.custom-modal {
  background: #fff;
  border-radius: 12px;
  width: 90%;
  max-width: 900px;
  max-height: 90vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.modal-header {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
}
.modal-body {
  padding: 1rem;
  overflow-y: auto;
}

/* ---------- ECONOMIC CALENDAR STYLES ---------- */
.stk-panel { background: #ffffff; border: 1px solid rgba(0, 0, 0, 0.06); border-radius: 16px; box-shadow: 0 8px 32px rgba(0,0,0,0.04); overflow: hidden; margin-bottom: 20px; }
.stk-header { display: flex; align-items: center; gap: 14px; padding: 22px 24px; background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); color: #0f172a; border-bottom: 1px solid rgba(0, 0, 0, 0.06); }
.stk-header__icon { width: 44px; height: 44px; border-radius: 12px; background: #ffffff; border: 1px solid rgba(0, 0, 0, 0.08); display: flex; align-items: center; justify-content: center; flex-shrink: 0; color: #0f172a; }
.stk-header__title { font-size: 1.2rem; font-weight: 700; margin: 0; line-height: 1.3; font-family: 'Outfit', sans-serif; color: #0f172a; }
.stk-header__sub { font-size: 0.82rem; color: #475569; margin: 2px 0 0; }
.stk-section { padding: 20px 24px; }
.stk-label { display: block; font-size: 0.82rem; font-weight: 600; color: #475569; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
.stk-input {
  width: 100%; padding: 9px 14px; border: 1px solid rgba(0, 0, 0, 0.1); border-radius: 8px;
  font-size: 0.85rem; color: #0f172a; background: #ffffff; transition: border-color 0.2s, box-shadow 0.2s; outline: none;
}
.stk-input:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.15); }
.stk-table-wrap { border-radius: 10px; border: 1px solid rgba(0, 0, 0, 0.06); overflow: hidden; background: #ffffff; }
.stk-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
.stk-th {
  padding: 10px 14px; text-align: left; font-size: 0.72rem; font-weight: 700;
  text-transform: uppercase; letter-spacing: 0.5px; color: #475569; background: #f1f5f9;
  border-bottom: 2px solid #e2e8f0; position: sticky; top: 0; z-index: 2;
}
.stk-th--right { text-align: right; }
.stk-row { cursor: pointer; transition: background 0.15s ease; }
.stk-row:hover { background: #f8fafc; }
.stk-row--active { background: rgba(59, 130, 246, 0.05) !important; }
.stk-td { padding: 10px 14px; border-bottom: 1px solid rgba(0, 0, 0, 0.04); vertical-align: middle; color: #334155; }
.stk-td--right { text-align: right; }
.stk-signal { display: inline-block; padding: 3px 10px; border-radius: 999px; font-size: 0.75rem; font-weight: 600; white-space: nowrap; }
.stk-signal--low { background: rgba(16, 185, 129, 0.08); color: #059669; border: 1px solid rgba(16, 185, 129, 0.2); }
.stk-signal--medium { background: rgba(245, 158, 11, 0.08); color: #d97706; border: 1px solid rgba(245, 158, 11, 0.2); }
.stk-signal--high { background: rgba(239, 68, 68, 0.08); color: #dc2626; border: 1px solid rgba(239, 68, 68, 0.2); box-shadow: 0 0 8px rgba(239, 68, 68, 0.1); }
.stk-btn {
  display: inline-flex; align-items: center; gap: 6px; padding: 9px 18px;
  border: none; border-radius: 8px; font-size: 0.84rem; font-weight: 600;
  cursor: pointer; transition: all 0.2s ease; white-space: nowrap;
}
.stk-btn:disabled { opacity: 0.45; cursor: not-allowed; }
.stk-btn--outline { background: rgba(255,255,255,0.03); color: #475569; border: 1px solid rgba(0, 0, 0, 0.08); }
.stk-btn--outline:hover:not(:disabled) { background: rgba(0, 0, 0, 0.02); }
.stk-loading { display: flex; justify-content: center; padding: 20px 0; }
.stk-spinner { width: 32px; height: 32px; border: 3px solid rgba(0, 0, 0, 0.06); border-top-color: #3b82f6; border-radius: 50%; animation: stk-spin 0.7s linear infinite; }
@keyframes stk-spin { to { transform: rotate(360deg); } }
.stk-message { text-align: center; font-size: 0.85rem; color: #64748b; padding: 10px 0; margin: 0; }

/* Mini Scale Marquee Styling */
.group-title--mini {
  font-size: 0.82rem !important;
  color: #334155 !important;
  font-weight: 700 !important;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.market-card-wrapper--mini {
  width: 140px !important;
}
.marquee-track--mini {
  gap: 0.5rem !important;
  padding-right: 0.5rem !important;
}
.market-card--mini {
  padding: 6px 10px !important;
  border-radius: 8px !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02) !important;
}
.market-card--mini .market-card__icon {
  width: 18px !important;
  height: 18px !important;
  font-size: 0.7rem !important;
  border-radius: 4px !important;
}
.market-card--mini .market-card__change {
  font-size: 0.55rem !important;
  padding: 0.5px 4px !important;
}
.market-card--mini .market-card__title {
  font-size: 0.65rem !important;
  margin-bottom: 1px !important;
}
.market-card--mini .market-card__price {
  font-size: 0.75rem !important;
  font-weight: 800 !important;
}
.market-card--mini .market-card__time {
  font-size: 0.52rem !important;
  line-height: 0.8rem !important;
  height: 0.8rem !important;
  margin-top: 1px !important;
}
.market-card--mini .market-card__sparkline {
  height: 12px !important;
  margin-top: 4px !important;
}

.thesis-card :deep(.ai-list-item) {
  list-style-type: none;
  position: relative;
  padding-left: 1.25rem;
  margin-bottom: 0.5rem;
  font-size: 0.95rem;
  line-height: 1.7;
}
.thesis-card :deep(.ai-list-item::before) {
  content: "•";
  color: #3b82f6;
  font-weight: bold;
  display: inline-block;
  width: 1rem;
  margin-left: -1rem;
  position: absolute;
  left: 0.25rem;
}

/* Custom Horizontal Scroll styling */
.custom-horizontal-scroll::-webkit-scrollbar {
  height: 6px;
}
.custom-horizontal-scroll::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.02);
  border-radius: 4px;
}
.custom-horizontal-scroll::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.12);
  border-radius: 4px;
  transition: background 0.2s;
}
.custom-horizontal-scroll::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.25);
}

/* Marquee Navigation Buttons */
.marquee-nav-btn {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(0, 0, 0, 0.08);
  color: #1e293b;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  opacity: 0;
  pointer-events: none;
}
.position-relative:hover .marquee-nav-btn {
  opacity: 1;
  pointer-events: auto;
}
.marquee-nav-btn:hover {
  background: #3b82f6;
  color: #ffffff;
  border-color: #3b82f6;
  box-shadow: 0 6px 20px rgba(59, 130, 246, 0.35);
  transform: translateY(-50%) scale(1.12);
}
.marquee-nav-btn:active {
  transform: translateY(-50%) scale(0.95);
}
.marquee-nav-btn--left {
  left: 8px;
}
.marquee-nav-btn--right {
  right: 8px;
}

/* Ensure horizontal scroll hides default ugly browser scrollbars but allows our custom one */
.custom-horizontal-scroll {
  scrollbar-width: thin;
  scrollbar-color: rgba(0, 0, 0, 0.12) transparent;
}

/* Separator between loop cycles in marquee */
.marquee-separator {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  flex-shrink: 0;
  height: 48px;
  align-self: center;
}
.marquee-separator-line {
  width: 2px;
  height: 100%;
  background: linear-gradient(to bottom, transparent, rgba(59, 130, 246, 0.3) 20%, rgba(59, 130, 246, 0.3) 80%, transparent);
  border-radius: 99px;
  position: relative;
}
.marquee-separator-dot {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 6px;
  height: 6px;
  background-color: #3b82f6;
  border-radius: 50%;
  box-shadow: 0 0 8px rgba(59, 130, 246, 0.6);
}
</style>
