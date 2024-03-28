using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.DataSource;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;
using TSLab.Script.Optimization;

namespace Samples.MA;

/***
 * Простой пример торговой стратегии по пересечению SMA
 */
public class StrategySimple2Sma : IExternalScript
{
    // таймфрейм
    public readonly IntOptimProperty A0Tf = new(1440, 10, 200, 60);
    
    public readonly IntOptimProperty A1MaSlowPeriod = new(5, 10, 200, 10);
    public readonly IntOptimProperty A2MaFastPeriod = new(5, 10, 200, 10);

    public readonly BoolOptimProperty YfullEquity = new(false); // реинвестировать профит?
    
    public void Execute(IContext ctx, ISecurity sec)
    {
        // Экономлю при оптимизациях
        if(A1MaSlowPeriod.Value == A2MaFastPeriod.Value) return;
        
        // Нужный таймфрейм собираю из минуток
        ISecurity secUpper = sec.CompressTo(new Interval(A0Tf, sec.IntervalBase));
        IList<double> maSlow = secUpper.Decompress(Series.SMA(secUpper.ClosePrices, A1MaSlowPeriod));
        IList<double> maFast = secUpper.Decompress(Series.SMA(secUpper.ClosePrices, A2MaFastPeriod));

        IList<bool> crossFall = new CrossOver().Execute(maSlow, maFast); // сверху вниз
        IList<bool> crossGrow = new CrossUnder().Execute(maSlow, maFast); // снизу вверх
        
        int i = 1;
        for (; i < ctx.BarsCount; i++)
        {
            IPosition pos = sec.Positions.GetLastPositionActive(i);

            if (pos == null)
            {
                if (!crossGrow[i] && !crossFall[i]) continue;
                
                double qty = QtyAvbl(sec, i);
                if (qty <= 0) break; // бюджет кончился
                    
                if (crossGrow[i]) sec.Positions.BuyAtMarket(i + 1, qty, $"b");
                else if (crossFall[i]) sec.Positions.SellAtMarket(i + 1, qty, $"s");

            }
            else if (pos.IsLong && crossFall[i] || pos.IsShort && crossGrow[i])
            {
                pos.CloseAtMarket(i + 1, $"exit");
            }

        }
        
        if(ctx.IsOptimization) return;
        
        ctx.First.AddList("MA Slow", maSlow, ListStyles.LINE, ScriptColors.Green, LineStyles.SOLID, PaneSides.RIGHT);
        ctx.First.AddList("MA Fast", maFast, ListStyles.LINE, ScriptColors.Yellow, LineStyles.SOLID, PaneSides.RIGHT);
    }
    
    /**
     * Расчет доступного к торговле колва Базовой Валюты
     * с учетом возможно реинвестирования ранее полученной прибыли
     */
    public double QtyAvbl(ISecurity source, int barNum, double perc = 100.0)
    {
        // Ситаю остатки с учетом профита пред сделок
        double quote = source.InitDeposit; 
        quote += source.Positions
            .GetClosedOrActiveForBar(barNum)
            .Sum(item => item.IsActiveForBar(barNum) ? item.OpenProfit(barNum) : item.Profit());

        // Если реинвест отключен, то меньшее - всегда Начальный Депозит или его остатки
        if (!YfullEquity) quote = Math.Min(quote, source.InitDeposit);
        
        return quote * (perc / 100) / source.ClosePrices[barNum];
    }
}