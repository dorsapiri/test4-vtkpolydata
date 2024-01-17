using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Kitware.VTK;
//using OpenCvSharp;

namespace test4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            
        }

        

        private void WindowsFormsHost_Loaded(object sender, RoutedEventArgs e)
        {
            ///<summary>
            ///Create Sphere
            ///</summary>

            //SphereProcessing();


            ///<summary>
            ///show Dicom
            ///</summary>

            //DicomContour();
            //DrowSkinPolygon();
            CuttingDicom();

            /*ReadDICOMSeries();
            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;*/

            //DicomView();
            //test();

        }

        private void DicomView()
        {
            vtkDICOMImageReader DicomImageReader = new vtkDICOMImageReader();   
            DicomImageReader.SetFileName(@"D:\DicomImages\A990410-BREAST\Study_1\CT_RAZEH GOUKEH^OZRA_S1_076.dcm");
            DicomImageReader.Update();

            vtkImageActor imageActor = new vtkImageActor();
            imageActor.SetInputData(DicomImageReader.GetOutput());

            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().AddActor(imageActor);
            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().SetBackground(.5, .5, .5);
            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().ResetCamera();

           
        }

        public void DicomContour() 
        {
            vtkDICOMImageReader DicomImageReader = new vtkDICOMImageReader();
            //DicomImageReader.SetDirectoryName(@"D:\DicomImages\A990410-BREAST\Study_1");
            DicomImageReader.SetFileName(@"D:\DicomImages\A990410-BREAST\Study_1\CT_RAZEH GOUKEH^OZRA_S1_076.dcm");
            DicomImageReader.Update();


            var imageData = DicomImageReader.GetOutput();
            var imagedataport = DicomImageReader.GetOutputPort();

            vtkContourFilter contourFilter = new vtkContourFilter();
            contourFilter.SetInputData(DicomImageReader.GetOutput());
            

            vtkImageMapToWindowLevelColors imageMapToWindowLevelColors = new vtkImageMapToWindowLevelColors();
            imageMapToWindowLevelColors.SetWindow(255);
            imageMapToWindowLevelColors.SetLevel(127.5);

            vtkPolyDataMapper polyDataMapper = vtkPolyDataMapper.New();
            polyDataMapper.SetInputConnection(contourFilter.GetOutputPort());

            vtkActor actor = new vtkActor();
            actor.SetMapper(polyDataMapper);
            
            

            vtkImageViewer2 imageViewer = vtkImageViewer2.New();
            //imageViewer.SetInputData(DicomImageReader.GetOutput());
            imageViewer.GetRenderer().AddActor(actor);
            imageViewer.GetRenderer().SetBackground(.5, .5, .5);
            imageViewer.SetInputData(imageMapToWindowLevelColors.GetOutput());
            imageViewer.SetRenderWindow(vRWC.RenderWindow);
            imageViewer.Render();

            

            

            //vRWC.RenderWindow.GetRenderers().GetFirstRenderer().AddActor(polyActor);
            //vRWC.RenderWindow.GetRenderers().GetFirstRenderer().SetBackground(0.1, 0.2, 0.4);
            //vRWC.RenderWindow.GetRenderers().GetFirstRenderer().ResetCamera();

        }

        public void DrowSkinPolygon()
        {
            //Contour Points in RT-Structure
            Read_Struct dicomRT = new Read_Struct();
            var RTstructs = dicomRT.GetStructList(@"D:\DicomImages\A990410-BREAST\Study_1\RAZEH GOUKEH^OZRA__RTS.dcm");
            var skin = RTstructs.First(x => x.Key.Equals("Skin")).Value;
            var slice100 = skin.SliceContours[99].ContourPoints;
            var slice1 = skin.SliceContours[0].ContourPoints;
            var zCoordination = skin.SliceContours[0].Z;

            var xMin = slice1.Min(p => p.X);
            var yMin = slice1.Min(p => p.Y);
            //VtkPoints add Contour Points 
            vtkPoints curvePoints = new vtkPoints();
            foreach (var point in slice1) 
            {
                var x = point.X - xMin;
                var y = point.Y - yMin;
                curvePoints.InsertNextPoint(x, y, zCoordination);
            }
            
            vtkCellArray curveCells = new vtkCellArray();
            var lastPid = slice1.IndexOf(slice1.Last());
            curveCells.InsertNextCell(lastPid);
            for (var point=0;point<lastPid; point++)
            {
                curveCells.InsertCellPoint(point);
            }
            
            vtkPolyData polyData = new vtkPolyData();
            polyData.SetPoints(curvePoints);
            //polyData.SetPolys(curveCells);
            polyData.SetLines(curveCells);

            vtkPolyDataMapper mapper = new vtkPolyDataMapper();
            mapper.SetInputData(polyData);

            vtkActor actor = new vtkActor();
            actor.SetMapper(mapper);
            actor.SetPosition(0, 0, 0);
            
            vtkRenderWindowInteractor interactor = new vtkRenderWindowInteractor();
            interactor.SetRenderWindow(vRWC.RenderWindow);

            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().AddActor(actor);
            //vRWC.RenderWindow.GetRenderers().GetFirstRenderer().ResetCamera();
            //vRWC.RenderWindow.GetRenderers().GetFirstRenderer().Render();


        }
        

        public void CuttingDicom()
        {
            vtkDICOMImageReader DicomImage = new vtkDICOMImageReader();
            DicomImage.SetFileName(@"D:\DicomImages\A990410-BREAST\Study_1\CT_RAZEH GOUKEH^OZRA_S1_076.dcm");
            //DicomImageReader.SetDirectoryName(@"D:\DicomImages\A990410-BREAST\Study_1");
            DicomImage.Update();

            /*vtkImageData imageData = DicomImage.GetOutput();

            vtkContourFilter contourFilter = new vtkContourFilter();
            contourFilter.SetInputData(DicomImage.GetOutput());

            vtkPolyData contourPolyData = contourFilter.GetOutput();
            contourFilter.Update();*/

            vtkImageViewer2 dicomViewer = new vtkImageViewer2();
            
            var imageCenter = DicomImage.GetOutput().GetCenter();

            //Contour Points in RT-Structure
            Read_Struct dicomRT = new Read_Struct();
            var RTstructs = dicomRT.GetStructList(@"D:\DicomImages\A990410-BREAST\Study_1\RAZEH GOUKEH^OZRA__RTS.dcm");
            var skin = RTstructs.First(x => x.Key.Equals("Skin")).Value;
            var slice1 = skin.SliceContours[0].ContourPoints;
            var zCoordination = skin.SliceContours[0].Z;
            var xMin = slice1.Min(p => p.X);
            var xMax = slice1.Max(p => p.X);
            var yMin = slice1.Min(p => p.Y);
            var yMax = slice1.Max(p => p.Y);
            var xCenter = (xMax - xMin) / 2;
            var yCenter = (yMax - yMin) / 2;
            var diffCenterX = imageCenter[0] - xCenter;
            var diffCenterY = imageCenter[1] - yCenter;
            //VtkPoints add Contour Points 
            vtkPoints curvePoints = new vtkPoints();
            foreach (var point in slice1)
            {
                var x = point.X - xMin + diffCenterX;
                var y = point.Y - yMin + diffCenterY;
                curvePoints.InsertNextPoint(x, y, zCoordination);
            }

            vtkCellArray curveCells = new vtkCellArray();
            var lastPid = slice1.IndexOf(slice1.Last());
            curveCells.InsertNextCell(lastPid);
            for (var point = 0; point < lastPid; point++)
            {
                curveCells.InsertCellPoint(point);
            }

            vtkPolyData polyData = new vtkPolyData();
            polyData.SetPoints(curvePoints);
            polyData.SetLines(curveCells);

            vtkImageData newImageData = new vtkImageData();
            vtkImageData imageData = DicomImage.GetOutput();
            var test = imageData.GetPointData().GetScalars().GetTuple1(131328);
            var fpointPosition = imageData.GetPoint(0);
            var lpointPosition = imageData.GetPoint(512*512-1);
            var minXcurve = polyData.GetBounds()[0];
            var maxXcurve = polyData.GetBounds()[1];
            var minYcurve = polyData.GetBounds()[2];
            var maxYcurve = polyData.GetBounds()[3];
            

            double[] croppingRegion = { minXcurve, maxXcurve, minYcurve, maxYcurve, 0, 0 };
            
            int[] dims = DicomImage.GetOutput().GetDimensions();
            double[] spacing = DicomImage.GetPixelSpacing();
            float[] origin = DicomImage.GetImagePositionPatient();

            //the region of interest (ROI)
            int roiXcoord = (int)Math.Round(minXcurve); 
            int roiYCoord = (int)Math.Round(minYcurve); 
            int roiWidth = (int)Math.Round(maxXcurve); 
            int roiHeight = (int)Math.Round(maxYcurve); 

            // Create a region of interest filter
            vtkExtractVOI roiFilter = vtkExtractVOI.New();
            roiFilter.SetInputConnection(DicomImage.GetOutputPort());
            roiFilter.SetVOI(roiXcoord, roiXcoord + roiWidth - 1, roiYCoord, roiYCoord + roiHeight - 1, 0, 0);
            roiFilter.Update();

            vtkImageActor imageActor = vtkImageActor.New();
            imageActor.SetInputData(roiFilter.GetOutput());

            vtkCamera camera = vRWC.RenderWindow.GetRenderers().GetFirstRenderer().GetActiveCamera();
            camera.SetPosition(origin[0], origin[1], origin[2] + spacing[2] * dims[2]);
            camera.SetFocalPoint(origin[0], origin[1], origin[2]);
            camera.SetViewUp(0, 1, 0);
            //camera.Azimuth(30.0);
            //camera.Elevation(30.0);

            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().AddActor(imageActor);
            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().ResetCamera();
            
            
        }


        //-------------------------Test Area-------------------
        vtkImageViewer2 _ImageViewer;
        vtkTextMapper _SliceStatusMapper;
        int _Slice;
        int _MinSlice;
        int _MaxSlice;
        private void ReadDICOMSeries()
        {
            // Caution: folder "DicomTestImages" don't exists by default in the standard vtk data folder
            // sample data are available at http://www.vtk.org/Wiki/images/1/12/VTK_Examples_StandardFormats_Input_DicomTestImages.zip
            
            string folder = @"D:\DicomImages\A990410-BREAST\Study_1";
            vtkDICOMImageReader reader = vtkDICOMImageReader.New();
            reader.SetDirectoryName(folder);
            reader.Update();
            // Visualize
            _ImageViewer = vtkImageViewer2.New();
            _ImageViewer.SetInputConnection(reader.GetOutputPort());
            // get range of slices (min is the first index, max is the last index)
            _ImageViewer.GetSliceRange(ref _MinSlice, ref _MaxSlice);
            Debug.WriteLine("slices range from : " + _MinSlice.ToString() + " to " + _MaxSlice.ToString());

            // slice status message
            vtkTextProperty sliceTextProp = vtkTextProperty.New();
            sliceTextProp.SetFontFamilyToCourier();
            sliceTextProp.SetFontSize(20);
            sliceTextProp.SetVerticalJustificationToBottom();
            sliceTextProp.SetJustificationToLeft();

            _SliceStatusMapper = vtkTextMapper.New();
            _SliceStatusMapper.SetInput("Slice No " + (_Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
            _SliceStatusMapper.SetTextProperty(sliceTextProp);

            vtkActor2D sliceStatusActor = vtkActor2D.New();
            sliceStatusActor.SetMapper(_SliceStatusMapper);
            sliceStatusActor.SetPosition(15, 10);

            // usage hint message
            vtkTextProperty usageTextProp = vtkTextProperty.New();
            usageTextProp.SetFontFamilyToCourier();
            usageTextProp.SetFontSize(14);
            usageTextProp.SetVerticalJustificationToTop();
            usageTextProp.SetJustificationToLeft();

            vtkTextMapper usageTextMapper = vtkTextMapper.New();
            usageTextMapper.SetInput("Slice with mouse wheel\nor Up/Down-Key");
            usageTextMapper.SetTextProperty(usageTextProp);

            vtkActor2D usageTextActor = vtkActor2D.New();
            usageTextActor.SetMapper(usageTextMapper);
            usageTextActor.GetPositionCoordinate().SetCoordinateSystemToNormalizedDisplay();
            usageTextActor.GetPositionCoordinate().SetValue(0.05, 0.95);

            //vtkRenderWindow renderWindow = renderWindowControl1.RenderWindow;
            

            vtkInteractorStyleImage interactorStyle = vtkInteractorStyleImage.New();
            //interactorStyle.MouseWheelForwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelForwardEvt);
            //interactorStyle.MouseWheelBackwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelBackwardEvt);

            

            vRWC.RenderWindow.GetInteractor().SetInteractorStyle(interactorStyle);
            vRWC.RenderWindow.GetRenderers().InitTraversal();
            vtkRenderer ren;
            while ((ren = vRWC.RenderWindow.GetRenderers().GetNextItem()) != null)
                ren.SetBackground(0.0, 0.0, 0.0);

            _ImageViewer.SetRenderWindow(vRWC.RenderWindow);
            _ImageViewer.GetRenderer().AddActor2D(sliceStatusActor);
            _ImageViewer.GetRenderer().AddActor2D(usageTextActor);
            _ImageViewer.SetSlice(_MinSlice);
            _ImageViewer.Render();
        }


        /// <summary>
        /// move forward to next slice
        /// </summary>
        private void MoveForwardSlice()
        {
            Debug.WriteLine(_Slice.ToString());
            if (_Slice < _MaxSlice)
            {
                _Slice += 1;
                _ImageViewer.SetSlice(_Slice);
                _SliceStatusMapper.SetInput("Slice No " + (_Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
                _ImageViewer.Render();
            }
        }


        /// <summary>
        /// move backward to next slice
        /// </summary>
        private void MoveBackwardSlice()
        {
            Debug.WriteLine(_Slice.ToString());
            if (_Slice > _MinSlice)
            {
                _Slice -= 1;
                _ImageViewer.SetSlice(_Slice);
                _SliceStatusMapper.SetInput("Slice No " + (_Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
                _ImageViewer.Render();
            }
        }


        /// <summary>
        /// eventhanndler to process keyboard input
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        /*protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            //Debug.WriteLine(DateTime.Now + ":" + msg.Msg + ", " + keyData);
            if (keyData == System.Windows.Forms.Keys.Up)
            {
                MoveForwardSlice();
                return true;
            }
            else if (keyData == System.Windows.Forms.Keys.Down)
            {
                MoveBackwardSlice();
                return true;
            }
            // don't forward the following keys
            // add all keys which are not supposed to get forwarded
            else if (
                  keyData == System.Windows.Forms.Keys.F
               || keyData == System.Windows.Forms.Keys.L
            )
            {
                return true;
            }
            return false;
        }*/
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                MoveBackwardSlice();
                
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                MoveForwardSlice();
            }
        }
        /// <summary>
        /// event handler for mousewheel forward event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /* void interactor_MouseWheelForwardEvt(vtkObject sender, vtkObjectEventArgs e)
         {
             MoveForwardSlice();
         }*/


        /// <summary>
        /// event handler for mousewheel backward event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*void interactor_MouseWheelBackwardEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            MoveBackwardSlice();
        }*/


        public void SphereProcessing()
        {
            vtkSphereSource sphereSource = vtkSphereSource.New();
            sphereSource.Update();

            vtkPoints selectionPoints = vtkPoints.New();

            selectionPoints.InsertPoint(0, -0.16553, 0.135971, 0.451972);
            selectionPoints.InsertPoint(1, -0.0880123, -0.134952, 0.4747);
            selectionPoints.InsertPoint(2, 0.00292618, -0.134604, 0.482459);
            selectionPoints.InsertPoint(3, 0.0641941, 0.067112, 0.490947);
            selectionPoints.InsertPoint(4, 0.15577, 0.0734765, 0.469245);
            selectionPoints.InsertPoint(5, 0.166667, -0.129217, 0.454622);
            selectionPoints.InsertPoint(6, 0.241259, -0.123363, 0.420581);
            selectionPoints.InsertPoint(7, 0.240334, 0.0727106, 0.432555);
            selectionPoints.InsertPoint(8, 0.308529, 0.0844311, 0.384357);
            selectionPoints.InsertPoint(9, 0.32672, -0.121674, 0.359187);
            selectionPoints.InsertPoint(10, 0.380721, -0.117342, 0.302527);
            selectionPoints.InsertPoint(11, 0.387804, 0.0455074, 0.312375);
            selectionPoints.InsertPoint(12, 0.43943, -0.111673, 0.211707);
            selectionPoints.InsertPoint(13, 0.470984, -0.0801913, 0.147919);
            selectionPoints.InsertPoint(14, 0.436777, 0.0688872, 0.233021);
            selectionPoints.InsertPoint(15, 0.44874, 0.188852, 0.109882);
            selectionPoints.InsertPoint(16, 0.391352, 0.254285, 0.176943);
            selectionPoints.InsertPoint(17, 0.373274, 0.154162, 0.294296);
            selectionPoints.InsertPoint(18, 0.274659, 0.311654, 0.276609);
            selectionPoints.InsertPoint(19, 0.206068, 0.31396, 0.329702);
            selectionPoints.InsertPoint(20, 0.263789, 0.174982, 0.387308);
            selectionPoints.InsertPoint(21, 0.213034, 0.175485, 0.417142);
            selectionPoints.InsertPoint(22, 0.169113, 0.261974, 0.390286);
            selectionPoints.InsertPoint(23, 0.102552, 0.25997, 0.414814);
            selectionPoints.InsertPoint(24, 0.131512, 0.161254, 0.454705);
            selectionPoints.InsertPoint(25, 0.000192443, 0.156264, 0.475307);
            selectionPoints.InsertPoint(26, -0.0392091, 0.000251724, 0.499943);
            selectionPoints.InsertPoint(27, -0.096161, 0.159646, 0.46438);

            vtkSelectPolyData loop = vtkSelectPolyData.New();
            loop.SetInputConnection(sphereSource.GetOutputPort());
            loop.SetLoop(selectionPoints);
            loop.GenerateSelectionScalarsOn();
            loop.SetSelectionModeToSmallestRegion(); //negative scalars inside

            vtkClipPolyData clip = //clips out positive region
            vtkClipPolyData.New();
            clip.SetInputConnection(loop.GetOutputPort());

            vtkPolyDataMapper clipMapper = vtkPolyDataMapper.New();
            clipMapper.SetInputConnection(clip.GetOutputPort());

            vtkLODActor clipActor = vtkLODActor.New();
            clipActor.SetMapper(clipMapper);

            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().AddActor(clipActor);
        }
        public void test()
        {
            vtkPoints points = vtkPoints.New();
            points.InsertNextPoint(0, 0, 0);
            points.InsertNextPoint(1, 0, 0);
            points.InsertNextPoint(1, 1, 0);
            points.InsertNextPoint(0, 1, 0);

            vtkCellArray cellArray = vtkCellArray.New();
            cellArray.InsertNextCell(4);
            cellArray.InsertCellPoint(0);
            cellArray.InsertCellPoint(1);
            cellArray.InsertCellPoint(2);
            cellArray.InsertCellPoint(3);

            // Create a PolyData to hold the points and cells
            vtkPolyData polyData = vtkPolyData.New();
            polyData.SetPoints(points);
            polyData.SetPolys(cellArray);

            // Create a mapper and actor
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.SetInputData(polyData);

            vtkActor actor = vtkActor.New();
            actor.SetMapper(mapper);

            // Add actor to renderer
            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().AddActor(actor);
            vRWC.RenderWindow.GetRenderers().GetFirstRenderer().ResetCamera();

        }

    }

}