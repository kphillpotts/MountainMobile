using MountainMobile.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
        private SKBitmap currentBitmap;
        private SKBitmap currentBitmapAspectCorrected;

        public LandscapeView()
        {
            InitializeComponent();
            this.BindingContext = viewModel = ViewModelLocator.SlideshowViewModel;
            currentBitmap = BitmapExtensions.LoadBitmapResource(this.GetType(), viewModel.CurrentLocation.ImageResource);
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
                currentBitmapAspectCorrected = CreateAspectCorrectedBitmap(currentBitmap, imageRect);

                canvas.DrawBitmap(currentBitmapAspectCorrected, imageRect);
                return;
            }
            else
            {
                // work out our hieghts and widths of our reactangles
                var bandHeight = currentBitmapAspectCorrected.Height / bandTranslationValues.Length;
                var bandWidth = currentBitmapAspectCorrected.Width;

                for (int i = 0; i < bandTranslationValues.Length; i++)
                {
                    var bandyOffset = i * bandHeight;
                    var bandxOffset = (float)bandTranslationValues[i];

                    // calculate the source rectangle
                    SKRect source = new SKRect(0, bandyOffset, bandWidth, bandyOffset + bandHeight);
                    
                    // calculate the destination (consider the animation value)
                    SKRect dest = new SKRect(bandxOffset, bandyOffset, bandxOffset + bandWidth, bandyOffset + bandHeight);

                    // draw the bitmap
                    canvas.DrawBitmap(currentBitmapAspectCorrected, source, dest);

                }


            }
        }

        private SKBitmap CreateAspectCorrectedBitmap(SKBitmap sourceBitmap, SKRect destRect)
        {
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

        const int numberOfBands = 5;
        double[] bandTranslationValues = new double[numberOfBands];
        private bool isAnimating;

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (this.AnimationIsRunning("TransitionAnimation"))
                return;

            var screenWidthPixels = ImageSkiaCanvas.CanvasSize.Width;

            // make sure we are all starting at 0;
            bandTranslationValues = new double[numberOfBands];
            
            var parentAnimation = new Animation();

            // create animations for each band and add to parent
            parentAnimation.Add(.2, .95, new Animation(v => bandTranslationValues[0] = v, 0, screenWidthPixels, Easing.SinInOut));
            parentAnimation.Add(.15, .90, new Animation(v => bandTranslationValues[1] = v, 0, screenWidthPixels + 300, Easing.SinInOut));
            parentAnimation.Add(.1, .85, new Animation(v => bandTranslationValues[2] = v, 0, screenWidthPixels + 400, Easing.SinInOut));
            parentAnimation.Add(.05, .8, new Animation(v => bandTranslationValues[3] = v, 0, screenWidthPixels + 500, Easing.SinInOut));
            parentAnimation.Add(0, .75, new Animation(v => bandTranslationValues[4] = v, 0, screenWidthPixels + 600, Easing.SinInOut));

            var skiaAnimation = new Animation(
            callback: v =>
            {
                isAnimating = true;
                ImageSkiaCanvas.InvalidateSurface();
            }, start: 0, end: screenWidthPixels, easing: Easing.SinInOut);

            parentAnimation.Add(0, 1, skiaAnimation);

            parentAnimation.Commit(this, "TransitionAnimation", 16, 2000, Easing.SinInOut,
            (v, c) =>
            {
                isAnimating = false;
            });

        }
    }
}