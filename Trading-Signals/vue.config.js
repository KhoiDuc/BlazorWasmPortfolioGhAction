const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  productionSourceMap: false,
  parallel: false,
  lintOnSave: false,
  configureWebpack: {
    cache: false,
  },
  devServer: {
    historyApiFallback: true,
    proxy: {
      '/tcanalysis/v1/evaluation': {
        target: 'https://apipubaws.tcbs.com.vn',
        changeOrigin: true,
        pathRewrite: {
          '^/tcanalysis/v1/evaluation': '/tcanalysis/v1/evaluation'
        }
      },
      '/tcanalysis/v1/ticker': {
        target: 'https://apipubaws.tcbs.com.vn',
        changeOrigin: true
      },
      '/stock-insight/v2/stock/bars-long-term': {
        target: 'https://apipubaws.tcbs.com.vn',
        changeOrigin: true
      },
      '/api/chat': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/api/settings': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/api': {
        target: 'https://live-rates.com',
        changeOrigin: true,
        pathRewrite: {
          '^/api': '' // Remove /api prefix
        }
      },
      '/cg': {
        target: 'https://api.coingecko.com',
        changeOrigin: true,
        pathRewrite: {
          '^/cg': ''
        }
      },
      '/world': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true,
        pathRewrite: {
          '^/world': ''
        }
      },

      '/v4': {
        target: 'https://api-finfo.vndirect.com.vn',
        changeOrigin: true
      },
      '/dnse-user-service': {
        target: 'https://services.entrade.com.vn',
        changeOrigin: true
      },
      '/dnse-auth-service': {
        target: 'https://services.entrade.com.vn',
        changeOrigin: true
      },
      '/dnse-order-service': {
        target: 'https://services.entrade.com.vn',
        changeOrigin: true
      },
      '/getPotentialSymbols': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/runSSHScript': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/getPotentialCoins': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/getPotentialFuturesCoins': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/getPotentialForexPairs': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/getRealEstate': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/inputOTP': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/community': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/journal': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/api/news/telegram': {
        target: 'http://152.53.208.182:8080',
        changeOrigin: true
      },
      '/ff_calendar_thisweek.json': {
        target: 'https://nfs.faireconomy.media',
        changeOrigin: true,
        pathRewrite: {
          '^/ff_calendar_thisweek.json': '/ff_calendar_thisweek.json'
        }
      },
      '/goldprice': {
        target: 'https://sjc.com.vn',
        changeOrigin: true,
        pathRewrite: {
          '^/goldprice': '/GoldPrice'
        }
      },
      '/silverprice': {
        target: 'https://giabac.phuquygroup.vn',
        changeOrigin: true,
        pathRewrite: {
          '^/silverprice': '/PhuQuyPrice'
        }
      },
      '/petrolimex/search': {
        target: 'https://portals.petrolimex.com.vn',
        changeOrigin: true,
        pathRewrite: {
          '^/petrolimex/search': '/~apis/portals/cms.item/search?object-identity=search&x-request=eyJGaWx0ZXJCeSI6eyJBbmQiOlt7IlN5c3RlbUlEIjp7IkVxdWFscyI6IjY3ODNkYzEyNzFmZjQ0OWU5NWI3NGE5NTIwOTY0MTY5In19LHsiUmVwb3NpdG9yeUlEIjp7IkVxdWFscyI6ImE5NTQ1MWUyM2I0NzRmZTU4ODZiZmI3Y2Y4NDNmNTNjIn19LHsiUmVwb3NpdG9yeUVudGl0eUlEIjp7IkVxdWFscyI6IjM4MDEzNzhmZTFlMDQ1YjFhZmExMGRlN2M1Nzc2MTI0In19LHsiU3RhdHVzIjp7IkVxdWFscyI6IlB1Ymxpc2hlZCJ9fV19LCJTb3J0QnkiOnsiTGFzdE1vZGlmaWVkIjoiRGVzY2VuZGluZyJ9LCJQYWdpbmF0aW9uIjp7IlRvdGFsUmVjb3JkcyI6LTEsIlRvdGFsUGFnZXMiOjAsIlBhZ2VTaXplIjowLCJQYWdlTnVtYmVyIjowfX0='
        }
      },
      '/yahoo-finance': {
        target: 'https://query1.finance.yahoo.com',
        changeOrigin: true,
        pathRewrite: {
          '^/yahoo-finance': ''
        }
      },
      '/cryto_rrgchart': {
        target: 'https://thehaohcm.alwaysdata.net',
        changeOrigin: true,
        pathRewrite: {
          '^/cryto_rrgchart': '/crypto_rrgchart.png'
        }
      },
      '/futures_rrgchart': {
        target: 'https://thehaohcm.alwaysdata.net',
        changeOrigin: true,
        pathRewrite: {
          '^/futures_rrgchart': '/futures_rrgchart.png'
        }
      },
      '/vnstock_rrgchart': {
        target: 'https://thehaohcm.alwaysdata.net',
        changeOrigin: true,
        pathRewrite: {
          '^/vnstock_rrgchart': '/vnstock_rrgchart.png'
        }
      },
      '/assets_rrgchart': {
        target: 'https://thehaohcm.alwaysdata.net',
        changeOrigin: true,
        pathRewrite: {
          '^/assets_rrgchart': '/assets_rrgchart.png'
        }
      },
      '/api/osint': {
        target: 'https://trading-api-dark-sunset-2092.fly.dev',
        changeOrigin: true
      },
      '/forex_rrgchart': {
        target: 'https://thehaohcm.alwaysdata.net',
        changeOrigin: true,
        pathRewrite: {
          '^/forex_rrgchart': '/forex_rrgchart.png'
        }
      },
      '/': {
        target: 'https://services.entrade.com.vn',
        changeOrigin: true
      }
    }
  }
})