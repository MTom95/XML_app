using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace XML_app.Models
{
      public class SoldCar : INotifyPropertyChanged
      {
            public string CarBrand { get; set; }
            public string CarModel { get; set; }
            public DateTime DateOfSale { get; set; }
            public double PriceWithTax { get; set; }
            public double Tax { get; set; }

            // Computed property: calculates the price without tax.
            public double PriceWithoutTax => PriceWithTax / (1 + (Tax / 100));

            // Computed property: concatenates CarBrand and CarModel.
            public string FullName => $"{CarBrand} {CarModel}";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public class CarSummary
      {
            public string FullName { get; set; }
            public double PriceWithTax { get; set; }
            public double PriceWithoutTax { get; set; }
      }
}
