using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk {
    public static class StockDataExtensions
    {
        public static decimal BodySize(this StockData data)
        {
            return Math.Abs(data.Close - data.Open);
        }

        public static decimal Range(this StockData data)
        {
            return data.High - data.Low;
        }

        public static decimal UpperWickSize(this StockData data)
        {
            return data.IsBullish
                ? data.High - data.Close
                : data.High - data.Open;
        }

        public static decimal LowerWickSize(this StockData data)
        {
            return data.IsBullish
                ? data.Open - data.Low
                : data.Close - data.Low;
        }

        public static decimal BodyMidPoint(this StockData data)
        {
            return (data.Open + data.Close) / 2;
        }

        public static bool IsBullish(this StockData data)
        {
            return data.Close > data.Open;
        }

        public static bool IsBearish(this StockData data)
        {
            return data.Close < data.Open;
        }
    }
    public class CandlestickPatternDetector
    {
        // Các ngưỡng được định nghĩa như hằng số để dễ điều chỉnh
        private const decimal DOJI_THRESHOLD = 0.1m;        // Thân nến < 10% tổng phạm vi được coi là Doji
        private const decimal MARUBOZU_THRESHOLD = 0.05m;   // Bóng nến < 5% được coi là không đáng kể
        private const decimal SPINNER_BODY_THRESHOLD = 0.4m; // Thân nến < 40% được coi là Spinning Top
        private const decimal TWEEZER_THRESHOLD = 0.002m;   // Sai số < 0.2% cho Tweezer
        private const decimal HARAMI_THRESHOLD = 0.6m;      // Thân nến con < 60% thân nến mẹ cho Harami
        private const decimal PRICE_GAP_THRESHOLD = 0.01m;  // Khoảng trống giá > 1% 
        private const decimal HAMMER_BODY_POSITION = 0.35m; // Thân nến nằm trong 35% trên cùng/dưới cùng
        private const decimal WICK_TO_BODY_RATIO = 2.0m;    // Bóng nến >= 2x thân nến
        private const decimal VOLUME_SURGE_THRESHOLD = 2.0m; // Tăng khối lượng > 2x
        private const decimal PRICE_MOVE_THRESHOLD = 3.0m;  // Biến động giá > 3%
        private const decimal WICK_THRESHOLD = 0.8m;        // Bóng nến đáng kể cho Spinning Top

        public static List<CandlestickPattern> DetectPatterns(List<StockData> historyData)
        {
            var patterns = new List<CandlestickPattern>();
            int count = historyData.Count;

            if (count < 1)
                return patterns;

            // Xác định xu hướng
            TrendInfo trend = DetermineTrend(historyData);

            // Phát hiện mẫu hình 1 nến
            DetectSingleCandlePatterns(historyData, patterns, trend);

            // Phát hiện mẫu hình 2 nến (nếu có đủ dữ liệu)
            if (count >= 2)
                DetectTwoCandlePatterns(historyData, patterns, trend);

            // Phát hiện mẫu hình 3 nến (nếu có đủ dữ liệu)
            if (count >= 3)
                DetectThreeCandlePatterns(historyData, patterns, trend);

            // Phát hiện mẫu hình 5 nến (nếu có đủ dữ liệu)
            if (count >= 5)
                DetectFiveCandlePatterns(historyData, patterns, trend);

            return patterns;
        }

        private static TrendInfo DetermineTrend(List<StockData> historyData)
        {
            int count = historyData.Count;
            var trend = new TrendInfo();

            // Xác định xu hướng dựa trên ít nhất 5 ngày trước đó
            if (count >= 6)
            {
                int downDays = 0;
                int upDays = 0;

                for (int i = count - 6; i < count - 1; i++)
                {
                    if (historyData[i].Close < historyData[i].Open)
                        downDays++;
                    else if (historyData[i].Close > historyData[i].Open)
                        upDays++;
                }

                trend.IsDowntrend = downDays >= 3 && historyData[count - 6].Close > historyData[count - 2].Close;
                trend.IsUptrend = upDays >= 3 && historyData[count - 6].Close < historyData[count - 2].Close;
            }

            // Xác định xu hướng ngắn hạn 3 ngày
            if (count >= 3)
            {
                trend.IsShortTermUptrend = historyData[^3].Close < historyData[^2].Close &&
                                          historyData[^2].Close < historyData[^1].Close;

                trend.IsShortTermDowntrend = historyData[^3].Close > historyData[^2].Close &&
                                            historyData[^2].Close > historyData[^1].Close;
            }

            return trend;
        }

        private static void DetectSingleCandlePatterns(List<StockData> historyData, List<CandlestickPattern> patterns, TrendInfo trend)
        {
            var latest = historyData[^1]; // Ngày gần nhất
            decimal latestBody = latest.BodySize;
            decimal latestRange = latest.Range;

            if (latestRange <= 0.0001m)
                return; // Tránh chia cho 0

            // Phát hiện Doji
            if (latestBody / latestRange < DOJI_THRESHOLD)
            {
                DetectDojiPatterns(latest, patterns, latestBody, latestRange);
            }

            // Phát hiện Marubozu
            DetectMarubozuPatterns(latest, patterns, latestRange);

            // Phát hiện Hammer/Hanging Man
            DetectHammerPatterns(latest, patterns, trend, latestBody, latestRange);

            // Phát hiện Inverted Hammer/Shooting Star
            DetectInvertedHammerPatterns(latest, patterns, trend, latestBody, latestRange);

            // Phát hiện Spinning Top
            DetectSpinningTop(latest, patterns, latestBody, latestRange);
        }

        private static void DetectDojiPatterns(StockData latest, List<CandlestickPattern> patterns, decimal latestBody, decimal latestRange)
        {
            // Phân loại Doji chi tiết
            if (latest.Open == latest.Close) // Doji hoàn hảo
            {
                patterns.Add(new CandlestickPattern(
                    "Four-Price Doji",
                    "Xuất hiện Four-Price Doji (thân nến không tồn tại - biến động cực thấp)",
                    PatternType.Doji));
            }
            else if (Math.Abs(latest.High - latest.Low) > 3 * latestBody &&
                    Math.Abs(latest.High - Math.Max(latest.Open, latest.Close)) > latestBody &&
                    Math.Abs(Math.Min(latest.Open, latest.Close) - latest.Low) > latestBody)
            {
                patterns.Add(new CandlestickPattern(
                    "Long-Legged Doji",
                    "Xuất hiện Long-Legged Doji (bóng dài hai phía - lưỡng lự mạnh)",
                    PatternType.Doji));
            }
            else if (Math.Abs(latest.High - Math.Max(latest.Open, latest.Close)) > 3 * latestBody &&
                    Math.Abs(Math.Min(latest.Open, latest.Close) - latest.Low) < 0.1m * latestRange)
            {
                patterns.Add(new CandlestickPattern(
                    "Gravestone Doji",
                    "Xuất hiện Gravestone Doji (bóng trên dài - dấu hiệu đảo chiều giảm)",
                    PatternType.Reversal,
                    Direction.Bearish));
            }
            else if (Math.Abs(Math.Min(latest.Open, latest.Close) - latest.Low) > 3 * latestBody &&
                    Math.Abs(latest.High - Math.Max(latest.Open, latest.Close)) < 0.1m * latestRange)
            {
                patterns.Add(new CandlestickPattern(
                    "Dragonfly Doji",
                    "Xuất hiện Dragonfly Doji (bóng dưới dài - dấu hiệu đảo chiều tăng)",
                    PatternType.Reversal,
                    Direction.Bullish));
            }
            else
            {
                patterns.Add(new CandlestickPattern(
                    "Doji",
                    "Xuất hiện Nến Doji (cho thấy sự lưỡng lự)",
                    PatternType.Doji));
            }
        }

        private static void DetectMarubozuPatterns(StockData latest, List<CandlestickPattern> patterns, decimal latestRange)
        {
            // Marubozu tăng (Đóng gần Cao, Mở gần Thấp, bóng nhỏ < threshold)
            if (latest.IsBullish &&
                Math.Abs(latest.Close - latest.High) / latestRange < MARUBOZU_THRESHOLD &&
                Math.Abs(latest.Open - latest.Low) / latestRange < MARUBOZU_THRESHOLD)
            {
                patterns.Add(new CandlestickPattern(
                    "Bullish Marubozu",
                    "Mẫu hình Nến Marubozu tăng (lực mua mạnh)",
                    PatternType.Continuation,
                    Direction.Bullish));
            }
            // Marubozu giảm
            else if (latest.IsBearish &&
                    Math.Abs(latest.Close - latest.Low) / latestRange < MARUBOZU_THRESHOLD &&
                    Math.Abs(latest.Open - latest.High) / latestRange < MARUBOZU_THRESHOLD)
            {
                patterns.Add(new CandlestickPattern(
                    "Bearish Marubozu",
                    "Mẫu hình Nến Marubozu giảm (lực bán mạnh)",
                    PatternType.Continuation,
                    Direction.Bearish));
            }
            // Closing Marubozu tăng
            else if (latest.IsBullish &&
                    Math.Abs(latest.Close - latest.High) / latestRange < MARUBOZU_THRESHOLD &&
                    Math.Abs(latest.Open - latest.Low) / latestRange >= MARUBOZU_THRESHOLD)
            {
                patterns.Add(new CandlestickPattern(
                    "Closing Bullish Marubozu",
                    "Mẫu hình Closing Marubozu tăng (thân dài đóng cửa gần mức cao)",
                    PatternType.Continuation,
                    Direction.Bullish));
            }
            // Closing Marubozu giảm
            else if (latest.IsBearish &&
                    Math.Abs(latest.Close - latest.Low) / latestRange < MARUBOZU_THRESHOLD &&
                    Math.Abs(latest.Open - latest.High) / latestRange >= MARUBOZU_THRESHOLD)
            {
                patterns.Add(new CandlestickPattern(
                    "Closing Bearish Marubozu",
                    "Mẫu hình Closing Marubozu giảm (thân dài đóng cửa gần mức thấp)",
                    PatternType.Continuation,
                    Direction.Bearish));
            }
        }

        private static void DetectHammerPatterns(StockData latest, List<CandlestickPattern> patterns, TrendInfo trend,
                                               decimal latestBody, decimal latestRange)
        {
            if (latestBody <= 0.0001m)
                return; // Tránh chia cho 0

            decimal lowerWick = latest.LowerWickSize;
            decimal upperWick = latest.UpperWickSize;

            // Thân nến ở phần trên và bóng dưới dài
            if (lowerWick >= WICK_TO_BODY_RATIO * latestBody &&
                upperWick / latestRange < 0.1m &&
                (latest.Open >= latest.High - latestRange * HAMMER_BODY_POSITION) &&
                (latest.Close >= latest.High - latestRange * HAMMER_BODY_POSITION))
            {
                if (trend.IsDowntrend)
                    patterns.Add(new CandlestickPattern(
                        "Hammer",
                        "Mẫu hình Nến Hammer/Búa (đảo chiều tăng - xuất hiện trong xu hướng giảm)",
                        PatternType.Reversal,
                        Direction.Bullish));
                else if (trend.IsUptrend)
                    patterns.Add(new CandlestickPattern(
                        "Hanging Man",
                        "Mẫu hình Nến Hanging Man/Người treo cổ (đảo chiều giảm - xuất hiện ở đỉnh xu hướng tăng)",
                        PatternType.Reversal,
                        Direction.Bearish));
                else
                {
                    if (latest.IsBullish)
                        patterns.Add(new CandlestickPattern(
                            "Hammer",
                            "Mẫu hình Nến Hammer/Búa (có thể đảo chiều tăng - cần xác nhận xu hướng)",
                            PatternType.Reversal,
                            Direction.Bullish));
                    else
                        patterns.Add(new CandlestickPattern(
                            "Hanging Man",
                            "Mẫu hình Nến Hanging Man/Người treo cổ (có thể đảo chiều giảm - cần xác nhận xu hướng)",
                            PatternType.Reversal,
                            Direction.Bearish));
                }
            }
        }

        private static void DetectInvertedHammerPatterns(StockData latest, List<CandlestickPattern> patterns, TrendInfo trend,
                                                       decimal latestBody, decimal latestRange)
        {
            if (latestBody <= 0.0001m)
                return; // Tránh chia cho 0

            decimal lowerWick = latest.LowerWickSize;
            decimal upperWick = latest.UpperWickSize;

            // Thân nến ở phần dưới và bóng trên dài
            if (upperWick >= WICK_TO_BODY_RATIO * latestBody &&
                lowerWick / latestRange < 0.1m &&
                (latest.Open <= latest.Low + latestRange * HAMMER_BODY_POSITION) &&
                (latest.Close <= latest.Low + latestRange * HAMMER_BODY_POSITION))
            {
                if (trend.IsUptrend)
                    patterns.Add(new CandlestickPattern(
                        "Shooting Star",
                        "Mẫu hình Nến Shooting Star/Sao băng (đảo chiều giảm - xuất hiện trong xu hướng tăng)",
                        PatternType.Reversal,
                        Direction.Bearish));
                else if (trend.IsDowntrend)
                    patterns.Add(new CandlestickPattern(
                        "Inverted Hammer",
                        "Mẫu hình Nến Inverted Hammer/Búa ngược (đảo chiều tăng - xuất hiện ở đáy xu hướng giảm)",
                        PatternType.Reversal,
                        Direction.Bullish));
                else
                {
                    if (latest.IsBearish)
                        patterns.Add(new CandlestickPattern(
                            "Shooting Star",
                            "Mẫu hình Nến Shooting Star/Sao băng (có thể đảo chiều giảm - cần xác nhận xu hướng)",
                            PatternType.Reversal,
                            Direction.Bearish));
                    else
                        patterns.Add(new CandlestickPattern(
                            "Inverted Hammer",
                            "Mẫu hình Nến Inverted Hammer/Búa ngược (có thể đảo chiều tăng - cần xác nhận xu hướng)",
                            PatternType.Reversal,
                            Direction.Bullish));
                }
            }
        }

        private static void DetectSpinningTop(StockData latest, List<CandlestickPattern> patterns, decimal latestBody, decimal latestRange)
        {
            if (latestBody <= 0.0001m)
                return; // Tránh chia cho 0

            decimal lowerWick = latest.LowerWickSize;
            decimal upperWick = latest.UpperWickSize;

            if (latestBody / latestRange < SPINNER_BODY_THRESHOLD && // Thân nến chiếm dưới 40% phạm vi
                lowerWick >= latestBody * WICK_THRESHOLD && // Bóng dưới đáng kể
                upperWick >= latestBody * WICK_THRESHOLD)   // Bóng trên đáng kể
            {
                patterns.Add(new CandlestickPattern(
                    "Spinning Top",
                    "Mẫu hình Spinning Top (cân bằng giữa người mua và người bán)",
                    PatternType.Indecision));
            }
        }

        private static void DetectTwoCandlePatterns(List<StockData> historyData, List<CandlestickPattern> patterns, TrendInfo trend)
        {
            var latest = historyData[^1];
            var previous = historyData[^2];
            decimal prevBody = previous.BodySize;
            decimal prevRange = previous.Range;

            // Phát hiện mẫu hình Engulfing
            DetectEngulfingPatterns(latest, previous, patterns);

            // Phát hiện mẫu hình Tweezer
            DetectTweezerPatterns(latest, previous, patterns);

            // Phát hiện Khối lượng đột biến
            DetectVolumeSurges(historyData, patterns);

            // Phát hiện Piercing Line & Dark Cloud Cover
            DetectPiercingAndDarkCloud(latest, previous, patterns);

            // Phát hiện Harami
            DetectHaramiPatterns(latest, previous, patterns, prevBody);

            // Phát hiện Inside Bar & Outside Bar
            DetectInsideOutsideBar(latest, previous, patterns);

            // Phát hiện Gap Up & Gap Down
            DetectGapPatterns(latest, previous, patterns);

            // Phát hiện Matching Low & Matching High
            DetectMatchingPatterns(latest, previous, patterns);
        }

        private static void DetectEngulfingPatterns(StockData latest, StockData previous, List<CandlestickPattern> patterns)
        {
            // Strict Bullish Engulfing (thân và bóng nến)
            if (previous.IsBearish && latest.IsBullish &&
                latest.Close >= previous.Open && latest.Open <= previous.Close && // Thân sau nhấn chìm thân trước
                latest.Low <= previous.Low && latest.High >= previous.High)      // Nến sau bao trùm toàn bộ nến trước
            {
                patterns.Add(new CandlestickPattern(
                    "Bullish Engulfing (Strict)",
                    "Mẫu hình Nến Nhấn chìm tăng (Bullish Engulfing - Strict)",
                    PatternType.Reversal,
                    Direction.Bullish));
            }
            // Strict Bearish Engulfing (thân và bóng nến)
            else if (previous.IsBullish && latest.IsBearish &&
                latest.Close <= previous.Open && latest.Open >= previous.Close && // Thân sau nhấn chìm thân trước
                latest.Low <= previous.Low && latest.High >= previous.High)      // Nến sau bao trùm toàn bộ nến trước
            {
                patterns.Add(new CandlestickPattern(
                    "Bearish Engulfing (Strict)",
                    "Mẫu hình Nến Nhấn chìm giảm (Bearish Engulfing - Strict)",
                    PatternType.Reversal,
                    Direction.Bearish));
            }
            // Phiên bản Engulfing thông thường (chỉ nhấn chìm thân)
            else if (previous.IsBearish && latest.IsBullish &&
                latest.Close > previous.Open && latest.Open < previous.Close)
            {
                patterns.Add(new CandlestickPattern(
                    "Bullish Engulfing",
                    "Mẫu hình Nến Nhấn chìm tăng (Bullish Engulfing - thân nến)",
                    PatternType.Reversal,
                    Direction.Bullish));
            }
            else if (previous.IsBullish && latest.IsBearish &&
                latest.Close < previous.Open && latest.Open > previous.Close)
            {
                patterns.Add(new CandlestickPattern(
                    "Bearish Engulfing",
                    "Mẫu hình Nến Nhấn chìm giảm (Bearish Engulfing - thân nến)",
                    PatternType.Reversal,
                    Direction.Bearish));
            }
        }

        private static void DetectTweezerPatterns(StockData latest, StockData previous, List<CandlestickPattern> patterns)
        {
            // Tweezer Bottoms (đáy nhíp - hai nến có mức thấp gần nhau)
            if (Math.Abs(latest.Low - previous.Low) / previous.Low < TWEEZER_THRESHOLD)
            {
                if (previous.IsBearish && latest.IsBullish)
                    patterns.Add(new CandlestickPattern(
                        "Tweezer Bottoms",
                        "Mẫu hình Tweezer Bottoms (Đáy nhíp - có thể đảo chiều tăng)",
                        PatternType.Reversal,
                        Direction.Bullish));
            }
            // Tweezer Tops (đỉnh nhíp - hai nến có mức cao gần nhau)
            if (Math.Abs(latest.High - previous.High) / previous.High < TWEEZER_THRESHOLD)
            {
                if (previous.IsBullish && latest.IsBearish)
                    patterns.Add(new CandlestickPattern(
                        "Tweezer Tops",
                        "Mẫu hình Tweezer Tops (Đỉnh nhíp - có thể đảo chiều giảm)",
                        PatternType.Reversal,
                        Direction.Bearish));
            }
        }

        private static void DetectVolumeSurges(List<StockData> historyData, List<CandlestickPattern> patterns)
        {
            int count = historyData.Count;
            var latest = historyData[^1];
            var previous = historyData[^2];

            // Sử dụng trung bình 5 ngày nếu có đủ dữ liệu
            if (count >= 6)
            {
                decimal avgVolume = 0;
                for (int i = count - 6; i < count - 1; i++)
                {
                    avgVolume += historyData[i].Volume;
                }
                avgVolume /= 5;

                if (latest.Volume > avgVolume * VOLUME_SURGE_THRESHOLD)
                {
                    if (latest.PercentChange > PRICE_MOVE_THRESHOLD)
                        patterns.Add(new CandlestickPattern(
                            "Volume Surge with Price Rise",
                            $"Tăng khối lượng đột biến (>{VOLUME_SURGE_THRESHOLD}x TB) kèm tăng giá mạnh (>{PRICE_MOVE_THRESHOLD}%)",
                            PatternType.Continuation,
                            Direction.Bullish));
                    else if (latest.PercentChange < -PRICE_MOVE_THRESHOLD)
                        patterns.Add(new CandlestickPattern(
                            "Volume Surge with Price Drop",
                            $"Tăng khối lượng đột biến (>{VOLUME_SURGE_THRESHOLD}x TB) kèm giảm giá mạnh (<-{PRICE_MOVE_THRESHOLD}%)",
                            PatternType.Continuation,
                            Direction.Bearish));
                }
            }
            else
            {
                // So sánh với ngày trước khi không đủ dữ liệu
                if (latest.Volume > previous.Volume * VOLUME_SURGE_THRESHOLD)
                {
                    if (latest.PercentChange > PRICE_MOVE_THRESHOLD)
                        patterns.Add(new CandlestickPattern(
                            "Volume Surge with Price Rise",
                            $"Tăng khối lượng đột biến (>{VOLUME_SURGE_THRESHOLD}x) kèm tăng giá mạnh (>{PRICE_MOVE_THRESHOLD}%)",
                            PatternType.Continuation,
                            Direction.Bullish));
                    else if (latest.PercentChange < -PRICE_MOVE_THRESHOLD)
                        patterns.Add(new CandlestickPattern(
                            "Volume Surge with Price Drop",
                            $"Tăng khối lượng đột biến (>{VOLUME_SURGE_THRESHOLD}x) kèm giảm giá mạnh (<-{PRICE_MOVE_THRESHOLD}%)",
                            PatternType.Continuation,
                            Direction.Bearish));
                }
            }
        }

        private static void DetectPiercingAndDarkCloud(StockData latest, StockData previous, List<CandlestickPattern> patterns)
        {
            // Mẫu hình Piercing Line (Nến xuyên - Bullish)
            if (previous.IsBearish && latest.IsBullish &&
                latest.Open < previous.Close &&
                latest.Close > previous.BodyMidPoint &&
                latest.Close < previous.Open)
            {
                patterns.Add(new CandlestickPattern(
                    "Piercing Line",
                    "Mẫu hình Nến Piercing Line/Xuyên thấu (có thể đảo chiều tăng)",
                    PatternType.Reversal,
                    Direction.Bullish));
            }

            // Mẫu hình Dark Cloud Cover (Mây đen che phủ - Bearish)
            if (previous.IsBullish && latest.IsBearish &&
                latest.Open > previous.Close &&
                latest.Close < previous.BodyMidPoint &&
                latest.Close > previous.Open)
            {
                patterns.Add(new CandlestickPattern(
                    "Dark Cloud Cover",
                    "Mẫu hình Nến Dark Cloud Cover/Mây đen che phủ (có thể đảo chiều giảm)",
                    PatternType.Reversal,
                    Direction.Bearish));
            }
        }

        private static void DetectHaramiPatterns(StockData latest, StockData previous, List<CandlestickPattern> patterns, decimal prevBody)
        {
            decimal latestBody = latest.BodySize;
            decimal latestRange = latest.Range;

            // Mẫu hình Harami (Mẹ bồng con)
            bool latestBodyInsidePreviousBody =
                Math.Max(latest.Open, latest.Close) < Math.Max(previous.Open, previous.Close) &&
                Math.Min(latest.Open, latest.Close) > Math.Min(previous.Open, previous.Close);

            if (latestBodyInsidePreviousBody && latestBody < prevBody * HARAMI_THRESHOLD) // Thân nến sau < 60% thân nến trước
            {
                // Bullish Harami (Nến trước giảm, nến sau tăng)
                if (previous.IsBearish && latest.IsBullish)
                {
                    patterns.Add(new CandlestickPattern(
                        "Bullish Harami",
                        "Mẫu hình Bullish Harami/Mẹ bồng con tăng (có thể đảo chiều tăng)",
                        PatternType.Reversal,
                        Direction.Bullish));
                }
                // Bearish Harami (Nến trước tăng, nến sau giảm)
                else if (previous.IsBullish && latest.IsBearish)
                {
                    patterns.Add(new CandlestickPattern(
                        "Bearish Harami",
                        "Mẫu hình Bearish Harami/Mẹ bồng con giảm (có thể đảo chiều giảm)",
                        PatternType.Reversal,
                        Direction.Bearish));
                }
                // Harami Cross (Nến sau là Doji) - Trường hợp đặc biệt
                else if (latestRange > 0.0001m && latestBody / latestRange < DOJI_THRESHOLD) // Check if latest is Doji
                {
                    patterns.Add(new CandlestickPattern(
                                        "Harami Cross",
                                        "Mẫu hình Harami Cross (cho thấy sự lưỡng lự mạnh)",
                                        PatternType.Reversal));
                }
            }
        }

        private static void DetectInsideOutsideBar(StockData latest, StockData previous, List<CandlestickPattern> patterns)
        {
            // Mẫu hình Inside Bar (nến sau nằm hoàn toàn trong phạm vi nến trước)
            if (latest.High < previous.High && latest.Low > previous.Low)
            {
                patterns.Add(new CandlestickPattern(
                    "Inside Bar",
                    "Mẫu hình Inside Bar (thường cho thấy sự tích lũy hoặc củng cố)",
                    PatternType.Continuation));
            }

            // Mẫu hình Outside Bar (nến sau bao phủ hoàn toàn phạm vi nến trước)
            if (latest.High > previous.High && latest.Low < previous.Low)
            {
                if (latest.IsBullish)
                    patterns.Add(new CandlestickPattern(
                        "Bullish Outside Bar",
                        "Mẫu hình Outside Bar tăng (có thể đảo chiều tăng)",
                        PatternType.Reversal,
                        Direction.Bullish));
                else
                    patterns.Add(new CandlestickPattern(
                        "Bearish Outside Bar",
                        "Mẫu hình Outside Bar giảm (có thể đảo chiều giảm)",
                        PatternType.Reversal,
                        Direction.Bearish));
            }
        }

        private static void DetectGapPatterns(StockData latest, StockData previous, List<CandlestickPattern> patterns)
        {
            // Thêm phân tích Gap (khoảng trống)
            if (latest.Low > previous.High)
            {
                patterns.Add(new CandlestickPattern(
                    "Gap Up",
                    "Gap Up (khoảng trống tăng - tín hiệu tăng mạnh)",
                    PatternType.Continuation,
                    Direction.Bullish));
            }
            else if (latest.High < previous.Low)
            {
                patterns.Add(new CandlestickPattern(
                    "Gap Down",
                    "Gap Down (khoảng trống giảm - tín hiệu giảm mạnh)",
                    PatternType.Continuation,
                    Direction.Bearish));
            }
        }

        private static void DetectMatchingPatterns(StockData latest, StockData previous, List<CandlestickPattern> patterns)
        {
            // Matching Low (hỗ trợ tiềm năng)
            if (previous.IsBearish && latest.IsBearish &&
                Math.Abs(previous.Close - latest.Close) / latest.Close < 0.001m)
            {
                patterns.Add(new CandlestickPattern(
                    "Matching Low",
                    "Mẫu hình Matching Low (đáy kép - hỗ trợ tiềm năng)",
                    PatternType.Support));
            }

            // Matching High (kháng cự tiềm năng)
            if (previous.IsBullish && latest.IsBullish &&
                Math.Abs(previous.Close - latest.Close) / latest.Close < 0.001m)
            {
                patterns.Add(new CandlestickPattern(
                    "Matching High",
                    "Mẫu hình Matching High (đỉnh kép - kháng cự tiềm năng)",
                    PatternType.Resistance));
            }
        }

        private static void DetectThreeCandlePatterns(List<StockData> historyData, List<CandlestickPattern> patterns, TrendInfo trend)
        {
            var latest = historyData[^1];
            var previous = historyData[^2];
            var twoDaysAgo = historyData[^3];

            // Phát hiện Morning/Evening Star
            DetectStarPatterns(latest, previous, twoDaysAgo, patterns);

            // Phát hiện Three White Soldiers/Black Crows
            DetectThreeSoldiersOrCrows(latest, previous, twoDaysAgo, patterns);

            // Phát hiện Abandoned Baby
            DetectAbandonedBaby(latest, previous, twoDaysAgo, patterns);

            // Phát hiện Tri-Star Doji
            DetectTriStarDoji(latest, previous, twoDaysAgo, patterns);

            // Phát hiện xu hướng ngắn hạn
            if (trend.IsShortTermUptrend)
            {
                patterns.Add(new CandlestickPattern(
                    "Short-term Uptrend",
                    "Xu hướng tăng ngắn hạn (3 ngày Close tăng liên tiếp)",
                    PatternType.Continuation,
                    Direction.Bullish));
            }
            else if (trend.IsShortTermDowntrend)
            {
                patterns.Add(new CandlestickPattern(
                    "Short-term Downtrend",
                    "Xu hướng giảm ngắn hạn (3 ngày Close giảm liên tiếp)",
                    PatternType.Continuation,
                    Direction.Bearish));
            }

            // Phát hiện Runaway Gap
            DetectRunawayGap(latest, previous, twoDaysAgo, patterns);
        }

        private static void DetectStarPatterns(StockData latest, StockData previous, StockData twoDaysAgo, List<CandlestickPattern> patterns)
        {
            // Morning Star (Sao Mai - Bullish)
            if (twoDaysAgo.IsBearish && // Nến giảm đầu tiên
                previous.BodySize < twoDaysAgo.BodySize * 0.5m && // Nến thứ hai thân nhỏ
                latest.IsBullish && // Nến tăng cuối cùng
                previous.BodyMidPoint < twoDaysAgo.Close && // Nến giữa thấp hơn đóng cửa nến đầu
                latest.Close > twoDaysAgo.BodyMidPoint) // Nến cuối đóng cửa trên giữa thân nến đầu
            {
                patterns.Add(new CandlestickPattern(
                    "Morning Star",
                    "Mẫu hình Morning Star/Sao Mai (đảo chiều tăng)",
                    PatternType.Reversal,
                    Direction.Bullish));

                // Biến thể Morning Doji Star
                if (previous.BodySize / previous.Range < DOJI_THRESHOLD)
                {
                    patterns.Add(new CandlestickPattern(
                        "Morning Doji Star",
                        "Mẫu hình Morning Doji Star/Sao Mai Doji (đảo chiều tăng mạnh)",
                        PatternType.Reversal,
                        Direction.Bullish));
                }
            }

            // Evening Star (Sao Hôm - Bearish)
            if (twoDaysAgo.IsBullish && // Nến tăng đầu tiên
                previous.BodySize < twoDaysAgo.BodySize * 0.5m && // Nến thứ hai thân nhỏ
                latest.IsBearish && // Nến giảm cuối cùng
                previous.BodyMidPoint > twoDaysAgo.Close && // Nến giữa cao hơn đóng cửa nến đầu
                latest.Close < twoDaysAgo.BodyMidPoint) // Nến cuối đóng cửa dưới giữa thân nến đầu
            {
                patterns.Add(new CandlestickPattern(
                    "Evening Star",
                    "Mẫu hình Evening Star/Sao Hôm (đảo chiều giảm)",
                    PatternType.Reversal,
                    Direction.Bearish));

                // Biến thể Evening Doji Star
                if (previous.BodySize / previous.Range < DOJI_THRESHOLD)
                {
                    patterns.Add(new CandlestickPattern(
                        "Evening Doji Star",
                        "Mẫu hình Evening Doji Star/Sao Hôm Doji (đảo chiều giảm mạnh)",
                        PatternType.Reversal,
                        Direction.Bearish));
                }
            }
        }

        private static void DetectThreeSoldiersOrCrows(StockData latest, StockData previous, StockData twoDaysAgo, List<CandlestickPattern> patterns)
        {
            // Three White Soldiers (Ba Chàng Lính Trắng - Bullish)
            if (twoDaysAgo.IsBullish && previous.IsBullish && latest.IsBullish &&
                previous.Close > twoDaysAgo.Close && latest.Close > previous.Close &&
                previous.Open > twoDaysAgo.Open && latest.Open > previous.Open &&
                previous.Open < previous.Close * 0.98m && // Không mở cửa quá cao
                latest.Open < latest.Close * 0.98m) // Không mở cửa quá cao
            {
                patterns.Add(new CandlestickPattern(
                    "Three White Soldiers",
                    "Mẫu hình Three White Soldiers/Ba Chàng Lính Trắng (xu hướng tăng mạnh)",
                    PatternType.Continuation,
                    Direction.Bullish));
            }

            // Three Black Crows (Ba Con Quạ Đen - Bearish)
            if (twoDaysAgo.IsBearish && previous.IsBearish && latest.IsBearish &&
                previous.Close < twoDaysAgo.Close && latest.Close < previous.Close &&
                previous.Open < twoDaysAgo.Open && latest.Open < previous.Open &&
                previous.Open > previous.Close * 1.02m && // Không mở cửa quá thấp
                latest.Open > latest.Close * 1.02m) // Không mở cửa quá thấp
            {
                patterns.Add(new CandlestickPattern(
                    "Three Black Crows",
                    "Mẫu hình Three Black Crows/Ba Con Quạ Đen (xu hướng giảm mạnh)",
                    PatternType.Continuation,
                    Direction.Bearish));
            }
        }

        private static void DetectAbandonedBaby(StockData latest, StockData previous, StockData twoDaysAgo, List<CandlestickPattern> patterns)
        {
            // Kiểm tra nếu nến giữa là Doji
            if (Math.Abs(previous.BodySize / previous.Range) < DOJI_THRESHOLD)
            {
                // Abandoned Baby Bullish
                if (twoDaysAgo.IsBearish && latest.IsBullish &&
                    previous.Low > twoDaysAgo.Close && // Gap down giữa nến 1 và 2
                    previous.Low > latest.Open) // Gap up giữa nến 2 và 3
                {
                    patterns.Add(new CandlestickPattern(
                        "Abandoned Baby Bullish",
                        "Mẫu hình Abandoned Baby Bullish/Em bé bị bỏ rơi tăng (đảo chiều tăng mạnh)",
                        PatternType.Reversal,
                        Direction.Bullish));
                }
                // Abandoned Baby Bearish
                else if (twoDaysAgo.IsBullish && latest.IsBearish &&
                        previous.High < twoDaysAgo.Close && // Gap up giữa nến 1 và 2
                        previous.High < latest.Open) // Gap down giữa nến 2 và 3
                {
                    patterns.Add(new CandlestickPattern(
                        "Abandoned Baby Bearish",
                        "Mẫu hình Abandoned Baby Bearish/Em bé bị bỏ rơi giảm (đảo chiều giảm mạnh)",
                        PatternType.Reversal,
                        Direction.Bearish));
                }
            }
        }

        private static void DetectTriStarDoji(StockData latest, StockData previous, StockData twoDaysAgo, List<CandlestickPattern> patterns)
        {
            bool isDoji1 = twoDaysAgo.Range > 0.0001m && twoDaysAgo.BodySize / twoDaysAgo.Range < DOJI_THRESHOLD;
            bool isDoji2 = previous.Range > 0.0001m && previous.BodySize / previous.Range < DOJI_THRESHOLD;
            bool isDoji3 = latest.Range > 0.0001m && latest.BodySize / latest.Range < DOJI_THRESHOLD;

            if (isDoji1 && isDoji2 && isDoji3)
            {
                patterns.Add(new CandlestickPattern(
                    "Tri-Star Doji",
                    "Mẫu hình Tri-Star Doji (3 Doji liên tiếp - tín hiệu đảo chiều cực mạnh)",
                    PatternType.Reversal));
            }
        }

        private static void DetectRunawayGap(StockData latest, StockData previous, StockData twoDaysAgo, List<CandlestickPattern> patterns)
        {
            bool gapUp1 = previous.Low > twoDaysAgo.High;
            bool gapUp2 = latest.Low > previous.High;
            bool bullish = twoDaysAgo.IsBullish && previous.IsBullish && latest.IsBullish;

            if (gapUp1 && gapUp2 && bullish)
            {
                patterns.Add(new CandlestickPattern(
                    "Bullish Runaway Gap",
                    "Runaway Gap (khoảng trống tăng tiếp diễn xu hướng)",
                    PatternType.Continuation,
                    Direction.Bullish));
            }

            bool gapDown1 = previous.High < twoDaysAgo.Low;
            bool gapDown2 = latest.High < previous.Low;
            bool bearish = twoDaysAgo.IsBearish && previous.IsBearish && latest.IsBearish;

            if (gapDown1 && gapDown2 && bearish)
            {
                patterns.Add(new CandlestickPattern(
                    "Bearish Runaway Gap",
                    "Runaway Gap giảm (khoảng trống giảm tiếp diễn xu hướng)",
                    PatternType.Continuation,
                    Direction.Bearish));
            }
        }

        private static void DetectFiveCandlePatterns(List<StockData> historyData, List<CandlestickPattern> patterns, TrendInfo trend)
        {
            // Phục hồi sau 3 phiên giảm liên tiếp
            DetectRecoveryAfterDecline(historyData, patterns);

            // Phát hiện Rising/Falling Three Methods
            DetectThreeMethods(historyData, patterns);
        }

        private static void DetectRecoveryAfterDecline(List<StockData> historyData, List<CandlestickPattern> patterns)
        {
            if (historyData[^5].Close > historyData[^4].Close &&
                historyData[^4].Close > historyData[^3].Close &&
                historyData[^3].Close > historyData[^2].Close && // 3 ngày giảm trước đó
                historyData[^1].Close > historyData[^2].Close && // Ngày cuối tăng
                historyData[^1].PercentChange > 1) // Mức tăng > 1% để có ý nghĩa
            {
                patterns.Add(new CandlestickPattern(
                    "Recovery After Decline",
                    "Phục hồi sau 3 phiên giảm liên tiếp (Close)",
                    PatternType.Reversal,
                    Direction.Bullish));
            }
        }

        private static void DetectThreeMethods(List<StockData> historyData, List<CandlestickPattern> patterns)
        {
            // Rising Three Methods (5-nến, Bullish continuation)
            if (historyData[^5].IsBullish && historyData[^5].BodySize > 0 &&
                historyData[^1].IsBullish &&
                historyData[^5].Close < historyData[^1].Close &&
                historyData[^4].High < historyData[^5].High &&
                historyData[^3].High < historyData[^5].High &&
                historyData[^2].High < historyData[^5].High)
            {
                patterns.Add(new CandlestickPattern(
                    "Rising Three Methods",
                    "Mẫu hình Rising Three Methods (tiếp tục xu hướng tăng)",
                    PatternType.Continuation,
                    Direction.Bullish));
            }

            // Falling Three Methods (5-nến, Bearish continuation)
            if (historyData[^5].IsBearish && historyData[^5].BodySize > 0 &&
                historyData[^1].IsBearish &&
                historyData[^5].Close > historyData[^1].Close &&
                historyData[^4].Low > historyData[^5].Low &&
                historyData[^3].Low > historyData[^5].Low &&
                historyData[^2].Low > historyData[^5].Low)
            {
                patterns.Add(new CandlestickPattern(
                    "Falling Three Methods",
                    "Mẫu hình Falling Three Methods (tiếp tục xu hướng giảm)",
                    PatternType.Continuation,
                    Direction.Bearish));
            }
        }
    }
}
