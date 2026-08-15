# ⚡ ESAPI Breast Skin Flash

## 📖 Overview
The **ESAPI Breast Skin Flash** tool is an open-source automation script developed for the Varian Eclipse Treatment Planning System (TPS). 

In breast radiotherapy planning (such as 3DCRT or Field-in-Field / hybrid IMRT), generating "skin flash" is critical to account for respiratory motion and daily setup uncertainties by extending the radiation fluence outside the patient's external contour. Creating virtual boluses or override structures manually to achieve this effect is a highly repetitive and time-consuming task. This script automates the entire skin flash generation process using robust ESAPI geometric operations.

## ✨ Key Features
* **Automated Flash Generation:** Instantly creates `FLASH_VOL` (a virtual bolus/density-override volume around the skin, with an assigned HU) and `BODY_Opti` (the union of the original BODY with the flash, ready to be used as the calculation body) to force the optimizer or dose calculation engine to extend fluence outside the skin.
* **Optional anterior zPTV_Expand:** On request, generates a `zPTV_Expand` control structure — the PTV expanded **in the anterior direction only** (never posteriorly toward the lung, nor laterally), cropped to `BODY_Opti` (the actual calculation body: tissue plus flash) and cropped with the original PTV, so it exists as its own non-overlapping anterior layer for optimization purposes. The anterior direction is auto-detected per case from the PTV/BODY geometry, so it holds for both supine and prone setups.
* **Customizable Margins:** Allows the user to quickly define the flash thickness, the assigned HU, and (if enabled) the anterior zPTV_Expand border (0–50 mm), based on clinical protocols.
* **Anatomy-Independent Logic:** Operates purely on Boolean geometry (Margin/And/Sub/Or on `SegmentVolume`), ensuring it works consistently regardless of patient anatomy or breast size.
* **Structure Set Safety Check:** Before writing any structure, the script confirms which Structure Set is about to be modified and reminds the user to duplicate it first (via Eclipse's native right-click **Copy** on the Structure Set) if the original must stay untouched — ESAPI has no scripting call to clone an entire Structure Set in one step, so this native Eclipse action is the recommended workflow.
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
   * If you need to keep the original Structure Set untouched, duplicate it first in Eclipse (right-click the Structure Set > **Copy**) and open the script on the copy — the script itself cannot clone a full Structure Set.
2. Run the compiled Breast Skin Flash `.dll`.
3. Select the breast PTV, the laterality, the flash thickness (mm) and the assigned HU.
4. (Optional) Check **Generate anterior zPTV_Expand** and set its anterior border (0–50 mm) if you also want the cropped PTV-extension structure for optimization. The expansion is applied strictly in the anterior direction — laterality (left/right breast) does not change it, since no lateral margin is applied at all.
5. Click **GENERAR FLASH Y BODY_OPTI**, confirm the Structure Set to modify in the safety prompt, and review the newly created structures (`FLASH_VOL`, `BODY_Opti`, and `zPTV_Expand` if enabled) in the TPS.
6. `BODY_Opti` is created as an `ORGAN` structure. To use it as the calculation body, go to the image/structure properties in Eclipse, change the original `BODY` to `ORGAN`, and change `BODY_Opti` to `EXTERNAL`.

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ⚠️ Clinical Disclaimer
**For Research and Educational Purposes Only.** This software is provided "as is", without warranty of any kind. It is the sole responsibility of the clinical user (Medical Physicist or Dosimetrist) to strictly verify and validate all generated contours, assigned Hounsfield Units (HU), and final dose distributions before using them for clinical patient treatment.
