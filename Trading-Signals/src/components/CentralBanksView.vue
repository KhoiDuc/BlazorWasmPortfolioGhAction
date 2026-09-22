<template>
  <div class="d-flex flex-column min-vh-100">
    <NavBar />
    <div class="container mt-4 flex-grow-1">
      <!-- Title -->
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2 pt-2">
        <h2 class="mb-0 fw-bold d-flex align-items-center gap-2 text-slate-800">
          <span>🏛️</span> Central Banks & Macro Rates
        </h2>
        <span class="badge bg-primary px-3 py-2 shadow-sm" style="background-color: #3b82f6 !important; font-size: 0.88rem; font-weight: 600;">
          Macro Intel Hub
        </span>
      </div>

      <!-- Introduction Panel -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card border-0 shadow-sm glass-panel border-glass" style="border-left: 4px solid #3b82f6 !important;">
            <div class="card-body py-3">
              <p class="mb-0" style="color: #475569; font-size: 0.95rem; line-height: 1.6;">
                Track monetary policy expectations and macro pivot points. "Global Bond Yields" provide real-time cost of capital across major economies, while "Rate Probabilities" show the market's expected target interest rates derived from futures trading prices.
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Cheat Sheet Panel -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card border-0 shadow-sm glass-panel border-glass cheat-sheet-card position-relative overflow-hidden">
            <!-- Header of Cheat Sheet -->
            <div 
              class="card-header border-0 d-flex justify-content-between align-items-center py-3 px-4" 
              style="background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); cursor: pointer; user-select: none;"
              @click="showCheatSheet = !showCheatSheet"
            >
              <div class="d-flex align-items-center gap-2">
                <span style="font-size: 1.2rem;">💡</span>
                <h5 class="mb-0 fw-bold text-slate-800" style="font-family: 'Outfit', sans-serif; font-size: 1.05rem;">
                  Bảng Giải Mã Nhanh: Ý Nghĩa Lợi Tức Trái Phiếu (Bond Yield Cheat Sheet)
                </h5>
              </div>
              <button class="btn btn-sm btn-link text-slate-500 p-0 d-flex align-items-center gap-1 text-decoration-none" style="font-size: 0.85rem; font-weight: 600;">
                <span>{{ showCheatSheet ? 'Thu gọn' : 'Mở rộng' }}</span>
                <span :style="{ transform: showCheatSheet ? 'rotate(180deg)' : 'rotate(0deg)', transition: 'transform 0.2s', display: 'inline-block' }">▼</span>
              </button>
            </div>

            <!-- Content -->
            <div v-show="showCheatSheet" class="card-body p-4 border-top" style="border-top: 1px solid rgba(0, 0, 0, 0.06) !important;">
              <div class="row g-4">
                <!-- 2-Year Bond Yield Column -->
                <div class="col-lg-6">
                  <div class="h-100 p-3 rounded-3 border" style="background-color: #f8fafc; border-color: rgba(0, 0, 0, 0.05) !important;">
                    <div class="d-flex align-items-center gap-2 mb-3">
                      <span class="badge badge-blue px-2.5 py-1 rounded-pill" style="font-weight: 600; font-size: 0.8rem;">
                        Ngắn hạn (2-Year)
                      </span>
                      <h6 class="mb-0 fw-bold text-slate-800" style="font-family: 'Outfit', sans-serif;">Lợi tức 2 năm (US2Y)</h6>
                    </div>
                    <p class="text-slate-600 mb-3" style="font-size: 0.88rem; line-height: 1.5;">
                      Phản ánh trực tiếp kỳ vọng chính sách tiền tệ ngắn hạn của Cục Dự trữ Liên bang (Fed). Cực kỳ nhạy cảm với các quyết định tăng/giảm lãi suất điều hành.
                    </p>

                    <!-- Rising -->
                    <div class="p-3 mb-2 rounded-2 yield-box-rising-green">
                      <div class="d-flex align-items-center gap-1.5 mb-1.5">
                        <span class="text-success fw-bold" style="font-size: 0.9rem;">📈 Lợi tức Tăng (Rising)</span>
                      </div>
                      <ul class="mb-0 ps-3 text-slate-700" style="font-size: 0.85rem; line-height: 1.55;">
                        <li><strong>Với Fed:</strong> Thị trường kỳ vọng Fed sẽ thắt chặt tiền tệ hơn, tăng lãi suất hoặc giữ lãi suất cao lâu hơn (Hawkish) để kiềm chế lạm phát.</li>
                        <li><strong>Với Nền kinh tế:</strong> Lãi suất cho vay ngắn hạn tăng, gây áp lực lên thanh khoản của các ngân hàng thương mại và tăng chi phí vay vốn ngắn hạn của doanh nghiệp.</li>
                      </ul>
                    </div>

                    <!-- Falling -->
                    <div class="p-3 rounded-2 yield-box-falling-blue">
                      <div class="d-flex align-items-center gap-1.5 mb-1.5">
                        <span class="text-primary fw-bold" style="font-size: 0.9rem;">📉 Lợi tức Giảm (Falling)</span>
                      </div>
                      <ul class="mb-0 ps-3 text-slate-700" style="font-size: 0.85rem; line-height: 1.55;">
                        <li><strong>Với Fed:</strong> Kỳ vọng Fed sắp giảm lãi suất hoặc nới lỏng tiền tệ (Dovish) để hỗ trợ nền kinh tế khi tăng trưởng chậm lại hoặc lạm phát hạ nhiệt.</li>
                        <li><strong>Với Nền kinh tế:</strong> Chi phí đi vay ngắn hạn giảm, giải tỏa áp lực thanh khoản cho hệ thống tài chính và kích thích dòng tiền lưu thông ngắn hạn.</li>
                      </ul>
                    </div>
                  </div>
                </div>

                <!-- 10-Year Bond Yield Column -->
                <div class="col-lg-6">
                  <div class="h-100 p-3 rounded-3 border" style="background-color: #f8fafc; border-color: rgba(0, 0, 0, 0.05) !important;">
                    <div class="d-flex align-items-center gap-2 mb-3">
                      <span class="badge badge-amber px-2.5 py-1 rounded-pill" style="font-weight: 600; font-size: 0.8rem;">
                        Dài hạn (10-Year)
                      </span>
                      <h6 class="mb-0 fw-bold text-slate-800" style="font-family: 'Outfit', sans-serif;">Lợi tức 10 năm (US10Y)</h6>
                    </div>
                    <p class="text-slate-600 mb-3" style="font-size: 0.88rem; line-height: 1.5;">
                      Phản ánh sức khỏe kinh tế, lạm phát và kỳ vọng tăng trưởng dài hạn. Đây là tham chiếu then chốt cho lãi suất thế chấp mua nhà, mua xe và trái phiếu doanh nghiệp.
                    </p>

                    <!-- Rising -->
                    <div class="p-3 mb-2 rounded-2 yield-box-rising-amber">
                      <div class="d-flex align-items-center gap-1.5 mb-1.5">
                        <span class="text-warning-dark fw-bold" style="font-size: 0.9rem;">📈 Lợi tức Tăng (Rising)</span>
                      </div>
                      <ul class="mb-0 ps-3 text-slate-700" style="font-size: 0.85rem; line-height: 1.55;">
                        <li><strong>Với Fed:</strong> Thường xảy ra khi kinh tế tăng trưởng mạnh hoặc kỳ vọng lạm phát dài hạn gia tăng. Fed có thể tự tin duy trì thắt chặt hoặc tăng lãi suất để kiềm chế lạm phát.</li>
                        <li><strong>Với Nền kinh tế:</strong> Làm tăng lãi suất vay dài hạn (ví dụ lãi suất vay mua nhà), hạ nhiệt thị trường bất động sản. Cho thấy dòng tiền đang rút khỏi trái phiếu (tài sản an toàn) sang tài sản rủi ro (cổ phiếu) hoặc lo ngại lạm phát dài hạn.</li>
                      </ul>
                    </div>

                    <!-- Falling -->
                    <div class="p-3 rounded-2 yield-box-falling-red">
                      <div class="d-flex align-items-center gap-1.5 mb-1.5">
                        <span class="text-danger fw-bold" style="font-size: 0.9rem;">📉 Lợi tức Giảm (Falling)</span>
                      </div>
                      <ul class="mb-0 ps-3 text-slate-700" style="font-size: 0.85rem; line-height: 1.55;">
                        <li><strong>Với Fed:</strong> Gây sức ép buộc Fed phải hạ lãi suất điều hành để kích thích nền kinh tế khi xuất hiện rủi ro giảm phát hoặc suy giảm tăng trưởng.</li>
                        <li><strong>Với Nền kinh tế:</strong> Lãi suất cho vay dài hạn giảm. Tuy nhiên, nếu giảm sâu, nó báo hiệu dòng tiền đang ồ ạt tìm nơi trú ẩn an toàn (mua mạnh trái phiếu) do thị trường lo ngại suy thoái kinh tế.</li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Divider -->
              <hr class="my-4" style="border-top: 1px solid rgba(0, 0, 0, 0.08);" />

              <!-- Bear Steepening vs Asset Rotation Comparison Table -->
              <div class="market-reaction-section mb-4">
                <h6 class="mb-3 fw-bold d-flex align-items-center gap-2 text-slate-800" style="font-family: 'Outfit', sans-serif;">
                  <span>⚖️</span> Cách phân biệt hai thời điểm qua phản ứng của thị trường
                </h6>
                <div class="table-responsive rounded-3 border overflow-hidden" style="border-color: rgba(0,0,0,0.06) !important;">
                  <table class="table table-hover align-middle mb-0" style="font-size: 0.88rem; background: #fff;">
                    <thead style="background-color: #f8fafc; border-bottom: 2px solid rgba(0, 0, 0, 0.06);">
                      <tr>
                        <th scope="col" style="width: 20%; font-weight: 700; color: #475569; padding: 12px 16px; border-bottom: 0;">Tiêu chí</th>
                        <th scope="col" style="width: 40%; font-weight: 700; color: #dc2626; padding: 12px 16px; border-bottom: 0;">
                          🔴 Thời điểm HIỆN TẠI (Bear Steepening) <br/>
                          <span class="fw-normal text-muted small" style="font-size: 0.78rem;">(Nhà đầu tư đang BÁN trái phiếu)</span>
                        </th>
                        <th scope="col" style="width: 40%; font-weight: 700; color: #16a34a; padding: 12px 16px; border-bottom: 0;">
                          🟢 Thời điểm BẠN NÓI (Asset Rotation) <br/>
                          <span class="fw-normal text-muted small" style="font-size: 0.78rem;">(Nhà đầu tư đang MUA trái phiếu vì lãi cao)</span>
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr>
                        <td class="fw-bold text-slate-700" style="padding: 12px 16px; background-color: #f8fafc;">Hành động của US10Y & US30Y</td>
                        <td style="padding: 12px 16px; color: #334155;">
                          <strong>Tăng liên tục không phanh</strong>, liên tục phá vỡ các vùng đỉnh cũ.
                        </td>
                        <td style="padding: 12px 16px; color: #334155;">
                          <strong>Chững đà tăng và bắt đầu tạo đỉnh</strong>, đi ngang hoặc giảm nhẹ khi dòng tiền lớn nhập cuộc bắt đáy.
                        </td>
                      </tr>
                      <tr>
                        <td class="fw-bold text-slate-700" style="padding: 12px 16px; background-color: #f8fafc;">Khối lượng giao dịch Trái phiếu</td>
                        <td style="padding: 12px 16px; color: #334155;">
                          <strong>Giá giảm kèm Volume Bán tăng vọt</strong> (Bán tháo để chạy trốn rủi ro lạm phát/nợ công).
                        </td>
                        <td style="padding: 12px 16px; color: #334155;">
                          <strong>Giá trái phiếu dừng giảm, Volume MUA tăng đột biến</strong> (Dòng tiền lớn chấp nhận mức lợi suất này để khóa lợi nhuận).
                        </td>
                      </tr>
                      <tr>
                        <td class="fw-bold text-slate-700" style="padding: 12px 16px; background-color: #f8fafc;">Diễn biến của Vàng (Gold)</td>
                        <td style="padding: 12px 16px; color: #334155;">
                          <strong>Vàng tăng mạnh</strong>. Người ta không tin chính phủ nên bỏ trái phiếu, chọn Vàng làm hầm trú ẩn tối thượng.
                        </td>
                        <td style="padding: 12px 16px; color: #334155;">
                          <strong>Vàng giảm mạnh</strong>. Trái phiếu Mỹ có lãi suất hấp dẫn hơn hẳn một tài sản không sinh lãi như Vàng.
                        </td>
                      </tr>
                      <tr>
                        <td class="fw-bold text-slate-700" style="padding: 12px 16px; background-color: #f8fafc;">Bản chất tâm lý thị trường</td>
                        <td style="padding: 12px 16px; color: #334155;">
                          <strong>Panic Selling (Bán tháo hoảng loạn)</strong> tài sản dài hạn vì sợ tương lai bất định.
                        </td>
                        <td style="padding: 12px 16px; color: #334155;">
                          <strong>Opportunistic Buying (Mua săn lùng cơ hội)</strong> vì lợi suất cố định đã quá "hời".
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <!-- Yield Curve Inversion Info -->
              <div class="mt-4 p-3 rounded-3 yield-inversion-box d-flex align-items-start gap-3">
                <span style="font-size: 1.4rem; line-height: 1.2;">⚠️</span>
                <div>
                  <h6 class="mb-1 fw-bold" style="font-family: 'Outfit', sans-serif; color: #c2410c !important;">Đường cong lãi suất đảo ngược (Yield Curve Inversion: US2Y > US10Y)</h6>
                  <p class="mb-0 text-slate-700" style="font-size: 0.85rem; line-height: 1.5;">
                    Hiện tượng xảy ra khi lợi suất ngắn hạn cao hơn lợi suất dài hạn. Đây là <strong>chỉ báo sớm đáng tin cậy nhất trong lịch sử</strong> báo trước một cuộc suy thoái kinh tế (Recession) thường diễn ra trong vòng 12-18 tháng tiếp theo, thể hiện sự bi quan sâu sắc của giới đầu tư về tăng trưởng dài hạn.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Top-Level Tabs -->
      <ul class="nav nav-pills mb-4 gap-2" role="tablist">
        <li class="nav-item">
          <button 
            class="nav-link rounded-pill px-4 fw-semibold" 
            :class="{ active: mainTab === 'bonds', 'bg-primary text-white shadow-sm': mainTab === 'bonds', 'bg-light text-secondary': mainTab !== 'bonds' }" 
            @click="mainTab = 'bonds'"
          >
            📊 Global Bond Yields
          </button>
        </li>
        <li class="nav-item">
          <button 
            class="nav-link rounded-pill px-4 fw-semibold" 
            :class="{ active: mainTab === 'rates', 'bg-primary text-white shadow-sm': mainTab === 'rates', 'bg-light text-secondary': mainTab !== 'rates' }" 
            @click="mainTab = 'rates'"
          >
            🏦 Rate Probabilities
          </button>
        </li>
      </ul>

      <!-- Bonds Tab Content -->
      <div v-if="mainTab === 'bonds'">
        <ul class="nav nav-tabs mb-4" role="tablist">
          <li class="nav-item" v-for="country in bondCountries" :key="country.id">
            <button 
              class="nav-link" 
              :class="{ active: activeBondTab === country.id }" 
              @click="activeBondTab = country.id"
            >
              {{ country.flag }} {{ country.name }}
            </button>
          </li>
        </ul>
        
        <div class="row g-4">
          <!-- 2-Year Bond Yield -->
          <div class="col-lg-6">
            <div class="card border-0 shadow-sm border-glass rounded-3 overflow-hidden glass-panel">
              <div class="card-header bg-warning-dark py-3 px-4 border-0">
                <h5 class="mb-0 fw-bold text-slate-800" style="font-family: 'Outfit', sans-serif;">2-Year Bond Yield</h5>
              </div>
              <div class="card-body p-0 position-relative">
                 <TradingViewChart v-if="currentBond2Y" :coin="currentBond2Y" :height="400" />
                 <div class="px-4 py-3 bg-light border-top d-flex justify-content-between align-items-center flex-wrap gap-2">
                   <div class="d-flex flex-column">
                     <span class="fw-semibold text-slate-700" style="font-size: 0.85rem;">Mã: {{ currentBond2Y }}</span>
                     <span v-if="activeBondTab !== 'us'" class="text-muted" style="font-size: 0.75rem;">
                       ⚠️ Bị giới hạn bản quyền nhúng widget của TradingView.
                     </span>
                     <span v-else class="text-success text-muted" style="font-size: 0.75rem;">
                       ✓ Đang hiển thị dữ liệu FRED Daily.
                     </span>
                   </div>
                   <a 
                     :href="`https://www.tradingview.com/chart/?symbol=${currentBond2Y}`" 
                     target="_blank" 
                     class="btn btn-sm btn-primary px-3 rounded-pill d-flex align-items-center gap-1 shadow-sm"
                     style="font-size: 0.8rem; font-weight: 600; background-color: #3b82f6 !important; border-color: #3b82f6 !important;"
                   >
                     <span>Xem trên TradingView ↗</span>
                   </a>
                 </div>
              </div>
            </div>
          </div>
          <!-- 5-Year Bond Yield -->
          <div class="col-lg-6">
            <div class="card border-0 shadow-sm border-glass rounded-3 overflow-hidden glass-panel">
              <div class="card-header bg-warning-dark py-3 px-4 border-0">
                <h5 class="mb-0 fw-bold text-slate-800" style="font-family: 'Outfit', sans-serif;">5-Year Bond Yield</h5>
              </div>
              <div class="card-body p-0 position-relative">
                 <TradingViewChart v-if="currentBond5Y" :coin="currentBond5Y" :height="400" />
                 <div class="px-4 py-3 bg-light border-top d-flex justify-content-between align-items-center flex-wrap gap-2">
                   <div class="d-flex flex-column">
                     <span class="fw-semibold text-slate-700" style="font-size: 0.85rem;">Mã: {{ currentBond5Y }}</span>
                     <span v-if="activeBondTab !== 'us'" class="text-muted" style="font-size: 0.75rem;">
                       ⚠️ Bị giới hạn bản quyền nhúng widget của TradingView.
                     </span>
                     <span v-else class="text-success text-muted" style="font-size: 0.75rem;">
                       ✓ Đang hiển thị dữ liệu FRED Daily.
                     </span>
                   </div>
                   <a 
                     :href="`https://www.tradingview.com/chart/?symbol=${currentBond5Y}`" 
                     target="_blank" 
                     class="btn btn-sm btn-primary px-3 rounded-pill d-flex align-items-center gap-1 shadow-sm"
                     style="font-size: 0.8rem; font-weight: 600; background-color: #3b82f6 !important; border-color: #3b82f6 !important;"
                   >
                     <span>Xem trên TradingView ↗</span>
                   </a>
                 </div>
              </div>
            </div>
          </div>
          <!-- 10-Year Bond Yield -->
          <div class="col-lg-6">
            <div class="card border-0 shadow-sm border-glass rounded-3 overflow-hidden glass-panel">
              <div class="card-header bg-warning-dark py-3 px-4 border-0">
                <h5 class="mb-0 fw-bold text-slate-800" style="font-family: 'Outfit', sans-serif;">10-Year Bond Yield</h5>
              </div>
              <div class="card-body p-0 position-relative">
                 <TradingViewChart v-if="currentBond10Y" :coin="currentBond10Y" :height="400" />
                 <div class="px-4 py-3 bg-light border-top d-flex justify-content-between align-items-center flex-wrap gap-2">
                   <div class="d-flex flex-column">
                     <span class="fw-semibold text-slate-700" style="font-size: 0.85rem;">Mã: {{ currentBond10Y }}</span>
                     <span v-if="activeBondTab !== 'us'" class="text-muted" style="font-size: 0.75rem;">
                       ⚠️ Bị giới hạn bản quyền nhúng widget của TradingView.
                     </span>
                     <span v-else class="text-success text-muted" style="font-size: 0.75rem;">
                       ✓ Đang hiển thị dữ liệu FRED Daily.
                     </span>
                   </div>
                   <a 
                     :href="`https://www.tradingview.com/chart/?symbol=${currentBond10Y}`" 
                     target="_blank" 
                     class="btn btn-sm btn-primary px-3 rounded-pill d-flex align-items-center gap-1 shadow-sm"
                     style="font-size: 0.8rem; font-weight: 600; background-color: #3b82f6 !important; border-color: #3b82f6 !important;"
                   >
                     <span>Xem trên TradingView ↗</span>
                   </a>
                 </div>
              </div>
            </div>
          </div>
          <!-- 30-Year Bond Yield -->
          <div class="col-lg-6">
            <div class="card border-0 shadow-sm border-glass rounded-3 overflow-hidden glass-panel">
              <div class="card-header bg-warning-dark py-3 px-4 border-0">
                <h5 class="mb-0 fw-bold text-slate-800" style="font-family: 'Outfit', sans-serif;">30-Year Bond Yield</h5>
              </div>
              <div class="card-body p-0 position-relative">
                 <TradingViewChart v-if="currentBond30Y" :coin="currentBond30Y" :height="400" />
                 <div class="px-4 py-3 bg-light border-top d-flex justify-content-between align-items-center flex-wrap gap-2">
                   <div class="d-flex flex-column">
                     <span class="fw-semibold text-slate-700" style="font-size: 0.85rem;">Mã: {{ currentBond30Y }}</span>
                     <span v-if="activeBondTab !== 'us'" class="text-muted" style="font-size: 0.75rem;">
                       ⚠️ Bị giới hạn bản quyền nhúng widget của TradingView.
                     </span>
                     <span v-else class="text-success text-muted" style="font-size: 0.75rem;">
                       ✓ Đang hiển thị dữ liệu FRED Daily.
                     </span>
                   </div>
                   <a 
                     :href="`https://www.tradingview.com/chart/?symbol=${currentBond30Y}`" 
                     target="_blank" 
                     class="btn btn-sm btn-primary px-3 rounded-pill d-flex align-items-center gap-1 shadow-sm"
                     style="font-size: 0.8rem; font-weight: 600; background-color: #3b82f6 !important; border-color: #3b82f6 !important;"
                   >
                     <span>Xem trên TradingView ↗</span>
                   </a>
                 </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Rates Probabilities Tab Content -->
      <div v-if="mainTab === 'rates'">
        <!-- Tab Buttons -->
        <ul class="nav nav-tabs mb-4" id="centralBankTabs" role="tablist">
          <li class="nav-item" v-for="tab in tabs" :key="tab.id">
            <button 
              class="nav-link" 
              :class="{ active: activeTab === tab.id }" 
              @click="activeTab = tab.id"
              type="button"
            >
              {{ tab.name }}
            </button>
          </li>
        </ul>

        <!-- Tab Content -->
        <div class="tab-content">
          <div v-for="tab in tabs" :key="tab.id" v-show="activeTab === tab.id" class="tab-pane fade show active">
            <div class="card border-0 shadow-sm border-glass rounded-3 overflow-hidden glass-panel">
              <!-- Header -->
              <div class="card-header bg-warning-dark d-flex justify-content-between align-items-center py-3 px-4 border-0">
                <h5 class="mb-0 fw-bold text-slate-800 d-flex align-items-center gap-2" style="font-family: 'Outfit', sans-serif;">
                  <span>{{ tab.flag }}</span> {{ tab.title }}
                </h5>
                <a :href="tab.url" target="_blank" class="btn btn-sm btn-outline-primary px-3" style="font-weight: 600; border-color: rgba(59, 130, 246, 0.2);">
                  Open Official Site ↗
                </a>
              </div>
              
              <!-- Body -->
              <div class="card-body p-0">
                <!-- Embed Chart as requested exactly by user -->
                <iframe 
                  :src="tab.embedUrl" 
                  width="100%" 
                  height="450" 
                  frameborder="0" 
                  style="border:none;" 
                  loading="lazy"
                ></iframe>
              </div>
            </div>
          </div>
        </div>
      </div>

    </div>
    <AppFooter class="mt-5" />
  </div>
</template>

<script>
import NavBar from './NavBar.vue';
import AppFooter from './AppFooter.vue';
import TradingViewChart from './TradingViewChart.vue';
import { ref, computed } from 'vue';

export default {
  name: 'CentralBanksView',
  components: {
    NavBar,
    AppFooter,
    TradingViewChart
  },
  setup() {
    const mainTab = ref('bonds');
    const showCheatSheet = ref(true);
    
    // Rate Probabilities states
    const activeTab = ref('fed');
    const tabs = ref([
      {
        id: 'fed',
        name: 'FED',
        flag: '🇺🇸',
        title: 'Federal Reserve Rate Probability Chart (US)',
        url: 'https://centralbank.watch/charts/federal-reserve-rate-probability-chart/',
        embedUrl: 'https://centralbank.watch/charts/federal-reserve-rate-probability-chart/?embed=true'
      },
      {
        id: 'boe',
        name: 'BOE',
        flag: '🇬🇧',
        title: 'Bank of England Rate Probability Chart (UK)',
        url: 'https://centralbank.watch/charts/bank-of-england-rate-probability-chart/',
        embedUrl: 'https://centralbank.watch/charts/bank-of-england-rate-probability-chart/?embed=true'
      },
      {
        id: 'ecb',
        name: 'ECB',
        flag: '🇪🇺',
        title: 'European Central Bank Rate Probability Chart (EU)',
        url: 'https://centralbank.watch/charts/ecb-rate-probability-chart/',
        embedUrl: 'https://centralbank.watch/charts/ecb-rate-probability-chart/?embed=true'
      },
      {
        id: 'rba',
        name: 'RBA',
        flag: '🇦🇺',
        title: 'Reserve Bank of Australia Rate Probability Chart (Australia)',
        url: 'https://centralbank.watch/charts/reserve-bank-of-australia-rate-probability-chart/',
        embedUrl: 'https://centralbank.watch/charts/reserve-bank-of-australia-rate-probability-chart/?embed=true'
      },
      {
        id: 'boc',
        name: 'BOC',
        flag: '🇨🇦',
        title: 'Bank of Canada Rate Probability Chart (Canada)',
        url: 'https://centralbank.watch/charts/bank-of-canada-rate-probability-chart/',
        embedUrl: 'https://centralbank.watch/charts/bank-of-canada-rate-probability-chart/?embed=true'
      }
    ]);

    // Bond Yields states
    const activeBondTab = ref('us');
    const bondCountries = ref([
      { id: 'us', name: 'US', flag: '🇺🇸', sym2y: 'FRED:DGS2', sym5y: 'FRED:DGS5', sym10y: 'FRED:DGS10', sym30y: 'FRED:DGS30' },
      { id: 'uk', name: 'UK', flag: '🇬🇧', sym2y: 'TVC:GB02Y', sym5y: 'TVC:GB05Y', sym10y: 'TVC:GB10Y', sym30y: 'TVC:GB30Y' },
      { id: 'eu', name: 'Europe', flag: '🇪🇺', sym2y: 'TVC:DE02Y', sym5y: 'TVC:DE05Y', sym10y: 'TVC:DE10Y', sym30y: 'TVC:DE30Y' },
      { id: 'jp', name: 'Japan', flag: '🇯🇵', sym2y: 'TVC:JP02Y', sym5y: 'TVC:JP05Y', sym10y: 'TVC:JP10Y', sym30y: 'TVC:JP30Y' },
      { id: 'au', name: 'Australia', flag: '🇦🇺', sym2y: 'TVC:AU02Y', sym5y: 'TVC:AU05Y', sym10y: 'TVC:AU10Y', sym30y: 'TVC:AU30Y' },
      { id: 'ca', name: 'Canada', flag: '🇨🇦', sym2y: 'TVC:CA02Y', sym5y: 'TVC:CA05Y', sym10y: 'TVC:CA10Y', sym30y: 'TVC:CA30Y' },
      { id: 'kr', name: 'South Korea', flag: '🇰🇷', sym2y: 'TVC:KR02Y', sym5y: 'TVC:KR05Y', sym10y: 'TVC:KR10Y', sym30y: 'TVC:KR30Y' },
      { id: 'cn', name: 'China', flag: '🇨🇳', sym2y: 'TVC:CN02Y', sym5y: 'TVC:CN05Y', sym10y: 'TVC:CN10Y', sym30y: 'TVC:CN30Y' },
      { id: 'vn', name: 'Vietnam', flag: '🇻🇳', sym2y: 'VN02Y', sym5y: 'VN05Y', sym10y: 'VN10Y', sym30y: 'VN30Y' } // Best effort symbols for VN
    ]);

    const currentBond2Y = computed(() => {
      const country = bondCountries.value.find(c => c.id === activeBondTab.value);
      return country ? country.sym2y : '';
    });

    const currentBond5Y = computed(() => {
      const country = bondCountries.value.find(c => c.id === activeBondTab.value);
      return country ? country.sym5y : '';
    });

    const currentBond10Y = computed(() => {
      const country = bondCountries.value.find(c => c.id === activeBondTab.value);
      return country ? country.sym10y : '';
    });

    const currentBond30Y = computed(() => {
      const country = bondCountries.value.find(c => c.id === activeBondTab.value);
      return country ? country.sym30y : '';
    });

    return {
      mainTab,
      activeTab,
      tabs,
      activeBondTab,
      bondCountries,
      currentBond2Y,
      currentBond5Y,
      currentBond10Y,
      currentBond30Y,
      showCheatSheet
    };
  }
};
</script>

<style scoped>
.nav-tabs {
  border-bottom: 1px solid rgba(0, 0, 0, 0.08) !important;
}
.nav-tabs .nav-link {
  color: #64748b;
  border: none;
  background: none;
  font-weight: 600;
  padding: 12px 24px;
  transition: all 0.2s;
  border-radius: 0;
  border-bottom: 3px solid transparent;
}
.nav-tabs .nav-link:hover {
  color: #0f172a;
  border-bottom: 3px solid rgba(0, 0, 0, 0.06);
}
.nav-tabs .nav-link.active {
  color: #3b82f6 !important;
  background: none;
  border: none;
  border-bottom: 3px solid #3b82f6 !important;
}

.nav-pills .nav-link {
  transition: all 0.3s;
}

.glass-panel {
  background: #ffffff !important;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.04) !important;
}
.border-glass {
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
}
.bg-warning-dark {
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%) !important;
  border-bottom: 1px solid rgba(0, 0, 0, 0.06) !important;
}

/* Cheat Sheet styles */
.text-slate-800 {
  color: #1e293b !important;
}
.text-slate-700 {
  color: #334155 !important;
}
.text-slate-600 {
  color: #475569 !important;
}
.text-slate-500 {
  color: #64748b !important;
}
.text-warning-dark {
  color: #b45309 !important;
}

.cheat-sheet-card {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.cheat-sheet-card:hover {
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.06) !important;
}

.badge-blue {
  background-color: #e0f2fe !important;
  color: #0369a1 !important;
  border: 1px solid rgba(3, 105, 161, 0.15) !important;
}

.badge-amber {
  background-color: #fef3c7 !important;
  color: #b45309 !important;
  border: 1px solid rgba(180, 83, 9, 0.15) !important;
}

.yield-box-rising-green {
  background-color: #f0fdf4 !important;
  border-left: 3px solid #10b981 !important;
  border-top: 1px solid rgba(16, 185, 129, 0.08) !important;
  border-right: 1px solid rgba(16, 185, 129, 0.08) !important;
  border-bottom: 1px solid rgba(16, 185, 129, 0.08) !important;
}

.yield-box-falling-blue {
  background-color: #eff6ff !important;
  border-left: 3px solid #3b82f6 !important;
  border-top: 1px solid rgba(59, 130, 246, 0.08) !important;
  border-right: 1px solid rgba(59, 130, 246, 0.08) !important;
  border-bottom: 1px solid rgba(59, 130, 246, 0.08) !important;
}

.yield-box-rising-amber {
  background-color: #fffbeb !important;
  border-left: 3px solid #f59e0b !important;
  border-top: 1px solid rgba(245, 158, 11, 0.08) !important;
  border-right: 1px solid rgba(245, 158, 11, 0.08) !important;
  border-bottom: 1px solid rgba(245, 158, 11, 0.08) !important;
}

.yield-box-falling-red {
  background-color: #fef2f2 !important;
  border-left: 3px solid #ef4444 !important;
  border-top: 1px solid rgba(239, 68, 68, 0.08) !important;
  border-right: 1px solid rgba(239, 68, 68, 0.08) !important;
  border-bottom: 1px solid rgba(239, 68, 68, 0.08) !important;
}

.yield-inversion-box {
  background-color: #fff7ed !important;
  border-left: 3px solid #ea580c !important;
  border-top: 1px solid rgba(234, 88, 12, 0.08) !important;
  border-right: 1px solid rgba(234, 88, 12, 0.08) !important;
  border-bottom: 1px solid rgba(234, 88, 12, 0.08) !important;
}
</style>
