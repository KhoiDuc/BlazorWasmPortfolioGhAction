<template>
  <div class="card mb-4 create-post-card">
    <div class="card-body">
      <h5 class="card-title mb-3">Share your thoughts</h5>
      <div class="mb-3">
        <textarea
          class="form-control"
          rows="3"
          placeholder="What's on your mind? Share strategies, news, or questions..."
          v-model="content"
        ></textarea>
      </div>
      
      <div v-if="imageUrl" class="mb-3 position-relative d-inline-block">
        <img :src="imageUrl" class="img-preview rounded" alt="Preview">
        <button 
          @click="removeImage" 
          class="btn btn-sm btn-danger position-absolute top-0 end-0 m-1 rounded-circle"
          style="width: 24px; height: 24px; padding: 0; line-height: 24px;"
        >
          &times;
        </button>
      </div>

      <div class="d-flex justify-content-between align-items-center">
        <div>
          <label class="btn btn-outline-secondary btn-sm me-2">
            <i class="fas fa-image"></i> Add Image
            <input type="file" @change="handleImageUpload" accept="image/*" class="d-none">
          </label>
        </div>
        <button 
          class="btn btn-primary" 
          @click="submitPost" 
          :disabled="!content.trim() && !imageUrl"
        >
          Post
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref } from 'vue';
import { useNotification } from '@kyvg/vue3-notification';
import communityService from '../services/communityService';

export default {
  name: 'CreatePost',
  emits: ['post-created'],
  setup(props, { emit }) {
    const { notify } = useNotification();
    const content = ref('');
    const imageUrl = ref('');
    const userInfo = ref(JSON.parse(localStorage.getItem('userInfo') || '{}'));

    const handleImageUpload = (event) => {
      const file = event.target.files[0];
      if (!file) return;

      if (file.size > 5 * 1024 * 1024) { // 5MB limit
        notify({
          type: 'error',
          title: 'File too large',
          text: 'File size too large. Max 5MB.'
        });
        return;
      }

      const reader = new FileReader();
      reader.onload = (e) => {
        imageUrl.value = e.target.result;
      };
      reader.readAsDataURL(file);
    };

    const removeImage = () => {
      imageUrl.value = '';
    };

    const submitPost = () => {
      if (!content.value.trim() && !imageUrl.value) return;

      const newPost = {
        content: content.value,
        image: imageUrl.value,
        user_id: String(userInfo.value.id || 'unknown'),
        user_name: userInfo.value.name || userInfo.value.username || 'Anonymous',
        user_code: userInfo.value.custodyCode || ''
      };

      communityService.savePost(newPost);
      
      // Reset form
      content.value = '';
      imageUrl.value = '';
      
      emit('post-created');
    };

    return {
      content,
      imageUrl,
      handleImageUpload,
      removeImage,
      submitPost
    };
  }
};
</script>

<style scoped>
.create-post-card {
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.03) !important;
  border: 1px solid rgba(0, 0, 0, 0.06) !important;
  background-color: #ffffff;
  color: #1e293b;
  border-radius: 16px !important;
  padding: 8px;
}

.card-title {
  font-family: 'Outfit', sans-serif;
  font-weight: 600;
  color: #0f172a;
}

.form-control {
  border: 1px solid #cbd5e1;
  border-radius: 12px;
  resize: none;
  padding: 12px;
  font-size: 14.5px;
  background-color: #ffffff;
  color: #0f172a;
  transition: all 0.25s ease;
}

.form-control:focus {
  background-color: #ffffff;
  color: #0f172a;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
  border-color: #3b82f6;
}

.form-control::placeholder {
  color: #64748b;
}

.btn-primary {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  border: none;
  font-weight: 600;
  border-radius: 30px;
  padding: 8px 24px;
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.2);
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(59, 130, 246, 0.3);
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
}

.btn-primary:active:not(:disabled) {
  transform: translateY(0);
}

.btn-outline-secondary {
  border-color: #cbd5e1;
  color: #475569;
  font-weight: 600;
  border-radius: 30px;
  padding: 8px 18px;
  background-color: transparent;
  transition: all 0.25s ease;
}

.btn-outline-secondary:hover {
  background-color: rgba(0, 0, 0, 0.03);
  color: #0f172a;
  border-color: #94a3b8;
}

.img-preview {
  max-width: 100%;
  max-height: 260px;
  object-fit: cover;
  border-radius: 12px;
  border: 1px solid rgba(0, 0, 0, 0.06);
}
</style>
