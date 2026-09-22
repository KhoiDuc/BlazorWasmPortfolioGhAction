<template>
  <div id="app" class="d-flex flex-column min-vh-100">
    <NavBar />
    <notifications />

    <div class="stk-page flex-grow-1">
      <div class="stk-container">

        <!-- Tab Navigation -->
        <div class="stk-tabs">
          <button
            class="stk-tab"
            :class="{ 'stk-tab--active': activeTab === 'vn' }"
            @click="activeTab = 'vn'"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3v18h18"/><path d="m19 9-5 5-4-4-3 3"/></svg>
            VN Stock
          </button>
          <button
            class="stk-tab"
            :class="{ 'stk-tab--active': activeTab === 'category' }"
            @click="activeTab = 'category'"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>
            Category
          </button>
          <button
            class="stk-tab"
            :class="{ 'stk-tab--active': activeTab === 'vn_rrg' }"
            @click="activeTab = 'vn_rrg'"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10"/><path d="M2 12h20"/></svg>
            RRG Chart
          </button>
          <button
            class="stk-tab"
            :class="{ 'stk-tab--active': activeTab === 'global' }"
            @click="activeTab = 'global'"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M2 12h20"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10"/><path d="M12 2a15 15 0 0 0-4 10 15 15 0 0 0 4 10"/></svg>
            Global Stock
          </button>
        </div>

        <!-- ==================== VN TAB ==================== -->
        <div v-show="activeTab === 'vn'">
          <div class="stk-panel">
            <!-- Header -->
            <div class="stk-header">
              <div class="stk-header__icon">
                <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3v18h18"/><path d="m19 9-5 5-4-4-3 3"/></svg>
              </div>
              <div>
                <h2 class="stk-header__title">Vietnam Stock Evaluator</h2>
                <p class="stk-header__sub">Search, evaluate &amp; discover potential VN stocks</p>
              </div>
            </div>

            <!-- Stock Selector -->
            <div class="stk-section">
              <label class="stk-label">Choose a stock symbol</label>
              <v-select
                v-model="selectedStock"
                :options="stocks"
                label="code"
                @input="onStockSelected"
                :filter-options="filterOptions"
                class="stk-select"
              ></v-select>
            </div>
          </div>

          <!-- Chart (sticky, outside panel so it works) -->
          <div ref="vnChartRef" v-if="selectedStock !== null && selectedStock.code !== ''" class="stk-sticky-chart">
            <div class="stk-chart-wrap">
              <iframe
                :src="`https://stockchart.vietstock.vn/?stockcode=${selectedStock.code}`"
                width="100%"
                height="380"
                frameborder="0"
              ></iframe>
              <div v-if="isLoading" class="stk-loading">
                <div class="stk-spinner"></div>
              </div>
            </div>

            <!-- Action Buttons Row -->
            <div v-if="selectedStock && selectedStock.code" class="stk-action-row">
              <!-- Price Alert Toggle (Left) -->
              <div class="stk-alert-toggle">
                <button class="stk-alert-toggle__btn" @click="showPriceAlert = !showPriceAlert">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>
                  Price Alert
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" :class="{ 'stk-chevron--open': showPriceAlert }"><polyline points="6 9 12 15 18 9"/></svg>
                </button>
                <div v-show="showPriceAlert" class="stk-alert-content">
                  <PriceAlertWidget
                    :symbol="selectedStock.code"
                    assetType="stock"
                  />
                </div>
              </div>
              <!-- View Chart Button (Right) -->
              <button class="stk-view-chart-btn" @click="viewChart">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M3 9h18"/><path d="M9 21V9"/></svg>
                View Chart
              </button>
            </div>
          </div>

          <!-- Potential Symbols Section -->
          <div class="stk-panel">
            <div class="stk-section stk-section--potential">
            <div class="stk-section-head">
              <h3 class="stk-section-head__title">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                Potential Symbols
              </h3>
              <span v-if="potentialStocks.latest_updated" class="stk-updated">
                Updated: {{ formatDate(potentialStocks.latest_updated) }}
              </span>
            </div>

            <!-- Filters -->
            <div class="stk-filters" v-if="potentialStocks.data && potentialStocks.data.length > 0">
              <div class="stk-filter-item">
                <input
                  type="text"
                  v-model="filterTextVN"
                  placeholder="Filter symbols..."
                  class="stk-input"
                />
              </div>
              <div class="stk-filter-item">
                <select v-model="selectedSignalType" class="stk-input">
                  <option value="">All Signals</option>
                  <option value="near_52w_ath">Highest 52W</option>
                  <option value="ema9_above_ema21">Uptrend</option>
                  <option value="top_growth_20d">Top Growth 20D</option>
                </select>
              </div>
            </div>

            <div class="stk-potential-summary" v-if="potentialStocks.data && totalMarketStocks > 0">
              <div class="stk-potential-summary__left">
                <span class="stk-potential-summary__label">Số cổ phiếu mạnh:</span>
                <span class="stk-potential-summary__value">{{ ema9AboveItemCount }} mã / {{ totalMarketStocks }} tổng ({{ ema9AbovePercentage  }})</span>
              </div>
              <div class="stk-potential-summary__right" :class="ema9AboveItemCount / totalMarketStocks >= 0.5 ? 'stk-potential-summary--bullish' : 'stk-potential-summary--bearish'">
                <span class="stk-potential-summary__sentiment">{{ ema9AboveItemCount / totalMarketStocks >= 0.5 ? 'Bullish' : 'Bearish' }}</span>
              </div>
            </div>
            <!-- Potential Stocks Table -->
              <div ref="vnTableWrapRef" class="stk-table-wrap stk-table-wrap--scroll" v-if="filteredPotentialStocks.length > 0">
              <table class="stk-table">
                <thead>
                  <tr>
                    <th class="stk-th">Symbol</th>
                    <th class="stk-th stk-th--right">Score Diff</th>
                    <th class="stk-th stk-th--right">Volume</th>
                    <th class="stk-th stk-th--center">Signal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="stock in filteredPotentialStocks"
                    :key="stock.symbol"
                    class="stk-row"
                    :class="{ 'stk-row--active': isVnRowActive(stock) }"
                    @click="selectVnStock(stock)"
                  >
                    <td class="stk-td stk-td--symbol" :title="`View ${stock.symbol} details`">{{ stock.symbol }}</td>
                    <td class="stk-td stk-td--right" :style="{ color: stock.score_diff >= 0 ? '#10b981' : '#ef4444', fontWeight: 'bold' }">
                      {{ stock.score_diff >= 0 ? '+' : '' }}{{ stock.score_diff.toFixed(2) }}%
                    </td>
                    <td class="stk-td stk-td--right stk-td--mono">{{ formatVolume(stock.volume) }}</td>
                    <td class="stk-td stk-td--center">
                      <template v-for="(label, index) in stock.signal_labels" :key="`${stock.symbol}-${stock.signal_types[index]}`">
                        <span class="stk-signal" :class="'stk-signal--' + stock.signal_types[index]">{{ label }}</span>
                      </template>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Actions -->
            <div class="stk-actions">
              <div v-if="potentialStocks.data && potentialStocks.data.length > 0" class="stk-actions__group">
                <button @click="exportCSV" class="stk-btn stk-btn--outline">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
                  Export CSV
                </button>
              </div>

              <div style="display: flex; gap: 10px; align-items: center; justify-content: center;">
                <button
                  v-if="!loadingPotentialStocks && !startScanning"
                  @click="startScanningStocks"
                  class="stk-btn stk-btn--primary stk-btn--scan"
                >
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                  Start Scanning
                </button>
                <button
                  class="stk-btn stk-btn--outline"
                  @click="runSSHScript('vnstock_potential')"
                  :disabled="isRunningPotentialScript"
                  style="border-color: #3b82f6; color: #3b82f6;"
                >
                  <span v-if="isRunningPotentialScript" class="stk-spinner" style="width: 14px; height: 14px; margin: 0; display: inline-block; border-width: 2px;"></span>
                  <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                  Run script
                </button>
                <div v-if="loadingPotentialStocks" class="stk-loading" style="padding: 0;">
                  <div class="stk-spinner" style="width: 24px; height: 24px;"></div>
                </div>
              </div>
            </div>
            <p v-if="message" class="stk-message">{{ message }}</p>
          </div>
          </div>
        </div>

        <!-- ==================== CATEGORY TAB ==================== -->
        <div v-show="activeTab === 'category'">
          <div class="stk-panel">
            <div class="stk-header">
              <div class="stk-header__icon">
                <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>
              </div>
              <div>
                <h2 class="stk-header__title">Sector Category</h2>
                <p class="stk-header__sub">Phân tích tương quan các nhóm ngành qua chart Vietstock</p>
              </div>
            </div>

            <!-- Filter -->
            <div class="stk-section">
              <input
                type="text"
                v-model="filterTextCategory"
                placeholder="Lọc nhóm ngành..."
                class="stk-input"
              />
            </div>
          </div>

          <!-- Category Chart (sticky) -->
          <div ref="categoryChartRef" v-if="selectedCategory" class="stk-sticky-chart">
            <div class="stk-chart-wrap">
              <iframe
                :src="`https://stockchart.vietstock.vn/?stockcode=${selectedCategory.code}`"
                width="100%"
                height="380"
                frameborder="0"
              ></iframe>
            </div>
            <div class="stk-category-badge">
              <span class="stk-category-badge__code">{{ selectedCategory.code }}</span>
              <span class="stk-category-badge__name">{{ selectedCategory.name }}</span>
            </div>
          </div>

          <!-- Category List -->
          <div class="stk-panel">
            <div ref="categoryTableWrapRef" class="stk-table-wrap stk-table-wrap--scroll">
              <table class="stk-table">
                <thead>
                  <tr>
                    <th class="stk-th">Mã</th>
                    <th class="stk-th">Mô tả</th>
                    <th class="stk-th stk-th--center">Sàn</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="cat in filteredCategories"
                    :key="cat.code"
                    class="stk-row"
                    :class="{ 'stk-row--active': selectedCategory && selectedCategory.code === cat.code }"
                    @click="selectCategory(cat)"
                  >
                    <td class="stk-td stk-td--symbol">{{ cat.code }}</td>
                    <td class="stk-td">{{ cat.name }}</td>
                    <td class="stk-td stk-td--center">
                      <span class="stk-category-exchange">HOSE &amp; HNX</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- ==================== GLOBAL TAB ==================== -->
        <div class="stk-panel" v-show="activeTab === 'global'">
          <div class="stk-header">
            <div class="stk-header__icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M2 12h20"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10"/><path d="M12 2a15 15 0 0 0-4 10 15 15 0 0 0 4 10"/></svg>
            </div>
            <div>
              <h2 class="stk-header__title">Global Stock Scanner</h2>
              <p class="stk-header__sub">Discover potential stocks across world markets</p>
            </div>
          </div>

          <!-- Empty State (before scanning) -->
          <div v-if="!startScanningGlobal && globalStocks.length === 0" class="stk-empty-state" style="padding: 60px 24px; text-align: center; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 16px;">
            <div class="stk-empty-icon" style="width: 80px; height: 80px; border-radius: 50%; background: #eff6ff; display: flex; align-items: center; justify-content: center; color: #3b82f6; margin-bottom: 8px;">
              <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10"/>
                <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/>
                <path d="M2 12h20"/>
              </svg>
            </div>
            <h3 style="font-size: 1.25rem; font-weight: 700; color: #1e293b; margin: 0;">Explore Global Markets</h3>
            <p style="font-size: 0.88rem; color: #64748b; max-width: 360px; line-height: 1.5; margin: 0;">
              Scan international stock exchanges to discover symbols currently showing strong signals and breakout setups.
            </p>
            <div style="display: flex; gap: 10px; align-items: center; justify-content: center; margin-top: 8px;">
              <button
                @click="startScanningWorld"
                class="stk-btn stk-btn--primary"
                style="padding: 11px 28px; font-size: 0.9rem; border-radius: 10px; display: inline-flex; align-items: center; gap: 6px; justify-content: center;"
              >
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                Start Scanning
              </button>
              <button
                class="stk-btn stk-btn--outline"
                @click="runSSHScript('world_potential')"
                :disabled="isRunningPotentialScript"
                style="padding: 11px 28px; font-size: 0.9rem; border-radius: 10px; display: inline-flex; align-items: center; gap: 6px; justify-content: center; border-color: #3b82f6; color: #3b82f6;"
              >
                <span v-if="isRunningPotentialScript" class="stk-spinner" style="width: 14px; height: 14px; margin: 0; display: inline-block; border-width: 2px;"></span>
                <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                Run script
              </button>
            </div>
          </div>

          <!-- Content (when scanning or data loaded) -->
          <div v-else class="stk-section">
            <div class="stk-actions" style="margin-bottom: 20px;">
              <div style="display: flex; gap: 10px; align-items: center; justify-content: center; width: 100%;">
                <button
                  v-if="!loadingGlobalStocks && !startScanningGlobal"
                  @click="startScanningWorld"
                  class="stk-btn stk-btn--primary stk-btn--scan"
                >
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                  Start Scanning
                </button>
                <button
                  class="stk-btn stk-btn--outline"
                  @click="runSSHScript('world_potential')"
                  :disabled="isRunningPotentialScript"
                  style="border-color: #3b82f6; color: #3b82f6;"
                >
                  <span v-if="isRunningPotentialScript" class="stk-spinner" style="width: 14px; height: 14px; margin: 0; display: inline-block; border-width: 2px;"></span>
                  <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                  Run script
                </button>
                <div v-if="loadingGlobalStocks" class="stk-loading" style="padding: 0;">
                  <div class="stk-spinner" style="width: 24px; height: 24px;"></div>
                </div>
              </div>
            </div>

            <!-- TradingView Chart -->
            <div v-if="selectedGlobalSymbol" class="stk-chart-wrap" style="margin-bottom: 16px;">
              <TradingViewChart :coin="selectedGlobalSymbol" />
            </div>

            <!-- Price Alert -->
            <PriceAlertWidget
              v-if="selectedGlobalSymbol"
              :symbol="selectedGlobalSymbol"
              assetType="stock"
              style="margin-bottom: 20px;"
            />

            <!-- Filters -->
            <div class="stk-filters" style="margin-bottom: 16px;">
              <div class="stk-filter-item" v-if="globalStocks.length > 0">
                <input type="text" v-model="filterTextGlobal" placeholder="Filter symbols..." class="stk-input" />
              </div>
              <div class="stk-filter-item" v-if="countriesList.length > 0">
                <select v-model="selectedCountry" class="stk-input">
                  <option value="">All Countries</option>
                  <option v-for="c in countriesList" :key="c" :value="c">{{ c }}</option>
                </select>
              </div>
            </div>

            <span v-if="globalLatestUpdated" class="stk-updated" style="display:block; text-align:right; margin-bottom:12px;">
              Updated: {{ formatDate(globalLatestUpdated) }}
            </span>

            <!-- Global Table -->
            <div ref="globalTableWrapRef" class="stk-table-wrap stk-table-wrap--scroll" v-if="filteredGlobalStocks.length > 0">
              <table class="stk-table">
                <thead>
                  <tr>
                    <th class="stk-th">Country</th>
                    <th class="stk-th">Symbol</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="item in filteredGlobalStocks"
                    :key="item.symbol"
                    class="stk-row"
                    :class="{ 'stk-row--active': selectedGlobalSymbol === item.symbol }"
                    @click="onSelectGlobal(item)"
                  >
                    <td class="stk-td">{{ item.country }}</td>
                    <td class="stk-td stk-td--symbol">{{ item.symbol }}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div class="stk-loading" v-if="loadingGlobalStocks" style="margin-top: 20px;">
              <div class="stk-spinner"></div>
            </div>
            <p v-if="messageGlobal" class="stk-message" style="margin-top: 10px;">{{ messageGlobal }}</p>
          </div>
        </div>

        <!-- ==================== RRG TAB ==================== -->
        <div class="stk-panel" v-show="activeTab === 'vn_rrg'">
          <div class="stk-header">
            <div class="stk-header__icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10"/><path d="M2 12h20"/></svg>
            </div>
            <div>
              <h2 class="stk-header__title">VN Stock RRG Chart</h2>
              <p class="stk-header__sub">Relative Rotation Graph for Vietnam market</p>
            </div>
          </div>
          <div class="stk-rrg-wrap">
            <div class="stk-rrg-actions" style="margin-bottom: 20px; display: flex; justify-content: center;">
              <button
                class="stk-btn stk-btn--primary"
                @click="runSSHScript('vnstock_rrg')"
                :disabled="isRunningRrgScript"
                style="min-width: 180px; justify-content: center;"
              >
                <span v-if="isRunningRrgScript" class="stk-spinner" style="width: 16px; height: 16px; border-top-color: #fff; margin: 0; display: inline-block; border-width: 2px;"></span>
                <span v-else style="display: inline-flex; align-items: center; gap: 6px;">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21.5 2v6h-6M21.34 15.57a10 10 0 1 1-.57-8.38l5.67-5.67"/></svg>
                  Generate RRG Chart
                </span>
              </button>
            </div>
            <img :src="vnstockRRGUrl" class="stk-rrg-img" alt="VN Stock RRG Chart" />
          </div>
        </div>

      </div>
    </div>
    <AppFooter />
  </div>
</template>

<script>
import NavBar from './NavBar.vue';
import AppFooter from './AppFooter.vue';
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue';
import vSelect from 'vue3-select';
import { useNotification } from "@kyvg/vue3-notification";
import TradingViewChart from './TradingViewChart.vue';
import PriceAlertWidget from './PriceAlertWidget.vue';

export default {
  name: 'StockMarket',
  components: {
    NavBar,
    AppFooter,
    vSelect,
    TradingViewChart,
    PriceAlertWidget,
  },
  props: {
    searchText: String,
  },
  emits: ['update:searchText', 'update:selectedStock'],
  setup(props, { emit }) {
    const { notify } = useNotification();
    // Tabs
    const activeTab = ref('vn');
    const vnChartRef = ref(null);
    const vnTableWrapRef = ref(null);
    const globalTableWrapRef = ref(null);
    const categoryChartRef = ref(null);
    const categoryTableWrapRef = ref(null);
    const showPriceAlert = ref(false);

    // Category data
    const categoryList = ref([
      { code: 'VS-PLASTICS', name: 'SX NHỰA - HÓA CHẤT' },
      { code: 'VS-FOOD&DRINK', name: 'THỰC PHẨM - ĐỒ UỐNG' },
      { code: 'VS-SEAFOOD', name: 'CHẾ BIẾN THỦY SẢN' },
      { code: 'VS-CONMATERIAL', name: 'VẬT LIỆU XÂY DỰNG' },
      { code: 'VS-UTILITIES', name: 'TIỆN ÍCH' },
      { code: 'VS-LOGISTICS', name: 'VẬN TẢI - KHO BÃI' },
      { code: 'VS-CONSTRUCT', name: 'XÂY DỰNG' },
      { code: 'VS-ACCOMMODATE', name: 'DỊCH VỤ LƯU TRÚ, ĂN UỐNG, GIẢI TRÍ' },
      { code: 'VS-ANCILLARY PRODUTION', name: 'SX PHỤ TRỢ' },
      { code: 'VS-E EQUIPMENT', name: 'THIẾT BỊ ĐIỆN' },
      { code: 'VS-PST SERVICE', name: 'DỊCH VỤ TƯ VẤN, HỖ TRỢ' },
      { code: 'VS-OTHER FINANCIAL', name: 'TÀI CHÍNH KHÁC' },
      { code: 'VS-INSURANCE', name: 'BẢO HIỂM' },
      { code: 'VS-REAL ESTATE', name: 'BẤT ĐỘNG SẢN' },
      { code: 'VS-SECURITIES', name: 'CHỨNG KHOÁN' },
      { code: 'VS-ICT', name: 'CÔNG NGHỆ VÀ THÔNG TIN' },
      { code: 'VS-RETAIL', name: 'BÁN LẺ' },
      { code: 'VS-HEALTHCARE', name: 'CHĂM SÓC SỨC KHỎE' },
      { code: 'VS-MINING&OIL', name: 'KHAI KHOÁNG' },
      { code: 'VS-BANKING', name: 'NGÂN HÀNG' },
      { code: 'VS-AGRI', name: 'NÔNG - LÂM - NGƯ' },
      { code: 'VS-MACHINERY', name: 'SX THIẾT BỊ, MÁY MÓC' },
      { code: 'VS-HOUSEHOLD', name: 'SX HÀNG GIA DỤNG' },
      { code: 'VS-RUBBER PROD', name: 'SẢN PHẨM CAO SU' },
      { code: 'VS-WHOLESALES', name: 'BÁN BUÔN' },
    ]);
    const selectedCategory = ref(null);
    const filterTextCategory = ref('');

    const isMenuOpen = ref(false);
    const toggleMenu = () => {
      isMenuOpen.value = !isMenuOpen.value;
    };
    const userInfo = ref(null);
    const selectedStock = ref({ code: 'VNINDEX' });
    const selectedVnRowKey = ref('');
    const stocks = ref([]);
    const companyName = ref(null);
    const currentPrice = ref(null);
    const fiPrice = ref(null); // Fundamental Index price
    const dcfPrice = ref(null); // DCF price
    const averagePrice = ref(null); // Average price
    const potentialStocks = ref({}); // VN potential symbols
    const loadingPotentialStocks = ref(false);
    const startScanning = ref(false);
    const message = ref(''); // VN message
    const isLoading = ref(false);
    const filterTextVN = ref('');
    const selectedSignalType = ref('');

    // Global potential symbols
    const globalStocks = ref([]); // [{ symbol, country }]
    const globalLatestUpdated = ref(null);
    const loadingGlobalStocks = ref(false);
    const startScanningGlobal = ref(false);
    const messageGlobal = ref('');
    const filterTextGlobal = ref('');
  const selectedGlobalSymbol = ref('SP:SPX');
    const selectedCountry = ref('');
    const countriesList = computed(() => {
      const set = new Set((globalStocks.value || []).map(i => i.country).filter(Boolean));
      return Array.from(set).sort();
    });

    const groupedPotentialStocks = computed(() => {
      const grouped = new Map();
      const items = potentialStocks.value.data || [];

      for (const stock of items) {
        if (!stock || !stock.symbol) {
          continue;
        }

        const existing = grouped.get(stock.symbol);
        const label = getSignalLabel(stock);
        const signalType = stock.signal_type || '';

        if (!existing) {
          grouped.set(stock.symbol, {
            symbol: stock.symbol,
            volume: stock.volume || 0,
            highest_price: stock.highest_price,
            lowest_price: stock.lowest_price,
            signal_types: signalType ? [signalType] : [],
            signal_labels: label ? [label] : [],
            score_diff: stock.score_diff || 0,
          });
        } else {
          existing.volume = Math.max(existing.volume, stock.volume || 0);
          if (typeof stock.highest_price === 'number' && stock.highest_price > existing.highest_price) {
            existing.highest_price = stock.highest_price;
          }
          if (typeof stock.lowest_price === 'number' && stock.lowest_price < existing.lowest_price) {
            existing.lowest_price = stock.lowest_price;
          }
          if (signalType && !existing.signal_types.includes(signalType)) {
            existing.signal_types.push(signalType);
          }
          if (label && !existing.signal_labels.includes(label)) {
            existing.signal_labels.push(label);
          }
          if (stock.score_diff !== undefined) {
            existing.score_diff = stock.score_diff;
          }
        }
      }

      return Array.from(grouped.values());
    });

    const totalMarketStocks = computed(() => {
      return (stocks.value || []).filter(stock => stock?.code?.length === 3).length;
    });

    const ema9AboveItemCount = computed(() => {
      const items = potentialStocks.value.data || [];
      const symbols = new Set();
      for (const item of items) {
        if (item?.signal_type === 'ema9_above_ema21' && item.symbol) {
          symbols.add(item.symbol);
        }
      }
      return symbols.size;
    });

    const ema9AbovePercentage = computed(() => {
      if (!totalMarketStocks.value) {
        return '0.00%';
      }
      return `${((ema9AboveItemCount.value / totalMarketStocks.value) * 100).toFixed(2)}%`;
    });

    const filteredPotentialStocks = computed(() => {
      const filtered = (groupedPotentialStocks.value || []).filter(stock => {
        const matchesText = !filterTextVN.value || stock.symbol.toLowerCase().includes(filterTextVN.value.toLowerCase());
        const matchesSignal = !selectedSignalType.value || stock.signal_types.includes(selectedSignalType.value);
        return matchesText && matchesSignal;
      });

      return filtered.sort((a, b) => {
        const signalLenA = a.signal_types ? a.signal_types.length : 0;
        const signalLenB = b.signal_types ? b.signal_types.length : 0;
        if (signalLenA !== signalLenB) {
          return signalLenB - signalLenA;
        }
        return Number(b.volume || 0) - Number(a.volume || 0);
      });
    });

    const filteredCategories = computed(() => {
      const q = (filterTextCategory.value || '').toLowerCase();
      if (!q) return categoryList.value;
      return categoryList.value.filter(cat =>
        cat.code.toLowerCase().includes(q) || cat.name.toLowerCase().includes(q)
      );
    });

    const filteredGlobalStocks = computed(() => {
      const q = (filterTextGlobal.value || '').toLowerCase();
      const country = selectedCountry.value;
      return globalStocks.value.filter(it => {
        const matchText = !q || (it.symbol || '').toLowerCase().includes(q);
        const matchCountry = !country || it.country === country;
        return matchText && matchCountry;
      });
    });

    onMounted(async () => {
      window.addEventListener('keydown', handleArrowNavigation);
      const response = await fetch('https://api-finfo.vndirect.com.vn/v4/stocks?q=type:STOCK~status:LISTED&fields=code&size=3000');
      const data = await response.json();
      stocks.value = data.data;
      emit('update:stocks', stocks.value);
      fetchStocks();
    });

    onUnmounted(() => {
      window.removeEventListener('keydown', handleArrowNavigation);
    });

    const updateSelectedStock = (newStock) => {
      selectedStock.value = newStock ? newStock : null;
    }

    const updateStocks = (newStocks) => {
      stocks.value = newStocks;
    }

     const startScanningStocks = () => {
       startScanning.value = true;
       fetchPotentialStocks();
     }

     // SSH script execution states
     const isRunningPotentialScript = ref(false);
     const isRunningRrgScript = ref(false);
     const vnstockRRGKey = ref(Date.now());
     const vnstockRRGUrl = computed(() => `/vnstock_rrgchart?t=${vnstockRRGKey.value}`);

      const runSSHScript = async (scriptType) => {
        const isRrg = scriptType === 'vnstock_rrg';
        if (isRrg) {
          isRunningRrgScript.value = true;
        } else {
          isRunningPotentialScript.value = true;
        }

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
            if (isRrg) {
              notify({ type: 'success', title: 'Success', text: 'VN Stock RRG Chart has been updated successfully!' });
              vnstockRRGKey.value = Date.now();
            } else if (scriptType === 'world_potential') {
              notify({ type: 'success', title: 'Success', text: 'Global Stock scanner script executed successfully!' });
              fetchPotentialWorldSymbols();
            } else {
              notify({ type: 'success', title: 'Success', text: 'VN Stock scanner script executed successfully!' });
              fetchPotentialStocks();
            }
          } else {
            throw new Error(data.error || 'Server returned an error');
          }
        } catch (error) {
          console.error('Error running SSH script:', error);
          notify({ type: 'error', title: 'Execution Failed', text: error.message || 'Failed to connect or run the SSH script.' });
        } finally {
          if (isRrg) {
            isRunningRrgScript.value = false;
          } else {
            isRunningPotentialScript.value = false;
          }
        }
      };

    const startScanningWorld = () => {
      startScanningGlobal.value = true;
      messageGlobal.value = '';
      fetchPotentialWorldSymbols();
    }

    const getVnRowKey = (stock) => stock?.symbol || '';

    const isVnRowActive = (stock) => {
      if (!stock) {
        return false;
      }
      if (selectedVnRowKey.value) {
        return selectedVnRowKey.value === getVnRowKey(stock);
      }
      return selectedStock.value && selectedStock.value.code === stock.symbol;
    };

    const selectVnStock = (stockOrSymbol, shouldScroll = true) => {
      const symbol = typeof stockOrSymbol === 'string'
        ? stockOrSymbol
        : stockOrSymbol?.symbol;

      if (!symbol) {
        return;
      }

      selectedStock.value = { code: symbol };

      if (typeof stockOrSymbol === 'object' && stockOrSymbol?.symbol) {
        selectedVnRowKey.value = getVnRowKey(stockOrSymbol);
      } else {
        const firstMatch = (filteredPotentialStocks.value || []).find((row) => row.symbol === symbol);
        selectedVnRowKey.value = firstMatch ? getVnRowKey(firstMatch) : '';
      }

      if (!shouldScroll) {
        return;
      }

      setTimeout(() => {
        if (vnChartRef.value) {
          const el = vnChartRef.value;
          const y = el.getBoundingClientRect().top + window.scrollY - 120;
          window.scrollTo({ top: y, behavior: 'smooth' });
        }
      }, 100);
    };

    const selectCategory = (cat, shouldScroll = true) => {
      selectedCategory.value = cat;
      if (shouldScroll) {
        setTimeout(() => {
          if (categoryChartRef.value) {
            const el = categoryChartRef.value;
            const y = el.getBoundingClientRect().top + window.scrollY - 120;
            window.scrollTo({ top: y, behavior: 'smooth' });
          }
        }, 100);
      }
    };

    const onSelectGlobal = (item) => {
      selectedGlobalSymbol.value = item.symbol;
    }

    const viewChart = () => {
      if (selectedStock.value && selectedStock.value.code) {
        window.open(`https://stockchart.vietstock.vn/?stockcode=${selectedStock.value.code}`, '_blank');
      }
    }

    const isTypingTarget = (target) => {
      if (!target) {
        return false;
      }
      const tagName = (target.tagName || '').toLowerCase();
      return tagName === 'input' || tagName === 'textarea' || tagName === 'select' || target.isContentEditable;
    };

    const scrollActiveRowIntoView = async (containerRef) => {
      await nextTick();
      const container = containerRef.value;
      if (!container) {
        return;
      }
      const activeRow = container.querySelector('.stk-row--active');
      if (activeRow) {
        activeRow.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
      }
    };

    const moveVnSelection = (direction) => {
      const rows = filteredPotentialStocks.value || [];
      if (!rows.length) {
        return;
      }

      let currentIndex = rows.findIndex((row) => getVnRowKey(row) === selectedVnRowKey.value);
      if (currentIndex === -1 && selectedStock.value?.code) {
        currentIndex = rows.findIndex((row) => row.symbol === selectedStock.value.code);
      }
      const baseIndex = currentIndex === -1
        ? (direction > 0 ? -1 : 0)
        : currentIndex;
      const nextIndex = (baseIndex + direction + rows.length) % rows.length;
      const nextRow = rows[nextIndex];

      if (nextRow) {
        selectVnStock(nextRow, false);
        scrollActiveRowIntoView(vnTableWrapRef);
      }
    };

    const moveCategorySelection = (direction) => {
      const rows = filteredCategories.value || [];
      if (!rows.length) return;

      const currentIndex = selectedCategory.value
        ? rows.findIndex((row) => row.code === selectedCategory.value.code)
        : -1;
      const baseIndex = currentIndex === -1
        ? (direction > 0 ? -1 : 0)
        : currentIndex;
      const nextIndex = (baseIndex + direction + rows.length) % rows.length;
      const nextCat = rows[nextIndex];

      if (nextCat) {
        selectCategory(nextCat, false);
        scrollActiveRowIntoView(categoryTableWrapRef);
      }
    };

    const moveGlobalSelection = (direction) => {
      const rows = filteredGlobalStocks.value || [];
      if (!rows.length) {
        return;
      }

      const currentIndex = rows.findIndex((row) => row.symbol === selectedGlobalSymbol.value);
      const baseIndex = currentIndex === -1
        ? (direction > 0 ? -1 : 0)
        : currentIndex;
      const nextIndex = (baseIndex + direction + rows.length) % rows.length;
      const nextItem = rows[nextIndex];

      if (nextItem) {
        onSelectGlobal(nextItem);
        scrollActiveRowIntoView(globalTableWrapRef);
      }
    };

    const handleArrowNavigation = (event) => {
      if (event.key !== 'ArrowDown' && event.key !== 'ArrowUp') {
        return;
      }
      if (isTypingTarget(event.target)) {
        return;
      }

      const direction = event.key === 'ArrowDown' ? 1 : -1;

      if (activeTab.value === 'vn') {
        moveVnSelection(direction);
        event.preventDefault();
      } else if (activeTab.value === 'category') {
        moveCategorySelection(direction);
        event.preventDefault();
      } else if (activeTab.value === 'global') {
        moveGlobalSelection(direction);
        event.preventDefault();
      }
    };

    watch(selectedStock, (newStock) => {
      if (newStock) {
        fetchCompanyInfo(newStock.code);
        evaluatePrice(newStock.code);
      } else {
        // Clear previous stock data when no stock is selected
        companyName.value = null;
        currentPrice.value = null;
        fiPrice.value = null;
        dcfPrice.value = null;
        averagePrice.value = null;
      }
    });

    const onStockSelected = (value) => {
      selectedVnRowKey.value = '';
      emit('update:selectedStock', value);
    };


    const filterOptions = (options, search) => {
      if (!search) {
        return options
      }
      return options.filter((option) =>
        option.code.toLowerCase().includes(search.toLowerCase())
      )
    }

    const fetchCompanyInfo = async (stockCode) => {
      isLoading.value = true;
      try {
        const response = await fetch(`https://services.entrade.com.vn/dnse-financial-product/securities/${stockCode}`);
        const data = await response.json();
        companyName.value = data.issuer || 'N/A';
        currentPrice.value = data.basicPrice || null;
      } catch (error) {
        console.error('Error fetching company info:', error);
        companyName.value = 'Error fetching data';
      } finally {
        isLoading.value = false;
      }
    };

    const evaluatePrice = async (ticket) => {
      isLoading.value = true;
      try {
        const res = await fetch(`/tcanalysis/v1/evaluation/${ticket}/evaluation`);
        if (res.status === 200) {
          const json_body = await res.json();

          // Fundamental Index method
          const pe = json_body.industry?.pe;
          const eps = json_body.eps;
          const pb = json_body.industry?.pb;
          const bvps = json_body.bvps;
          const evebitda = json_body.industry?.evebitda;
          const ebitda = json_body.ebitda;

          fiPrice.value = (pe && eps && pb && bvps && evebitda && ebitda) ? Math.round(((pe * eps) + (pb * bvps) + (evebitda * ebitda)) / 3) : null;

          // DCF method
          const enterpriceValue = json_body.enterpriseValue;
          const cash = json_body.cash;
          const shortTermDebt = json_body.shortTermDebt;
          const longTermDebt = json_body.longTermDebt;
          const minorityInterest = json_body.minorityInterest;
          const cap_value = enterpriceValue + cash + shortTermDebt + longTermDebt + minorityInterest;
          const shareOutstanding = json_body.shareOutstanding;

          dcfPrice.value = (cap_value && shareOutstanding) ? Math.round(cap_value / shareOutstanding) : null;

          // Average both Fundamental Index and DCF method
          averagePrice.value = (fiPrice.value != null && dcfPrice.value != null) ? Math.round((fiPrice.value + dcfPrice.value) / 2) : null;
        }
      }
      catch (error) {
        console.error('Error fetching evaluation data:', error);
      } finally {
        isLoading.value = false;
      }
    }

    const fetchPotentialStocks = async () => {
      loadingPotentialStocks.value = true;
      isLoading.value = true;
      message.value = '';
      try {
        const response = await fetch('/getPotentialSymbols');
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        potentialStocks.value = data; // Assign directly
        
        // Show message if no data
        if (!data.data || data.data.length === 0) {
          message.value = 'No potential stocks available at the moment.';
        } else {
          message.value = `Found ${data.data.length} potential stocks.`;
        }
      } catch (error) {
        console.error('Error fetching potential stocks:', error);
        potentialStocks.value = { data: [] }; // Clear the list on error
        message.value = 'Failed to load potential stocks. Please try again later.';
      } finally {
        loadingPotentialStocks.value = false;
        isLoading.value = false;
      }
    };

    // Global: fetch via proxy /world
    const fetchPotentialWorldSymbols = async () => {
      loadingGlobalStocks.value = true;
      messageGlobal.value = '';
      try {
        const res = await fetch('/world/getPotentialWorldSymbols');
        if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
        const json = await res.json();
        const items = Array.isArray(json) ? json : (json.data || []);
        globalStocks.value = items.map(it => ({ symbol: it.symbol, country: it.country }));
        globalLatestUpdated.value = json.latest_updated || null;
        
        if (globalStocks.value.length === 0) {
          messageGlobal.value = 'No global symbols available at the moment.';
        } else {
          messageGlobal.value = `Found ${globalStocks.value.length} global symbols.`;
        }
      } catch (e) {
        console.error('Error fetching global symbols:', e);
        globalStocks.value = [];
        messageGlobal.value = 'Failed to load global symbols. Please try again later.';
      } finally {
        loadingGlobalStocks.value = false;
      }
    };

    const formatDate = (dateString) => {
      const date = new Date(dateString);
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');
      const seconds = String(date.getSeconds()).padStart(2, '0');

      return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    }

    const getSignalLabel = (stock) => {
      const signalType = stock?.signal_type;
      const labelMap = {
        near_52w_ath: 'Highest 52W',
        ema9_above_ema21: 'Uptrend',
        top_growth_20d: 'Top Growth 20D',
      };
      return stock?.signal_label || labelMap[signalType] || signalType || 'N/A';
    };

    const exportCSV = () => {
      if (!filteredPotentialStocks.value.length) {
        return;
      }

      const rows = filteredPotentialStocks.value.map(stock => `${stock.symbol},"${stock.signal_labels.join(' | ')}",${stock.volume ?? 0},${stock.highest_price},${stock.lowest_price}`);
      const csvContent = "data:text/csv;charset=utf-8," + "symbol,signals,volume,highest_price,lowest_price\n" + rows.join("\n");
      const encodedUri = encodeURI(csvContent);
      const link = document.createElement("a");
      link.setAttribute("href", encodedUri);
      link.setAttribute("download", "potential_stocks.csv");
      document.body.appendChild(link); // Required for Firefox

      link.click(); // This will download the data file named "potential_stocks.csv".

      document.body.removeChild(link);
    };

    const fetchStocks = async () => {
      const response = await fetch('https://api-finfo.vndirect.com.vn/v4/stocks?q=type:STOCK~status:LISTED&fields=code&size=3000');
      const data = await response.json();
      stocks.value = data.data;
    };

    return {
      activeTab,
      vnChartRef,
      vnTableWrapRef,
      globalTableWrapRef,
      categoryChartRef,
      categoryTableWrapRef,
      showPriceAlert,
      selectVnStock,
      isVnRowActive,
      selectedStock,
      stocks,
      onStockSelected,
      filterOptions,
      companyName,
      currentPrice,
      fiPrice,
      dcfPrice,
      averagePrice,
      formatNumber,
      formatVolume,
      potentialStocks,
      updateSelectedStock,
      updateStocks,
      loadingPotentialStocks,
      exportCSV,
      startScanningStocks,
      startScanningWorld,
      onSelectGlobal,
      formatDate,
      getSignalLabel,
      isLoading,
      toggleMenu,
      isMenuOpen,
      userInfo,
      // VN tab state
      filterTextVN,
      selectedSignalType,
      filteredPotentialStocks,
      ema9AboveItemCount,
      totalMarketStocks,
      ema9AbovePercentage,
      message,
      // Category tab state
      categoryList,
      selectedCategory,
      filterTextCategory,
      filteredCategories,
      selectCategory,
      // Global tab state
      globalStocks,
      globalLatestUpdated,
      loadingGlobalStocks,
      startScanningGlobal,
      messageGlobal,
      filterTextGlobal,
      filteredGlobalStocks,
      selectedGlobalSymbol,
      selectedCountry,
      countriesList,
      isRunningPotentialScript,
      isRunningRrgScript,
      vnstockRRGUrl,
      viewChart,
      runSSHScript
    };
  },
};

const formatNumber = (number) => {
  if (number === null || number === undefined) {
    return 'N/A';
  }
  return number.toLocaleString() + ' VND';
}

const formatVolume = (volume) => {
  if (volume === null || volume === undefined) {
    return '0';
  }
  return Number(volume).toLocaleString();
}
</script>

<style scoped>
/* ============================== */
/*  STOCK PAGE – Modern Dark UI   */
/* ============================== */

.stk-page {
  background: #ffffff;
  padding: 20px 0 40px;
}

.stk-container {
  max-width: 1280px;
  margin: 0 auto;
  padding: 0 24px;
}

/* ---------- TABS ---------- */
.stk-tabs {
  display: flex;
  gap: 6px;
  margin-bottom: 20px;
  overflow-x: auto;
  scrollbar-width: none;
  -webkit-overflow-scrolling: touch;
}
.stk-tabs::-webkit-scrollbar { display: none; }

.stk-tab {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 10px 20px;
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: 10px;
  font-size: 0.88rem;
  font-weight: 600;
  color: #475569;
  background: rgba(0, 0, 0, 0.02);
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
  box-shadow: 0 1px 3px rgba(0,0,0,0.03);
}
.stk-tab:hover {
  color: #0f172a;
  background: rgba(0, 0, 0, 0.05);
}
.stk-tab--active {
  color: #fff !important;
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%) !important;
  box-shadow: 0 4px 12px rgba(59,130,246,0.2) !important;
  border-color: rgba(0, 0, 0, 0.05);
}
.stk-tab svg { flex-shrink: 0; }

/* ---------- PANEL ---------- */
.stk-panel {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.04);
  overflow: hidden;
  margin-bottom: 20px;
}

/* ---------- HEADER ---------- */
.stk-header {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 22px 24px;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  color: #0f172a;
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}
.stk-header__icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.08);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.stk-header__title {
  font-size: 1.2rem;
  font-weight: 700;
  margin: 0;
  line-height: 1.3;
  font-family: 'Outfit', sans-serif;
  color: #0f172a;
}
.stk-header__sub {
  font-size: 0.82rem;
  color: #475569;
  margin: 2px 0 0;
}

/* ---------- SECTIONS ---------- */
.stk-section {
  padding: 20px 24px;
}
.stk-section--potential {
  border-top: 1px solid rgba(0, 0, 0, 0.06);
}
.stk-label {
  display: block;
  font-size: 0.82rem;
  font-weight: 600;
  color: #475569;
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* ---------- SECTION HEAD ---------- */
.stk-section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 14px;
}
.stk-section-head__title {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 1rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  font-family: 'Outfit', sans-serif;
}
.stk-updated {
  font-size: 0.75rem;
  color: #64748b;
  font-weight: 500;
}
.stk-potential-summary {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
  padding: 0 24px 12px;
  color: #475569;
  font-size: 0.92rem;
}
.stk-potential-summary__left {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  align-items: center;
}
.stk-potential-summary__label {
  font-weight: 700;
  color: #475569;
}
.stk-potential-summary__value {
  color: #0f172a;
}
.stk-potential-summary__right {
  display: flex;
  gap: 8px;
  align-items: center;
  padding: 6px 12px;
  border-radius: 999px;
  font-weight: 700;
}
.stk-potential-summary--bullish {
  background: rgba(16, 185, 129, 0.15);
  color: #10b981;
  border: 1px solid rgba(16, 185, 129, 0.25);
}
.stk-potential-summary--bearish {
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.25);
}
.stk-potential-summary__percentage {
  font-size: 0.98rem;
}
.stk-potential-summary__sentiment {
  font-size: 0.82rem;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

/* ---------- STOCK INFO GRID ---------- */
.stk-info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 10px;
  padding: 0 24px 20px;
}
.stk-info-card {
  background: rgba(0, 0, 0, 0.02);
  border-radius: 10px;
  padding: 14px 16px;
  border: 1px solid rgba(0, 0, 0, 0.06);
  transition: transform 0.15s ease, box-shadow 0.15s ease;
}
.stk-info-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border-color: rgba(0, 0, 0, 0.1);
}
.stk-info-card--accent {
  background: rgba(59, 130, 246, 0.08);
  border-color: rgba(59, 130, 246, 0.2);
}
.stk-info-card--highlight {
  background: rgba(245, 158, 11, 0.08);
  border-color: rgba(245, 158, 11, 0.2);
}
.stk-info-card__label {
  display: block;
  font-size: 0.72rem;
  font-weight: 600;
  color: #475569;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  margin-bottom: 4px;
}
.stk-info-card__value {
  display: block;
  font-size: 0.95rem;
  font-weight: 700;
  color: #0f172a;
  word-break: break-word;
}

/* ---------- CHART (sticky) ---------- */
.stk-sticky-chart {
  position: sticky;
  top: 60px;
  z-index: 20;
  background: #ffffff;
  padding: 12px 0;
  margin-bottom: 12px;
}
.stk-chart-wrap {
  position: relative;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.06);
  background: #ffffff;
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
}
.stk-chart-wrap iframe {
  display: block;
  border: none;
}

/* ---------- ACTION ROW ---------- */
.stk-action-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-top: 10px;
  flex-wrap: wrap;
}

/* ---------- VIEW CHART BUTTON ---------- */
.stk-view-chart-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 7px 14px;
  border: 1px solid rgba(59, 130, 246, 0.2);
  border-radius: 8px;
  background: rgba(59, 130, 246, 0.08);
  color: #2563eb;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
}
.stk-view-chart-btn:hover {
  background: rgba(59, 130, 246, 0.15);
  border-color: rgba(59, 130, 246, 0.35);
  color: #1d4ed8;
}

/* ---------- PRICE ALERT TOGGLE ---------- */
.stk-alert-toggle {
  margin-top: 0;
}
.stk-alert-toggle__btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 7px 14px;
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 8px;
  background: rgba(0, 0, 0, 0.02);
  color: #475569;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
}
.stk-alert-toggle__btn:hover {
  background: rgba(59, 130, 246, 0.08);
  border-color: rgba(59, 130, 246, 0.2);
  color: #2563eb;
}
.stk-alert-toggle__btn svg:last-child {
  transition: transform 0.2s ease;
}
.stk-chevron--open {
  transform: rotate(180deg);
}
.stk-alert-content {
  margin-top: 8px;
}

/* ---------- FILTERS ---------- */
.stk-filters {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  margin-bottom: 14px;
}
.stk-filter-item {
  flex: 1 1 200px;
}
.stk-input {
  width: 100%;
  padding: 9px 14px;
  border: 1px solid rgba(0, 0, 0, 0.1);
  border-radius: 8px;
  font-size: 0.85rem;
  color: #0f172a;
  background: #ffffff;
  transition: border-color 0.2s, box-shadow 0.2s;
  outline: none;
}
.stk-input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
}

/* ---------- TABLE ---------- */
.stk-table-wrap {
  border-radius: 10px;
  border: 1px solid rgba(0, 0, 0, 0.06);
  overflow: hidden;
  background: #ffffff;
}
.stk-table-wrap--scroll {
  max-height: 480px;
  overflow-y: auto;
}
.stk-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}
.stk-th {
  padding: 10px 14px;
  text-align: left;
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: #475569;
  background: #f1f5f9;
  border-bottom: 2px solid #e2e8f0;
  position: sticky;
  top: 0;
  z-index: 2;
}
.stk-th--right { text-align: right; }
.stk-th--center { text-align: center; }
.stk-th--chk { width: 36px; text-align: center; }

.stk-row {
  cursor: pointer;
  transition: background 0.15s ease;
}
.stk-row:hover {
  background: #f8fafc;
}
.stk-row--active {
  background: rgba(59, 130, 246, 0.05) !important;
}
.stk-td {
  padding: 10px 14px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.04);
  vertical-align: middle;
  color: #334155;
}
.stk-td--chk { width: 36px; text-align: center; }
.stk-td--right { text-align: right; }
.stk-td--center { text-align: center; }
.stk-td--symbol {
  font-weight: 700;
  color: #2563eb;
}
.stk-td--mono {
  font-family: 'SF Mono', 'Fira Code', monospace;
  font-size: 0.82rem;
}

.stk-checkbox {
  width: 16px;
  height: 16px;
  accent-color: #3b82f6;
  cursor: pointer;
}

/* ---------- SIGNALS ---------- */
.stk-signal {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
  white-space: nowrap;
}
.stk-signal--near_52w_ath {
  background: rgba(59, 130, 246, 0.08);
  color: #2563eb;
  border: 1px solid rgba(59, 130, 246, 0.2);
}
.stk-signal--ema9_above_ema21 {
  background: rgba(16, 185, 129, 0.08);
  color: #059669;
  border: 1px solid rgba(16, 185, 129, 0.2);
}
.stk-signal--top_growth_20d {
  background: rgba(245, 158, 11, 0.08);
  color: #d97706;
  border: 1px solid rgba(245, 158, 11, 0.2);
}

/* ---------- BUTTONS ---------- */
.stk-actions {
  padding: 16px 0 0;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-wrap: wrap;
  gap: 10px;
}
.stk-actions__group {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  justify-content: center;
}

.stk-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 9px 18px;
  border: none;
  border-radius: 8px;
  font-size: 0.84rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
}
.stk-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}
.stk-btn--primary {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: #fff;
  box-shadow: 0 2px 8px rgba(59,130,246,0.3);
}
.stk-btn--primary:hover:not(:disabled) {
  background: linear-gradient(135deg, #2563eb, #1d4ed8);
  box-shadow: 0 4px 14px rgba(37,99,235,0.4);
  transform: translateY(-1px);
}
.stk-btn--secondary {
  background: rgba(255, 255, 255, 0.08);
  color: #f1f5f9;
}
.stk-btn--secondary:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.15);
}
.stk-btn--outline {
  background: rgba(255, 255, 255, 0.03);
  color: #cbd5e1;
  border: 1px solid rgba(255, 255, 255, 0.08);
}
.stk-btn--outline:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.08);
  border-color: rgba(255, 255, 255, 0.15);
}
.stk-btn--scan {
  min-width: 180px;
  justify-content: center;
}

/* ---------- LOADING / SPINNER ---------- */
.stk-loading {
  display: flex;
  justify-content: center;
  padding: 20px 0;
}
.stk-spinner {
  width: 32px;
  height: 32px;
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: stk-spin 0.7s linear infinite;
}
@keyframes stk-spin {
  to { transform: rotate(360deg); }
}

/* ---------- MESSAGE ---------- */
.stk-message {
  text-align: center;
  font-size: 0.85rem;
  color: #64748b;
  padding: 10px 0;
  margin: 0;
}

/* ---------- RRG ---------- */
.stk-rrg-wrap {
  padding: 24px;
  text-align: center;
}
.stk-rrg-img {
  max-width: 100%;
  height: auto;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.5);
  border: 1px solid rgba(255, 255, 255, 0.08);
}

/* ---------- CATEGORY BADGE ---------- */
.stk-category-badge {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 10px;
  padding: 8px 14px;
  background: linear-gradient(135deg, #1e293b 0%, #0d0f17 100%);
  border-radius: 10px;
  color: #fff;
  border: 1px solid rgba(255, 255, 255, 0.06);
}
.stk-category-badge__code {
  font-weight: 700;
  font-size: 0.9rem;
  color: #60a5fa;
}
.stk-category-badge__name {
  font-size: 0.82rem;
  color: rgba(255,255,255,0.75);
}

/* ---------- CATEGORY EXCHANGE TAG ---------- */
.stk-category-exchange {
  display: inline-block;
  padding: 3px 10px;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 600;
  background: rgba(59, 130, 246, 0.12);
  color: #60a5fa;
  border: 1px solid rgba(59, 130, 246, 0.2);
  white-space: nowrap;
}

/* ---------- v-select override ---------- */
.stk-select :deep(.vs__dropdown-toggle) {
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 6px 10px;
  min-height: 40px;
  background: #ffffff;
  transition: border-color 0.2s, box-shadow 0.2s;
}
.stk-select :deep(.vs__dropdown-toggle:focus-within) {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59,130,246,0.15);
}
.stk-select :deep(.vs__search) {
  font-size: 0.88rem;
  color: #0f172a;
}
.stk-select :deep(.vs__selected) {
  font-size: 0.88rem;
  font-weight: 600;
  color: #0f172a;
}
.stk-select :deep(.vs__dropdown-menu) {
  border-radius: 8px;
  border: 1px solid #cbd5e1;
  background: #ffffff;
  box-shadow: 0 8px 24px rgba(0,0,0,0.05);
  max-height: 280px;
}
.stk-select :deep(.vs__dropdown-option) {
  color: #334155;
  padding: 8px 16px;
}
.stk-select :deep(.vs__dropdown-option--highlight) {
  background: rgba(59, 130, 246, 0.08);
  color: #2563eb;
}

/* ---------- RESPONSIVE ---------- */
@media (max-width: 640px) {
  .stk-container { padding: 0 10px; }
  .stk-header { padding: 16px; }
  .stk-section { padding: 16px; }
  .stk-sticky-chart {
    padding: 0 12px 10px;
  }
  .stk-tab {
    padding: 8px 14px;
    font-size: 0.82rem;
  }
  .stk-filters {
    flex-direction: column;
  }
  .stk-table-wrap--scroll {
    max-height: 400px;
  }
}
</style>
