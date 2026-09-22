<template>
  <div class="news-item" :class="{ 'news-expired': item.status !== 'active' }">
    <div class="ni-top">
      <div class="ni-info">
        <h4 class="ni-title">{{ item.title }}</h4>
        <p class="ni-content" :class="{ 'ni-expanded': isExpanded }">{{ item.content }}</p>
        <div class="ni-meta-actions">
          <a v-if="item.source_url" :href="item.source_url" target="_blank" rel="noopener noreferrer" class="ni-source">
            Nguồn
          </a>
          <button 
            v-if="item.content && item.content.length > 100" 
            @click="isExpanded = !isExpanded" 
            class="ni-expand-btn"
          >
            <span>{{ isExpanded ? 'Thu gọn' : 'Xem thêm' }}</span>
            <svg class="chevron-icon" :class="{ 'rotate-180': isExpanded }" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="width: 10px; height: 10px;">
              <polyline points="6 9 12 15 18 9"></polyline>
            </svg>
          </button>
        </div>
      </div>
      <span class="ni-importance" :class="'ni-imp-' + item.importance">
        {{ item.importance }}
      </span>
    </div>
    <div class="ni-bottom">
      <button v-if="showActions" @click="$emit('toggle')" class="ni-status-btn" :class="item.status === 'active' ? 'ni-active' : 'ni-inactive'">
        {{ item.status === 'active' ? 'Active' : 'Expired' }}
      </button>
      <span v-else class="ni-status-badge" :class="item.status === 'active' ? 'badge-active' : 'badge-inactive'">
        {{ item.status === 'active' ? 'Active' : 'Expired' }}
      </span>

      <div v-if="showActions" class="ni-actions">
        <button @click="$emit('edit')" class="ni-act-btn" title="Sửa">
          <svg width="14" height="14" viewBox="0 0 16 16" fill="none"><path d="M11.5 1.5l3 3L5 14H2v-3L11.5 1.5z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>
        </button>
        <button @click="$emit('delete')" class="ni-act-btn ni-act-danger" title="Xóa">
          <svg width="14" height="14" viewBox="0 0 16 16" fill="none"><path d="M2 4h12M5.33 4V2.67a1.33 1.33 0 011.34-1.34h2.66a1.33 1.33 0 011.34 1.34V4m2 0v9.33a1.33 1.33 0 01-1.34 1.34H4.67a1.33 1.33 0 01-1.34-1.34V4h9.34z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

defineProps({
  item: Object,
  showActions: {
    type: Boolean,
    default: true
  }
})
defineEmits(['toggle', 'edit', 'delete'])

const isExpanded = ref(false)
</script>

<style scoped>
.news-item {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 0.85rem 1rem;
  transition: border-color 0.15s, box-shadow 0.15s;
}
.news-item:hover {
  border-color: #cbd5e1;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.news-expired {
  opacity: 0.6;
  background: #f8fafc;
}

/* ΓöÇΓöÇ Top row ΓöÇΓöÇ */
.ni-top {
  display: flex;
  gap: 0.75rem;
  align-items: flex-start;
  margin-bottom: 0.65rem;
}
.ni-info {
  flex: 1;
  min-width: 0;
}
.ni-title {
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 0.3rem;
  font-size: 0.9rem;
  line-height: 1.35;
}
.ni-content {
  font-size: 0.82rem;
  color: #64748b;
  margin: 0;
  line-height: 1.45;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  transition: max-height 0.25s ease;
}
.ni-content.ni-expanded {
  display: block;
  -webkit-line-clamp: unset;
  line-clamp: unset;
  overflow: visible;
}
.ni-meta-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-top: 0.4rem;
}
.ni-source {
  display: inline-block;
  font-size: 0.78rem;
  color: #3b82f6;
  text-decoration: none;
  font-weight: 500;
}
.ni-source:hover {
  color: #1d4ed8;
  text-decoration: underline;
}

/* ΓöÇΓöÇ Importance badge ΓöÇΓöÇ */
.ni-importance {
  flex-shrink: 0;
  padding: 0.25rem 0.6rem;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 700;
  white-space: nowrap;
}
.ni-imp-1 { background: #dbeafe; color: #1e40af; }
.ni-imp-2 { background: #d1fae5; color: #065f46; }
.ni-imp-3 { background: #fef3c7; color: #92400e; }
.ni-imp-4 { background: #fed7aa; color: #9a3412; }
.ni-imp-5 { background: #fee2e2; color: #991b1b; }

/* ΓöÇΓöÇ Bottom row ΓöÇΓöÇ */
.ni-bottom {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px solid #f1f5f9;
}
.ni-status-btn {
  flex: 1;
  padding: 0.35rem 0.6rem;
  border: none;
  border-radius: 6px;
  font-size: 0.78rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
  text-align: left;
}
.ni-active {
  background: #ecfdf5;
  color: #065f46;
}
.ni-active:hover { background: #d1fae5; }
.ni-inactive {
  background: #f1f5f9;
  color: #64748b;
}
.ni-inactive:hover { background: #e2e8f0; }

.ni-actions {
  display: flex;
  gap: 0.35rem;
}
.ni-act-btn {
  width: 30px;
  height: 30px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: #fff;
  color: #64748b;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s;
}
.ni-act-btn:hover {
  background: #eef2ff;
  color: #3b82f6;
  border-color: #93c5fd;
}
.ni-act-danger:hover {
  background: #fef2f2;
  color: #ef4444;
  border-color: #fca5a5;
}

@media (max-width: 640px) {
  .news-item {
    padding: 0.7rem 0.8rem;
  }
  .ni-title {
    font-size: 0.85rem;
  }
}

.ni-status-badge {
  padding: 0.35rem 0.75rem;
  border-radius: 6px;
  font-size: 0.78rem;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
}
.badge-active {
  background: #ecfdf5;
  color: #065f46;
  border: 1px solid rgba(16, 185, 129, 0.15);
}
.badge-inactive {
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid rgba(148, 163, 184, 0.15);
}

.ni-expand-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  background: none;
  border: none;
  padding: 0;
  font-size: 0.78rem;
  color: #3b82f6;
  font-weight: 500;
  cursor: pointer;
  transition: color 0.15s ease;
}
.ni-expand-btn:hover {
  color: #1d4ed8;
}
.chevron-icon {
  transition: transform 0.2s ease;
}
.chevron-icon.rotate-180 {
  transform: rotate(180deg);
}
</style>
