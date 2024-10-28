﻿using System.Collections.Generic;
using UCT.Service;
using UnityEngine;

namespace Debug
{
    /// <summary>
    /// 用于Debug时的文本渐变
    /// </summary>
    public class DebugStringGradient
    {
        private readonly Color _defaultColor;
        private Color _targetColor;
        private readonly List<int> _gradientNumberList;
        private const int GradientNumberMax = 120;
        private readonly string _gradientString;
        private readonly List<Color> _colorList, _targetColorList;

        public DebugStringGradient(string text)
        {
            _defaultColor = GameUtilityService.GetRandomColor(); 
            _targetColor = GameUtilityService.GetDifferentRandomColor(_defaultColor);
            _gradientString = text;
            
            _colorList = new List<Color>(text.Length);
            _gradientNumberList = new List<int>(text.Length);
            
            for (var i = 0; i < text.Length; i++)
            {
                _colorList[i] = _defaultColor;
                _targetColorList[i] = _targetColor;
                _gradientNumberList[i] = -i;
            }
        }

        
        public string UpdateStringGradient()
        {
            string result = null;
            for (var i = 0; i < _gradientString.Length; i++)
            {
                _gradientNumberList[i]++;

                if (_gradientNumberList[i] >= GradientNumberMax)
                {
                    _gradientNumberList[i] = 0; // 重置为0
                    if (i == _gradientString.Length - 1)
                        _targetColor = GameUtilityService.GetDifferentRandomColor(_targetColor);
                }

                // 如果当前值 >= 0，进行颜色计算
                if (_gradientNumberList[i] >= 0)
                {
                    // 计算当前的渐变值，假设我们希望渐变到目标颜色
                    var t = (float)_gradientNumberList[i] / GradientNumberMax; // 计算比例

                    // 插值计算
                    _colorList[i] = Color.Lerp(_defaultColor, _targetColorList[i], t);
                }
                
                result += TextProcessingService.StringColor(_colorList[i], _gradientString[i].ToString());
            }
            return result;
        }
    }
}