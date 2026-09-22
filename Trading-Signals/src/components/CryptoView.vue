<template>
  <div id="app" class="d-flex flex-column min-vh-100">
    <NavBar />
    <notifications />
    <div class="stk-page flex-grow-1">
      <div class="stk-container">

        <!-- Tab Navigation -->
        <div class="stk-tabs">
          <button class="stk-tab" :class="{ 'stk-tab--active': activeTab === 'coins' }" @click="activeTab = 'coins'">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3v18h18"/><path d="m19 9-5 5-4-4-3 3"/></svg>
            Potential Coins
          </button>
          <button class="stk-tab" :class="{ 'stk-tab--active': activeTab === 'rrg' }" @click="activeTab = 'rrg'">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10"/><path d="M2 12h20"/></svg>
            RRG Chart
          </button>
        </div>

        <!-- ==================== COINS TAB ==================== -->
        <div v-show="activeTab === 'coins'">
          <div class="stk-panel">
            <!-- Header -->
            <div class="stk-header">
              <div class="stk-header__icon">
                <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3v18h18"/><path d="m19 9-5 5-4-4-3 3"/></svg>
              </div>
              <div>
                <h2 class="stk-header__title">Crypto Scanner</h2>
                <p class="stk-header__sub">Discover potential coins near highs &amp; with EMA9 &ge; EMA21</p>
              </div>
            </div>

            <!-- Coin Input -->
            <div class="stk-section">
              <label class="stk-label">Search coin symbol</label>
              <div style="display:flex; gap:8px;">
                <input
                  type="text"
                  class="stk-input"
                  v-model="coinInputText"
                  @keydown.enter="updateSelectedCoin"
                  @input="coinInputText = $event.target.value.toUpperCase()"
                  placeholder="Enter coin (e.g. BTCUSDT) and press Enter"
                />
                <button class="stk-btn stk-btn--primary" @click="updateSelectedCoin" :disabled="!coinInputText || !coinInputText.trim()">
                  View
                </button>
              </div>
            </div>
          </div>

          <!-- Chart (sticky) -->
          <div ref="chartRef" class="stk-sticky-chart">
            <div class="stk-chart-wrap">
              <TradingViewChart :coin="selectedCoin" :height="380" />
            </div>
            <!-- Price Alert Toggle -->
            <div class="stk-alert-toggle">
              <button class="stk-alert-toggle__btn" @click="showPriceAlert = !showPriceAlert">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>
                Price Alert
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" :class="{ 'stk-chevron--open': showPriceAlert }"><polyline points="6 9 12 15 18 9"/></svg>
              </button>
              <div v-show="showPriceAlert" class="stk-alert-content">
                <PriceAlertWidget :symbol="selectedCoin" assetType="crypto" />
              </div>
            </div>
          </div>

          <!-- Potential Coins Section -->
          <div class="stk-panel">
            <div class="stk-section stk-section--potential">
              <div class="stk-section-head">
                <h3 class="stk-section-head__title">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                  Potential Coins
                </h3>
                <span v-if="potentialCoins.latest_updated" class="stk-updated">
                  Updated: {{ formatDate(potentialCoins.latest_updated) }}
                </span>
              </div>

              <!-- Filters -->
              <div class="stk-filters" v-if="potentialCoins.data && potentialCoins.data.length > 0">
                <div class="stk-filter-item">
                  <input type="text" v-model="filterText" placeholder="Filter coins..." class="stk-input" />
                </div>
                <div class="stk-filter-item">
                  <select v-model="selectedSignalType" class="stk-input">
                    <option value="">All Signals</option>
                    <option value="near_52w_ath">Near 52W High</option>
                    <option value="near_ath">Near ATH</option>
                    <option value="ema9_above_ema21">Uptrend</option>
                  </select>
                </div>
              </div>

              <!-- Coins Table -->
              <div ref="tableWrapRef" class="stk-table-wrap stk-table-wrap--scroll" v-if="filteredPotentialCoins.length > 0">
                <table class="stk-table">
                  <thead>
                    <tr>
                      <th class="stk-th stk-th--chk"></th>
                      <th class="stk-th">Coin</th>
                      <th class="stk-th stk-th--right">Market Cap</th>
                      <th class="stk-th stk-th--right">Score Diff</th>
                      <th class="stk-th stk-th--center">Signal</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      v-for="coin in filteredPotentialCoins"
                      :key="getRowKey(coin)"
                      class="stk-row"
                      :class="{ 'stk-row--active': isRowActive(coin) }"
                      @click="selectCoin(coin)"
                    >
                      <td class="stk-td stk-td--chk">
                        <input type="checkbox" class="stk-checkbox" @click.stop="toggleStock(coin.crypto)" />
                      </td>
                      <td class="stk-td stk-td--symbol" :title="`View ${coin.crypto} chart`">{{ coin.crypto }}</td>
                      <td class="stk-td stk-td--right" style="font-weight: 600; color: #475569;">
                        {{ formatMarketCap(coin.market_cap) }}
                      </td>
                      <td class="stk-td stk-td--right" :style="{ color: coin.score_diff >= 0 ? '#10b981' : '#ef4444', fontWeight: 'bold' }">
                        {{ coin.score_diff >= 0 ? '+' : '' }}{{ coin.score_diff.toFixed(2) }}%
                      </td>
                      <td class="stk-td stk-td--center">
                        <template v-for="(label, index) in coin.signal_labels" :key="`${coin.crypto}-${coin.signal_types[index]}`">
                          <span class="stk-signal" :class="'stk-signal--' + coin.signal_types[index]" style="margin: 2px;">{{ label }}</span>
                        </template>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

                 <div style="display: flex; gap: 10px; align-items: center; justify-content: center;">
                  <button
                    v-if="!loadingPotentialCoins && !startScanning"
                    @click="startScanningCoins"
                    class="stk-btn stk-btn--primary stk-btn--scan"
                  >
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                    Start Scanning
                  </button>
                  <button
                    class="stk-btn stk-btn--outline"
                    @click="runSSHScript('crypto_potential')"
                    :disabled="isRunningPotentialScript"
                    style="border-color: #3b82f6; color: #3b82f6;"
                  >
                    <span v-if="isRunningPotentialScript" class="stk-spinner" style="width: 14px; height: 14px; margin: 0; display: inline-block; border-width: 2px;"></span>
                    <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                    Run script
                  </button>
                  <div v-if="loadingPotentialCoins" class="stk-loading" style="padding: 0;">
                    <div class="stk-spinner" style="width: 24px; height: 24px;"></div>
                  </div>
                </div>
              <p v-if="message" class="stk-message">{{ message }}</p>
            </div>
          </div>
        </div>

        <!-- ==================== RRG TAB ==================== -->
        <div class="stk-panel" v-show="activeTab === 'rrg'">
          <div class="stk-header">
            <div class="stk-header__icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10"/><path d="M2 12h20"/></svg>
            </div>
            <div>
              <h2 class="stk-header__title">Crypto RRG Chart</h2>
              <p class="stk-header__sub">Relative Rotation Graph for crypto market</p>
            </div>
          </div>
          <div class="stk-rrg-wrap">
            <div class="stk-rrg-actions" style="margin-bottom: 20px; display: flex; justify-content: center;">
              <button
                class="stk-btn stk-btn--primary"
                @click="runSSHScript('crypto_rrg')"
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
            <img :src="cryptoRRGUrl" class="stk-rrg-img" alt="Crypto RRG Chart" />
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
import { ref, onMounted, onUnmounted, computed, nextTick } from 'vue';
import { useNotification } from "@kyvg/vue3-notification";
import TradingViewChart from './TradingViewChart.vue';
import PriceAlertWidget from './PriceAlertWidget.vue';

const { notify } = useNotification();

export default {
  name: 'CryptoView',
  components: { NavBar, AppFooter, TradingViewChart, PriceAlertWidget },
  setup() {
    const activeTab = ref('coins');
    const selectedCoin = ref('BTCUSDT');
    const selectedRowKey = ref('');
    const coinInputText = ref('');
    const potentialCoins = ref({});
    const loadingPotentialCoins = ref(false);
    const startScanning = ref(false);
    const filterText = ref('');
    const selectedSignalType = ref('');
    const message = ref('');
    const showPriceAlert = ref(false);
    const chartRef = ref(null);
    const tableWrapRef = ref(null);

    // ---------- DATA FETCHING ----------
    onMounted(async () => {
      window.addEventListener('keydown', handleArrowNavigation);
      notify({ type: "info", title: "Welcome!", text: "The application has loaded successfully." });
    });

    onUnmounted(() => {
      window.removeEventListener('keydown', handleArrowNavigation);
    });

    const fetchPotentialCoins = async () => {
      loadingPotentialCoins.value = true;
      message.value = '';
      try {
        const response = await fetch('/getPotentialCoins');
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const data = await response.json();
        potentialCoins.value = data;
        if (!data.data || data.data.length === 0) {
          message.value = 'No potential coins available at the moment.';
        } else {
          message.value = `Found ${data.data.length} potential coins.`;
        }
      } catch (error) {
        console.error('Error fetching potential coins:', error);
        potentialCoins.value = { data: [] };
        message.value = 'Failed to load potential coins. Please try again later.';
      } finally {
        loadingPotentialCoins.value = false;
      }
    };

    const startScanningCoins = () => {
      startScanning.value = true;
      fetchPotentialCoins();
    };

    // SSH script execution states
    const isRunningPotentialScript = ref(false);
    const isRunningRrgScript = ref(false);
    const cryptoRRGKey = ref(Date.now());
    const cryptoRRGUrl = computed(() => `/cryto_rrgchart?t=${cryptoRRGKey.value}`);

    const runSSHScript = async (scriptType) => {
      const isRrg = scriptType === 'crypto_rrg';
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
          notify({
            type: 'success',
            title: 'Success',
            text: isRrg ? 'Crypto RRG Chart has been updated successfully!' : 'Crypto scanner script executed successfully!',
          });
          if (isRrg) {
            cryptoRRGKey.value = Date.now();
          } else {
            fetchPotentialCoins();
          }
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
        if (isRrg) {
          isRunningRrgScript.value = false;
        } else {
          isRunningPotentialScript.value = false;
        }
      }
    };

    // ---------- FILTERING ----------
    const getSignalLabel = (coin) => {
      const labelMap = {
        near_52w_ath: 'Near 52W High',
        near_ath: 'Near ATH',
        ema9_above_ema21: 'Uptrend',
      };
      return coin?.signal_label || labelMap[coin?.signal_type] || coin?.signal_type || 'N/A';
    };

    const groupedPotentialCoins = computed(() => {
      const grouped = new Map();
      const items = potentialCoins.value.data || [];

      for (const coin of items) {
        if (!coin || !coin.crypto) {
          continue;
        }

        const existing = grouped.get(coin.crypto);
        const label = getSignalLabel(coin);
        const signalType = coin.signal_type || '';

        if (!existing) {
          grouped.set(coin.crypto, {
            crypto: coin.crypto,
            signal_types: signalType ? [signalType] : [],
            signal_labels: label ? [label] : [],
            market_cap: coin.market_cap || 0,
            score_diff: coin.score_diff || 0,
          });
        } else {
          if (signalType && !existing.signal_types.includes(signalType)) {
            existing.signal_types.push(signalType);
          }
          if (label && !existing.signal_labels.includes(label)) {
            existing.signal_labels.push(label);
          }
          if (coin.score_diff !== undefined) {
            existing.score_diff = coin.score_diff;
          }
        }
      }

      return Array.from(grouped.values());
    });

    const filteredPotentialCoins = computed(() => {
      const filtered = (groupedPotentialCoins.value || []).filter(coin => {
        const matchesText = !filterText.value ||
          coin.crypto.toLowerCase().includes(filterText.value.toLowerCase());
        const matchesSignal = !selectedSignalType.value ||
          coin.signal_types.includes(selectedSignalType.value);
        return matchesText && matchesSignal;
      });

      return filtered.sort((a, b) => {
        if (a.market_cap !== b.market_cap) {
          return b.market_cap - a.market_cap;
        }
        const signalLenA = a.signal_types ? a.signal_types.length : 0;
        const signalLenB = b.signal_types ? b.signal_types.length : 0;
        if (signalLenA !== signalLenB) {
          return signalLenB - signalLenA;
        }
        return a.crypto.localeCompare(b.crypto);
      });
    });

    // ---------- ROW SELECTION & NAVIGATION ----------
    const getRowKey = (coin) => coin.crypto;

    const isRowActive = (coin) => {
      if (selectedRowKey.value) return selectedRowKey.value === getRowKey(coin);
      return selectedCoin.value === coin.crypto;
    };

    const selectCoin = (coin, shouldScroll = true) => {
      selectedCoin.value = coin.crypto;
      selectedRowKey.value = getRowKey(coin);

      if (!shouldScroll) return;
      setTimeout(() => {
        if (chartRef.value) {
          const y = chartRef.value.getBoundingClientRect().top + window.scrollY - 120;
          window.scrollTo({ top: y, behavior: 'smooth' });
        }
      }, 100);
    };

    const scrollActiveRowIntoView = async () => {
      await nextTick();
      const container = tableWrapRef.value;
      if (!container) return;
      const activeRow = container.querySelector('.stk-row--active');
      if (activeRow) activeRow.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    };

    const moveSelection = (direction) => {
      const rows = filteredPotentialCoins.value;
      if (!rows.length) return;

      let currentIndex = rows.findIndex(r => getRowKey(r) === selectedRowKey.value);
      if (currentIndex === -1) {
        currentIndex = rows.findIndex(r => r.crypto === selectedCoin.value);
      }
      const baseIndex = currentIndex === -1 ? (direction > 0 ? -1 : 0) : currentIndex;
      const nextIndex = (baseIndex + direction + rows.length) % rows.length;
      const nextRow = rows[nextIndex];

      if (nextRow) {
        selectCoin(nextRow, false);
        scrollActiveRowIntoView();
      }
    };

    const isTypingTarget = (target) => {
      if (!target) return false;
      const tag = (target.tagName || '').toLowerCase();
      return tag === 'input' || tag === 'textarea' || tag === 'select' || target.isContentEditable;
    };

    const handleArrowNavigation = (event) => {
      if (event.key !== 'ArrowDown' && event.key !== 'ArrowUp') return;
      if (isTypingTarget(event.target)) return;
      if (activeTab.value !== 'coins') return;

      const direction = event.key === 'ArrowDown' ? 1 : -1;
      moveSelection(direction);
      event.preventDefault();
    };

    // ---------- MISC ----------
    const updateSelectedCoin = () => {
      const input = coinInputText.value.trim().toUpperCase();
      if (input) {
        selectedCoin.value = input;
        selectedRowKey.value = '';
        notify({ type: "success", title: "Chart Updated", text: `Switched to ${input}` });
      }
    };

    const toggleStock = () => { /* checkbox placeholder */ };

    const exportCSV = () => {
      const data = filteredPotentialCoins.value;
      if (!data || data.length === 0) return;
      const rows = data.map(c => `${c.crypto},"${(c.signal_types || []).join('; ')}","${(c.signal_labels || []).join('; ')}"`);
      const csvContent = "data:text/csv;charset=utf-8," + "coin,signal_types,signal_labels\n" + rows.join("\n");
      const link = document.createElement("a");
      link.setAttribute("href", encodeURI(csvContent));
      link.setAttribute("download", "potential_coins.csv");
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    };

    const formatMarketCap = (val) => {
      if (!val || val === 0) return 'N/A';
      if (val >= 1e12) {
        return `$${(val / 1e12).toFixed(2)}T`;
      }
      if (val >= 1e9) {
        return `$${(val / 1e9).toFixed(2)}B`;
      }
      if (val >= 1e6) {
        return `$${(val / 1e6).toFixed(2)}M`;
      }
      return `$${val.toLocaleString()}`;
    };

    const formatDate = (dateString) => {
      const d = new Date(dateString);
      const pad = (n) => String(n).padStart(2, '0');
      return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
    };

    return {
      activeTab, selectedCoin, coinInputText, updateSelectedCoin,
      potentialCoins, filteredPotentialCoins, loadingPotentialCoins,
      startScanning, startScanningCoins, filterText, selectedSignalType,
      message, showPriceAlert, chartRef, tableWrapRef,
      getSignalLabel, getRowKey, isRowActive, selectCoin,
      toggleStock, exportCSV, formatDate, formatMarketCap,
      isRunningPotentialScript, isRunningRrgScript, cryptoRRGUrl, runSSHScript
    };
  }
}
</script>

<style scoped>
/* ============================== */
/*  CRYPTO PAGE – Matches Stock   */
/* ============================== */

.stk-page { background: #ffffff; padding: 20px 0 40px; }
.stk-container { max-width: 1280px; margin: 0 auto; padding: 0 24px; }

/* ---------- TABS ---------- */
.stk-tabs { display: flex; gap: 6px; margin-bottom: 20px; overflow-x: auto; scrollbar-width: none; }
.stk-tabs::-webkit-scrollbar { display: none; }
.stk-tab {
  display: inline-flex; align-items: center; gap: 6px; padding: 10px 20px;
  border: 1px solid rgba(0, 0, 0, 0.06); border-radius: 10px; font-size: 0.88rem; font-weight: 600;
  color: #475569; background: rgba(0, 0, 0, 0.02); cursor: pointer; transition: all 0.2s ease;
  white-space: nowrap; box-shadow: 0 1px 3px rgba(0,0,0,0.03);
}
.stk-tab:hover { color: #0f172a; background: rgba(0, 0, 0, 0.05); }
.stk-tab--active { color: #fff !important; background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%) !important; box-shadow: 0 4px 12px rgba(59, 130, 246, 0.2) !important; border-color: rgba(0, 0, 0, 0.05); }

/* ---------- PANEL ---------- */
.stk-panel { background: #ffffff; border: 1px solid rgba(0, 0, 0, 0.06); border-radius: 16px; box-shadow: 0 8px 32px rgba(0,0,0,0.04); overflow: hidden; margin-bottom: 20px; }

/* ---------- HEADER ---------- */
.stk-header { display: flex; align-items: center; gap: 14px; padding: 22px 24px; background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); color: #0f172a; border-bottom: 1px solid rgba(0, 0, 0, 0.06); }
.stk-header__icon { width: 44px; height: 44px; border-radius: 12px; background: #ffffff; border: 1px solid rgba(0, 0, 0, 0.08); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.stk-header__title { font-size: 1.2rem; font-weight: 700; margin: 0; line-height: 1.3; font-family: 'Outfit', sans-serif; color: #0f172a; }
.stk-header__sub { font-size: 0.82rem; color: #475569; margin: 2px 0 0; }

/* ---------- SECTIONS ---------- */
.stk-section { padding: 20px 24px; }
.stk-section--potential { border-top: 1px solid rgba(0, 0, 0, 0.06); }
.stk-label { display: block; font-size: 0.82rem; font-weight: 600; color: #475569; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
.stk-section-head { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 8px; margin-bottom: 14px; }
.stk-section-head__title { display: inline-flex; align-items: center; gap: 8px; font-size: 1rem; font-weight: 700; color: #0f172a; margin: 0; font-family: 'Outfit', sans-serif; }
.stk-updated { font-size: 0.75rem; color: #64748b; font-weight: 500; }

/* ---------- CHART (sticky) ---------- */
.stk-sticky-chart { position: sticky; top: 60px; z-index: 20; background: #ffffff; padding: 12px 0; margin-bottom: 12px; }
.stk-chart-wrap { position: relative; border-radius: 12px; overflow: hidden; border: 1px solid rgba(0, 0, 0, 0.06); background: #ffffff; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03); }

/* ---------- PRICE ALERT ---------- */
.stk-alert-toggle { margin-top: 10px; }
.stk-alert-toggle__btn {
  display: inline-flex; align-items: center; gap: 6px; padding: 7px 14px;
  border: 1px solid rgba(0, 0, 0, 0.08); border-radius: 8px; background: rgba(0, 0, 0, 0.02);
  color: #475569; font-size: 0.82rem; font-weight: 600; cursor: pointer; transition: all 0.15s;
}
.stk-alert-toggle__btn:hover { background: rgba(59,130,246,0.08); border-color: rgba(59,130,246,0.2); color: #2563eb; }
.stk-alert-toggle__btn svg:last-child { transition: transform 0.2s ease; }
.stk-chevron--open { transform: rotate(180deg); }
.stk-alert-content { margin-top: 8px; }

/* ---------- FILTERS ---------- */
.stk-filters { display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 14px; }
.stk-filter-item { flex: 1 1 200px; }
.stk-input {
  width: 100%; padding: 9px 14px; border: 1px solid rgba(0, 0, 0, 0.1); border-radius: 8px;
  font-size: 0.85rem; color: #0f172a; background: #ffffff; transition: border-color 0.2s, box-shadow 0.2s; outline: none;
}
.stk-input:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.15); }

/* ---------- TABLE ---------- */
.stk-table-wrap { border-radius: 10px; border: 1px solid rgba(0, 0, 0, 0.06); overflow: hidden; background: #ffffff; }
.stk-table-wrap--scroll { max-height: 480px; overflow-y: auto; }
.stk-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
.stk-th {
  padding: 10px 14px; text-align: left; font-size: 0.72rem; font-weight: 700;
  text-transform: uppercase; letter-spacing: 0.5px; color: #475569; background: #f1f5f9;
  border-bottom: 2px solid #e2e8f0; position: sticky; top: 0; z-index: 2;
}
.stk-th--center { text-align: center; }
.stk-th--right { text-align: right; }
.stk-th--chk { width: 36px; text-align: center; }

.stk-row { cursor: pointer; transition: background 0.15s ease; }
.stk-row:hover { background: #f8fafc; }
.stk-row--active { background: rgba(59, 130, 246, 0.05) !important; }
.stk-td { padding: 10px 14px; border-bottom: 1px solid rgba(0, 0, 0, 0.04); vertical-align: middle; color: #334155; }
.stk-td--chk { width: 36px; text-align: center; }
.stk-td--center { text-align: center; }
.stk-td--right { text-align: right; }
.stk-td--symbol { font-weight: 700; color: #2563eb; }

.stk-checkbox { width: 16px; height: 16px; accent-color: #3b82f6; cursor: pointer; }

/* ---------- SIGNALS ---------- */
.stk-signal { display: inline-block; padding: 3px 10px; border-radius: 999px; font-size: 0.75rem; font-weight: 600; white-space: nowrap; }
.stk-signal--near_52w_ath { background: rgba(59, 130, 246, 0.08); color: #2563eb; border: 1px solid rgba(59, 130, 246, 0.2); }
.stk-signal--near_ath { background: rgba(245, 158, 11, 0.08); color: #d97706; border: 1px solid rgba(245, 158, 11, 0.2); }
.stk-signal--ema9_above_ema21 { background: rgba(16, 185, 129, 0.08); color: #059669; border: 1px solid rgba(16, 185, 129, 0.2); }

/* ---------- BUTTONS ---------- */
.stk-actions { padding: 16px 0 0; display: flex; align-items: center; justify-content: center; flex-wrap: wrap; gap: 10px; }
.stk-actions__group { display: flex; gap: 10px; flex-wrap: wrap; justify-content: center; }
.stk-btn {
  display: inline-flex; align-items: center; gap: 6px; padding: 9px 18px;
  border: none; border-radius: 8px; font-size: 0.84rem; font-weight: 600;
  cursor: pointer; transition: all 0.2s ease; white-space: nowrap;
}
.stk-btn:disabled { opacity: 0.45; cursor: not-allowed; }
.stk-btn--primary { background: linear-gradient(135deg, #3b82f6, #2563eb); color: #fff; box-shadow: 0 2px 8px rgba(59,130,246,0.3); }
.stk-btn--primary:hover:not(:disabled) { background: linear-gradient(135deg, #2563eb, #1d4ed8); box-shadow: 0 4px 14px rgba(37,99,235,0.4); transform: translateY(-1px); }
.stk-btn--outline { background: rgba(255,255,255,0.03); color: #cbd5e1; border: 1px solid rgba(255,255,255,0.08); }
.stk-btn--outline:hover:not(:disabled) { background: rgba(255,255,255,0.08); border-color: rgba(255,255,255,0.15); }
.stk-btn--scan { min-width: 180px; justify-content: center; }

/* ---------- LOADING ---------- */
.stk-loading { display: flex; justify-content: center; padding: 20px 0; }
.stk-spinner { width: 32px; height: 32px; border: 3px solid rgba(255,255,255,0.1); border-top-color: #3b82f6; border-radius: 50%; animation: stk-spin 0.7s linear infinite; }
@keyframes stk-spin { to { transform: rotate(360deg); } }

.stk-message { text-align: center; font-size: 0.85rem; color: #64748b; padding: 10px 0; margin: 0; }

/* ---------- RRG ---------- */
.stk-rrg-wrap { padding: 24px; text-align: center; }
.stk-rrg-img { max-width: 100%; height: auto; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.5); border: 1px solid rgba(255, 255, 255, 0.08); }

/* ---------- RESPONSIVE ---------- */
@media (max-width: 640px) {
  .stk-container { padding: 0 10px; }
  .stk-header { padding: 16px; }
  .stk-section { padding: 16px; }
  .stk-sticky-chart { padding: 0 12px 10px; }
  .stk-tab { padding: 8px 14px; font-size: 0.82rem; }
  .stk-filters { flex-direction: column; }
  .stk-table-wrap--scroll { max-height: 400px; }
}
</style>