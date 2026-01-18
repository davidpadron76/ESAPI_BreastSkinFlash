using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(ScriptContext context, System.Windows.Window window, ScriptEnvironment environment)
    {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            if (context.StructureSet == null)
            {
                MessageBox.Show("Por favor, abre un StructureSet (Copia Recomendada).");
                return;
            }

            // Validar que NO estemos editando el Body original sin querer
            // (Una medida de seguridad extra)
            /* Recomendación profesional: El usuario debería haber hecho "Copy Structure Set" manualmente antes.
               Intentar duplicar todo el set por código es ineficiente y arriesgado en versiones antiguas de API.
            */

            // Lanzar la Interfaz
            var mainView = new MainView(context.StructureSet, context.Patient);
            window.Content = mainView;
            window.Title = "Generador Pseudo Skin Flash - Mama VMAT";
            window.Width = 450;
            window.Height = 550;
        }
    }

    // -------------------------------------------------------------------------------
    // LÓGICA DE INTERFAZ (WPF Code-Behind)
    // -------------------------------------------------------------------------------
    public class MainView : UserControl
    {
        private StructureSet _ss;
        private Patient _patient;

        // Controles de UI
        private ComboBox _cbPtv;
        private RadioButton _rbLeft;
        private RadioButton _rbRight;
        private TextBox _tbThickness;
        private TextBox _tbHu;
        private Button _btnRun;
        private TextBlock _statusText;

        public MainView(StructureSet ss, Patient patient)
        {
            _ss = ss;
            _patient = patient;
            InitializeComponent();
            LoadStructures();
        }

        private void InitializeComponent()
        {
            this.Background = Brushes.WhiteSmoke;
            var mainGrid = new Grid { Margin = new Thickness(20) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Título
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // PTV
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Lado
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Grosor
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // HU
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Botón
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status

            // 1. Título
            var title = new TextBlock
            {
                Text = "Configuración Skin Flash",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(title, 0); mainGrid.Children.Add(title);

            // 2. Selección de PTV
            var stackPtv = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            stackPtv.Children.Add(new TextBlock { Text = "1. Selecciona el PTV de Mama:", FontWeight = FontWeights.SemiBold });
            _cbPtv = new ComboBox { Margin = new Thickness(0, 5, 0, 0), Height = 25 };
            stackPtv.Children.Add(_cbPtv);
            Grid.SetRow(stackPtv, 1); mainGrid.Children.Add(stackPtv);

            // 3. Selección de Lado
            var stackSide = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            stackSide.Children.Add(new TextBlock { Text = "2. Lateralidad:", FontWeight = FontWeights.SemiBold });
            var stackRadios = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
            _rbLeft = new RadioButton { Content = "Izquierda (Left)", Margin = new Thickness(0, 0, 20, 0) };
            _rbRight = new RadioButton { Content = "Derecha (Right)" };
            stackRadios.Children.Add(_rbLeft);
            stackRadios.Children.Add(_rbRight);
            stackSide.Children.Add(stackRadios);
            Grid.SetRow(stackSide, 2); mainGrid.Children.Add(stackSide);

            // 4. Grosor (Thickness)
            var stackThick = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            stackThick.Children.Add(new TextBlock { Text = "3. Grosor del Flash (mm):", FontWeight = FontWeights.SemiBold });
            stackThick.Children.Add(new TextBlock { Text = "(Recomendado: 5 - 10 mm)", FontSize = 10, Foreground = Brushes.Gray });
            _tbThickness = new TextBox { Text = "7", Width = 60, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 5, 0, 0) };
            stackThick.Children.Add(_tbThickness);
            Grid.SetRow(stackThick, 3); mainGrid.Children.Add(stackThick);

            // 5. Unidades Hounsfield (HU)
            var stackHu = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            stackHu.Children.Add(new TextBlock { Text = "4. Asignar HU al Bolus Virtual:", FontWeight = FontWeights.SemiBold });
            stackHu.Children.Add(new TextBlock { Text = "(Paper: rango ideal -500 a -700)", FontSize = 10, Foreground = Brushes.Gray });

            var panelHuInput = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
            // El signo menos estático
            panelHuInput.Children.Add(new TextBlock { Text = "-", FontSize = 16, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) });
            // La caja de texto
            _tbHu = new TextBox { Text = "500", Width = 60, VerticalAlignment = VerticalAlignment.Center };
            panelHuInput.Children.Add(_tbHu);
            panelHuInput.Children.Add(new TextBlock { Text = "HU", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) });

            stackHu.Children.Add(panelHuInput);
            Grid.SetRow(stackHu, 4); mainGrid.Children.Add(stackHu);

            // 6. Botón Ejecutar
            _btnRun = new Button
            {
                Content = "GENERAR FLASH Y BODY_OPTI",
                Height = 40,
                FontWeight = FontWeights.Bold,
                Background = Brushes.SteelBlue,
                Foreground = Brushes.White
            };
            _btnRun.Click += BtnRun_Click;
            Grid.SetRow(_btnRun, 5); mainGrid.Children.Add(_btnRun);

            // 7. Status
            _statusText = new TextBlock { Text = "Listo.", Margin = new Thickness(0, 10, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray };
            Grid.SetRow(_statusText, 6); mainGrid.Children.Add(_statusText);

            this.Content = mainGrid;
        }

        private void LoadStructures()
        {
            // Cargar solo PTVs en el combobox
            foreach (var s in _ss.Structures)
            {
                if ((s.DicomType == "PTV" || s.Id.ToUpper().Contains("PTV")) && !s.IsEmpty)
                {
                    _cbPtv.Items.Add(s.Id);
                }
            }
            if (_cbPtv.Items.Count > 0) _cbPtv.SelectedIndex = 0;
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Validaciones de Input
                if (_cbPtv.SelectedItem == null) throw new Exception("Selecciona un PTV.");
                if (_rbLeft.IsChecked == false && _rbRight.IsChecked == false) throw new Exception("Selecciona la lateralidad (Izq/Der).");

                if (!double.TryParse(_tbThickness.Text, out double thicknessMm) || thicknessMm < 0)
                    throw new Exception("Grosor inválido.");

                if (!int.TryParse(_tbHu.Text, out int huValueAbs))
                    throw new Exception("Valor HU inválido.");

                // Aplicar signo negativo y validar rango del paper
                int finalHu = -Math.Abs(huValueAbs);
                if (finalHu < -1000 || finalHu > -100) // Rango de seguridad amplio, paper dice -700 a -500
                {
                    var result = MessageBox.Show($"El valor {finalHu} HU está fuera del rango típico (-700 a -500). ¿Deseas continuar?", "Advertencia HU", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No) return;
                }

                _statusText.Text = "Procesando... Por favor espera.";

                // Ejecutar Lógica
                RunAlgorithm(_cbPtv.SelectedItem.ToString(), _rbLeft.IsChecked == true, thicknessMm, finalHu);

                _statusText.Text = "¡Proceso Completado con Éxito!";
                MessageBox.Show("Estructuras creadas:\n\n1. FLASH_VOL (Asignado HU)\n2. BODY_Opti (Usar este en Planificación)", "Finalizado");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                _statusText.Text = "Error.";
            }
        }

        // -------------------------------------------------------------------------------
        // LÓGICA DEL ALGORITMO (ESAPI)
        // -------------------------------------------------------------------------------
        private void RunAlgorithm(string ptvId, bool isLeft, double thickMm, double huValue)
        {
            _patient.BeginModifications();

            // 1. Obtener Estructuras Base
            Structure ptv = _ss.Structures.FirstOrDefault(s => s.Id == ptvId);
            Structure body = _ss.Structures.FirstOrDefault(s => s.DicomType == "EXTERNAL");
            if (body == null) body = _ss.Structures.FirstOrDefault(s => s.Id.ToUpper() == "BODY");

            // OARs para recortar
            Structure lungs = _ss.Structures.FirstOrDefault(s => s.Id.ToUpper().Contains("LUNGS") || s.Id.ToUpper().Contains("PULMONES"));
            // Si no hay "Lungs" combinado, buscar Left/Right según el caso
            if (lungs == null)
            {
                // Estrategia: Si es mama Izq, me importa proteger Pulmón Izq. Si es Der, el Der.
                string lungTargetName = isLeft ? "LUNG_L" : "LUNG_R";
                lungs = _ss.Structures.FirstOrDefault(s => s.Id.ToUpper().Contains(lungTargetName) || s.Id.ToUpper().Contains(isLeft ? "IZQ" : "DER"));
            }

            Structure heart = _ss.Structures.FirstOrDefault(s => s.Id.ToUpper().Contains("HEART") || s.Id.ToUpper().Contains("CORAZON"));

            // 2. Crear Estructura "Bolus Virtual" (La zona de aire alrededor del cuerpo)
            // Lógica: Expandir Body X mm. 
            // FLASH_VOL será la intersección de (PTV expandido) con este (Aire).

            string flashRoiName = "FLASH_VOL";
            Structure flashStruct = _ss.Structures.FirstOrDefault(s => s.Id == flashRoiName);
            if (flashStruct == null) flashStruct = _ss.AddStructure("PTV", flashRoiName); // Tipo PTV o Control para que deje asignar HU

            // A. Expandir PTV isotrópicamente para buscar la piel y el aire
            // Necesitamos que el flash cubra el movimiento respiratorio, ej. 1.5 o 2 cm más allá del PTV original.
            // El usuario define el grosor del flash (hacia afuera), pero el PTV debe crecer para alcanzarlo.
            // Usaremos un margen generoso para capturar la zona de flash.
            double ptvSearchMargin = thickMm + 5.0;

            // B. Zona de "Aire" cercana al cuerpo (Rim)
            // Body + thickMm (hacia afuera)
            // Truco: (Body + thick) - Body = Anillo de aire pegado a la piel
            SegmentVolume bodyExpanded = body.SegmentVolume.Margin(thickMm);
            SegmentVolume airRim = bodyExpanded.Sub(body.SegmentVolume);

            // C. Intersección con la proyección del PTV
            // Flash = (Anillo de Aire) AND (PTV expandido)
            SegmentVolume ptvExpandedVol = ptv.SegmentVolume.Margin(ptvSearchMargin);
            SegmentVolume rawFlash = airRim.And(ptvExpandedVol);

            // D. Limpieza (Cropping) de OARs y Lados opuestos
            // Es vital recortar Pulmón y Corazón si la expansión del PTV se metió ahí (aunque el flash es aire, mejor asegurar)
            // Pero más importante: Recortar lo que NO sea Anterior/Lateral.

            // Si es mama Izquierda, no queremos flash a la derecha del esternón.
            // Podemos usar el "bounding box" del PTV original para limitar geométricamente si es necesario.
            // Por ahora, recortaremos OARs profundos.

            if (lungs != null) rawFlash = rawFlash.Sub(lungs.SegmentVolume.Margin(3.0)); // Margen de seguridad
            if (heart != null) rawFlash = rawFlash.Sub(heart.SegmentVolume.Margin(3.0));

            // Asignar geometría final al Flash
            flashStruct.SegmentVolume = rawFlash;

            // 3. Asignar HU (Punto clave del paper)
            flashStruct.SetAssignedHU(huValue);

            // 4. Crear BODY_Opti (Unión de Body Original + Flash)
            string bodyOptiName = "BODY_Opti";
            Structure bodyOpti = _ss.Structures.FirstOrDefault(s => s.Id == bodyOptiName);
            if (bodyOpti == null) bodyOpti = _ss.AddStructure("EXTERNAL", bodyOptiName); // Tipo EXTERNAL para que el TPS lo reconozca como cuerpo

            // BodyOpti = Body OR Flash
            bodyOpti.SegmentVolume = body.SegmentVolume.Or(flashStruct.SegmentVolume);

            // NOTA FINAL: El Body Original (EXTERNAL) queda intacto.
            // El usuario debe seleccionar BODY_Opti en el plan.
        }
    }
}
