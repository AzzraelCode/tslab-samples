using System;
using System.Collections.Generic;
using System.Linq;
using TSLab.DataSource;
using TSLab.Script;
using TSLab.Script.Handlers;
using TSLab.Script.Helpers;
using TSLab.Script.Optimization;

namespace Samples.Other;

public class Sma : IExternalScript
{
    // таймфрейм
    public readonly IntOptimProperty A0Tf = new(1440, 10, 200, 60);
    
    public readonly IntOptimProperty B1 = new(50, 10, 200, 10);
    public readonly IntOptimProperty B2 = new(75, 10, 200, 10);
    public readonly IntOptimProperty B3 = new(100, 10, 200, 10);
    public readonly IntOptimProperty B4 = new(125, 10, 200, 10);
    
    public void Execute(IContext ctx, ISecurity sec)
    {
        // Нужный таймфрейм собираю из минуток
        ISecurity secUpper = sec.CompressTo(new Interval(A0Tf, sec.IntervalBase));
        // для отрисовки и/или отсл пересечения с нулем
        IList<double> zeroes = new List<double>(new double[sec.Bars.Count]);
        
        // вьюхи для отрисовки индикаторов не совпадающих по масштабу с графиком цены
        IGraphPane? pane1 = ctx.CreateGraphPane("Pane1", "Pane1");
        pane1.AddList("0", zeroes, ListStyles.LINE, ScriptColors.Gray, LineStyles.DASH, PaneSides.RIGHT);
        IGraphPane? pane2 = ctx.CreateGraphPane("Pane2", "Pane2");
        pane2.AddList("0", zeroes, ListStyles.LINE, ScriptColors.Gray, LineStyles.DASH, PaneSides.RIGHT);


        
        // 1 Простая Скользящая Средняя
        IList<double> sma1Upper = Series.SMA(secUpper.ClosePrices, B1);
        IList<double> sma1 = secUpper.Decompress(sma1Upper);
        ctx.First.AddList("sma1", sma1, ListStyles.LINE, ScriptColors.Yellow, LineStyles.SOLID, PaneSides.RIGHT);        

        // 2 Экспоненциальная скользящая средняя
        IList<double> sma2Upper = Series.EMA(secUpper.ClosePrices, B2);
        IList<double> sma2 = secUpper.Decompress(sma2Upper);
        ctx.First.AddList("sma2", sma2, ListStyles.LINE, ScriptColors.Blue, LineStyles.SOLID, PaneSides.RIGHT);    

        // 3 Стандартное Отклонение от SMA с периодом заданным вторым аргементом
        IList<double> stDevUpper = Series.StDev(secUpper.ClosePrices, B1);
        IList<double> stDev = secUpper.Decompress(stDevUpper);
        pane1.AddList("stDev", stDev, ListStyles.LINE, ScriptColors.Blue, LineStyles.SOLID, PaneSides.RIGHT);
        
        // 4 Стандартная Историческая Волатильность от SMA с периодом заданным вторым аргементом
        IList<double> volatilityUpper = Series.Volatility(secUpper.ClosePrices, B1);
        IList<double> volatility = secUpper.Decompress(volatilityUpper);
        pane2.AddList("volatility", volatility, ListStyles.LINE, ScriptColors.Yellow, LineStyles.SOLID, PaneSides.RIGHT);
    }
}