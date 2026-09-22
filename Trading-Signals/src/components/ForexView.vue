<template>
  <div class="d-flex flex-column min-vh-100 stk-page-bg">
    <NavBar />
    <notifications />
    <div class="stk-page flex-grow-1">
      <div class="stk-container">
        <!-- Tab Navigation -->
        <div class="stk-tabs">
          <button class="stk-tab" :class="{ 'stk-tab--active': activeTab === 'potential' }" @click="activeTab = 'potential'">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3v18h18"/><path d="m19 9-5 5-4-4-3 3"/></svg>
            Potential Forex Pairs
          </button>
          <button class="stk-tab" :class="{ 'stk-tab--active': activeTab === 'prices' }" @click="activeTab = 'prices'">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
            Commodity Prices
          </button>
          <button class="stk-tab" :class="{ 'stk-tab--active': activeTab === 'rrg' }" @click="activeTab = 'rrg'">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10"/><path d="M2 12h20"/></svg>
            RRG Chart
          </button>
          <button class="stk-tab" :class="{ 'stk-tab--active': activeTab === 'cheatsheet' }" @click="activeTab = 'cheatsheet'">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/></svg>
            Forex Cheat Sheet
          </button>
        </div>

        <!-- Tab Content -->
        <div class="stk-content">
          <!-- ==================== COMMODITY PRICES TAB ==================== -->
          <div v-show="activeTab === 'prices'">
            <div class="stk-panel">
              <div class="stk-header">
                <div class="stk-header__icon">
                  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                </div>
                <div>
                  <h2 class="stk-header__title">Commodity Prices</h2>
                  <p class="stk-header__sub">Real-time price feeds for oils, gold, silver and metals</p>
                </div>
              </div>
              <div class="stk-section">
                <CurrencyPrices />
              </div>
            </div>
          </div>

          <!-- ==================== POTENTIAL FOREX PAIRS TAB ==================== -->
          <div v-show="activeTab === 'potential'">
            <div class="stk-panel">
              <div class="stk-header">
                <div class="stk-header__icon">
                  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3v18h18"/><path d="m19 9-5 5-4-4-3 3"/></svg>
                </div>
                <div>
                  <h2 class="stk-header__title">Forex Scanner</h2>
                  <p class="stk-header__sub">Discover divergent strengths, buy strong and sell weak currencies</p>
                </div>
              </div>

              <!-- Pair Input -->
              <div class="stk-section">
                <label class="stk-label">Search forex pair symbol</label>
                <div style="display:flex; gap:8px;">
                  <input
                    type="text"
                    class="stk-input"
                    v-model="pairInputText"
                    @keydown.enter="updateSelectedPair"
                    @input="pairInputText = $event.target.value.toUpperCase()"
                    placeholder="Enter pair (e.g. EURUSD) and press Enter"
                  />
                  <button class="stk-btn stk-btn--primary" @click="updateSelectedPair" :disabled="!pairInputText || !pairInputText.trim()">
                    View
                  </button>
                </div>
              </div>
            </div>

            <!-- Before You Trade Checklist (5s review) -->
            <div class="stk-panel stk-checklist-panel" style="margin-bottom: 20px;">
              <div class="stk-checklist-header" @click="showChecklist = !showChecklist">
                <div style="display: flex; align-items: center; gap: 8px;">
                  <span class="stk-checklist-icon">🧠</span>
                  <h4 class="stk-checklist-title">Before You Trade Checklist (5s Review)</h4>
                </div>
                <div class="stk-checklist-arrow">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" :class="{ 'stk-chevron--open': showChecklist }"><polyline points="6 9 12 15 18 9"/></svg>
                </div>
              </div>
              <div v-show="showChecklist" class="stk-checklist-body">
                <div class="stk-checklist-items">
                  <label class="stk-checklist-item" :class="{ 'stk-checklist-item--checked': checklist.dxy }">
                    <input type="checkbox" v-model="checklist.dxy" />
                    <span class="stk-checklist-text"><strong>1. DXY đang thế nào?</strong> DXY tăng = Sell EURUSD/GBPUSD/AUDUSD | Buy USDJPY/USDCAD.</span>
                  </label>
                  <label class="stk-checklist-item" :class="{ 'stk-checklist-item--checked': checklist.risk }">
                    <input type="checkbox" v-model="checklist.risk" />
                    <span class="stk-checklist-text"><strong>2. Tâm lý Risk-On / Risk-Off?</strong> Risk-On = Buy AUDUSD, NZDUSD. Risk-Off = Buy XAUUSD, CHF, JPY.</span>
                  </label>
                  <label class="stk-checklist-item" :class="{ 'stk-checklist-item--checked': checklist.oil }">
                    <input type="checkbox" v-model="checklist.oil" />
                    <span class="stk-checklist-text"><strong>3. Giá dầu mỏ (Oil) tăng hay giảm?</strong> Oil tăng = CAD mạnh (Sell USDCAD), Oil giảm = CAD yếu (Buy USDCAD).</span>
                  </label>
                  <label class="stk-checklist-item" :class="{ 'stk-checklist-item--checked': checklist.bond }">
                    <input type="checkbox" v-model="checklist.bond" />
                    <span class="stk-checklist-text"><strong>4. Lợi suất Bond Mỹ 10Y?</strong> Lợi suất tăng = USDJPY tăng (Yen yếu), XAUUSD giảm (vàng yếu).</span>
                  </label>
                  <label class="stk-checklist-item" :class="{ 'stk-checklist-item--checked': checklist.news }">
                    <input type="checkbox" v-model="checklist.news" />
                    <span class="stk-checklist-text"><strong>5. Tin tức đỏ (High Impact) sắp tới?</strong> Tránh vào lệnh trong 30 phút trước/sau tin mạnh (CPI, NFP, FOMC).</span>
                  </label>
                </div>
                <div class="stk-checklist-footer">
                  <button class="stk-btn stk-btn--outline" @click="resetChecklist" style="padding: 4px 12px; font-size: 0.75rem; border-radius: 6px;">
                    Reset Checklist
                  </button>
                  <span class="stk-checklist-tip">💡 Tích đủ 5 ô để chắc chắn không đi ngược dòng tiền lớn!</span>
                </div>
              </div>
            </div>

            <!-- Sticky Chart Panel (Matches Crypto) -->
            <div ref="chartRef" class="stk-sticky-chart" v-if="selectedPair && activeTab === 'potential'">
              <div class="stk-chart-wrap">
                <TradingViewChart :coin="selectedPairSymbol" :height="380" />
              </div>
              <!-- Price Alert Toggle -->
              <div class="stk-alert-toggle">
                <button class="stk-alert-toggle__btn" @click="showPriceAlert = !showPriceAlert">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>
                  Price Alert
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" :class="{ 'stk-chevron--open': showPriceAlert }"><polyline points="6 9 12 15 18 9"/></svg>
                </button>
                <div v-show="showPriceAlert" class="stk-alert-content">
                  <PriceAlertWidget :symbol="selectedPair" assetType="forex" />
                </div>
              </div>
            </div>

            <!-- Potential Forex Pairs List -->
            <div class="stk-panel" v-if="forexPairs.length > 0">
              <div class="stk-section stk-section--potential">
                <div class="stk-section-head">
                  <h3 class="stk-section-head__title">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                    Potential Forex Pairs
                  </h3>
                  <span v-if="latestUpdated" class="stk-updated">
                    Updated: {{ formatDateTime(latestUpdated) }}
                  </span>
                </div>

                <div ref="tableWrapRef" class="stk-table-wrap stk-table-wrap--scroll">
                  <table class="stk-table">
                    <thead>
                      <tr>
                        <th class="stk-th">Pair</th>
                        <th class="stk-th stk-th--center">Action</th>
                        <th class="stk-th stk-th--right">Score Diff</th>
                        <th class="stk-th">Note</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr
                        v-for="pair in forexPairs"
                        :key="pair.pair"
                        class="stk-row"
                        :class="{ 'stk-row--active': isRowActive(pair) }"
                        @click="selectPair(pair)"
                      >
                        <td class="stk-td stk-td--symbol" :title="`View ${pair.pair} chart`">{{ pair.pair }}</td>
                        <td class="stk-td stk-td--center">
                          <span class="stk-signal" :class="pair.action === 'Buy' ? 'stk-signal--buy' : 'stk-signal--sell'">
                            {{ pair.action }}
                          </span>
                        </td>
                        <td class="stk-td stk-td--right" style="font-weight: 600;">
                          {{ pair.score_diff.toFixed(2) }}%
                        </td>
                        <td class="stk-td">{{ pair.note || '-' }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>

                <div class="stk-actions">
                  <div style="display: flex; gap: 10px; align-items: center; justify-content: center; width: 100%;">
                    <button class="stk-btn stk-btn--primary stk-btn--scan" @click="scanForexPairs" :disabled="isScanning">
                      <span v-if="isScanning" class="stk-spinner" style="width: 14px; height: 14px; margin: 0; display: inline-block; border-width: 2px;"></span>
                      <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                      Start Scanning
                    </button>
                    <button class="stk-btn stk-btn--outline" @click="runSSHScript('forex_potential')" :disabled="isRunningPotentialScript" style="border-color: #3b82f6; color: #3b82f6;">
                      <span v-if="isRunningPotentialScript" class="stk-spinner" style="width: 14px; height: 14px; margin: 0; display: inline-block; border-width: 2px;"></span>
                      <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                      Run script
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <div v-else-if="!isScanning && scanAttempted" class="stk-message">
              No forex pairs found. Please run the scan.
            </div>

            <div v-if="isScanning" class="stk-loading">
              <div class="stk-spinner"></div>
            </div>
          </div>

          <!-- ==================== RRG CHART TAB ==================== -->
          <div v-show="activeTab === 'rrg'">
            <div class="stk-panel">
              <div class="stk-header">
                <div class="stk-header__icon">
                  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 2a15 15 0 0 1 4 10 15 15 0 0 1-4 10"/><path d="M2 12h20"/></svg>
                </div>
                <div>
                  <h2 class="stk-header__title">Forex RRG Chart</h2>
                  <p class="stk-header__sub">Relative Rotation Graph for major Forex pairs against USD</p>
                </div>
              </div>
              <div class="stk-rrg-wrap">
                <div class="stk-rrg-actions" style="margin-bottom: 20px; display: flex; justify-content: center;">
                  <button
                    class="stk-btn stk-btn--primary"
                    @click="runSSHScript('forex_rrg')"
                    :disabled="isRunningRrgScript"
                    style="min-width: 180px; justify-content: center;"
                  >
                    <span v-if="isRunningRrgScript" class="stk-spinner" style="width: 16px; height: 16px; border-top-color: #fff; margin: 0; display: inline-block; border-width: 2px;"></span>
                    <span v-else style="display: inline-flex; align-items: center; gap: 6px;">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21.5 2v6h-6M21.34 15.57a10 10 0 1 1-.57-8.38l5.67-5.67"/></svg>
                      Generate RRG Chart
                    </span>
                  </button>
                </div>
                <img :src="forexRRGUrl" class="stk-rrg-img" alt="Forex RRG Chart" />
                <p class="stk-message" style="margin-top: 10px;">
                  Relative Rotation Graph (RRG) tracks the strength and momentum of currencies. Updated daily.
                </p>
              </div>
            </div>
          </div>

          <!-- ==================== FOREX CHEAT SHEET TAB ==================== -->
          <div v-show="activeTab === 'cheatsheet'">
            <div class="stk-panel" style="margin-bottom: 24px;">
              <div class="stk-header" style="background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);">
                <div class="stk-header__icon" style="color: #3b82f6;">
                  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
                </div>
                <div>
                  <h2 class="stk-header__title">Forex Cheat Sheet &amp; Personality Hub</h2>
                  <p class="stk-header__sub">Understand the character, correlations, and compatibility of major currency pairs before trading</p>
                </div>
              </div>
            </div>

            <!-- Forex Sessions & Volatility Hours -->
            <ForexSessions />

            <!-- Checklist Quick Reference Matrix -->
            <div class="stk-cheatsheet-grid">
              <!-- Card 1: Market Sentiment Matrix -->
              <div class="stk-panel">
                <div class="stk-section-head" style="padding: 16px 20px 0;">
                  <h3 class="stk-section-head__title">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z"/><line x1="4" y1="22" x2="4" y2="15"/></svg>
                    Market Sentiment &amp; Correlation Matrix
                  </h3>
                </div>
                <div class="stk-section" style="padding-top: 12px;">
                  <div class="stk-table-wrap">
                    <table class="stk-table">
                      <thead>
                        <tr>
                          <th class="stk-th">Thị trường</th>
                          <th class="stk-th">Mạnh nhất / Buy</th>
                          <th class="stk-th">Yếu nhất / Sell</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 600;">Kinh tế Mỹ mạnh (Lãi suất ↑)</td>
                          <td class="stk-td"><span class="stk-signal stk-signal--buy">DXY, USDJPY, USDCAD</span></td>
                          <td class="stk-td"><span class="stk-signal stk-signal--sell">EURUSD, GBPUSD, AUDUSD</span></td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 600;">Risk-On (Chứng khoán ↑, Lạc quan)</td>
                          <td class="stk-td"><span class="stk-signal stk-signal--buy">AUDUSD, NZDUSD</span></td>
                          <td class="stk-td"><span class="stk-signal stk-signal--sell">USDCHF, USDJPY</span></td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 600;">Risk-Off (Chứng khoán ↓, Lo sợ)</td>
                          <td class="stk-td"><span class="stk-signal stk-signal--buy">USDCHF, XAUUSD, USDJPY</span></td>
                          <td class="stk-td"><span class="stk-signal stk-signal--sell">AUDUSD, NZDUSD</span></td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 600;">Giá dầu tăng (Oil ↑)</td>
                          <td class="stk-td"><span class="stk-signal stk-signal--buy">CAD (USDCAD giảm)</span></td>
                          <td class="stk-td"><span class="stk-signal stk-signal--sell">USDCAD</span></td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 600;">Chiến tranh &amp; Khủng hoảng hoãn loạn</td>
                          <td class="stk-td"><span class="stk-signal stk-signal--buy">XAUUSD, CHF, JPY</span></td>
                          <td class="stk-td"><span class="stk-signal stk-signal--sell">AUDUSD, NZDUSD</span></td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>

              <!-- Card 2: EA & Trader Behavioral Ratings -->
              <div class="stk-panel">
                <div class="stk-section-head" style="padding: 16px 20px 0;">
                  <h3 class="stk-section-head__title">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="12 2 2 7 12 12 22 7 12 2"/><polyline points="2 17 12 22 22 17"/><polyline points="2 12 12 17 22 12"/></svg>
                    Behavioral Ratings &amp; EA Target
                  </h3>
                </div>
                <div class="stk-section" style="padding-top: 12px;">
                  <div class="stk-table-wrap">
                    <table class="stk-table">
                      <thead>
                        <tr>
                          <th class="stk-th">Cặp tiền</th>
                          <th class="stk-th">Độ ngoan / Cảnh báo</th>
                          <th class="stk-th">Giao dịch / EA phù hợp</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 700; color: #2563eb;">EURUSD</td>
                          <td class="stk-td"><span class="stk-badge stk-badge--green">🟢 Rất ngoan</span></td>
                          <td class="stk-td">Price Action (Bob Volman), SMC, Scalping, Learn EA first</td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 700; color: #2563eb;">USDJPY</td>
                          <td class="stk-td"><span class="stk-badge stk-badge--green">🟢 Ngoan</span></td>
                          <td class="stk-td">Trend following, Scalping (Lãi suất chênh lệch Mỹ-Nhật)</td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 700; color: #2563eb;">AUDUSD</td>
                          <td class="stk-td"><span class="stk-badge stk-badge--green">🟢 Ngoan</span></td>
                          <td class="stk-td">Swing trade (Theo chứng khoán &amp; Trung Quốc), Commodity</td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 700; color: #2563eb;">USDCAD</td>
                          <td class="stk-td"><span class="stk-badge stk-badge--yellow">🟡 Trung bình</span></td>
                          <td class="stk-td">Trend/Swing trade (Đi theo giá dầu mỏ)</td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 700; color: #2563eb;">NZDUSD</td>
                          <td class="stk-td"><span class="stk-badge stk-badge--yellow">🟡 Trung bình</span></td>
                          <td class="stk-td">Swing trade (Giống AUD nhưng đi chậm &amp; spread cao hơn)</td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 700; color: #2563eb;">USDCHF</td>
                          <td class="stk-td"><span class="stk-badge stk-badge--orange">🟠 Khó chịu</span></td>
                          <td class="stk-td">Ninja - Trú ẩn an toàn, đi chậm, cẩn thận đảo chiều bất ngờ</td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 700; color: #2563eb;">GBPUSD</td>
                          <td class="stk-td"><span class="stk-badge stk-badge--red">🔴 Hay quét SL</span></td>
                          <td class="stk-td">Gã say rượu - Giảm lot 50%, RR lớn nhưng quét SL rất rát</td>
                        </tr>
                        <tr class="stk-row-static">
                          <td class="stk-td" style="font-weight: 700; color: #2563eb;">XAUUSD</td>
                          <td class="stk-td"><span class="stk-badge stk-badge--toxic">☠️ Cháy nhanh nhất</span></td>
                          <td class="stk-td">Đừng FOMO! Biến động cực mạnh, quét 2 đầu, tránh khi mới học</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </div>

            <!-- Forex Pair Profiles -->
            <div class="stk-panel">
              <div class="stk-section-head" style="padding: 20px 24px 0;">
                <h3 class="stk-section-head__title">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                  Forex Symbol Profiles (Tính cách chi tiết)
                </h3>
              </div>
              <div class="stk-section" style="padding-top: 16px;">
                <div class="stk-profiles-grid">
                  <div class="stk-profile-card" v-for="profile in pairProfiles" :key="profile.symbol">
                    <div class="stk-profile-card__header">
                      <span class="stk-profile-symbol">{{ profile.symbol }}</span>
                      <span class="stk-profile-nickname">{{ profile.nickname }}</span>
                    </div>
                    <div class="stk-profile-card__content">
                      <div class="stk-profile-row">
                        <span class="stk-profile-label">Tính cách:</span>
                        <span class="stk-profile-value">{{ profile.personality }}</span>
                      </div>
                      <div class="stk-profile-row">
                        <span class="stk-profile-label">Ưa thích (Buy khi):</span>
                        <span class="stk-profile-value" style="color: #059669;">{{ profile.likes }}</span>
                      </div>
                      <div class="stk-profile-row">
                        <span class="stk-profile-label">Bất lợi (Tránh/Sell khi):</span>
                        <span class="stk-profile-value" style="color: #dc2626;">{{ profile.dislikes }}</span>
                      </div>
                      <div class="stk-profile-row">
                        <span class="stk-profile-label">Đề xuất:</span>
                        <span class="stk-profile-value" style="font-weight: 600;">{{ profile.advice }}</span>
                      </div>

                      <!-- Exness CFD Setup Note -->
                      <div class="stk-profile-setup-box">
                        <div class="stk-profile-row stk-profile-setup-box__full" v-if="profile.symbol === 'DXY'">
                          <span class="stk-profile-label" style="color: #1e40af;">Chỉ số tham chiếu:</span>
                          <span class="stk-profile-value" style="font-weight: 600; color: #1e3a8a;">{{ profile.entryZone }}</span>
                        </div>
                        <template v-else>
                          <div class="stk-profile-row stk-profile-setup-box__full">
                            <span class="stk-profile-label" style="color: #1e40af;">Vùng mua (Entry):</span>
                            <span class="stk-profile-value" style="font-weight: 600; color: #1e3a8a;">{{ profile.entryZone }}</span>
                          </div>
                          <div class="stk-profile-row">
                            <span class="stk-profile-label" style="color: #4f46e5;">Lot size / $1000:</span>
                            <span class="stk-profile-value" style="font-weight: 700; color: #4338ca;">{{ profile.lotSize }}</span>
                          </div>
                          <div class="stk-profile-row">
                            <span class="stk-profile-label" style="color: #9a3412;">Cắt lỗ (SL):</span>
                            <span class="stk-profile-value" style="font-weight: 700; color: #9a3412;">{{ profile.slRule }}</span>
                          </div>
                          <div class="stk-profile-row stk-profile-setup-box__full">
                            <span class="stk-profile-label" style="color: #166534;">Chốt lời (TP):</span>
                            <span class="stk-profile-value" style="font-weight: 700; color: #14532d;">{{ profile.tpRule }}</span>
                          </div>
                        </template>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <AppFooter />
  </div>
</template>

<script>
import NavBar from './NavBar.vue';
import AppFooter from './AppFooter.vue';
import CurrencyPrices from './CurrencyPrices.vue';
import TradingViewChart from './TradingViewChart.vue';
import PriceAlertWidget from './PriceAlertWidget.vue';
import ForexSessions from './ForexSessions.vue';
import { ref, onMounted, computed, onUnmounted, nextTick } from 'vue';
import axios from 'axios';
import { useNotification } from "@kyvg/vue3-notification";

export default {
  components: {
    NavBar,
    AppFooter,
    CurrencyPrices,
    TradingViewChart,
    PriceAlertWidget,
    ForexSessions
  },
  setup() {
    const { notify } = useNotification();
    const activeTab = ref('potential');
    
    // Forex pairs state
    const forexPairs = ref([]);
    const isScanning = ref(false);
    const scanAttempted = ref(false);
    const latestUpdated = ref(null);
    const selectedPair = ref('EURUSD');
    const selectedRowKey = ref('');
    const pairInputText = ref('');
    const showPriceAlert = ref(false);
    
    // Before-trade checklist states
    const showChecklist = ref(true);
    const checklist = ref({
      dxy: false,
      risk: false,
      oil: false,
      bond: false,
      news: false
    });
    
    const resetChecklist = () => {
      checklist.value = {
        dxy: false,
        risk: false,
        oil: false,
        bond: false,
        news: false
      };
    };

    const pairProfiles = [
      {
        symbol: 'DXY',
        nickname: 'Ông trùm USD',
        personality: 'Chậm, quyền lực, dẫn dắt toàn thị trường Forex. Đi xu hướng dài hạn.',
        likes: 'Fed tăng lãi suất, kinh tế Mỹ mạnh mẽ, số liệu CPI & NFP vượt kỳ vọng.',
        dislikes: 'Fed giảm lãi suất, các chính sách nới lỏng tiền tệ, USD suy yếu.',
        advice: 'Luôn nhìn DXY đầu tiên để xác định hướng đi cho các cặp tiền tệ đuôi USD.',
        lotSize: 'N/A',
        entryZone: 'Chỉ báo tham chiếu (Không giao dịch trực tiếp bằng lot)',
        slRule: 'N/A',
        tpRule: 'N/A'
      },
      {
        symbol: 'EURUSD',
        nickname: 'Quý ông lịch lãm',
        personality: 'Đi sạch, kỹ thuật đẹp, tôn trọng các vùng hỗ trợ/kháng cự. Thích hợp cho người mới và EA.',
        likes: 'Cấu trúc Price Action sạch, giao dịch theo Bob Volman, SMC hoặc Supply/Demand.',
        dislikes: 'Tin tức bất ngờ đột ngột từ cả EU và Mỹ, các cuộc khủng hoảng tài chính châu Âu.',
        advice: 'Tỷ lệ fake breakout cao nhưng sau khi phá vỡ giả thường chạy xu hướng rất đẹp.',
        lotSize: '0.08 - 0.10 lot / $1,000 vốn (Tài khoản Pro/Zero)',
        entryZone: 'Pullback EMA 21/50 (H1/H4) hoặc Discount Zone (Fibo > 0.5)',
        slRule: 'Dưới Swing Low 15 - 20 pips (Lỗ ~12$ - 20$)',
        tpRule: 'Target R:R = 1:2 hoặc kháng cự H1/H4 gần nhất'
      },
      {
        symbol: 'GBPUSD',
        nickname: 'Gã say rượu (Cable)',
        personality: 'Nóng tính, biên độ biến động cực kỳ lớn, chuyên quét stop-loss rất rát.',
        likes: 'Giao dịch có tin tức kích thích momentum mạnh, các mô hình hồi quy để bắt RR lớn.',
        dislikes: 'Giai đoạn thị trường đi ngang (Sideway) tích lũy khó chịu.',
        advice: 'Giảm khối lượng lệnh (lot size) xuống 50% so với EURUSD để bảo toàn vốn.',
        lotSize: '0.04 - 0.05 lot / $1,000 vốn (Biên độ mạnh, giảm 50% lot)',
        entryZone: 'Quét thanh khoản (Liquidity Sweep) Á/Âu, Order Block H1/H4',
        slRule: 'Dưới Swing Low/OB từ 25 - 30 pips (Lỗ ~10$ - 15$)',
        tpRule: 'Target R:R = 1:2.5 hoặc 1:3'
      },
      {
        symbol: 'USDJPY',
        nickname: 'Samurai',
        personality: 'Đi xu hướng cực mạnh và rất dài, pullback điều chỉnh thường nông và ít.',
        likes: 'Lợi suất trái phiếu Mỹ tăng mạnh, chênh lệch chính sách lãi suất giữa Fed và BOJ rộng.',
        dislikes: 'Ngân hàng Trung ương Nhật Bản (BOJ) can thiệp đột xuất (có thể giật 200-500 pips).',
        advice: 'Thích hợp nhất cho chiến lược Trend Following (giao dịch theo xu hướng dài).',
        lotSize: '0.08 - 0.10 lot / $1,000 vốn',
        entryZone: 'Môi trường DXY & US10Y tăng, mua khi hồi về hỗ trợ H4',
        slRule: 'Dưới Swing Low 20 - 25 pips (Lỗ ~14$ - 25$)',
        tpRule: 'Target R:R = 1:2 hoặc đỉnh cũ H4/D1'
      },
      {
        symbol: 'USDCHF',
        nickname: 'Ninja',
        personality: 'Trầm lắng, đi chậm, âm thầm nhưng rất khó lường. Xem là tài sản trú ẩn an toàn.',
        likes: 'Tâm lý thị trường lo sợ rủi ro toàn cầu (Risk-Off), khủng hoảng chính trị.',
        dislikes: 'Thị trường hưng phấn lạc quan (Risk-On), tiền tệ dòng chảy chuyển dịch nhanh.',
        advice: 'Thường đi ngược EURUSD, cấu trúc kỹ thuật không đẹp bằng EURUSD, giao dịch cẩn thận.',
        lotSize: '0.08 - 0.10 lot / $1,000 vốn',
        entryZone: 'Tâm lý Risk-Off hoặc EURUSD chạm kháng cự mạnh',
        slRule: 'Dưới hỗ trợ kỹ thuật H1/H4 từ 15 - 20 pips (Lỗ ~12$ - 20$)',
        tpRule: 'Target R:R = 1:1.5 hoặc 1:2'
      },
      {
        symbol: 'USDCAD',
        nickname: 'Thợ mỏ dầu',
        personality: 'Đi theo sự chuyển dịch của thị trường hàng hóa, đặc biệt bị giá dầu mỏ chi phối.',
        likes: 'Giá dầu mỏ giảm sâu (CAD yếu dẫn đến USDCAD tăng mạnh).',
        dislikes: 'Giá dầu mỏ tăng trưởng mạnh mẽ (CAD mạnh dẫn đến USDCAD giảm sâu).',
        advice: 'Bắt buộc phải xem biểu đồ dầu thô (WTI/Brent) trước khi quyết định vào lệnh.',
        lotSize: '0.08 - 0.10 lot / $1,000 vốn',
        entryZone: 'Khi giá Dầu giảm + DXY tăng, canh mua tại Key Level H4',
        slRule: 'Dưới hỗ trợ cứng H4 từ 20 - 25 pips (Lỗ ~14$ - 25$)',
        tpRule: 'Target R:R = 1:2 hoặc kháng cự H4 gần nhất'
      },
      {
        symbol: 'AUDUSD',
        nickname: 'Kẻ liều lĩnh',
        personality: 'Nhạy cảm cực độ với tâm lý rủi ro (Risk-On), kinh tế Trung Quốc và thị trường chứng khoán.',
        likes: 'Thị trường chứng khoán tăng mạnh, kinh tế Trung Quốc phục hồi tốt.',
        dislikes: 'Tâm lý phòng thủ lo ngại rủi ro (Risk-Off), khủng hoảng kinh tế toàn cầu.',
        advice: 'Phù hợp cho các lệnh giao dịch trung hạn (Swing Trade) vì xu hướng tương đối mượt.',
        lotSize: '0.08 - 0.10 lot / $1,000 vốn',
        entryZone: 'Tâm lý Risk-On, mua tại hỗ trợ H1/H4 có từ chối giá',
        slRule: 'Dưới Swing Low 15 - 20 pips (Lỗ ~12$ - 20$)',
        tpRule: 'Target R:R = 1:2 hoặc đỉnh cũ'
      },
      {
        symbol: 'NZDUSD',
        nickname: 'Em trai AUD',
        personality: 'Tính cách tương đồng với AUDUSD nhưng thanh khoản thấp hơn và đi chậm hơn.',
        likes: 'Thị trường lạc quan (Risk-On), kinh tế nông nghiệp/thương mại phát triển.',
        dislikes: 'Thị trường sợ hãi (Risk-Off), đô la Mỹ tăng giá mạnh mẽ.',
        advice: 'Biên độ nhỏ hơn, spread cao hơn một chút, có thể đóng vai trò chỉ báo đi sau AUD.',
        lotSize: '0.08 - 0.10 lot / $1,000 vốn',
        entryZone: 'Theo chân AUDUSD (chờ AUDUSD bứt phá trước)',
        slRule: 'Dưới hỗ trợ H1/H4 từ 15 - 20 pips (Lỗ ~12$ - 20$)',
        tpRule: 'Target R:R = 1:2'
      },
      {
        symbol: 'XAUUSD',
        nickname: 'Con quái vật (Gold)',
        personality: 'Tính cách khó lường nhất, giật cực mạnh, quét thanh khoản 2 đầu trước khi chạy.',
        likes: 'Lạm phát phi mã, khủng hoảng địa chính trị, chiến tranh, dòng tiền phòng thủ.',
        dislikes: 'Đồng USD tăng mạnh mẽ, lợi suất trái phiếu thực tế của Mỹ (Real Yield) tăng cao.',
        advice: 'TUYỆT ĐỐI KHÔNG FOMO. Đi lệnh nhỏ, cắt lỗ xa. Người mới làm quen EA nên tránh xa.',
        lotSize: '0.01 - 0.02 lot / $1,000 vốn (Tối đa 0.05 lot, cực kỳ rủi ro)',
        entryZone: 'Quét thanh khoản Á/Âu, tin địa chính trị/lạm phát, OB H4/D1',
        slRule: 'Rộng từ 40 - 60 pips dưới đáy cũ (Lỗ ~4$ - 12$, tối đa 30$ nếu đi 0.05 lot)',
        tpRule: 'Target R:R = 1:3 hoặc đỉnh cũ H4/D1'
      }
    ];
    
    const chartRef = ref(null);
    const tableWrapRef = ref(null);

    onMounted(async () => {
      window.addEventListener('keydown', handleArrowNavigation);
      
      // Auto-fetch potential forex pairs on mount
      scanForexPairs();

      notify({ type: "info", title: "Forex Interface", text: "Successfully switched to Forex interface." });
    });

    onUnmounted(() => {
      window.removeEventListener('keydown', handleArrowNavigation);
    });

    const getTradingViewSymbol = (pair) => {
      if (!pair) return 'FX:EURUSD';
      
      const symbolMap = {
        'WTI': 'TVC:USOIL',
        'USOIL': 'TVC:USOIL',
        'UKOIL': 'TVC:UKOIL',
        'XAUUSD': 'OANDA:XAUUSD',
        'XAGUSD': 'OANDA:XAGUSD'
      };
      
      return symbolMap[pair] || `FX:${pair}`;
    };

    const selectedPairSymbol = computed(() => {
      return getTradingViewSymbol(selectedPair.value);
    });



    const scanForexPairs = async () => {
      isScanning.value = true;
      scanAttempted.value = true;
      try {
        const response = await axios.get('/getPotentialForexPairs');
        forexPairs.value = response.data.data || [];
        latestUpdated.value = response.data.latest_updated;
      } catch (error) {
        console.error('Error fetching forex pairs:', error);
        forexPairs.value = [];
      } finally {
        isScanning.value = false;
      }
    };

    const formatDateTime = (dateString) => {
      if (!dateString) return '';
      // Server stores naive datetime in UTC+7 (Asia/Bangkok)
      // Add timezone offset so JavaScript parses it correctly, then
      // toLocaleString() will convert to the user's local timezone
      const isoString = dateString.replace(' ', 'T') + '+07:00';
      const date = new Date(isoString);
      if (isNaN(date.getTime())) {
        // Fallback: try parsing as-is
        return new Date(dateString).toLocaleString();
      }
      return date.toLocaleString();
    };

    const getRowKey = (pair) => pair.pair;

    const isRowActive = (pair) => {
      if (selectedRowKey.value) return selectedRowKey.value === getRowKey(pair);
      return selectedPair.value === pair.pair;
    };

    const selectPair = (pair, shouldScroll = true) => {
      selectedPair.value = pair.pair;
      selectedRowKey.value = getRowKey(pair);

      if (!shouldScroll) return;
      setTimeout(() => {
        if (chartRef.value) {
          const y = chartRef.value.getBoundingClientRect().top + window.scrollY - 120;
          window.scrollTo({ top: y, behavior: 'smooth' });
        }
      }, 100);
    };

    const scrollActiveRowIntoView = async () => {
      await nextTick();
      const container = tableWrapRef.value;
      if (!container) return;
      const activeRow = container.querySelector('.stk-row--active');
      if (activeRow) activeRow.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    };

    const moveSelection = (direction) => {
      const rows = forexPairs.value;
      if (!rows.length) return;

      let currentIndex = rows.findIndex(r => getRowKey(r) === selectedRowKey.value);
      if (currentIndex === -1) {
        currentIndex = rows.findIndex(r => r.pair === selectedPair.value);
      }
      const baseIndex = currentIndex === -1 ? (direction > 0 ? -1 : 0) : currentIndex;
      const nextIndex = (baseIndex + direction + rows.length) % rows.length;
      const nextRow = rows[nextIndex];

      if (nextRow) {
        selectPair(nextRow, false);
        scrollActiveRowIntoView();
      }
    };

    const isTypingTarget = (target) => {
      if (!target) return false;
      const tag = (target.tagName || '').toLowerCase();
      return tag === 'input' || tag === 'textarea' || tag === 'select' || target.isContentEditable;
    };

    const handleArrowNavigation = (event) => {
      if (event.key !== 'ArrowDown' && event.key !== 'ArrowUp') return;
      if (isTypingTarget(event.target)) return;
      if (activeTab.value !== 'potential') return;

      const direction = event.key === 'ArrowDown' ? 1 : -1;
      moveSelection(direction);
      event.preventDefault();
    };

    const updateSelectedPair = () => {
      const input = pairInputText.value.trim().toUpperCase();
      if (input) {
        selectedPair.value = input;
        selectedRowKey.value = '';
        notify({ type: "success", title: "Chart Updated", text: `Switched to ${input}` });
      }
    };

    // SSH script execution states
    const isRunningPotentialScript = ref(false);
    const isRunningRrgScript = ref(false);
    const forexRRGKey = ref(Date.now());
    const forexRRGUrl = computed(() => `/forex_rrgchart?t=${forexRRGKey.value}`);

    const runSSHScript = async (scriptType) => {
      const isRrg = scriptType === 'forex_rrg';
      if (isRrg) {
        isRunningRrgScript.value = true;
      } else {
        isRunningPotentialScript.value = true;
      }

      try {
        const response = await fetch('/runSSHScript', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ script_type: scriptType }),
        });
        const data = await response.json();
        if (response.ok && data.success) {
          notify({
            type: 'success',
            title: 'Success',
            text: isRrg ? 'Forex RRG Chart has been updated successfully!' : 'Forex scanner script executed successfully!',
          });
          if (isRrg) {
            forexRRGKey.value = Date.now();
          } else {
            scanForexPairs();
          }
        } else {
          throw new Error(data.error || 'Server returned an error');
        }
      } catch (error) {
        console.error('Error running SSH script:', error);
        notify({
          type: 'error',
          title: 'Execution Failed',
          text: error.message || 'Failed to connect or run the SSH script.',
        });
      } finally {
        if (isRrg) {
          isRunningRrgScript.value = false;
        } else {
          isRunningPotentialScript.value = false;
        }
      }
    };

    return {
      activeTab,
      forexPairs,
      isScanning,
      scanAttempted,
      latestUpdated,
      scanForexPairs,
      formatDateTime,
      selectedPair,
      selectedPairSymbol,
      selectPair,
      isRowActive,
      getRowKey,
      pairInputText,
      updateSelectedPair,
      showPriceAlert,
      showChecklist,
      checklist,
      resetChecklist,
      pairProfiles,
      chartRef,
      tableWrapRef,
      isRunningPotentialScript,
      isRunningRrgScript,
      forexRRGUrl,
      runSSHScript
    };
  },
};
</script>

<style scoped>
/* ============================== */
/*  FOREX PAGE – Matches Stock/Crypto */
/* ============================== */

.stk-page-bg {
  background-color: #ffffff;
}

.stk-page { background: #ffffff; padding: 20px 0 40px; }
.stk-container { max-width: 1280px; margin: 0 auto; padding: 0 24px; }

/* ---------- TABS ---------- */
.stk-tabs { display: flex; gap: 6px; margin-bottom: 20px; overflow-x: auto; scrollbar-width: none; }
.stk-tabs::-webkit-scrollbar { display: none; }
.stk-tab {
  display: inline-flex; align-items: center; gap: 6px; padding: 10px 20px;
  border: 1px solid rgba(0, 0, 0, 0.06); border-radius: 10px; font-size: 0.88rem; font-weight: 600;
  color: #475569; background: rgba(0, 0, 0, 0.02); cursor: pointer; transition: all 0.2s ease;
  white-space: nowrap; box-shadow: 0 1px 3px rgba(0,0,0,0.03);
}
.stk-tab:hover { color: #0f172a; background: rgba(0, 0, 0, 0.05); }
.stk-tab--active { color: #fff !important; background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%) !important; box-shadow: 0 4px 12px rgba(59, 130, 246, 0.2) !important; border-color: rgba(0, 0, 0, 0.05); }

/* ---------- PANEL ---------- */
.stk-panel { background: #ffffff; border: 1px solid rgba(0, 0, 0, 0.06); border-radius: 16px; box-shadow: 0 8px 32px rgba(0,0,0,0.04); overflow: hidden; margin-bottom: 20px; }

/* ---------- HEADER ---------- */
.stk-header { display: flex; align-items: center; gap: 14px; padding: 22px 24px; background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); color: #0f172a; border-bottom: 1px solid rgba(0, 0, 0, 0.06); }
.stk-header__icon { width: 44px; height: 44px; border-radius: 12px; background: #ffffff; border: 1px solid rgba(0, 0, 0, 0.08); display: flex; align-items: center; justify-content: center; flex-shrink: 0; color: #0f172a; }
.stk-header__title { font-size: 1.2rem; font-weight: 700; margin: 0; line-height: 1.3; font-family: 'Outfit', sans-serif; color: #0f172a; }
.stk-header__sub { font-size: 0.82rem; color: #475569; margin: 2px 0 0; }

/* ---------- SECTIONS ---------- */
.stk-section { padding: 20px 24px; }
.stk-section--potential { border-top: 1px solid rgba(0, 0, 0, 0.06); }
.stk-label { display: block; font-size: 0.82rem; font-weight: 600; color: #475569; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
.stk-section-head { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 8px; margin-bottom: 14px; }
.stk-section-head__title { display: inline-flex; align-items: center; gap: 8px; font-size: 1rem; font-weight: 700; color: #0f172a; margin: 0; font-family: 'Outfit', sans-serif; }
.stk-updated { font-size: 0.75rem; color: #64748b; font-weight: 500; }

/* ---------- CHART (sticky) ---------- */
.stk-sticky-chart { position: sticky; top: 60px; z-index: 20; background: #ffffff; padding: 12px 0; margin-bottom: 12px; }
.stk-chart-wrap { position: relative; border-radius: 12px; overflow: hidden; border: 1px solid rgba(0, 0, 0, 0.06); background: #ffffff; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03); }

/* ---------- PRICE ALERT ---------- */
.stk-alert-toggle { margin-top: 10px; }
.stk-alert-toggle__btn {
  display: inline-flex; align-items: center; gap: 6px; padding: 7px 14px;
  border: 1px solid rgba(0, 0, 0, 0.08); border-radius: 8px; background: rgba(0, 0, 0, 0.02);
  color: #475569; font-size: 0.82rem; font-weight: 600; cursor: pointer; transition: all 0.15s;
}
.stk-alert-toggle__btn:hover { background: rgba(59,130,246,0.08); border-color: rgba(59,130,246,0.2); color: #2563eb; }
.stk-alert-toggle__btn svg:last-child { transition: transform 0.2s ease; }
.stk-chevron--open { transform: rotate(180deg); }
.stk-alert-content { margin-top: 8px; }

/* ---------- FILTERS ---------- */
.stk-input {
  width: 100%; padding: 9px 14px; border: 1px solid rgba(0, 0, 0, 0.1); border-radius: 8px;
  font-size: 0.85rem; color: #0f172a; background: #ffffff; transition: border-color 0.2s, box-shadow 0.2s; outline: none;
}
.stk-input:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.15); }

/* ---------- TABLE ---------- */
.stk-table-wrap { border-radius: 10px; border: 1px solid rgba(0, 0, 0, 0.06); overflow: hidden; background: #ffffff; }
.stk-table-wrap--scroll { max-height: 480px; overflow-y: auto; }
.stk-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
.stk-th {
  padding: 10px 14px; text-align: left; font-size: 0.72rem; font-weight: 700;
  text-transform: uppercase; letter-spacing: 0.5px; color: #475569; background: #f1f5f9;
  border-bottom: 2px solid #e2e8f0; position: sticky; top: 0; z-index: 2;
}
.stk-th--center { text-align: center; }
.stk-th--right { text-align: right; }

.stk-row { cursor: pointer; transition: background 0.15s ease; }
.stk-row:hover { background: #f8fafc; }
.stk-row--active { background: rgba(59, 130, 246, 0.05) !important; }
.stk-td { padding: 10px 14px; border-bottom: 1px solid rgba(0, 0, 0, 0.04); vertical-align: middle; color: #334155; }
.stk-td--center { text-align: center; }
.stk-td--right { text-align: right; }
.stk-td--symbol { font-weight: 700; color: #2563eb; }

/* ---------- SIGNALS & BADGES ---------- */
.stk-signal { display: inline-block; padding: 3px 10px; border-radius: 999px; font-size: 0.75rem; font-weight: 600; white-space: nowrap; }
.stk-signal--buy { background: rgba(16, 185, 129, 0.08); color: #059669; border: 1px solid rgba(16, 185, 129, 0.2); }
.stk-signal--sell { background: rgba(239, 68, 68, 0.08); color: #dc2626; border: 1px solid rgba(239, 68, 68, 0.2); }
.stk-signal--low { background: rgba(16, 185, 129, 0.08); color: #059669; border: 1px solid rgba(16, 185, 129, 0.2); }
.stk-signal--medium { background: rgba(245, 158, 11, 0.08); color: #d97706; border: 1px solid rgba(245, 158, 11, 0.2); }
.stk-signal--high { background: rgba(239, 68, 68, 0.08); color: #dc2626; border: 1px solid rgba(239, 68, 68, 0.2); box-shadow: 0 0 8px rgba(239, 68, 68, 0.1); }

/* ---------- BUTTONS ---------- */
.stk-actions { padding: 16px 0 0; display: flex; align-items: center; justify-content: center; flex-wrap: wrap; gap: 10px; }
.stk-btn {
  display: inline-flex; align-items: center; gap: 6px; padding: 9px 18px;
  border: none; border-radius: 8px; font-size: 0.84rem; font-weight: 600;
  cursor: pointer; transition: all 0.2s ease; white-space: nowrap;
}
.stk-btn:disabled { opacity: 0.45; cursor: not-allowed; }
.stk-btn--primary { background: linear-gradient(135deg, #3b82f6, #2563eb); color: #fff; box-shadow: 0 2px 8px rgba(59,130,246,0.3); }
.stk-btn--primary:hover:not(:disabled) { background: linear-gradient(135deg, #2563eb, #1d4ed8); box-shadow: 0 4px 14px rgba(37,99,235,0.4); transform: translateY(-1px); }
.stk-btn--outline { background: rgba(255,255,255,0.03); color: #475569; border: 1px solid rgba(0, 0, 0, 0.08); }
.stk-btn--outline:hover:not(:disabled) { background: rgba(0, 0, 0, 0.02); }
.stk-btn--scan { min-width: 180px; justify-content: center; }

/* ---------- LOADING ---------- */
.stk-loading { display: flex; justify-content: center; padding: 20px 0; }
.stk-spinner { width: 32px; height: 32px; border: 3px solid rgba(0, 0, 0, 0.06); border-top-color: #3b82f6; border-radius: 50%; animation: stk-spin 0.7s linear infinite; }
@keyframes stk-spin { to { transform: rotate(360deg); } }

.stk-message { text-align: center; font-size: 0.85rem; color: #64748b; padding: 10px 0; margin: 0; }

/* ---------- RRG ---------- */
.stk-rrg-wrap { padding: 24px; text-align: center; }
.stk-rrg-img { max-width: 100%; height: auto; border-radius: 12px; box-shadow: 0 8px 32px rgba(0,0,0,0.08); border: 1px solid rgba(0, 0, 0, 0.06); }

/* ---------- RESPONSIVE ---------- */
@media (max-width: 640px) {
  .stk-container { padding: 0 10px; }
  .stk-header { padding: 16px; }
  .stk-section { padding: 16px; }
  .stk-sticky-chart { padding: 0 12px 10px; }
  .stk-tab { padding: 8px 14px; font-size: 0.82rem; }
  .stk-table-wrap--scroll { max-height: 400px; }
}

/* ---------- CHEATSHEET & CHECKLIST STYLES ---------- */
.stk-checklist-panel {
  border-left: 4px solid #3b82f6 !important;
}
.stk-checklist-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  cursor: pointer;
  background: #f8fafc;
  user-select: none;
}
.stk-checklist-header:hover {
  background: #f1f5f9;
}
.stk-checklist-icon {
  font-size: 1.1rem;
}
.stk-checklist-title {
  font-size: 0.9rem;
  font-weight: 700;
  margin: 0;
  color: #0f172a;
}
.stk-checklist-body {
  padding: 16px 20px;
  border-top: 1px solid rgba(0, 0, 0, 0.06);
}
.stk-checklist-items {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.stk-checklist-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 8px 12px;
  border-radius: 8px;
  cursor: pointer;
  background: #f8fafc;
  transition: all 0.15s;
}
.stk-checklist-item:hover {
  background: #f1f5f9;
}
.stk-checklist-item--checked {
  background: rgba(16, 185, 129, 0.04) !important;
  border: 1px solid rgba(16, 185, 129, 0.2);
}
.stk-checklist-item--checked .stk-checklist-text {
  color: #059669;
  text-decoration: line-through;
  opacity: 0.8;
}
.stk-checklist-text {
  font-size: 0.84rem;
  color: #334155;
  line-height: 1.4;
  text-align: left;
}
.stk-checklist-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 14px;
  padding-top: 12px;
  border-top: 1px dashed rgba(0,0,0,0.08);
}
.stk-checklist-tip {
  font-size: 0.78rem;
  color: #64748b;
  font-style: italic;
}

.stk-cheatsheet-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 20px;
}
@media (max-width: 1024px) {
  .stk-cheatsheet-grid {
    grid-template-columns: 1fr;
  }
}

.stk-row-static {
  background: #ffffff;
}

.stk-badge {
  display: inline-block;
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
}
.stk-badge--green { background: rgba(16, 185, 129, 0.1); color: #047857; }
.stk-badge--yellow { background: rgba(245, 158, 11, 0.1); color: #b45309; }
.stk-badge--orange { background: rgba(249, 115, 22, 0.1); color: #c2410c; }
.stk-badge--red { background: rgba(239, 68, 68, 0.1); color: #b91c1c; }
.stk-badge--toxic { background: #fee2e2; color: #991b1b; animation: toxic-pulse 2s infinite; }

@keyframes toxic-pulse {
  0% { transform: scale(1); }
  50% { transform: scale(1.02); box-shadow: 0 0 8px rgba(153, 27, 27, 0.2); }
  100% { transform: scale(1); }
}

.stk-profiles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 16px;
}

.stk-profile-setup-box {
  background: #f0f7ff;
  border: 1px solid #bfdbfe;
  border-radius: 8px;
  padding: 10px 12px;
  margin-top: 10px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px 12px;
}
.stk-profile-setup-box__full {
  grid-column: span 2;
}
@media (max-width: 480px) {
  .stk-profile-setup-box {
    grid-template-columns: 1fr;
  }
  .stk-profile-setup-box__full {
    grid-column: span 1;
  }
}

@media (max-width: 640px) {
  .stk-profiles-grid {
    grid-template-columns: 1fr;
  }
}

.stk-profile-card {
  background: #ffffff;
  border: 1px solid rgba(0,0,0,0.06);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0,0,0,0.02);
  transition: transform 0.2s, box-shadow 0.2s;
  text-align: left;
}
.stk-profile-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0,0,0,0.05);
  border-color: rgba(59,130,246,0.15);
}
.stk-profile-card__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 14px;
  background: #f8fafc;
  border-bottom: 1px solid rgba(0,0,0,0.04);
}
.stk-profile-symbol {
  font-size: 1rem;
  font-weight: 800;
  color: #2563eb;
  font-family: 'Outfit', sans-serif;
}
.stk-profile-nickname {
  font-size: 0.78rem;
  font-weight: 700;
  color: #64748b;
  text-transform: uppercase;
  background: #e2e8f0;
  padding: 2px 8px;
  border-radius: 4px;
}
.stk-profile-card__content {
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.stk-profile-row {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.stk-profile-label {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  color: #64748b;
  letter-spacing: 0.3px;
}
.stk-profile-value {
  font-size: 0.8rem;
  color: #334155;
  line-height: 1.35;
}
</style>