﻿using System;
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

        
        // *** Простая Скользящая Средняя
        // IList<double> sma1Upper = Series.SMA(secUpper.ClosePrices, B1);
        // ctx.First.AddList("sma1", secUpper.Decompress(sma1Upper), ListStyles.LINE, ScriptColors.Yellow, LineStyles.SOLID, PaneSides.RIGHT);        

        // *** Экспоненциальная скользящая средняя
        // IList<double> sma2Upper = Series.EMA(secUpper.ClosePrices, B2);
        // ctx.First.AddList("sma2", secUpper.Decompress(sma2Upper), ListStyles.LINE, ScriptColors.Blue, LineStyles.SOLID, PaneSides.RIGHT);    

        // *** Стандартное Отклонение от SMA с периодом заданным вторым аргементом
        // IList<double> stDevUpper = Series.StDev(secUpper.ClosePrices, B1);
        // pane1.AddList("stDev", secUpper.Decompress(stDevUpper), ListStyles.LINE, ScriptColors.Blue, LineStyles.SOLID, PaneSides.RIGHT);
        
        // *** Стандартная Историческая Волатильность от SMA с периодом заданным вторым аргементом
        // IList<double> volatilityUpper = Series.Volatility(secUpper.ClosePrices, B1);
        // pane1.AddList("volatility", secUpper.Decompress(volatilityUpper), ListStyles.LINE, ScriptColors.Yellow, LineStyles.SOLID, PaneSides.RIGHT);
        
        // *** Полосы Болинжера. Можно получить верхнюю и нижнюю, медиана = Series.SMA(secUpper.ClosePrices, B1)
        // Volatility Indicator (ta.volatility)
        // https://school.stockcharts.com/doku.php?id=technical_indicators:bollinger_bands
        // IList<double> bbTopUpper = Series.BollingerBands(secUpper.ClosePrices, B1, B4, true);
        // ctx.First.AddList("BB Top Line", secUpper.Decompress(bbTopUpper), ListStyles.LINE, ScriptColors.Green, LineStyles.SOLID, PaneSides.RIGHT); 
        // IList<double> bbBotUpper = Series.BollingerBands(secUpper.ClosePrices, B1, B4, false);
        // ctx.First.AddList("BB Bot Line", secUpper.Decompress(bbBotUpper), ListStyles.LINE, ScriptColors.Red, LineStyles.SOLID, PaneSides.RIGHT); 

        // *** Average True Range
        // Volatility Indicator (ta.volatility)
        // http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:average_true_range_atr
        // IList<double>? atrUpper = Series.AverageTrueRange(secUpper.Bars, B1);
        // pane1.AddList("atr", secUpper.Decompress(atrUpper), ListStyles.LINE, ScriptColors.Red, LineStyles.SOLID, PaneSides.RIGHT);
        
        // *** RSI (Relative Strength Index)
        // Momentum Indicator (ta.momentum)
        // https://www.investopedia.com/terms/r/rsi.asp
        IList<double>? rsiUpper = Series.RSI(secUpper.ClosePrices, B1);
        pane1.AddList("rsi", secUpper.Decompress(rsiUpper), ListStyles.LINE, ScriptColors.Red, LineStyles.SOLID, PaneSides.RIGHT);
        
    }
}