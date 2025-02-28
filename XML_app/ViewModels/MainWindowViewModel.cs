using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using XML_app.Models;

namespace XML_app.ViewModels
{
      public enum SortMode { Original, Descending, Ascending }

      public class MainWindowViewModel : INotifyPropertyChanged
      {
            // The collection displayed by the ListView.
            private ObservableCollection<object> _items;
            public ObservableCollection<object> Items
            {
                  get { return _items; }
                  set { _items = value; OnPropertyChanged(nameof(Items)); }
            }

            // The loaded original data.
            public ObservableCollection<SoldCar> LoadedXML { get; set; } = new ObservableCollection<SoldCar>();

            private bool _isGrouped;
            public bool IsGrouped
            {
                  get { return _isGrouped; }
                  set
                  {
                        if (_isGrouped != value)
                        {
                              _isGrouped = value;
                              OnPropertyChanged(nameof(IsGrouped));
                              OnPropertyChanged(nameof(ToggleButtonContent)); // notify that ToggleButtonContent has changed
                              ApplyGroupingAndSorting();
                        }
                  }
            }

            // New property for the ToggleButton text.
            public string ToggleButtonContent
            {
                  get { return IsGrouped ? "Zobraz všechny\nprodeje" : "Zobraz prodeje\ndle modelu"; }
            }
            private SortMode _globalSortMode = SortMode.Original;
            public SortMode GlobalSortMode
            {
                  get { return _globalSortMode; }
                  set { _globalSortMode = value; OnPropertyChanged(nameof(GlobalSortMode)); ApplyGroupingAndSorting(); }
            }

            // Property used by the sort button's Content.
            // Could maybe sort by price on the grouped view as well...
            public string SortButtonContent
            {
                  get
                  {
                        string sortBy = IsGrouped ? "Model" : "Cena";
                        string czPreklad = "";
                        string arrow = "";
                        switch (GlobalSortMode)
                        {     //Ideally I'd use an arrow image, because a character in a string looks odd to scale.
                              case SortMode.Ascending:
                                    czPreklad = "Vzestupně";
                                    arrow = " ↑";
                                    break;
                              case SortMode.Descending:
                                    czPreklad = "Sestupně";
                                    arrow = " ↓";
                                    break;
                              default:
                                    // When Original, no translation or arrow is shown.
                                    czPreklad = "";
                                    arrow = "";
                                    break;
                        }
                        return $"Seřadit: {czPreklad} ({sortBy}){arrow}";
                  }
            }

            // Commands
            public ICommand GenerateXmlCommand { get; }
            public ICommand LoadXmlCommand { get; }
            public ICommand SortCommand { get; }
            public ICommand ToggleGroupCommand { get; }

            public MainWindowViewModel()
            {
                  GenerateXmlCommand = new RelayCommand(_ => GenerateXml());
                  LoadXmlCommand = new RelayCommand(_ => LoadXml());
                  SortCommand = new RelayCommand(_ => CycleSortMode());
                  ToggleGroupCommand = new RelayCommand(_ => ToggleGroup());
                  Items = new ObservableCollection<object>();
            }

            private void GenerateXml()
            {
                  // Create sample data.
                  var cars = new[]
                  {
                      new SoldCar { CarModel = "Oktávia", DateOfSale = DateTime.ParseExact("02-12-2010", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 500000, Tax = 20 },
                      new SoldCar { CarModel = "Felicia", DateOfSale = DateTime.ParseExact("03-12-2010", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 210000, Tax = 20 },
                      new SoldCar { CarModel = "Fábia", DateOfSale = DateTime.ParseExact("04-12-2010", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 350000, Tax = 20 },
                      new SoldCar { CarModel = "Oktávia", DateOfSale = DateTime.ParseExact("04-12-2010", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 500000, Tax = 20 },
                      new SoldCar { CarModel = "Oktávia", DateOfSale = DateTime.ParseExact("05-12-2010", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 500000, Tax = 20 },
                      new SoldCar { CarModel = "Fábia", DateOfSale = DateTime.ParseExact("05-12-2010", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 350000, Tax = 20 },
                      new SoldCar { CarModel = "Fábia", DateOfSale = DateTime.ParseExact("06-12-2010", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 350000, Tax = 20 },
                      new SoldCar { CarModel = "Forman", DateOfSale = DateTime.ParseExact("04-12-2000", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 100000, Tax = 19 },
                      new SoldCar { CarModel = "Favorit", DateOfSale = DateTime.ParseExact("02-12-2000", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 80000, Tax = 19 },
                      new SoldCar { CarModel = "Forman", DateOfSale = DateTime.ParseExact("02-12-2000", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 100000, Tax = 19 },
                      new SoldCar { CarModel = "Felicia", DateOfSale = DateTime.ParseExact("02-12-2000", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 210000, Tax = 19 },
                      new SoldCar { CarModel = "Felicia", DateOfSale = DateTime.ParseExact("02-12-2000", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 210000, Tax = 19 },
                      new SoldCar { CarModel = "Oktávia", DateOfSale = DateTime.ParseExact("02-12-2010", "dd-MM-yyyy", CultureInfo.InvariantCulture), PriceWithTax = 500000, Tax = 20 },
                  };
                  // Set CarBrand for each car.
                  foreach (var car in cars)
                  {
                        car.CarBrand = "Škoda";
                  }

                  LoadedXML = new ObservableCollection<SoldCar>(cars);

                  // Build the XML structure using LINQ to XML.
                  XElement xmlRoot = new XElement("soldCar",
                      from car in cars
                      select new XElement("Car",
                          new XAttribute("CarBrand", car.CarBrand),
                          new XAttribute("CarModel", car.CarModel),
                          new XAttribute("FullName", car.FullName),
                          new XAttribute("DateOfSale", car.DateOfSale.ToString("dd-MM-yyyy")),
                          new XAttribute("PriceWithTax", car.PriceWithTax),
                          new XAttribute("PriceWithoutTax", car.PriceWithoutTax),
                          new XAttribute("Tax", car.Tax)
                      ));
                  string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prodane_vozy_prosinec.xml");
                  xmlRoot.Save(filePath);
                  MessageBox.Show($"Your XML file has been created at {filePath}.");
                  ApplyGroupingAndSorting();
            }

            private void LoadXml()
            {
                  //Choose a XML or any file.
                  OpenFileDialog dlg = new OpenFileDialog
                  {
                        Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                        Title = "Select an XML file"
                  };

                  if (dlg.ShowDialog() == true)
                  {
                        XElement xml = XElement.Load(dlg.FileName);
                        var cars = xml.Elements("Car")
                            .Select(x => new SoldCar
                            {
                                  CarBrand = (string)x.Attribute("CarBrand"),
                                  CarModel = (string)x.Attribute("CarModel"),
                                  DateOfSale = DateTime.ParseExact((string)x.Attribute("DateOfSale"), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                                  PriceWithTax = (double)x.Attribute("PriceWithTax"),
                                  Tax = (double)x.Attribute("Tax")
                            }).ToList();
                        LoadedXML = new ObservableCollection<SoldCar>(cars);
                        ApplyGroupingAndSorting();
                  }
            }

            private void CycleSortMode()
            {
                  GlobalSortMode = (SortMode)(((int)GlobalSortMode + 1) % 3);
            }

            private void ToggleGroup()
            {
                  IsGrouped = !IsGrouped;
            }

            private void ApplyGroupingAndSorting()
            {
                  if (LoadedXML == null) return;

                  if (IsGrouped)
                  {
                        // Group by FullName and sum PriceWithTax and PriceWithoutTax.
                        var grouped = LoadedXML
                            .GroupBy(car => car.FullName)
                            .Select(g => new CarSummary
                            {
                                  FullName = g.Key,
                                  PriceWithTax = g.Sum(c => c.PriceWithTax),
                                  PriceWithoutTax = g.Sum(c => c.PriceWithoutTax)
                            });
                        // Sort grouped data by FullName.
                        if (GlobalSortMode == SortMode.Ascending)
                              grouped = grouped.OrderBy(x => x.FullName);
                        else if (GlobalSortMode == SortMode.Descending)
                              grouped = grouped.OrderByDescending(x => x.FullName);
                        Items = new ObservableCollection<object>(grouped);
                  }
                  else
                  {
                        // Sort the original LoadedXML by PriceWithTax.
                        IEnumerable<SoldCar> sorted = LoadedXML;
                        if (GlobalSortMode == SortMode.Ascending)
                              sorted = sorted.OrderBy(x => x.PriceWithTax);
                        else if (GlobalSortMode == SortMode.Descending)
                              sorted = sorted.OrderByDescending(x => x.PriceWithTax);
                        Items = new ObservableCollection<object>(sorted);
                  }
                  OnPropertyChanged(nameof(SortButtonContent));
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      // Simple RelayCommand implementation.
      public class RelayCommand : ICommand
      {
            private readonly Action<object> _execute;
            private readonly Func<object, bool> _canExecute;
            public event EventHandler CanExecuteChanged;

            public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            {
                  _execute = execute;
                  _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
            public void Execute(object parameter) => _execute(parameter);
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
      }
}