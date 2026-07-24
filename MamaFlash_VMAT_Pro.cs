using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Globalization;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// Mandatory for scripts that modify data (BeginModifications)
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
            if (context.StructureSet == null)
            {
                MessageBox.Show("Por favor, abre un StructureSet (Copia Recomendada).");
                return;
            }

            // Lanzar la Interfaz
            var mainView = new MainView(context.StructureSet, context.Patient);
            window.Content = mainView;
            window.Title = "Generador Pseudo Skin Flash - Mama VMAT";
            window.Width = 450;
            window.Height = 640;
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
        private CheckBox _cbEnableZptv;
        private TextBox _tbZptvMargin;
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
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // zPTV_Expand
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

            // 6. zPTV_Expand (opcional)
            var stackZptv = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            _cbEnableZptv = new CheckBox { Content = "5. Generar zPTV_Expand (opcional)", FontWeight = FontWeights.SemiBold };
            _cbEnableZptv.Checked += (s, e) => { _tbZptvMargin.IsEnabled = true; };
            _cbEnableZptv.Unchecked += (s, e) => { _tbZptvMargin.IsEnabled = false; };
            stackZptv.Children.Add(_cbEnableZptv);
            stackZptv.Children.Add(new TextBlock { Text = "(PTV expandido dentro de la zona de Flash, recortado con el PTV original)", FontSize = 10, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap });

            var panelZptvInput = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
            panelZptvInput.Children.Add(new TextBlock { Text = "Borde del zPTV: ", VerticalAlignment = VerticalAlignment.Center });
            _tbZptvMargin = new TextBox { Text = "10", Width = 60, IsEnabled = false, VerticalAlignment = VerticalAlignment.Center };
            panelZptvInput.Children.Add(_tbZptvMargin);
            panelZptvInput.Children.Add(new TextBlock { Text = " mm", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(3, 0, 0, 0) });
            stackZptv.Children.Add(panelZptvInput);
            Grid.SetRow(stackZptv, 5); mainGrid.Children.Add(stackZptv);

            // 7. Botón Ejecutar
            _btnRun = new Button
            {
                Content = "GENERAR FLASH Y BODY_OPTI",
                Height = 40,
                FontWeight = FontWeights.Bold,
                Background = Brushes.SteelBlue,
                Foreground = Brushes.White
            };
            _btnRun.Click += BtnRun_Click;
            Grid.SetRow(_btnRun, 6); mainGrid.Children.Add(_btnRun);

            // 8. Status
            _statusText = new TextBlock { Text = "Listo.", Margin = new Thickness(0, 10, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray };
            Grid.SetRow(_statusText, 7); mainGrid.Children.Add(_statusText);

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

                // ESAPI Best Practice: Use InvariantCulture for parsing
                if (!double.TryParse(_tbThickness.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double thicknessMm) || thicknessMm < 0)
                    throw new Exception("Grosor inválido.");

                if (!int.TryParse(_tbHu.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int huValueAbs))
                    throw new Exception("Valor HU inválido.");

                // Aplicar signo negativo y validar rango del paper
                int finalHu = -Math.Abs(huValueAbs);
                if (finalHu < -1000 || finalHu > -100) // Rango de seguridad amplio, paper dice -700 a -500
                {
                    var result = MessageBox.Show($"El valor {finalHu} HU está fuera del rango típico (-700 a -500). ¿Deseas continuar?", "Advertencia HU", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No) return;
                }

                bool enableZptv = _cbEnableZptv.IsChecked == true;
                double zptvMargin = 0;
                if (enableZptv)
                {
                    if (!double.TryParse(_tbZptvMargin.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out zptvMargin) || zptvMargin < 0)
                        throw new Exception("Borde de zPTV_Expand inválido.");
                }

                // Confirmación: el Structure Set activo se va a modificar. ESAPI no permite duplicar
                // un Structure Set completo desde el script; si se quiere conservar el original intacto,
                // hay que duplicarlo ANTES desde Eclipse (clic derecho sobre el Structure Set > Copiar),
                // que sí clona todas las estructuras de una sola vez, y correr el script sobre la copia.
                var confirmSet = MessageBox.Show(
                    $"Se van a crear/modificar estructuras en el Structure Set actualmente abierto: '{_ss.Id}'.\n\n" +
                    "Si quieres conservar el original sin cambios: cancela, duplica el Structure Set completo en Eclipse (clic derecho sobre el Structure Set > Copiar) y vuelve a abrir el script sobre esa copia.\n\n" +
                    "¿Continuar modificando el Structure Set actual?",
                    "Confirmar Structure Set", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmSet == MessageBoxResult.No)
                {
                    _statusText.Text = "Cancelado por el usuario.";
                    return;
                }

                _statusText.Text = "Procesando... Por favor espera.";

                // Ejecutar Lógica
                RunAlgorithm(_cbPtv.SelectedItem.ToString(), _rbLeft.IsChecked == true, thicknessMm, finalHu, enableZptv, zptvMargin);

                _statusText.Text = "¡Proceso Completado con Éxito!";
                var summary = new StringBuilder();
                summary.AppendLine("Estructuras creadas:");
                summary.AppendLine();
                summary.AppendLine("1. FLASH_VOL (Asignado HU)");
                summary.AppendLine("2. BODY_Opti (Usar este en Planificación)");
                if (enableZptv) summary.AppendLine("3. zPTV_Expand (PTV extendido a la zona de Flash, recortado con el PTV original)");
                summary.AppendLine();
                summary.AppendLine("NOTA IMPORTANTE:");
                summary.AppendLine("BODY_Opti se ha creado como tipo ORGAN.");
                summary.Append("Para usarlo como cuerpo de cálculo, ve a la pestaña de imágenes en Eclipse, cambia el BODY original a ORGAN, y cambia BODY_Opti a EXTERNAL.");
                MessageBox.Show(summary.ToString(), "Finalizado");
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
        private void RunAlgorithm(string ptvId, bool isLeft, double thickMm, double huValue, bool enableZptv, double zptvMarginMm)
        {
            _patient.BeginModifications();

            // 1. Obtener Estructuras Base
            Structure ptv = _ss.Structures.FirstOrDefault(s => s.Id == ptvId);
            Structure body = _ss.Structures.FirstOrDefault(s => s.DicomType == "EXTERNAL");
            if (body == null) body = _ss.Structures.FirstOrDefault(s => s.Id.ToUpper() == "BODY");

            if (ptv == null) throw new Exception($"No se encontró el PTV '{ptvId}' en el Structure Set.");
            if (ptv.IsEmpty) throw new Exception($"El PTV '{ptvId}' no tiene contorno (está vacío).");
            if (body == null) throw new Exception("No se encontró estructura BODY/EXTERNAL en el Structure Set.");

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
            string flashRoiName = "FLASH_VOL";
            Structure flashStruct = _ss.Structures.FirstOrDefault(s => s.Id == flashRoiName);
            if (flashStruct == null) flashStruct = _ss.AddStructure("PTV", flashRoiName); // Tipo PTV o Control para que deje asignar HU

            // A. Expandir PTV isotrápicamente para buscar la piel y el aire
            double ptvSearchMargin = thickMm + 5.0;

            // B. Zona de "Aire" cercana al cuerpo (Rim)
            SegmentVolume bodyExpanded = body.SegmentVolume.Margin(thickMm);
            SegmentVolume airRim = bodyExpanded.Sub(body.SegmentVolume);

            // C. Intersección con la proyección del PTV
            SegmentVolume ptvExpandedVol = ptv.SegmentVolume.Margin(ptvSearchMargin);
            SegmentVolume rawFlash = airRim.And(ptvExpandedVol);

            // D. Limpieza (Cropping) de OARs
            if (lungs != null) rawFlash = rawFlash.Sub(lungs.SegmentVolume.Margin(3.0)); // Margen de seguridad
            if (heart != null) rawFlash = rawFlash.Sub(heart.SegmentVolume.Margin(3.0));

            // Asignar geometría final al Flash
            flashStruct.SegmentVolume = rawFlash;

            // 3. Asignar HU (Punto clave del paper)
            flashStruct.SetAssignedHU(huValue);

            // 4. (Opcional) Crear zPTV_Expand: el PTV expandido "borde" mm, limitado a la zona
            // del BODY expandido (la misma zona de Flash), y recortado con el PTV original para
            // que quede como una estructura separada (el anillo nuevo, sin solaparse con el PTV).
            if (enableZptv)
            {
                string zPtvName = "zPTV_Expand";
                Structure zPtvStruct = _ss.Structures.FirstOrDefault(s => s.Id == zPtvName);
                if (zPtvStruct == null) zPtvStruct = _ss.AddStructure("CONTROL", zPtvName);

                SegmentVolume ptvExpandedForZ = ptv.SegmentVolume.Margin(zptvMarginMm);
                SegmentVolume zPtvRaw = ptvExpandedForZ.And(bodyExpanded);
                zPtvRaw = zPtvRaw.Sub(ptv.SegmentVolume); // Crop con el PTV original

                zPtvStruct.SegmentVolume = zPtvRaw;
            }

            // 5. Crear BODY_Opti (Unión de Body Original + Flash)
            string bodyOptiName = "BODY_Opti";
            Structure bodyOpti = _ss.Structures.FirstOrDefault(s => s.Id == bodyOptiName);
            
            // ESAPI Best Practice: Cannot have two EXTERNAL structures. Create as ORGAN, warn user to swap types in UI.
            if (bodyOpti == null) bodyOpti = _ss.AddStructure("ORGAN", bodyOptiName); 

            // BodyOpti = Body OR Flash
            bodyOpti.SegmentVolume = body.SegmentVolume.Or(flashStruct.SegmentVolume);
        }
    }
}