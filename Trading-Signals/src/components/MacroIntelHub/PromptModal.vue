<template>
  <div class="prompt-modal-overlay" @click.self="$emit('close')">
    <div class="prompt-modal-box">
      <button @click="$emit('close')" class="modal-close">✕</button>
      
      <div class="modal-header">
        <h2>🤖 AI Strategy Prompt</h2>
        <p>Sử dụng prompt này để tạo chiến lược giao dịch dựa trên dữ liệu vĩ mô</p>
      </div>

      <textarea readonly :value="prompt" class="prompt-textarea"></textarea>

      <div class="modal-actions">
        <button @click="copy" class="copy-btn" :class="{ copied }">
          {{ copied ? '✓ Đã copy!' : '📋 Copy Prompt' }}
        </button>
        <button @click="$emit('close')" class="close-btn">Đóng</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
const props = defineProps({ prompt: String })
const copied = ref(false)

function copy() {
  navigator.clipboard.writeText(props.prompt)
  copied.value = true
  setTimeout(() => copied.value = false, 2000)
}
</script>

<style scoped>
.prompt-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 2000;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  animation: fadeIn 0.2s ease-out;
}

@keyframes fadeIn {
  0% { opacity: 0; }
  100% { opacity: 1; }
}

.prompt-modal-box {
  background: white;
  border-radius: 12px;
  box-shadow: 0 16px 48px 0 rgba(0, 0, 0, 0.2);
  padding: 2rem;
  max-width: 600px;
  width: 100%;
  position: relative;
  animation: slideUp 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

@keyframes slideUp {
  0% {
    opacity: 0;
    transform: translateY(20px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-close {
  position: absolute;
  top: 1rem;
  right: 1rem;
  width: 36px;
  height: 36px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: #f8fafc;
  cursor: pointer;
  font-size: 1.1rem;
  color: #64748b;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  outline: none;
}

.modal-close:hover {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

.modal-header {
  margin-bottom: 1.5rem;
}

.modal-header h2 {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 0.5rem 0;
}

.modal-header p {
  color: #64748b;
  font-size: 0.9rem;
  margin: 0;
}

.prompt-textarea {
  width: 100%;
  height: 300px;
  padding: 1rem;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', monospace;
  font-size: 0.85rem;
  color: #1e293b;
  background: #f8fafc;
  resize: none;
  line-height: 1.6;
  margin-bottom: 1.5rem;
}

.prompt-textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.copy-btn,
.close-btn {
  padding: 0.65rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
  outline: none;
}

.copy-btn {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
}

.copy-btn:hover:not(.copied) {
  background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
  transform: translateY(-2px);
}

.copy-btn.copied {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  box-shadow: 0 2px 8px rgba(16, 185, 129, 0.2);
}

.close-btn {
  background: #e2e8f0;
  color: #334155;
}

.close-btn:hover {
  background: #cbd5e1;
}

@media (max-width: 640px) {
  .prompt-modal-box {
    padding: 1.5rem;
  }

  .modal-header h2 {
    font-size: 1.25rem;
  }

  .prompt-textarea {
    height: 200px;
    font-size: 0.8rem;
  }

  .modal-actions {
    flex-direction: column;
  }

  .copy-btn,
  .close-btn {
    width: 100%;
  }
}
</style>
