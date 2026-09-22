<template>
  <div class="ai-settings-section">
    <!-- AI Status Bar -->
    <div class="ai-status-bar">
      <div class="ai-status-left">
        <span class="ai-status-icon">🤖</span>
        <span class="ai-status-label">Platform Intelligence</span>
        <span :class="['ai-status-badge', aiEnabled ? 'enabled' : 'disabled']">
          {{ aiEnabled ? 'ON' : 'OFF' }}
        </span>
      </div>
      <div class="ai-status-right">
        <label class="ai-toggle-label">
          <span class="ai-toggle-text">AI Auto-Analysis</span>
          <div class="ai-toggle-switch" @click="toggleAI" :class="{ active: aiEnabled }">
            <div class="ai-toggle-thumb"></div>
          </div>
        </label>
        <button @click="openPromptEditor" class="ai-prompt-btn" title="Sửa Prompt Template">
          ✏️ Edit Prompt
        </button>
      </div>
    </div>

    <!-- Prompt Editor Modal -->
    <div v-if="showModal" class="ai-modal-overlay" @click.self="showModal = false">
      <div class="ai-modal-box">
        <div class="ai-modal-header">
          <h3 class="ai-modal-title">🤖 AI Prompt Template</h3>
          <button @click="showModal = false" class="ai-modal-close">✕</button>
        </div>
        <div class="ai-modal-body">
          <p class="ai-modal-desc">
            Tùy chỉnh prompt gửi cho AI. Sử dụng <code>{{NEWS_ITEMS}}</code> làm placeholder cho danh sách tin tức.
          </p>
          <textarea
            v-model="promptText"
            class="ai-prompt-textarea"
            rows="12"
            placeholder="Nhập prompt template cho AI..."
          ></textarea>
        </div>
        <div class="ai-modal-footer">
          <button @click="resetToDefault" class="ai-btn ai-btn-secondary">↺ Reset Default</button>
          <button @click="savePrompt" :disabled="saving" class="ai-btn ai-btn-primary">
            {{ saving ? 'Đang lưu...' : '💾 Lưu Prompt' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const aiEnabled = ref(true)
const promptText = ref('')
const showModal = ref(false)
const saving = ref(false)

const defaultPrompt = `Bạn là chuyên gia phân tích vĩ mô. Hãy xác thực các sự kiện kinh tế vĩ mô đang diễn ra dưới đây là đúng hay sai, đã kết hạn (đã xảy ra) hay chưa, đưa ra các tin liên quan (nếu có) và phân tích tác động của chúng:
{{NEWS_ITEMS}}

Yêu cầu: Phân tích tác động chéo, dòng tiền (flow of funds) và đưa ra nhận định cho Vàng, USD (DXY), lợi suất trái phiếu, Crypto, Chứng khoán Mỹ, Chứng khoán Việt Nam (các nhóm ngành hưởng lợi), bất động sản Việt Nam. Trình bày dưới dạng Bullet points, súc tích, đi thẳng vào vấn đề. Nếu có sự phân kỳ (Divergence) giữa tin tức và biểu đồ kỹ thuật (giả định), hãy đưa ra cảnh báo cho nhà đầu tư. Nếu có tin tức nào quan trọng nhưng chưa xuất hiện trong danh sách trên, hãy bổ sung vào phân tích. Nếu có tiền, tôi nên để vào đâu lúc này?`

function authHeader() {
  const token = localStorage.getItem('token')
  return token ? { 'Authorization': `Bearer ${token}` } : {}
}

async function fetchSettings() {
  try {
    const r = await fetch('/api/settings', { headers: authHeader() })
    if (!r.ok) throw new Error('Failed to fetch settings')
    const data = await r.json()
    // data is map[string]string where key -> value
    if (data.ai_enabled !== undefined) {
      aiEnabled.value = data.ai_enabled === 'true'
    }
    if (data.ai_prompt_template !== undefined && data.ai_prompt_template) {
      promptText.value = data.ai_prompt_template
    } else {
      promptText.value = defaultPrompt
    }
  } catch (e) {
    console.error('fetchSettings error:', e)
  }
}

async function toggleAI() {
  aiEnabled.value = !aiEnabled.value
  try {
    await fetch('/api/settings/update', {
      method: 'POST',
      headers: { ...authHeader(), 'Content-Type': 'application/json' },
      body: JSON.stringify({ key: 'ai_enabled', value: aiEnabled.value ? 'true' : 'false' })
    })
  } catch (e) {
    console.error('toggleAI error:', e)
    aiEnabled.value = !aiEnabled.value
  }
}

function openPromptEditor() {
  showModal.value = true
}

function resetToDefault() {
  promptText.value = defaultPrompt
}

async function savePrompt() {
  saving.value = true
  try {
    await fetch('/api/settings/update', {
      method: 'POST',
      headers: { ...authHeader(), 'Content-Type': 'application/json' },
      body: JSON.stringify({ key: 'ai_prompt_template', value: promptText.value })
    })
    showModal.value = false
  } catch (e) {
    console.error('savePrompt error:', e)
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  fetchSettings()
})
</script>

<style scoped>
.ai-settings-section {
  margin-bottom: 1.5rem;
}

.ai-status-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: #f8fafc;
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 10px;
}

.ai-status-left {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.ai-status-icon {
  font-size: 1.2rem;
}

.ai-status-label {
  font-weight: 600;
  font-size: 0.85rem;
  color: #334155;
}

.ai-status-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.ai-status-badge.enabled {
  background: rgba(16, 185, 129, 0.12);
  color: #059669;
  border: 1px solid rgba(16, 185, 129, 0.2);
}

.ai-status-badge.disabled {
  background: rgba(244, 63, 94, 0.1);
  color: #e11d48;
  border: 1px solid rgba(244, 63, 94, 0.2);
}

.ai-status-right {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.ai-toggle-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  user-select: none;
}

.ai-toggle-text {
  font-size: 0.8rem;
  color: #475569;
  font-weight: 500;
}

.ai-toggle-switch {
  width: 40px;
  height: 22px;
  background: #cbd5e1;
  border-radius: 11px;
  position: relative;
  transition: background 0.25s ease;
  cursor: pointer;
}

.ai-toggle-switch.active {
  background: #10b981;
}

.ai-toggle-thumb {
  width: 18px;
  height: 18px;
  background: #fff;
  border-radius: 50%;
  position: absolute;
  top: 2px;
  left: 2px;
  transition: transform 0.25s ease;
  box-shadow: 0 1px 3px rgba(0,0,0,0.15);
}

.ai-toggle-switch.active .ai-toggle-thumb {
  transform: translateX(18px);
}

.ai-prompt-btn {
  font-size: 0.8rem;
  padding: 0.35rem 0.75rem;
  border-radius: 6px;
  border: 1px solid rgba(0, 0, 0, 0.1);
  background: #fff;
  color: #475569;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
  white-space: nowrap;
}

.ai-prompt-btn:hover {
  border-color: #3b82f6;
  color: #2563eb;
  background: rgba(59, 130, 246, 0.05);
}

/* Modal */
.ai-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 2000;
  background: rgba(15, 23, 42, 0.5);
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

.ai-modal-box {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 14px;
  box-shadow: 0 20px 60px rgba(15,23,42,0.2);
  max-width: 680px;
  width: 100%;
  animation: slideUp .25s cubic-bezier(.4,0,.2,1);
}

@keyframes slideUp {
  from { transform: translateY(20px); opacity: 0; }
  to { transform: none; opacity: 1; }
}

.ai-modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
}

.ai-modal-title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  color: #0f172a;
}

.ai-modal-close {
  background: none;
  border: none;
  font-size: 1.3rem;
  color: #94a3b8;
  cursor: pointer;
  padding: 0.25rem;
  line-height: 1;
  border-radius: 4px;
  transition: all 0.15s;
}

.ai-modal-close:hover {
  color: #0f172a;
  background: rgba(0, 0, 0, 0.05);
}

.ai-modal-body {
  padding: 1.25rem 1.5rem;
}

.ai-modal-desc {
  font-size: 0.85rem;
  color: #64748b;
  margin: 0 0 1rem;
  line-height: 1.5;
}

.ai-modal-desc code {
  background: rgba(59, 130, 246, 0.08);
  color: #2563eb;
  padding: 0.15rem 0.4rem;
  border-radius: 4px;
  font-size: 0.8rem;
}

.ai-prompt-textarea {
  width: 100%;
  padding: 0.75rem 1rem;
  border: 1px solid rgba(0, 0, 0, 0.12);
  border-radius: 8px;
  font-size: 0.85rem;
  font-family: 'SF Mono', 'Cascadia Code', 'Consolas', monospace;
  line-height: 1.6;
  resize: vertical;
  background: #fafbfc;
  color: #0f172a;
  transition: border-color 0.15s, box-shadow 0.15s;
}

.ai-prompt-textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.12);
  background: #fff;
}

.ai-modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 0.75rem 1.5rem 1.25rem;
}

.ai-btn {
  font-weight: 600;
  font-size: 0.85rem;
  padding: 0.55rem 1.1rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.ai-btn-primary {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: #fff;
}

.ai-btn-primary:hover {
  background: linear-gradient(135deg, #2563eb, #1d4ed8);
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
}

.ai-btn-secondary {
  background: #fff;
  color: #475569;
  border-color: rgba(0, 0, 0, 0.12);
}

.ai-btn-secondary:hover {
  background: #f1f5f9;
  border-color: rgba(0, 0, 0, 0.2);
}

@media (max-width: 640px) {
  .ai-status-bar {
    flex-direction: column;
    align-items: flex-start;
  }
  .ai-status-right {
    width: 100%;
    justify-content: space-between;
  }
  .ai-modal-box {
    max-width: 95vw;
  }
}
</style>