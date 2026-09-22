<template>
  <div class="jnl">
    <!-- Header -->
    <div class="jnl-header">
      <div class="jnl-header-left">
        <div class="jnl-total-label">Tổng tài sản ròng (VND)</div>
        <div class="jnl-total-value" :class="{ 'jnl-negative': totalAssetValueVnd < 0 }">
          {{ formatCurrency(totalAssetValueVnd, 'VND') }}
        </div>
        <div class="jnl-meta">
          <span v-if="isRateLoading" class="jnl-meta-item">⏳ Đang tải tỷ giá...</span>
          <span v-else-if="usdToVndRate" class="jnl-meta-item">💱 1 USD = {{ formatNumber(usdToVndRate) }} VND</span>
          <span v-else-if="hasUsdEntries" class="jnl-meta-item jnl-meta-warn">⚠️ Thiếu tỷ giá USD/VND</span>
          <span v-if="goldLatestDate" class="jnl-meta-item">🥇 Gold: {{ goldLatestDate }}</span>
        </div>
      </div>
      <div class="jnl-header-actions">
        <button class="jnl-chart-btn" @click="openAllocationModal">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none"><path d="M8 8L8 1a7 7 0 1 1-6.06 3.5L8 8z" stroke="currentColor" stroke-width="1.3" stroke-linejoin="round"/><path d="M8 1a7 7 0 0 1 7 7H8V1z" fill="currentColor" opacity="0.35"/></svg>
          Tỷ lệ danh mục
        </button>
        <button class="jnl-add-btn" @click="openModal('add')">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none"><path d="M8 3v10M3 8h10" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>
          Thêm mới
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="jnl-loading">
      <div class="spinner-border text-primary" role="status"></div>
      <p>Đang tải dữ liệu...</p>
    </div>

    <!-- Empty -->
    <div v-else-if="entries.length === 0" class="jnl-empty">
      <div class="jnl-empty-icon">📒</div>
      <h5>Chưa có khoản đầu tư nào</h5>
      <p>Bắt đầu bằng cách thêm tài sản hoặc khoản nợ đầu tiên</p>
      <button class="jnl-add-btn" @click="openModal('add')">+ Thêm mới</button>
    </div>

    <!-- Table -->
    <div v-else class="jnl-table-wrap">
      <table class="jnl-table">
        <thead>
          <tr>
            <th>
              <button type="button" class="jnl-sort-btn" @click="toggleSort('entry_date')">
                <span>Ngày</span>
                <span class="jnl-sort-indicator" :class="getSortIndicatorClass('entry_date')">{{ getSortIndicator('entry_date') }}</span>
              </button>
            </th>
            <th>
              <button type="button" class="jnl-sort-btn" @click="toggleSort('asset_type')">
                <span>Loại</span>
                <span class="jnl-sort-indicator" :class="getSortIndicatorClass('asset_type')">{{ getSortIndicator('asset_type') }}</span>
              </button>
            </th>
            <th>
              <button type="button" class="jnl-sort-btn" @click="toggleSort('symbol')">
                <span>Tên / Mã</span>
                <span class="jnl-sort-indicator" :class="getSortIndicatorClass('symbol')">{{ getSortIndicator('symbol') }}</span>
              </button>
            </th>
            <th class="text-end">
              <button type="button" class="jnl-sort-btn jnl-sort-btn--right" @click="toggleSort('quantity')">
                <span>SL</span>
                <span class="jnl-sort-indicator" :class="getSortIndicatorClass('quantity')">{{ getSortIndicator('quantity') }}</span>
              </button>
            </th>
            <th class="text-end">
              <button type="button" class="jnl-sort-btn jnl-sort-btn--right" @click="toggleSort('price')">
                <span>Giá mua</span>
                <span class="jnl-sort-indicator" :class="getSortIndicatorClass('price')">{{ getSortIndicator('price') }}</span>
              </button>
            </th>
            <th class="text-end">
              <button type="button" class="jnl-sort-btn jnl-sort-btn--right" @click="toggleSort('book_value')">
                <span>Giá trị</span>
                <span class="jnl-sort-indicator" :class="getSortIndicatorClass('book_value')">{{ getSortIndicator('book_value') }}</span>
              </button>
            </th>
            <th class="text-end">
              <button type="button" class="jnl-sort-btn jnl-sort-btn--right" @click="toggleSort('current_value')">
                <span>Hiện tại</span>
                <span class="jnl-sort-indicator" :class="getSortIndicatorClass('current_value')">{{ getSortIndicator('current_value') }}</span>
              </button>
            </th>
            <th class="text-end">
              <button type="button" class="jnl-sort-btn jnl-sort-btn--right" @click="toggleSort('change_percent')">
                <span>% Thay đổi</span>
                <span class="jnl-sort-indicator" :class="getSortIndicatorClass('change_percent')">{{ getSortIndicator('change_percent') }}</span>
              </button>
            </th>
            <th class="text-center">Info</th>
            <th class="text-center">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="entry in sortedEntries" :key="entry.id" :class="{ 'jnl-row-debt': entry.asset_type === 'DEBT' }">
            <td class="jnl-cell-date">{{ formatDate(entry.entry_date) }}</td>
            <td>
              <span class="jnl-badge" :class="'jnl-badge--' + (entry.asset_type || 'OTHER').toLowerCase()">
                {{ entry.asset_type }}
              </span>
            </td>
            <td class="jnl-cell-symbol">
              {{ entry.symbol }}
              <span class="jnl-currency-tag" :class="entry.currency === 'USD' ? 'jnl-currency-tag--usd' : ''">{{ entry.currency || 'VND' }}</span>
            </td>
            <td class="text-end">{{ formatNumber(entry.quantity) }}</td>
            <td class="text-end">{{ formatCurrency(entry.price, entry.currency) }}</td>
            <td class="text-end fw-600">
              <span :class="entry.asset_type === 'DEBT' ? 'jnl-negative' : ''">
                {{ formatCurrency(entry.asset_type === 'DEBT' ? -(entry.price * entry.quantity) : (entry.price * entry.quantity), entry.currency) }}
              </span>
            </td>
            <td class="text-end fw-600">
              <template v-if="entry.asset_type === 'DEBT'">
                <span class="jnl-negative">{{ formatCurrency(-(entry.price * entry.quantity), entry.currency) }}</span>
              </template>
              <template v-else-if="getCurrentValue(entry) !== null">
                {{ formatCurrency(getCurrentValue(entry), entry.currency) }}
              </template>
              <template v-else>
                <span class="jnl-muted">—</span>
              </template>
            </td>
            <td class="text-end">
              <template v-if="entry.asset_type === 'DEBT' || entry.asset_type === 'CASH'">
                <span class="jnl-muted">—</span>
              </template>
              <template v-else>
                <span v-if="getChangePercent(entry) !== null"
                  class="jnl-change"
                  :class="getChangePercent(entry) > 0 ? 'jnl-change--up' : getChangePercent(entry) < 0 ? 'jnl-change--down' : ''">
                  {{ getChangePercent(entry) > 0 ? '+' : '' }}{{ getChangePercent(entry).toFixed(2) }}%
                </span>
                <span v-else class="jnl-muted">—</span>
              </template>
            </td>
            <td class="text-center">
              <span v-if="entry.notes" class="jnl-note-icon" :title="entry.notes">
                <svg width="16" height="16" viewBox="0 0 16 16" fill="none"><circle cx="8" cy="8" r="7" stroke="currentColor" stroke-width="1.4"/><path d="M8 7v4" stroke="currentColor" stroke-width="1.4" stroke-linecap="round"/><circle cx="8" cy="5" r="0.8" fill="currentColor"/></svg>
                <span class="jnl-note-tooltip">{{ entry.notes }}</span>
              </span>
              <span v-else class="jnl-muted">—</span>
            </td>
            <td class="text-center jnl-cell-actions">
              <button class="jnl-icon-btn jnl-icon-btn--edit" @click="openModal('edit', entry)" title="Sửa">
                <svg width="15" height="15" viewBox="0 0 16 16" fill="none"><path d="M11.5 1.5l3 3L5 14H2v-3L11.5 1.5z" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"/></svg>
              </button>
              <button class="jnl-icon-btn jnl-icon-btn--del" @click="deleteEntry(entry.id)" title="Xóa">
                <svg width="15" height="15" viewBox="0 0 16 16" fill="none"><path d="M2 4h12M5.333 4V2.667a1.333 1.333 0 011.334-1.334h2.666a1.333 1.333 0 011.334 1.334V4m2 0v9.333a1.333 1.333 0 01-1.334 1.334H4.667a1.333 1.333 0 01-1.334-1.334V4h9.334z" stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- AI Section -->
    <div class="jnl-ai" :class="{ 'jnl-ai--initial': !generatedPrompt }">
      <div class="jnl-ai-header">
        <svg width="20" height="20" viewBox="0 0 20 20" fill="none"><circle cx="10" cy="10" r="9" stroke="#3b82f6" stroke-width="1.5"/><path d="M7 10l2 2 4-4" stroke="#3b82f6" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>
        <h4>AI Market Analysis</h4>
      </div>
      <div v-if="!generatedPrompt">
        <button class="jnl-ai-btn" @click="generateAiPrompt">✨ Tạo prompt phân tích</button>
      </div>
      <div v-else>
        <textarea class="jnl-ai-textarea" rows="5" v-model="generatedPrompt"></textarea>
        <div class="jnl-ai-actions">
          <button class="jnl-ai-btn jnl-ai-btn--go" @click="askAI" :disabled="isAnalyzing">
            {{ isAnalyzing ? '⏳ Đang phân tích...' : '🚀 Hỏi AI' }}
          </button>
          <button class="jnl-ai-btn jnl-ai-btn--cancel" @click="generatedPrompt = ''">Hủy</button>
        </div>
        <div v-if="aiResponse" class="jnl-ai-result">
          <strong>📊 Kết quả phân tích:</strong>
          <div class="jnl-ai-content" style="margin-top: 0.5rem;" v-html="parsedAiResponse"></div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="jnl-overlay" @click.self="closeModal">
      <div class="jnl-modal">
        <div class="jnl-modal-header">
          <h3>{{ modalMode === 'add' ? '➕ Thêm khoản mới' : '✏️ Chỉnh sửa' }}</h3>
          <button class="jnl-modal-close" @click="closeModal">✕</button>
        </div>
        <form @submit.prevent="submitForm" class="jnl-form">
          <div class="jnl-form-group">
            <label>Loại tài sản</label>
            <select v-model="formData.asset_type" required>
              <option value="STOCK">📈 Cổ phiếu</option>
              <option value="CRYPTO">₿ Crypto</option>
              <option value="GOLD">🥇 Vàng</option>
              <option value="SILVER">🥈 Bạc</option>
              <option value="CASH">💵 Tiền mặt</option>
              <option value="REAL_ESTATE">🏠 Bất động sản</option>
              <option value="DEBT">🔴 Nợ (Debt)</option>
              <option value="OTHER">📦 Khác</option>
            </select>
          </div>

          <div v-if="isRealEstate" class="jnl-form-group">
            <label>Loại bất động sản</label>
            <select v-model="realEstateCategory">
              <option value="NHA">🏠 Nhà</option>
              <option value="DAT">🧱 Đất</option>
              <option value="CHUNG_CU">🏢 Chung cư</option>
            </select>
          </div>

          <div class="jnl-form-group">
            <label>Tên / Mã</label>
            <input type="text" v-model="formData.symbol" placeholder="VD: SJC, VN30, BTC..." :disabled="isCash || isDebt" :required="!isCash && !isDebt" />
          </div>

          <div class="jnl-form-row">
            <div class="jnl-form-group">
              <label>{{ isDebt ? 'Số tiền nợ' : 'Số lượng' }}</label>
              <input type="text" inputmode="numeric"
                :value="quantityDisplay"
                @input="onQuantityInput"
                @blur="onQuantityBlur"
                @focus="onQuantityFocus"
                required />
            </div>
            <div class="jnl-form-group">
              <label>Giá (mỗi đơn vị)</label>
              <input type="text" inputmode="numeric"
                :value="priceDisplay"
                @input="onPriceInput"
                @blur="onPriceBlur"
                @focus="onPriceFocus"
                :disabled="isCash || isDebt"
                :required="!isCash && !isDebt" />
            </div>
          </div>

          <div class="jnl-form-row">
            <div class="jnl-form-group">
              <label>Ngày</label>
              <input type="datetime-local" v-model="formData.entry_date" required />
            </div>
            <div class="jnl-form-group">
              <label>Tiền tệ</label>
              <select v-model="formData.currency" required>
                <option value="VND">VND</option>
                <option value="USD">USD</option>
              </select>
            </div>
          </div>

          <div class="jnl-form-group mb-3">
            <div class="form-check form-switch p-0 d-flex align-items-center justify-content-between">
              <label class="form-check-label fw-semibold text-secondary mb-0" for="manualCurrentPriceSwitch" style="cursor: pointer;">
                ⚙️ Tự nhập giá hiện tại thủ công
              </label>
              <input class="form-check-input" type="checkbox" role="switch" id="manualCurrentPriceSwitch" v-model="useManualCurrentPrice" style="cursor: pointer; width: 2.2em; height: 1.1em; float: right; margin-left: auto;">
            </div>
          </div>

          <div v-if="useManualCurrentPrice" class="jnl-form-group">
            <label>Giá hiện tại thủ công (mỗi đơn vị)</label>
            <input type="text" inputmode="numeric"
              :value="manualCurrentPriceDisplay"
              @input="onManualCurrentPriceInput"
              @blur="onManualCurrentPriceBlur"
              @focus="onManualCurrentPriceFocus"
              placeholder="VD: 78,500,000"
              required />
          </div>

          <div class="jnl-form-group">
            <label>Ghi chú</label>
            <textarea v-model="formData.notes" rows="2" placeholder="Lãi suất, mục đích, ghi nhớ..."></textarea>
          </div>

          <button type="submit" class="jnl-submit-btn">
            {{ modalMode === 'add' ? '💾 Lưu' : '✅ Cập nhật' }}
          </button>
        </form>
      </div>
    </div>

    <div v-if="showAllocationModal" class="jnl-overlay" @click.self="closeAllocationModal">
      <div class="jnl-modal jnl-modal--allocation">
        <div class="jnl-modal-header">
          <h3>🥧 Tỷ lệ danh mục tài sản</h3>
          <button class="jnl-modal-close" @click="closeAllocationModal">✕</button>
        </div>
        <div class="jnl-allocation-body">
          <div v-if="allocationSegments.length === 0" class="jnl-allocation-empty">
            Chưa có dữ liệu tài sản để hiển thị biểu đồ.
          </div>
          <template v-else>
            <div class="jnl-pie-wrap">
              <div class="jnl-pie-chart" :style="pieChartConicStyle">
                <div class="jnl-pie-center">
                  <strong>100%</strong>
                  <span>Danh mục</span>
                </div>
              </div>
            </div>
            <div class="jnl-allocation-total">
              Tổng tài sản quy đổi: {{ formatCurrency(totalAllocationValue, 'VND') }}
            </div>
            <div class="jnl-allocation-list">
              <div v-for="segment in allocationSegments" :key="segment.key" class="jnl-allocation-item">
                <span class="jnl-allocation-label">
                  <span class="jnl-allocation-dot" :style="{ backgroundColor: segment.color }"></span>
                  {{ segment.label }}
                </span>
                <span class="jnl-allocation-value">
                  {{ segment.percent.toFixed(1) }}% ({{ formatCurrency(segment.value, 'VND') }})
                </span>
              </div>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, reactive, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { parseMarkdown } from '@/utils/markdown';

export default {
  name: 'JournalComponent',
  props: {
    accountNumber: {
      type: String,
      default: ''
    }
  },
  setup(props) {
    const router = useRouter();
    const entries = ref([]);
    const isLoading = ref(true);
    const showModal = ref(false);
    const showAllocationModal = ref(false);
    const modalMode = ref('add');
    const realEstateCategory = ref('NHA');
    const useManualCurrentPrice = ref(false);
    const manualCurrentPriceDisplay = ref('0');
    const formData = reactive({
      id: null,
      asset_type: 'STOCK',
      symbol: '',
      quantity: 1,
      price: 0,
      currency: 'VND',
      entry_date: new Date().toISOString().slice(0, 16),
      notes: '',
      current_price: null
    });

    const quantityDisplay = ref('1');
    const priceDisplay = ref('0');
    
    const isCash = computed(() => formData.asset_type === 'CASH');
    const isDebt = computed(() => formData.asset_type === 'DEBT');
    const isRealEstate = computed(() => formData.asset_type === 'REAL_ESTATE');

    const realEstateSymbolMap = {
      NHA: 'NHA',
      DAT: 'DAT',
      CHUNG_CU: 'CHUNG_CU'
    };

    const assetTypeLabels = {
      STOCK: 'Cổ phiếu',
      CRYPTO: 'Crypto',
      GOLD: 'Vàng',
      SILVER: 'Bạc',
      CASH: 'Tiền mặt',
      REAL_ESTATE: 'Bất động sản',
      DEBT: 'Nợ',
      OTHER: 'Khác'
    };
    const sortState = ref({
      field: 'entry_date',
      direction: 'desc'
    });

    const setRealEstateSymbolFromCategory = () => {
      if (!isRealEstate.value) return;
      formData.symbol = realEstateSymbolMap[realEstateCategory.value] || 'NHA';
    };

    watch(() => formData.asset_type, (newType) => {
        if (newType === 'CASH') {
            formData.symbol = 'CASH';
            formData.price = 1;
            priceDisplay.value = '1';
        } else if (newType === 'DEBT') {
            formData.symbol = 'DEBT';
            formData.price = 1;
            priceDisplay.value = '1';
      } else if (newType === 'REAL_ESTATE') {
        setRealEstateSymbolFromCategory();
        }
    });

    watch(realEstateCategory, () => {
      setRealEstateSymbolFromCategory();
    });

    // AI Feature State
    const generatedPrompt = ref('');
    const aiResponse = ref('');
    const parsedAiResponse = computed(() => parseMarkdown(aiResponse.value));
    const isAnalyzing = ref(false);
    const usdToVndRate = ref(null);
    const isRateLoading = ref(false);
    const marketRates = ref([]);
    const goldPriceRows = ref([]);
    const goldLatestDate = ref('');
    const dealProfitBySymbol = ref({});
    const FX_RATE_CACHE_KEY = 'journal_usd_vnd_rate_cache';
    const FX_RATE_CACHE_TTL_MS = 24 * 60 * 60 * 1000;
    const MARKET_RATES_CACHE_KEY = 'journal_market_rates_cache';
    const MARKET_RATES_CACHE_TTL_MS = 30 * 60 * 1000;
    const FX_RATE_ERROR_COOLDOWN_KEY = 'journal_usd_vnd_rate_error_cooldown_until';
    const FX_RATE_ERROR_COOLDOWN_MS = 60 * 60 * 1000;
    const GOLD_PRICE_API_URL = 'https://trading-signals-pi.vercel.app/goldprice/services/priceservice.ashx';
    const GOLD_PRICE_CACHE_KEY = 'journal_gold_prices_cache';
    const GOLD_PRICE_CACHE_TTL_MS = 15 * 60 * 1000;

    const hasUsdEntries = computed(() => {
      return entries.value.some(entry => (entry.currency || 'VND') === 'USD');
    });

    const toNumber = (value) => {
      const num = typeof value === 'string'
        ? parseFloat(value.replace(/,/g, ''))
        : Number(value);
      return Number.isFinite(num) ? num : null;
    };

    const extractUsdVndRate = (rates) => {
      if (!Array.isArray(rates)) return null;

      const findByCode = (codes) => rates.find(item => {
        const code = String(item?.currency || item?.symbol || item?.pair || '')
          .toUpperCase()
          .replace(/[^A-Z]/g, '');
        return codes.includes(code);
      });

      const direct = findByCode(['USDVND']);
      if (direct) {
        return toNumber(direct.rate) ?? toNumber(direct.close) ?? toNumber(direct.bid) ?? toNumber(direct.ask);
      }

      const inverse = findByCode(['VNDUSD']);
      if (inverse) {
        const inverseRate = toNumber(inverse.rate) ?? toNumber(inverse.close) ?? toNumber(inverse.bid) ?? toNumber(inverse.ask);
        return inverseRate ? 1 / inverseRate : null;
      }

      return null;
    };

    const fetchUsdVndRate = async () => {
      isRateLoading.value = true;
      try {
        const response = await fetch('/api/rates');
        if (!response.ok) {
          localStorage.setItem(FX_RATE_ERROR_COOLDOWN_KEY, String(Date.now() + FX_RATE_ERROR_COOLDOWN_MS));
          return;
        }

        const data = await response.json();
        marketRates.value = Array.isArray(data) ? data : [];
        localStorage.setItem(MARKET_RATES_CACHE_KEY, JSON.stringify({
          data: marketRates.value,
          cachedAt: Date.now()
        }));
        const parsedRate = extractUsdVndRate(data);
        if (parsedRate) {
          usdToVndRate.value = parsedRate;
          localStorage.setItem(FX_RATE_CACHE_KEY, JSON.stringify({
            rate: parsedRate,
            cachedAt: Date.now()
          }));
          localStorage.removeItem(FX_RATE_ERROR_COOLDOWN_KEY);
          return;
        }

        localStorage.setItem(FX_RATE_ERROR_COOLDOWN_KEY, String(Date.now() + FX_RATE_ERROR_COOLDOWN_MS));
      } catch (error) {
        console.error('Error fetching USD/VND rate:', error);
        localStorage.setItem(FX_RATE_ERROR_COOLDOWN_KEY, String(Date.now() + FX_RATE_ERROR_COOLDOWN_MS));
      } finally {
        isRateLoading.value = false;
      }
    };

    const loadUsdVndRate = async () => {
      let hasFreshCache = false;
      let hasFreshMarketRates = false;

      try {
        const marketRatesCacheRaw = localStorage.getItem(MARKET_RATES_CACHE_KEY);
        if (marketRatesCacheRaw) {
          const cache = JSON.parse(marketRatesCacheRaw);
          const cachedAt = Number(cache?.cachedAt);
          const cachedData = Array.isArray(cache?.data) ? cache.data : [];
          const isFresh = Number.isFinite(cachedAt) && (Date.now() - cachedAt) < MARKET_RATES_CACHE_TTL_MS;

          if (cachedData.length > 0) {
            marketRates.value = cachedData;
            hasFreshMarketRates = isFresh;
          }
        }
      } catch (error) {
        console.error('Error reading market rates cache:', error);
      }

      try {
        const cacheRaw = localStorage.getItem(FX_RATE_CACHE_KEY);
        if (cacheRaw) {
          const cache = JSON.parse(cacheRaw);
          const cachedRate = toNumber(cache?.rate);
          const cachedAt = Number(cache?.cachedAt);
          const isFresh = Number.isFinite(cachedAt) && (Date.now() - cachedAt) < FX_RATE_CACHE_TTL_MS;

          if (cachedRate) {
            usdToVndRate.value = cachedRate;
            hasFreshCache = isFresh;
          }
        }
      } catch (error) {
        console.error('Error reading USD/VND rate cache:', error);
      }

      if (hasFreshCache && hasFreshMarketRates) return;

      const cooldownUntil = Number(localStorage.getItem(FX_RATE_ERROR_COOLDOWN_KEY) || 0);
      if (Number.isFinite(cooldownUntil) && cooldownUntil > Date.now()) {
        return;
      }

      await fetchUsdVndRate();
    };

    const normalizeCode = (value) => {
      return String(value || '').toUpperCase().replace(/[^A-Z0-9]/g, '');
    };

    const normalizeText = (value) => {
      return String(value || '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/đ/g, 'd')
        .replace(/Đ/g, 'D')
        .toUpperCase()
        .replace(/[^A-Z0-9 ]/g, ' ')
        .replace(/\s+/g, ' ')
        .trim();
    };

    const getRateNumber = (rateItem) => {
      return toNumber(rateItem?.rate) ?? toNumber(rateItem?.close) ?? toNumber(rateItem?.bid) ?? toNumber(rateItem?.ask);
    };

    const findMarketRateExact = (candidates) => {
      if (!Array.isArray(marketRates.value) || marketRates.value.length === 0) return null;

      const exactCandidates = candidates
        .map(item => String(item || '').toUpperCase().trim())
        .filter(Boolean);
      if (exactCandidates.length === 0) return null;

      for (const candidate of exactCandidates) {
        const matched = marketRates.value.find(item => String(item?.currency || item?.symbol || item?.pair || '').toUpperCase().trim() === candidate);
        if (!matched) continue;

        const value = getRateNumber(matched);
        if (value !== null) return value;
      }

      return null;
    };

    const findMarketRate = (candidates) => {
      if (!Array.isArray(marketRates.value) || marketRates.value.length === 0) return null;

      const normalizedCandidates = candidates.map(normalizeCode).filter(Boolean);
      if (normalizedCandidates.length === 0) return null;

      for (const item of marketRates.value) {
        const itemCode = normalizeCode(item?.currency || item?.symbol || item?.pair);
        if (!itemCode || !normalizedCandidates.includes(itemCode)) continue;

        const value = getRateNumber(item);
        if (value !== null) return value;
      }

      return null;
    };

    const toBuyValueVnd = (item) => {
      let value = null;
      const buyValue = toNumber(item?.BuyValue);

      if (buyValue !== null) {
        value = buyValue;
      } else {
        const buyText = String(item?.Buy || '').replace(/,/g, '');
        value = toNumber(buyText);
      }

      if (value === null) return null;

      // Gold prices in Vietnam are usually quoted per tael (lượng) and are currently around 70M - 90M VND.
      // We normalize the value to raw VND based on its scale:
      // - Values under 1000 (e.g. 81.3) represent million VND
      // - Values under 1000000 (e.g. 81300) represent thousand VND
      // - Values 1000000 and above (e.g. 81300000) are already in VND
      if (value < 1000) {
        return value * 1000000;
      } else if (value < 1000000) {
        return value * 1000;
      } else {
        return value;
      }
    };

    const loadGoldPrices = async () => {
      try {
        const cacheRaw = localStorage.getItem(GOLD_PRICE_CACHE_KEY);
        if (cacheRaw) {
          const cache = JSON.parse(cacheRaw);
          const cachedAt = Number(cache?.cachedAt);
          const isFresh = Number.isFinite(cachedAt) && (Date.now() - cachedAt) < GOLD_PRICE_CACHE_TTL_MS;
          const cachedData = Array.isArray(cache?.data) ? cache.data : [];
          const cachedLatestDate = String(cache?.latestDate || '');

          if (cachedData.length > 0) {
            goldPriceRows.value = cachedData;
            goldLatestDate.value = cachedLatestDate;
            if (isFresh) return;
          }
        }
      } catch (error) {
        console.error('Error reading gold price cache:', error);
      }

      let success = false;
      let rows = [];
      let latestDate = '';

      // 1. Try local/proxy relative SJC endpoint
      try {
        const response = await fetch('/goldprice/services/priceservice.ashx');
        if (response.ok && response.headers.get('content-type')?.includes('json')) {
          const data = await response.json();
          if (data && Array.isArray(data.data) && data.data.length > 0) {
            rows = data.data;
            latestDate = String(data.latestDate || '');
            success = true;
          }
        }
      } catch (error) {
        console.warn('SJC relative fetch failed, trying absolute...', error);
      }

      // 2. Try absolute Vercel path
      if (!success) {
        try {
          const response = await fetch(GOLD_PRICE_API_URL);
          if (response.ok && response.headers.get('content-type')?.includes('json')) {
            const data = await response.json();
            if (data && Array.isArray(data.data) && data.data.length > 0) {
              rows = data.data;
              latestDate = String(data.latestDate || '');
              success = true;
            }
          }
        } catch (error) {
          console.warn('SJC absolute fetch failed, trying fallback...', error);
        }
      }

      // 3. Fallback: Try giavang.now public API (CORS-friendly, no Cloudflare block)
      if (!success) {
        try {
          const response = await fetch('https://giavang.now/api/prices');
          if (response.ok) {
            const data = await response.json();
            if (data && data.success && data.prices) {
              rows = [];
              for (const [key, item] of Object.entries(data.prices)) {
                if (key === 'XAUUSD') continue; // Skip world gold
                rows.push({
                  Id: key,
                  TypeName: item.name || key,
                  BranchName: item.name?.toLowerCase().includes('hanoi') || item.name?.toLowerCase().includes('hà nội') ? 'Hà Nội' : 'TP.HCM',
                  Buy: String(item.buy),
                  Sell: String(item.sell),
                  BuyValue: item.buy,
                  SellValue: item.sell
                });
              }
              latestDate = `${data.date || ''} ${data.time || ''}`.trim();
              if (rows.length > 0) {
                success = true;
              }
            }
          }
        } catch (error) {
          console.error('All SJC gold price API sources failed:', error);
        }
      }

      if (success && rows.length > 0) {
        goldPriceRows.value = rows;
        goldLatestDate.value = latestDate;
        try {
          localStorage.setItem(GOLD_PRICE_CACHE_KEY, JSON.stringify({
            data: rows,
            latestDate,
            cachedAt: Date.now()
          }));
        } catch (error) {
          console.error('Error writing gold price cache:', error);
        }
      }
    };

    const findGoldBuyValueBySymbol = (symbol) => {
      const normalizedSymbol = normalizeText(symbol);
      if (!normalizedSymbol || !Array.isArray(goldPriceRows.value) || goldPriceRows.value.length === 0) return null;

      const rowsWithBranch = goldPriceRows.value
        .filter(item => {
          const typeName = normalizeText(item?.TypeName);
          return typeName.includes(normalizedSymbol);
        })
        .sort((a, b) => {
          const aHcm = normalizeText(a?.BranchName).includes('HO CHI MINH') ? 1 : 0;
          const bHcm = normalizeText(b?.BranchName).includes('HO CHI MINH') ? 1 : 0;
          return bHcm - aHcm;
        });

      if (rowsWithBranch.length === 0) return null;

      return toBuyValueVnd(rowsWithBranch[0]);
    };

    const getNormalizedSymbol = (entry) => String(entry?.symbol || '').toUpperCase().trim();

    const getUnrealizedProfit = (entry) => {
      const normalizedSymbol = getNormalizedSymbol(entry);
      return toNumber(dealProfitBySymbol.value[normalizedSymbol]) ?? 0;
    };

    const hasDealBySymbol = (entry) => {
      const normalizedSymbol = getNormalizedSymbol(entry);
      return Object.prototype.hasOwnProperty.call(dealProfitBySymbol.value, normalizedSymbol);
    };

    const getCurrentPrice = (entry) => {
      if (entry && entry.current_price !== undefined && entry.current_price !== null && entry.current_price !== 0) {
        return entry.current_price;
      }

      const assetType = String(entry?.asset_type || '').toUpperCase();
      const quantity = toNumber(entry?.quantity) ?? 0;
      const entryPrice = toNumber(entry?.price);

      // unrealizedProfit is for the whole position, so convert to per-unit only for display.
      if (assetType === 'STOCK' && hasDealBySymbol(entry) && entryPrice !== null && quantity > 0) {
        return entryPrice + (getUnrealizedProfit(entry) / quantity);
      }

      if (assetType !== 'CRYPTO' && assetType !== 'GOLD') {
        return entryPrice;
      }

      if (assetType === 'GOLD') {
        const goldBuyValueVnd = findGoldBuyValueBySymbol(entry?.symbol);
        if (goldBuyValueVnd === null) return null;

        const currency = entry?.currency || 'VND';

        if (currency === 'VND') {
          return goldBuyValueVnd;
        }

        if (!usdToVndRate.value) return null;
        return goldBuyValueVnd / usdToVndRate.value;
      }

      const symbol = String(entry?.symbol || '').toUpperCase().trim();
      if (!symbol) return null;

      const symbolNoSlash = symbol.replace('/', '');
      let base = symbolNoSlash;
      if (symbolNoSlash.endsWith('USDT')) {
        base = symbolNoSlash.slice(0, -4);
      } else if (symbolNoSlash.endsWith('USD')) {
        base = symbolNoSlash.slice(0, -3);
      }

      // CRYPTO pricing should prioritize exact BASE/USD first, then BASEUSD, then BASE itself.
      const pairSlash = `${base}/USD`;
      const pairCompact = `${base}USD`;
      const rateInUsd = findMarketRateExact([pairSlash, pairCompact]) ?? 
                        findMarketRate([pairSlash, pairCompact]) ?? 
                        findMarketRateExact([base]) ?? 
                        findMarketRate([base]);
      if (rateInUsd === null) return null;

      const currency = entry?.currency || 'VND';
      if (currency === 'VND') {
        const marketPrice = usdToVndRate.value ? rateInUsd * usdToVndRate.value : null;
        return marketPrice;
      }
      return rateInUsd;
    };

    const getCurrentValue = (entry) => {
      const assetType = String(entry?.asset_type || '').toUpperCase();
      const quantity = toNumber(entry?.quantity) ?? 0;
      const entryPrice = toNumber(entry?.price) ?? 0;

      if (assetType === 'STOCK' && hasDealBySymbol(entry)) {
        return (entryPrice * quantity) + getUnrealizedProfit(entry);
      }

      if (assetType === 'GOLD') {
        const unitCurrentPrice = getCurrentPrice(entry);
        if (unitCurrentPrice === null) return entryPrice * quantity;
        return unitCurrentPrice * quantity;
      }

      const currentPrice = getCurrentPrice(entry);
      if (currentPrice === null) return entryPrice * quantity;
      return currentPrice * quantity;
    };

    const getEntryDisplayValue = (entry) => {
      const currentValue = getCurrentValue(entry);
      if (currentValue !== null) return currentValue;
      return (entry?.price || 0) * (entry?.quantity || 0);
    };

    const getBookValue = (entry) => {
      const rawValue = (toNumber(entry?.price) ?? 0) * (toNumber(entry?.quantity) ?? 0);
      return String(entry?.asset_type || '').toUpperCase() === 'DEBT' ? -rawValue : rawValue;
    };

    const compareNullable = (left, right) => {
      const leftMissing = left === null || left === undefined || left === '';
      const rightMissing = right === null || right === undefined || right === '';

      if (leftMissing && rightMissing) return 0;
      if (leftMissing) return 1;
      if (rightMissing) return -1;

      if (typeof left === 'number' && typeof right === 'number') {
        return left - right;
      }

      return String(left).localeCompare(String(right), 'vi', { numeric: true, sensitivity: 'base' });
    };

    const getSortValue = (entry, field) => {
      switch (field) {
        case 'entry_date':
          return new Date(entry?.entry_date || 0).getTime();
        case 'asset_type':
          return assetTypeLabels[String(entry?.asset_type || '').toUpperCase()] || String(entry?.asset_type || '');
        case 'symbol':
          return `${String(entry?.symbol || '').trim()} ${String(entry?.currency || 'VND').trim()}`.trim();
        case 'quantity':
          return toNumber(entry?.quantity) ?? 0;
        case 'price':
          return toNumber(entry?.price) ?? 0;
        case 'book_value':
          return getBookValue(entry);
        case 'current_value': {
          const currentValue = getCurrentValue(entry);
          if (currentValue === null) return null;
          return String(entry?.asset_type || '').toUpperCase() === 'DEBT' ? -currentValue : currentValue;
        }
        case 'change_percent':
          return getChangePercent(entry);
        default:
          return entry?.[field];
      }
    };

    const sortedEntries = computed(() => {
      const { field, direction } = sortState.value;
      const directionFactor = direction === 'asc' ? 1 : -1;

      return entries.value
        .map((entry, index) => ({ entry, index }))
        .sort((left, right) => {
          const valueCompare = compareNullable(
            getSortValue(left.entry, field),
            getSortValue(right.entry, field)
          );

          if (valueCompare !== 0) {
            return valueCompare * directionFactor;
          }

          return left.index - right.index;
        })
        .map(item => item.entry);
    });

    const convertToVnd = (amount, currency) => {
      const normalizedCurrency = currency || 'VND';
      if (normalizedCurrency === 'USD') {
        return usdToVndRate.value ? amount * usdToVndRate.value : 0;
      }
      return amount;
    };

    const totalAssetValueVnd = computed(() => {
      return entries.value.reduce((sum, entry) => {
        const assetType = String(entry?.asset_type || '').toUpperCase();
        const entryValue = getEntryDisplayValue(entry);
        const vndValue = convertToVnd(entryValue, entry.currency);
        // DEBT is subtracted from total
        return sum + (assetType === 'DEBT' ? -vndValue : vndValue);
      }, 0);
    });

    const allocationSegments = computed(() => {
      const totalsByType = {};

      for (const entry of entries.value) {
        const assetType = String(entry?.asset_type || 'OTHER').toUpperCase();
        const entryValue = getEntryDisplayValue(entry);
        const vndValue = convertToVnd(entryValue, entry.currency);
        if (!Number.isFinite(vndValue) || vndValue <= 0) continue;

        // DEBT is shown in allocation as a liability bucket, so it uses absolute value.
        const segmentValue = assetType === 'DEBT' ? Math.abs(vndValue) : vndValue;

        totalsByType[assetType] = (totalsByType[assetType] || 0) + segmentValue;
      }

      const rows = Object.entries(totalsByType)
        .map(([key, value]) => ({ key, value }))
        .sort((a, b) => b.value - a.value);

      const total = rows.reduce((sum, row) => sum + row.value, 0);
      if (total <= 0) return [];

      const typeColorMap = {
        DEBT: '#dc2626'
      };
      const palette = ['#2563eb', '#16a34a', '#d97706', '#06b6d4', '#7c3aed', '#db2777', '#475569', '#65a30d'];

      return rows.map((row, idx) => ({
        ...row,
        label: assetTypeLabels[row.key] || row.key,
        percent: (row.value / total) * 100,
        color: typeColorMap[row.key] || palette[idx % palette.length]
      }));
    });

    const totalAllocationValue = computed(() => {
      return allocationSegments.value.reduce((sum, segment) => sum + segment.value, 0);
    });

    const pieChartConicStyle = computed(() => {
      if (allocationSegments.value.length === 0) {
        return { background: 'conic-gradient(#e2e8f0 0 360deg)' };
      }

      let currentAngle = 0;
      const slices = allocationSegments.value.map(segment => {
        const start = currentAngle;
        const end = currentAngle + (segment.percent / 100) * 360;
        currentAngle = end;
        return `${segment.color} ${start.toFixed(2)}deg ${end.toFixed(2)}deg`;
      });

      return { background: `conic-gradient(${slices.join(', ')})` };
    });

    const getChangePercent = (entry) => {
      const assetType = String(entry?.asset_type || '').toUpperCase();
      if (assetType === 'DEBT' || assetType === 'CASH') return null;
      const totalCost = (toNumber(entry?.price) ?? 0) * (toNumber(entry?.quantity) ?? 0);
      if (totalCost === 0) return null;
      const currentVal = getCurrentValue(entry);
      if (currentVal === null) return null;
      return ((currentVal - totalCost) / totalCost) * 100;
    };

    const toggleSort = (field) => {
      if (sortState.value.field === field) {
        sortState.value.direction = sortState.value.direction === 'asc' ? 'desc' : 'asc';
        return;
      }

      sortState.value = {
        field,
        direction: field === 'entry_date' ? 'desc' : 'asc'
      };
    };

    const getSortIndicator = (field) => {
      if (sortState.value.field !== field) return '↕';
      return sortState.value.direction === 'asc' ? '↑' : '↓';
    };

    const getSortIndicatorClass = (field) => {
      return sortState.value.field === field ? 'is-active' : '';
    };

    const generateAiPrompt = async () => {
        const now = new Date().toLocaleString('vi-VN');
        let assetsList = '';
        entries.value.forEach(entry => {
            assetsList += `- ${entry.symbol} (${entry.asset_type}): ${entry.quantity} units @ ${formatCurrency(entry.price, entry.currency)}\n`;
        });
        
        let thesesText = '';
        try {
          const response = await fetch('/api/osint/theses');
          if (response.ok) {
            const theses = await response.json();
            if (theses && theses.length > 0) {
              thesesText = '\n--- TÌNH HÌNH VĨ MÔ HIỆN TẠI ---\n';
              theses.slice(0, 3).forEach(t => {
                thesesText += `Nhận định: ${t.thesis}\nCơ sở: ${t.supporting_evidence}\n\n`;
              });
            }
          }
        } catch (e) {
          console.error("Error fetching theses for prompt", e);
        }

        generatedPrompt.value = `Hôm nay là ${now}.
Dưới đây là Danh mục tài sản hiện tại của tôi:
${assetsList}
${thesesText}
Nhiệm vụ của bạn là: Tính ra giá trị hiện tại của toàn bộ tài sản này, sau đó kết hợp với tình hình vĩ mô để phân tích, đánh giá rủi ro và đưa ra lời khuyên hành động cụ thể cho từng tài sản mà tôi đang nắm giữ.`;
        aiResponse.value = '';
    };

    const askAI = async () => {
        if (!generatedPrompt.value || generatedPrompt.value.trim() === '') {
            return;
        }
        
        isAnalyzing.value = true;
        aiResponse.value = '';
        
        try {
            const response = await fetch('/api/chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    message: generatedPrompt.value,
                    use_groq: false
                })
            });

            if (!response.ok) {
                throw new Error('Không thể kết nối đến máy chủ AI');
            }

            const data = await response.json();

            if (data.gemini_failed) {
                const proceedWithGroq = confirm("Gemini API không khả dụng. Bạn có muốn chuyển sang sử dụng Groq (có tính phí) không?");
                if (proceedWithGroq) {
                    aiResponse.value = '🔄 Đang chuyển hướng yêu cầu sang Groq... Vui lòng đợi.';
                    const groqResponse = await fetch('/api/chat', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            message: generatedPrompt.value,
                            use_groq: true
                        })
                    });

                    if (!groqResponse.ok) {
                        throw new Error('Groq API cũng gặp sự cố');
                    }

                    const groqData = await groqResponse.json();
                    aiResponse.value = groqData.response || 'Không thể lấy phản hồi từ Groq.';
                } else {
                    aiResponse.value = '❌ Yêu cầu phân tích đã bị hủy.';
                }
            } else {
                aiResponse.value = data.response || 'Không thể lấy phản hồi từ AI.';
            }
        } catch (err) {
            console.error('Ask AI Error:', err);
            aiResponse.value = 'Rất tiếc, đã xảy ra lỗi trong quá trình xử lý yêu cầu AI. Vui lòng thử lại sau.';
        } finally {
            isAnalyzing.value = false;
        }
    };

    const getUserInfo = () => {
        const userInfoStr = localStorage.getItem('userInfo');
        if (!userInfoStr) return null;
        try {
            return JSON.parse(userInfoStr);
        } catch (e) {
            return null;
        }
    };

    const getHeaders = () => {
        const userInfo = getUserInfo();
        const token = localStorage.getItem('token');
        return {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            'X-User-ID': userInfo ? userInfo.id || userInfo.custodyCode : '' 
        };
    };

    const fetchEntries = async () => {
      isLoading.value = true;
      try {
        const userInfo = getUserInfo();
        if (!userInfo) {
            // router.push('/login'); // Do not force redirect here as it might be inside tab
            return;
        }

        const userId = userInfo.id || userInfo.custodyCode;
        const response = await fetch(`/journal?user_id=${userId}`, {
            headers: getHeaders()
        });
        
        if (response.status === 401) {
            router.push('/login');
            return;
        }

        if (response.ok) {
            const data = await response.json();
            entries.value = data || [];
        }
      } catch (error) {
        console.error('Error fetching journal:', error);
      } finally {
        isLoading.value = false;
      }
    };

    const fetchDealsProfitBySymbol = async () => {
      const accountNumber = String(props.accountNumber || '').trim();
      if (!accountNumber) {
        dealProfitBySymbol.value = {};
        return;
      }

      try {
        const token = localStorage.getItem('token');
        if (!token) return;
        const response = await fetch(`/dnse-deal-service/deals?accountNo=${encodeURIComponent(accountNumber)}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!response.ok) {
          return;
        }

        const data = await response.json();
        const nextProfitBySymbol = {};
        const dealsList = Array.isArray(data?.deals) ? data.deals : [];

        for (const item of dealsList) {
          const symbol = String(item?.symbol || '').toUpperCase().trim();
          if (!symbol) continue;

          const unrealizedProfit = toNumber(item?.unrealizedProfit) ?? 0;
          nextProfitBySymbol[symbol] = (nextProfitBySymbol[symbol] || 0) + unrealizedProfit;
        }

        dealProfitBySymbol.value = nextProfitBySymbol;
      } catch (error) {
        console.error('Error fetching deal profits:', error);
      }
    };

    const openModal = (mode, entry = null) => {
      modalMode.value = mode;
      if (mode === 'edit' && entry) {
        formData.id = entry.id;
        formData.asset_type = entry.asset_type;
        formData.symbol = entry.symbol;
        formData.quantity = entry.quantity;
        formData.price = entry.price;
        formData.currency = entry.currency || 'VND';
        if (entry.asset_type === 'REAL_ESTATE') {
          realEstateCategory.value = Object.prototype.hasOwnProperty.call(realEstateSymbolMap, entry.symbol)
            ? entry.symbol
            : 'NHA';
        }
        quantityDisplay.value = formatNumber(entry.quantity);
        priceDisplay.value = formatNumber(entry.price);
        // Format date for datetime-local input (YYYY-MM-DDTHH:mm)
        formData.entry_date = new Date(entry.entry_date).toISOString().slice(0, 16);
        formData.notes = entry.notes;

        if (entry.current_price !== undefined && entry.current_price !== null && entry.current_price !== 0) {
          useManualCurrentPrice.value = true;
          formData.current_price = entry.current_price;
          manualCurrentPriceDisplay.value = formatNumber(entry.current_price);
        } else {
          useManualCurrentPrice.value = false;
          formData.current_price = null;
          manualCurrentPriceDisplay.value = '0';
        }
      } else {
        // Reset form
        formData.id = null;
        formData.asset_type = 'STOCK';
        formData.symbol = '';
        realEstateCategory.value = 'NHA';
        formData.quantity = 0;
        formData.price = 0;
        formData.currency = 'VND';
        quantityDisplay.value = '0';
        priceDisplay.value = '0';
        formData.entry_date = new Date().toISOString().slice(0, 16);
        formData.notes = '';

        useManualCurrentPrice.value = false;
        formData.current_price = null;
        manualCurrentPriceDisplay.value = '0';
      }
      showModal.value = true;
    };

    const openAllocationModal = () => {
      showAllocationModal.value = true;
    };

    const closeAllocationModal = () => {
      showAllocationModal.value = false;
    };

    const closeModal = () => {
      showModal.value = false;
    };

    const submitForm = async () => {
        try {
            const userInfo = getUserInfo();
            const userId = userInfo ? (userInfo.id || userInfo.custodyCode) : '';
            
            // Backend expects user_id in query for all methods
            const url = `/journal?user_id=${userId}`;
            const method = modalMode.value === 'add' ? 'POST' : 'PUT';
            const body = { ...formData };
            body.entry_date = new Date(body.entry_date).toISOString();

            if (!useManualCurrentPrice.value) {
                body.current_price = null;
            }

            const response = await fetch(url, {
                method: method,
                headers: getHeaders(),
                body: JSON.stringify(body)
            });

            if (response.ok) {
                closeModal();
                fetchEntries();
            } else {
                const errorText = await response.text();
                notify({ type: 'error', title: 'Error', text: `Failed to save entry: ${response.status} ${response.statusText}\n${errorText}` });
            }
        } catch (error) {
            console.error("Error saving entry:", error);
            notify({ type: 'error', title: 'Error', text: 'An error occurred.' });
        }
    };

    const deleteEntry = async (id) => {
        if (!confirm("Are you sure you want to delete this entry?")) return;
        
        try {
            const userInfo = getUserInfo();
            const userId = userInfo ? (userInfo.id || userInfo.custodyCode) : '';
            
            const response = await fetch(`/journal?id=${id}&user_id=${userId}`, {
                method: 'DELETE',
                headers: getHeaders()
            });

            if (response.ok) {
                fetchEntries();
            } else {
                const errorText = await response.text();
                notify({ type: 'error', title: 'Error', text: `Failed to delete entry: ${response.status} ${response.statusText}\n${errorText}` });
            }
        } catch (error) {
            console.error("Error deleting entry:", error);
        }
    };

    const onQuantityInput = (e) => {
        const raw = e.target.value.replace(/[^0-9.]/g, '');
        quantityDisplay.value = raw;
        formData.quantity = parseFloat(raw) || 0;
    };
    const onQuantityBlur = () => { quantityDisplay.value = formatNumber(formData.quantity); };
    const onQuantityFocus = () => { quantityDisplay.value = formData.quantity === 0 ? '' : String(formData.quantity); };

    const onPriceInput = (e) => {
        const raw = e.target.value.replace(/[^0-9.]/g, '');
        priceDisplay.value = raw;
        formData.price = parseFloat(raw) || 0;
    };
    const onPriceBlur = () => { priceDisplay.value = formatNumber(formData.price); };
    const onPriceFocus = () => { priceDisplay.value = formData.price === 0 ? '' : String(formData.price); };

    const onManualCurrentPriceInput = (e) => {
        const raw = e.target.value.replace(/[^0-9.]/g, '');
        manualCurrentPriceDisplay.value = raw;
        formData.current_price = parseFloat(raw) || 0;
    };
    const onManualCurrentPriceBlur = () => { manualCurrentPriceDisplay.value = formatNumber(formData.current_price || 0); };
    const onManualCurrentPriceFocus = () => { manualCurrentPriceDisplay.value = (!formData.current_price || formData.current_price === 0) ? '' : String(formData.current_price); };

    const formatDate = (dateStr) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString() + ' ' + new Date(dateStr).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };

    const formatNumber = (value) => {
        return new Intl.NumberFormat('en-US').format(value);
    };

    const formatCurrency = (value, currency) => {
        const cur = currency || 'VND';
        if (cur === 'USD') {
            return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
        }
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
    };

    const getBadgeClass = (type) => {
        switch(type) {
            case 'GOLD': return 'bg-warning text-dark';
            case 'STOCK': return 'bg-success';
            case 'CRYPTO': return 'bg-info text-dark';
            case 'SILVER': return 'bg-secondary';
            case 'CASH': return 'bg-dark text-white';
            case 'DEBT': return 'bg-danger';
            default: return 'bg-primary';
        }
    };

    onMounted(() => {
      loadUsdVndRate();
      loadGoldPrices();
      fetchEntries();
    });

    watch(() => props.accountNumber, () => {
      fetchDealsProfitBySymbol();
    }, { immediate: true });

    return {
      entries,
      isLoading,
      showModal,
      showAllocationModal,
      modalMode,
      formData,
      realEstateCategory,
      quantityDisplay,
      priceDisplay,
      useManualCurrentPrice,
      manualCurrentPriceDisplay,
      onQuantityInput,
      onQuantityBlur,
      onQuantityFocus,
      onPriceInput,
      onPriceBlur,
      onPriceFocus,
      onManualCurrentPriceInput,
      onManualCurrentPriceBlur,
      onManualCurrentPriceFocus,
      openModal,
      openAllocationModal,
      closeAllocationModal,
      closeModal,
      submitForm,
      deleteEntry,
      formatDate,
      formatNumber,
      formatCurrency,
      getBadgeClass,
      getCurrentPrice,
      getCurrentValue,
      getChangePercent,
      sortedEntries,
      toggleSort,
      getSortIndicator,
      getSortIndicatorClass,
      totalAssetValueVnd,
      isCash,
      isDebt,
      isRealEstate,
      generatedPrompt,
      aiResponse,
      parsedAiResponse,
      generateAiPrompt,
      askAI,
      isAnalyzing,
      usdToVndRate,
      isRateLoading,
      hasUsdEntries,
      allocationSegments,
      totalAllocationValue,
      pieChartConicStyle
      ,goldLatestDate
    };
  }
};
</script>

<style scoped>
/* ── Layout ── */
.jnl { padding: 0; }

/* ── Header ── */
.jnl-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 1rem;
  padding: 1.25rem 1.5rem;
  background: linear-gradient(135deg, #1e293b 0%, #334155 100%);
  border-radius: 14px;
  margin-bottom: 1.5rem;
  color: #fff;
}
.jnl-total-label {
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: #94a3b8;
  margin-bottom: 2px;
}
.jnl-total-value {
  font-size: 1.75rem;
  font-weight: 800;
  letter-spacing: -0.5px;
  color: #34d399;
}
.jnl-total-value.jnl-negative { color: #f87171; }
.jnl-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-top: 0.4rem;
}
.jnl-meta-item {
  font-size: 0.75rem;
  color: #94a3b8;
}
.jnl-meta-warn { color: #fbbf24; }

.jnl-add-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 0.6rem 1.2rem;
  background: #3b82f6;
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
  flex-shrink: 0;
}
.jnl-add-btn:hover { background: #2563eb; box-shadow: 0 4px 14px rgba(59,130,246,0.35); }

.jnl-header-actions {
  display: flex;
  gap: 0.65rem;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.jnl-chart-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 0.6rem 1rem;
  background: rgba(148, 163, 184, 0.2);
  color: #e2e8f0;
  border: 1px solid rgba(148, 163, 184, 0.35);
  border-radius: 10px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}

.jnl-chart-btn:hover {
  background: rgba(148, 163, 184, 0.32);
  border-color: rgba(148, 163, 184, 0.5);
}

/* ── Loading + Empty ── */
.jnl-loading {
  text-align: center;
  padding: 4rem 1rem;
  color: #64748b;
}
.jnl-loading p { margin-top: 0.75rem; }
.jnl-empty {
  text-align: center;
  padding: 3.5rem 1rem;
  background: #fff;
  border: 1px dashed #e2e8f0;
  border-radius: 14px;
}
.jnl-empty-icon { font-size: 3rem; margin-bottom: 0.75rem; }
.jnl-empty h5 { color: #1e293b; font-weight: 700; }
.jnl-empty p { color: #64748b; margin-bottom: 1rem; }

/* ── Table ── */
.jnl-table-wrap {
  overflow-x: auto;
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 1px 8px rgba(0,0,0,0.04);
  margin-bottom: 1.5rem;
}
.jnl-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}
.jnl-table thead {
  background: #f8fafc;
  border-bottom: 2px solid #e2e8f0;
}
.jnl-table th {
  padding: 0.65rem 0.75rem;
  font-weight: 700;
  color: #475569;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  white-space: nowrap;
}
.jnl-sort-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: 100%;
  padding: 0;
  border: 0;
  background: transparent;
  color: inherit;
  font: inherit;
  text-transform: inherit;
  letter-spacing: inherit;
  cursor: pointer;
}
.jnl-sort-btn--right {
  justify-content: flex-end;
}
.jnl-sort-indicator {
  color: #94a3b8;
  font-size: 0.72rem;
  line-height: 1;
  transition: color 0.15s ease;
}
.jnl-sort-indicator.is-active,
.jnl-sort-btn:hover .jnl-sort-indicator {
  color: #2563eb;
}
.jnl-table td {
  padding: 0.6rem 0.75rem;
  vertical-align: middle;
  border-bottom: 1px solid #f1f5f9;
  color: #334155;
}
.jnl-table tbody tr:hover { background: #f8fafc; }
.jnl-table .text-end { text-align: right; }
.jnl-table .text-center { text-align: center; }
.jnl-table .fw-600 { font-weight: 600; }

.jnl-row-debt { background: #fef2f2 !important; }
.jnl-row-debt:hover { background: #fee2e2 !important; }

.jnl-cell-date {
  font-size: 0.78rem;
  color: #64748b;
  white-space: nowrap;
}
.jnl-cell-symbol {
  font-weight: 700;
  color: #1e293b;
  white-space: nowrap;
}
.jnl-currency-tag {
  display: inline-block;
  font-size: 0.65rem;
  font-weight: 600;
  padding: 1px 5px;
  border-radius: 4px;
  margin-left: 5px;
  background: #e0e7ff;
  color: #3730a3;
  vertical-align: middle;
}
.jnl-currency-tag--usd {
  background: #d1fae5;
  color: #065f46;
}
.jnl-cell-notes {
  max-width: 160px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: #64748b;
  font-size: 0.8rem;
}
.jnl-muted { color: #cbd5e1; }
.jnl-negative { color: #ef4444 !important; font-weight: 600; }

/* ── Note Info Icon + Tooltip ── */
.jnl-note-icon {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: 6px;
  color: #64748b;
  cursor: pointer;
  transition: all 0.15s;
}
.jnl-note-icon:hover {
  color: #3b82f6;
  background: #eff6ff;
}
.jnl-note-tooltip {
  display: none;
  position: absolute;
  bottom: calc(100% + 8px);
  left: 50%;
  transform: translateX(-50%);
  background: #1e293b;
  color: #f1f5f9;
  font-size: 0.78rem;
  font-weight: 500;
  line-height: 1.45;
  padding: 8px 12px;
  border-radius: 8px;
  white-space: normal;
  word-break: break-word;
  min-width: 160px;
  max-width: 280px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.18);
  z-index: 100;
  text-align: left;
}
.jnl-note-tooltip::after {
  content: '';
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%);
  border: 6px solid transparent;
  border-top-color: #1e293b;
}
.jnl-note-icon:hover .jnl-note-tooltip {
  display: block;
  animation: jnlTooltipIn 0.15s ease;
}
@keyframes jnlTooltipIn {
  from { opacity: 0; transform: translateX(-50%) translateY(4px); }
  to { opacity: 1; transform: translateX(-50%) translateY(0); }
}

/* ── Badge ── */
.jnl-badge {
  display: inline-block;
  font-size: 0.65rem;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 6px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  white-space: nowrap;
}
.jnl-badge--stock { background: #d1fae5; color: #065f46; }
.jnl-badge--crypto { background: #dbeafe; color: #1e40af; }
.jnl-badge--gold { background: #fef3c7; color: #92400e; }
.jnl-badge--silver { background: #f1f5f9; color: #475569; }
.jnl-badge--cash { background: #e2e8f0; color: #1e293b; }
.jnl-badge--real_estate { background: #ede9fe; color: #5b21b6; }
.jnl-badge--debt { background: #fee2e2; color: #991b1b; }
.jnl-badge--other { background: #e0e7ff; color: #3730a3; }

/* ── % Change ── */
.jnl-change {
  font-weight: 700;
  font-size: 0.82rem;
  padding: 2px 6px;
  border-radius: 4px;
}
.jnl-change--up { color: #059669; background: #d1fae5; }
.jnl-change--down { color: #dc2626; background: #fee2e2; }

/* ── Action buttons ── */
.jnl-cell-actions { white-space: nowrap; }
.jnl-icon-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  border-radius: 7px;
  border: 1px solid #e2e8f0;
  background: #fff;
  color: #64748b;
  cursor: pointer;
  transition: all 0.15s;
  margin: 0 2px;
}
.jnl-icon-btn:hover { border-color: #94a3b8; }
.jnl-icon-btn--edit:hover { color: #2563eb; border-color: #93c5fd; background: #eff6ff; }
.jnl-icon-btn--del:hover { color: #dc2626; border-color: #fca5a5; background: #fef2f2; }

/* ── AI Section ── */
.jnl-ai {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  padding: 1.5rem;
  margin-top: 1.5rem;
  transition: all 0.3s ease;
}
.jnl-ai--initial {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 3rem 2rem;
  background: linear-gradient(145deg, #ffffff, #f8fafc);
  border: 1px dashed #cbd5e1;
}
.jnl-ai--initial .jnl-ai-header {
  justify-content: center;
  margin-bottom: 1.25rem;
  flex-direction: column;
  gap: 12px;
}
.jnl-ai--initial .jnl-ai-header svg {
  width: 32px;
  height: 32px;
}
.jnl-ai--initial .jnl-ai-header h4 {
  font-size: 1.2rem;
  letter-spacing: -0.01em;
}
.jnl-ai--initial .jnl-ai-btn {
  padding: 0.65rem 1.75rem;
  font-size: 0.925rem;
  box-shadow: 0 4px 15px rgba(37, 99, 235, 0.12);
  transform: translateY(0);
}
.jnl-ai--initial .jnl-ai-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(37, 99, 235, 0.2);
}
.jnl-ai-header {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-bottom: 1.25rem;
}
.jnl-ai-header h4 {
  font-size: 1.05rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}
.jnl-ai-textarea {
  width: 100%;
  max-width: 900px;
  margin: 0 auto 0.75rem auto;
  display: block;
  border: 1.5px solid #e2e8f0;
  border-radius: 8px;
  padding: 0.75rem;
  font-size: 0.9rem;
  font-family: inherit;
  resize: vertical;
  background: #f8fafc;
  color: #1e293b;
  outline: none;
  transition: border-color 0.2s;
}
.jnl-ai-textarea:focus { border-color: #3b82f6; background: #fff; }
.jnl-ai-actions {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
}
.jnl-ai-btn {
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  border: none;
  cursor: pointer;
  transition: 0.2s;
  background: #eff6ff;
  color: #2563eb;
}
.jnl-ai-btn:hover { background: #dbeafe; }
.jnl-ai-btn--go { background: #059669; color: #fff; }
.jnl-ai-btn--go:hover { background: #047857; }
.jnl-ai-btn--cancel { background: #f1f5f9; color: #64748b; }
.jnl-ai-btn--cancel:hover { background: #e2e8f0; }
.jnl-ai-result {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 1rem;
  margin-top: 0.75rem;
  font-size: 0.9rem;
  color: #334155;
}

/* ── Modal ── */
.jnl-overlay {
  position: fixed;
  inset: 0;
  z-index: 1050;
  background: rgba(0,0,0,0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  animation: jnlFadeIn 0.15s ease;
}
@keyframes jnlFadeIn { from { opacity: 0; } to { opacity: 1; } }

.jnl-modal {
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.2);
  max-width: 480px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  animation: jnlSlideUp 0.2s ease;
}
@keyframes jnlSlideUp { from { transform: translateY(16px); opacity: 0; } to { transform: none; opacity: 1; } }

.jnl-modal--allocation {
  max-width: 560px;
}

.jnl-modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem 0.75rem;
  border-bottom: 1px solid #f1f5f9;
}
.jnl-modal-header h3 {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}
.jnl-modal-close {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: none;
  background: #f1f5f9;
  color: #64748b;
  font-size: 1.1rem;
  cursor: pointer;
  transition: 0.15s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.jnl-modal-close:hover { background: #e2e8f0; color: #1e293b; }

/* ── Form ── */
.jnl-form { padding: 1.25rem 1.5rem 1.5rem; }
.jnl-form-group { margin-bottom: 1rem; }
.jnl-form-group label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #475569;
  margin-bottom: 0.35rem;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}
.jnl-form-group input:not([type="checkbox"]),
.jnl-form-group select,
.jnl-form-group textarea {
  width: 100%;
  padding: 0.6rem 0.85rem;
  border: 1.5px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.9rem;
  font-family: inherit;
  color: #1e293b;
  background: #f8fafc;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}
.jnl-form-group input:not([type="checkbox"]):focus,
.jnl-form-group select:focus,
.jnl-form-group textarea:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59,130,246,0.1);
  background: #fff;
}
.jnl-form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}
.jnl-submit-btn {
  width: 100%;
  padding: 0.7rem;
  background: #3b82f6;
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: 0.2s;
  margin-top: 0.5rem;
}
.jnl-submit-btn:hover { background: #2563eb; box-shadow: 0 4px 14px rgba(59,130,246,0.3); }

.jnl-allocation-body {
  padding: 1.1rem 1.5rem 1.5rem;
}

.jnl-allocation-empty {
  text-align: center;
  color: #64748b;
  padding: 1.25rem 0;
}

.jnl-pie-wrap {
  display: flex;
  justify-content: center;
  margin: 0.5rem 0 1rem;
}

.jnl-pie-chart {
  width: 220px;
  height: 220px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: inset 0 0 0 1px rgba(15, 23, 42, 0.06);
}

.jnl-pie-center {
  width: 110px;
  height: 110px;
  border-radius: 50%;
  background: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  box-shadow: 0 2px 12px rgba(15, 23, 42, 0.08);
  color: #334155;
}

.jnl-pie-center strong {
  font-size: 1.3rem;
  line-height: 1;
}

.jnl-pie-center span {
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 0.2rem;
}

.jnl-allocation-total {
  text-align: center;
  font-size: 0.88rem;
  color: #0f172a;
  font-weight: 600;
  margin-bottom: 0.9rem;
}

.jnl-allocation-list {
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  overflow: hidden;
}

.jnl-allocation-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.6rem;
  padding: 0.6rem 0.8rem;
  border-bottom: 1px solid #f1f5f9;
  font-size: 0.84rem;
}

.jnl-allocation-item:last-child {
  border-bottom: none;
}

.jnl-allocation-label {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  font-weight: 600;
  color: #1e293b;
}

.jnl-allocation-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
}

.jnl-allocation-value {
  text-align: right;
  color: #475569;
}

/* ── Responsive ── */
@media (max-width: 768px) {
  .jnl-header { flex-direction: column; }
  .jnl-header-actions { width: 100%; justify-content: stretch; }
  .jnl-chart-btn, .jnl-add-btn { width: 100%; justify-content: center; }
  .jnl-total-value { font-size: 1.35rem; }
  .jnl-form-row { grid-template-columns: 1fr; }
  .jnl-table { font-size: 0.78rem; }
  .jnl-table th, .jnl-table td { padding: 0.5rem 0.5rem; }
  .jnl-allocation-item {
    flex-direction: column;
    align-items: flex-start;
  }
  .jnl-allocation-value {
    text-align: left;
  }
}

/* AI Content Premium Styles */
.jnl-ai-content {
  color: #334155;
  line-height: 1.65;
  font-size: 0.925rem;
}

/* Beautiful custom headers with left borders */
.jnl-ai-content :deep(.ai-header-h3) {
  font-size: 1.2rem;
  color: #0f172a;
  font-weight: 700;
  margin-top: 1.5rem;
  margin-bottom: 0.75rem;
  padding-left: 0.75rem;
  border-left: 4px solid #3b82f6;
  letter-spacing: -0.02em;
}

.jnl-ai-content :deep(.ai-header-h4) {
  font-size: 1.1rem;
  color: #1e293b;
  font-weight: 600;
  margin-top: 1.25rem;
  margin-bottom: 0.5rem;
  padding-left: 0.5rem;
  border-left: 3px solid #10b981;
}

.jnl-ai-content :deep(.ai-header-h5) {
  font-size: 1.0rem;
  color: #334155;
  font-weight: 600;
  margin-top: 1rem;
  margin-bottom: 0.5rem;
  padding-left: 0.5rem;
  border-left: 3px solid #f59e0b;
}

.jnl-ai-content :deep(.ai-header-h6) {
  font-size: 0.95rem;
  color: #475569;
  font-weight: 600;
  margin-top: 0.85rem;
  margin-bottom: 0.4rem;
  padding-left: 0.4rem;
  border-left: 2.5px solid #8b5cf6;
}

/* Dynamic thin-gradient divider */
.jnl-ai-content :deep(.ai-hr) {
  border: 0;
  height: 1px;
  background: linear-gradient(to right, rgba(226, 232, 240, 0), rgba(226, 232, 240, 1), rgba(226, 232, 240, 0));
  margin: 1.5rem 0;
}

/* Lists styling using modern emerald dots */
.jnl-ai-content :deep(.ai-list-item) {
  list-style: none;
  position: relative;
  padding-left: 1.25rem;
  margin-bottom: 0.5rem;
}

.jnl-ai-content :deep(.ai-list-item::before) {
  content: '';
  position: absolute;
  left: 0.25rem;
  top: 0.55rem;
  width: 6px;
  height: 6px;
  background-color: #10b981;
  border-radius: 50%;
  box-shadow: 0 0 8px rgba(16, 185, 129, 0.6);
}

/* Premium styled Table */
.jnl-ai-content :deep(.table-responsive) {
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03);
  border: 1px solid #e2e8f0;
  margin-top: 1rem;
  margin-bottom: 1rem;
}

.jnl-ai-content :deep(.custom-ai-table) {
  width: 100%;
  margin-bottom: 0;
  border-collapse: collapse;
  font-size: 0.85rem;
  background-color: #ffffff;
}

.jnl-ai-content :deep(.custom-ai-table th) {
  background: #1e293b !important;
  color: #ffffff !important;
  font-weight: 600;
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 0.05em;
  padding: 10px 14px;
  border: none;
  text-align: left;
}

.jnl-ai-content :deep(.custom-ai-table td) {
  padding: 10px 14px;
  color: #475569;
  border-bottom: 1px solid #f1f5f9;
  transition: background-color 0.2s ease;
  text-align: left;
}

.jnl-ai-content :deep(.custom-ai-table tr:last-child td) {
  border-bottom: none;
}

.jnl-ai-content :deep(.custom-ai-table tr:nth-child(even) td) {
  background-color: #f8fafc;
}

.jnl-ai-content :deep(.custom-ai-table tr:hover td) {
  background-color: rgba(59, 130, 246, 0.04) !important;
  color: #0f172a;
}

/* Inline code and blocks styling */
.jnl-ai-content :deep(.custom-code-inline) {
  background-color: #f1f5f9 !important;
  color: #ef4444 !important;
  padding: 2px 6px !important;
  border-radius: 4px !important;
  font-family: SFMono-Regular, Consolas, monospace !important;
  font-size: 0.85em !important;
}

.jnl-ai-content :deep(.custom-code-block) {
  background-color: #0f172a !important;
  color: #e2e8f0 !important;
  padding: 14px 18px !important;
  border-radius: 10px !important;
  font-family: SFMono-Regular, Consolas, monospace !important;
  font-size: 0.85rem !important;
  margin: 1rem 0 !important;
  overflow-x: auto !important;
  border: 1px solid #1e293b;
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.3);
}

.jnl-ai-content :deep(strong) {
  color: #0f172a;
  font-weight: 600;
}
</style>
