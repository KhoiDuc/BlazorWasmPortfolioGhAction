<template>
  <div class="stk-page flex-grow-1 d-flex flex-column">
    <NavBar />
    <div class="stk-container flex-grow-1 py-4">
      
      <!-- Premium Terminal Panel -->
      <div class="stk-panel">
        
        <!-- Header -->
        <div class="stk-header">
          <div class="stk-header__icon text-primary">
            <i class="fa-solid fa-building-circle-check fs-4"></i>
          </div>
          <div>
            <h2 class="stk-header__title">Real Estate Market Intelligence</h2>
            <p class="stk-header__sub">Track regional property prices, listings, and trends in Vietnam</p>
          </div>
        </div>

        <div class="stk-section">
          <!-- Filters Row -->
          <div class="row g-3 mb-4">
            <div class="col-md-3">
              <label class="stk-label">Region</label>
              <select v-model="selectedRegion" class="stk-input" @change="fetchData">
                <option value="">All Regions</option>
                <option v-for="r in regions" :key="r" :value="r">{{ r }}</option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="stk-label">Property Type</label>
              <select v-model="selectedType" class="stk-input" @change="fetchData">
                <option value="">All Types</option>
                <option v-for="t in types" :key="t" :value="t">{{ t }}</option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="stk-label">Location Keywords</label>
              <input v-model="filterLocation" class="stk-input" placeholder="Search town, street..." @keyup.enter="fetchData" />
            </div>
            <div class="col-md-3 d-flex align-items-end">
              <button class="stk-btn stk-btn--primary w-100" @click="fetchData">
                <i class="fa-solid fa-rotate me-2"></i>Refresh Data
              </button>
            </div>
          </div>

          <!-- Chart Section -->
          <div class="stk-card mb-4">
            <div class="stk-card-header">
              <i class="fa-solid fa-chart-line me-2 text-primary"></i>Price Per m² History (Average)
            </div>
            <div class="stk-card-body" style="height: 380px; position: relative;">
              <Line v-if="loaded" :data="chartData" :options="chartOptions" />
              <div v-else class="text-center py-5">
                <div class="spinner-border text-primary mb-3" role="status"></div>
                <div class="text-muted">Analyzing market trends...</div>
              </div>
            </div>
          </div>

          <!-- Listings Table -->
          <div class="stk-card">
            <div class="stk-card-header d-flex justify-content-between align-items-center">
              <div>
                <i class="fa-solid fa-list-ul me-2 text-primary"></i>Recent Real Estate Listings
              </div>
              <span class="badge bg-primary-glow px-2 py-1" style="font-size: 0.75rem;">
                {{ items.length }} Records Found
              </span>
            </div>
            <div class="stk-table-wrap table-responsive">
              <table class="stk-table">
                <thead>
                  <tr>
                    <th class="stk-th">Date</th>
                    <th class="stk-th">Region</th>
                    <th class="stk-th">Type</th>
                    <th class="stk-th">Location</th>
                    <th class="stk-th">Area</th>
                    <th class="stk-th">Title</th>
                    <th class="stk-th text-end">Price</th>
                    <th class="stk-th text-end">Price/m²</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in items" :key="item.id" class="stk-row">
                    <td class="stk-td text-muted" style="font-size: 0.8rem;">{{ formatDate(item.fetched_at) }}</td>
                    <td class="stk-td fw-semibold text-dark">{{ item.region }}</td>
                    <td class="stk-td">
                      <span :class="['stk-type-badge', item.property_type ? item.property_type.toLowerCase() : '']">
                        {{ item.property_type || '-' }}
                      </span>
                    </td>
                    <td class="stk-td text-muted">{{ item.location }}</td>
                    <td class="stk-td text-dark">{{ item.area ? item.area + ' m²' : '-' }}</td>
                    <td class="stk-td">
                      <a :href="item.url" target="_blank" class="stk-link">
                        {{ truncate(item.title, 45) }}
                        <i class="fa-solid fa-arrow-up-right-from-square ms-1" style="font-size: 0.7rem; opacity: 0.7;"></i>
                      </a>
                    </td>
                    <td class="stk-td fw-bold text-end text-success">{{ item.price_text }}</td>
                    <td class="stk-td fw-semibold text-end text-info">{{ formatPricePerM2(item) }}</td>
                  </tr>
                  <tr v-if="items.length === 0">
                    <td colspan="8" class="text-center p-5 text-muted">
                      <i class="fa-regular fa-folder-open fs-3 mb-3 d-block text-secondary"></i>
                      No properties found matching your selection criteria.
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
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
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import { Line } from 'vue-chartjs';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

export default {
  name: 'RealEstateView',
  components: {
    NavBar,
    AppFooter,
    Line
  },
  setup() {
    const items = ref([]);
    const loaded = ref(false);
    const selectedRegion = ref('');
    const selectedType = ref('');
    const filterLocation = ref('');
    
    // Hardcoded for now based on scraper script
    const regions = ["Hồ Chí Minh", "Bình Dương", "Bà Rịa - Vũng Tàu", "Đồng Nai"];
    const types = ["House", "Land", "Apartment"];

    const fetchData = async () => {
      loaded.value = false;
      try {
        const params = {};
        if (selectedRegion.value) params.region = selectedRegion.value;
        if (selectedType.value) params.type = selectedType.value;
        if (filterLocation.value) params.location = filterLocation.value;
        
        const response = await axios.get('/getRealEstate', { params });
        items.value = response.data || [];
      } catch (e) {
        console.error("Error loading real estate data", e);
      } finally {
        loaded.value = true;
      }
    };

    const chartData = computed(() => {
        // Group by Date and Region to compute average
        const grouped = {};
        items.value.forEach(item => {
            if (item.area > 0 && item.price_numeric > 0) {
                const date = new Date(item.fetched_at).toLocaleDateString();
                if (!grouped[date]) grouped[date] = [];
                grouped[date].push(item.price_numeric / item.area);
            }
        });

        const labels = Object.keys(grouped).sort();
        const dataPoints = labels.map(date => {
            const prices = grouped[date];
            const sum = prices.reduce((a, b) => a + b, 0);
            return prices.length ? sum / prices.length : 0;
        });

        return {
            labels: labels,
            datasets: [
                {
                    label: 'Average Price / m² (VND)',
                    backgroundColor: 'rgba(59, 130, 246, 0.2)',
                    borderColor: '#60a5fa',
                    borderWidth: 2,
                    pointBackgroundColor: '#2563eb',
                    pointBorderColor: '#60a5fa',
                    pointHoverBackgroundColor: '#fff',
                    pointHoverBorderColor: '#2563eb',
                    data: dataPoints,
                    fill: true,
                    tension: 0.2
                }
            ]
        };
    });

    const chartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
          x: {
              title: { display: true, text: 'Date', color: '#94a3b8', font: { family: 'Inter', weight: 600 } },
              ticks: { color: '#94a3b8', font: { family: 'Inter' } },
              grid: { color: 'rgba(255, 255, 255, 0.05)' }
          },
          y: {
              title: { display: true, text: 'Price/m² (VND)', color: '#94a3b8', font: { family: 'Inter', weight: 600 } },
              ticks: {
                  color: '#94a3b8',
                  font: { family: 'Inter' },
                  callback: function(value) {
                      return value.toLocaleString() + ' đ';
                  }
              },
              grid: { color: 'rgba(255, 255, 255, 0.05)' }
          }
      },
      plugins: {
          legend: {
              labels: {
                  color: '#f1f5f9',
                  font: { family: 'Inter', weight: 600 }
              }
          },
          tooltip: {
              backgroundColor: '#1e2235',
              titleColor: '#fff',
              bodyColor: '#cbd5e1',
              borderColor: 'rgba(255, 255, 255, 0.1)',
              borderWidth: 1,
              callbacks: {
                  label: function(context) {
                      return context.raw.toLocaleString() + ' đ/m²';
                  }
              }
          }
      }
    };

    const formatDate = (str) => {
        if(!str) return '';
        return new Date(str).toLocaleString();
    };

    const truncate = (str, len) => {
        if (!str) return '';
        return str.length > len ? str.substring(0, len) + "..." : str;
    };

    const formatPricePerM2 = (item) => {
        if (item.area > 0 && item.price_numeric > 0) {
             const val = Math.round(item.price_numeric / item.area);
             return val.toLocaleString() + ' đ/m²';
        }
        return '-';
    };

    onMounted(() => {
        fetchData();
    });

    return {
        items,
        loaded,
        selectedRegion,
        selectedType,
        filterLocation,
        regions,
        types,
        fetchData,
        chartData,
        chartOptions,
        formatDate,
        truncate,
        formatPricePerM2
    };
  }
};
</script>

<style scoped>
/* ============================== */
/*  REAL ESTATE – Sleek Theme    */
/* ============================== */

.stk-page {
  background: #ffffff;
  min-height: 100vh;
  color: #1e293b;
}

.stk-container {
  max-width: 1440px;
  margin: 0 auto;
  padding: 0 24px;
}

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
  font-size: 1.25rem;
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
  padding: 24px;
}
.stk-label {
  display: block;
  font-size: 0.78rem;
  font-weight: 600;
  color: #475569;
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
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
  box-shadow: 0 0 0 3px rgba(59,130,246,0.15);
}

/* ---------- BUTTON ---------- */
.stk-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 10px 18px;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid transparent;
  outline: none;
}
.stk-btn--primary {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  border-color: rgba(255,255,255,0.1);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.2);
}
.stk-btn--primary:hover {
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
  box-shadow: 0 6px 16px rgba(59, 130, 246, 0.35);
}

/* ---------- CARDS ---------- */
.stk-card {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: 12px;
  overflow: hidden;
}
.stk-card-header {
  padding: 14px 20px;
  background: #f8fafc;
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
  font-family: 'Outfit', sans-serif;
  font-weight: 600;
  font-size: 0.92rem;
  color: #0f172a;
  display: flex;
  align-items: center;
}
.stk-card-body {
  padding: 20px;
  background: #ffffff;
}

/* ---------- TABLE ---------- */
.stk-table-wrap {
  border-radius: 0 0 12px 12px;
  overflow: hidden;
  max-height: 520px;
  border: 1px solid rgba(0, 0, 0, 0.06);
}
.stk-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}
.stk-th {
  padding: 12px 16px;
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
.stk-td {
  padding: 12px 16px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.04);
  vertical-align: middle;
  color: #334155;
}
.stk-row {
  cursor: pointer;
  transition: background 0.15s ease;
}
.stk-row:hover {
  background: #f8fafc;
}

/* ---------- LINKS ---------- */
.stk-link {
  color: #2563eb;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.15s ease;
}
.stk-link:hover {
  color: #1d4ed8;
  text-decoration: underline;
}

/* ---------- BADGES ---------- */
.stk-type-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 0.72rem;
  font-weight: 600;
  letter-spacing: 0.3px;
  text-transform: uppercase;
}
.stk-type-badge.house {
  background: rgba(16, 185, 129, 0.08);
  color: #059669;
  border: 1px solid rgba(16, 185, 129, 0.2);
}
.stk-type-badge.land {
  background: rgba(245, 158, 11, 0.08);
  color: #d97706;
  border: 1px solid rgba(245, 158, 11, 0.2);
}
.stk-type-badge.apartment {
  background: rgba(59, 130, 246, 0.08);
  color: #2563eb;
  border: 1px solid rgba(59, 130, 246, 0.2);
}
.bg-primary-glow {
  background: rgba(59, 130, 246, 0.08);
  border: 1px solid rgba(59, 130, 246, 0.2);
  color: #2563eb;
}
</style>
