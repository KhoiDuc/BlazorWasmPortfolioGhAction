<template>
  <form @submit.prevent="onSubmit" class="news-form">
    <div class="form-header">
      <h2>📰 Tạo/Sửa Tin Tức</h2>
      <p>Thêm một sự kiện vĩ mô mới hoặc chỉnh sửa tin tức hiện có</p>
    </div>

    <div class="form-group">
      <label class="form-label">Tiêu đề <span class="required">*</span></label>
      <input v-model="form.title" required placeholder="VD: FED tăng lãi suất 0.25%" class="form-input" />
    </div>

    <div class="form-group">
      <label class="form-label">Nội dung <span class="required">*</span></label>
      <textarea v-model="form.content" required placeholder="Mô tả chi tiết về sự kiện" class="form-textarea" rows="3"></textarea>
    </div>

    <div class="form-group">
      <label class="form-label">Nguồn (URL)</label>
      <input v-model="form.source_url" placeholder="https://example.com" class="form-input" />
    </div>

    <div class="form-group">
      <label class="form-label">Mức độ quan trọng <span class="required">*</span></label>
      <div class="importance-selector">
        <button v-for="n in 5" :key="n" type="button" @click="form.importance = n" 
                :class="['importance-btn', { active: form.importance === n }]">
          {{ n }}⭐
        </button>
      </div>
    </div>

    <div class="form-actions">
      <button type="submit" class="form-btn submit-btn">💾 Lưu</button>
      <button type="button" @click="handleCancel" class="form-btn cancel-btn">Hủy</button>
    </div>
  </form>
</template>

<script setup>
import { reactive, watch } from 'vue'
const props = defineProps({
  modelValue: Object
})
const emit = defineEmits(['update:modelValue', 'submit', 'cancel'])
const form = reactive({
  title: '',
  content: '',
  source_url: '',
  importance: 3
})
watch(() => props.modelValue, val => {
  if (val) Object.assign(form, val)
}, { immediate: true })
function onSubmit() {
  emit('submit', { ...form })
}
function handleCancel() {
  emit('cancel')
}
</script>

<style scoped>
.news-form {
  width: 100%;
}

.form-header {
  margin-bottom: 1.5rem;
}

.form-header h2 {
  font-size: 1.25rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 0.5rem 0;
}

.form-header p {
  color: #64748b;
  font-size: 0.9rem;
  margin: 0;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-label {
  display: block;
  font-weight: 600;
  color: #334155;
  margin-bottom: 0.5rem;
  font-size: 0.95rem;
}

.required {
  color: #ef4444;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 0.75rem 1rem;
  border: 1.5px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.95rem;
  font-family: inherit;
  background: #f8fafc;
  color: #1e293b;
  transition: all 0.2s;
  outline: none;
}

.form-input:focus,
.form-textarea:focus {
  border-color: #3b82f6;
  background: white;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-input::placeholder,
.form-textarea::placeholder {
  color: #94a3b8;
}

.importance-selector {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.importance-btn {
  flex: 1;
  min-width: 60px;
  padding: 0.75rem 1rem;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  background: #f8fafc;
  color: #64748b;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  outline: none;
}

.importance-btn:hover {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

.importance-btn.active {
  border-color: #ef4444;
  background: #fee2e2;
  color: #991b1b;
  box-shadow: 0 2px 8px rgba(239, 68, 68, 0.2);
}

.form-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 1.75rem;
  padding-top: 1.25rem;
  border-top: 1px solid #e2e8f0;
}

.form-btn {
  padding: 0.6rem 1.25rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
  outline: none;
}

.submit-btn {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
}

.submit-btn:hover {
  background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
  transform: translateY(-2px);
}

.cancel-btn {
  background: #e2e8f0;
  color: #334155;
}

.cancel-btn:hover {
  background: #cbd5e1;
}

@media (max-width: 640px) {
  .form-actions {
    flex-direction: column-reverse;
  }

  .form-btn {
    width: 100%;
  }

  .importance-selector {
    gap: 0.25rem;
  }

  .importance-btn {
    min-width: 50px;
    padding: 0.6rem 0.5rem;
    font-size: 0.85rem;
  }
}
</style>
