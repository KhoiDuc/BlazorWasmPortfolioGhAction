<template>
  <div class="stk-panel forex-sessions-panel">
    <!-- Header -->
    <div class="stk-header session-header">
      <div class="stk-header__icon session-header-icon">
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10" />
          <polyline points="12 6 12 12 16 14" />
        </svg>
      </div>
      <div class="header-main-info">
        <h2 class="stk-header__title">Phiên Giao Dịch & Khung Giờ Biến Động</h2>
        <p class="stk-header__sub">Trạng thái các phiên giao dịch chính và các thời điểm thị trường biến động mạnh (quy đổi sang Giờ Việt Nam - UTC+7)</p>
      </div>
      <div class="session-header-badge">
        <span class="dst-auto-badge">
          <span class="pulse-green"></span>
          Tự động đồng bộ giờ hệ thống
        </span>
      </div>
    </div>
    
    <div class="stk-section session-body">
      <!-- Top Section: Clock, Speedometer & Season Selector -->
      <div class="session-top-controls">
        <!-- Live Clock -->
        <div class="live-clock-card">
          <div class="clock-label">GIỜ VIỆT NAM (HO CHI MINH)</div>
          <div class="clock-time">{{ formattedTime }}</div>
          <div class="clock-date">{{ formattedDate }}</div>
        </div>
        
        <!-- Volatility Speedometer -->
        <div class="volatility-speedometer-card">
          <div class="speed-header">
            <span class="speed-label">Biến động hiện tại:</span>
            <span class="speed-value" :style="{ color: currentVolatility.color }">{{ currentVolatility.level }}</span>
          </div>
          <div class="speed-bar-container">
            <div class="speed-bar-fill" :style="{ width: `${currentVolatility.score}%`, backgroundColor: currentVolatility.color }"></div>
          </div>
          <div class="speed-desc">{{ currentVolatility.desc }}</div>
        </div>
        
        <!-- DST / Season Toggle -->
        <div class="season-toggle-card">
          <div class="toggle-label">Múi giờ phiên giao dịch</div>
          <div class="toggle-buttons">
            <button 
              class="toggle-btn" 
              :class="{ 'toggle-btn--active': selectedSeason === 'summer' }"
              @click="selectedSeason = 'summer'"
            >
              ☀️ Mùa Hè (DST)
            </button>
            <button 
              class="toggle-btn" 
              :class="{ 'toggle-btn--active': selectedSeason === 'winter' }"
              @click="selectedSeason = 'winter'"
            >
              ❄️ Mùa Đông (Standard)
            </button>
          </div>
          <div class="toggle-note">
            Đang hiển thị theo <strong>{{ selectedSeason === 'summer' ? 'Giờ Mùa Hè' : 'Giờ Mùa Đông' }}</strong>.
            <span v-if="isSystemSeasonDST === (selectedSeason === 'summer')" class="system-match">
              (Khớp với thời gian thực tế)
            </span>
            <span v-else class="system-preview">(Chế độ xem trước)</span>
          </div>
        </div>
      </div>
      
      <!-- Rollover Alert Banner -->
      <div v-if="isRolloverHour" class="rollover-warning-banner">
        <span class="warning-icon">⚠️</span>
        <div class="warning-content">
          <div class="warning-title">CẢNH BÁO: ĐANG TRONG KHUNG GIỜ ROLLOVER ({{ rolloverRangeString }})</div>
          <p class="warning-desc">Thị trường đang chuyển giao ngày mới. Thanh khoản cực mỏng, spread giãn rộng đột ngột (đặc biệt các cặp AUD, NZD, chéo). Hạn chế vào lệnh mới và cân nhắc nới rộng Stop Loss.</p>
        </div>
      </div>
      
      <!-- Live Session Cards Grid -->
      <div class="sessions-grid">
        <div 
          v-for="s in sessionList" 
          :key="s.id" 
          class="session-card"
          :class="{ 'session-card--active': s.isOpen }"
        >
          <div class="session-card-header">
            <div class="session-name">
              <span class="session-flag">{{ s.flag }}</span>
              <span class="session-title">{{ s.name }}</span>
            </div>
            <div 
              class="session-status" 
              :class="s.isOpen ? 'session-status--open' : 'session-status--closed'"
            >
              <span class="status-dot"></span>
              {{ s.isOpen ? 'ĐANG MỞ' : 'ĐÃ ĐÓNG' }}
            </div>
          </div>
          
          <div class="session-card-body">
            <div class="session-info-row">
              <span class="info-label">Mở cửa:</span>
              <span class="info-value">{{ formatHourString(s.openHour) }}</span>
            </div>
            <div class="session-info-row">
              <span class="info-label">Đóng cửa:</span>
              <span class="info-value">{{ formatHourString(s.closeHour) }}</span>
            </div>
            <div class="session-info-row">
              <span class="info-label">Biến động:</span>
              <span class="stk-signal" :class="s.volClass">{{ s.volatility }}</span>
            </div>
            <p class="session-card-desc">{{ s.description }}</p>
          </div>
          
          <div class="session-card-footer" :class="{ 'session-card-footer--active': s.isOpen }">
            <span class="footer-icon">⏳</span>
            <span class="countdown-text">{{ s.countdown }}</span>
          </div>
        </div>
      </div>
      
      <!-- Horizontal Timeline Visualization -->
      <div class="timeline-container">
        <div class="timeline-header-wrap">
          <h4 class="timeline-title">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="vertical-align: middle; margin-right: 4px;"><path d="M3 3v18h18"/><path d="m19 9-5 5-4-4-3 3"/></svg>
            Trực Quan Hóa Chu Kỳ 24 Giờ & Khung Giờ Vàng
          </h4>
          <span class="timeline-subtitle">Các vùng đè nhau màu vàng/đỏ thể hiện thời điểm thanh khoản và biến động đạt cực đại.</span>
        </div>
        
        <div class="timeline-wrapper">
          <div class="timeline-main">
            <!-- Left Column: Session Labels -->
            <div class="timeline-labels-column">
              <div class="timeline-label-cell">🇦🇺 Sydney</div>
              <div class="timeline-label-cell">🇯🇵 Tokyo</div>
              <div class="timeline-label-cell">🇬🇧 London</div>
              <div class="timeline-label-cell">🇺🇸 New York</div>
              <div class="timeline-label-cell axis-empty-cell"></div>
            </div>
            
            <!-- Right Column: Visual Tracks Area -->
            <div class="timeline-tracks-area">
              <!-- Background Highlight Overlays -->
              <!-- Golden Hours (London/NY Overlap) -->
              <div 
                class="zone-overlay zone-golden" 
                :style="goldenZoneStyle"
                title="Khung Giờ Vàng (Giao thoa Âu-Mỹ) - Thị trường biến động mạnh nhất"
              >
                <span class="zone-label-text">GOLDEN HOURS 🔥</span>
              </div>
              
              <!-- Rollover Zone -->
              <div 
                class="zone-overlay zone-rollover" 
                :style="rolloverZoneStyle"
                title="Giờ Rollover - Thanh khoản thấp, Giãn spread rộng"
              >
                <span class="zone-label-text">ROLLOVER ⚠️</span>
              </div>
              
              <!-- Live Current Time Line Indicator -->
              <div class="current-time-line" :style="currentTimeLineStyle">
                <div class="time-marker-tooltip">{{ formattedTimeShort }}</div>
              </div>
              
              <!-- Tracks contents -->
              <!-- Sydney Track -->
              <div class="track-cell">
                <div class="lane-bar sydney-bar" :style="getLaneBarStyles(5, 14)">
                  05:00 - 14:00
                </div>
              </div>
              
              <!-- Tokyo Track -->
              <div class="track-cell">
                <div class="lane-bar tokyo-bar" :style="getLaneBarStyles(6, 15)">
                  06:00 - 15:00
                </div>
              </div>
              
              <!-- London Track -->
              <div class="track-cell">
                <div class="lane-bar london-bar" :style="getLondonBarStyles">
                  {{ selectedSeason === 'summer' ? '14:00 - 23:00' : '15:00 - 24:00' }}
                </div>
              </div>
              
              <!-- New York Track (Split into 2 bars due to midnight crossing) -->
              <div class="track-cell">
                <div class="lane-bar newyork-bar NY-evening" :style="getNYEveningBarStyles">
                  {{ selectedSeason === 'summer' ? '19:00 - 24:00' : '20:00 - 24:00' }}
                </div>
                <div class="lane-bar newyork-bar NY-morning" :style="getNYMorningBarStyles">
                  {{ selectedSeason === 'summer' ? '00:00 - 04:00' : '00:00 - 05:00' }}
                </div>
              </div>
              
              <!-- Hours X-Axis Ticks -->
              <div class="timeline-axis-ticks">
                <span v-for="h in axisHours" :key="h" class="axis-tick" :style="{ left: `${h / 24 * 100}%` }">
                  <span class="tick-label">{{ formatTick(h) }}</span>
                </span>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Legend -->
        <div class="timeline-legend">
          <div class="legend-item">
            <span class="legend-box sydney-legend"></span> Sydney (Úc)
          </div>
          <div class="legend-item">
            <span class="legend-box tokyo-legend"></span> Tokyo (Á)
          </div>
          <div class="legend-item">
            <span class="legend-box london-legend"></span> London (Âu)
          </div>
          <div class="legend-item">
            <span class="legend-box newyork-legend"></span> New York (Mỹ)
          </div>
          <div class="legend-item">
            <span class="legend-box golden-legend"></span> Giờ Vàng (Giao thoa Âu-Mỹ)
          </div>
          <div class="legend-item">
            <span class="legend-box rollover-legend"></span> Rollover (Giãn spread)
          </div>
        </div>
      </div>
      
      <!-- Key Trading Tips -->
      <div class="sessions-tips-panel">
        <h4 class="tips-title">💡 Ghi nhớ quan trọng khi giao dịch Forex tại Việt Nam:</h4>
        <ul class="tips-list">
          <li>
            <strong>14:00 - 15:00 (Mùa hè/Mùa đông)</strong>: Mở phiên Âu. Khối lượng giao dịch tăng nhanh, xu hướng chính trong ngày bắt đầu hình thành rõ nét.
          </li>
          <li>
            <strong>19:00 - 23:00 (Mùa hè) / 20:00 - 24:00 (Mùa đông)</strong>: Khung giờ giao thoa Âu - Mỹ. Đây là **Golden Hours** - thời điểm vàng để giao dịch vì tính thanh khoản cực cao, spread thấp, sóng đi mượt và chạy xa nhất. Hầu hết các tin tức đỏ quan trọng (CPI, Non-farm, GDP) của Mỹ đều công bố vào lúc 19:30 hoặc 20:30.
          </li>
          <li>
            <strong>UTC 00:00 (07:00 sáng VN)</strong>: Nến D1 (Daily) đóng cửa. Hầu hết các sàn cập nhật swap (phí qua đêm) và phân tích kỹ thuật cập nhật nến mới.
          </li>
          <li>
            <strong>01:00 / 02:00 sáng (Thứ Năm hàng tuần)</strong>: Tin FOMC công bố quyết định lãi suất của Fed. Thị trường thường biến động giật hai đầu cực mạnh, tốt nhất nên đứng ngoài.
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted, computed } from 'vue';

export default {
  name: 'ForexSessions',
  setup() {
    const currentTime = ref(new Date());
    const isSystemSeasonDST = ref(false);
    const selectedSeason = ref('summer');
    let timer = null;

    // Standard timezone calculation helper for UTC+7 (VN Time)
    const updateTime = () => {
      try {
        const options = {
          timeZone: 'Asia/Ho_Chi_Minh',
          year: 'numeric',
          month: 'numeric',
          day: 'numeric',
          hour: 'numeric',
          minute: 'numeric',
          second: 'numeric',
          hour12: false
        };
        const formatter = new Intl.DateTimeFormat('en-US', options);
        const parts = formatter.formatToParts(new Date());
        const p = {};
        parts.forEach(part => {
          p[part.type] = part.value;
        });
        currentTime.value = new Date(p.year, p.month - 1, p.day, p.hour, p.minute, p.second);
      } catch (e) {
        currentTime.value = new Date();
      }
    };

    // US DST Detector (Second Sunday of March to first Sunday of November)
    const checkUSDST = (date) => {
      const year = date.getFullYear();
      const march = new Date(year, 2, 1);
      const startDST = new Date(year, 2, 14 - march.getDay());
      startDST.setHours(2, 0, 0, 0);

      const november = new Date(year, 10, 1);
      const endDST = new Date(year, 10, 7 - november.getDay());
      endDST.setHours(2, 0, 0, 0);

      return date >= startDST && date < endDST;
    };

    const detectSeason = () => {
      const vnNow = currentTime.value;
      const isSummer = checkUSDST(vnNow);
      isSystemSeasonDST.value = isSummer;
      selectedSeason.value = isSummer ? 'summer' : 'winter';
    };

    onMounted(() => {
      updateTime();
      detectSeason();
      timer = setInterval(updateTime, 1000);
    });

    onUnmounted(() => {
      if (timer) clearInterval(timer);
    });

    // Formatting Helpers
    const formattedTime = computed(() => {
      const hours = String(currentTime.value.getHours()).padStart(2, '0');
      const minutes = String(currentTime.value.getMinutes()).padStart(2, '0');
      const seconds = String(currentTime.value.getSeconds()).padStart(2, '0');
      return `${hours}:${minutes}:${seconds}`;
    });

    const formattedTimeShort = computed(() => {
      const hours = String(currentTime.value.getHours()).padStart(2, '0');
      const minutes = String(currentTime.value.getMinutes()).padStart(2, '0');
      return `${hours}:${minutes}`;
    });

    const formattedDate = computed(() => {
      const days = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
      const dayName = days[currentTime.value.getDay()];
      const day = String(currentTime.value.getDate()).padStart(2, '0');
      const month = String(currentTime.value.getMonth() + 1).padStart(2, '0');
      const year = currentTime.value.getFullYear();
      return `${dayName}, Ngày ${day}/${month}/${year}`;
    });

    const formatHourString = (hour) => {
      return `${String(hour).padStart(2, '0')}:00`;
    };

    // Helper to calculate whether a session is open
    const isSessionOpen = (open, close, currentHour) => {
      if (open < close) {
        return currentHour >= open && currentHour < close;
      } else {
        // Midnight crossing (e.g. New York 19:00 - 04:00)
        return currentHour >= open || currentHour < close;
      }
    };

    // Helper to compute countdown
    const getSessionCountdown = (open, close, now) => {
      const currentHour = now.getHours();
      const currentMinute = now.getMinutes();

      const isOpen = isSessionOpen(open, close, currentHour);

      const getMinutesDiff = (startH, startM, targetH, targetM) => {
        let diff = (targetH * 60 + targetM) - (startH * 60 + startM);
        if (diff < 0) {
          diff += 24 * 60; // Wrap around 24 hours
        }
        return diff;
      };

      if (isOpen) {
        const diffMin = getMinutesDiff(currentHour, currentMinute, close, 0);
        const h = Math.floor(diffMin / 60);
        const m = diffMin % 60;
        return `Đóng cửa sau ${h}h ${m}m`;
      } else {
        const diffMin = getMinutesDiff(currentHour, currentMinute, open, 0);
        const h = Math.floor(diffMin / 60);
        const m = diffMin % 60;
        return `Mở cửa sau ${h}h ${m}m`;
      }
    };

    // Live Volatility Speedometer Logic
    const currentVolatility = computed(() => {
      const now = currentTime.value;
      const hour = now.getHours();
      const isSummer = selectedSeason.value === 'summer';

      if (isSummer) {
        if (hour >= 19 && hour < 23) {
          return {
            level: 'Cực cao 🔥',
            score: 95,
            desc: 'Giao thoa Âu - Mỹ (Golden Hours). Thanh khoản lớn nhất ngày, tin tức đỏ Mỹ công bố. Sóng chạy rất mạnh.',
            color: '#dc2626'
          };
        } else if (hour >= 14 && hour < 19) {
          return {
            level: 'Cao ⚡',
            score: 75,
            desc: 'Phiên Âu đang mở cửa. Sóng chạy sôi nổi, thích hợp giao dịch các cặp tiền đuôi EUR, GBP.',
            color: '#ea580c'
          };
        } else if (hour >= 23 || hour < 4) {
          return {
            level: 'Trung bình - Cao 🌙',
            score: 60,
            desc: 'Phiên Mỹ hoạt động độc lập. Thanh khoản giảm dần nhưng vẫn có những nhịp điệu riêng.',
            color: '#eab308'
          };
        } else if (hour === 4) {
          return {
            level: 'Rủi ro giãn Spread ⚠️',
            score: 15,
            desc: 'Khung giờ Rollover. Các sàn giao dịch kết chuyển ngày mới, spread giãn rất rộng, nên đứng ngoài.',
            color: '#a855f7'
          };
        } else {
          return {
            level: 'Thấp - Trung bình 💤',
            score: 35,
            desc: 'Phiên Á mở cửa. Thị trường di chuyển tương đối chậm, thích hợp giao dịch tích lũy sideway.',
            color: '#10b981'
          };
        }
      } else {
        // Winter Time (+1 hour offset for Europe/US)
        if (hour >= 20 && hour < 24) {
          return {
            level: 'Cực cao 🔥',
            score: 95,
            desc: 'Giao thoa Âu - Mỹ (Golden Hours). Thanh khoản lớn nhất ngày, tin tức đỏ Mỹ công bố. Sóng chạy rất mạnh.',
            color: '#dc2626'
          };
        } else if (hour >= 15 && hour < 20) {
          return {
            level: 'Cao ⚡',
            score: 75,
            desc: 'Phiên Âu đang mở cửa. Sóng chạy sôi nổi, thích hợp giao dịch các cặp tiền đuôi EUR, GBP.',
            color: '#ea580c'
          };
        } else if (hour >= 0 && hour < 5) {
          return {
            level: 'Trung bình - Cao 🌙',
            score: 60,
            desc: 'Phiên Mỹ hoạt động độc lập. Thanh khoản giảm dần nhưng vẫn có những nhịp điệu riêng.',
            color: '#eab308'
          };
        } else if (hour === 5) {
          return {
            level: 'Rủi ro giãn Spread ⚠️',
            score: 15,
            desc: 'Khung giờ Rollover. Các sàn giao dịch kết chuyển ngày mới, spread giãn rất rộng, nên đứng ngoài.',
            color: '#a855f7'
          };
        } else {
          return {
            level: 'Thấp - Trung bình 💤',
            score: 35,
            desc: 'Phiên Á mở cửa. Thị trường di chuyển tương đối chậm, thích hợp giao dịch tích lũy sideway.',
            color: '#10b981'
          };
        }
      }
    });

    const isRolloverHour = computed(() => {
      const hour = currentTime.value.getHours();
      const isSummer = selectedSeason.value === 'summer';
      return isSummer ? hour === 4 : hour === 5;
    });

    const rolloverRangeString = computed(() => {
      const isSummer = selectedSeason.value === 'summer';
      return isSummer ? '04:00 - 05:00 sáng' : '05:00 - 06:00 sáng';
    });

    // Session configurations
    const sessionList = computed(() => {
      const isSummer = selectedSeason.value === 'summer';
      const now = currentTime.value;
      const currentHour = now.getHours();

      const config = [
        {
          id: 'sydney',
          name: 'Sydney (Úc)',
          flag: '🇦🇺',
          openHour: 5,
          closeHour: 14,
          volatility: 'Thấp',
          volClass: 'stk-signal--low',
          description: 'Khởi động ngày mới, biên độ dao động nhẹ. Phù hợp giao dịch AUD, NZD.'
        },
        {
          id: 'tokyo',
          name: 'Tokyo (Châu Á)',
          flag: '🇯🇵',
          openHour: 6,
          closeHour: 15,
          volatility: 'Trung bình',
          volClass: 'stk-signal--medium',
          description: 'Phiên giao dịch chính của Châu Á, tập trung khối lượng lớn quanh đồng Yên JPY.'
        },
        {
          id: 'london',
          name: 'London (Châu Âu)',
          flag: '🇬🇧',
          openHour: isSummer ? 14 : 15,
          closeHour: isSummer ? 23 : 24,
          volatility: 'Cao',
          volClass: 'stk-signal--high',
          description: 'Phiên Âu sôi động, thanh khoản cao, sóng chạy rõ xu hướng. Trọng tâm EUR, GBP.'
        },
        {
          id: 'newyork',
          name: 'New York (Mỹ)',
          flag: '🇺🇸',
          openHour: isSummer ? 19 : 20,
          closeHour: isSummer ? 4 : 5,
          volatility: 'Cực cao',
          volClass: 'stk-signal--high',
          description: 'Phiên Mỹ đầy biến động lớn khi kết hợp với phiên Âu và đón nhận tin tức kinh tế quan trọng.'
        }
      ];

      return config.map(s => {
        const isOpen = isSessionOpen(s.openHour, s.closeHour, currentHour);
        const countdown = getSessionCountdown(s.openHour, s.closeHour, now);
        return {
          ...s,
          isOpen,
          countdown
        };
      });
    });

    // Timeline calculations
    const getLaneBarStyles = (open, close) => {
      const left = (open / 24) * 100;
      const width = ((close - open) / 24) * 100;
      return {
        left: `${left}%`,
        width: `${width}%`
      };
    };

    const getLondonBarStyles = computed(() => {
      const isSummer = selectedSeason.value === 'summer';
      const open = isSummer ? 14 : 15;
      const close = isSummer ? 23 : 24;
      return getLaneBarStyles(open, close);
    });

    const getNYEveningBarStyles = computed(() => {
      const isSummer = selectedSeason.value === 'summer';
      const open = isSummer ? 19 : 20;
      return getLaneBarStyles(open, 24);
    });

    const getNYMorningBarStyles = computed(() => {
      const isSummer = selectedSeason.value === 'summer';
      const close = isSummer ? 4 : 5;
      return getLaneBarStyles(0, close);
    });

    const goldenZoneStyle = computed(() => {
      const isSummer = selectedSeason.value === 'summer';
      const open = isSummer ? 19 : 20;
      const close = isSummer ? 23 : 24;
      return getLaneBarStyles(open, close);
    });

    const rolloverZoneStyle = computed(() => {
      const isSummer = selectedSeason.value === 'summer';
      const open = isSummer ? 4 : 5;
      const close = isSummer ? 5 : 6;
      return getLaneBarStyles(open, close);
    });

    const currentTimeLineStyle = computed(() => {
      const now = currentTime.value;
      const hours = now.getHours();
      const minutes = now.getMinutes();
      const pct = ((hours + minutes / 60) / 24) * 100;
      return {
        left: `${pct}%`
      };
    });

    const axisHours = [0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22];
    const formatTick = (h) => {
      return `${String(h).padStart(2, '0')}:00`;
    };

    return {
      currentTime,
      isSystemSeasonDST,
      selectedSeason,
      formattedTime,
      formattedTimeShort,
      formattedDate,
      formatHourString,
      isRolloverHour,
      rolloverRangeString,
      currentVolatility,
      sessionList,
      getLaneBarStyles,
      getLondonBarStyles,
      getNYEveningBarStyles,
      getNYMorningBarStyles,
      goldenZoneStyle,
      rolloverZoneStyle,
      currentTimeLineStyle,
      axisHours,
      formatTick
    };
  }
};
</script>

<style scoped>
.forex-sessions-panel {
  border-left: 4px solid #3b82f6 !important;
  margin-bottom: 24px;
}

.session-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-main-info {
  flex-grow: 1;
}

.session-header-badge {
  display: flex;
  align-items: center;
}

.dst-auto-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: rgba(16, 185, 129, 0.08);
  border: 1px solid rgba(16, 185, 129, 0.2);
  color: #059669;
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 600;
}

.pulse-green {
  width: 8px;
  height: 8px;
  background-color: #10b981;
  border-radius: 50%;
  display: inline-block;
  box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
  animation: pulse 1.6s infinite;
}

@keyframes pulse {
  0% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
  }
  70% {
    transform: scale(1);
    box-shadow: 0 0 0 6px rgba(16, 185, 129, 0);
  }
  100% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0);
  }
}

.session-body {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* Controls Grid */
.session-top-controls {
  display: grid;
  grid-template-columns: 1fr 1.2fr 1.1fr;
  gap: 16px;
}

@media (max-width: 900px) {
  .session-top-controls {
    grid-template-columns: 1fr;
  }
}

/* Control Cards */
.live-clock-card, .volatility-speedometer-card, .season-toggle-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 14px 16px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

/* Clock Card styles */
.live-clock-card {
  align-items: center;
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
  border: none;
  color: #ffffff;
  box-shadow: inset 0 0 20px rgba(255,255,255,0.05);
}

.clock-label {
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 1px;
  color: #94a3b8;
  margin-bottom: 4px;
}

.clock-time {
  font-family: 'Outfit', sans-serif;
  font-size: 2rem;
  font-weight: 800;
  letter-spacing: 1px;
  color: #3b82f6;
  text-shadow: 0 0 10px rgba(59, 130, 246, 0.4);
  line-height: 1.1;
}

.clock-date {
  font-size: 0.8rem;
  font-weight: 500;
  color: #cbd5e1;
  margin-top: 4px;
}

/* Volatility Speedometer Card styles */
.speed-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.speed-label {
  font-size: 0.78rem;
  font-weight: 700;
  color: #475569;
}

.speed-value {
  font-size: 0.85rem;
  font-weight: 800;
}

.speed-bar-container {
  height: 6px;
  background: #e2e8f0;
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 8px;
}

.speed-bar-fill {
  height: 100%;
  border-radius: 3px;
  transition: width 0.5s ease, background-color 0.5s ease;
}

.speed-desc {
  font-size: 0.74rem;
  color: #64748b;
  line-height: 1.35;
  text-align: left;
}

/* Toggle Card styles */
.toggle-label {
  font-size: 0.78rem;
  font-weight: 700;
  color: #475569;
  margin-bottom: 8px;
  text-align: left;
}

.toggle-buttons {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
}

.toggle-btn {
  flex: 1;
  padding: 6px 10px;
  font-size: 0.76rem;
  font-weight: 600;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  background: #ffffff;
  color: #475569;
  cursor: pointer;
  transition: all 0.15s ease;
}

.toggle-btn:hover {
  background: #f1f5f9;
  border-color: #94a3b8;
}

.toggle-btn--active {
  background: #3b82f6 !important;
  color: #ffffff !important;
  border-color: #3b82f6 !important;
  box-shadow: 0 2px 6px rgba(59, 130, 246, 0.2);
}

.toggle-note {
  font-size: 0.7rem;
  color: #64748b;
  text-align: left;
}

.system-match {
  color: #10b981;
  font-weight: 600;
}

.system-preview {
  color: #ea580c;
  font-weight: 600;
}

/* Rollover warning banner */
.rollover-warning-banner {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  background: rgba(168, 85, 247, 0.08);
  border: 1px solid rgba(168, 85, 247, 0.25);
  border-radius: 12px;
  padding: 12px 16px;
  text-align: left;
  animation: borderPulse 2s infinite ease-in-out;
}

@keyframes borderPulse {
  0% { border-color: rgba(168, 85, 247, 0.25); box-shadow: 0 0 0 0 rgba(168, 85, 247, 0.1); }
  50% { border-color: rgba(168, 85, 247, 0.5); box-shadow: 0 0 12px 0 rgba(168, 85, 247, 0.15); }
  100% { border-color: rgba(168, 85, 247, 0.25); box-shadow: 0 0 0 0 rgba(168, 85, 247, 0.1); }
}

.warning-icon {
  font-size: 1.4rem;
  line-height: 1;
}

.warning-title {
  font-size: 0.82rem;
  font-weight: 800;
  color: #7e22ce;
  margin-bottom: 2px;
}

.warning-desc {
  font-size: 0.78rem;
  color: #581c87;
  margin: 0;
  line-height: 1.4;
}

/* Grid of Sessions */
.sessions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 16px;
}

.session-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.01);
  display: flex;
  flex-direction: column;
  transition: all 0.2s ease;
  text-align: left;
}

.session-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.05);
}

.session-card--active {
  border-color: rgba(59, 130, 246, 0.4);
  box-shadow: 0 4px 16px rgba(59, 130, 246, 0.06);
}

.session-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 14px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

.session-name {
  display: flex;
  align-items: center;
  gap: 6px;
}

.session-flag {
  font-size: 1.1rem;
}

.session-title {
  font-size: 0.85rem;
  font-weight: 700;
  color: #1e293b;
}

.session-status {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.68rem;
  font-weight: 800;
  padding: 2px 6px;
  border-radius: 4px;
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  display: inline-block;
}

.session-status--open {
  background: rgba(16, 185, 129, 0.1);
  color: #047857;
}

.session-status--open .status-dot {
  background-color: #10b981;
  box-shadow: 0 0 6px #10b981;
}

.session-status--closed {
  background: rgba(100, 116, 139, 0.1);
  color: #475569;
}

.session-status--closed .status-dot {
  background-color: #64748b;
}

.session-card-body {
  padding: 12px 14px;
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.session-info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.78rem;
}

.info-label {
  color: #64748b;
  font-weight: 500;
}

.info-value {
  color: #1e293b;
  font-weight: 700;
}

.session-card-desc {
  font-size: 0.74rem;
  color: #64748b;
  line-height: 1.4;
  margin: 6px 0 0;
  border-top: 1px dashed #f1f5f9;
  padding-top: 6px;
}

.session-card-footer {
  padding: 8px 14px;
  background: #f8fafc;
  border-top: 1px solid #f1f5f9;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.74rem;
  font-weight: 600;
  color: #64748b;
  transition: all 0.2s ease;
}

.session-card-footer--active {
  background: rgba(59, 130, 246, 0.03);
  color: #2563eb;
}

/* Timeline Style */
.timeline-container {
  border: 1px solid #e2e8f0;
  background: #ffffff;
  border-radius: 12px;
  padding: 16px;
  text-align: left;
}

.timeline-header-wrap {
  margin-bottom: 12px;
}

.timeline-title {
  font-size: 0.85rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.timeline-subtitle {
  font-size: 0.7rem;
  color: #64748b;
}

.timeline-wrapper {
  margin-top: 10px;
  overflow-x: auto;
  padding-top: 20px; /* Space for current-time tooltip */
}

.timeline-main {
  display: flex;
  min-width: 750px;
  position: relative;
}

.timeline-labels-column {
  width: 90px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  border-right: 1px solid #e2e8f0;
}

.timeline-label-cell {
  height: 32px;
  font-size: 0.78rem;
  font-weight: 600;
  color: #475569;
  display: flex;
  align-items: center;
}

.axis-empty-cell {
  height: 24px;
}

.timeline-tracks-area {
  flex-grow: 1;
  position: relative;
  display: flex;
  flex-direction: column;
}

.track-cell {
  height: 32px;
  position: relative;
  display: flex;
  align-items: center;
  border-bottom: 1px dashed #f1f5f9;
}

.track-cell:nth-child(4) {
  border-bottom: 1px solid #cbd5e1; /* separate from axis */
}

/* Lane bars */
.lane-bar {
  position: absolute;
  height: 18px;
  border-radius: 4px;
  font-size: 0.65rem;
  font-weight: 700;
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  padding: 0 4px;
}

.sydney-bar {
  background: linear-gradient(90deg, #60a5fa, #3b82f6);
}

.tokyo-bar {
  background: linear-gradient(90deg, #34d399, #10b981);
}

.london-bar {
  background: linear-gradient(90deg, #f87171, #ef4444);
}

.newyork-bar {
  background: linear-gradient(90deg, #fb923c, #f97316);
}

/* Overlap background zones */
.zone-overlay {
  position: absolute;
  top: 0;
  bottom: 24px; /* Stop before axis */
  pointer-events: none;
  z-index: 1;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  padding-bottom: 4px;
}

.zone-golden {
  background: rgba(239, 68, 68, 0.05);
  border-left: 2px dashed rgba(239, 68, 68, 0.25);
  border-right: 2px dashed rgba(239, 68, 68, 0.25);
}

.zone-rollover {
  background: rgba(168, 85, 247, 0.05);
  border-left: 2px dashed rgba(168, 85, 247, 0.2);
  border-right: 2px dashed rgba(168, 85, 247, 0.2);
}

.zone-label-text {
  font-size: 0.58rem;
  font-weight: 800;
  letter-spacing: 0.5px;
  background: #ffffff;
  padding: 1px 4px;
  border-radius: 3px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}

.zone-golden .zone-label-text {
  color: #dc2626;
  border: 1px solid rgba(239, 68, 68, 0.2);
}

.zone-rollover .zone-label-text {
  color: #7e22ce;
  border: 1px solid rgba(168, 85, 247, 0.2);
}

/* Current time line vertical indicator */
.current-time-line {
  position: absolute;
  top: -12px;
  bottom: 0px;
  width: 2px;
  background-color: #dc2626;
  z-index: 10;
  box-shadow: 0 0 6px rgba(220, 38, 38, 0.4);
}

.current-time-line::after {
  content: '';
  position: absolute;
  top: 0;
  left: -3px;
  width: 8px;
  height: 8px;
  background-color: #dc2626;
  border-radius: 50%;
  box-shadow: 0 0 8px rgba(220, 38, 38, 0.8);
}

.time-marker-tooltip {
  position: absolute;
  top: -24px;
  transform: translateX(-50%);
  background: #dc2626;
  color: #ffffff;
  font-size: 0.65rem;
  font-weight: 800;
  padding: 2px 6px;
  border-radius: 4px;
  white-space: nowrap;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
}

.time-marker-tooltip::after {
  content: '';
  position: absolute;
  bottom: -4px;
  left: 50%;
  transform: translateX(-50%);
  border-width: 4px 4px 0;
  border-style: solid;
  border-color: #dc2626 transparent transparent;
  display: block;
  width: 0;
}

/* X-Axis Ticks */
.timeline-axis-ticks {
  height: 24px;
  position: relative;
  border-top: 1px solid #e2e8f0;
}

.axis-tick {
  position: absolute;
  top: 0;
  transform: translateX(-50%);
  display: flex;
  flex-direction: column;
  align-items: center;
}

.axis-tick::before {
  content: '';
  width: 1px;
  height: 5px;
  background: #cbd5e1;
}

.tick-label {
  font-size: 0.62rem;
  font-weight: 600;
  color: #64748b;
  margin-top: 2px;
}

/* Timeline Legend */
.timeline-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-top: 14px;
  padding-top: 10px;
  border-top: 1px dashed #f1f5f9;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.72rem;
  font-weight: 600;
  color: #64748b;
}

.legend-box {
  width: 12px;
  height: 12px;
  border-radius: 3px;
  display: inline-block;
}

.sydney-legend { background: linear-gradient(90deg, #60a5fa, #3b82f6); }
.tokyo-legend { background: linear-gradient(90deg, #34d399, #10b981); }
.london-legend { background: linear-gradient(90deg, #f87171, #ef4444); }
.newyork-legend { background: linear-gradient(90deg, #fb923c, #f97316); }
.golden-legend { background: rgba(239, 68, 68, 0.08); border: 1px dashed rgba(239, 68, 68, 0.5); }
.rollover-legend { background: rgba(168, 85, 247, 0.08); border: 1px dashed rgba(168, 85, 247, 0.5); }

/* Tips section */
.sessions-tips-panel {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 16px;
  text-align: left;
}

.tips-title {
  font-size: 0.8rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 10px 0;
}

.tips-list {
  margin: 0;
  padding-left: 18px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.tips-list li {
  font-size: 0.76rem;
  color: #475569;
  line-height: 1.45;
}
</style>
