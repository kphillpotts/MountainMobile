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
    public partial class PortraitView : ContentView
    {
        // currently displayed stuff
        Label currentHeading;
        Label currentBody;
        SKBitmap currentBitmap;

        // offscreen stuff
        Label offscreenHeading;
        Label offscreenBody;
        SKBitmap offscreenBitmap;

        SlideShowViewModel viewModel;

        public PortraitView()
        {
            InitializeComponent();

            this.BindingContext = viewModel = ViewModelLocator.SlideshowViewModel;

            currentHeading = Heading1;
            currentBody = Body1;
            currentBitmap = BitmapExtensions.LoadBitmapResource(this.GetType(), viewModel.CurrentLocation.ImageResource);

            offscreenHeading = Heading2;
            offscreenBody = Body2;

            // set intial values
            offscreenHeading.Text = viewModel.CurrentLocation.Title;
            offscreenBody.Text = viewModel.CurrentLocation.Description;

        }

        private void UpdateOffScreenElements()
        {
            offscreenHeading.Text = viewModel.NextLocation.Title;
            offscreenBody.Text = viewModel.NextLocation.Description;
            offscreenBitmap = BitmapExtensions.LoadBitmapResource(this.GetType(), viewModel.NextLocation.ImageResource);
        }


        private void ImageSkiaCanvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKRect imageRect = new SKRect(0, 0, info.Width, info.Height);

            // render out the bitmap
            SKRect outgoingImageRect;
            if (transitionValue >= 1)
                outgoingImageRect = new SKRect(0 - (float)outgoingOffset, 0, info.Width, info.Height);
            else
                outgoingImageRect = new SKRect(0, 0, info.Width, info.Height);

            canvas.DrawBitmap(currentBitmap, outgoingImageRect, BitmapStretch.AspectFill);

            if (transitionValue <= 0) return;

            // draw our clipping path
            if (offscreenBitmap != null)
            {
                // animate int the rect that the image is being rendered to
                int movementAmount = 600;
                float offset = (float)(movementAmount - (movementAmount * (transitionValue / 2)));
                SKRect incomingRect = new SKRect(0, 0, info.Width + offset, info.Height);

                // draw the faded version of the image
                using (SKPaint transparentPaint = new SKPaint())
                {
                    var opacity = Math.Max((transitionValue - .5) * .5, 0);
                    transparentPaint.Color = transparentPaint.Color.WithAlpha((byte)(0xFF * opacity));
                    canvas.DrawBitmap(
                        bitmap: offscreenBitmap,
                        dest: incomingRect,
                        stretch: BitmapStretch.AspectFill,
                        paint: transparentPaint);
                }

                var clipPath = CalculateClipPath(info, transitionValue);
                canvas.ClipPath(clipPath, SKClipOperation.Intersect);
                canvas.DrawBitmap(offscreenBitmap, incomingRect, BitmapStretch.AspectFill);
            }

        }

        private SKPath CalculateClipPath(SKImageInfo info, double transitionValue)
        {
            // calculate offset
            var xDelta = transitionValue > 1 ? info.Width : info.Width * transitionValue;
            var yDelta = transitionValue < 1 ? 0 : (info.Height / 2) * (transitionValue - 1);
            var xPos = info.Width - xDelta;
            var yPos1 = (info.Height / 2) - yDelta;
            var yPos2 = (info.Height / 2) + yDelta;

            // construct our path
            SKPath path = new SKPath();
            path.MoveTo(info.Width, 0);
            path.LineTo((float)xPos, (float)yPos1);
            path.LineTo((float)xPos, (float)yPos2);
            path.LineTo(info.Width, info.Height);
            return path;
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

        double transitionValue = 0;
        private double outgoingOffset;

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            // see if the animation is running
            if (this.AnimationIsRunning("TransitionAnimation"))
                return;

            // update the elements
            UpdateOffScreenElements();

            // move the current stuff off the screen
            var onscreenHeadingSlideOut = new Animation(v => currentHeading.TranslationX = v, 0, -this.Width, Easing.SinIn);
            var onscreenHeadingFade = new Animation(v => currentHeading.Opacity = v, 1, 0, Easing.SinIn);
            var onscreenBodySlideOut = new Animation(v => currentBody.TranslationX = v, 0, -this.Width, Easing.SinIn);
            var onscreenBodyFade = new Animation(v => currentBody.Opacity = v, 1, 0, Easing.SinIn);

            // move the offscreen stuff onto the screen
            var offscreenHeadingSlideIn = new Animation(v => offscreenHeading.TranslationX = v, this.Width, 0, Easing.SinOut);
            var offscreenHeadingFadeIn = new Animation(v => offscreenHeading.Opacity = v, 0, 1, Easing.SinOut);
            var offscreenBodySlideIn = new Animation(v => offscreenBody.TranslationX = v, this.Width, 0, Easing.SinOut);
            var offscreenBodyFade = new Animation(v => offscreenBody.Opacity = v, 0, 1, Easing.SinIn);

            // animation for skia elements
            var skiaAnimation = new Animation(
                callback: v =>
                {
                    transitionValue = v;
                    ImageSkiaCanvas.InvalidateSurface();
                }, start: 0, end: 2, easing: Easing.SinInOut);

            var outgoingImageAnimation = new Animation(
                callback: v =>
                {
                    outgoingOffset = v;

                }, start: 0, end: this.Width, easing: Easing.CubicInOut);

            var parentAnimation = new Animation();

            // outgoing child animations
            parentAnimation.Add(0, 1, onscreenHeadingSlideOut);
            parentAnimation.Add(0, .5, onscreenHeadingFade);
            parentAnimation.Add(.2, 1, onscreenBodySlideOut);
            parentAnimation.Add(0, 1, onscreenBodyFade);

            // inbound child animations
            parentAnimation.Add(.2, 1, offscreenHeadingSlideIn);
            parentAnimation.Add(.2, 1, offscreenHeadingFadeIn);
            parentAnimation.Add(.4, 1, offscreenBodySlideIn);
            parentAnimation.Add(.4, 1, offscreenBodyFade);

            // add skia animations
            parentAnimation.Add(0, 1, skiaAnimation);
            parentAnimation.Add(.5, 1, outgoingImageAnimation);

            parentAnimation.Commit(this, "TransitionAnimation", 16, 750, null,
                (v, c) =>
                {
                    viewModel.MoveNext();
                    CycleElements();
                    currentBitmap = offscreenBitmap;
                });
        }

    }
}