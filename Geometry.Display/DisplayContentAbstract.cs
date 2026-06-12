using System.Drawing;
using Rhino.Geometry;
using Woodpecker.Animation.CodeManager;
using System;
using System.Collections.Generic;

namespace Woodpecker.Animation.Geometry.Display
{
    public abstract class DisplayContentAbstract<T> : IDisplayContent
    {
        public Dictionary<string, object> Attributes;
        public DisplayContentAbstract(T content, Color color)
        {
            this.DisplayObject = content;
            this.m_Colour = color;
            this.Attributes = new Dictionary<string,object>();
        }
        public T DisplayObject { get; }
        public Color DisplayColour => GetColor();
        public abstract BoundingBox ClippingBox {get;}
        public abstract bool IsValid {get;}
        /// <summary>
        /// The original colour
        /// </summary>
        protected readonly Color m_Colour;
        public bool Visible {get; set;} = true;
        private double _t;
        public double t
        {
            get { return _t; }
            set
            {
                _t = Math.Min(1, Math.Max(0, value));
            }
        }
        public double transparency => 1 - (m_Colour.A / 255.0) * _t;
        protected virtual Color GetColor()
        {
            var _transparency = 1 - (m_Colour.A / 255.0) * _t;
            return Color.FromArgb((int)Math.Round(m_Colour.A * t), m_Colour.R, m_Colour.G, m_Colour.B);
        }
        public void SetT(double t)
        {
            this.t = t;
        }
    }
}