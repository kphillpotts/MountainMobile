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
    public partial class OrientationContentPage : ContentPage
    {
        private double width = 0;
        private double height = 0;
        protected Type LandscapeLayoutType;
        protected Type PortraitLayoutType;


        public OrientationContentPage() : base()
        {
            Init();
        }

        private void Init()
        {
            width = this.Width;
            height = this.Height;
            UpdateLayout();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); // have to call the base

            if (this.width != width || this.height != height)
            {
                this.width = width;
                this.height = height;

                UpdateLayout();
            }
        }

        void UpdateLayout()
        {
            if (width > height)
            {
                SetupLandscapeLayout();
            }
            else
            {
                SetupPortraitLayout();
            }
        }

        protected virtual void SetupLandscapeLayout()
        {
            if (LandscapeLayoutType != null)
            {
                Content = Activator.CreateInstance(LandscapeLayoutType) as View;
            }
        }

        protected virtual void SetupPortraitLayout()
        {
            if (PortraitLayoutType != null)
            {
                Content = Activator.CreateInstance(PortraitLayoutType) as View;
            }
        }

    }
}