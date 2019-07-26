using MountainMobile.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MountainMobile.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LandscapeView : ContentView
    {
        private SlideShowViewModel viewModel;

        Label currentHeading;
        Label currentBody;
        SKBitmap currentBitmap;

        // offscreen stuff
        Label offscreenHeading;
        Label offscreenBody;
        SKBitmap offscreenBitmap;

        // animation values
        const int numberOfBands = 5;
        double[] bandTranslationValues = new double[numberOfBands];
        double[] incomingBandTranslationValues = new double[numberOfBands];
        private bool isAnimating;

        float buttonSize;
        float buttonTop;
        double density;

        public LandscapeView()
        {
            InitializeComponent();
            this.BindingContext = viewModel = ViewModelLocator.SlideshowViewModel;

            density = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
            buttonSize = (float)(30 * density);
            buttonTop = (float)(150 * density) + buttonSize;

            // set initial values
            currentHeading = Heading1;
            currentBody = Body1;
            offscreenHeading = Heading2;
            offscreenBody = Body2;

            // set initial onscreen text
            currentHeading.Text = viewModel.CurrentLocation.Title;
            currentBody.Text = viewModel.CurrentLocation.Description;

        }

        private void CycleElements()
        {
            if (currentHeading == Heading1)
            {
                currentHeading = Heading2;
                currentBody = Body2;
                offscreenHeading = Heading1;
                offscreenBody = Body1;

            }
            else
            {
                currentHeading = Heading1;
                currentBody = Body1;
                offscreenHeading = Heading2;
                offscreenBody = Body2;
            }
        }

        private void SKCanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKRect imageRect = new SKRect(0, 0, info.Width, info.Height);

            if (!isAnimating)
            {
                if (currentBitmap == null)
                    currentBitmap = CreateAspectCorrectedBitmap(viewModel.CurrentLocation.ImageResource, imageRect);

                canvas.DrawBitmap(currentBitmap, imageRect);
                return;
            }
            else
            {
                // work out our hieghts and widths of our reactangles
                var bandHeight = currentBitmap.Height / bandTranslationValues.Length;
                var bandWidth = currentBitmap.Width;

                for (int i = 0; i < bandTranslationValues.Length; i++)
                {
                    var bandyOffset = i * bandHeight;

                    DrawBitmapSliceAtOffset(canvas, currentBitmap,
                        bandyOffset, (float)bandTranslationValues[i], bandWidth, bandHeight);

                    DrawBitmapSliceAtOffset(canvas, offscreenBitmap,
                        bandyOffset, (float)incomingBandTranslationValues[i], bandWidth, bandHeight);
                }
            }
        }

        private void DrawBitmapSliceAtOffset(SKCanvas canvas, SKBitmap bitmap, 
            float bandStartY, float offsetX, float bandWidth, float bandHeight)
        {
            // calculate the source rectangle
            SKRect source = new SKRect(0, bandStartY, bandWidth, bandStartY + bandHeight);

            // calculate the destination (consider the animation value)
            SKRect dest = new SKRect(offsetX, bandStartY, offsetX + bandWidth, bandStartY + bandHeight);

            // draw the bitmap
            canvas.DrawBitmap(bitmap, source, dest);
        }

        private SKBitmap CreateAspectCorrectedBitmap(string resourceName, SKRect destRect)
        {
            // load the bitmap
            var sourceBitmap = BitmapExtensions.LoadBitmapResource(this.GetType(), resourceName);

            // create a bitmap
            SKBitmap aspectFixedBitmap = new SKBitmap
                ((int)destRect.Width, (int)destRect.Height);

            // creaete a canvas for that bitmap
            using (SKCanvas aspectBitmapCanvas = new SKCanvas(aspectFixedBitmap))
            {
                // render the image onto the canvas
                aspectBitmapCanvas.DrawBitmap(sourceBitmap, destRect, BitmapStretch.AspectFill);
            }
            return aspectFixedBitmap;
        }

        enum Direction
        {
            Next,
            Previous
        }

        private void AddChildAnimationForOutgoing(Animation parentAnimation, Direction direction)
        {
            var screenWidthInPixels = ImageSkiaCanvas.CanvasSize.Width;

            // create animations for each band and add to parent
            if (direction == Direction.Next)
            {
                parentAnimation.Add(0.20, 0.70, new Animation(v => bandTranslationValues[0] = v, 0, screenWidthInPixels, Easing.SinInOut));
                parentAnimation.Add(0.15, 0.65, new Animation(v => bandTranslationValues[1] = v, 0, screenWidthInPixels, Easing.SinInOut));
                parentAnimation.Add(0.10, 0.60, new Animation(v => bandTranslationValues[2] = v, 0, screenWidthInPixels, Easing.SinInOut));
                parentAnimation.Add(0.05, 0.55, new Animation(v => bandTranslationValues[3] = v, 0, screenWidthInPixels, Easing.SinInOut));
                parentAnimation.Add(0.00, 0.50, new Animation(v => bandTranslationValues[4] = v, 0, screenWidthInPixels, Easing.SinInOut));
            }
            else
            {
                parentAnimation.Add(0.20, 0.70, new Animation(v => bandTranslationValues[4] = v, 0, -screenWidthInPixels, Easing.SinInOut));
                parentAnimation.Add(0.15, 0.65, new Animation(v => bandTranslationValues[3] = v, 0, -screenWidthInPixels, Easing.SinInOut));
                parentAnimation.Add(0.10, 0.60, new Animation(v => bandTranslationValues[2] = v, 0, -screenWidthInPixels, Easing.SinInOut));
                parentAnimation.Add(0.05, 0.55, new Animation(v => bandTranslationValues[1] = v, 0, -screenWidthInPixels, Easing.SinInOut));
                parentAnimation.Add(0.00, 0.50, new Animation(v => bandTranslationValues[0] = v, 0, -screenWidthInPixels, Easing.SinInOut));
            }

        }

        private void AddChildAnimationsForIncoming(Animation parentAnimation, Direction direction)
        {
            var screenWidthInPixels = ImageSkiaCanvas.CanvasSize.Width;

            if (direction == Direction.Next)
            {
                // create animations for each band and add to parent
                parentAnimation.Add(0.50, 1.00, new Animation(v => incomingBandTranslationValues[0] = v, -screenWidthInPixels, 0, Easing.SinInOut));
                parentAnimation.Add(0.45, 0.95, new Animation(v => incomingBandTranslationValues[1] = v, -screenWidthInPixels, 0, Easing.SinInOut));
                parentAnimation.Add(0.40, 0.90, new Animation(v => incomingBandTranslationValues[2] = v, -screenWidthInPixels, 0, Easing.SinInOut));
                parentAnimation.Add(0.35, 0.85, new Animation(v => incomingBandTranslationValues[3] = v, -screenWidthInPixels, 0, Easing.SinInOut));
                parentAnimation.Add(0.30, 0.80, new Animation(v => incomingBandTranslationValues[4] = v, -screenWidthInPixels, 0, Easing.SinInOut));
            }
            else
            {
                // create animations for each band and add to parent
                parentAnimation.Add(0.50, 1.00, new Animation(v => incomingBandTranslationValues[4] = v, screenWidthInPixels, 0, Easing.SinInOut));
                parentAnimation.Add(0.45, 0.95, new Animation(v => incomingBandTranslationValues[3] = v, screenWidthInPixels, 0, Easing.SinInOut));
                parentAnimation.Add(0.40, 0.90, new Animation(v => incomingBandTranslationValues[2] = v, screenWidthInPixels, 0, Easing.SinInOut));
                parentAnimation.Add(0.35, 0.85, new Animation(v => incomingBandTranslationValues[1] = v, screenWidthInPixels, 0, Easing.SinInOut));
                parentAnimation.Add(0.30, 0.80, new Animation(v => incomingBandTranslationValues[0] = v, screenWidthInPixels, 0, Easing.SinInOut));
            }
        }

        private void AddChildAnimationsForText(Animation parentAnimation, Direction direction)
        {
            // move the current stuff off the screen
            if (direction == Direction.Next)
            {
                parentAnimation.Add(0.00, 0.50, new Animation(v => currentHeading.TranslationX = v, 0, this.Width, Easing.SinIn));
                parentAnimation.Add(0.00, 0.50, new Animation(v => currentHeading.Opacity = v, 1, 0, Easing.SinIn));
                parentAnimation.Add(0.30, 0.70, new Animation(v => offscreenHeading.TranslationX = v, -offscreenHeading.Width, 0, Easing.SinOut));
                parentAnimation.Add(0.30, 0.70, new Animation(v => offscreenHeading.Opacity = v, 0, 1, Easing.SinOut));
            }
            else
            {
                parentAnimation.Add(0.00, 0.50, new Animation(v => currentHeading.TranslationX = v, 0, -offscreenHeading.Width, Easing.SinIn));
                parentAnimation.Add(0.00, 0.50, new Animation(v => currentHeading.Opacity = v, 1, 0, Easing.SinIn));
                parentAnimation.Add(0.30, 0.70, new Animation(v => offscreenHeading.TranslationX = v, this.Width, 0, Easing.SinOut));
                parentAnimation.Add(0.30, 0.70, new Animation(v => offscreenHeading.Opacity = v, 0, 1, Easing.SinOut));
            }

            parentAnimation.Add(0.10, 0.60, new Animation(v => currentBody.TranslationY = v, 0, 30, Easing.SinIn));
            parentAnimation.Add(0.10, 0.60, new Animation(v => currentBody.Opacity = v, 1, 0, Easing.SinIn));
            parentAnimation.Add(0.30, 0.90, new Animation(v => offscreenBody.TranslationY = v, 30, 0, Easing.SinOut));
            parentAnimation.Add(0.30, 0.90, new Animation(v => offscreenBody.Opacity = v, 0, 1, Easing.SinOut));


        }


        private static T[] GetInitializedArray<T>(int length, T initialValue)
        {
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = initialValue;
            }
            return result;
        }

        private void MoveNext()
        {
            if (this.AnimationIsRunning("TransitionAnimation"))
                return;

            var screenWidthPixels = ImageSkiaCanvas.CanvasSize.Width;

            // load up the image we are animating in
            var screenRect = new SKRect(0, 0, ImageSkiaCanvas.CanvasSize.Width, ImageSkiaCanvas.CanvasSize.Height);
            offscreenBitmap = CreateAspectCorrectedBitmap(viewModel.NextLocation.ImageResource, screenRect);
            offscreenHeading.Text = viewModel.NextLocation.Title;
            offscreenBody.Text = viewModel.NextLocation.Description;
            offscreenHeading.Opacity = 0;
            offscreenBody.Opacity = 0;

            // make sure we are setting the starting locations;
            bandTranslationValues = GetInitializedArray<double>(numberOfBands, 0);
            incomingBandTranslationValues = GetInitializedArray<double>(numberOfBands, -screenWidthPixels);


            var parentAnimation = new Animation();

            AddChildAnimationForOutgoing(parentAnimation, Direction.Next);
            AddChildAnimationsForIncoming(parentAnimation, Direction.Next);
            AddChildAnimationsForText(parentAnimation, Direction.Next);

            var skiaAnimation = new Animation(
            callback: v =>
            {
                isAnimating = true;
                ImageSkiaCanvas.InvalidateSurface();
            }, start: 0, end: 1, easing: Easing.Linear);

            parentAnimation.Add(0, 1, skiaAnimation);

            parentAnimation.Commit(this, "TransitionAnimation", 16, 2000, Easing.SinInOut,
            (v, c) =>
            {
                isAnimating = false;
                viewModel.MoveNext();
                CycleElements();
                currentBitmap = offscreenBitmap;
                SideBarCanvas.InvalidateSurface();
            });
        }

        private void MovePrevious()
        {
            if (this.AnimationIsRunning("TransitionAnimation"))
                return;

            var screenWidthPixels = ImageSkiaCanvas.CanvasSize.Width;

            // load up the image we are animating in
            var screenRect = new SKRect(0, 0, ImageSkiaCanvas.CanvasSize.Width, ImageSkiaCanvas.CanvasSize.Height);
            offscreenBitmap = CreateAspectCorrectedBitmap(viewModel.PreviousLocation.ImageResource, screenRect);
            offscreenHeading.Text = viewModel.PreviousLocation.Title;
            offscreenBody.Text = viewModel.PreviousLocation.Description;
            offscreenHeading.Opacity = 0;
            offscreenBody.Opacity = 0;

            // make sure we are setting the starting locations;
            bandTranslationValues = GetInitializedArray<double>(numberOfBands, 0);
            incomingBandTranslationValues = GetInitializedArray<double>(numberOfBands, screenWidthPixels);

            var parentAnimation = new Animation();

            AddChildAnimationForOutgoing(parentAnimation, Direction.Previous);
            AddChildAnimationsForIncoming(parentAnimation, Direction.Previous);
            AddChildAnimationsForText(parentAnimation, Direction.Previous);

            var skiaAnimation = new Animation(
            callback: v =>
            {
                isAnimating = true;
                ImageSkiaCanvas.InvalidateSurface();
            }, start: 0, end: 1, easing: Easing.Linear);

            parentAnimation.Add(0, 1, skiaAnimation);

            parentAnimation.Commit(this, "TransitionAnimation", 16, 2000, Easing.SinInOut,
            (v, c) =>
            {
                isAnimating = false;
                viewModel.MovePrevious();
                CycleElements();
                currentBitmap = offscreenBitmap;
                SideBarCanvas.InvalidateSurface();
            });
        }



        private void SideBarCanvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            // clear our background
            SKRect backgroundRect = new SKRect(0, 0, info.Width - buttonSize, info.Height);

            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipRect(GetPreviousButtonRect(), SKClipOperation.Difference);
                canvas.DrawRect(backgroundRect, new SKPaint() { Color = SKColors.White });
            }

            // draw our button
            var nextButtonRect = GetNextButtonRect();
            canvas.DrawRect(nextButtonRect, new SKPaint() { Color=SKColors.White });

            var previousButtonRect = GetPreviousButtonRect();

            var arrowSize = 7 * (float)density;
            // draw our arrows - next button
            SKPath nextArrowPath = new SKPath();
            nextArrowPath.MoveTo(nextButtonRect.MidX - (arrowSize / 2), nextButtonRect.MidY - (arrowSize / 2));
            nextArrowPath.LineTo(nextButtonRect.MidX + (arrowSize / 2), nextButtonRect.MidY);
            nextArrowPath.LineTo(nextButtonRect.MidX - (arrowSize / 2), nextButtonRect.MidY + (arrowSize / 2));
            nextArrowPath.LineTo(nextButtonRect.MidX - (arrowSize / 2), nextButtonRect.MidY - (arrowSize / 2));
            nextArrowPath.Close();
            canvas.DrawPath(nextArrowPath, new SKPaint() { Color = SKColors.Black });

            // draw our arrows - previous button
            SKPath previousArrowPath = new SKPath();
            previousArrowPath.MoveTo(previousButtonRect.MidX + (arrowSize / 2), previousButtonRect.MidY - (arrowSize / 2));
            previousArrowPath.LineTo(previousButtonRect.MidX - (arrowSize / 2), previousButtonRect.MidY);
            previousArrowPath.LineTo(previousButtonRect.MidX + (arrowSize / 2), previousButtonRect.MidY + (arrowSize / 2));
            previousArrowPath.LineTo(previousButtonRect.MidX + (arrowSize / 2), previousButtonRect.MidY - (arrowSize / 2));
            previousArrowPath.Close();
            canvas.DrawPath(previousArrowPath, new SKPaint() { Color = SKColors.White });

            DrawPageIndicators(canvas, backgroundRect);
        }

        private void DrawPageIndicators(SKCanvas canvas, SKRect sideBar)
        {
            const float indicatorSize = 10;
            const float padding = 8;

            // where should they be in the Y Axis
            var indicatorY = sideBar.Height - ((indicatorSize * 2) + padding);
            var xStart = (sideBar.Width - (2*padding)) - ((indicatorSize + padding) * viewModel.LocationPages.Count());

            for (int i = 0; i < viewModel.LocationPages.Count(); i++)
            {
                var indicatorX = xStart + ((indicatorSize + padding) * i);

                var indicatorRect = new SKRect(indicatorX, indicatorY, indicatorX + indicatorSize, indicatorY + indicatorSize);

                // is this the current page
                SKPaintStyle style = viewModel.LocationPages.IndexOf(viewModel.CurrentLocation) == i ? SKPaintStyle.StrokeAndFill : SKPaintStyle.Stroke;
                canvas.DrawRect(indicatorRect, new SKPaint() { Color = SKColors.Black, Style = style, StrokeWidth=2 });
            }


        }

        private SKRect GetPreviousButtonRect()
        {
            SKRect returnRect = new SKRect(SideBarCanvas.CanvasSize.Width - (2*buttonSize), buttonTop,
                SideBarCanvas.CanvasSize.Width - buttonSize, buttonTop + buttonSize);

            return returnRect;
        }

        private SKRect GetNextButtonRect()
        {
            SKRect returnRect = new SKRect(SideBarCanvas.CanvasSize.Width - buttonSize, buttonTop,
                SideBarCanvas.CanvasSize.Width, buttonTop + buttonSize);

            return returnRect;
        }

        private void SideBarCanvas_Touch(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
        {

            if (GetPreviousButtonRect().Contains(e.Location))
            {
                MovePrevious();
            }

            if (GetNextButtonRect().Contains(e.Location))
            {
                MoveNext();
            }

        }
    }
}