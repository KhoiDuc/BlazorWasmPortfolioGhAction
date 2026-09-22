<template>
  <div class="currency-prices-container py-4">
    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
      <h2 class="mb-0 fw-bold d-flex align-items-center gap-2 text-dark">
        <span class="fs-4">💱</span> Oil & Commodity Prices
      </h2>
      
      <div class="search-wrapper">
        <i class="bi bi-search search-icon"></i>
        <input 
          type="text" 
          v-model="filterText" 
          class="form-control form-control-lg search-input" 
          placeholder="Filter by commodity or pair..." 
        />
      </div>
    </div>
    
    <!-- TradingView Chart Popup -->
    <div v-if="selectedSymbol" class="chart-overlay" @click.self="closeChart">
      <div class="chart-container">
        <div class="d-flex justify-content-between align-items-center mb-3 border-bottom pb-2">
          <h4 class="mb-0 fw-bold d-flex align-items-center gap-2">
            <i class="bi bi-graph-up-arrow text-primary"></i> {{ selectedSymbol }} Chart
          </h4>
          <button class="btn btn-sm btn-outline-danger rounded-pill px-3 fw-bold" @click="closeChart">
            <i class="bi bi-x-lg"></i> Close
          </button>
        </div>
        <TradingViewChart :coin="getTradingViewSymbol(selectedSymbol)" />
      </div>
    </div>
    
    <div v-if="isLoading" class="d-flex justify-content-center py-5">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    
    <div v-else class="card shadow-sm border-0 rounded-4 overflow-hidden glass-panel">
      <div class="table-responsive">
        <table class="table table-borderless align-middle mb-0 custom-table">
          <thead class="bg-light">
            <tr>
              <th class="text-uppercase text-secondary small fw-bold py-3 ps-4">Commodity</th>
              <th class="text-uppercase text-secondary small fw-bold py-3">Rate</th>
              <th class="text-uppercase text-secondary small fw-bold py-3">Bid</th>
              <th class="text-uppercase text-secondary small fw-bold py-3">Ask</th>
              <th class="text-uppercase text-secondary small fw-bold py-3">High</th>
              <th class="text-uppercase text-secondary small fw-bold py-3">Low</th>
              <th class="text-uppercase text-secondary small fw-bold py-3">Open</th>
              <th class="text-uppercase text-secondary small fw-bold py-3">Close</th>
              <th class="text-uppercase text-secondary small fw-bold py-3 pe-4 text-end">Timestamp</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredCurrencyData" :key="item.currency" 
                @click="selectSymbol(item.currency)"
                class="cursor-pointer row-hover-effect">
              <td class="ps-4 py-3">
                <div class="d-flex align-items-center gap-2">
                  <span class="symbol-badge">{{ getSymbolPrefix(item.currency) }}</span>
                  <strong class="text-dark">{{ item.currency }}</strong>
                </div>
              </td>
              <td class="py-3 fw-bold text-dark fs-6">{{ item.rate }}</td>
              <td class="py-3 text-muted">{{ item.bid }}</td>
              <td class="py-3 text-muted">{{ item.ask }}</td>
              <td class="py-3 text-success fw-medium">{{ item.high }}</td>
              <td class="py-3 text-danger fw-medium">{{ item.low }}</td>
              <td class="py-3 text-muted">{{ item.open }}</td>
              <td class="py-3 text-muted">{{ item.close }}</td>
              <td class="py-3 pe-4 text-end text-secondary small">{{ formatTimestamp(item.timestamp) }}</td>
            </tr>
            <tr v-if="filteredCurrencyData.length === 0">
              <td colspan="9" class="text-center py-5 text-muted">
                <i class="bi bi-inbox fs-2 d-block mb-2 text-light-muted"></i>
                No matching commodities found for "{{ filterText }}"
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';
import TradingViewChart from './TradingViewChart.vue';

export default {
  components: {
    TradingViewChart
  },
  setup() {
    const currencyData = ref([]);
    const isLoading = ref(false);
    const filterText = ref('');
    const selectedSymbol = ref(null);

    onMounted(async () => {
      isLoading.value = true;
      try {
        const response = await axios.get('/api/rates'); // Use proxy
        if (Array.isArray(response.data)) {
          currencyData.value = response.data;
        } else {
          console.warn('Invalid data format received from /api/rates:', typeof response.data);
          currencyData.value = [];
        }
      } catch (error) {
        console.error('Error fetching currency data:', error);
        currencyData.value = [];
      } finally {
        isLoading.value = false;
      }
    });
    
    const filteredCurrencyData = computed(() => {
      if (!Array.isArray(currencyData.value)) return [];
      return currencyData.value.filter(item =>
        item && item.currency && item.currency.toLowerCase().includes(filterText.value.toLowerCase())
      );
    });

    const formatTimestamp = (timestamp) => {
      if (!timestamp) return 'N/A';
      const date = new Date(parseInt(timestamp));
      return date.toLocaleString('en-US', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false
      });
    };

    const selectSymbol = (symbol) => {
      selectedSymbol.value = symbol;
    };

    const closeChart = () => {
      selectedSymbol.value = null;
    };

    const getTradingViewSymbol = (symbol) => {
      // Map to forex pairs
      if (symbol.includes('/')) {
        return `FX:${symbol.replace(/\//g, '')}`;
      }
      
      // Map to NASDAQ symnols
      if (symbol.includes('#')) {
        return `NASDAQ:${symbol.replace('#', '')}`;
      }

      // Map commodity names to TradingView symbols
      const symbolMap = {
        'CrudeOIL': 'TVC:USOIL',
        'BRENT_OIL': 'TVC:UKOIL',
        'HEATING_OIL': 'NYMEX:HO1!',
        'USOil': 'TVC:USOIL',
        'UKOil': 'TVC:UKOIL'
      };
      return symbolMap[symbol] || `FX:${symbol}`;
    };

    const getSymbolPrefix = (symbol) => {
      if (!symbol) return '';
      if (symbol.includes('/')) return symbol.split('/')[0];
      if (symbol.length > 3) return symbol.substring(0, 3).toUpperCase();
      return symbol.substring(0, 2).toUpperCase();
    };

    return {
      currencyData,
      isLoading,
      filterText,
      filteredCurrencyData,
      formatTimestamp,
      selectedSymbol,
      selectSymbol,
      closeChart,
      getTradingViewSymbol,
      getSymbolPrefix
    };
  },
};
</script>
<style scoped>
.currency-prices-container {
  font-family: 'Outfit', 'Inter', 'Segoe UI', Roboto, sans-serif;
}

.search-wrapper {
  position: relative;
  width: 100%;
  max-width: 350px;
}

.search-icon {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-size: 1.1rem;
}

.search-input {
  padding-left: 48px;
  border-radius: 50px;
  border: 1px solid rgba(0,0,0,0.08);
  background-color: #f8fafc;
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);
  transition: all 0.3s ease;
  font-size: 0.95rem;
}

.search-input:focus {
  background-color: #ffffff;
  border-color: #3b82f6;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
  outline: none;
}

.glass-panel {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.05);
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.03);
}

.custom-table {
  border-collapse: separate;
  border-spacing: 0;
}

.custom-table thead th {
  background-color: #f8fafc;
  border-bottom: 2px solid #e2e8f0;
  color: #64748b;
  font-size: 0.75rem;
  letter-spacing: 0.5px;
  white-space: nowrap;
}

.row-hover-effect {
  transition: all 0.2s ease;
  border-bottom: 1px solid #f1f5f9;
}

.row-hover-effect:last-child {
  border-bottom: none;
}

.row-hover-effect:hover {
  background-color: #f8fafc !important;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.03);
  position: relative;
  z-index: 1;
}

.symbol-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, #e0e7ff 0%, #c7d2fe 100%);
  color: #4338ca;
  border-radius: 12px;
  font-weight: 700;
  font-size: 0.8rem;
  letter-spacing: -0.5px;
  box-shadow: 0 2px 8px rgba(67, 56, 202, 0.15);
}

.cursor-pointer {
  cursor: pointer;
}

.text-light-muted {
  color: #cbd5e1 !important;
}

.chart-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(15, 23, 42, 0.7);
  backdrop-filter: blur(8px);
  z-index: 1050;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
  animation: fadeIn 0.3s ease;
}

.chart-container {
  background: #ffffff;
  padding: 24px;
  border-radius: 20px;
  width: 90%;
  max-width: 1200px;
  max-height: 85vh;
  overflow-y: auto;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.3);
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@media (max-width: 768px) {
  .search-wrapper {
    max-width: 100%;
  }
  .chart-container {
    width: 95%;
    max-height: 90vh;
    padding: 16px;
  }
}
</style>