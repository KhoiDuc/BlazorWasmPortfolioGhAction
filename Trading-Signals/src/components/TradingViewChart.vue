<template>
    <div :id="containerId" ref="chartContainer" class="tradingview-chart-container" :style="{ minHeight: `${height}px`, height: `${height}px` }"></div>
  </template>
  
  <script setup>
  // eslint-disable-next-line no-undef
  const props = defineProps({
    coin: String,
    height: {
      type: [Number, String],
      default: 600
    }
  })

  import { ref, onMounted, watch } from 'vue'
  
  const chartContainer = ref(null)
  const containerId = `tradingview_chart_${Math.random().toString(36).substr(2, 9)}`
  
  const initChart = (coin) => {
    if (!window.TradingView) {
      console.error('⚠️ TradingView script chưa sẵn sàng!')
      return
    }

    // Clear previous chart
    if (chartContainer.value) {
      chartContainer.value.innerHTML = ''
    }

    // Coins not listed on Binance - use alternative exchanges
    const notOnBinance = {
      'XMRUSDT': 'KRAKEN:XMRUSD',
      'XMRBTC': 'KRAKEN:XMRBTC',
      'XMR': 'KRAKEN:XMRUSD',
      'ZCASHUSDT': 'KRAKEN:ZECUSD',
      'ZEC': 'KRAKEN:ZECUSD'
    }

    let symbol = coin
    
    // If coin has exchange prefix already, use as-is
    if (coin.includes(':')) {
      symbol = coin
    } 
    // Check if it's a crypto not on Binance
    else if (notOnBinance[coin.toUpperCase()]) {
      symbol = notOnBinance[coin.toUpperCase()]
    }
    // If it's a crypto pair ending with USDT, use Binance
    else if (coin && coin.toUpperCase().endsWith('USDT')) {
      symbol = `BINANCE:${coin}`
    }
    // Otherwise use raw symbol (stocks)
    else {
      symbol = coin
    }
  
    new window.TradingView.widget({
      container_id: containerId,
      width: '100%',
      height: props.height,
      symbol: symbol,
      interval: '1D',
      timezone: 'Asia/BangKok', // UTC+7
      theme: 'light', 
      style: '1',
      locale: 'en',
      toolbar_bg: '#f1f3f6',
      enable_publishing: false,
    })
  }
  
  onMounted(() => {
    // Nạp script TradingView nếu chưa có
    if (!window.TradingView) {
      const script = document.createElement('script')
      script.src = 'https://s3.tradingview.com/tv.js'
      script.onload = () => initChart(props.coin)
      document.body.appendChild(script)
    } else {
      initChart(props.coin)
    }
  })
  
  // Khi prop coin thay đổi thì tự load lại chart
  watch(() => props.coin, (newCoin) => {
    initChart(newCoin)
  })
  </script>
  
  <style scoped>
  .tradingview-chart-container {
    width: 100%;
  }
  </style>
  