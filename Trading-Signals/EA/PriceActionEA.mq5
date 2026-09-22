//+------------------------------------------------------------------+
//|              PriceActionOrderBlockEA_News_Calendar_Asian.mq5     |
//|                        Copyright 2025, Hao Nguyen                |
//+------------------------------------------------------------------+
#property copyright "Copyright 2025, Hao Nguyen"
#property link "https://home-software.com"
#property version "11.00"

//--- Include MQL5 Trade library
#include <Trade\Trade.mqh>

//--- Global arrays (replacing input arrays)
string Symbols[] = {"EURUSD", "USDJPY", "GBPUSD"}; // Symbols to trade
double ATR_Multiplier[] = {1.6, 1.8, 2.2};         // ATR multiplier per symbol
double PinBarTailRatio[] = {2.0, 2.5, 2.0};        // Pin Bar tail ratio per symbol
double FibonacciLevels[] = {0.382, 0.5, 0.618};    // Fibonacci retracement levels

//--- Input parameters
input double RiskPercent = 2.0;                          // Risk per trade (% of account balance)
input double RR_Ratio = 1.5;                             // Initial Reward-to-Risk Ratio
input double Extended_RR_Ratio = 2.0;                    // Extended R:R for strong trend
input int EMA_Period = 25;                               // EMA period for trend filter
input int RSI_Period = 14;                               // RSI period for overbought/oversold
input double RSI_Overbought = 70.0;                      // RSI overbought level
input double RSI_Oversold = 30.0;                        // RSI oversold level
input int ATR_Period = 14;                               // ATR period for dynamic SL
input int OrderBlockLookback = 20;                       // Lookback period for Order Block
input double BreakevenFactor = 0.5;                      // Move SL to breakeven when profit = SL * Factor
input double TrailingStopFactor = 1.0;                   // Trailing stop when profit = SL * Factor
input double PartialCloseRatio = 1.0;                    // Close 50% position at 1.0 * SL (1R)
input int MaxTradesPerSymbol = 2;                        // Max open trades per symbol
input int MaxTotalTrades = 4;                            // Max total open trades across all symbols
input ENUM_TIMEFRAMES Timeframe = PERIOD_M5;             // Trading timeframe
input ENUM_TIMEFRAMES HigherTimeframe = PERIOD_H1;       // Higher timeframe for trend
input bool TradeOnNews = true;                           // Trade after high-impact news
input int NewsTradeDelay = 60;                           // Seconds to wait after news
input int NewsAvoidBefore = 3600;                        // Seconds to avoid before news
input int NewsAvoidAfter = 3600;                         // Seconds to avoid after news
input bool UseFibonacciLimit = true;                     // Use Fibonacci Limit Orders after news
input int FibonacciExpiration = 7200;                    // Fibonacci Limit Order expiration (2 hours)
input double ATR_Filter_Ratio = 1.2;                     // ATR filter ratio for volatility
input double Volume_Filter_Ratio = 1.5;                  // Volume filter ratio for confirmation
input int SRLookback = 5;                                // Lookback period for support/resistance
input double SR_Proximity_Factor = 0.5;                  // Proximity factor for S/R (±ATR)

//--- Global variables
double LotSize;
double StopLoss;
int emaHandle[], rsiHandle[], atrHandle[], emaHandleHigher[], fractalHandle[], volumeHandle[];
string countryCodes[];

//+------------------------------------------------------------------+
//| Expert initialization function                                     |
//+------------------------------------------------------------------+
int OnInit()
{
   ArrayResize(emaHandle, ArraySize(Symbols));
   ArrayResize(rsiHandle, ArraySize(Symbols));
   ArrayResize(atrHandle, ArraySize(Symbols));
   ArrayResize(emaHandleHigher, ArraySize(Symbols));
   ArrayResize(fractalHandle, ArraySize(Symbols));
   ArrayResize(volumeHandle, ArraySize(Symbols));

   for (int i = 0; i < ArraySize(Symbols); i++)
   {
      string symbol = Symbols[i];
      emaHandle[i] = iMA(symbol, Timeframe, EMA_Period, 0, MODE_EMA, PRICE_CLOSE);
      rsiHandle[i] = iRSI(symbol, Timeframe, RSI_Period, PRICE_CLOSE);
      atrHandle[i] = iATR(symbol, Timeframe, ATR_Period);
      emaHandleHigher[i] = iMA(symbol, HigherTimeframe, EMA_Period, 0, MODE_EMA, PRICE_CLOSE);
      fractalHandle[i] = iFractals(symbol, Timeframe);
      volumeHandle[i] = iMA(symbol, Timeframe, 10, 0, MODE_EMA, VOLUME_TICK);

      if (emaHandle[i] == INVALID_HANDLE || rsiHandle[i] == INVALID_HANDLE ||
          atrHandle[i] == INVALID_HANDLE || emaHandleHigher[i] == INVALID_HANDLE ||
          fractalHandle[i] == INVALID_HANDLE || volumeHandle[i] == INVALID_HANDLE)
      {
         Print("Failed to create indicator handles for ", symbol);
         return (INIT_FAILED);
      }
   }

   // Initialize country codes for news filtering
   MqlCalendarCountry countries[];
   int count = CalendarCountries(countries);
   if (count <= 0)
   {
      PrintFormat("CalendarCountries failed! Error %d", GetLastError());
      return (INIT_FAILED);
   }

   // Store codes for US, JP, EU, GB
   string targetCountries[] = {"US", "JP", "EU", "GB"};
   ArrayResize(countryCodes, ArraySize(targetCountries));
   int found = 0;
   for (int i = 0; i < count && found < ArraySize(targetCountries); i++)
   {
      for (int j = 0; j < ArraySize(targetCountries); j++)
      {
         if (countries[i].code == targetCountries[j])
         {
            countryCodes[j] = countries[i].code;
            found++;
            break;
         }
      }
   }
   if (found < ArraySize(targetCountries))
   {
      Print("Failed to find all required country codes for news filtering");
      return (INIT_FAILED);
   }

   // Note: Ensure "https://faireconomy.media" and "https://www.mql5.com" are added to allowed URLs in
   // MetaTrader 5: Tools -> Options -> Expert Advisors -> Allow WebRequest for listed URL
   return (INIT_SUCCEEDED);
}

//+------------------------------------------------------------------+
//| Expert deinitialization function                                   |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
{
   for (int i = 0; i < ArraySize(Symbols); i++)
   {
      IndicatorRelease(emaHandle[i]);
      IndicatorRelease(rsiHandle[i]);
      IndicatorRelease(atrHandle[i]);
      IndicatorRelease(emaHandleHigher[i]);
      IndicatorRelease(fractalHandle[i]);
      IndicatorRelease(volumeHandle[i]);
   }
}

//+------------------------------------------------------------------+
//| Calculate GMT offset based on local machine time                  |
//+------------------------------------------------------------------+
int GetGMTOffset()
{
   datetime localTime = TimeLocal();
   datetime gmtTime = TimeGMT();
   return (int)((localTime - gmtTime) / 3600); // Offset in hours
}

//+------------------------------------------------------------------+
//| Check if current time is in Asian session (00:00-09:00 GMT)       |
//+------------------------------------------------------------------+
bool IsAsianSession()
{
   datetime currentTime = TimeCurrent();
   MqlDateTime timeStruct;
   TimeToStruct(currentTime, timeStruct);

   int gmtOffset = GetGMTOffset();
   int gmtHour = (timeStruct.hour - gmtOffset + 24) % 24;

   return (gmtHour >= 0 && gmtHour < 9);
}

//+------------------------------------------------------------------+
//| Check for high-impact JPY/USD news in ±24 hours                   |
//+------------------------------------------------------------------+
bool HasJPYorUSDHighImpactNews()
{
   datetime currentTime = TimeTradeServer();
   datetime from = currentTime - 3600 * 24;
   datetime to = currentTime + 3600 * 24;

   MqlCalendarValue values[];
   for (int i = 0; i < ArraySize(countryCodes); i++)
   {
      string countryCode = countryCodes[i];
      if (countryCode == "US" || countryCode == "JP")
      {
         int count = CalendarValueHistory(values, from, to, countryCode, NULL);
         if (count < 0)
         {
            PrintFormat("CalendarValueHistory failed for country %s! Error %d", countryCode, GetLastError());
            continue;
         }

         for (int j = 0; j < count; j++)
         {
            if (values[j].impact_type == 2) // High-impact events
            {
               return true;
            }
         }
      }
   }
   return false;
}

//+------------------------------------------------------------------+
//| Check if market is volatile enough (ATR > threshold)              |
//+------------------------------------------------------------------+
bool IsMarketVolatile(string symbol)
{
   int symbolIndex = ArrayFindSymbol(symbol);
   double atr[];
   ArraySetAsSeries(atr, true);
   CopyBuffer(atrHandle[symbolIndex], 0, 0, 1, atr);

   // Compare current ATR to a threshold (e.g., average ATR over 20 periods)
   double atrAvg[];
   ArraySetAsSeries(atrAvg, true);
   int atrHandleAvg = iMA(symbol, Timeframe, 20, 0, MODE_EMA, PRICE_CLOSE);
   if (atrHandleAvg == INVALID_HANDLE)
   {
      Print("Failed to create ATR average handle for ", symbol);
      return false;
   }
   CopyBuffer(atrHandleAvg, 0, 0, 1, atrAvg);
   IndicatorRelease(atrHandleAvg);

   return atr[0] >= atrAvg[0] * ATR_Filter_Ratio;
}

//+------------------------------------------------------------------+
//| Check volume confirmation (Volume > EMA Volume)                  |
//+------------------------------------------------------------------+
bool CheckVolumeConfirmation(string symbol)
{
   int symbolIndex = ArrayFindSymbol(symbol);
   double volume[], emaVolume[];
   ArraySetAsSeries(volume, true);
   ArraySetAsSeries(emaVolume, true);
   CopyBuffer(iVolume(symbol, Timeframe, VOLUME_TICK), 0, 0, 1, volume);
   CopyBuffer(volumeHandle[symbolIndex], 0, 0, 1, emaVolume);

   return volume[0] >= emaVolume[0] * Volume_Filter_Ratio;
}

//+------------------------------------------------------------------+
//| Check if price is near support/resistance (Fractals)              |
//+------------------------------------------------------------------+
bool CheckNearSupportResistance(string symbol, double price)
{
   int symbolIndex = ArrayFindSymbol(symbol);
   double fractalUpper[], fractalLower[], atr[];
   ArraySetAsSeries(fractalUpper, true);
   ArraySetAsSeries(fractalLower, true);
   ArraySetAsSeries(atr, true);
   CopyBuffer(fractalHandle[symbolIndex], UPPER_LINE, 0, SRLookback, fractalUpper);
   CopyBuffer(fractalHandle[symbolIndex], LOWER_LINE, 0, SRLookback, fractalLower);
   CopyBuffer(atrHandle[symbolIndex], 0, 0, 1, atr);

   double proximity = atr[0] * SR_Proximity_Factor;

   for (int i = 0; i < SRLookback; i++)
   {
      if (fractalUpper[i] != EMPTY_VALUE && MathAbs(price - fractalUpper[i]) <= proximity)
         return true;
      if (fractalLower[i] != EMPTY_VALUE && MathAbs(price - fractalLower[i]) <= proximity)
         return true;
   }
   return false;
}

//+------------------------------------------------------------------+
//| Get swing high/low for Fibonacci calculation                      |
//+------------------------------------------------------------------+
void GetSwingHighLow(string symbol, double &swingHigh, double &swingLow)
{
   swingHigh = iHigh(symbol, Timeframe, 1);
   swingLow = iLow(symbol, Timeframe, 1);

   for (int i = 2; i <= 3; i++)
   {
      double high = iHigh(symbol, Timeframe, i);
      double low = iLow(symbol, Timeframe, i);
      if (high > swingHigh)
         swingHigh = high;
      if (low < swingLow)
         swingLow = low;
   }
}

//+------------------------------------------------------------------+
//| Place Fibonacci Limit Orders after news                           |
//+------------------------------------------------------------------+
void PlaceFibonacciLimitOrders(string symbol, bool isBuy, int symbolIndex)
{
   double swingHigh, swingLow;
   GetSwingHighLow(symbol, swingHigh, swingLow);

   double range = swingHigh - swingLow;
   double atr[];
   ArraySetAsSeries(atr, true);
   CopyBuffer(atrHandle[symbolIndex], 0, 0, 1, atr);

   if (range < 2 * atr[0])
      return;

   double currentPrice = SymbolInfoDouble(symbol, SYMBOL_BID);
   double sl = CalculateDynamicSL(symbol, symbolIndex);
   double lot = CalculateLotSize(symbol, RiskPercent);
   datetime expiration = TimeCurrent() + FibonacciExpiration;

   CTrade trade;

   for (int i = 0; i < ArraySize(FibonacciLevels); i++)
   {
      double fibLevel;
      double limitPrice;

      if (isBuy)
      {
         fibLevel = swingHigh - range * FibonacciLevels[i];
         if (currentPrice > fibLevel && CheckNearSupportResistance(symbol, fibLevel))
         {
            limitPrice = fibLevel;
            double slPrice = swingLow - sl;
            double tpPrice = limitPrice + (limitPrice - slPrice) * Extended_RR_Ratio;
            trade.BuyLimit(lot, limitPrice, symbol, slPrice, tpPrice, ORDER_TIME_SPECIFIED, expiration, "Buy Limit - Fibonacci News");
         }
      }
      else
      {
         fibLevel = swingLow + range * FibonacciLevels[i];
         if (currentPrice < fibLevel && CheckNearSupportResistance(symbol, fibLevel))
         {
            limitPrice = fibLevel;
            double slPrice = swingHigh + sl;
            double tpPrice = limitPrice - (slPrice - limitPrice) * Extended_RR_Ratio;
            trade.SellLimit(lot, limitPrice, symbol, slPrice, tpPrice, ORDER_TIME_SPECIFIED, expiration, "Sell Limit - Fibonacci News");
         }
      }
   }
}

//+------------------------------------------------------------------+
//| Expert tick function                                              |
//+------------------------------------------------------------------+
void OnTick()
{
   ManagePositionsForAllSymbols();

   if (CountTotalTrades() >= MaxTotalTrades)
      return;

   for (int i = 0; i < ArraySize(Symbols); i++)
   {
      string symbol = Symbols[i];
      static datetime lastBar[];
      ArrayResize(lastBar, ArraySize(Symbols));
      datetime currentBar = iTime(symbol, Timeframe, 0);

      if (lastBar[i] != currentBar)
      {
         lastBar[i] = currentBar;

         if (IsAsianSession() && !HasJPYorUSDHighImpactNews())
         {
            Print("Skipping trading in Asian session: No JPY/USD high-impact news");
            continue;
         }

         if (!IsMarketVolatile(symbol))
         {
            Print("Skipping trading: Market volatility too low for ", symbol);
            continue;
         }

         int newsStatus = CheckNews(symbol);
         if (newsStatus == 0)
            continue;

         if (CountOpenTrades(symbol) >= MaxTradesPerSymbol)
            continue;

         LotSize = CalculateLotSize(symbol, RiskPercent);

         if (TradeOnNews && UseFibonacciLimit && newsStatus == 1)
         {
            double ema[], emaHigher[];
            ArraySetAsSeries(ema, true);
            ArraySetAsSeries(emaHigher, true);
            CopyBuffer(emaHandle[i], 0, 0, 5, ema);
            CopyBuffer(emaHandleHigher[i], 0, 0, 3, emaHigher);

            bool isBuy = iClose(symbol, Timeframe, 1) > ema[1] && iClose(symbol, HigherTimeframe, 1) > emaHigher[1];
            bool isSell = iClose(symbol, Timeframe, 1) < ema[1] && iClose(symbol, HigherTimeframe, 1) < emaHigher[1];

            if (isBuy || isSell)
            {
               PlaceFibonacciLimitOrders(symbol, isBuy, i);
               continue;
            }
         }

         if ((CheckPinBar(symbol, i) || CheckInsideBar(symbol)) && CheckVolumeConfirmation(symbol))
         {
            double ema[], rsi[], emaHigher[];
            ArraySetAsSeries(ema, true);
            ArraySetAsSeries(rsi, true);
            ArraySetAsSeries(emaHigher, true);
            CopyBuffer(emaHandle[i], 0, 0, 5, ema);
            CopyBuffer(rsiHandle[i], 0, 0, 3, rsi);
            CopyBuffer(emaHandleHigher[i], 0, 0, 3, emaHigher);

            bool strongTrend = IsStrongTrend(symbol);
            bool hasMomentum = CheckPriceMomentum(symbol);

            if (CheckOrderBlock(symbol, true, i) && iClose(symbol, Timeframe, 1) > ema[1] &&
                rsi[1] < RSI_Oversold && iClose(symbol, HigherTimeframe, 1) > emaHigher[1] &&
                hasMomentum && CheckNearSupportResistance(symbol, iClose(symbol, Timeframe, 1)) &&
                (newsStatus == 1 || !TradeOnNews))
            {
               OpenBuyTrade(symbol, strongTrend, i);
            }
            else if (CheckOrderBlock(symbol, false, i) && iClose(symbol, Timeframe, 1) < ema[1] &&
                     rsi[1] > RSI_Overbought && iClose(symbol, HigherTimeframe, 1) < emaHigher[1] &&
                     hasMomentum && CheckNearSupportResistance(symbol, iClose(symbol, Timeframe, 1)) &&
                     (newsStatus == 1 || !TradeOnNews))
            {
               OpenSellTrade(symbol, strongTrend, i);
            }
         }
      }
   }
}

//+------------------------------------------------------------------+
//| Check news status for a symbol using MT5 Calendar                 |
//+------------------------------------------------------------------+
int CheckNews(string symbol)
{
   datetime currentTime = TimeTradeServer();
   datetime from = currentTime - 3600 * 24;
   datetime to = currentTime + 3600 * 24;

   MqlCalendarValue values[];
   for (int i = 0; i < ArraySize(countryCodes); i++)
   {
      string countryCode = countryCodes[i];
      int count = CalendarValueHistory(values, from, to, countryCode, NULL);
      if (count < 0)
      {
         PrintFormat("CalendarValueHistory failed for country %s! Error %d", countryCode, GetLastError());
         continue;
      }

      for (int j = 0; j < count; j++)
      {
         bool affectsSymbol = false;
         if (countryCode == "US")
            affectsSymbol = (symbol == "EURUSD" || symbol == "USDJPY" || symbol == "GBPUSD");
         else if (countryCode == "EU")
            affectsSymbol = (symbol == "EURUSD");
         else if (countryCode == "GB")
            affectsSymbol = (symbol == "GBPUSD");
         else if (countryCode == "JP")
            affectsSymbol = (symbol == "USDJPY");

         if (affectsSymbol && values[j].impact_type == 2) // High-impact events
         {
            datetime newsTime = values[j].time;
            int secondsToNews = (int)(currentTime - newsTime);

            if (secondsToNews >= -NewsAvoidBefore && secondsToNews < NewsAvoidAfter)
            {
               if (TradeOnNews && secondsToNews >= NewsTradeDelay && secondsToNews < NewsAvoidAfter)
               {
                  PrintFormat("Trading allowed %d s after high-impact news: event ID %I64d", NewsTradeDelay, values[j].event_id);
                  return 1;
               }
               PrintFormat("Skipping trading due to high-impact news: event ID %I64d", values[j].event_id);
               return 0;
            }
         }
      }
   }
   return -1;
}

//+------------------------------------------------------------------+
//| Calculate lot size based on risk percentage                       |
//+------------------------------------------------------------------+
double CalculateLotSize(string symbol, double riskPercent)
{
   double balance = AccountInfoDouble(ACCOUNT_BALANCE);
   double riskAmount = balance * (riskPercent / 100);
   double tickSize = SymbolInfoDouble(symbol, SYMBOL_TRADE_TICK_SIZE);
   double tickValue = SymbolInfoDouble(symbol, SYMBOL_TRADE_TICK_VALUE);
   StopLoss = CalculateDynamicSL(symbol, ArrayFindSymbol(symbol));
   double slPips = StopLoss / (tickSize * 10);
   double lot = NormalizeDouble(riskAmount / (slPips * tickValue), 2);
   return MathMin(lot, SymbolInfoDouble(symbol, SYMBOL_VOLUME_MAX));
}

//+------------------------------------------------------------------+
//| Find symbol index in Symbols array                                |
//+------------------------------------------------------------------+
int ArrayFindSymbol(string symbol)
{
   for (int i = 0; i < ArraySize(Symbols); i++)
      if (Symbols[i] == symbol)
         return i;
   return 0;
}

//+------------------------------------------------------------------+
//| Calculate dynamic stop loss based on ATR                          |
//+------------------------------------------------------------------+
double CalculateDynamicSL(string symbol, int symbolIndex)
{
   double atr[];
   ArraySetAsSeries(atr, true);
   CopyBuffer(atrHandle[symbolIndex], 0, 0, 1, atr);
   symbolIndex = MathMin(symbolIndex, ArraySize(ATR_Multiplier) - 1);
   return atr[0] * ATR_Multiplier[symbolIndex];
}

//+------------------------------------------------------------------+
//| Check for strong trend based on EMA slope                         |
//+------------------------------------------------------------------+
bool IsStrongTrend(string symbol)
{
   int symbolIndex = ArrayFindSymbol(symbol);
   double ema[];
   ArraySetAsSeries(ema, true);
   CopyBuffer(emaHandle[symbolIndex], 0, 0, 5, ema);
   double slope = (ema[0] - ema[4]) / 4;
   double atr[];
   ArraySetAsSeries(atr, true);
   CopyBuffer(atrHandle[symbolIndex], 0, 0, 1, atr);
   return MathAbs(slope) > atr[0] * 0.5;
}

//+------------------------------------------------------------------+
//| Check price momentum for signal confirmation                      |
//+------------------------------------------------------------------+
bool CheckPriceMomentum(string symbol)
{
   double close1 = iClose(symbol, Timeframe, 1);
   double close2 = iClose(symbol, Timeframe, 2);
   double atr[];
   ArraySetAsSeries(atr, true);
   int symbolIndex = ArrayFindSymbol(symbol);
   CopyBuffer(atrHandle[symbolIndex], 0, 0, 1, atr);
   double momentum = MathAbs(close1 - close2);
   return momentum > atr[0] * 0.3;
}

//+------------------------------------------------------------------+
//| Check for Pin Bar                                                 |
//+------------------------------------------------------------------+
bool CheckPinBar(string symbol, int symbolIndex)
{
   double open = iOpen(symbol, Timeframe, 1);
   double close = iClose(symbol, Timeframe, 1);
   double high = iHigh(symbol, Timeframe, 1);
   double low = iLow(symbol, Timeframe, 1);
   double body = MathAbs(open - close);
   double upperTail = high - MathMax(open, close);
   double lowerTail = MathMin(open, close) - low;
   double totalRange = high - low;

   symbolIndex = MathMin(symbolIndex, ArraySize(PinBarTailRatio) - 1);
   if (lowerTail > body * PinBarTailRatio[symbolIndex] && lowerTail > totalRange * 0.6)
      return true;
   if (upperTail > body * PinBarTailRatio[symbolIndex] && upperTail > totalRange * 0.6)
      return true;
   return false;
}

//+------------------------------------------------------------------+
//| Check for Inside Bar                                              |
//+------------------------------------------------------------------+
bool CheckInsideBar(string symbol)
{
   double high1 = iHigh(symbol, Timeframe, 1);
   double low1 = iLow(symbol, Timeframe, 1);
   double high2 = iHigh(symbol, Timeframe, 2);
   double low2 = iLow(symbol, Timeframe, 2);

   if (high1 < high2 && low1 > low2)
      return true;
   return false;
}

//+------------------------------------------------------------------+
//| Check for Order Block (supply/demand zone)                        |
//+------------------------------------------------------------------+
bool CheckOrderBlock(string symbol, bool isBullish, int symbolIndex)
{
   for (int i = 1; i <= OrderBlockLookback; i++)
   {
      double open = iOpen(symbol, Timeframe, i);
      double close = iClose(symbol, Timeframe, i);
      double high = iHigh(symbol, Timeframe, i);
      double low = iLow(symbol, Timeframe, i);
      double currentPrice = iClose(symbol, Timeframe, 1);

      if (isBullish && close > open && (close - open) > (high - low) * 0.7 &&
          CheckVolumeConfirmation(symbol))
      {
         if (currentPrice <= high && currentPrice >= low)
            return true;
      }
      if (!isBullish && close < open && (open - close) > (high - low) * 0.7 &&
          CheckVolumeConfirmation(symbol))
      {
         if (currentPrice >= low && currentPrice <= high)
            return true;
      }
   }
   return false;
}

//+------------------------------------------------------------------+
//| Find nearest support/resistance level                             |
//+------------------------------------------------------------------+
double FindNearestLevel(string symbol, bool isBuy)
{
   double level = 0;
   for (int i = 1; i <= 5; i++)
   {
      double high = iHigh(symbol, Timeframe, i);
      double low = iLow(symbol, Timeframe, i);
      if (isBuy && (level == 0 || low < level))
         level = low;
      if (!isBuy && (level == 0 || high > level))
         level = high;
   }
   return level;
}

//+------------------------------------------------------------------+
//| Open Buy Trade (Market Order)                                     |
//+------------------------------------------------------------------+
void OpenBuyTrade(string symbol, bool strongTrend, int symbolIndex)
{
   double price = SymbolInfoDouble(symbol, SYMBOL_ASK);
   double sl = iLow(symbol, Timeframe, 1) - CalculateDynamicSL(symbol, symbolIndex);
   double tp = price + (price - sl) * (strongTrend ? Extended_RR_Ratio : RR_Ratio);
   double lot = CalculateLotSize(symbol, RiskPercent);

   CTrade trade;
   trade.Buy(lot, symbol, price, sl, tp, "Buy Order - Price Action News");
}

//+------------------------------------------------------------------+
//| Open Sell Trade (Market Order)                                    |
//+------------------------------------------------------------------+
void OpenSellTrade(string symbol, bool strongTrend, int symbolIndex)
{
   double price = SymbolInfoDouble(symbol, SYMBOL_BID);
   double sl = iHigh(symbol, Timeframe, 1) + CalculateDynamicSL(symbol, symbolIndex);
   double tp = price - (sl - price) * (strongTrend ? Extended_RR_Ratio : RR_Ratio);
   double lot = CalculateLotSize(symbol, RiskPercent);

   CTrade trade;
   trade.Sell(lot, symbol, price, sl, tp, "Sell Order - Price Action News");
}

//+------------------------------------------------------------------+
//| Manage positions for all symbols in real-time                     |
//+------------------------------------------------------------------+
void ManagePositionsForAllSymbols()
{
   for (int i = PositionsTotal() - 1; i >= 0; i--)
   {
      ulong ticket = PositionGetTicket(i);
      if (PositionSelectByTicket(ticket))
      {
         string symbol = PositionGetString(POSITION_SYMBOL);
         double openPrice = PositionGetDouble(POSITION_PRICE_OPEN);
         double sl = PositionGetDouble(POSITION_SL);
         double tp = PositionGetDouble(POSITION_TP);
         double currentPrice = SymbolInfoDouble(symbol, PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_BUY ? SYMBOL_BID : SYMBOL_ASK);
         double profit = PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_BUY ? currentPrice - openPrice : openPrice - currentPrice;
         double initialSL = PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_BUY ? openPrice - sl : sl - openPrice;

         double dynamicSL = CalculateDynamicSL(symbol, ArrayFindSymbol(symbol));
         CTrade trade;

         if (profit >= initialSL * BreakevenFactor && sl != openPrice)
         {
            double newSL = openPrice;
            trade.PositionModify(ticket, newSL, tp);
            continue;
         }

         if (profit >= initialSL * PartialCloseRatio && PositionGetDouble(POSITION_VOLUME) > 0)
         {
            double closeLot = PositionGetDouble(POSITION_VOLUME) * 0.5;
            trade.PositionClosePartial(ticket, closeLot);

            double newTP = PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_BUY ? openPrice + initialSL * (IsStrongTrend(symbol) ? 2.5 : Extended_RR_Ratio) : openPrice - initialSL * (IsStrongTrend(symbol) ? 2.5 : Extended_RR_Ratio);
            trade.PositionModify(ticket, sl, newTP);
            continue;
         }

         if (profit >= initialSL * TrailingStopFactor && CheckPriceMomentum(symbol))
         {
            double nearestLevel = FindNearestLevel(symbol, PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_BUY);
            double newSL = PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_BUY ? MathMax(currentPrice - dynamicSL * 0.75, nearestLevel) : MathMin(currentPrice + dynamicSL * 0.75, nearestLevel);
            if ((PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_BUY && newSL > sl) ||
                (PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_SELL && newSL < sl))
            {
               double newTP = PositionGetInteger(POSITION_TYPE) == POSITION_TYPE_BUY ? openPrice + initialSL * (IsStrongTrend(symbol) ? 2.5 : Extended_RR_Ratio) : openPrice - initialSL * (IsStrongTrend(symbol) ? 2.5 : Extended_RR_Ratio);
               trade.PositionModify(ticket, newSL, newTP);
            }
         }
      }
   }
}

//+------------------------------------------------------------------+
//| Count total open trades across all symbols                        |
//+------------------------------------------------------------------+
int CountTotalTrades()
{
   return PositionsTotal();
}

//+------------------------------------------------------------------+
//| Count open trades for a symbol                                    |
//+------------------------------------------------------------------+
int CountOpenTrades(string symbol)
{
   int count = 0;
   for (int i = PositionsTotal() - 1; i >= 0; i--)
   {
      ulong ticket = PositionGetTicket(i);
      if (PositionSelectByTicket(ticket) && PositionGetString(POSITION_SYMBOL) == symbol)
         count++;
   }
   return count;
}
//+------------------------------------------------------------------+
