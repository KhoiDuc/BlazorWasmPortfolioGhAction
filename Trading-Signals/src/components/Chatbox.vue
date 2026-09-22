<template>
  <div class="ai-chatbox-wrapper">
    <!-- Chat Launcher Button -->
    <button v-if="!isOpen" class="chat-launcher shadow-lg" @click="toggleChat" title="Ask AI">
      <div class="launcher-pulse"></div>
      <svg class="launcher-icon" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
      </svg>
      <span class="launcher-text">Ask AI</span>
    </button>

    <!-- Chat Container -->
    <div v-else class="chat-container shadow-2xl" :class="{ 'chat-container--mobile': isMobile, 'chat-container--maximized': isMaximized && !isMobile }">
      <!-- Chat Header -->
      <div class="chat-header">
        <div class="header-info">
          <div class="avatar-wrapper">
            <div class="bot-avatar">🤖</div>
            <span class="status-indicator"></span>
          </div>
          <div>
            <h4 class="header-title">Trợ lý AI Giao dịch</h4>
            <span class="header-status">Đang trực tuyến</span>
          </div>
        </div>
        <div class="header-actions">
          <button 
            v-if="!isMobile" 
            class="chat-maximize-btn" 
            @click="toggleMaximize" 
            :title="isMaximized ? 'Thu nhỏ cửa sổ' : 'Phóng to cửa sổ'"
          >
            <svg v-if="!isMaximized" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <path d="M15 3h6v6"></path>
              <path d="M9 21H3v-6"></path>
              <line x1="21" y1="3" x2="14" y2="10"></line>
              <line x1="3" y1="21" x2="10" y2="14"></line>
            </svg>
            <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <path d="M4 14h6v6"></path>
              <path d="M20 10h-6V4"></path>
              <line x1="14" y1="10" x2="21" y2="3"></line>
              <line x1="10" y1="14" x2="3" y2="21"></line>
            </svg>
          </button>
          <button class="chat-close-btn" @click="toggleChat" title="Thu nhỏ">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>
      </div>

      <!-- Active Context Banner -->
      <div v-if="activeContext" class="context-banner d-flex align-items-center justify-content-between p-2 px-3">
        <div class="d-flex align-items-center gap-2 overflow-hidden me-2">
          <span class="context-icon">🧠</span>
          <span class="context-text text-truncate">
            Bối cảnh: Nhận định vĩ mô & Tin tức Telegram
          </span>
        </div>
        <button class="clear-context-btn" @click="clearContext" title="Xóa bối cảnh">
          &times;
        </button>
      </div>

      <!-- Chat Body -->
      <div class="chat-body" ref="chatBody">
        <div v-for="(msg, idx) in messages" :key="idx" class="message-row" :class="`message-row--${msg.sender}`">
          <!-- Bot Avatar for AI responses -->
          <div v-if="msg.sender === 'ai'" class="message-avatar">🤖</div>
          
          <!-- Message Bubble -->
          <div class="message-bubble" :class="`message-bubble--${msg.sender}`">
            <!-- User sent multiple images -->
            <div v-if="msg.images && msg.images.length > 0" class="message-images-grid" :class="`message-images-grid--${msg.images.length}`">
              <div v-for="(img, idx) in msg.images" :key="idx" class="message-image-wrapper">
                <img :src="img" class="message-image" alt="Hình ảnh gửi kèm" />
              </div>
            </div>
            <!-- Backwards compatibility for single image -->
            <div v-else-if="msg.image" class="message-image-container">
              <img :src="msg.image" class="message-image" alt="Hình ảnh gửi lên" />
            </div>

            <div v-if="msg.text" class="message-text" v-html="formatMessageText(msg.text)"></div>
            
            <!-- Dynamic interactive fallback actions -->
            <div v-if="msg.isFallback" class="fallback-actions">
              <button class="fallback-btn fallback-btn--confirm" @click="confirmGroq(msg.originalPrompt)">
                🚀 Có, sử dụng Groq
              </button>
              <button class="fallback-btn fallback-btn--cancel" @click="cancelGroq">
                Hủy
              </button>
            </div>
            
            <span class="message-time">{{ msg.time }}</span>
          </div>
        </div>

        <!-- Typing Indicator -->
        <div v-if="isLoading" class="message-row message-row--ai">
          <div class="message-avatar">🤖</div>
          <div class="message-bubble message-bubble--ai message-bubble--typing">
            <div class="typing-indicator">
              <span></span>
              <span></span>
              <span></span>
            </div>
          </div>
        </div>
      </div>

      <!-- Image Preview Area -->
      <div v-if="selectedImages.length > 0" class="image-preview-container">
        <div v-for="(img, index) in selectedImages" :key="index" class="image-preview-wrapper">
          <img :src="img" class="image-preview" alt="Xem trước ảnh" />
          <button type="button" class="remove-image-btn" @click="removeImage(index)" title="Xóa hình ảnh">
            <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>
      </div>

      <!-- Chat Footer / Input -->
      <form class="chat-footer" @submit.prevent="sendMessage(newMessage, false)">
        <!-- Hidden file input -->
        <input 
          type="file" 
          ref="fileInput" 
          accept="image/*" 
          style="display: none" 
          @change="onImageSelected" 
          multiple
        />
        
        <!-- Image select button -->
        <button 
          type="button" 
          class="chat-image-btn" 
          @click="triggerFileInput" 
          title="Chọn hình ảnh" 
          :disabled="isLoading"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
            <circle cx="8.5" cy="8.5" r="1.5"></circle>
            <polyline points="21 15 16 10 5 21"></polyline>
          </svg>
        </button>

        <input 
          v-model="newMessage" 
          type="text" 
          placeholder="Nhập tin nhắn giao dịch..." 
          :disabled="isLoading" 
          class="chat-input"
          ref="inputField"
          @paste="onPaste"
        />
        <button type="submit" class="chat-send-btn" :disabled="isLoading || (!newMessage.trim() && selectedImages.length === 0)">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="22" y1="2" x2="11" y2="13"></line>
            <polygon points="22 2 15 22 11 13 2 9 22 2"></polygon>
          </svg>
        </button>
      </form>
    </div>
  </div>
</template>

<script>
/* eslint-disable vue/multi-word-component-names */
import { ref, onMounted, onUnmounted, nextTick } from 'vue';
import { parseMarkdown } from '@/utils/markdown';

export default {
  name: 'AiChatbox',
  setup() {
    const isOpen = ref(false);
    const activeContext = ref(null);
    const messages = ref([
      {
        sender: 'ai',
        text: 'Xin chào! Tôi là Trợ lý AI Giao dịch. Bạn cần tôi hỗ trợ gì về tin tức, phân tích thị trường hoặc quản lý danh mục đầu tư hôm nay?',
        time: getCurrentTime()
      }
    ]);
    const newMessage = ref('');
    const isLoading = ref(false);
    const chatBody = ref(null);
    const isMobile = ref(false);
    const isMaximized = ref(false);
    const inputField = ref(null);

    // Image upload state variables (Supports multiple images)
    const selectedImages = ref([]);
    const fileInput = ref(null);

    function getCurrentTime() {
      const now = new Date();
      return now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }

    function toggleChat() {
      isOpen.value = !isOpen.value;
      if (isOpen.value) {
        checkMobile();
        scrollToBottom();
        nextTick(() => {
          if (inputField.value) inputField.value.focus();
        });
      }
    }

    function checkMobile() {
      isMobile.value = window.innerWidth <= 768;
    }

    function toggleMaximize() {
      isMaximized.value = !isMaximized.value;
      scrollToBottom();
    }

    function scrollToBottom() {
      nextTick(() => {
        if (chatBody.value) {
          chatBody.value.scrollTop = chatBody.value.scrollHeight;
        }
      });
    }

    function triggerFileInput() {
      if (fileInput.value) {
        fileInput.value.click();
      }
    }

    function removeImage(index) {
      selectedImages.value.splice(index, 1);
    }

    function clearAllImages() {
      selectedImages.value = [];
      if (fileInput.value) {
        fileInput.value.value = '';
      }
    }

    const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB limit
    const MAX_IMAGES = 5; // Allow max 5 images at once

    function handleImageFile(file) {
      if (!file) return;
      if (!file.type.startsWith('image/')) {
        notify({ type: 'error', title: 'Error', text: 'Vui lòng chỉ chọn tệp hình ảnh!' });
        return;
      }
      if (file.size > MAX_FILE_SIZE) {
        notify({ type: 'error', title: 'Error', text: `Dung lượng ảnh "${file.name}" vượt quá 5MB. Vui lòng chọn ảnh nhẹ hơn!` });
        return;
      }
      if (selectedImages.value.length >= MAX_IMAGES) {
        notify({ type: 'error', title: 'Error', text: `Bạn chỉ có thể gửi tối đa ${MAX_IMAGES} hình ảnh cùng lúc!` });
        return;
      }

      const reader = new FileReader();
      reader.onload = (e) => {
        selectedImages.value.push(e.target.result);
      };
      reader.readAsDataURL(file);
    }

    function onImageSelected(event) {
      const files = event.target.files;
      if (files) {
        for (let i = 0; i < files.length; i++) {
          handleImageFile(files[i]);
        }
      }
      if (fileInput.value) fileInput.value.value = '';
    }

    function onPaste(event) {
      const items = (event.clipboardData || event.originalEvent.clipboardData).items;
      let hasImage = false;
      for (const item of items) {
        if (item.type.indexOf('image') !== -1) {
          const file = item.getAsFile();
          handleImageFile(file);
          hasImage = true;
        }
      }
      if (hasImage) {
        event.preventDefault();
      }
    }

    async function sendMessage(text, useGroq = false) {
      const userText = text ? text.trim() : '';
      const userImages = [...selectedImages.value];

      if (!userText && userImages.length === 0) return;

      // Clear current form inputs immediately
      newMessage.value = '';
      clearAllImages();

      if (!useGroq) {
        // Only append user message if not retrying Groq fallback
        messages.value.push({
          sender: 'user',
          text: userText,
          images: userImages,
          time: getCurrentTime()
        });
      }

      isLoading.value = true;
      scrollToBottom();

      try {
        const payload = {
          message: userText,
          use_groq: useGroq,
          images: userImages
        };

        if (activeContext.value) {
          payload.thesis_context = `Nhận định: ${activeContext.value.thesis}\nTư vấn: ${activeContext.value.advice}`;
          payload.telegram_context = activeContext.value.telegramContext;
        }

        const response = await fetch('/api/chat', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(payload)
        });

        if (!response.ok) {
          throw new Error('Server returned non-OK status');
        }

        const data = await response.json();

        if (data.gemini_failed) {
          // Gemini failed - show interactive fallback choice
          messages.value.push({
            sender: 'ai',
            text: 'Hệ thống Gemini hiện tại không khả dụng. Bạn có muốn chuyển sang sử dụng Groq (có tính phí) để tiếp tục không?',
            time: getCurrentTime(),
            isFallback: true,
            originalPrompt: userText
          });
        } else {
          // Success
          messages.value.push({
            sender: 'ai',
            text: data.response || 'Tôi không nhận được phản hồi phù hợp từ AI.',
            time: getCurrentTime()
          });
        }
      } catch (err) {
        console.error('AI Chatbox Error:', err);
        messages.value.push({
          sender: 'ai',
          text: 'Rất tiếc, đã có lỗi xảy ra khi kết nối tới trợ lý AI. Vui lòng thử lại sau ít phút.',
          time: getCurrentTime()
        });
      } finally {
        isLoading.value = false;
        scrollToBottom();
      }
    }

    function confirmGroq(originalPrompt) {
      // Remove the fallback selection prompt from history for cleanliness
      messages.value = messages.value.filter(m => !m.isFallback);
      
      // Let the user know we are retrying via Groq
      messages.value.push({
        sender: 'ai',
        text: '🔄 Đang chuyển hướng yêu cầu sang hệ thống Groq...',
        time: getCurrentTime()
      });
      
      sendMessage(originalPrompt, true);
    }

    function cancelGroq() {
      // Remove fallback option
      messages.value = messages.value.filter(m => !m.isFallback);
      messages.value.push({
        sender: 'ai',
        text: '❌ Yêu cầu phân tích đã được hủy bỏ.',
        time: getCurrentTime()
      });
      scrollToBottom();
    }

    function formatMessageText(text) {
      return parseMarkdown(text);
    }

    const handleOpenChatWithContext = (event) => {
      const { thesis, advice, telegramContext } = event.detail;
      activeContext.value = { thesis, advice, telegramContext };
      isOpen.value = true;
      newMessage.value = `Hãy phân tích sâu hơn về nhận định vĩ mô này dựa trên các tin tức Telegram mới nhất.`;
      
      nextTick(() => {
        if (inputField.value) inputField.value.focus();
        scrollToBottom();
      });
    };

    const clearContext = () => {
      activeContext.value = null;
    };

    onMounted(() => {
      checkMobile();
      window.addEventListener('resize', checkMobile);
      window.addEventListener('open-chat-with-context', handleOpenChatWithContext);
    });

    onUnmounted(() => {
      window.removeEventListener('resize', checkMobile);
      window.removeEventListener('open-chat-with-context', handleOpenChatWithContext);
    });

    return {
      isOpen,
      messages,
      newMessage,
      isLoading,
      chatBody,
      isMobile,
      isMaximized,
      inputField,
      selectedImages,
      fileInput,
      toggleChat,
      toggleMaximize,
      triggerFileInput,
      removeImage,
      onImageSelected,
      onPaste,
      sendMessage,
      confirmGroq,
      cancelGroq,
      formatMessageText,
      activeContext,
      clearContext
    };
  }
};
</script>

<style scoped>
/* Wrapper and Floating Button */
.ai-chatbox-wrapper {
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  position: fixed;
  bottom: 24px;
  right: 24px;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.chat-launcher {
  position: relative;
  width: 120px;
  height: 52px;
  border-radius: 26px;
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0 16px;
  font-weight: 600;
  font-size: 0.95rem;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.chat-launcher:hover {
  transform: translateY(-4px) scale(1.05);
  box-shadow: 0 12px 24px rgba(16, 185, 129, 0.4);
}

.launcher-pulse {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  border-radius: 26px;
  border: 2px solid #10b981;
  animation: launcher-pulse-animation 2s infinite;
  opacity: 0;
  pointer-events: none;
}

@keyframes launcher-pulse-animation {
  0% {
    transform: scale(1);
    opacity: 0.8;
  }
  100% {
    transform: scale(1.25);
    opacity: 0;
  }
}

.launcher-icon {
  flex-shrink: 0;
}

.launcher-text {
  letter-spacing: 0.2px;
}

/* Chat Panel Container */
.chat-container {
  display: flex;
  flex-direction: column;
  width: 375px;
  height: 550px;
  background-color: #ffffff;
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.08);
  animation: slide-in 0.35s cubic-bezier(0.16, 1, 0.3, 1);
  transform-origin: bottom right;
  transition: width 0.3s cubic-bezier(0.25, 0.8, 0.25, 1), height 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.chat-container--maximized {
  width: 800px !important;
  height: 750px !important;
  max-width: calc(100vw - 48px);
  max-height: calc(100vh - 48px);
  border-radius: 20px;
}

.chat-container--mobile {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  width: 100% !important;
  height: 100% !important;
  border-radius: 0;
  border: none;
}

@keyframes slide-in {
  0% {
    transform: scale(0.85);
    opacity: 0;
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

/* Header */
.chat-header {
  background: linear-gradient(135deg, #00b050 0%, #008a3d 100%);
  color: white;
  padding: 16px 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.header-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.avatar-wrapper {
  position: relative;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.3rem;
  border: 2px solid rgba(255, 255, 255, 0.3);
}

.bot-avatar {
  line-height: 1;
}

.status-indicator {
  position: absolute;
  bottom: -2px;
  right: -2px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background-color: #10b981;
  border: 2.5px solid #00b050;
  box-shadow: 0 0 8px #10b981;
}

.header-title {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 700;
  letter-spacing: 0.3px;
}

.header-status {
  font-size: 0.75rem;
  opacity: 0.85;
  display: flex;
  align-items: center;
  gap: 4px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.chat-maximize-btn {
  background: none;
  border: none;
  color: rgba(255, 255, 255, 0.85);
  cursor: pointer;
  padding: 6px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.chat-maximize-btn:hover {
  background: rgba(255, 255, 255, 0.15);
  color: white;
  transform: scale(1.1);
}

.chat-close-btn {
  background: none;
  border: none;
  color: rgba(255, 255, 255, 0.85);
  cursor: pointer;
  padding: 6px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.chat-close-btn:hover {
  background: rgba(255, 255, 255, 0.15);
  color: white;
  transform: rotate(90deg);
}

/* Body / Messages */
.chat-body {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
  background-color: #f8fafc;
  display: flex;
  flex-direction: column;
  gap: 16px;
  scroll-behavior: smooth;
}

.message-row {
  display: flex;
  align-items: flex-end;
  gap: 8px;
  animation: msg-appear 0.25s ease-out forwards;
}

.message-row--ai {
  align-self: flex-start;
  max-width: 85%;
}

.message-row--user {
  align-self: flex-end;
  max-width: 85%;
  flex-direction: row-reverse;
}

@keyframes msg-appear {
  0% {
    opacity: 0;
    transform: translateY(8px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
  }
}

.message-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background-color: #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
  border: 1px solid rgba(0, 0, 0, 0.05);
  flex-shrink: 0;
}

.message-bubble {
  position: relative;
  padding: 12px 16px;
  border-radius: 18px;
  font-size: 0.9rem;
  line-height: 1.45;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
}

.message-bubble--ai {
  background-color: #ffffff;
  color: #1e293b;
  border-bottom-left-radius: 4px;
  border: 1px solid rgba(0, 0, 0, 0.04);
}

.message-bubble--user {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  border-bottom-right-radius: 4px;
  box-shadow: 0 4px 10px rgba(16, 185, 129, 0.15);
}

.message-text {
  word-break: break-word;
}

.message-time {
  display: block;
  font-size: 0.68rem;
  margin-top: 4px;
  opacity: 0.6;
  text-align: right;
}

.message-bubble--user .message-time {
  color: rgba(255, 255, 255, 0.85);
}

/* Fallback Selection Layout */
.fallback-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 12px;
  border-top: 1px dashed rgba(0, 0, 0, 0.08);
  padding-top: 10px;
}

.fallback-btn {
  padding: 8px 12px;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 600;
  border: none;
  cursor: pointer;
  transition: all 0.2s ease;
  width: 100%;
  text-align: center;
}

.fallback-btn--confirm {
  background-color: #10b981;
  color: white;
  box-shadow: 0 2px 4px rgba(16, 185, 129, 0.2);
}

.fallback-btn--confirm:hover {
  background-color: #059669;
  transform: translateY(-1px);
}

.fallback-btn--cancel {
  background-color: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.fallback-btn--cancel:hover {
  background-color: #e2e8f0;
  color: #475569;
}

/* Typing Indicator Animation */
.message-bubble--typing {
  padding: 12px 18px;
}

.typing-indicator {
  display: flex;
  align-items: center;
  gap: 4px;
  height: 12px;
}

.typing-indicator span {
  display: inline-block;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background-color: #64748b;
  animation: typing-bounce 1.4s infinite ease-in-out both;
}

.typing-indicator span:nth-child(1) {
  animation-delay: -0.32s;
}

.typing-indicator span:nth-child(2) {
  animation-delay: -0.16s;
}

@keyframes typing-bounce {
  0%, 80%, 100% { 
    transform: scale(0);
  } 
  40% { 
    transform: scale(1);
  }
}

.image-preview-container {
  background-color: #ffffff;
  padding: 10px 16px;
  border-top: 1px solid #f1f5f9;
  display: flex;
  align-items: center;
  gap: 12px;
  overflow-x: auto;
  animation: preview-slide-in 0.25s ease-out;
}

@keyframes preview-slide-in {
  from { transform: translateY(10px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.image-preview-wrapper {
  position: relative;
  width: 56px;
  height: 56px;
  border-radius: 8px;
  border: 1.5px solid #e2e8f0;
  overflow: visible;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
  flex-shrink: 0;
}

.image-preview {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 6px;
}

.remove-image-btn {
  position: absolute;
  top: -6px;
  right: -6px;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background-color: #ef4444;
  color: white;
  border: 1.5px solid #ffffff;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 4px rgba(239, 68, 68, 0.3);
  transition: all 0.2s ease;
}

.remove-image-btn:hover {
  background-color: #dc2626;
  transform: scale(1.1);
}

.chat-image-btn {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background-color: #f8fafc;
  color: #64748b;
  border: 1.5px solid #e2e8f0;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.chat-image-btn:hover:not(:disabled) {
  background-color: #f1f5f9;
  color: #0f172a;
  border-color: #cbd5e1;
}

.chat-image-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Multi-Image Grid inside User Message */
.message-images-grid {
  display: grid;
  gap: 6px;
  margin-top: 2px;
  margin-bottom: 8px;
  max-width: 100%;
}

.message-images-grid--2 {
  grid-template-columns: repeat(2, 1fr);
  width: 240px;
}

.message-images-grid--3, .message-images-grid--4, .message-images-grid--5 {
  grid-template-columns: repeat(3, 1fr);
  width: 280px;
}

.message-images-grid .message-image-wrapper {
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  aspect-ratio: 1 / 1;
}

.message-images-grid .message-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

/* For Single/Legacy Image container */
.message-image-container {
  margin-top: 2px;
  margin-bottom: 8px;
  border-radius: 12px;
  overflow: hidden;
  max-width: 100%;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.message-image {
  display: block;
  max-width: 100%;
  max-height: 220px;
  object-fit: contain;
  border-radius: 8px;
  background-color: rgba(0, 0, 0, 0.02);
}

/* Footer / Input form */
.chat-footer {
  background-color: #ffffff;
  padding: 12px 16px;
  display: flex;
  align-items: center;
  gap: 10px;
  border-top: 1px solid #f1f5f9;
}

.chat-input {
  flex: 1;
  border: 1.5px solid #e2e8f0;
  border-radius: 20px;
  padding: 10px 16px;
  font-size: 0.9rem;
  color: #1e293b;
  outline: none;
  background-color: #f8fafc;
  transition: all 0.2s ease;
}

.chat-input:focus {
  border-color: #10b981;
  background-color: #ffffff;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.1);
}

.chat-input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.chat-send-btn {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background-color: #f1f5f9;
  color: #64748b;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.chat-send-btn:hover:not(:disabled) {
  background-color: #10b981;
  color: white;
  transform: scale(1.05);
}

.chat-send-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Code Syntax Formatting inside messages */
:deep(.chatbox-code-block) {
  background-color: #0f172a !important;
  color: #e2e8f0 !important;
  padding: 10px 14px !important;
  border-radius: 8px !important;
  font-family: SFMono-Regular, Consolas, "Liberation Mono", Menlo, monospace !important;
  font-size: 0.8rem !important;
  margin: 6px 0 !important;
  overflow-x: auto !important;
  white-space: pre !important;
}

:deep(.chatbox-code-inline) {
  background-color: #f1f5f9 !important;
  color: #ef4444 !important;
  padding: 2px 5px !important;
  border-radius: 4px !important;
  font-family: SFMono-Regular, Consolas, "Liberation Mono", Menlo, monospace !important;
  font-size: 0.8rem !important;
}

/* AI Content Premium Styles inside Chatbox */
:deep(.ai-header-h3) {
  font-size: 1.1rem !important;
  color: #0f172a !important;
  font-weight: 700 !important;
  margin-top: 1rem !important;
  margin-bottom: 0.5rem !important;
  padding-left: 0.5rem !important;
  border-left: 3.5px solid #3b82f6 !important;
}

:deep(.ai-header-h4) {
  font-size: 1.0rem !important;
  color: #1e293b !important;
  font-weight: 600 !important;
  margin-top: 0.75rem !important;
  margin-bottom: 0.4rem !important;
  padding-left: 0.4rem !important;
  border-left: 3px solid #10b981 !important;
}

:deep(.ai-header-h5) {
  font-size: 0.95rem !important;
  color: #334155 !important;
  font-weight: 600 !important;
  margin-top: 0.6rem !important;
  margin-bottom: 0.3rem !important;
  padding-left: 0.4rem !important;
  border-left: 3px solid #f59e0b !important;
}

:deep(.ai-header-h6) {
  font-size: 0.9rem !important;
  color: #475569 !important;
  font-weight: 600 !important;
  margin-top: 0.5rem !important;
  margin-bottom: 0.25rem !important;
  padding-left: 0.35rem !important;
  border-left: 2.5px solid #8b5cf6 !important;
}

:deep(.ai-hr) {
  border: 0 !important;
  height: 1px !important;
  background: linear-gradient(to right, rgba(226, 232, 240, 0), rgba(226, 232, 240, 1), rgba(226, 232, 240, 0)) !important;
  margin: 1rem 0 !important;
}

:deep(.ai-list-item) {
  list-style: none !important;
  position: relative !important;
  padding-left: 1rem !important;
  margin-bottom: 0.3rem !important;
  line-height: 1.5 !important;
}

:deep(.ai-list-item::before) {
  content: '' !important;
  position: absolute !important;
  left: 0.15rem !important;
  top: 0.5rem !important;
  width: 5px !important;
  height: 5px !important;
  background-color: #10b981 !important;
  border-radius: 50% !important;
  box-shadow: 0 0 6px rgba(16, 185, 129, 0.6) !important;
}

/* Chatbox Tables Styling */
:deep(.table-responsive) {
  border-radius: 8px !important;
  overflow: hidden !important;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.02) !important;
  border: 1px solid #e2e8f0 !important;
  margin: 0.75rem 0 !important;
}

:deep(.custom-ai-table) {
  width: 100% !important;
  margin-bottom: 0 !important;
  border-collapse: collapse !important;
  font-size: 0.8rem !important;
  background-color: #ffffff !important;
}

:deep(.custom-ai-table th) {
  background: #1e293b !important;
  color: #ffffff !important;
  font-weight: 600 !important;
  text-transform: uppercase !important;
  font-size: 0.7rem !important;
  letter-spacing: 0.03em !important;
  padding: 8px 10px !important;
  border: none !important;
  text-align: left !important;
}

:deep(.custom-ai-table td) {
  padding: 8px 10px !important;
  color: #475569 !important;
  border-bottom: 1px solid #f1f5f9 !important;
  transition: background-color 0.15s ease !important;
  text-align: left !important;
}

:deep(.custom-ai-table tr:last-child td) {
  border-bottom: none !important;
}

:deep(.custom-ai-table tr:nth-child(even) td) {
  background-color: #f8fafc !important;
}

:deep(.custom-ai-table tr:hover td) {
  background-color: rgba(59, 130, 246, 0.04) !important;
  color: #0f172a !important;
}

/* Fallback/Legacy code styling compatibility */
:deep(.custom-code-inline) {
  background-color: #f1f5f9 !important;
  color: #ef4444 !important;
  padding: 2px 5px !important;
  border-radius: 4px !important;
  font-family: SFMono-Regular, Consolas, "Liberation Mono", Menlo, monospace !important;
  font-size: 0.8rem !important;
}

:deep(.custom-code-block) {
  background-color: #0f172a !important;
  color: #e2e8f0 !important;
  padding: 10px 14px !important;
  border-radius: 8px !important;
  font-family: SFMono-Regular, Consolas, "Liberation Mono", Menlo, monospace !important;
  font-size: 0.8rem !important;
  margin: 6px 0 !important;
  overflow-x: auto !important;
  white-space: pre !important;
}

/* Context Banner Styles */
.context-banner {
  background: linear-gradient(135deg, #f1f5f9 0%, #e2e8f0 100%);
  border-bottom: 1px solid #cbd5e1;
  display: flex;
  justify-content: space-between;
  align-items: center;
  animation: slide-down 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  z-index: 10;
  width: 100%;
}
.context-icon {
  font-size: 1rem;
  flex-shrink: 0;
}
.context-text {
  font-size: 0.78rem;
  font-weight: 600;
  color: #475569;
  letter-spacing: 0.2px;
}
.clear-context-btn {
  background: none;
  border: none;
  color: #94a3b8;
  font-size: 1.3rem;
  cursor: pointer;
  line-height: 1;
  padding: 0 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  border-radius: 50%;
  width: 20px;
  height: 20px;
}
.clear-context-btn:hover {
  color: #ef4444;
  background-color: rgba(239, 68, 68, 0.08);
}
@keyframes slide-down {
  from {
    transform: translateY(-10px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}
</style>
