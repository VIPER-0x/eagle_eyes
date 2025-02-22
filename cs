using System;  
using cAlgo.API;  
using cAlgo.API.Internals;  

namespace Samples  
{  
    public static class SymbolExtensions  
    {  
        /// <summary>  
        /// Normalizes the pips value to the symbol's pip size.  
        /// </summary>  
        /// <param name="symbol">The symbol</param>  
        /// <param name="pips">The pips value</param>  
        /// <returns>Normalized pips value</returns>  
        public static double NormalizePips(this Symbol symbol, double pips)  
        {  
            return pips * symbol.PipSize;  
        }  
    }  

    [Robot(AccessRights = AccessRights.None)]  
    public class Sample : Robot  
    {  
        [Parameter("Risk/Reward Ratio", DefaultValue = 3.0)]  
        public double RiskRewardRatio { get; set; }  

        protected override void OnStart()  
        {  
            // Start the timer with a 2-second interval  
            Timer.Start(TimeSpan.FromSeconds(10));  
            Print("Robot started. Timer initialized to check positions every 10 seconds.");  
        }  

        protected override void OnTimer()  
        {  
            // Check open positions every 10 seconds  
            CheckAndModifyPositions();  
        }  

        private void CheckAndModifyPositions()  
        {  
            Print("Checking open positions...");  

            foreach (var position in Positions)  
            {  
                if (position.SymbolName == Symbol.Name)  
                {  
                    Print($"Found position: ID={position.Id}, TradeType={position.TradeType}, EntryPrice={position.EntryPrice}");  

                    // Check if the position already has a stop loss  
                    if (position.StopLoss == null)  
                    {  
                        Print("Position does not have a stop loss. Calculating and applying stop loss and take profit...");  

                        var stopLossPrice = CalculateStopLossPrice(position);  
                        var takeProfitPrice = CalculateTakeProfitPrice(position, stopLossPrice);  

                        Print($"Calculated Stop Loss: {stopLossPrice}, Take Profit: {takeProfitPrice}");  

                        var result = ModifyPosition(position, stopLossPrice, takeProfitPrice, ProtectionType.Absolute);  

                        if (result.IsSuccessful)  
                        {  
                            Print("Position modified successfully.");  
                        }  
                        else  
                        {  
                            Print($"Failed to modify position. Error: {result.Error}");  
                        }  
                    }  
                    else  
                    {  
                        Print("Position already has a stop loss. No action taken.");  
                    }  
                }  
            }  
        }  

        private double CalculateStopLossPrice(Position position)  
        {  
            // Replace 60 with 1/24 * close price (last bar's close price)  
            var closePrice = Bars.LastBar.Close;  
            var stopLossInPrice = position.TradeType == TradeType.Buy  
                ? Symbol.Bid - (closePrice / 1024)  
                : Symbol.Ask + (closePrice /1024);  

            Print($"Calculated Stop Loss Price: {stopLossInPrice}");  
            return stopLossInPrice;  
        }  

        private double CalculateTakeProfitPrice(Position position, double stopLossPrice)  
        {  
            var stopLossDistance = Math.Abs(position.EntryPrice - stopLossPrice);  
            var takeProfitDistance = stopLossDistance * RiskRewardRatio;  

            var takeProfitPrice = position.TradeType == TradeType.Buy  
                ? position.EntryPrice + takeProfitDistance  
                : position.EntryPrice - takeProfitDistance;  

            Print($"Calculated Take Profit Price: {takeProfitPrice}");  
            return takeProfitPrice;  
        }  
    }  
}
