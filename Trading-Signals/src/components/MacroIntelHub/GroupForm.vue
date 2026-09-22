<template>
  <form @submit.prevent="$emit('submit', group)" class="group-form">
    <div class="form-header">
      <h2>📋 Tạo/Sửa Nhóm Sự Kiện</h2>
      <p>Tổng chức tin tức vĩ mô theo nhóm để dễ quản lý</p>
    </div>

    <div class="form-group">
      <label class="form-label">Tên nhóm <span class="required">*</span></label>
      <input v-model="group.name" required placeholder="VD: FED, Địa chính trị, Thị trường lao động" class="form-input" />
    </div>

    <div class="form-group">
      <label class="form-label">Mô tả</label>
      <textarea v-model="group.description" placeholder="Mô tả chi tiết về nhóm sự kiện này" class="form-textarea" rows="3"></textarea>
    </div>

    <div class="form-actions">
      <button type="submit" class="form-btn submit-btn">💾 Lưu</button>
      <button type="button" @click="$emit('cancel')" class="form-btn cancel-btn">Hủy</button>
    </div>
  </form>
</template>

<script setup>
import { reactive, watch } from 'vue'

defineEmits(['submit', 'cancel'])

const props = defineProps({
  modelValue: Object
})
const group = reactive({
  name: '',
  description: ''
})
watch(() => props.modelValue, val => {
  if (val) Object.assign(group, val)
}, { immediate: true })
</script>

<style scoped>
.group-form {
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
}
</style>
