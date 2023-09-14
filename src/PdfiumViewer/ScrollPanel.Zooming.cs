using PdfiumViewer.Enums;
using System;
using System.ComponentModel;

namespace PdfiumViewer
{
    public partial class ScrollPanel
    {
        public const double DefaultZoomMin = 0.1;
        public const double DefaultZoomMax = 5;
        public const double DefaultZoomFactor = 1.2;

        /// <summary>
        /// Zoom allowed.
        /// </summary>
        [DefaultValue(1.0)]
        public bool IsZoomAllowed { get; set; } = true;

        /// <summary>
        /// Zoom mode (FitHeight, FitWidth or None)
        /// </summary>
        public PdfViewerZoomMode ZoomMode { get; protected set; } = PdfViewerZoomMode.FitHeight;

        /// <summary>
        /// Current zoom level.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(1.0)]
        public double Zoom { get; set; } = 1.0;

        /// <summary>
        /// Maximum zoom level.
        /// </summary>
        [DefaultValue(DefaultZoomMin)] public double ZoomMin { get; set; } = DefaultZoomMin;

        /// <summary>
        /// Minimum zoom level.
        /// </summary>
        [DefaultValue(DefaultZoomMax)] public double ZoomMax { get; set; } = DefaultZoomMax;

        /// <summary>
        /// Zoom step.
        /// </summary>
        [DefaultValue(DefaultZoomFactor)] public double ZoomFactor { get; set; } = DefaultZoomFactor;

        /// <summary>
        /// Zooms the PDF document in one step.
        /// </summary>
        public void ZoomIn()
        {
            SetZoom(Zoom * ZoomFactor);
        }

        /// <summary>
        /// Zooms the PDF document out one step.
        /// </summary>
        public void ZoomOut()
        {
            SetZoom(Zoom / ZoomFactor);
        }

        public void SetZoom(double zoom)
        {
            if (IsZoomAllowed)
            {
                var newZoom = Math.Min(Math.Max(zoom, ZoomMin), ZoomMax);
                if (newZoom != Zoom)
                {
                    Zoom = newZoom;
                    ZoomMode = PdfViewerZoomMode.None;
                    OnPagesDisplayModeChanged();
                }
            }
        }

        public void SetZoomMode(PdfViewerZoomMode mode)
        {
            if (ZoomMode != mode)
            {
                ZoomMode = mode;
                OnPagesDisplayModeChanged();
            }
        }
    }
}
