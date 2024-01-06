using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kitware.VTK;

namespace test4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int min;
        int max;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowsFormsHost_Loaded(object sender, RoutedEventArgs e)
        {
            ///<summary>
            ///Create Sphere
            ///</summary>
           
            /*vtkSphereSource sphereSource = new vtkSphereSource();
            sphereSource.SetRadius(100);
            sphereSource.Update();

            vtkPolyDataMapper SphereMapper = new vtkPolyDataMapper();
            SphereMapper.SetInputData(sphereSource.GetOutput());
            sphereSource.Update();

            vtkActor sphereActor = new vtkActor();
            sphereActor.SetMapper(SphereMapper);
            //sphereActor.GetProperty().SetColor(0.5, 0.5, 0.5);

            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().AddActor(sphereActor);*/
            

            ///<summary>
            ///show Dicom
            ///</summary>
            
            vtkDICOMImageReader DicomImageReader = new vtkDICOMImageReader();
            //DicomImageReader.SetDirectoryName(@"D:\DicomImages\A990410-BREAST\Study_1");
            DicomImageReader.SetFileName(@"D:\DicomImages\A990410-BREAST\Study_1\CT_RAZEH GOUKEH^OZRA_S1_076.dcm");
            DicomImageReader.Update();


            var imageData = DicomImageReader.GetOutput();
            
            
            

            vtkPoints dicomPoints = new vtkPoints();
            Read_Struct dicomRT = new Read_Struct();
            var RTstructs = dicomRT.GetStructList(@"D:\DicomImages\A990410-BREAST\Study_1\RAZEH GOUKEH^OZRA__RTS.dcm");
            var skin = RTstructs.First(x => x.Key.Equals("Skin")).Value;
            
            var slice99 = skin.SliceContours[99].ContourPoints;
               
            foreach (var point in slice99)
                dicomPoints.InsertPoint(slice99.IndexOf(point), point.X, point.Y, skin.SliceContours[99].Z);

            /*vtkPolyData polyData = new vtkPolyData();
            polyData.SetPoints(dicomPoints);*/

            vtkSelectPolyData skinloop = new vtkSelectPolyData();
            skinloop.SetInputData(imageData);
            //skinloop.SetInputConnection(0,imagepolydata);
            skinloop.SetLoop(dicomPoints);
            //skinloop.SetLoop(polyData.GetPoints());
            skinloop.GenerateSelectionScalarsOn();
            skinloop.SetSelectionModeToSmallestRegion();
            

            vtkClipPolyData clipPolyData = new vtkClipPolyData();
            //clipPolyData.SetInputData(skinloop.GetOutput());
            clipPolyData.SetInputConnection(skinloop.GetOutputPort());
            //clipPolyData.Update();

            vtkPolyDataMapper polyDataMapper = new vtkPolyDataMapper();
            //polyDataMapper.SetInputData(clipPolyData.GetOutput());
            polyDataMapper.SetInputConnection(clipPolyData.GetOutputPort());
            //polyDataMapper.Update();

            vtkLODActor polyActor = new vtkLODActor();
            polyActor.SetMapper(polyDataMapper);

            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().AddActor(polyActor);
            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().SetBackground(0.1, 0.2, 0.4);
            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().ResetCamera();

        }
    }
}