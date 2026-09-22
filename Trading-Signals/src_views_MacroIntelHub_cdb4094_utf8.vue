<template>
  <div class="hub-container-wrapper">
    <NavBar />
    <div class="macro-hub-container">
      <!-- Header -->
      <div class="hub-header">
        <div class="hub-header-left">
          <h1 class="hub-title">Macro Intelligence Hub</h1>
          <p class="hub-subtitle">Quß║ún l├╜ v├á ph├ón t├¡ch c├íc sß╗▒ kiß╗çn v─⌐ m├┤ ß║únh h╞░ß╗ƒng ─æß║┐n thß╗ï tr╞░ß╗¥ng</p>
        </div>
        <div class="hub-header-actions">
          <button @click="showGroupForm = true" class="macro-btn macro-btn-blue">
            + Nh├│m mß╗¢i
          </button>
          <button @click="generatePrompt" class="macro-btn macro-btn-yellow">
            ≡ƒñû AI Strategy
          </button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="hub-loading">
        <div class="spinner-border text-primary mb-3" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="text-muted">─Éang tß║úi dß╗» liß╗çu...</p>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="hub-error">
        <span class="hub-error-icon">ΓÜá∩╕Å</span>
        <div>
          <strong>Lß╗ùi:</strong>
          <p class="mb-0 small">{{ error }}</p>
        </div>
      </div>

      <!-- World State & Pending Changes (OSINT) -->
      <PendingChanges :changes="pendingChanges" @approve="approveChange" @reject="rejectChange" />
      <WorldState :worldState="worldState" :loading="loadingState" />

      <!-- Empty -->
      <div v-else-if="groups && groups.length === 0" class="hub-empty">
        <div class="hub-empty-inner">
          <div class="hub-empty-icon">≡ƒôè</div>
          <h5>Ch╞░a c├│ nh├│m sß╗▒ kiß╗çn n├áo</h5>
          <p class="text-muted mb-4">H├úy tß║ío nh├│m mß╗¢i ─æß╗â bß║»t ─æß║ºu quß║ún l├╜ tin tß╗⌐c v─⌐ m├┤</p>
          <button @click="showGroupForm = true" class="macro-btn macro-btn-blue">+ Tß║ío nh├│m ─æß║ºu ti├¬n</button>
        </div>
      </div>

      <!-- Groups Grid -->
      <div v-else-if="groups && groups.length > 0" class="hub-grid">
        <GroupCard v-for="group in groups" :key="group.id" :group="group" :isReadOnly="group.name === 'Telegram News'"
          @edit="editGroup(group)" @delete="deleteGroup(group)" @updateConclusion="updateConclusion(group, $event)">
          <div>
            <div class="news-section-header">
              <span class="news-section-title">≡ƒô░ Tin tức</span>
              <button v-if="group.name !== 'Telegram News'" @click="addNews(group)" class="macro-btn macro-btn-green macro-btn-sm">+ Thêm</button>
            </div>
            <div v-if="news[group.id] && news[group.id].length" class="news-list">
              <NewsItem v-for="item in news[group.id]" :key="item.id" :item="item" :show-actions="group.name !== 'Telegram News'"
                @toggle="toggleStatus(item)" @edit="editNews(item)" @delete="deleteNews(item)" />
            </div>
            <div v-else class="news-empty">
              ≡ƒô¡ Ch╞░a c├│ tin tß╗⌐c n├áo
            </div>
          </div>
        </GroupCard>
      </div>

      <!-- Forms & Modal -->
      <div class="macro-modal-overlay" v-if="showGroupForm || showNewsForm">
        <div class="macro-modal-box">
          <button @click="resetGroupForm(); resetNewsForm();" class="macro-modal-close">Γ£ò</button>
          <GroupForm v-if="showGroupForm" :modelValue="editingGroup" @submit="saveGroup" @cancel="resetGroupForm" />
          <NewsItemForm v-if="showNewsForm" :modelValue="editingNews" @submit="saveNews" @cancel="resetNewsForm" />
        </div>
      </div>
      <PromptModal v-if="showPromptModal" :prompt="promptText" @close="showPromptModal = false" />
    </div>
    <AppFooter />
  </div>
</template>

<script setup>
import NavBar from '../components/NavBar.vue'
import AppFooter from '../components/AppFooter.vue'

import { ref, reactive, onMounted } from 'vue'
import GroupCard from '../components/MacroIntelHub/GroupCard.vue'
import NewsItem from '../components/MacroIntelHub/NewsItem.vue'
import NewsItemForm from '../components/MacroIntelHub/NewsItemForm.vue'
import GroupForm from '../components/MacroIntelHub/GroupForm.vue'
import PromptModal from '../components/MacroIntelHub/PromptModal.vue'
import WorldState from '../components/MacroIntelHub/WorldState.vue'
import PendingChanges from '../components/MacroIntelHub/PendingChanges.vue'

const groups = ref([])
const news = reactive({})
const loading = ref(true)
const error = ref('')
const showGroupForm = ref(false)
const showNewsForm = ref(false)
const editingGroup = ref(null)
const editingNews = ref(null)
const showPromptModal = ref(false)
const promptText = ref('')

// OSINT State
const worldState = ref({})
const pendingChanges = ref([])
const loadingState = ref(false)

function getUserId() {
  try {
    const stored = localStorage.getItem('userInfo')
    if (stored) {
      const info = JSON.parse(stored)
      return info.id || info.custodyCode || ''
    }
  } catch (e) {
    console.error('Error reading userInfo:', e)
  }
  return ''
}

function fetchGroups() {
  loading.value = true
  error.value = ''
  const uid = getUserId()
  console.log('Fetching news groups for user:', uid)
  fetch(`/api/news-groups${uid ? '?user_id=' + encodeURIComponent(uid) : ''}`, { headers: authHeader() })
    .then(async r => {
      console.log('Response status:', r.status, r.ok)
      if (!r.ok) {
        const err = await r.text();
        console.error('API error response:', err)
        error.value = `API Error: ${r.status} - ${err}`
        throw new Error('API returned error')
      }
      try {
        const text = await r.text()
        console.log('Raw response:', text)
        const data = text ? JSON.parse(text) : null
        console.log('Parsed data:', data)
        return Array.isArray(data) ? data : []
      } catch (e) {
        console.error('JSON parse error:', e)
        error.value = `Parse error: ${e.message}`
        return []
      }
    })
    .then(data => {
      console.log('Processing data:', data, Array.isArray(data), data.length)
      groups.value = data || []
      if (data && data.length > 0) {
        data.forEach(g => {
          console.log('Fetching news for group:', g.id)
          fetchNews(g.id)
        })
      }
    })
    .catch(e => {
      console.error('fetchGroups error:', e)
      if (!error.value) {
        error.value = `Error: ${e.message}`
      }
      groups.value = []
    })
    .finally(() => {
      loading.value = false
      console.log('Fetch complete, groups:', groups.value)
    })
}
function fetchNews(groupId) {
  fetch(`/api/news-items?group_id=${groupId}`, { headers: authHeader() })
    .then(r => {
      if (!r.ok) {
        console.error(`Fetch news items for group ${groupId} failed:`, r.status)
        return []
      }
      return r.json().catch(e => {
        console.error(`JSON parse error for group ${groupId}:`, e)
        return []
      })
    })
    .then(data => {
      if (Array.isArray(data)) {
        news[groupId] = data
      } else {
        console.warn(`Invalid data for group ${groupId}:`, data)
        news[groupId] = []
      }
    })
    .catch(e => {
      console.error(`fetchNews error for groupId ${groupId}:`, e)
      news[groupId] = []
    })
}
function addNews(group) {
  editingNews.value = { group_id: group.id, importance: 3, status: 'active' }
  showNewsForm.value = true
}
function editNews(item) {
  editingNews.value = { ...item }
  showNewsForm.value = true
}
function saveNews(item) {
  const method = item.id ? 'PUT' : 'POST'
  const url = item.id
    ? `/api/news-items?id=${item.id}`
    : `/api/news-items?group_id=${item.group_id}`
  fetch(url, {
    method,
    headers: { ...authHeader(), 'Content-Type': 'application/json' },
    body: JSON.stringify(item)
  })
    .then(r => {
      if (!r.ok) {
        console.error('saveNews failed:', r.status)
        return
      }
      fetchNews(item.group_id)
      resetNewsForm()
    })
    .catch(e => {
      console.error('saveNews error:', e)
    })
}
function deleteNews(item) {
  fetch(`/api/news-items?id=${item.id}`, { method: 'DELETE', headers: authHeader() })
    .then(r => {
      if (!r.ok) {
        console.error('deleteNews failed:', r.status)
        return
      }
      fetchNews(item.group_id)
    })
    .catch(e => {
      console.error('deleteNews error:', e)
    })
}
function toggleStatus(item) {
  fetch(`/api/news-items/toggle?id=${item.id}`, { method: 'POST', headers: authHeader() })
    .then(r => {
      if (!r.ok) {
        console.error('toggleStatus failed:', r.status)
        return
      }
      fetchNews(item.group_id)
    })
    .catch(e => {
      console.error('toggleStatus error:', e)
    })
}
function editGroup(group) {
  editingGroup.value = { ...group }
  showGroupForm.value = true
}
function saveGroup(group) {
  if (!group.name || !group.name.trim()) {
    alert('T├¬n nh├│m kh├┤ng ─æ╞░ß╗úc ─æß╗â trß╗æng!')
    return
  }
  const uid = getUserId()
  const method = group.id ? 'PUT' : 'POST'
  const url = group.id ? `/api/news-groups?id=${group.id}` : '/api/news-groups'
  const payload = { ...group, user_id: uid }
  fetch(url, {
    method,
    headers: { ...authHeader(), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  })
    .then(r => {
      if (!r.ok) {
        console.error('saveGroup failed:', r.status)
        return
      }
      fetchGroups()
      resetGroupForm()
    })
    .catch(e => {
      console.error('saveGroup error:', e)
    })
}
function deleteGroup(group) {
  fetch(`/api/news-groups?id=${group.id}`, { method: 'DELETE', headers: authHeader() })
    .then(r => {
      if (!r.ok) {
        console.error('deleteGroup failed:', r.status)
        return
      }
      fetchGroups()
    })
    .catch(e => {
      console.error('deleteGroup error:', e)
    })
}
function updateConclusion(group, conclusion) {
  fetch(`/api/news-groups?id=${group.id}`, {
    method: 'PUT',
    headers: { ...authHeader(), 'Content-Type': 'application/json' },
    body: JSON.stringify({ ...group, conclusion })
  })
    .then(r => {
      if (!r.ok) {
        console.error('updateConclusion failed:', r.status)
      }
    })
    .catch(e => {
      console.error('updateConclusion error:', e)
    })
}
function resetGroupForm() {
  editingGroup.value = null
  showGroupForm.value = false
}
function resetNewsForm() {
  editingNews.value = null
  showNewsForm.value = false
}
function generatePrompt() {
  fetch('/api/news-groups/generate-prompt', { headers: authHeader() })
    .then(r => {
      if (!r.ok) {
        console.error('generatePrompt failed:', r.status)
        return {}
      }
      return r.json()
    })
    .then(data => {
      if (data && data.prompt) {
        promptText.value = data.prompt
        showPromptModal.value = true
      } else {
        console.warn('generatePrompt returned invalid data:', data)
      }
    })
    .catch(e => {
      console.error('generatePrompt error:', e)
    })
}
function authHeader() {
  // ─Éß╗ông bß╗Ö vß╗¢i Community/MyPortfolio: truyß╗ün token ─æ─âng nhß║¡p
  const token = localStorage.getItem('token');
  return token ? { 'Authorization': `Bearer ${token}` } : {};
}

onMounted(() => {
  console.log('MacroIntelHub component mounted')
  fetchGroups()
  fetchWorldState()
  fetchPendingChanges()
})

function fetchWorldState() {
  loadingState.value = true
  fetch('/api/osint/world-state', { headers: authHeader() })
    .then(r => r.json())
    .then(data => worldState.value = data || {})
    .catch(e => console.error('fetchWorldState error:', e))
    .finally(() => loadingState.value = false)
}

function fetchPendingChanges() {
  fetch('/api/osint/changes/pending', { headers: authHeader() })
    .then(r => r.json())
    .then(data => pendingChanges.value = data || [])
    .catch(e => console.error('fetchPendingChanges error:', e))
}

function approveChange(id) {
  fetch(`/api/osint/changes/${id}/approve`, { method: 'POST', headers: authHeader() })
    .then(r => {
      if (r.ok) {
        fetchWorldState();
        fetchPendingChanges();
      }
    })
}

function rejectChange(id) {
  fetch(`/api/osint/changes/${id}/reject`, { method: 'POST', headers: authHeader() })
    .then(r => {
      if (r.ok) {
        fetchPendingChanges();
      }
    })
}
</script>

<style scoped>
/* ======================================= */
/*  MACRO HUB ΓÇô Premium Terminal Theme     */
/* ======================================= */

.hub-container-wrapper {
  background: #ffffff;
  min-height: 100vh;
}

/* ΓöÇΓöÇ Container ΓöÇΓöÇ */
.macro-hub-container {
  max-width: 1280px;
  margin: 0 auto;
  padding: 2rem 1.5rem;
  color: #1e293b;
}

/* ΓöÇΓöÇ Header ΓöÇΓöÇ */
.hub-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 1.5rem;
  margin-bottom: 2.5rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}

.hub-title {
  font-size: 2rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0 0 0.4rem 0;
  letter-spacing: -0.5px;
  font-family: 'Outfit', sans-serif;
  background: linear-gradient(135deg, #0f172a 0%, #334155 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.hub-subtitle {
  color: #475569;
  font-size: 0.95rem;
  margin: 0;
}

.hub-header-actions {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}

/* ΓöÇΓöÇ Buttons ΓöÇΓöÇ */
.macro-btn {
  font-weight: 600;
  border-radius: 8px;
  padding: 0.6rem 1.2rem;
  transition: all 0.2s ease;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
  outline: none;
  border: 1px solid transparent;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.9rem;
  white-space: nowrap;
}
.macro-btn:active {
  transform: scale(0.97);
}
.macro-btn-sm {
  padding: 0.35rem 0.85rem;
  font-size: 0.8rem;
}
.macro-btn-blue {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: #fff;
  border-color: rgba(255, 255, 255, 0.08);
}
.macro-btn-blue:hover {
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
  box-shadow: 0 4px 14px rgba(37,99,235,0.2);
}
.macro-btn-yellow {
  background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
  color: #0f172a;
  border-color: rgba(0, 0, 0, 0.05);
}
.macro-btn-yellow:hover {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  box-shadow: 0 4px 14px rgba(245,158,11,0.25);
}
.macro-btn-green {
  background: rgba(16, 185, 129, 0.08);
  color: #059669;
  border: 1px solid rgba(16, 185, 129, 0.2);
}
.macro-btn-green:hover {
  background: #10b981;
  color: #fff;
  box-shadow: 0 4px 14px rgba(16,185,129,0.2);
}

/* ΓöÇΓöÇ Loading ΓöÇΓöÇ */
.hub-loading {
  text-align: center;
  padding: 5rem 1rem;
}

/* ΓöÇΓöÇ Error ΓöÇΓöÇ */
.hub-error {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  background: rgba(244, 63, 94, 0.08);
  border: 1px solid rgba(244, 63, 94, 0.15);
  color: #be123c;
  padding: 1.25rem 1.5rem;
  border-radius: 10px;
  margin-bottom: 1.5rem;
}
.hub-error-icon {
  font-size: 1.5rem;
  flex-shrink: 0;
}

/* ΓöÇΓöÇ Empty state ΓöÇΓöÇ */
.hub-empty {
  text-align: center;
  padding: 4rem 1rem;
}
.hub-empty-inner {
  display: inline-block;
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 16px;
  padding: 3rem 3.5rem;
  box-shadow: 0 8px 32px rgba(0,0,0,0.05);
}
.hub-empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

/* ΓöÇΓöÇ Groups Grid ΓöÇΓöÇ */
.hub-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}
@media (min-width: 992px) {
  .hub-grid {
    grid-template-columns: 1fr 1fr;
  }
}

/* ΓöÇΓöÇ News section inside cards ΓöÇΓöÇ */
.news-section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 0.75rem;
  margin-bottom: 0.75rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}
.news-section-title {
  font-weight: 700;
  font-size: 0.82rem;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.news-list {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}
.news-empty {
  text-align: center;
  padding: 2rem 1rem;
  color: #94a3b8;
  background: rgba(0, 0, 0, 0.02);
  border-radius: 8px;
  font-size: 0.9rem;
  border: 1px dashed rgba(0, 0, 0, 0.08);
}

/* ΓöÇΓöÇ Modal ΓöÇΓöÇ */
.macro-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 1000;
  background: rgba(15, 23, 42, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  backdrop-filter: blur(4px);
  animation: fadeIn .2s ease;
}
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
.macro-modal-box {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 14px;
  box-shadow: 0 20px 60px rgba(15,23,42,0.15);
  padding: 2rem 1.75rem 1.75rem;
  max-width: 460px;
  width: 100%;
  position: relative;
  color: #0f172a;
  animation: slideUp .25s cubic-bezier(.4,0,.2,1);
}
@keyframes slideUp {
  from { transform: translateY(20px); opacity: 0; }
  to { transform: none; opacity: 1; }
}
.macro-modal-close {
  position: absolute;
  top: 12px;
  right: 14px;
  background: rgba(0, 0, 0, 0.03);
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: 50%;
  width: 34px;
  height: 34px;
  font-size: 1.2rem;
  color: #64748b;
  cursor: pointer;
  transition: all 0.15s;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: center;
}
.macro-modal-close:hover {
  background: rgba(0, 0, 0, 0.08);
  color: #0f172a;
}

/* ΓöÇΓöÇ Modal form overrides ΓöÇΓöÇ */
:deep(.macro-modal-box input),
:deep(.macro-modal-box textarea),
:deep(.macro-modal-box select) {
  border-radius: 8px !important;
  border: 1px solid rgba(0, 0, 0, 0.1) !important;
  padding: 0.65rem 1rem !important;
  font-size: 0.95rem !important;
  width: 100%;
  background: #f8fafc !important;
  color: #0f172a !important;
  transition: border-color .15s, box-shadow .15s;
}
:deep(.macro-modal-box input:focus),
:deep(.macro-modal-box textarea:focus),
:deep(.macro-modal-box select:focus) {
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 3px rgba(59,130,246,0.15) !important;
  outline: none;
  background: #ffffff !important;
  color: #0f172a !important;
}

/* ΓöÇΓöÇ Responsive ΓöÇΓöÇ */
@media (max-width: 640px) {
  .macro-hub-container {
    padding: 1rem 0.75rem;
  }
  .hub-header {
    flex-direction: column;
    gap: 1rem;
  }
  .hub-title {
    font-size: 1.4rem;
  }
  .hub-header-actions {
    width: 100%;
  }
  .hub-header-actions .macro-btn {
    flex: 1;
    justify-content: center;
  }
  .macro-modal-box {
    padding: 1.5rem 1rem 1.25rem;
  }
  .hub-empty-inner {
    padding: 2rem 1.5rem;
  }
}

/* ΓöÇΓöÇ Deep sub-components overrides ΓöÇΓöÇ */
:deep(.group-card) {
  background: #ffffff !important;
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.04) !important;
  color: #0f172a !important;
  transition: all 0.25s ease;
}
:deep(.group-card:hover) {
  border-color: rgba(0, 0, 0, 0.15) !important;
  box-shadow: 0 8px 32px rgba(59, 130, 246, 0.06) !important;
}
:deep(.group-title) {
  color: #0f172a !important;
  font-family: 'Outfit', sans-serif !important;
  font-weight: 700 !important;
}
:deep(.group-desc) {
  color: #475569 !important;
}
:deep(.group-card-footer) {
  border-top: 1px solid rgba(0, 0, 0, 0.06) !important;
}
:deep(.gc-action-btn) {
  background: rgba(0, 0, 0, 0.02) !important;
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
  color: #475569 !important;
}
:deep(.gc-action-btn:hover) {
  background: rgba(59, 130, 246, 0.08) !important;
  color: #2563eb !important;
  border-color: rgba(59, 130, 246, 0.15) !important;
}
:deep(.gc-action-danger:hover) {
  background: rgba(244, 63, 94, 0.08) !important;
  color: #e11d48 !important;
  border-color: rgba(244, 63, 94, 0.15) !important;
}
:deep(.conclusion-label) {
  color: #475569 !important;
}
:deep(.conclusion-textarea) {
  background: #f8fafc !important;
  color: #0f172a !important;
  border: 1px solid rgba(0, 0, 0, 0.1) !important;
}
:deep(.conclusion-textarea:focus) {
  background: #ffffff !important;
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15) !important;
  color: #0f172a !important;
}

:deep(.news-item) {
  background: rgba(0, 0, 0, 0.01) !important;
  border: 1px solid rgba(0, 0, 0, 0.05) !important;
  color: #0f172a !important;
  border-radius: 8px !important;
}
:deep(.news-item-title) {
  color: #0f172a !important;
}
:deep(.ni-active) {
  background: rgba(16, 185, 129, 0.08) !important;
  border-color: rgba(16, 185, 129, 0.18) !important;
  color: #065f46 !important;
}
:deep(.ni-active:hover) {
  background: rgba(16, 185, 129, 0.12) !important;
}
:deep(.ni-inactive) {
  background: rgba(148, 163, 184, 0.08) !important;
  border-color: rgba(148, 163, 184, 0.18) !important;
  color: #334155 !important;
}
:deep(.ni-inactive:hover) {
  background: rgba(148, 163, 184, 0.12) !important;
}
:deep(.ni-action-btn) {
  background: rgba(0, 0, 0, 0.02) !important;
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
  color: #475569 !important;
}
:deep(.ni-action-btn:hover) {
  background: rgba(59, 130, 246, 0.08) !important;
  color: #2563eb !important;
}
:deep(.ni-action-danger:hover) {
  background: rgba(244, 63, 94, 0.08) !important;
  color: #e11d48 !important;
}

:deep(.macro-modal-box) {
  background: #ffffff !important;
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
  color: #0f172a !important;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.1) !important;
}
:deep(.form-label), :deep(label) {
  color: #475569 !important;
  font-weight: 600 !important;
}
:deep(.macro-modal-title) {
  color: #0f172a !important;
  font-family: 'Outfit', sans-serif !important;
}
:deep(.hub-empty-inner) {
  background: #ffffff !important;
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
  color: #0f172a !important;
}
:deep(.hub-empty-inner h5) {
  color: #0f172a !important;
}
</style>
