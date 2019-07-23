using System;
using System.Collections.Generic;
using System.Text;

namespace MountainMobile.ViewModels
{
    public static class ViewModelLocator
    {
        static SlideShowViewModel slideshowVM;

        public static SlideShowViewModel SlideshowViewModel =>
            slideshowVM ?? (slideshowVM = new SlideShowViewModel());
    }
}
