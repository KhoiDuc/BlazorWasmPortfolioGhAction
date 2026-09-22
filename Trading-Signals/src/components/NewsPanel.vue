<template>
  <div class="news-panel" v-if="isVisible">
    <!-- Sticky Glassmorphic Header -->
    <div class="news-panel-header sticky-top">
      <!-- Row 1: Title & Close Button -->
      <div class="header-top-row d-flex justify-content-between align-items-center mb-3">
        <h5 class="panel-title m-0 d-flex align-items-center gap-2">
          <svg class="title-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M19 20H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v1m2 4a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H9a2 2 0 0 1-2-2v-1" />
          </svg>
          Trading News
        </h5>
        
        <!-- Premium close button in the top right -->
        <button class="btn btn-action btn-close-custom" @click="togglePanel" title="Đóng">
          <svg class="action-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      </div>

      <!-- Row 2: Expanded Speech Control & Configuration Utilities -->
      <div class="header-actions-row d-flex align-items-center justify-content-between gap-2 mb-3">
        <!-- Expanded Speech Synthesis Button taking up maximum available room -->
        <button 
          class="btn btn-speech flex-grow-1 d-flex align-items-center justify-content-center" 
          :class="{ active: speechActive, speaking: isSpeaking }"
          @click="toggleSpeech" 
          :title="speechActive ? 'Dừng đọc tin' : 'Nghe tin mới nhất'"
        >
          <svg v-if="!speechActive" class="speech-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"></polygon>
            <path d="M19.07 4.93a10 10 0 0 1 0 14.14M15.54 8.46a5 5 0 0 1 0 7.07"></path>
          </svg>
          <svg v-else class="speech-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="4" y="4" width="16" height="16" rx="2" ry="2"></rect>
          </svg>
          <span class="speech-label ms-1">{{ speechActive ? (isSpeaking ? 'Dừng' : 'Chờ tin...') : 'Nghe tin' }}</span>
          
          <!-- Beautiful Soundwave Visualizer -->
          <div v-if="speechActive && isSpeaking" class="speech-soundwave ms-2">
            <span class="sw-bar"></span>
            <span class="sw-bar"></span>
            <span class="sw-bar"></span>
          </div>
        </button>

        <!-- Utility settings & refresh buttons grouped on the right -->
        <div class="d-flex align-items-center gap-2">
          <!-- Speech Settings Toggle Button -->
          <button 
            class="btn btn-action" 
            :class="{ 'btn-settings-active': showSpeechSettings }"
            @click="showSpeechSettings = !showSpeechSettings" 
            title="Cấu hình giọng đọc"
          >
            <svg class="action-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="3"></circle>
              <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"></path>
            </svg>
          </button>

          <!-- Refresh Button -->
          <button class="btn btn-action" @click="refreshData" title="Làm mới">
            <svg class="action-icon refresh-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M23 4v6h-6"></path>
              <path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"></path>
            </svg>
          </button>
        </div>
      </div>

      <!-- Collapsible Speech Settings Panel -->
      <div v-if="showSpeechSettings" class="speech-settings-panel p-3 mb-3 rounded-3">
        <div class="settings-title mb-2 d-flex align-items-center justify-content-between">
          <span class="small-title">CẤU HÌNH GIỌNG ĐỌC</span>
          <button class="btn-close-sm" @click="showSpeechSettings = false">&times;</button>
        </div>
        
        <div class="setting-item mb-3">
          <label class="setting-label small text-secondary d-block mb-1">Giọng đọc (Hệ thống):</label>
          <select 
            v-model="selectedVoiceURI" 
            @change="saveSpeechSettings" 
            class="form-select form-select-sm speech-select w-100"
          >
            <option v-for="voice in getVietnameseVoices()" :key="voice.voiceURI" :value="voice.voiceURI">
              {{ voice.name }}
            </option>
            <option v-if="getVietnameseVoices().length === 0" value="">
              Giọng mặc định hệ thống
            </option>
          </select>
        </div>

        <div class="setting-item">
          <div class="setting-label small text-secondary d-flex justify-content-between mb-1">
            <span>Tốc độ đọc:</span>
            <strong class="text-neon-blue">{{ speechRate }}x</strong>
          </div>
          <input 
            type="range" 
            min="0.8" 
            max="1.5" 
            step="0.05" 
            v-model.number="speechRate" 
            @change="saveSpeechSettings"
            class="form-range speech-range w-100"
          >
          <div class="range-labels d-flex justify-content-between text-secondary mt-1" style="font-size: 0.65rem;">
            <span>Chậm (0.8x)</span>
            <span>Chuẩn (1.0x)</span>
            <span>Nhanh (1.5x)</span>
          </div>
        </div>
      </div>

      <!-- Tactile Channel Selector Select Box -->
      <div class="news-tabs-wrapper">
        <select 
          :value="activeTab" 
          @change="switchTab($event.target.value)" 
          class="form-select news-channel-select"
        >
          <option 
            v-for="channel in channels" 
            :key="channel" 
            :value="channel"
          >
            {{ formatChannelName(channel) }}
          </option>
        </select>
      </div>

      <!-- Glowing Micro Auto-Refresh Progress Bar -->
      <div class="progress-bar-container">
        <div class="refresh-progress-bar" :style="{ width: (countdown / 60 * 100) + '%' }"></div>
      </div>
    </div>
    
    <!-- News Content Section -->
    <div class="news-panel-content p-3">
      <!-- Live Status Badge Row -->
      <div class="news-meta-row d-flex justify-content-between align-items-center mb-3">
        <span class="live-indicator d-flex align-items-center gap-2">
          <span class="live-dot"></span>
          <span class="live-text">Live Feed</span>
        </span>
        <span class="countdown-badge">Auto-refresh in <strong class="text-neon">{{ countdown }}s</strong></span>
      </div>

      <!-- News Items List -->
      <ul class="list-unstyled m-0">
        <li v-for="(item, index) in newsItems" :key="index" class="mb-4">
          <div ref="newsCard" class="card news-card border-0" :class="{ 'reading-border': speechActive && currentlyReadingIndex === index && activeTab === currentSpeakingTab }">
            <!-- Image Section with glowing overlay -->
            <div class="card-img-wrapper" v-if="item.imageUrl">
              <img :src="item.imageUrl" class="card-img-top" alt="News Image">
              <div class="card-img-glow"></div>
            </div>
            
            <div class="card-body p-3">
              <!-- Content description -->
              <div class="card-text text-secondary mb-3 small" :class="{'text-truncate-3': !expandedItems[index]}">
                <span v-if="!expandedItems[index]" v-html="item.truncated"></span>
                <span v-else v-html="item.description"></span>
              </div>

              <!-- Footer with time and read more -->
              <div class="card-footer-row d-flex justify-content-between align-items-center mt-2">
                <small class="news-date d-flex align-items-center gap-1">
                  <svg class="clock-icon" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" style="width: 14px; height: 14px;">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  {{ formatDate(item.date_published) }}
                </small>
                
                <div class="d-flex align-items-center gap-2">
                  <!-- Individual Speech Button -->
                  <button 
                    @click.stop="toggleArticleSpeech(item, index)"
                    class="btn btn-expand p-0 z-index-top d-inline-flex align-items-center gap-1 me-2"
                    :title="speechActive && currentlyReadingIndex === index && activeTab === currentSpeakingTab ? 'Dừng đọc' : 'Đọc tin này'"
                  >
                    <svg v-if="!(speechActive && currentlyReadingIndex === index && activeTab === currentSpeakingTab)" class="speech-icon-small" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" style="width: 14px; height: 14px;">
                      <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"></polygon>
                      <path d="M15.54 8.46a5 5 0 0 1 0 7.07" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"></path>
                    </svg>
                    <svg v-else class="speech-icon-small text-danger animate-pulse-speech" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" style="width: 14px; height: 14px;">
                      <rect x="4" y="4" width="16" height="16" rx="2" ry="2" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"></rect>
                    </svg>
                  </button>

                  <button v-if="hasLongContent(item.description)" 
                          @click.stop="toggleExpand(index)" 
                          class="btn btn-expand p-0 z-index-top d-inline-flex align-items-center gap-1">
                    <span>{{ expandedItems[index] ? 'Collapse' : 'Expand' }}</span>
                    <svg class="chevron-icon" :class="{ 'rotate-180': expandedItems[index] }" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="width: 12px; height: 12px;">
                      <polyline points="6 9 12 15 18 9"></polyline>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </li>
      </ul>
    </div>
  </div>
</template>

<script>
import { notify } from '@kyvg/vue3-notification';

export default {
  name: 'NewsPanel',
  props: {
    isVisible: Boolean,
  },
  data() {
    return {
      newsItems: [],
      countdown: 60,
      intervalId: null,
      expandedItems: [], 
      activeTab: '',
      isSpeaking: false,
      availableVoices: [],
      speechActive: false,
      channels: [],
      newsData: {},
      lastReadTitles: {},
      currentlyReadingIndex: 0,
      currentSpeakingTab: '',
      currentUtterance: null,
      
      // Playlist and sentence queue properties
      speechQueue: [],
      currentQueueIndex: 0,
      articleSentences: [],
      currentSentenceIndex: 0,

      // Speech options
      showSpeechSettings: false,
      speechRate: 1.05, // Default to a gentle speed
      selectedVoiceURI: '',
    };
  },
  created() {
    this.loadSpeechSettings();
    this.fetchData();
    this.startCountdown();
    this.loadSpeechVoices();
  },
  beforeUnmount() {
    clearInterval(this.intervalId);
    this.stopSpeech();
  },
  methods: {
    switchTab(tab) {
        this.activeTab = tab;
        this.expandedItems = []; // Reset expanded status of cards on tab changes
        this.newsItems = this.newsData[tab] || [];
    },
    formatChannelName(channel) {
      if (!channel) return '';
      const nameMap = {
        'vnwallstreet': 'VN Wall Street',
        'vnwallstreetcoin': 'VN Wall Street Coin',
        'vnws_crypto': 'VNWS Crypto',
        'news_haidang': 'News Hải Đăng',
        'vietgaptrading': 'VietGap Trading',
        'tintucvnws': 'Tin Tức VNWS',
        'ktnews24': 'KT News'
      };
      return nameMap[channel.toLowerCase()] || (channel.charAt(0).toUpperCase() + channel.slice(1));
    },
    async fetchData() {
       try {
        const res = await fetch('/api/news/telegram');
        const data = await res.json();
        
        this.channels = data.channels || [];
        this.newsData = data.news || {};
        
        for (const channel of this.channels) {
            if (this.newsData[channel]) {
                this.newsData[channel] = this.newsData[channel].map(item => {
                    const desc = item.description || '';
                    return {
                        ...item,
                        truncated: desc.substring(0, 200) + (desc.length > 200 ? '...' : ''),
                        expanded: false
                    };
                });
            }
        }

        if (this.channels.length > 0) {
            if (!this.activeTab || !this.channels.includes(this.activeTab)) {
                this.activeTab = this.channels[0];
            }
            this.newsItems = this.newsData[this.activeTab] || [];
        } else {
            this.newsItems = [];
        }
        
        // Check if there are any new articles to read in Live Listener Mode
        this.checkForNewSpeechArticles();
      } catch (error) {
        console.error('Error fetching news data:', error);
      }
    },
    refreshData() {
      this.fetchData();
      this.countdown = 60; 
    },
    startCountdown() {
      this.intervalId = setInterval(() => {
        this.countdown--;
        if (this.countdown <= 0) {
          this.refreshData();
        }
      }, 1000);
    },
    togglePanel() {
      this.$emit('toggle');
    },
    formatDate(dateString) {
      const date = new Date(dateString);
      const now = new Date();
      const diffMs = now - date;
      const diffMins = Math.floor(diffMs / 60000);
      const diffHrs = Math.floor(diffMins / 60);
      
      if (diffMins < 60) return `${diffMins}m ago`;
      if (diffHrs < 24) return `${diffHrs}h ago`;
      return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
    },
    loadSpeechVoices() {
      const updateVoices = () => {
        this.availableVoices = speechSynthesis.getVoices();
        const viVoices = this.getVietnameseVoices();
        if (viVoices.length > 0 && !this.selectedVoiceURI) {
          const savedVoice = localStorage.getItem('trading_news_selected_voice');
          this.selectedVoiceURI = savedVoice || viVoices[0].voiceURI;
        }
      };

      updateVoices();
      if ('onvoiceschanged' in speechSynthesis) {
        speechSynthesis.onvoiceschanged = updateVoices;
      }
    },
    getVietnameseVoices() {
      const voices = this.availableVoices.length > 0 ? this.availableVoices : speechSynthesis.getVoices();
      return voices.filter(voice => {
        const normalized = `${voice.lang} ${voice.name} ${voice.voiceURI}`.toLowerCase();
        return /\bvi\b|vi-vn|vi_vn|vietnamese|việt|viet/.test(normalized);
      });
    },
    getVietnameseVoice() {
      const voices = this.getVietnameseVoices();
      if (this.selectedVoiceURI) {
        const selected = voices.find(v => v.voiceURI === this.selectedVoiceURI);
        if (selected) return selected;
      }
      return voices[0] || null;
    },
    loadSpeechSettings() {
      const savedRate = localStorage.getItem('trading_news_speech_rate');
      if (savedRate) {
        this.speechRate = parseFloat(savedRate);
      }
      const savedVoice = localStorage.getItem('trading_news_selected_voice');
      if (savedVoice) {
        this.selectedVoiceURI = savedVoice;
      }
    },
    saveSpeechSettings() {
      localStorage.setItem('trading_news_speech_rate', this.speechRate.toString());
      localStorage.setItem('trading_news_selected_voice', this.selectedVoiceURI);
    },
    toggleExpand(index) {
        const expanded = !this.expandedItems[index];
        this.expandedItems[index] = expanded;
        this.expandedItems = [...this.expandedItems]; // Trigger Vue reactivity
    },
    toggleSpeech() {
      if (this.speechActive) {
        this.stopSpeech();
      } else {
        this.startSpeechQueue();
      }
    },
    toggleArticleSpeech(item, index) {
      // If we are currently reading this exact article in the active tab, stop speech.
      if (this.speechActive && this.currentlyReadingIndex === index && this.activeTab === this.currentSpeakingTab) {
        this.stopSpeech();
        return;
      }
      
      // Otherwise, stop any current speech and start reading this single article
      this.stopSpeech();
      
      this.speechQueue = [{
        article: item,
        tab: this.activeTab,
        index: index
      }];
      
      this.speechActive = true;
      this.currentQueueIndex = 0;
      this.currentlyReadingIndex = index;
      this.currentSpeakingTab = this.activeTab;
      
      try {
        speechSynthesis.cancel();
      } catch (e) {
        console.error(e);
      }
      
      setTimeout(() => {
        if (this.speechActive) {
          this.speakQueueItem();
        }
      }, 100);
    },
    startSpeechQueue() {
      this.speechQueue = [];
      this.currentSpeakingTab = '';
      
      this.channels.forEach(channel => {
        const list = this.newsData[channel];
        if (list && list.length > 0) {
          this.speechQueue.push({
            article: list[0],
            tab: channel,
            index: 0
          });
          this.lastReadTitles[channel] = list[0].title;
        }
      });
      
      if (this.speechQueue.length === 0) {
        notify({
          type: 'info',
          title: 'Thông báo',
          text: 'Không có tin tức mới nào để phát.'
        });
        return;
      }
      
      this.speechActive = true;
      this.currentQueueIndex = 0;
      this.currentlyReadingIndex = 0;
      
      try {
        speechSynthesis.cancel();
      } catch (e) {
        console.error(e);
      }
      
      setTimeout(() => {
        if (this.speechActive) {
          this.speakQueueItem();
        }
      }, 100);
    },
    stopSpeech() {
      this.speechActive = false;
      this.isSpeaking = false;
      this.speechQueue = [];
      this.currentQueueIndex = 0;
      this.articleSentences = [];
      this.currentSentenceIndex = 0;
      this.currentlyReadingIndex = 0;
      this.currentSpeakingTab = '';
      this.expandedItems = []; // Collapse all cards when stopping speech
      
      if (this.currentUtterance) {
        this.currentUtterance.onstart = null;
        this.currentUtterance.onend = null;
        this.currentUtterance.onerror = null;
        this.currentUtterance = null;
      }
      
      try {
        speechSynthesis.cancel();
      } catch (e) {
        console.error('Error cancelling speech synthesis:', e);
      }
    },
    speakQueueItem() {
      if (!this.speechActive) return;
      if (this.currentQueueIndex >= this.speechQueue.length) {
        this.stopSpeech();
        return;
      }
      
      const item = this.speechQueue[this.currentQueueIndex];
      const article = item.article;
      const tab = item.tab;
      
      // Auto switch active tab to match the currently spoken article
      this.activeTab = tab;
      this.currentSpeakingTab = tab;
      this.expandedItems = [];
      this.newsItems = this.newsData[tab] || [];
      
      this.currentlyReadingIndex = item.index !== undefined ? item.index : 0;
      this.expandedItems = [];
      this.expandedItems[this.currentlyReadingIndex] = true; // Auto-expand the currently spoken article card
      this.expandedItems = [...this.expandedItems]; // Force Vue reactivity trigger
      
      this.lastReadTitles[tab] = article.title;
      this.isSpeaking = true;
      
      // Scroll the expanded active card smoothly into view
      this.$nextTick(() => {
        const cardElements = this.$refs.newsCard;
        if (cardElements && cardElements[this.currentlyReadingIndex]) {
          cardElements[this.currentlyReadingIndex].scrollIntoView({
            behavior: 'smooth',
            block: 'nearest'
          });
        }
      });
      
      const cleanText = this.cleanSpeechText(article.description);
      this.articleSentences = this.splitIntoSentences(cleanText);
      this.currentSentenceIndex = 0;
      
      if (this.articleSentences.length === 0) {
        this.nextQueueItem();
        return;
      }
      
      this.speakSentence();
    },
    speakSentence() {
      if (!this.speechActive) return;
      
      if (this.currentSentenceIndex >= this.articleSentences.length) {
        this.nextQueueItem();
        return;
      }
      
      const sentence = this.articleSentences[this.currentSentenceIndex];
      
      if (this.currentUtterance) {
        this.currentUtterance.onstart = null;
        this.currentUtterance.onend = null;
        this.currentUtterance.onerror = null;
      }
      
      const utterance = new SpeechSynthesisUtterance(sentence);
      this.currentUtterance = utterance;
      
      utterance.rate = this.speechRate || 1.05; 
      utterance.pitch = 1.0;
      
      const vietnameseVoice = this.getVietnameseVoice();
      if (vietnameseVoice) {
        utterance.voice = vietnameseVoice;
        utterance.lang = vietnameseVoice.lang || 'vi-VN';
      } else {
        utterance.lang = 'vi-VN';
      }
      
      utterance.onstart = () => {
        this.isSpeaking = true;
      };
      
      utterance.onend = () => {
        this.currentUtterance = null;
        if (this.speechActive) {
          this.currentSentenceIndex++;
          setTimeout(() => {
            if (this.speechActive) {
              this.speakSentence();
            }
          }, 250);
        }
      };
      
      utterance.onerror = (e) => {
        console.error('Speech synthesis sentence error:', e);
        this.currentUtterance = null;
        if (this.speechActive) {
          this.currentSentenceIndex++;
          setTimeout(() => {
            if (this.speechActive) {
              this.speakSentence();
            }
          }, 200);
        }
      };
      
      speechSynthesis.speak(utterance);
    },
    nextQueueItem() {
      if (!this.speechActive) return;
      
      this.currentQueueIndex++;
      if (this.currentQueueIndex < this.speechQueue.length) {
        setTimeout(() => {
          if (this.speechActive) {
            this.speakQueueItem();
          }
        }, 1500);
      } else {
        this.finishActiveQueue();
      }
    },
    finishActiveQueue() {
      this.isSpeaking = false;
      this.speechQueue = [];
      this.currentQueueIndex = 0;
      this.articleSentences = [];
      this.currentSentenceIndex = 0;
      this.currentlyReadingIndex = 0;
      this.expandedItems = []; // Collapse expanded cards
      
      if (this.currentUtterance) {
        this.currentUtterance.onstart = null;
        this.currentUtterance.onend = null;
        this.currentUtterance.onerror = null;
        this.currentUtterance = null;
      }
    },
    checkForNewSpeechArticles() {
      if (!this.speechActive) return;
      
      let newArticlesAdded = false;
      
      this.channels.forEach(channel => {
        const list = this.newsData[channel];
        if (list && list.length > 0) {
          const lastTitle = this.lastReadTitles[channel];
          
          if (lastTitle) {
            const lastReadIndex = list.findIndex(item => item.title === lastTitle);
            const newArticlesForTab = [];
            
            if (lastReadIndex !== -1) {
              // Items from index 0 to lastReadIndex - 1 are brand new.
              // We traverse backwards from lastReadIndex - 1 down to 0 to read older-new first.
              for (let i = lastReadIndex - 1; i >= 0; i--) {
                newArticlesForTab.push({ article: list[i], originalIndex: i });
              }
            } else {
              // If the last read title is not found in the newly loaded feed (e.g. rolled off),
              // we read all newly loaded items from oldest to newest to avoid missing any.
              const count = list.length;
              for (let i = count - 1; i >= 0; i--) {
                newArticlesForTab.push({ article: list[i], originalIndex: i });
              }
            }
            
            newArticlesForTab.forEach(itemInfo => {
              const alreadyInQueue = this.speechQueue.some(qItem => qItem.article.title === itemInfo.article.title);
              if (!alreadyInQueue) {
                this.speechQueue.push({
                  article: itemInfo.article,
                  tab: channel,
                  index: itemInfo.originalIndex
                });
                newArticlesAdded = true;
              }
            });
          }
        }
      });
      
      if (newArticlesAdded) {
        if (!this.isSpeaking) {
          this.isSpeaking = true;
          this.speakQueueItem();
        }
      }
    },
    splitIntoSentences(text) {
      if (!text) return [];
      const sentences = [];
      const regex = /[^.?!;\n]+[.?!;\n]*/g;
      let match;
      while ((match = regex.exec(text)) !== null) {
        const sentence = match[0].trim();
        if (sentence.length > 0) {
          sentences.push(sentence);
        }
      }
      return sentences.length > 0 ? sentences : [text];
    },
    stripHtml(html) {
      const tmp = document.createElement('DIV');
      tmp.innerHTML = html;
      return tmp.textContent || tmp.innerText || '';
    },
    hasLongContent(desc) {
      if (!desc) return false;
      if (desc.length > 150) return true;
      // Check if there are multiple line breaks causing CSS truncation (line-clamp)
      const breaks = (desc.match(/<br\s*\/?>/gi) || []).length + 
                     (desc.match(/<p.*?>/gi) || []).length + 
                     (desc.match(/<div.*?>/gi) || []).length + 
                     (desc.match(/\n/g) || []).length;
      return breaks >= 3;
    },
    cleanSpeechText(text) {
      if (!text) return '';
      let clean = this.stripHtml(text);
      
      clean = clean.replace(/[⭐★☆]+/g, '');
      clean = clean.replace(/[\uD800-\uDBFF][\uDC00-\uDFFF]/g, ''); 
      clean = clean.replace(/[\u2600-\u27BF]/g, ''); 
      clean = clean.replace(/[▫▪•■□▲▼►◄◆◇○●®™©]/g, '');
      
      clean = clean
        .replace(/\s+/g, ' ')
        .replace(/\s+([.,;:?!])/g, '$1')
        .trim();
        
      return clean;
    }
  },
};
</script>

<style scoped>
/* ── News Panel Shell ────────────────────────────────────── */
.news-panel {
  position: fixed;
  top: 0;
  right: 0;
  width: 420px;
  max-width: 100vw;
  height: 100vh;
  background: linear-gradient(180deg, rgba(13, 16, 27, 0.96) 0%, rgba(22, 25, 38, 0.98) 100%);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border-left: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow: -10px 0 40px rgba(0, 0, 0, 0.6);
  overflow-y: auto;
  z-index: 1101;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  color: #f8fafc;
}

/* Custom Scrollbar */
.news-panel::-webkit-scrollbar {
  width: 6px;
}
.news-panel::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.01);
}
.news-panel::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.12);
  border-radius: 3px;
  transition: background 0.2s;
}
.news-panel::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.25);
}

/* ── Sticky Header ────────────────────────────────────── */
.news-panel-header {
  background: rgba(13, 16, 27, 0.85);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  padding: 1.25rem 1.25rem 0 1.25rem;
  z-index: 10;
}

.panel-title {
  font-size: 1.15rem;
  font-weight: 700;
  letter-spacing: -0.2px;
  color: #f8fafc;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.title-icon {
  width: 20px;
  height: 20px;
  color: #ff4757;
  filter: drop-shadow(0 0 6px rgba(255, 71, 87, 0.4));
}

/* Action buttons */
.btn-speech {
  background: rgba(99, 179, 237, 0.12);
  border: 1px solid rgba(99, 179, 237, 0.25);
  border-radius: 30px;
  color: #63b3ed;
  font-size: 0.8rem;
  font-weight: 600;
  padding: 6px 14px;
  transition: all 0.2s ease;
  position: relative;
  overflow: hidden;
  white-space: nowrap;
}
.btn-speech:hover {
  background: rgba(99, 179, 237, 0.2);
  border-color: rgba(99, 179, 237, 0.4);
  color: #90cdf4;
  transform: translateY(-1px);
}
.btn-speech.active {
  background: linear-gradient(135deg, rgba(239, 68, 68, 0.2) 0%, rgba(239, 68, 68, 0.3) 100%);
  border-color: rgba(239, 68, 68, 0.4);
  color: #f87171;
}
.btn-speech.speaking {
  box-shadow: 0 0 12px rgba(239, 68, 68, 0.25);
}

.speech-icon {
  width: 14px;
  height: 14px;
}

.speech-label {
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* Soundwave visualizer */
.speech-soundwave {
  display: flex;
  align-items: flex-end;
  gap: 2px;
  height: 10px;
}
.sw-bar {
  display: inline-block;
  width: 2px;
  height: 100%;
  background-color: currentColor;
  border-radius: 1px;
  animation: bounce 0.8s ease-in-out infinite alternate;
}
.sw-bar:nth-child(2) {
  animation-delay: 0.15s;
}
.sw-bar:nth-child(3) {
  animation-delay: 0.3s;
}

@keyframes bounce {
  0% { height: 2px; }
  100% { height: 10px; }
}

.btn-action {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 50%;
  color: #94a3b8;
  width: 32px;
  height: 32px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}
.btn-action:hover {
  background: rgba(255, 255, 255, 0.08);
  border-color: rgba(255, 255, 255, 0.15);
  color: #f8fafc;
  transform: translateY(-1px);
}
.btn-close-custom:hover {
  background: rgba(239, 68, 68, 0.15);
  border-color: rgba(239, 68, 68, 0.25);
  color: #f87171;
}

.action-icon {
  width: 16px;
  height: 16px;
}
.refresh-icon:hover {
  animation: spin 0.8s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes spin {
  100% { transform: rotate(360deg); }
}

/* ── Channel Selector Select Box ────────────────────────────────────── */
.news-tabs-wrapper {
  margin-top: 1rem;
  margin-bottom: 0.75rem;
}

.news-channel-select {
  width: 100%;
  background-color: rgba(255, 255, 255, 0.04) !important;
  border: 1px solid rgba(255, 255, 255, 0.08) !important;
  color: #f8fafc !important;
  border-radius: 12px !important;
  padding: 8px 12px !important;
  font-size: 0.85rem !important;
  font-weight: 600 !important;
  outline: none !important;
  box-shadow: none !important;
  cursor: pointer;
  transition: all 0.25s ease;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3e%3cpath fill='none' stroke='%2394a3b8' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='m2 5 6 6 6-6'/%3e%3c/svg%3e") !important;
  background-repeat: no-repeat !important;
  background-position: right 12px center !important;
  background-size: 12px 12px !important;
  appearance: none !important;
  -webkit-appearance: none !important;
}
.news-channel-select:hover {
  background-color: rgba(255, 255, 255, 0.08) !important;
  border-color: rgba(255, 255, 255, 0.15) !important;
}
.news-channel-select:focus {
  border-color: #ff4757 !important;
  box-shadow: 0 0 0 3px rgba(255, 71, 87, 0.15) !important;
}
.news-channel-select option {
  background-color: #161926;
  color: #f8fafc;
  font-weight: 500;
}

/* ── Auto-Refresh Progress Bar ────────────────────────────────────── */
.progress-bar-container {
  height: 2px;
  background: rgba(255, 255, 255, 0.02);
  width: calc(100% + 2.5rem);
  margin-left: -1.25rem;
  overflow: hidden;
  position: relative;
}

.refresh-progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #ff4757, #ff6b81);
  box-shadow: 0 0 6px rgba(255, 71, 87, 0.6);
  transition: width 1s linear;
}

/* ── Live / Meta Row ────────────────────────────────────── */
.news-meta-row {
  margin-top: 0.5rem;
}

.live-indicator {
  background: rgba(16, 185, 129, 0.08);
  border: 1px solid rgba(16, 185, 129, 0.15);
  border-radius: 20px;
  padding: 4px 10px;
}

.live-dot {
  width: 6px;
  height: 6px;
  background-color: #10b981;
  border-radius: 50%;
  box-shadow: 0 0 8px #10b981;
  animation: pulse-live 1.5s infinite alternate;
}

@keyframes pulse-live {
  0% { opacity: 0.4; transform: scale(0.9); }
  100% { opacity: 1; transform: scale(1.1); }
}

.live-text {
  color: #34d399;
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.8px;
  text-transform: uppercase;
}

.countdown-badge {
  color: #64748b;
  font-size: 0.75rem;
}
.text-neon {
  color: #ff4757;
  text-shadow: 0 0 4px rgba(255, 71, 87, 0.2);
}

/* ── News Cards ────────────────────────────────────── */
.news-card {
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid rgba(255, 255, 255, 0.05) !important;
  border-radius: 16px !important;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
}
.news-card:hover {
  transform: translateY(-3px);
  background: rgba(255, 255, 255, 0.04);
  border-color: rgba(255, 255, 255, 0.1) !important;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.35);
}

/* Highlight border for card currently being read */
.reading-border {
  border: 1px solid rgba(99, 179, 237, 0.4) !important;
  box-shadow: 0 0 15px rgba(99, 179, 237, 0.15), 0 4px 20px rgba(0, 0, 0, 0.15) !important;
  background: rgba(99, 179, 237, 0.02) !important;
}

.card-img-wrapper {
  position: relative;
  width: 100%;
  padding-bottom: 56.25%; /* 16:9 */
  overflow: hidden;
  background-color: rgba(255, 255, 255, 0.01);
}

.card-img-top {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}
.news-card:hover .card-img-top {
  transform: scale(1.05);
}

.card-img-glow {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(180deg, transparent 60%, rgba(13, 16, 27, 0.85) 100%);
  pointer-events: none;
}

.card-text {
  color: #cbd5e1 !important;
  line-height: 1.55;
  font-size: 0.85rem !important;
}

/* Custom styling for parsed telegram HTML inside card content */
.card-text :deep(a) {
  color: #63b3ed;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.15s ease;
}
.card-text :deep(a):hover {
  color: #90cdf4;
  text-decoration: underline;
}

.card-text :deep(br) {
  margin-bottom: 6px;
}

.text-truncate-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Footer Section */
.news-date {
  color: #64748b;
  font-size: 0.75rem;
}
.clock-icon {
  width: 13px;
  height: 13px;
  color: rgba(255, 255, 255, 0.25);
}

.btn-expand {
  background: transparent;
  border: none;
  color: #63b3ed;
  font-size: 0.8rem;
  font-weight: 600;
  transition: all 0.2s ease;
}
.btn-expand:hover {
  color: #90cdf4;
  transform: translateX(1px);
}

.chevron-icon {
  width: 12px;
  height: 12px;
  transition: transform 0.25s ease;
}
.rotate-180 {
  transform: rotate(180deg);
}

.z-index-top {
  position: relative;
  z-index: 2;
}

.speech-icon-small {
  color: #63b3ed;
  transition: all 0.2s ease;
}
.speech-icon-small:hover {
  color: #90cdf4;
  transform: scale(1.1);
}
.animate-pulse-speech {
  animation: pulse-speech 1.2s infinite alternate;
}
@keyframes pulse-speech {
  0% { transform: scale(1); opacity: 0.8; }
  100% { transform: scale(1.15); opacity: 1; filter: drop-shadow(0 0 4px rgba(239, 68, 68, 0.6)); }
}

/* Smooth layout animation */
.news-panel-content ul {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Speech Settings Panel Styles */
.speech-settings-panel {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.07);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.05), 0 4px 15px rgba(0, 0, 0, 0.25);
  animation: slideDown 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}

.settings-title {
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  padding-bottom: 6px;
}

.small-title {
  font-size: 0.7rem;
  font-weight: 800;
  letter-spacing: 1px;
  color: #ff4757;
}

.btn-close-sm {
  background: transparent;
  border: none;
  color: #94a3b8;
  font-size: 1.1rem;
  line-height: 1;
  padding: 0;
  cursor: pointer;
  transition: color 0.15s ease;
}
.btn-close-sm:hover {
  color: #ef4444;
}

.setting-label {
  font-weight: 600;
  letter-spacing: 0.2px;
}

.speech-select {
  background-color: rgba(13, 16, 27, 0.8) !important;
  border: 1px solid rgba(255, 255, 255, 0.1) !important;
  color: #f8fafc !important;
  border-radius: 8px !important;
  padding: 6px 10px !important;
  font-size: 0.8rem !important;
  outline: none !important;
  box-shadow: none !important;
}
.speech-select:focus {
  border-color: rgba(99, 179, 237, 0.4) !important;
}

.speech-select option {
  background-color: #0d101b;
  color: #f8fafc;
}

.text-neon-blue {
  color: #63b3ed;
  text-shadow: 0 0 4px rgba(99, 179, 237, 0.3);
}

.btn-settings-active {
  background: rgba(99, 179, 237, 0.15) !important;
  border-color: rgba(99, 179, 237, 0.3) !important;
  color: #63b3ed !important;
}

/* Custom range input (slider) styling */
.speech-range {
  height: 6px;
  border-radius: 3px;
  background: rgba(255, 255, 255, 0.08);
  outline: none;
  transition: background 450ms ease-in;
  -webkit-appearance: none;
}
.speech-range::-webkit-slider-thumb {
  -webkit-appearance: none;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  background: #63b3ed;
  box-shadow: 0 0 6px rgba(99, 179, 237, 0.8);
  cursor: pointer;
  transition: transform 0.1s ease;
}
.speech-range::-webkit-slider-thumb:hover {
  transform: scale(1.2);
}
</style>