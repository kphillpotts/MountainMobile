using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MountainMobile
{
    public static class Location
    {
        public static ObservableCollection<LocationModel> LocationPages { get; set; }

        static Location()
        {
            LocationPages = new ObservableCollection<LocationModel>()
            {
                new LocationModel() {
                    Title ="Thórsmörk",
                    Description ="Thórsmörk is a mountain ridge in Iceland that was named after the Norse god Thor (Þór). It is situated in the south of Iceland between the glaciers Tindfjallajökull and Eyjafjallajökull.",
                    ImageResource="MountainMobile.Images.Thorsmork.jpg"},

                 new LocationModel() {
                    Title ="Öræfajökull",
                    Description ="Öræfajökull is located at the southern extremity of the Vatnajökull glacier and overlooking the Ring Road between Höfn and Vík.",
                    ImageResource="MountainMobile.Images.Oraefojokull.jpg"},

                 new LocationModel() {
                    Title ="Bárðarbunga",
                    Description ="Bárðarbunga is a subglacial stratovolcano located under the ice cap of Vatnajökull glacier within the Vatnajökull National Park in Iceland. It rises to 2,009 metres above sea level",
                    ImageResource="MountainMobile.Images.bardarbunga.jpg"},
            };
        }
    }


    public class LocationModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageResource { get; set; }
    }
}
