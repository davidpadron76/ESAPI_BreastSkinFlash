# ⚡ ESAPI Breast Skin Flash

## 📖 Overview
The **ESAPI Breast Skin Flash** tool is an open-source automation script developed for the Varian Eclipse Treatment Planning System (TPS). 

In breast radiotherapy planning (such as 3DCRT or Field-in-Field / hybrid IMRT), generating "skin flash" is critical to account for respiratory motion and daily setup uncertainties by extending the radiation fluence outside the patient's external contour. Creating virtual boluses or override structures manually to achieve this effect is a highly repetitive and time-consuming task. This script automates the entire skin flash generation process using robust ESAPI geometric operations.

## ✨ Key Features
* **Automated Flash Generation:** Instantly creates the necessary virtual structures (e.g., virtual bolus or density override volumes) to force the optimizer or dose calculation engine to extend fluence outside the skin.
* **Customizable Margins:** Allows the user to quickly define the required expansion margin (in millimeters) based on clinical protocols.
* **Anatomy-Independent Logic:** Operates purely on Boolean geometry, ensuring it works consistently regardless of patient anatomy or breast size.
* **Workflow Efficiency:** Eliminates tedious manual contouring and structure manipulation, saving significant time during the breast planning process.

## 💻 System Requirements
* **Eclipse TPS:** Version 15.5 or higher.
* **.NET Framework:** Compatible with your clinic's specific ESAPI version (e.g., 4.5 for v15.6, or 4.6+ for v16+).

## 🛠️ Installation & Compilation
To ensure proper functionality within the Eclipse environment, this project must be compiled into a `.dll` library.

1. Clone or download this repository to your local machine.
2. Open the solution file (`.sln`) using **Visual Studio**.
3. In the Solution Explorer, right-click the solution and select **Restore NuGet Packages**.
4. Build the solution (`Ctrl + Shift + B` or `Build > Build Solution`).
5. Locate the compiled `.dll` file inside the `bin\Debug` or `bin\Release` folder.
6. In Eclipse, open the Script Runner, navigate to the folder containing your compiled `.dll`, and execute it.

## 🚀 How to Use
1. Open a Breast Patient and the corresponding Structure Set/Plan in Eclipse.
2. Run the compiled Breast Skin Flash `.dll`.
3. Select the required target volumes and define the skin flash margin parameters in the UI.
4. Click **Generate** and review the newly created flash structures in the TPS.

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ⚠️ Clinical Disclaimer
**For Research and Educational Purposes Only.** This software is provided "as is", without warranty of any kind. It is the sole responsibility of the clinical user (Medical Physicist or Dosimetrist) to strictly verify and validate all generated contours, assigned Hounsfield Units (HU), and final dose distributions before using them for clinical patient treatment.
