<template>
  <div class="world-state-card" :class="{ 'ws-borderless': borderless }">
    <div class="ws-header">
      <h3 class="ws-title">🌍 Current World State</h3>
      <span class="ws-updated" v-if="worldState.updated_at">Cập nhật: {{ formatDate(worldState.updated_at) }}</span>
    </div>
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
    </div>
    <div v-else-if="Object.keys(stateData).length === 0" class="text-muted text-center py-4">Chưa có dữ liệu World State</div>
    <div v-else class="ws-grid">
      <div v-for="(fields, entity) in sortedEntities" :key="entity" class="ws-entity-card">
        <div class="entity-header">
          <h4 class="entity-title">{{ formatEntityName(entity) }}</h4>
          <span class="entity-updated" v-if="fields._updated_at">Cập nhật: {{ formatDate(fields._updated_at) }}</span>
        </div>
        <div class="field-list">
          <div v-for="(value, key) in visibleFields(fields)" :key="key" class="field-item">
            <span class="field-key">{{ formatKey(key) }}</span>
            <div v-if="parseValue(value)" class="field-list-value">
              <div v-for="(item, idx) in parseValue(value)" :key="idx" class="field-list-item">
                <span class="bullet-dot">•</span>
                <span class="item-text" :class="valueClass(item)">{{ item }}</span>
              </div>
            </div>
            <span v-else class="field-value" :class="valueClass(value)">{{ value }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  worldState: {
    type: Object,
    default: () => ({})
  },
  loading: Boolean,
  borderless: {
    type: Boolean,
    default: false
  }
});

const stateData = computed(() => {
  if (!props.worldState.state_json) return {};
  if (typeof props.worldState.state_json === 'string') {
    try {
      return JSON.parse(props.worldState.state_json);
    } catch(e) {
      return {};
    }
  }
  return props.worldState.state_json;
});

// Sort entities by _updated_at descending (most recently updated first).
// Entities without _updated_at are placed at the end.
const sortedEntities = computed(() => {
  const data = stateData.value;
  const entries = Object.entries(data);
  entries.sort((a, b) => {
    const timeA = a[1] && a[1]._updated_at ? new Date(a[1]._updated_at).getTime() : 0;
    const timeB = b[1] && b[1]._updated_at ? new Date(b[1]._updated_at).getTime() : 0;
    return timeB - timeA; // descending: newest first
  });
  return Object.fromEntries(entries);
});

const formatEntityName = (name) => {
  if (!name) return '';
  return name.replace(/_/g, ' ').toUpperCase();
};

const formatKey = (key) => {
  if (!key) return '';
  return key.replace(/_/g, ' ');
};

const formatDate = (dateStr) => {
  if (!dateStr) return '';
  try {
    const d = new Date(dateStr);
    // Use local timezone (Intl.DateTimeFormat with timeZoneName omitted = local)
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
  } catch(e) {
    return dateStr;
  }
};

const valueClass = (value) => {
  if (!value) return '';
  const val = value.toLowerCase();
  // Red/Accent Red (Hawkish, tightening, high risk, production cuts, rate hikes, SBV draining liquidity)
  if (
    val.includes('hawkish') || val.includes('tightening') || val.includes('high') || val.includes('elevated') || val.includes('risk') || val.includes('danger') ||
    val.includes('diều hâu') || val.includes('thắt chặt') || val.includes('rủi ro') || val.includes('nguy hiểm') || val.includes('tăng lãi suất') || 
    val.includes('cắt giảm sản lượng') || val.includes('hút ròng') || val.includes('hút thanh khoản') || val.includes('tăng lãi suất điều hành')
  ) {
    return 'value-accent-red';
  }
  // Green/Accent Green (Dovish, easing, low risk, stable, positive, production increases, rate cuts, SBV injecting liquidity)
  if (
    val.includes('dovish') || val.includes('easing') || val.includes('low') || val.includes('stable') || val.includes('positive') ||
    val.includes('bồ câu') || val.includes('nới lỏng') || val.includes('ổn định') || val.includes('tích cực') || val.includes('hạ lãi suất') || 
    val.includes('tăng sản lượng') || val.includes('bơm ròng') || val.includes('bơm thanh khoản') || val.includes('hạ lãi suất điều hành')
  ) {
    return 'value-accent-green';
  }
  // Orange/Accent Orange (Neutral, correction, bifurcated, slowing, neutral, steady/hold)
  if (
    val.includes('neutral') || val.includes('correction') || val.includes('bifurcated') || val.includes('slowing') ||
    val.includes('trung lập') || val.includes('chậm lại') || val.includes('điều chỉnh') || val.includes('phân hóa') || val.includes('giữ nguyên')
  ) {
    return 'value-accent-orange';
  }
  return '';
};

const visibleFields = (fields) => {
  const result = {};
  for (const key of Object.keys(fields)) {
    if (key.startsWith('_')) continue;
    result[key] = fields[key];
  }
  return result;
};

const parseValue = (val) => {
  if (!val) return null;
  if (Array.isArray(val)) return val;
  if (typeof val === 'string') {
    const trimmed = val.trim();
    if (trimmed.startsWith('[') && trimmed.endsWith(']')) {
      try {
        const parsed = JSON.parse(trimmed);
        if (Array.isArray(parsed)) {
          return parsed;
        }
      } catch (e) {
        // Not a JSON array
      }
    }
  }
  return null;
};
</script>

<style scoped>
.world-state-card {
  background: #fff;
  border-radius: 16px;
  border: 1px solid rgba(0,0,0,0.06);
  padding: 1.75rem;
  margin-bottom: 2rem;
  box-shadow: 0 10px 30px rgba(0,0,0,0.02);
}
.world-state-card.ws-borderless {
  background: transparent;
  border: none;
  box-shadow: none;
  padding: 0;
  margin-bottom: 0;
}
.world-state-card.ws-borderless .ws-header {
  margin-bottom: 1.25rem;
  padding-bottom: 0.75rem;
}
.world-state-card.ws-borderless .ws-title {
  font-size: 1.15rem;
  font-weight: 700;
}
.ws-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.75rem;
  border-bottom: 1px solid rgba(0,0,0,0.05);
  padding-bottom: 1.25rem;
}
.ws-title {
  margin: 0;
  font-family: 'Outfit', sans-serif;
  font-size: 1.35rem;
  font-weight: 800;
  color: #0f172a;
}
.ws-updated {
  font-size: 0.8rem;
  color: #64748b;
  font-weight: 500;
}
.ws-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.25rem;
}
.ws-entity-card {
  background: linear-gradient(145deg, #ffffff 0%, #f8fafc 100%);
  border: 1px solid rgba(59, 130, 246, 0.08);
  border-radius: 14px;
  padding: 1.25rem;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.01);
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  display: flex;
  flex-direction: column;
}
.ws-entity-card:hover {
  transform: translateY(-2px);
  border-color: rgba(59, 130, 246, 0.18);
  box-shadow: 0 8px 24px rgba(59, 130, 246, 0.06);
}

@media (min-width: 640px) and (max-width: 1023px) {
  .ws-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  .ws-entity-card:last-child:nth-child(2n-1) {
    grid-column: span 2;
  }
}

@media (min-width: 1024px) {
  .ws-grid {
    grid-template-columns: repeat(3, 1fr);
  }
  .ws-entity-card:last-child:nth-child(3n-2) {
    grid-column: span 3;
  }
  .ws-entity-card:last-child:nth-child(3n-1) {
    grid-column: span 2;
  }
}

.entity-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding-bottom: 0.6rem;
  border-bottom: 1px solid rgba(59, 130, 246, 0.08);
}
.entity-title {
  font-family: 'Outfit', sans-serif;
  font-size: 0.85rem;
  font-weight: 800;
  color: #2563eb;
  margin: 0;
  letter-spacing: 0.5px;
}
.entity-updated {
  font-size: 0.65rem;
  color: #94a3b8;
  font-weight: 500;
  white-space: nowrap;
}
.field-list {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
  flex-grow: 1;
}
.field-item {
  display: flex;
  flex-direction: column;
}
.field-key {
  color: #64748b;
  text-transform: uppercase;
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.7px;
  margin-bottom: 0.2rem;
}
.field-value {
  font-size: 0.9rem;
  font-weight: 600;
  color: #1e293b;
  line-height: 1.4;
}

/* Value accent styling */
.value-accent-red {
  color: #dc2626;
}
.value-accent-green {
  color: #16a34a;
}
.value-accent-orange {
  color: #ea580c;
}

/* List/Array value custom styling */
.field-list-value {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-top: 0.25rem;
  padding-left: 0.15rem;
}
.field-list-item {
  display: flex;
  align-items: flex-start;
  font-size: 0.85rem;
  line-height: 1.4;
}
.bullet-dot {
  color: #3b82f6;
  margin-right: 0.4rem;
  font-weight: 800;
  font-size: 0.9rem;
  line-height: 1.1;
}
.item-text {
  flex: 1;
  color: #334155;
  font-weight: 500;
}
</style>
