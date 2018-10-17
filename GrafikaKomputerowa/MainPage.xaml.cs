using GrafikaKomputerowa.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GrafikaKomputerowa
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private Point startPointOfDragging;
        private bool pointerMovedHandlerIsAdded;
        private bool pointerPressedHandlerIsAdded;
        private bool pointerReleasedHandlerIsAdded;
        private bool pointerShapePressed;
        private string selectedOption;
        private bool pointerIsPressed;
        private Shape draggingElement;
        private Shape resizingElement;
        private Shape selectedElement;
        private Image draggingImage;
        private Image selectedImage;
        private Color color = Colors.White;


        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            pointerMovedHandlerIsAdded = false;
            pointerIsPressed = false;

            //BitmapImage b = new BitmapImage(new Uri("C:\\Users\\Slightom\\Downloads\\ppm-obrazy-testowe\\ppm-test-01-p3.ppm"));
            //myBitMap.Source = b;
        }


        #region PAINT

        #region dragging
        private void Shape_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.Caption = "drag and drop"; // Sets custom UI text
            e.DragUIOverride.IsCaptionVisible = false; // Sets if the caption is visible
            e.DragUIOverride.IsContentVisible = true; // Sets if the dragged content is visible
            e.DragUIOverride.IsGlyphVisible = false; // Sets if the glyph is visibile
        }

        private async void Shape_Drop(object sender, DragEventArgs e)
        {
            var point = e.GetPosition(MyCanvas);
            point.X = Math.Round(point.X, 0);
            point.Y = Math.Round(point.Y, 0);

            var droppingShape = MyCanvas.Children.OfType<Shape>().Where(x => x.Name == draggingElement.Name).FirstOrDefault();
            var leftPoistionOnCanvas = point.X - startPointOfDragging.X;
            var topPoistionOnCanvas = point.Y - startPointOfDragging.Y;

            if (droppingShape != null)
            {
                if (droppingShape is Line)
                {
                    var line = droppingShape as Line;
                    topPoistionOnCanvas = (line.Y1 < line.Y2) ? topPoistionOnCanvas : topPoistionOnCanvas + 2 * startPointOfDragging.Y;
                    var oldLeftOnCanvas = line.X1;
                    var oldTopOnCanvas = line.Y1;
                    if (leftPoistionOnCanvas > oldLeftOnCanvas)
                    {
                        line.X1 += Modul(oldLeftOnCanvas - leftPoistionOnCanvas);
                        line.X2 += Modul(oldLeftOnCanvas - leftPoistionOnCanvas);
                    }
                    else
                    {
                        line.X1 -= Modul(oldLeftOnCanvas - leftPoistionOnCanvas);
                        line.X2 -= Modul(oldLeftOnCanvas - leftPoistionOnCanvas);
                    }
                    if (topPoistionOnCanvas > oldTopOnCanvas)
                    {
                        line.Y1 += Modul(oldTopOnCanvas - topPoistionOnCanvas);
                        line.Y2 += Modul(oldTopOnCanvas - topPoistionOnCanvas);
                    }
                    else
                    {
                        line.Y1 -= Modul(oldTopOnCanvas - topPoistionOnCanvas);
                        line.Y2 -= Modul(oldTopOnCanvas - topPoistionOnCanvas);
                    }
                }
                else
                {
                    Canvas.SetLeft(MyCanvas.Children.OfType<Shape>().Where(x => x.Name == draggingElement.Name).FirstOrDefault(), leftPoistionOnCanvas);
                    Canvas.SetTop(MyCanvas.Children.OfType<Shape>().Where(x => x.Name == draggingElement.Name).FirstOrDefault(), topPoistionOnCanvas);
                }

                droppingShape.Opacity = 1.0;
            }
            else
            {
                var droppingImage = MyCanvas.Children.OfType<Image>().Where(x => x.Name == draggingImage.Name).FirstOrDefault();

                Canvas.SetLeft(MyCanvas.Children.OfType<Image>().Where(x => x.Name == draggingImage.Name).FirstOrDefault(), leftPoistionOnCanvas);
                Canvas.SetTop(MyCanvas.Children.OfType<Image>().Where(x => x.Name == draggingImage.Name).FirstOrDefault(), topPoistionOnCanvas);

                droppingImage.Opacity = 1.0;
            }

            draggingElement = null;
            draggingImage = null;

            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Shape_DragStarting(UIElement sender, DragStartingEventArgs e)
        {
            draggingElement = sender as Shape;
            draggingImage = sender as Image;
            startPointOfDragging = e.GetPosition(MyCanvas);
            var difX = (sender is Line) ? Modul(startPointOfDragging.X - (sender as Line).X1) : Modul(startPointOfDragging.X - Canvas.GetLeft(sender));
            var difY = (sender is Line) ? Modul(startPointOfDragging.Y - (sender as Line).Y1) : Modul(startPointOfDragging.Y - Canvas.GetTop(sender));
            startPointOfDragging.X = Math.Round(difX, 0);
            startPointOfDragging.Y = Math.Round(difY, 0);

            e.Data.RequestedOperation = DataPackageOperation.Move;
            sender.Opacity = 0.2;

            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        }

        


        #endregion

        private void ConfirmDimensions_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOption != "CursorListBoxItem")
            {
                switch (selectedOption)
                {
                    case "ResizeListBoxItem":
                        if (MyCanvas.Children.OfType<Line>().Where(x => x.Name == selectedElement.Name).Count() > 0)
                        {
                            var qw = MyCanvas.Children.OfType<Line>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                            qw.X1 += 20;
                            qw.Y1 += 20;
                            //qw.X2 = Math.Round(double.Parse(X2TextBox.Text), 0);
                            //qw.Y2 = Math.Round(double.Parse(Y2TextBox.Text), 0);

                        }
                        else if (MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == selectedElement.Name).Count() > 0)
                        {
                            var qw = MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                            qw.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                            qw.Height = Modul(double.Parse(Y1TextBox.Text) - double.Parse(Y2TextBox.Text));

                            Canvas.SetLeft(qw, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                            Canvas.SetTop(qw, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                        }
                        else
                        {

                            var qw = MyCanvas.Children.OfType<Ellipse>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                            qw.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                            qw.Height = qw.Width;

                            Canvas.SetLeft(qw, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                            Canvas.SetTop(qw, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                        }
                        break;
                    case "LineListBoxItem":
                        Line l2 = new Line();
                        l2.Name = "l" + MyCanvas.Children.Count();
                        l2.Stroke = new SolidColorBrush(color);
                        l2.X1 = SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text));
                        l2.X2 = BiggerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text));
                        l2.Y1 = (double.Parse(X1TextBox.Text) < double.Parse(X2TextBox.Text)) ? double.Parse(Y1TextBox.Text) : double.Parse(Y2TextBox.Text);
                        l2.Y2 = (double.Parse(X1TextBox.Text) > double.Parse(X2TextBox.Text)) ? double.Parse(Y1TextBox.Text) : double.Parse(Y2TextBox.Text);
                        l2.StrokeThickness = 2;
                        l2.DragStarting += Shape_DragStarting;
                        MyCanvas.Children.Add(l2);
                        Canvas.SetLeft(l2, 0);
                        Canvas.SetTop(l2, 0);
                        break;
                    case "RectangleListBoxItem":
                        Rectangle r = new Rectangle();
                        r.Name = "r" + MyCanvas.Children.Count();
                        r.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                        r.Height = Modul(double.Parse(Y1TextBox.Text) - double.Parse(Y2TextBox.Text));
                        r.Stroke = new SolidColorBrush(color);
                        r.StrokeThickness = 3;
                        r.DragStarting += Shape_DragStarting;
                        MyCanvas.Children.Add(r);
                        Canvas.SetLeft(r, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                        Canvas.SetTop(r, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                        break;
                    case "CircleListBoxItem":
                        Ellipse el = new Ellipse();
                        el.Name = "e" + MyCanvas.Children.Count();
                        el.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                        el.Height = el.Width;
                        el.StrokeThickness = 3;
                        el.Stroke = new SolidColorBrush(color);
                        el.DragStarting += Shape_DragStarting;
                        MyCanvas.Children.Add(el);
                        Canvas.SetLeft(el, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                        Canvas.SetTop(el, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                        break;
                }
            }
            else
            {
                if (MyCanvas.Children.OfType<Line>().Where(x => x.Name == selectedElement.Name).Count() > 0)
                {
                    var qw = MyCanvas.Children.OfType<Line>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                    qw.X1 = Math.Round(double.Parse(X1TextBox.Text), 0);
                    qw.Y1 = Math.Round(double.Parse(Y1TextBox.Text), 0);
                    qw.X2 = Math.Round(double.Parse(X2TextBox.Text), 0);
                    qw.Y2 = Math.Round(double.Parse(Y2TextBox.Text), 0);
                }
                else if (MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == selectedElement.Name).Count() > 0)
                {
                    var qw = MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                    qw.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                    qw.Height = Modul(double.Parse(Y1TextBox.Text) - double.Parse(Y2TextBox.Text));

                    Canvas.SetLeft(qw, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                    Canvas.SetTop(qw, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                }
                else
                {
                    var qw = MyCanvas.Children.OfType<Ellipse>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                    qw.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                    qw.Height = qw.Width;

                    Canvas.SetLeft(qw, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                    Canvas.SetTop(qw, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                }
            }
        }


        private void SelectMenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedOption = SelectShapeListBox.Items.Cast<ListBoxItem>().Where(p => p.IsSelected).FirstOrDefault().Name;
            switch (selectedOption)
            {
                case "CursorListBoxItem":
                    if (MyCanvas != null)
                    {
                        SetPointerMovedEvent(false);
                        SetPointerPressed(false);
                        SetPointerReleased(false);
                        SetPointerEnteredEventForAllObjects(true);
                        SetCanDragForAllObject(true);
                        SetShapePointerPressed(true);
                        //SetShapePointerPressedForAllObjects(true);
                    }

                    break;
                case "ResizeListBoxItem":
                    SetPointerMovedEvent(true);
                    SetPointerPressed(false);
                    SetPointerReleased(false);
                    SetPointerEnteredEventForAllObjects(false);
                    SetCanDragForAllObject(true);
                    SetShapePointerPressed(false);
                    break;
                case "LineListBoxItem":
                    SetPointerMovedEvent(true);
                    SetPointerPressed(true);
                    SetPointerReleased(true);
                    SetPointerEnteredEventForAllObjects(false);
                    SetCanDragForAllObject(false);
                    SetShapePointerPressed(false);
                    //SetShapePointerPressedForAllObjects(false);
                    break;
                case "RectangleListBoxItem":
                    SetPointerMovedEvent(true);
                    SetPointerPressed(true);
                    SetPointerReleased(true);
                    SetPointerEnteredEventForAllObjects(false);
                    SetCanDragForAllObject(false);
                    SetShapePointerPressed(false);
                    //SetShapePointerPressedForAllObjects(false);
                    break;
                case "CircleListBoxItem":
                    SetPointerMovedEvent(true);
                    SetPointerPressed(true);
                    SetPointerReleased(true);
                    SetPointerEnteredEventForAllObjects(false);
                    SetCanDragForAllObject(false);
                    SetShapePointerPressed(false);
                    //SetShapePointerPressedForAllObjects(false);
                    break;
            }
        }



        #region setting pointer handlers
        private void SetPointerReleased(bool add)
        {
            if (add)
            {
                if (!pointerReleasedHandlerIsAdded)
                {
                    MyCanvas.PointerReleased += MyCanvas_PointerReleased;
                    pointerReleasedHandlerIsAdded = true;
                }
            }
            else if (pointerReleasedHandlerIsAdded)
            {
                MyCanvas.PointerReleased -= MyCanvas_PointerReleased;
                pointerReleasedHandlerIsAdded = false;
            }
        }

        private void SetPointerPressed(bool add)
        {
            if (add)
            {
                if (!pointerPressedHandlerIsAdded)
                {
                    MyCanvas.PointerPressed += MyCanvas_PointerPressed;
                    pointerPressedHandlerIsAdded = true;
                }
            }
            else if (pointerPressedHandlerIsAdded)
            {
                MyCanvas.PointerPressed -= MyCanvas_PointerPressed;
                pointerPressedHandlerIsAdded = false;
            }
        }

        private void SetPointerMovedEvent(bool add)
        {
            if (add)
            {
                if (!pointerMovedHandlerIsAdded)
                {
                    MyCanvas.PointerMoved += MyCanvas_PointerMoved;
                    pointerMovedHandlerIsAdded = true;
                }
            }
            else if (pointerMovedHandlerIsAdded)
            {
                MyCanvas.PointerMoved -= MyCanvas_PointerMoved;
                pointerMovedHandlerIsAdded = false;
            }
        }

        private void SetPointerEnteredEventForAllObjects(bool v)
        {
            if (v)
            {
                foreach (var c in MyCanvas.Children.OfType<Shape>())
                {
                    if (!c.Name.Contains("pointerEntered"))
                    {
                        c.PointerEntered += Shape_PointerEntered;
                        c.PointerExited += Shape_PointerExited;
                        c.Name += "pointerEntered";
                    }
                }
            }
            else
            {
                foreach (var c in MyCanvas.Children.OfType<Shape>())
                {
                    if (c.Name.Contains("pointerEntered"))
                    {
                        c.PointerEntered -= Shape_PointerEntered;
                        c.PointerExited -= Shape_PointerExited;
                        c.Name = c.Name.Replace("pointerEntered", "");
                    }
                }
            }
        }

        private void SetShapePointerPressed(bool add)
        {
            if (add)
            {
                if (!pointerShapePressed)
                {
                    MyCanvas.PointerPressed += Shape_PointerPressed;
                    pointerShapePressed = true;
                }
            }
            else if (pointerShapePressed)
            {
                MyCanvas.PointerPressed -= Shape_PointerPressed;
                pointerShapePressed = false;
            }
        }

        //private void SetShapePointerPressedForAllObjects(bool v)
        //{
        //    if (v)
        //    {
        //        foreach (var c in MyCanvas.Children.OfType<Shape>())
        //        {
        //            if (!c.Name.Contains("shapePointerPressed"))
        //            {
        //                c.PointerPressed += Shape_PointerPressed;
        //                c.Name += "shapePointerPressed";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var c in MyCanvas.Children.OfType<Shape>())
        //        {
        //            if (c.Name.Contains("shapePointerPressed"))
        //            {
        //                c.PointerPressed -= Shape_PointerPressed;
        //                c.Name = c.Name.Replace("shapePointerPressed", "");
        //            }
        //        }
        //    }
        //}
        #endregion

        #region pointer handlers
        private void MyCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!pointerIsPressed)
            {
                X1TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.X, 0).ToString();
                Y1TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.Y, 0).ToString();
            }
            else
            {
                X2TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.X, 0).ToString();
                Y2TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.Y, 0).ToString();
            }

        }

        private void MyCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            pointerIsPressed = true;
            X1TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.X, 0).ToString();
            Y1TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.Y, 0).ToString();
        }

        private void MyCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            pointerIsPressed = false;
            X2TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.X, 0).ToString();
            Y2TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.Y, 0).ToString();

            switch (selectedOption)
            {
                case "LineListBoxItem":
                    Line l2 = new Line();
                    l2.Name = "l" + MyCanvas.Children.Count();
                    l2.Stroke = new SolidColorBrush(color);
                    l2.X1 = Math.Round(SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)), 0);
                    l2.Y1 = Math.Round((double.Parse(X1TextBox.Text) < double.Parse(X2TextBox.Text)) ? double.Parse(Y1TextBox.Text) : double.Parse(Y2TextBox.Text), 0);
                    l2.X2 = Math.Round(BiggerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)), 0);
                    l2.Y2 = Math.Round((double.Parse(X1TextBox.Text) > double.Parse(X2TextBox.Text)) ? double.Parse(Y1TextBox.Text) : double.Parse(Y2TextBox.Text), 0);
                    l2.StrokeThickness = 2;
                    l2.DragStarting += Shape_DragStarting;
                    MyCanvas.Children.Add(l2);

                    Canvas.SetLeft(l2, 0);
                    Canvas.SetTop(l2, 0);
                    break;

                case "RectangleListBoxItem":
                    Rectangle r = new Rectangle();
                    r.Name = "r" + MyCanvas.Children.Count();
                    r.Width = Math.Round(Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text)), 0);
                    r.Height = Math.Round(Modul(double.Parse(Y1TextBox.Text) - double.Parse(Y2TextBox.Text)), 0);
                    r.Stroke = new SolidColorBrush(color);
                    r.StrokeThickness = 3;
                    r.DragStarting += Shape_DragStarting;
                    MyCanvas.Children.Add(r);
                    Canvas.SetLeft(r, Math.Round(SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)), 0));
                    Canvas.SetTop(r, Math.Round(HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)), 0));
                    break;

                case "CircleListBoxItem":
                    Ellipse el = new Ellipse();
                    el.Name = "e" + MyCanvas.Children.Count();
                    el.Width = Math.Round(Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text)), 0);
                    el.Height = el.Width;
                    el.StrokeThickness = 3;
                    el.Stroke = new SolidColorBrush(color);
                    el.DragStarting += Shape_DragStarting;
                    MyCanvas.Children.Add(el);
                    Canvas.SetLeft(el, Math.Round(SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)), 0));
                    Canvas.SetTop(el, Math.Round(HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)), 0));
                    break;
            }

        }

        private void Shape_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);

            if (sender is Line) { selectedElement = MyCanvas.Children.OfType<Line>().Where(x => x.Name == (sender as Line).Name).FirstOrDefault(); }
            else if (sender is Rectangle) { selectedElement = MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == (sender as Rectangle).Name).FirstOrDefault(); }
            else { selectedElement = MyCanvas.Children.OfType<Ellipse>().Where(x => x.Name == (sender as Ellipse).Name).FirstOrDefault(); }

        }

        private void Shape_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Shape_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (selectedElement is Line)
            {
                selectedElement = MyCanvas.Children.OfType<Line>().Where(x => x.Name == (selectedElement as Line).Name).FirstOrDefault();
                var line = (selectedElement as Line);
                X1TextBox.Text = line.X1.ToString();
                X2TextBox.Text = line.X2.ToString();
                Y1TextBox.Text = line.Y1.ToString();
                Y2TextBox.Text = line.Y2.ToString();

            }
            else if (selectedElement is Rectangle)
            {
                selectedElement = MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == (selectedElement as Rectangle).Name).FirstOrDefault();
                X1TextBox.Text = Canvas.GetLeft(selectedElement).ToString();
                X2TextBox.Text = (Canvas.GetLeft(selectedElement) + selectedElement.Width).ToString();
                Y1TextBox.Text = Canvas.GetTop(selectedElement).ToString();
                Y2TextBox.Text = (Canvas.GetTop(selectedElement) + selectedElement.Height).ToString();
            }
            else
            {
                selectedElement = MyCanvas.Children.OfType<Ellipse>().Where(x => x.Name == (selectedElement as Ellipse).Name).FirstOrDefault();
                X1TextBox.Text = Canvas.GetLeft(selectedElement).ToString();
                X2TextBox.Text = (Canvas.GetLeft(selectedElement) + selectedElement.Width).ToString();
                Y1TextBox.Text = Canvas.GetTop(selectedElement).ToString();
                Y2TextBox.Text = (Canvas.GetTop(selectedElement) + selectedElement.Height).ToString();
            }
        }
        #endregion

        #region helpful functions 

        private double HigherFrom(double v1, double v2)
        {
            if (v1 < v2) { return v1; }
            else { return v2; }
        }

        private double SmallerFrom(double v1, double v2)
        {
            if (v1 < v2) { return v1; }
            else { return v2; }
        }

        private double LowerFrom(double v1, double v2)
        {
            if (v1 > v2) { return v1; }
            else { return v2; }
        }

        private double BiggerFrom(double v1, double v2)
        {
            if (v1 > v2) { return v1; }
            else { return v2; }
        }

        private double Modul(double v)
        {
            if (v < 0) { return v * (-1); }
            else { return v; }
        }

        private void SetCanDragForAllObject(bool v)
        {
            foreach (var c in MyCanvas.Children) { c.CanDrag = v; }
        }

        #endregion

        #endregion

        #region PPM

        #endregion



    }
}
